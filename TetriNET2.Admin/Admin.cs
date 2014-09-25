using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private readonly List<BanEntryData> _banned;

        private IProxy _proxy;

        public Admin(IFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");

            _factory = factory;
            _clients = new List<ClientAdminData>();
            _admins = new List<AdminData>();
            _rooms = new List<GameRoomAdminData>();
            _banned = new List<BanEntryData>();

            Assembly entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                Version version = entryAssembly.GetName().Version;
                Version = new Versioning
                {
                    Major = version.Major,
                    Minor = version.Minor,
                };
            }// else, we suppose SetVersion will be called later, before connecting
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

            Connected.Do(x => x(result, serverVersion, adminId));
        }

        public void OnDisconnected()
        {
            Disconnected.Do(x => x());
        }

        public void OnServerStopped()
        {
            ServerStopped.Do(x => x());
        }

        public void OnClientConnected(Guid clientId, string name, string team, string address)
        {
            ClientAdminData client = _clients.FirstOrDefault(x => x.Id == clientId);
            if (client == null)
                _clients.Add(new ClientAdminData
                    {
                        Id = clientId,
                        Name = name,
                        Team = team,
                        ConnectTime = DateTime.Now,
                        Address = address,
                        //LastActionFromClient = 
                        //LastActionToClient = 
                        //Roles = 
                        //State = 
                        //TimeoutCount = 
                    });
            else
            {
                client.Name = name;
                client.Team = team;
                client.Address = address;
                client.ConnectTime = DateTime.Now;
                //LastActionFromClient = 
                //LastActionToClient = 
                //Roles = 
                //State = 
                //TimeoutCount = 
            }

            ClientConnected.Do(x => x(clientId, name, team));
        }

        public void OnClientDisconnected(Guid clientId, LeaveReasons reason)
        {
            _clients.RemoveAll(x => x.Id == clientId);

            ClientDisconnected.Do(x => x(clientId, reason));
        }

        public void OnAdminConnected(Guid adminId, string name, string address)
        {
            AdminData admin = _admins.FirstOrDefault(x => x.Id == adminId);
            if (admin == null)
                _admins.Add(new AdminData
                {
                    Id = adminId,
                    Name = name,
                    ConnectTime = DateTime.Now,
                    Address = address,
                });
            else
            {
                admin.Name = name;
                admin.Address = address;
                admin.ConnectTime = DateTime.Now;
            }

            AdminConnected.Do(x => x(adminId, name));
        }

        public void OnAdminDisconnected(Guid adminId, LeaveReasons reason)
        {
            _admins.RemoveAll(x => x.Id == adminId);

            AdminDisconnected.Do(x => x(adminId, reason));
        }

        public void OnGameCreated(Guid clientId, GameRoomAdminData game)
        {
            GameRoomAdminData room = _rooms.FirstOrDefault(x => x.Id == game.Id);
            if (room == null)
                _rooms.Add(game);
            else
            {
                room.Name = game.Name;
                room.Rule = game.Rule;
                room.Options = game.Options;
                room.State = game.State;
                room.Clients = game.Clients;
            }

            GameCreated.Do(x => x(clientId, game));
        }

        public void OnServerMessageReceived(string message)
        {
            ServerMessageReceived.Do(x => x(message));
        }

        public void OnBroadcastMessageReceived(Guid clientId, string message)
        {
            BroadcastMessageReceived.Do(x => x(clientId, message));
        }

        public void OnPrivateMessageReceived(Guid adminId, string message)
        {
            PrivateMessageReceived.Do(x => x(adminId, message));
        }

        public void OnAdminListReceived(List<AdminData> admins)
        {
            _admins.Clear();
            _admins.AddRange(admins);

            AdminListReceived.Do(x => x(admins));
        }

        public void OnClientListReceived(List<ClientAdminData> clients)
        {
            _clients.Clear();
            _clients.AddRange(clients);

            ClientListReceived.Do(x => x(clients));
        }

        public void OnClientListInRoomReceived(Guid roomId, List<ClientAdminData> clients)
        {
            ClientListInRoomReceived.Do(x => x(roomId, clients));
        }

        public void OnRoomListReceived(List<GameRoomAdminData> rooms)
        {
            _rooms.Clear();
            _rooms.AddRange(rooms);

            RoomListReceived.Do(x => x(rooms));
        }

        public void OnBannedListReceived(List<BanEntryData> entries)
        {
            _banned.Clear();
            _banned.AddRange(entries);

            BannedListReceived.Do(x => x(entries));
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

        public IEnumerable<BanEntryData> Banned
        {
            get { return _banned; }
        }

        public void SetVersion(int major, int minor)
        {
            Version = new Versioning
                {
                    Major = major,
                    Minor = minor,
                };
        }

        public event ConnectionLostEventHandler ConnectionLost;
        public event ConnectedEventHandler Connected;
        public event DisconnectedEventHandler Disconnected;
        public event ServerStoppedEventHandler ServerStopped;
        public event ClientConnectedEventHandler ClientConnected;
        public event ClientDisconnectedEventHandler ClientDisconnected;
        public event AdminConnectedEventHandler AdminConnected;
        public event AdminDisconnectedEventHandler AdminDisconnected;
        public event GameCreatedEventHandler GameCreated;
        public event ServerMessageReceivedEventHandler ServerMessageReceived;
        public event BroadcastMessageReceivedEventHandler BroadcastMessageReceived;
        public event PrivateMessageReceivedEventHandler PrivateMessageReceived;
        public event AdminListReceivedEventHandler AdminListReceived;
        public event ClientListReceivedEventHandler ClientListReceived;
        public event ClientListInRoomReceivedEventHandler ClientListInRoomReceived;
        public event RoomListReceivedEventHandler RoomListReceived;
        public event BannedListReceivedEventHandler BannedListReceived;

        public bool Connect(string address, string name, string password)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (name == null)
                throw new ArgumentNullException("name");

            if (Version == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Cannot connect, version is not set");
                return false;
            }

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

                _proxy.AdminConnect(Version, name, password);

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

            ConnectionLost.Do(x => x());
        }
    }
}
