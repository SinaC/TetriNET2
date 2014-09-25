using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Helpers;
using TetriNET2.Common.Logger;
using TetriNET2.Server.Interfaces;
using TetriNET2.Server.Interfaces.IHost;

namespace TetriNET2.Server
{
    public sealed class Server : IServer, IDisposable
    {
        private const int HeartbeatDelay = 300; // in ms
        private const int TimeoutDelay = 500; // in ms
        private const int MaxTimeoutCountBeforeDisconnection = 3;
        private const bool IsTimeoutDetectionActive = false;
        private const int MinRestartDelay = 30; // in seconds

        private readonly List<IHost> _hosts = new List<IHost>();
        private readonly IFactory _factory;
        private readonly IPasswordManager _passwordManager;
        private readonly IBanManager _banManager;
        private readonly IClientManager _clientManager;
        private readonly IAdminManager _adminManager;
        private readonly IGameRoomManager _gameRoomManager;

        private CancellationTokenSource _cancellationTokenSource;
        private Task _timeoutTask;
        private Timer _restartTimer;
        private int _restartSecondLeft;
        private bool _isRestartRunning;

        public Server(IFactory factory, IPasswordManager passwordManager, IBanManager banManager, IClientManager clientManager, IAdminManager adminManager, IGameRoomManager gameRoomManager)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");
            if (passwordManager == null)
                throw new ArgumentNullException("passwordManager");
            if (banManager == null)
                throw new ArgumentNullException("banManager");
            if (clientManager == null)
                throw new ArgumentNullException("clientManager");
            if (adminManager == null)
                throw new ArgumentNullException("adminManager");
            if (gameRoomManager == null)
                throw new ArgumentNullException("gameRoomManager");

            _factory = factory;
            _passwordManager = passwordManager;
            _banManager = banManager;
            _clientManager = clientManager;
            _adminManager = adminManager;
            _gameRoomManager = gameRoomManager;

            State = ServerStates.Waiting;

            Assembly entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                Version version = entryAssembly.GetName().Version;
                Version = new Versioning
                    {
                        Major = version.Major,
                        Minor = version.Minor,
                    };
            } // else, we suppose SetVersion will be called later, before connecting
        }

        #region IServer

        public event PerformRestartServerEventHandler PerformRestartServer;

        public ServerStates State { get; private set; }

        public Versioning Version { get; private set; }

        public bool AddHost(IHost host)
        {
            if (host == null)
                throw new ArgumentNullException("host");

            if (_hosts.Any(x => x == host))
            {
                Log.Default.WriteLine(LogLevels.Warning, "Host already added");
                return false;
            }

            if (State != ServerStates.Waiting)
            {
                Log.Default.WriteLine(LogLevels.Error, "Trying to add host when server is already started");
                return false;
            }

            _hosts.Add(host);

            // ------
            // Client
            // Connect/disconnect/keep alive
            host.HostClientConnect += OnClientConnect;
            host.HostClientDisconnect += OnClientDisconnect;
            host.HostClientHeartbeat += OnClientHeartbeat;

            // Wait+Game room
            host.HostClientSendPrivateMessage += OnClientSendPrivateMessage;
            host.HostClientSendBroadcastMessage += OnClientSendBroadcastMessage;
            host.HostClientChangeTeam += OnClientChangeTeam;

            // Wait room
            host.HostClientJoinGame += OnClientJoinGame;
            host.HostClientJoinRandomGame += OnClientJoinRandomGame;
            host.HostClientCreateAndJoinGame += OnClientCreateAndJoinGame;
            host.HostClientGetRoomList += OnClientGetRoomList;
            host.HostClientGetClientList += OnClientGetClientList;

            // Game room as game master (player or spectator)
            host.HostClientStartGame += OnClientStartGame;
            host.HostClientStopGame += OnClientStopGame;
            host.HostClientPauseGame += OnClientPauseGame;
            host.HostClientResumeGame += OnClientResumeGame;
            host.HostClientChangeOptions += OnClientChangeOptions;
            host.HostClientVoteKick += OnClientVoteKick;
            host.HostClientVoteKickAnswer += OnClientVoteKickAnswer;
            host.HostClientResetWinList += OnClientResetWinList;

            // Game room as player or spectator
            host.HostClientLeaveGame += OnClientLeaveGame;
            host.HostClientGetGameClientList += OnClientGetGameClientList;

            // Game room as player
            host.HostClientPlacePiece += OnClientPlacePiece;
            host.HostClientModifyGrid += OnClientModifyGrid;
            host.HostClientUseSpecial += OnClientUseSpecial;
            host.HostClientClearLines += OnClientClearLines;
            host.HostClientGameLost += OnClientGameLost;
            host.HostClientFinishContinuousSpecial += OnClientFinishContinuousSpecial;
            host.HostClientEarnAchievement += OnClientEarnAchievement;

            // ------
            // Admin
            // Connect/Disconnect
            host.HostAdminConnect += OnAdminConnect;
            host.HostAdminDisconnect += OnAdminDisconnect;

            // Messaging
            host.HostAdminSendPrivateAdminMessage += OnAdminSendPrivateAdminMessage;
            host.HostAdminSendPrivateMessage += OnAdminSendPrivateMessage;
            host.HostAdminSendBroadcastMessage += OnAdminSendBroadcastMessage;

            // Monitoring
            host.HostAdminGetAdminList += OnAdminGetAdminList;
            host.HostAdminGetClientList += OnAdminGetClientList;
            host.HostAdminGetClientListInRoom += OnAdminGetClientListInRoom;
            host.HostAdminGetRoomList += OnAdminGetRoomList;
            host.HostAdminGetBannedList += OnAdminGetBannedList;

            // Kick/Ban
            host.HostAdminKick += OnAdminKick;
            host.HostAdminBan += OnAdminBan;

            // Server commands
            host.HostAdminRestartServer += OnAdminRestartServer;

            Debug.Assert(CheckEvents(host), "Every host events must be handled");

            return true;
        }

        public bool SetVersion(int major, int minor)
        {
            if (State != ServerStates.Waiting)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot change version while server is already started");
                return false;
            }

            Version = new Versioning
                {
                    Major = major,
                    Minor = minor
                };
            return true;
        }

        public bool SetAdminPassword(string name, string cryptedPassword)
        {
            return _passwordManager.Add(DomainTypes.Admin, name, cryptedPassword);
        }

        public bool Start()
        {
            Log.Default.WriteLine(LogLevels.Info, "Starting server");

            if (Version == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Cannot start server until a version has been specified");
                return false;
            }
            if (_hosts.Count == 0)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot start server without any host");
                return false;
            }
            if (State != ServerStates.Waiting)
            {
                Log.Default.WriteLine(LogLevels.Info, "Server already started");
                return false;
            }
            //
            State = ServerStates.Starting;

            _cancellationTokenSource = new CancellationTokenSource();
            _timeoutTask = Task.Factory.StartNew(TimeoutTask, _cancellationTokenSource.Token);

            // Start hosts
            foreach (IHost host in _hosts)
                host.Start();

            // TODO: create and start some game rooms

            State = ServerStates.Started;

            Log.Default.WriteLine(LogLevels.Info, "Server started");
            return true;
        }

        public bool Stop()
        {
            Log.Default.WriteLine(LogLevels.Info, "Stopping server");

            if (State != ServerStates.Started)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Server not started");
                return false;
            }
            //
            State = ServerStates.Stopping;

            try
            {
                // Stop worker threads
                _cancellationTokenSource.Cancel();

                _timeoutTask.Wait(2000);
            }
            catch (AggregateException ex)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Aggregate exception while stopping. Exception: {0}", ex.Flatten());
            }

            // Inform clients
            lock (_clientManager.LockObject)
                foreach (IClient client in _clientManager.Clients)
                    client.OnServerStopped();

            // Inform admins
            lock (_adminManager.LockObject)
                foreach (IAdmin admin in _adminManager.Admins)
                    admin.OnServerStopped();

            // Stop games
            lock (_gameRoomManager.LockObject)
                foreach (IGameRoom game in _gameRoomManager.Rooms)
                    game.Stop();

            // Stop hosts
            foreach (IHost host in _hosts)
                host.Stop();

            // Clear clients, games, wait, admins
            _clientManager.Clear();
            _gameRoomManager.Clear();
            _adminManager.Clear();

            //
            State = ServerStates.Waiting;

            Log.Default.WriteLine(LogLevels.Info, "Server stopped");
            return true;
        }

        #endregion

        #region IHost events handler

        // Client
        private void OnClientConnect(ITetriNETClientCallback callback, IPAddress address, Versioning version, string name, string team)
        {
            Log.Default.WriteLine(LogLevels.Info, "Connect client {0}[{1}] {2}.{3}", name, address == null ? "???" : address.ToString(), version == null ? -1 : version.Major, version == null ? -1 : version.Minor);

            if (callback == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "!!!Null callback!!!");
                return;
            }

            ConnectResults result = ConnectResults.Successfull;
            if (version == null || Version.Major != version.Major || Version.Minor != version.Minor)
            {
                result = ConnectResults.FailedIncompatibleVersion;
                Log.Default.WriteLine(LogLevels.Warning, "Cannot connect {0}[{1}] server version {2}.{3} is incompatible", name, address == null ? "???" : address.ToString(), Version.Major, Version.Minor);
            }
            else
            {
                lock (_clientManager.LockObject)
                {
                    if (_clientManager.Contains(name, callback))
                    {
                        result = ConnectResults.FailedClientAlreadyExists;
                        Log.Default.WriteLine(LogLevels.Warning, "Cannot connect {0}[{1}] because it already exists", name, address == null ? "???" : address.ToString());
                    }
                    else if (_clientManager.ClientCount >= _clientManager.MaxClients)
                    {
                        result = ConnectResults.FailedTooManyClients;
                        Log.Default.WriteLine(LogLevels.Warning, "Cannot connect {0}[{1}] because too many clients already connected", name, address == null ? "???" : address.ToString());
                    }
                    else if (String.IsNullOrEmpty(name) || name.Length > 20 || name.Any(x => !Char.IsLetterOrDigit(x)))
                    {
                        result = ConnectResults.FailedInvalidName;
                        Log.Default.WriteLine(LogLevels.Warning, "Cannot connect {0}[{1}] because name is invalid", name, address == null ? "???" : address.ToString());
                    }
                    else if (_banManager.IsBanned(address))
                    {
                        result = ConnectResults.FailedBanned;
                        Log.Default.WriteLine(LogLevels.Warning, "Cannot connect {0}[{1}] because client is banned", name, address == null ? "???" : address.ToString());
                    }
                    else
                    {
                        IClient client = _factory.CreateClient(name, team, address, callback);
                        bool added = _clientManager.Add(client);
                        if (!added)
                        {
                            result = ConnectResults.FailedInternalError;
                            Log.Default.WriteLine(LogLevels.Warning, "Cannot connect {0}[{1}] because it cannot be added to client manager", name, address == null ? "???" : address.ToString());
                        }
                        else
                        {
                            // Build game room list
                            List<GameRoomData> games;
                            lock (_gameRoomManager.LockObject)
                                games = _gameRoomManager.Rooms.Select(r => new GameRoomData
                                    {
                                        Id = r.Id,
                                        Name = r.Name,
                                        Clients = BuildClientDatas(r),
                                    }).ToList();
                            // Handle connection lost
                            client.ConnectionLost += OnClientConnectionLost;
                            // Inform client about connection succeed
                            client.OnConnected(result, Version, client.Id, games);
                            // Client is alive
                            client.ResetTimeout();
                            // Send message to clients
                            lock (_clientManager.LockObject)
                                foreach (IClient target in _clientManager.Clients.Where(c => c != client))
                                    target.OnClientConnected(client.Id, client.Name, client.Team);
                            // Send message to admin
                            lock (_adminManager.LockObject)
                                foreach (IAdmin target in _adminManager.Admins)
                                    target.OnClientConnected(client.Id, client.Name, client.Team, client.Address.ToString());
                            // Hosts
                            foreach (IHost host in _hosts)
                                host.AddClient(client);
                            //
                            Log.Default.WriteLine(LogLevels.Info, "Connect Client {0}[{1}] succeed", name, address == null ? "???" : address.ToString());
                        }
                    }
                }
            }

            if (result != ConnectResults.Successfull) // Inform client about connection fail
                callback.OnConnected(result, Version, Guid.Empty, null);
        }

        private void OnClientDisconnect(IClient client)
        {
            Log.Default.WriteLine(LogLevels.Info, "Disconnect client:{0}", client.Name);

            OnClientLeft(client, LeaveReasons.Disconnected);
        }

        private void OnClientHeartbeat(IClient client)
        {
            client.ResetTimeout();
        }

        private void OnClientSendPrivateMessage(IClient client, IClient target, string message)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client send private message:{0} {1} {2}", client.Name, target.Name, message);

            target.OnPrivateMessageReceived(client.Id, message);
        }

        private void OnClientSendBroadcastMessage(IClient client, string message)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client send broadcast message:{0} {1}", client.Name, message);

            // Send message to clients
            lock (_clientManager.LockObject)
                foreach (IClient target in _clientManager.Clients)
                    target.OnBroadcastMessageReceived(client.Id, message);
            // Send message to admin
            lock (_adminManager.LockObject)
                foreach (IAdmin target in _adminManager.Admins)
                    target.OnBroadcastMessageReceived(client.Id, message);
        }

        private void OnClientChangeTeam(IClient client, string team)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client change team:{0} {1}", client.Name, team);

            client.Team = String.IsNullOrWhiteSpace(team) ? null : team;
            // Send message to clients
            lock (_clientManager.LockObject)
                foreach (IClient target in _clientManager.Clients)
                    target.OnTeamChanged(client.Id, team);
        }

        private void OnClientJoinGame(IClient client, IGameRoom game, string password, bool asSpectator)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client join game:{0} {1} {2}", client.Name, game.Name, asSpectator);

            GameJoinResults result = GameJoinResults.Successfull;
            if (client.Game != null)
            {
                result = GameJoinResults.FailedAlreadyInGame;
                Log.Default.WriteLine(LogLevels.Warning, "Client {0} is already in game {1}", client.Name, client.Game.Name);
            }
            else if (game.Password != null && game.Password != password)
            {
                result = GameJoinResults.FailedWrongPassword;
                Log.Default.WriteLine(LogLevels.Warning, "Wrong password from {0} for joining game {1}", client.Name, game.Name);
            }
            else if (!asSpectator && game.PlayerCount >= game.MaxPlayers)
            {
                result = GameJoinResults.FailedTooManyPlayers;
                Log.Default.WriteLine(LogLevels.Warning, "Too many players for {0} in game {1}", client.Name, game.Name);
            }
            else if (asSpectator && game.SpectatorCount >= game.MaxSpectators)
            {
                result = GameJoinResults.FailedTooManySpectators;
                Log.Default.WriteLine(LogLevels.Warning, "Too many spectators for {0} in game {1}", client.Name, game.Name);
            }
            else
            {
                lock (game.LockObject)
                {
                    // Add client in game
                    bool joined = game.Join(client, asSpectator);
                    if (!joined)
                    {
                        result = GameJoinResults.FailedInternalError;
                        Log.Default.WriteLine(LogLevels.Warning, "Client {0} cannot join game {1}", client.Name, game.Name);
                    }
                }
            }
            if (result != GameJoinResults.Successfull)
                client.OnGameJoined(result, game.Id, null, false);
        }

        private void OnClientJoinRandomGame(IClient client, bool asSpectator)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client join random game:{0} {1}", client.Name, asSpectator);

            GameJoinResults result = GameJoinResults.Successfull;

            if (client.Game != null)
            {
                result = GameJoinResults.FailedAlreadyInGame;
                Log.Default.WriteLine(LogLevels.Warning, "Client {0} is already in game {1}", client.Name, client.Game.Name);
            }
            else
            {
                // Search a suitable room
                IGameRoom game;
                lock (_gameRoomManager.LockObject)
                    game = _gameRoomManager.Rooms.FirstOrDefault(x =>
                        x.Password == null
                        && ((!asSpectator && x.PlayerCount < x.MaxPlayers)
                            ||
                            (asSpectator && x.SpectatorCount < x.MaxSpectators)));

                if (game == null)
                {
                    result = GameJoinResults.FailedNoRoomAvailable;
                    Log.Default.WriteLine(LogLevels.Warning, "Client {0} cannot find a suitable random game", client.Name);
                }
                else
                {
                    lock (game.LockObject)
                    {
                        // Add client in game
                        bool joined = game.Join(client, asSpectator);
                        if (!joined)
                        {
                            result = GameJoinResults.FailedInternalError;
                            Log.Default.WriteLine(LogLevels.Warning, "Client {0} cannot join game {1}", client.Name, game.Name);
                        }
                    }
                }
            }
            if (result != GameJoinResults.Successfull)
                client.OnGameJoined(result, Guid.Empty, null, false);
        }

        private void OnClientCreateAndJoinGame(IClient client, string name, string password, GameRules rule, bool asSpectator)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client create and join game: {0} {1} {2}", client.Name, name, asSpectator);

            GameCreateResults result = GameCreateResults.Successfull;

            if (client.Game != null)
            {
                result = GameCreateResults.FailedAlreadyInGame;
                Log.Default.WriteLine(LogLevels.Warning, "Client {0} is already in game {1}", client.Name, client.Game.Name);
            }
            else
            {
                lock (_gameRoomManager.LockObject)
                {
                    if (_gameRoomManager.RoomCount >= _gameRoomManager.MaxRooms)
                    {
                        result = GameCreateResults.FailedTooManyRooms;
                        Log.Default.WriteLine(LogLevels.Warning, "Client {0} cannot create game {1} because there is too many rooms", client.Name, name);
                    }
                    else if (_gameRoomManager[name] != null)
                    {
                        result = GameCreateResults.FailedAlreadyExists;
                        Log.Default.WriteLine(LogLevels.Warning, "Client {0} cannot create game {1} because it already exists", client.Name, name);
                    }
                    else
                    {
                        GameOptions options = new GameOptions();
                        options.Initialize(rule);
                        IGameRoom game = _factory.CreateGameRoom(name, 6, 10, rule, options, password);
                        bool added = _gameRoomManager.Add(game);
                        if (!added)
                        {
                            result = GameCreateResults.FailedInternalError;
                            Log.Default.WriteLine(LogLevels.Warning, "Created game {0} by {1} cannot be added", name, client.Name);
                        }
                        else
                        {
                            //
                            GameRoomData clientDescription = new GameRoomData
                            {
                                Id = game.Id,
                                Name = game.Name,
                                Rule = game.Rule,
                                Clients = new List<ClientData>()
                            };
                            GameRoomAdminData adminDescription = new GameRoomAdminData
                            {
                                Id = game.Id,
                                Name = game.Name,
                                Rule = game.Rule,
                                State = game.State,
                                Options = game.Options,
                                Clients = BuildClientAdminDatas(game)
                            };

                            // Inform client
                            client.OnGameCreated(result, clientDescription);

                            // Inform other clients
                            lock (_clientManager.LockObject)
                                foreach (IClient target in _clientManager.Clients.Where(c => c != client))
                                    target.OnClientGameCreated(client.Id, clientDescription);

                            // Inform admins
                            lock (_adminManager.LockObject)
                                foreach (IAdmin target in _adminManager.Admins)
                                    target.OnGameCreated(client.Id, adminDescription);

                            // Hosts
                            foreach (IHost host in _hosts)
                                host.AddGameRoom(game);

                            // Start room
                            game.Start(_cancellationTokenSource);

                            // Add client in game
                            bool joined = game.Join(client, asSpectator);
                            if (!joined)
                            {
                                result = GameCreateResults.FailedInternalError;
                                Log.Default.WriteLine(LogLevels.Warning, "Game {0} created by {1} but client cannot join", name, client.Name);
                            }
                        }
                    }
                }
            }
            if (result != GameCreateResults.Successfull)
                client.OnGameCreated(result, null);
        }

        private void OnClientGetRoomList(IClient client)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client get room list: {0}", client.Name);

            // Build list
            List<GameRoomData> list;
            lock (_gameRoomManager)
                list = _gameRoomManager.Rooms.Select(x => new GameRoomData
                {
                    Id = x.Id,
                    Name = x.Name,
                    Rule = x.Rule,
                    Options = x.Options,
                    Clients = BuildClientDatas(x)
                }).ToList();
            // Send list
            client.OnRoomListReceived(list);
        }

        private void OnClientGetClientList(IClient client)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client get client list", client.Name);

            // Build list
            List<ClientData> list;
            lock (_clientManager.LockObject)
                list = _clientManager.Clients.Select(BuildClientData).ToList();
            // Send list
            client.OnClientListReceived(list);
        }

        private void OnClientStartGame(IClient client)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client start game: {0}", client.Name);

            IGameRoom game = client.Game;
            if (game == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot start game, client {0} is not in a game room", client.Name);
                return;
            }
            if (!client.IsGameMaster)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot start game, client {0} is not game master", client.Name);
                return;
            }
            //
            game.StartGame(client); // StartGame is responsible for using Callback
        }

        private void OnClientStopGame(IClient client)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client stop game: {0}", client.Name);

            IGameRoom game = client.Game;
            if (game == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot stop game, client {0} is not in a game room", client.Name);
                return;
            }
            if (!client.IsGameMaster)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot stop game, client {0} is not game master", client.Name);
                return;
            }
            //
            game.StopGame(client); // StopGame is responsible for using Callback
        }

        private void OnClientPauseGame(IClient client)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client pause game: {0}", client.Name);

            IGameRoom game = client.Game;
            if (game == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot pause game, client {0} is not in a game room", client.Name);
                return;
            }
            if (!client.IsGameMaster)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot pause game, client {0} is not game master", client.Name);
                return;
            }
            //
            game.PauseGame(client); // PauseGame is responsible for using Callback
        }

        private void OnClientResumeGame(IClient client)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client resume game: {0}", client.Name);

            IGameRoom game = client.Game;
            if (game == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot resume game, client {0} is not in a game room", client.Name);
                return;
            }
            if (!client.IsGameMaster)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot resume game, client {0} is not game master", client.Name);
                return;
            }
            //
            game.ResumeGame(client); // ResumeGame is responsible for using Callback
        }

        private void OnClientChangeOptions(IClient client, GameOptions options)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client change options: {0}", client.Name);

            IGameRoom game = client.Game;
            if (game == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot change options, client {0} is not in a game room", client.Name);
                return;
            }
            if (!client.IsGameMaster)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot change options, client {0} is not game master", client.Name);
                return;
            }
            // Remove duplicates (just in case)
            options.SpecialOccurancies = options.SpecialOccurancies.GroupBy(x => x.Value).Select(x => x.First()).ToList();
            options.PieceOccurancies = options.PieceOccurancies.GroupBy(x => x.Value).Select(x => x.First()).ToList();

            // Check options before accepting them
            bool accepted = options.IsValid;
            if (accepted)
                game.ChangeOptions(client, options); // ChangeOptions is responsible for using Callback
            else
                Log.Default.WriteLine(LogLevels.Info, "Invalid options");
        }

        private void OnClientResetWinList(IClient client)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client reset winlist: {0}", client.Name);

            IGameRoom game = client.Game;
            if (game == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot reset winlist, client {0} is not in a game room", client.Name);
                return;
            }
            if (!client.IsGameMaster)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot reset winlist, client {0} is not game master", client.Name);
                return;
            }
            //
            game.ResetWinList(client); // ResetWinList is responsible for using Callback
        }

        private void OnClientVoteKick(IClient client, IClient target, string reason)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client vote kick: {0}", client.Name);

            if (client.Game == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot vote kick, client {0} is not in a game room", client.Name);
                return;
            }
            if (target.Game == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot vote kick, target {0} is not in a game room", target.Name);
                return;
            }
            if (client.Game == target.Game)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot vote kick, client {0} and target {1} are not in the same game", client.Name, target.Name);
                return;
            }
            //
            IGameRoom game = client.Game;
            game.VoteKick(client, target, reason); // VoteKick is responsible for using Callback
        }

        private void OnClientVoteKickAnswer(IClient client, bool accepted)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client vote kick answer: {0}", client.Name);

            IGameRoom game = client.Game;
            if (game == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot handle vote kick answer, client {0} is not in a game room", client.Name);
                return;
            }
            //
            game.VoteKickAnswer(client, accepted); // VoteKickAnswer is responsible for using Callback
        }

        private void OnClientLeaveGame(IClient client)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client leave game: {0}", client.Name);

            IGameRoom game = client.Game;
            if (game == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot leave game, client {0} is not in a game room", client.Name);
                return;
            }
            //
            game.Leave(client); // Leave is responsible for using Callback
        }

        private void OnClientGetGameClientList(IClient client)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client get game client list", client.Name);

            if (client.Game == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot get game client list, {0} is not in a game room", client.Name);
                return;
            }
            // Build list
            List<ClientData> list = BuildClientDatas(client.Game);
            // Send list
            client.OnGameClientListReceived(list);
        }

        private void OnClientPlacePiece(IClient client, int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client place piece: {0}", client.Name);

            IGameRoom game = client.Game;
            if (game == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot place piece, client {0} is not in a game room", client.Name);
                return;
            }
            //
            game.PlacePiece(client, pieceIndex, highestIndex, piece, orientation, posX, posY, grid); // PlacePiece is responsible for using Callback
        }

        private void OnClientModifyGrid(IClient client, byte[] grid)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client modify grid: {0}", client.Name);

            IGameRoom game = client.Game;
            if (game == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot modify grid, client {0} is not in a game room", client.Name);
                return;
            }
            //
            game.ModifyGrid(client, grid); // ModifyGrid is responsible for using Callback
        }

        private void OnClientUseSpecial(IClient client, IClient target, Specials special)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client use special: {0}", client.Name);

            IGameRoom game = client.Game;
            if (game == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot use special, client {0} is not in a game room", client.Name);
                return;
            }
            //
            game.UseSpecial(client, target, special); // UseSpecial is responsible for using Callback
        }

        private void OnClientClearLines(IClient client, int count)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client clear lines: {0}", client.Name);

            IGameRoom game = client.Game;
            if (game == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot clear lines, client {0} is not in a game room", client.Name);
                return;
            }
            //
            game.ClearLines(client, count); // ClearLines is responsible for using Callback
        }

        private void OnClientGameLost(IClient client)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client game lost: {0}", client.Name);

            IGameRoom game = client.Game;
            if (game == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot game lost, client {0} is not in a game room", client.Name);
                return;
            }
            //
            game.GameLost(client); // GameLost is responsible for using Callback
        }

        private void OnClientFinishContinuousSpecial(IClient client, Specials special)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client finish continuous special: {0}", client.Name);

            IGameRoom game = client.Game;
            if (game == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot finish continuous special, client {0} is not in a game room", client.Name);
                return;
            }
            //
            game.FinishContinuousSpecial(client, special); // FinishContinuousSpecial is responsible for using Callback
        }

        private void OnClientEarnAchievement(IClient client, int achievementId, string achievementTitle)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client earn achievement: {0}", client.Name);

            IGameRoom game = client.Game;
            if (game == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot earn achievement, client {0} is not in a game room", client.Name);
                return;
            }
            //
            game.EarnAchievement(client, achievementId, achievementTitle); // EarnAchievement  is responsible for using Callback
        }

        // Admin
        private void OnAdminConnect(ITetriNETAdminCallback callback, IPAddress address, Versioning version, string name, string password)
        {
            Log.Default.WriteLine(LogLevels.Info, "Connect admin {0}[1] {2}.{3}", name, address == null ? "???" : address.ToString(), version == null ? -1 : version.Major, version == null ? -1 : version.Minor);

            ConnectResults result = ConnectResults.Successfull;

            if (version == null || Version.Major != version.Major || Version.Minor != version.Minor)
                result = ConnectResults.FailedIncompatibleVersion;
            else
            {
                lock (_adminManager.LockObject)
                {
                    if (_adminManager.Contains(name, callback))
                    {
                        result = ConnectResults.FailedAdminAlreadyExists;
                        Log.Default.WriteLine(LogLevels.Warning, "Cannot connect {0}[{1}] because it already exists", name, address == null ? "???" : address.ToString());
                    }
                    else if (_adminManager.AdminCount >= _adminManager.MaxAdmins)
                    {
                        result = ConnectResults.FailedTooManyAdmins;
                        Log.Default.WriteLine(LogLevels.Warning, "Cannot connect {0}[{1}] because too many admins already connected", name, address == null ? "???" : address.ToString());
                    }
                    else if (String.IsNullOrEmpty(name) || name.Length > 20 || name.Any(x => !Char.IsLetterOrDigit(x)))
                    {
                        result = ConnectResults.FailedInvalidName;
                        Log.Default.WriteLine(LogLevels.Warning, "Cannot connect {0}[{1}] because name is invalid", name, address == null ? "???" : address.ToString());
                    }
                    else if (_banManager.IsBanned(address))
                    {
                        result = ConnectResults.FailedBanned;
                        Log.Default.WriteLine(LogLevels.Warning, "Cannot connect {0}[{1}] because admin is banned", name, address == null ? "???" : address.ToString());
                    }
                    else if (!_passwordManager.Check(DomainTypes.Admin, name, password))
                    {
                        result = ConnectResults.FailedWrongAdminPassword;
                        Log.Default.WriteLine(LogLevels.Warning, "Cannot connect {0}[{1}] because wrong password", name, address == null ? "???" : address.ToString());
                    }
                    else
                    {
                        IAdmin admin = _factory.CreateAdmin(name, address, callback);
                        bool added = _adminManager.Add(admin);
                        if (!added)
                        {
                            result = ConnectResults.FailedInternalError;
                            Log.Default.WriteLine(LogLevels.Warning, "Cannot connect {0}[{1}] because it cannot be added to admin manager", name, address == null ? "???" : address.ToString());
                        }
                        else
                        {
                            // Handle connection lost
                            admin.ConnectionLost += OnAdminConnectionLost;
                            // Inform admin about connection succeed
                            admin.OnConnected(result, Version, admin.Id);
                            // Send message to other admins
                            lock (_adminManager.LockObject)
                                foreach (IAdmin target in _adminManager.Admins.Where(a => a != admin))
                                    target.OnAdminConnected(admin.Id, admin.Name, admin.Address.ToString());
                            // Hosts
                            foreach (IHost host in _hosts)
                                host.AddAdmin(admin);
                            //
                            Log.Default.WriteLine(LogLevels.Info, "Connect Admin {0}[{1}] succeed", name, address == null ? "???" : address.ToString());
                        }
                    }
                }
            }

            if (result != ConnectResults.Successfull) // Inform admin about connection fail
                callback.OnConnected(result, Version, Guid.Empty);
        }

        private void OnAdminDisconnect(IAdmin admin)
        {
            Log.Default.WriteLine(LogLevels.Info, "Disconnect admin:{0}", admin.Name);

            OnAdminLeft(admin, LeaveReasons.Disconnected);
        }

        private void OnAdminSendPrivateAdminMessage(IAdmin admin, IAdmin target, string message)
        {
            Log.Default.WriteLine(LogLevels.Info, "Admin send admin private message:{0} {1} {2}", admin.Name, target.Name, message);

            target.OnPrivateMessageReceived(admin.Id, message);
        }

        private void OnAdminSendPrivateMessage(IAdmin admin, IClient client, string message)
        {
            Log.Default.WriteLine(LogLevels.Info, "Admin send client private message:{0} {1} {2}", admin.Name, client.Name, message);

            client.OnPrivateMessageReceived(admin.Id, message);
        }

        private void OnAdminSendBroadcastMessage(IAdmin admin, string message)
        {
            Log.Default.WriteLine(LogLevels.Info, "Admin send broadcast message:{0} {1}", admin.Name, message);

            // Send message to clients
            lock (_clientManager.LockObject)
                foreach (IClient target in _clientManager.Clients)
                    target.OnBroadcastMessageReceived(admin.Id, message);
            // Send message to admin
            lock (_adminManager.LockObject)
                foreach (IAdmin target in _adminManager.Admins)
                    target.OnBroadcastMessageReceived(admin.Id, message);
        }

        private void OnAdminGetAdminList(IAdmin admin)
        {
            Log.Default.WriteLine(LogLevels.Info, "Admin asks for admin list:{0}", admin.Name);

            // Build list
            List<AdminData> list;
            lock (_adminManager.LockObject)
                list = _adminManager.Admins.Select(x => new AdminData
                {
                    Id = x.Id,
                    Name = x.Name,
                    Address = x.Address.ToString(),
                    ConnectTime = x.ConnectTime
                }).ToList();
            // Send list
            admin.OnAdminListReceived(list);
        }

        private void OnAdminGetClientList(IAdmin admin)
        {
            Log.Default.WriteLine(LogLevels.Info, "Admin asks for client list:{0}", admin.Name);

            // Build list
            List<ClientAdminData> list;
            lock (_clientManager.LockObject)
                list = _clientManager.Clients.Select(BuildClientAdminData).ToList();
            // Send list
            admin.OnClientListReceived(list);
        }

        private void OnAdminGetClientListInRoom(IAdmin admin, IGameRoom room)
        {
            Log.Default.WriteLine(LogLevels.Info, "Admin asks for client list in room:{0} {1}", admin.Name, room.Name);

            // Build list
            List<ClientAdminData> list = BuildClientAdminDatas(room);
            // Send list
            admin.OnClientListReceived(list);
        }

        private void OnAdminGetRoomList(IAdmin admin)
        {
            Log.Default.WriteLine(LogLevels.Info, "Admin asks for room list:{0}", admin.Name);

            // Build list
            List<GameRoomAdminData> list;
            lock (_gameRoomManager)
                list = _gameRoomManager.Rooms.Select(x => new GameRoomAdminData
                {
                    Id = x.Id,
                    Name = x.Name,
                    State = x.State,
                    Rule = x.Rule,
                    Options = x.Options,
                    Clients = BuildClientAdminDatas(x)
                }).ToList();
            // Send list
            admin.OnRoomListReceived(list);
        }

        private void OnAdminGetBannedList(IAdmin admin)
        {
            Log.Default.WriteLine(LogLevels.Info, "Admin asks for banned list:{0}", admin.Name);

            // Build list
            List<BanEntryData> list = _banManager.Entries.ToList();
            // Send list
            admin.OnBannedListReceived(list);
        }

        private void OnAdminKick(IAdmin admin, IClient client, string reason)
        {
            Log.Default.WriteLine(LogLevels.Info, "Admin kick: {0} {1} {2}", admin.Name, client.Name, reason);

            // Inform target and other clients
            lock (_clientManager.LockObject)
            {
                string message = String.Format("{0} has been kicked (reason: {1})", client.Name, reason);
                foreach (IClient other in _clientManager.Clients.Where(c => c != client))
                    other.OnServerMessageReceived(message);
                client.OnServerMessageReceived(String.Format("You have been kicked (reason: {0}", reason));
            }

            // Inform admin and other admins
            lock (_adminManager.LockObject)
            {
                string message = String.Format("{0} has been kicked by {1} (reason: {2})", client.Name, admin.Name, reason);
                foreach(IAdmin other in _adminManager.Admins.Where(a => a != admin))
                    other.OnServerMessageReceived(message);
                admin.OnServerMessageReceived(String.Format("You have kicked {0} (reason: {1})", client.Name, reason));
            }

            // Kick
            OnClientLeft(client, LeaveReasons.Kick);
        }

        private void OnAdminBan(IAdmin admin, IClient client, string reason)
        {
            Log.Default.WriteLine(LogLevels.Info, "Admin ban: {0} {1} {2}", admin.Name, client.Name, reason);

            // Inform target and other clients
            lock (_clientManager.LockObject)
            {
                string message = String.Format("{0} has been banned (reason: {1})", client.Name, reason);
                foreach (IClient other in _clientManager.Clients.Where(c => c != client))
                    other.OnServerMessageReceived(message);
                client.OnServerMessageReceived(String.Format("You have been banned (reason: {0}", reason));
            }

            // Inform admin and other admins
            lock (_adminManager.LockObject)
            {
                string message = String.Format("{0} has been banned by {1} (reason: {2})", client.Name, admin.Name, reason);
                foreach (IAdmin other in _adminManager.Admins.Where(a => a != admin))
                    other.OnServerMessageReceived(message);
                admin.OnServerMessageReceived(String.Format("You have banned {0} (reason: {1})", client.Name, reason));
            }

            // Ban
            _banManager.Ban(client.Name, client.Address, reason);

            // Kick
            OnClientLeft(client, LeaveReasons.Kick);
        }

        private void OnAdminRestartServer(IAdmin admin, int seconds)
        {
            Log.Default.WriteLine(LogLevels.Info, "Admin ask for server restart:{0} {1}", admin.Name, seconds);

            if (_isRestartRunning)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot restart server, a restart is already running");
                return;
            }
            if (seconds <= MinRestartDelay)
            {
                Log.Default.WriteLine(LogLevels.Error, "Cannot restart server, delay must be > to min restart delay ({0} seconds)", MinRestartDelay);
                return;
            }
            if (PerformRestartServer == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Cannot restart server, No event associated to PerformRestartServer");
                return;
            }

            SendRestartMessage(seconds);

            _isRestartRunning = true;
            _restartSecondLeft = seconds;
            _restartTimer = new Timer(RestartCallback, null, 0, 1000);
        }

        #endregion

        #region IDisposable

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_cancellationTokenSource != null)
                    _cancellationTokenSource.Dispose();
                if (_restartTimer != null)
                    _restartTimer.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
        
        private void OnClientConnectionLost(IClient client)
        {
            OnClientLeft(client, LeaveReasons.ConnectionLost);
        }

        private void OnAdminConnectionLost(IAdmin admin)
        {
            OnAdminLeft(admin, LeaveReasons.ConnectionLost);
        }

        private void OnClientLeft(IClient client, LeaveReasons reason)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client left:{0} {1}", client.Name, reason);

            // Remove from game room
            IGameRoom game = client.Game;
            if (game != null)
            {
                lock (game.LockObject)
                    game.Leave(client);
            }

            // Remove from client manager
            lock (_clientManager.LockObject)
                _clientManager.Remove(client);

            //// Inform client   TODO: should be done only if connection is still valid, otherwise recursive call via ExceptionFreeAction
            //client.OnDisconnected();

            // Inform clients and admins
            foreach (IClient target in _clientManager.Clients) // no need to check on client, already removed from collection
                target.OnClientDisconnected(client.Id, reason);
            foreach (IAdmin admin in _adminManager.Admins)
                admin.OnClientDisconnected(client.Id, reason);

            // Hosts
            foreach (IHost host in _hosts)
                host.RemoveClient(client);
        }

        private void OnAdminLeft(IAdmin admin, LeaveReasons reason)
        {
            Log.Default.WriteLine(LogLevels.Info, "Admin left:{0} {1}", admin.Name, reason);

            // Remove from admin manager
            lock (_adminManager.LockObject)
                _adminManager.Remove(admin);

            // Inform admin
            admin.OnDisconnected();

            // Inform other admins
            foreach (IAdmin other in _adminManager.Admins) // no need to check on admin, already removed from collection
                other.OnAdminDisconnected(admin.Id, reason);

            // Hosts
            foreach (IHost host in _hosts)
                host.RemoveAdmin(admin);
        }

        private void RestartCallback(object state)
        {
            Log.Default.WriteLine(LogLevels.Info, "RestartCallback: Seconds left:{0} IsRestartRunning:{1}", _restartSecondLeft, _isRestartRunning);

            // Final countdown
            if (_restartSecondLeft <= 0) // Let's restart
            {
                // Reset restart info
                _isRestartRunning = false;
                _restartTimer.Change(Timeout.Infinite, Timeout.Infinite);
                // Perform restart
                PerformRestartServer.Do(x => x(this));
            }
            else
            {
                // Decrement countdown
                _restartSecondLeft--;

                // Send message to clients and admins
                if (
                    _restartSecondLeft <= 10 // every second if less than 10 seconds left
                    || (_restartSecondLeft <= 60 && (_restartSecondLeft%10 == 0)) // every 10 seconds if less than 1 minute left
                    || (_restartSecondLeft <= 5*60 && (_restartSecondLeft%30 == 0)) // every 30 seconds if less than 5 minutes left
                    || (_restartSecondLeft%60 == 0) // every minute otherwise
                    )
                    SendRestartMessage(_restartSecondLeft);
            }
        }

        private void TimeoutTask()
        {
            Log.Default.WriteLine(LogLevels.Info, "TimeoutTask started");

            List<IClient> timeoutClients = new List<IClient>();
            try
            {
                while (true)
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        Log.Default.WriteLine(LogLevels.Info, "Stop background task event raised");
                        break;
                    }

                    // TODO: don't check every client on each loop, should test 10 by 10
                    // Check client timeout + send heartbeat if needed
                    timeoutClients.Clear();
                    List<IClient> clients;
                    lock (_clientManager.LockObject)
                        clients = _clientManager.Clients.ToList();
                    foreach (IClient client in clients)
                    {
                        // Check client timeout
                        TimeSpan timespan = DateTime.Now - client.LastActionFromClient;
                        if (timespan.TotalMilliseconds > TimeoutDelay && IsTimeoutDetectionActive)
                        {
                            Log.Default.WriteLine(LogLevels.Info, "Timeout++ for client {0}", client.Name);
                            // Update timeout count
                            client.SetTimeout();
                            if (client.TimeoutCount >= MaxTimeoutCountBeforeDisconnection)
                            {
                                Log.Default.WriteLine(LogLevels.Info, "Max Timeout count reached for client {0} -> disconnect", client.Name);
                                timeoutClients.Add(client);
                            }
                        }

                        // Send heartbeat if needed
                        TimeSpan delayFromPreviousHeartbeat = DateTime.Now - client.LastActionToClient;
                        if (delayFromPreviousHeartbeat.TotalMilliseconds > HeartbeatDelay)
                            client.OnHeartbeatReceived(); // -> this could fail and remove client from client manager
                    }
                    // Remove timeout clients if any
                    if (timeoutClients.Any())
                        foreach(IClient timeoutClient in timeoutClients)
                            OnClientLeft(timeoutClient, LeaveReasons.Timeout);

                    // Stop task if stop event is raised
                    bool signaled = _cancellationTokenSource.Token.WaitHandle.WaitOne(10);
                    if (signaled)
                    {
                        Log.Default.WriteLine(LogLevels.Info, "Stop background task event raised");
                        break;
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                Log.Default.WriteLine(LogLevels.Error, "TimeoutTask exception. Exception: {0}", ex);
            }

            Log.Default.WriteLine(LogLevels.Info, "TimeoutTask stopped");
        }

        private void SendRestartMessage(int seconds)
        {
            string msg = BuildRestartMessage(seconds);
            lock (_clientManager.LockObject)
                foreach (IClient client in _clientManager.Clients)
                    client.OnServerMessageReceived(msg);
            lock (_adminManager.LockObject)
                foreach (IAdmin other in _adminManager.Admins)
                    other.OnServerMessageReceived(msg);
        }

        private static string BuildRestartMessage(int seconds)
        {
            return seconds <= 0
                ? "***Server will restart NOW***"
                : String.Format("***Server will restart in {0} seconds***", seconds);
        }

        private static List<ClientAdminData> BuildClientAdminDatas(IGameRoom room)
        {
            lock (room)
                return room.Clients.Select(BuildClientAdminData).ToList();
        }

        private static ClientAdminData BuildClientAdminData(IClient client)
        {
            return new ClientAdminData
            {
                Id = client.Id,
                Name = client.Name,
                Team = client.Team,
                Address = client.Address.ToString(),
                State = client.State,
                Roles = client.Roles,
                ConnectTime = client.ConnectTime,
                LastActionFromClient = client.LastActionFromClient,
                LastActionToClient = client.LastActionToClient,
                TimeoutCount = client.TimeoutCount,
            };
        }
        
        private static List<ClientData> BuildClientDatas(IGameRoom room)
        {
            lock (room)
                return room.Clients.Select(BuildClientData).ToList();
        }

        private static ClientData BuildClientData(IClient client)
        {
            return new ClientData
            {
                Id = client.Id,
                Name = client.Name,
                GameId = client.Game == null ? Guid.Empty : client.Game.Id,
                IsPlayer = client.IsPlayer,
                IsSpectator = client.IsSpectator,
                IsGameMaster = client.IsGameMaster
            };
        }

        // Check if every events of instance are handled
        private static bool CheckEvents<T>(T instance)
        {
            Type t = instance.GetType();
            EventInfo[] events = t.GetEvents();
            foreach (EventInfo e in events)
            {
                if (e.DeclaringType == null)
                    return false;
                FieldInfo fi = e.DeclaringType.GetField(e.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if (fi == null)
                    return false;
                object value = fi.GetValue(instance);
                if (value == null)
                    return false;
            }
            return true;
        }
    }
}
