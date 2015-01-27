using System;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using TetriNET2.Admin.Interfaces;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Helpers;
using TetriNET2.Common.Logger;

namespace TetriNET2.Admin
{
    public sealed class WCFProxy : IProxy
    {
        private DuplexChannelFactory<ITetriNETAdmin> _factory;
        private readonly ITetriNETAdmin _proxy;

        public WCFProxy(ITetriNETAdminCallback callback, string address)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");
            if (address == null)
                throw new ArgumentNullException("address");

            // Get WCF endpoint
            EndpointAddress endpointAddress = new EndpointAddress(address);

            // Create WCF proxy from endpoint
            Log.Default.WriteLine(LogLevels.Debug, "Connecting to server:{0}", endpointAddress.Uri);
            Binding binding = new NetTcpBinding(SecurityMode.None);
            InstanceContext instanceContext = new InstanceContext(callback);
            _factory = new DuplexChannelFactory<ITetriNETAdmin>(instanceContext, binding, endpointAddress);
            _proxy = _factory.CreateChannel(instanceContext);
        }

        private void ExceptionFreeAction(Action action, [CallerMemberName]string actionName = null)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Log.Default.WriteLine(LogLevels.Error, "Exception:{0} {1}", actionName, ex);
                ConnectionLost.Do(x => x());
                _factory.Do(x => x.Abort());
            }
        }

        #region IProxy

        public event ProxyAdminConnectionLostEventHandler ConnectionLost;

        public bool Disconnect()
        {
            if (_factory == null)
                return false; // should connect first
            try
            {
                _factory.Close();
            }
            catch (Exception ex)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Exception:{0}", ex);
                _factory.Abort();
            }
            _factory = null;
            return true;
        }

        #region ITetriNETAdmin

        public void AdminConnect(Versioning version, string name, string password)
        {
            ExceptionFreeAction(() => _proxy.AdminConnect(version, name, password));
        }

        public void AdminDisconnect()
        {
            ExceptionFreeAction(() => _proxy.AdminDisconnect());
        }
        
        public void AdminSendPrivateAdminMessage(Guid targetAdminId, string message)
        {
            ExceptionFreeAction(() => _proxy.AdminSendPrivateAdminMessage(targetAdminId, message));
        }

        public void AdminSendPrivateMessage(Guid targetClientId, string message)
        {
            ExceptionFreeAction(() => _proxy.AdminSendPrivateMessage(targetClientId, message));
        }

        public void AdminSendBroadcastMessage(string message)
        {
            ExceptionFreeAction(() => _proxy.AdminSendBroadcastMessage(message));
        }

        public void AdminGetAdminList()
        {
            ExceptionFreeAction(() => _proxy.AdminGetAdminList());
        }

        public void AdminGetClientList()
        {
            ExceptionFreeAction(() => _proxy.AdminGetClientList());
        }

        public void AdminGetClientListInGame(Guid gameId)
        {
            ExceptionFreeAction(() => _proxy.AdminGetClientListInGame(gameId));
        }

        public void AdminGetGameList()
        {
            ExceptionFreeAction(() => _proxy.AdminGetGameList());
        }

        public void AdminGetBannedList()
        {
            ExceptionFreeAction(() => _proxy.AdminGetBannedList());
        }

        public void AdminCreateGame(string name, GameRules rule, string password)
        {
            ExceptionFreeAction(() => _proxy.AdminCreateGame(name, rule, password));
        }

        public void AdminDeleteGame(Guid gameId)
        {
            ExceptionFreeAction(() => _proxy.AdminDeleteGame(gameId));
        }

        public void AdminKick(Guid targetId, string reason)
        {
            ExceptionFreeAction(() => _proxy.AdminKick(targetId, reason));
        }

        public void AdminBan(Guid targetId, string reason)
        {
            ExceptionFreeAction(() => _proxy.AdminBan(targetId, reason));
        }

        public void AdminRestartServer(int seconds)
        {
            ExceptionFreeAction(() => _proxy.AdminRestartServer(seconds));
        }

        #endregion

        #endregion
    }
}
