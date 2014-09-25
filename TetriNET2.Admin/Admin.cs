using System;
using System.Collections.Generic;
using TetriNET2.Admin.Interfaces;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Helpers;
using TetriNET2.Common.Logger;

namespace TetriNET2.Admin
{
    public class Admin : IAdmin
    {
        private readonly IFactory _factory;
        private readonly List<ClientAdminData> _clients;
        private readonly List<AdminData> _admins;
        private readonly List<GameRoomAdminData> _rooms;

        private IProxy _proxy;

        public Admin(IFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");

            _factory = factory;
            _clients = new List<ClientAdminData>();
            _admins = new List<AdminData>();
            _rooms = new List<GameRoomAdminData>();
        }

        #region IAdmin

        #region ITetriNETAdminCallback

        public void OnConnected(ConnectResults result, Versioning serverVersion, Guid adminId)
        {
            if (result == ConnectResults.Successfull)
            {
                _clients.Clear();
                _admins.Clear();
                _rooms.Clear();

                _admins.Add(new AdminData
                    {
                        Id = adminId,
                        Name = Name,
                        Address = "localhost",
                        ConnectTime = DateTime.Now,
                    });
            }

            AdminOnConnected.Do(x => x(result, serverVersion, adminId));
        }

        public void OnDisconnected()
        {
            AdminOnDisconnected.Do(x => x());
        }

        public void OnServerStopped()
        {
            AdminOnServerStopped.Do(x => x());
        }

        public void OnClientConnected(Guid clientId, string name, string team, string address)
        {
            _clients.Add(new ClientAdminData
                {
                    Id = clientId,
                    Name = name,
                    Team = team,
                    ConnectTime = DateTime.Now,
                    Address = address,
                });

            AdminOnClientConnected.Do(x => x(clientId, name, team));
        }

        public void OnClientDisconnected(Guid clientId, LeaveReasons reason)
        {
            AdminOnClientDisconnected.Do(x => x(clientId, reason));
        }

        public void OnAdminConnected(Guid adminId, string name, string address)
        {
            AdminOnAdminConnected.Do(x => x(adminId, name));
        }

        public void OnAdminDisconnected(Guid adminId, LeaveReasons reason)
        {
            AdminOnAdminDisconnected.Do(x => x(adminId, reason));
        }

        public void OnGameCreated(Guid clientId, GameDescription game)
        {
            AdminOnGameCreated.Do(x => x(clientId, game));
        }

        public void OnServerMessageReceived(string message)
        {
            AdminOnServerMessageReceived.Do(x => x(message));
        }

        public void OnBroadcastMessageReceived(Guid clientId, string message)
        {
            AdminOnBroadcastMessageReceived.Do(x => x(clientId, message));
        }

        public void OnPrivateMessageReceived(Guid adminId, string message)
        {
            AdminOnPrivateMessageReceived.Do(x => x(adminId, message));
        }

        public void OnAdminListReceived(List<AdminData> admins)
        {
            AdminOnAdminListReceived.Do(x => x(admins));
        }

        public void OnClientListReceived(List<ClientAdminData> clients)
        {
            AdminOnClientListReceived.Do(x => x(clients));
        }

        public void OnClientListInRoomReceived(Guid roomId, List<ClientAdminData> clients)
        {
            AdminOnClientListInRoomReceived.Do(x => x(roomId, clients));
        }

        public void OnRoomListReceived(List<GameRoomAdminData> rooms)
        {
            AdminOnRoomListReceived.Do(x => x(rooms));
        }

        public void OnBannedListReceived(List<BanEntryData> entries)
        {
            AdminOnBannedListReceived.Do(x => x(entries));
        }

        #endregion

        public string Name { get; private set; }

        public Versioning Version { get; private set; }

        public IEnumerable<ClientAdminData> Clients
        {
            get { return _clients; }
        }

        public IEnumerable<AdminData> Admins
        {
            get { return _admins; }
        }
        
        public IEnumerable<GameRoomAdminData> Rooms
        {
            get { return _rooms; }
        }

        public void SetVersion(int major, int minor)
        {
            Version = new Versioning
                {
                    Major = major,
                    Minor = minor,
                };
        }

        public event AdminConnectionLostEventHandler AdminConnectionLost;
        public event AdminOnConnectedEventHandler AdminOnConnected;
        public event AdminOnDisconnectedEventHandler AdminOnDisconnected;
        public event AdminOnServerStoppedEventHandler AdminOnServerStopped;
        public event AdminOnClientConnectedEventHandler AdminOnClientConnected;
        public event AdminOnClientDisconnectedEventHandler AdminOnClientDisconnected;
        public event AdminOnAdminConnectedEventHandler AdminOnAdminConnected;
        public event AdminOnAdminDisconnectedEventHandler AdminOnAdminDisconnected;
        public event AdminOnGameCreatedEventHandler AdminOnGameCreated;
        public event AdminOnServerMessageReceivedEventHandler AdminOnServerMessageReceived;
        public event AdminOnBroadcastMessageReceivedEventHandler AdminOnBroadcastMessageReceived;
        public event AdminOnPrivateMessageReceivedEventHandler AdminOnPrivateMessageReceived;
        public event AdminOnAdminListReceivedEventHandler AdminOnAdminListReceived;
        public event AdminOnClientListReceivedEventHandler AdminOnClientListReceived;
        public event AdminOnClientListInRoomReceivedEventHandler AdminOnClientListInRoomReceived;
        public event AdminOnRoomListReceivedEventHandler AdminOnRoomListReceived;
        public event AdminOnBannedListReceivedEventHandler AdminOnBannedListReceived;

        public bool Connect(string address, Versioning version, string name, string password)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (version == null)
                throw new ArgumentNullException("version");
            if (name == null)
                throw new ArgumentNullException("name");

            if (_proxy != null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Proxy already created, must disconnect before reconnecting");
                return false;
            }

            try
            {
                _proxy = _factory.CreateProxy(this, address);
                _proxy.ConnectionLost += OnConnectionLost;

                Name = name;

                _proxy.AdminConnect(version, name, password);

                return true;
            }
            catch(Exception ex)
            {
                Log.Default.WriteLine(LogLevels.Error, "Problem in Connect. Exception:{0}", ex.ToString());
                return false;
            }
        }

        public bool Disconnect()
        {
            if (_proxy != null)
            {
                _proxy.ConnectionLost -= OnConnectionLost;
                _proxy.Disconnect();
                _proxy = null;
            }
            return true;
        }

        public bool SendPrivateAdminMessage(Guid targetAdminId, string message)
        {
            _proxy.AdminSendPrivateAdminMessage(targetAdminId, message);
            return true;
        }

        public bool SendPrivateMessage(Guid targetClientId, string message)
        {
            _proxy.AdminSendPrivateMessage(targetClientId, message);
            return true;
        }

        public bool SendBroadcastMessage(string message)
        {
            _proxy.AdminSendBroadcastMessage(message);
            return true;
        }

        public bool GetAdminList()
        {
            _proxy.AdminGetAdminList();
            return true;
        }

        public bool GetClientList()
        {
            _proxy.AdminGetClientList();
            return true;
        }

        public bool GetClientListInRoom(Guid roomId)
        {
            _proxy.AdminGetClientListInRoom(roomId);
            return true;
        }

        public bool GetRoomList()
        {
            _proxy.AdminGetRoomList();
            return true;
        }

        public bool GetBannedList()
        {
            _proxy.AdminGetBannedList();
            return true;
        }

        public bool Kick(Guid targetId, string reason)
        {
            _proxy.AdminKick(targetId, reason);
            return true;
        }

        public bool Ban(Guid targetId, string reason)
        {
            _proxy.AdminBan(targetId, reason);
            return true;
        }

        public bool RestartServer(int seconds)
        {
            _proxy.AdminRestartServer(seconds);
            return true;
        }

        #endregion

        private void OnConnectionLost()
        {
            Disconnect();

            AdminConnectionLost.Do(x => x());
        }
    }
}
