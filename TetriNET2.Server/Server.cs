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
using TetriNET2.Common.Logger;
using TetriNET2.Common.Randomizer;
using TetriNET2.Server.Interfaces;

namespace TetriNET2.Server
{
    public sealed class Server : IServer, IDisposable
    {
        private const int HeartbeatDelay = 300; // in ms
        private const int TimeoutDelay = 500; // in ms
        private const int MaxTimeoutCountBeforeDisconnection = 3;
        private const bool IsTimeoutDetectionActive = true;

        private readonly List<IHost> _hosts = new List<IHost>();
        private readonly IFactory _factory;
        private readonly IBanManager _banManager;
        private readonly IClientManager _clientManager;
        private readonly IAdminManager _adminManager;
        private readonly IGameRoomManager _gameRoomManager;

        private CancellationTokenSource _cancellationTokenSource;
        private Task _timeoutTask;

        public Server(IFactory factory, IBanManager banManager, IClientManager clientManager, IAdminManager adminManager, IGameRoomManager gameRoomManager)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");
            if (banManager == null)
                throw new ArgumentNullException("banManager");
            if (clientManager == null)
                throw new ArgumentNullException("clientManager");
            if (adminManager == null)
                throw new ArgumentNullException("adminManager");
            if (gameRoomManager == null)
                throw new ArgumentNullException("gameRoomManager");

            _factory = factory;
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

        public ServerStates State { get; private set; }

        public Versioning Version { get; private set; }

        public bool AddHost(IHost host)
        {
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
            host.ClientConnect += OnClientConnect;
            host.ClientDisconnect += OnClientDisconnect;
            host.ClientHeartbeat += OnClientHeartbeat;

            // Wait+Game room
            host.ClientSendPrivateMessage += OnClientSendPrivateMessage;
            host.ClientSendBroadcastMessage += OnClientSendBroadcastMessage;
            host.ClientChangeTeam += OnClientChangeTeam;

            // Wait room
            host.ClientJoinGame += OnClientJoinGame;
            host.ClientJoinRandomGame += OnClientJoinRandomGame;
            host.ClientCreateAndJoinGame += OnClientCreateAndJoinGame;

            // Game room as game master (player or spectator)
            host.ClientStartGame += OnClientStartGame;
            host.ClientStopGame += OnClientStopGame;
            host.ClientPauseGame += OnClientPauseGame;
            host.ClientResumeGame += OnClientResumeGame;
            host.ClientChangeOptions += OnClientChangeOptions;
            host.ClientVoteKick += OnClientVoteKick;
            host.ClientVoteKickAnswer += OnClientVoteKickAnswer;
            host.ClientResetWinList += OnClientResetWinList;

            // Game room as player or spectator
            host.ClientLeaveGame += OnClientLeaveGame;

            // Game room as player
            host.ClientPlacePiece += OnClientPlacePiece;
            host.ClientModifyGrid += OnClientModifyGrid;
            host.ClientUseSpecial += OnClientUseSpecial;
            host.ClientClearLines += OnClientClearLines;
            host.ClientGameLost += OnClientGameLost;
            host.ClientFinishContinuousSpecial += OnClientFinishContinuousSpecial;
            host.ClientEarnAchievement += OnClientEarnAchievement;

            // ------
            // Admin
            // Connect/Disconnect
            host.AdminConnect += OnAdminConnect;
            host.AdminDisconnect += OnAdminDisconnect;

            // Messaging
            host.AdminSendPrivateAdminMessage += OnAdminSendPrivateAdminMessage;
            host.AdminSendPrivateMessage += OnAdminSendPrivateMessage;
            host.AdminSendBroadcastMessage += OnAdminSendBroadcastMessage;

            // Monitoring
            host.AdminGetAdminList += OnAdminGetAdminList;
            host.AdminGetClientList += OnAdminGetClientList;
            host.AdminGetClientListInRoom += OnAdminGetClientListInRoom;
            host.AdminGetRoomList += OnAdminGetRoomList;
            host.AdminGetBannedList += OnAdminGetBannedList;

            // Kick/Ban
            host.AdminKick += OnAdminKick;
            host.AdminBan += OnAdminBan;

            // Server commands
            host.AdminRestartServer += OnAdminRestartServer;

            Debug.Assert(CheckEvents(host), "Every host events must be handled");

            return true;
        }

        public void SetVersion(int major, int minor)
        {
            if (State != ServerStates.Waiting)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot change version while server is already started");
                return;
            }

            Version = new Versioning
                {
                    Major = major,
                    Minor = minor
                };
        }

        public void Start()
        {
            Log.Default.WriteLine(LogLevels.Info, "Starting server");

            if (Version == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Cannot start server until a version has been specified");
                return;
            }
            if (_hosts.Count != 0)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot start server without any host");
                return;
            }
            if (State != ServerStates.Waiting)
            {
                Log.Default.WriteLine(LogLevels.Info, "Server already started");
                return;
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
        }

        public void Stop()
        {
            Log.Default.WriteLine(LogLevels.Info, "Stopping server");

            if (State != ServerStates.Started)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Server not started");
                return;
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
        }

        #endregion

        private void OnClientConnectLost(IClient client)
        {
            OnClientLeft(client, LeaveReasons.ConnectionLost);
        }

        private void OnClientLeft(IClient client, LeaveReasons reason)
        {
            throw new NotImplementedException();
            // TODO: 
            //  remove from client manager, wait room, game room, host
            //  if playing, check if game can continue without player
            //  inform other clients, players/spectators, admins
        }

        private void OnAdminLeft(IClient client, LeaveReasons reason)
        {
            throw new NotImplementedException();
            // TODO: remove from admin manager, inform other admin
        }

        #region IHost events handler

        // Client
        private void OnClientConnect(ITetriNETCallback callback, IPAddress address, Versioning version, string name, string team)
        {
            Log.Default.WriteLine(LogLevels.Info, "Connect client {0}[1] {2}.{3}", name, address == null ? "???" : address.ToString(), version == null ? -1 : version.Major, version == null ? -1 : version.Minor);

            ConnectResults result = ConnectResults.Successfull;

            if (version == null || Version.Major != version.Major || Version.Minor != version.Minor)
                result = ConnectResults.FailedIncompatibleVersion;
            else
            {
                lock (_clientManager.LockObject)
                {
                    if (_clientManager.Contains(name, callback))
                    {
                        result = ConnectResults.FailedPlayerAlreadyExists;
                        Log.Default.WriteLine(LogLevels.Warning, "Cannot connect {0}[{1}] because it already exists", name, address == null ? "???" : address.ToString());
                    }
                    else if (_clientManager.ClientCount >= _clientManager.MaxClients)
                    {
                        result = ConnectResults.FailedTooManyClients;
                        Log.Default.WriteLine(LogLevels.Warning, "Cannot connect {0}[{1}] because too many clients already connected", name, address == null ? "???" : address.ToString());
                    }
                    else if (String.IsNullOrEmpty(name) || name.Length > 20)
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
                            List<GameDescription> games;
                            lock (_gameRoomManager.LockObject)
                                games = _gameRoomManager.Rooms.Select(r => new GameDescription
                                    {
                                        Id = r.Id,
                                        Name = r.Name,
                                        Players = r.Players.Select(p => p.Name).ToList(),
                                    }).ToList();
                            // Handle connection lost
                            client.ConnectionLost += OnClientConnectLost;
                            // Inform client about connection succeed
                            client.OnConnected(result, Version, client.Id, games);
                            // Client is alive
                            client.ResetTimeout();
                            // Send message to clients
                            lock (_clientManager.LockObject)
                                foreach (IClient target in _clientManager.Clients)
                                    target.OnClientConnected(client.Id, client.Name, client.Team);
                            // Send message to admin
                            lock (_adminManager.LockObject)
                                foreach (IAdmin target in _adminManager.Admins)
                                    target.OnClientConnected(client.Id, client.Name, client.Team);
                            //
                            Log.Default.WriteLine(LogLevels.Info, "Connect {0}[{1}] succeed", name, address == null ? "???" : address.ToString());
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
            if (game.Password != null && game.Password != password)
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
                client.OnGameJoined(result, game.Id, null);
        }

        private void OnClientJoinRandomGame(IClient client, bool asSpectator)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client join random game:{0} {1}", client.Name, asSpectator);

            GameJoinResults result = GameJoinResults.Successfull;
            // Search a suitable room
            IGameRoom game;
            lock (_gameRoomManager.LockObject)
                game = _gameRoomManager.Rooms.FirstOrDefault(x => x.Password == null && ((!asSpectator && x.PlayerCount < x.MaxPlayers) || (asSpectator && x.SpectatorCount < x.MaxSpectators)));

            if (game == null)
            {
                result = GameJoinResults.FailedNoRoomAvailable;
                Log.Default.WriteLine(LogLevels.Info, "Client {0} cannot find a suitable random game");
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
                client.OnGameJoined(result, Guid.Empty, null);
        }

        private void OnClientCreateAndJoinGame(IClient client, string name, string password, GameRules rule, bool asSpectator)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client create and join game: {0} {1} {2}", client.Name, name, asSpectator);

            GameCreateResults result = GameCreateResults.Successfull;

            lock(_gameRoomManager.LockObject)
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
                    // TODO: fill options in function of rule
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
                        GameDescription description = new GameDescription
                        {
                            Id = game.Id,
                            Name = game.Name,
                            Rule = game.Rule,
                            Players = null
                        };

                        // Inform client
                        client.OnGameCreated(result, description);

                        // Inform other clients
                        lock (_clientManager.LockObject)
                            foreach (IClient target in _clientManager.Clients.Where(c => c != client))
                                target.OnClientGameCreated(client.Id, description);

                        // Inform admins
                        lock (_adminManager.LockObject)
                            foreach (IAdmin target in _adminManager.Admins)
                                target.OnGameCreated(client.Id, description);

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
            if (result != GameCreateResults.Successfull)
                client.OnGameCreated(result, null);
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
            game.StartGame(); // StartGame is responsible for using Callback
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
            game.StopGame(); // StopGame is responsible for using Callback
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
            game.PauseGame(); // PauseGame is responsible for using Callback
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
            game.ResumeGame(); // ResumeGame is responsible for using Callback
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
            bool accepted =
                RangeRandom.SumOccurancies(options.PieceOccurancies) == 100 &&
                RangeRandom.SumOccurancies(options.SpecialOccurancies) == 100 &&
                options.InventorySize >= 1 && options.InventorySize <= 15 &&
                options.LinesToMakeForSpecials >= 1 && options.LinesToMakeForSpecials <= 4 &&
                options.SpecialsAddedEachTime >= 1 && options.SpecialsAddedEachTime <= 4 &&
                options.DelayBeforeSuddenDeath >= 0 && options.DelayBeforeSuddenDeath <= 15 &&
                options.SuddenDeathTick >= 1 && options.SuddenDeathTick <= 30 &&
                options.StartingLevel >= 0 && options.StartingLevel <= 100;
            if (accepted)
                game.ChangeOptions(options);
            else
                Log.Default.WriteLine(LogLevels.Info, "Invalid options");
        }

        private void OnClientVoteKick(IClient client, IClient target)
        {
            throw new NotImplementedException();
        }

        private void OnClientVoteKickAnswer(IClient client, bool accepted)
        {
            throw new NotImplementedException();
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
            game.ResetWinList();
        }

        private void OnClientLeaveGame(IClient client)
        {
            throw new NotImplementedException();
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

            game.PlacePiece(client, pieceIndex, highestIndex, piece, orientation, posX, posY, grid);
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

            game.ModifyGrid(client, grid);
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

            game.UseSpecial(client, target, special);
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

            game.ClearLines(client, count);
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

            game.GameLost(client);
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

            game.FinishContinuousSpecial(client, special);
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

            game.EarnAchievement(client, achievementId, achievementTitle);
        }

        // Admin
        private void OnAdminConnect(ITetriNETAdminCallback callback, IPAddress address, Versioning version, string name, string password)
        {
            throw new NotImplementedException();
        }

        private void OnAdminDisconnect(IAdmin admin)
        {
            throw new NotImplementedException();
        }

        private void OnAdminSendPrivateAdminMessage(IAdmin admin, IAdmin target, string message)
        {
            throw new NotImplementedException();
        }

        private void OnAdminSendPrivateMessage(IAdmin admin, IClient client, string message)
        {
            throw new NotImplementedException();
        }

        private void OnAdminSendBroadcastMessage(IAdmin admin, string message)
        {
            throw new NotImplementedException();
        }

        private void OnAdminGetAdminList(IAdmin admin)
        {
            throw new NotImplementedException();
        }

        private void OnAdminGetClientList(IAdmin admin)
        {
            throw new NotImplementedException();
        }

        private void OnAdminGetClientListInRoom(IAdmin admin, IGameRoom room)
        {
            throw new NotImplementedException();
        }

        private void OnAdminGetRoomList(IAdmin admin)
        {
            throw new NotImplementedException();
        }

        private void OnAdminGetBannedList(IAdmin admin)
        {
            throw new NotImplementedException();
        }

        private void OnAdminKick(IAdmin admin, IClient client, string reason)
        {
            throw new NotImplementedException();
        }

        private void OnAdminBan(IAdmin admin, IClient client, string reason)
        {
            throw new NotImplementedException();
        }

        private void OnAdminRestartServer(IAdmin admin, int seconds)
        {
            throw new NotImplementedException();
        }

        #endregion

        private void TimeoutTask()
        {
            Log.Default.WriteLine(LogLevels.Info, "TimeoutTask started");

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
                    foreach (IClient client in _clientManager.Clients)
                    {
                        // Check player timeout
                        TimeSpan timespan = DateTime.Now - client.LastActionFromClient;
                        if (timespan.TotalMilliseconds > TimeoutDelay && IsTimeoutDetectionActive)
                        {
                            Log.Default.WriteLine(LogLevels.Info, "Timeout++ for client {0}", client.Name);
                            // Update timeout count
                            client.SetTimeout();
                            if (client.TimeoutCount >= MaxTimeoutCountBeforeDisconnection)
                                OnClientLeft(client, LeaveReasons.Timeout);
                        }

                        // Send heartbeat if needed
                        TimeSpan delayFromPreviousHeartbeat = DateTime.Now - client.LastActionToClient;
                        if (delayFromPreviousHeartbeat.TotalMilliseconds > HeartbeatDelay)
                            client.OnHeartbeatReceived();
                    }

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

        #region IDisposable

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cancellationTokenSource.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
