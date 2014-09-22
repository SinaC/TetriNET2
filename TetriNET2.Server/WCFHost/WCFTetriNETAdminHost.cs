using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.Contracts.WCF;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Helpers;
using TetriNET2.Common.Logger;
using TetriNET2.Server.Interfaces;
using TetriNET2.Server.Interfaces.IHost;

namespace TetriNET2.Server.WCFHost
{
    public partial class WCFHost : IHost
    {
        [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
        public sealed class WCFAdminServiceHost : IWCFTetriNETAdmin, IDisposable
        {
            private ServiceHost _serviceHost;
            private readonly IHost _host;

            public int Port { get; set; }

            public WCFAdminServiceHost(IHost host)
            {
                if (host == null)
                    throw new ArgumentNullException("host");
                _host = host;
            }

            public void Start()
            {
                Uri baseAddress = new Uri(String.Format("net.tcp://localhost:{0}", Port));

                _serviceHost = new ServiceHost(this, baseAddress);
                _serviceHost.AddServiceEndpoint(typeof(IWCFTetriNETAdmin), new NetTcpBinding(SecurityMode.None), "/TetriNET2Admin");
                _serviceHost.Description.Behaviors.Add(new IPFilterServiceBehavior(_host.BanManager, _host.ClientManager));
                _serviceHost.Open();

                Log.Default.WriteLine(LogLevels.Info, "WCF Admin Host opened on {0}", baseAddress);

                foreach (var endpt in _serviceHost.Description.Endpoints)
                {
                    Log.Default.WriteLine(LogLevels.Debug, "Enpoint address:\t{0}", endpt.Address);
                    Log.Default.WriteLine(LogLevels.Debug, "Enpoint binding:\t{0}", endpt.Binding);
                    Log.Default.WriteLine(LogLevels.Debug, "Enpoint contract:\t{0}", endpt.Contract.ContractType.Name);
                }
            }

            public void Stop()
            {
                // Close service host
                _serviceHost.Do(x => x.Close());
            }

            #region IWCFTetriNETAdmin

            public void AdminConnect(Versioning version, string name, string password)
            {
                _host.AdminConnect(Callback, Address, version, name, password);
            }

            public void AdminDisconnect()
            {
                _host.AdminDisconnect(Callback);
            }

            public void AdminSendPrivateAdminMessage(Guid targetAdminId, string message)
            {
                _host.AdminSendPrivateAdminMessage(Callback, targetAdminId, message);
            }

            public void AdminSendPrivateMessage(Guid targetClientId, string message)
            {
                _host.AdminSendPrivateMessage(Callback, targetClientId, message);
            }

            public void AdminSendBroadcastMessage(string message)
            {
                _host.AdminSendBroadcastMessage(Callback, message);
            }

            public void AdminGetAdminList()
            {
                _host.AdminGetAdminList(Callback);
            }

            public void AdminGetClientList()
            {
                _host.AdminGetClientList(Callback);
            }

            public void AdminGetClientListInRoom(Guid roomId)
            {
                _host.AdminGetClientListInRoom(Callback, roomId);
            }

            public void AdminGetRoomList()
            {
                _host.AdminGetRoomList(Callback);
            }

            public void AdminGetBannedList()
            {
                _host.AdminGetBannedList(Callback);
            }

            public void AdminKick(Guid targetId, string reason)
            {
                _host.AdminKick(Callback, targetId, reason);
            }

            public void AdminBan(Guid targetId, string reason)
            {
                _host.AdminBan(Callback, targetId, reason);
            }

            public void AdminRestartServer(int seconds)
            {
                _host.AdminRestartServer(Callback, seconds);
            }

            #endregion

            #region IDisposable

            private void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (_serviceHost != null)
                        _serviceHost.Close();
                }
            }

            public void Dispose()
            {
                Dispose(true);
            }

            #endregion

            private ITetriNETAdminCallback Callback
            {
                get
                {
                    return OperationContext.Current.GetCallbackChannel<ITetriNETAdminCallback>();
                }
            }

            private IPAddress Address
            {
                get
                {
                    MessageProperties messageProperties = OperationContext.Current.IncomingMessageProperties;
                    RemoteEndpointMessageProperty endpointProperty = messageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                    if (endpointProperty != null)
                        return IPAddress.Parse(endpointProperty.Address);
                    return null;
                }
            }
        }

        #region ITetriNETAdminHost

        public event HostAdminConnectEventHandler HostAdminConnect;
        public event HostAdminDisconnectEventHandler HostAdminDisconnect;
        public event HostAdminSendPrivateAdminMessageEventHandler HostAdminSendPrivateAdminMessage;
        public event HostAdminSendPrivateMessageEventHandler HostAdminSendPrivateMessage;
        public event HostAdminSendBroadcastMessageEventHandler HostAdminSendBroadcastMessage;
        public event HostAdminGetAdminListEventHandler HostAdminGetAdminList;
        public event HostAdminGetClientListEventHandler HostAdminGetClientList;
        public event HostAdminGetClientListInRoomEventHandler HostAdminGetClientListInRoom;
        public event HostAdminGetRoomListEventHandler HostAdminGetRoomList;
        public event HostAdminGetBannedListEventHandler HostAdminGetBannedList;
        public event HostAdminKickEventHandler HostAdminKick;
        public event HostAdminBanEventHandler HostAdminBan;
        public event HostAdminRestartServerEventHandler HostAdminRestartServer;

        #region ITetriNETAdmin

        public void AdminConnect(ITetriNETAdminCallback callback, IPAddress address, Versioning version, string name, string password)
        {
            HostAdminConnect.Do(x => x(callback, address, version, name, password));
        }

        public void AdminDisconnect(ITetriNETAdminCallback callback)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null)
                HostAdminDisconnect.Do(x => x(admin));
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminDisconnect from unknown admin");
        }

        public void AdminSendPrivateAdminMessage(ITetriNETAdminCallback callback, Guid targetAdminId, string message)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null)
            {
                IAdmin target = AdminManager[targetAdminId];
                if (target != null)
                    HostAdminSendPrivateAdminMessage.Do(x => x(admin, target, message));
                else
                    Log.Default.WriteLine(LogLevels.Warning, "AdminSendPrivateAdminMessage to unknown admin");
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminSendPrivateAdminMessage from unknown admin");
        }

        public void AdminSendPrivateMessage(ITetriNETAdminCallback callback, Guid targetClientId, string message)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null)
            {
                IClient target = ClientManager[targetClientId];
                if (target != null)
                    HostAdminSendPrivateMessage.Do(x => x(admin, target, message));
                else
                    Log.Default.WriteLine(LogLevels.Warning, "AdminSendPrivateMessage to unknown client");
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminSendPrivateMessage from unknown admin");
        }

        public void AdminSendBroadcastMessage(ITetriNETAdminCallback callback, string message)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null)
                HostAdminSendBroadcastMessage.Do(x => x(admin, message));
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminSendBroadcastMessage from unknown admin");
        }

        public void AdminGetAdminList(ITetriNETAdminCallback callback)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null)
                HostAdminGetAdminList.Do(x => x(admin));
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminGetAdminList from unknown admin");
        }

        public void AdminGetClientList(ITetriNETAdminCallback callback)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null)
                HostAdminGetClientList.Do(x => x(admin));
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminGetClientList from unknown admin");
        }

        public void AdminGetClientListInRoom(ITetriNETAdminCallback callback, Guid roomId)
        {
            IAdmin admin = AdminManager[callback];
            IGameRoom game = GameRoomManager[roomId];
            if (admin != null && game != null && HostAdminGetClientListInRoom != null)
                HostAdminGetClientListInRoom.Do(x => x(admin, game));
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminGetClientListInRoom from unknown admin");
        }

        public void AdminGetRoomList(ITetriNETAdminCallback callback)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null && HostAdminGetRoomList != null)
                HostAdminGetRoomList.Do(x => x(admin));
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminGetRoomList from unknown admin");
        }

        public void AdminGetBannedList(ITetriNETAdminCallback callback)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null && HostAdminGetBannedList != null)
                HostAdminGetBannedList.Do(x => x(admin));
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminGetBannedList from unknown admin");
        }

        public void AdminKick(ITetriNETAdminCallback callback, Guid targetId, string reason)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null)
            {
                IClient target = ClientManager[targetId];
                if (target != null)
                    HostAdminKick.Do(x => x(admin, target, reason));
                else
                    Log.Default.WriteLine(LogLevels.Warning, "AdminKick to unknown client");
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminKick from unknown admin");
        }

        public void AdminBan(ITetriNETAdminCallback callback, Guid targetId, string reason)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null)
            {
                IClient target = ClientManager[targetId];
                if (target != null)
                    HostAdminBan.Do(x => x(admin, target, reason));
                else
                    Log.Default.WriteLine(LogLevels.Warning, "AdminBan to unknown client");
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminBan from unknown admin");
        }

        public void AdminRestartServer(ITetriNETAdminCallback callback, int seconds)
        {
            IAdmin admin = AdminManager[callback];
            if (admin != null && HostAdminRestartServer != null)
                HostAdminRestartServer.Do(x => x(admin, seconds));
            else
                Log.Default.WriteLine(LogLevels.Warning, "AdminRestartServer from unknown admin");
        }

        #endregion

        #endregion
    }
}
