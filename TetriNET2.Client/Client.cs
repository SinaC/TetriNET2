using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using TetriNET2.Client.Interfaces;
using TetriNET2.Common.ActionQueue;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Helpers;
using TetriNET2.Common.Logger;

// TODO: 
//  use ClientInfo instead of ClientData
//  check state on every callback and call
//  move
//  board
//  special

namespace TetriNET2.Client
{
    public enum States
    {
        Created, // --> Connecting
        Connecting, // --> Connected
        Connected, // --> WaitInGame
        WaitInGame, // --> Connected | Playing
        Playing, // --> Connected | GameLost | Paused
        GamePaused, // --> Connected | Playing
        GameLost // --> Connected | WaitInGame
    }

    public class Client : IClient
    {
        private const int HeartbeatDelay = 300; // in ms
        private const int TimeoutDelay = 500; // in ms
        private const int MaxTimeoutCountBeforeDisconnection = 3;
        private const bool IsTimeoutDetectionActive = false;
        private const bool AutomaticallyMoveDown = false;
        private const int GameTimerIntervalStartValue = 1050; // level 0: 1050, level 1: 1040, ..., level 100: 50

        private readonly IFactory _factory;
        private readonly IActionQueue _actionQueue;
        private readonly List<ClientData> _clients;
        private readonly List<ClientData> _gameClients;
        private readonly List<GameData> _games;
        private readonly IInventory _inventory;
        private readonly IPieceBag _pieceBag;

        private DateTime _gameStartTime;
        private DateTime _gamePausedTime;

        private IProxy _proxy;

        private readonly Task _timeoutTask;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly System.Timers.Timer _gameTimer;

        private States _state;
        private Guid _clientId;
        private bool _isGameMaster;
        private DateTime _lastActionFromServer;
        private int _timeoutCount;
        private int _pieceIndex;

        public Client(IFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _actionQueue = factory.CreateActionQueue();
            _clients = new List<ClientData>();
            _gameClients = new List<ClientData>();
            _games = new List<GameData>();
            _pieceBag = factory.CreatePieceBag(32);
            _inventory = factory.CreateInventory(10);

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

            _state = States.Created;
            _clientId = Guid.Empty;
            _lastActionFromServer = DateTime.Now;
            _timeoutCount = 0;
            _pieceIndex = 0;

            _gameTimer = new System.Timers.Timer
            {
                Interval = GameTimerIntervalStartValue
            };
            _gameTimer.Elapsed += GameTimerOnElapsed;

            _cancellationTokenSource = new CancellationTokenSource();
            _timeoutTask = Task.Factory.StartNew(TimeoutTask, _cancellationTokenSource.Token);
            _actionQueue.Start(_cancellationTokenSource);
        }

        #region IClient
        
        #region ITetriNETClientCallback

        public void OnConnected(ConnectResults result, Versioning serverVersion, Guid clientId, List<GameData> games)
        {
            ClientData client = null;
            if (result == ConnectResults.Successfull)
            {
                Log.Default.WriteLine(LogLevels.Info, "Connected as player {0}", clientId);

                _clients.Clear();
                _gameClients.Clear();
                _games.Clear();

                _games.AddRange(games);

                _clientId = clientId;

                client = new ClientData
                    {
                        Id = clientId,
                        Name = Name,
                        Team = Team,
                    };
                _clients.Add(client);

                _state = States.Connected;
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Warning, "Wrong id {0}", clientId);

                _state = States.Created;
            }

            Connected.Do(x => x(result, serverVersion, client, games));
        }

        public void OnDisconnected()
        {
            Log.Default.WriteLine(LogLevels.Info, "Disconnected");

            Disconnected.Do(x => x());

            _state = States.Created;
            _clientId = Guid.Empty;
            _isGameMaster = false;
        }

        public void OnHeartbeatReceived()
        {
            ResetTimeout();
        }

        public void OnServerStopped()
        {
            Log.Default.WriteLine(LogLevels.Info, "Server stopped");

            ResetTimeout();
            OnConnectionLost();

            _state = States.Created;
            _clientId = Guid.Empty;
            _isGameMaster = false;
        }

        public void OnGameListReceived(List<GameData> games)
        {
            Log.Default.WriteLine(LogLevels.Info, "Game list received");

            ResetTimeout();

            _games.Clear();
            _games.AddRange(games);

            GameListReceived.Do(x => x(games));
        }

        public void OnClientListReceived(List<ClientData> clients)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client list received");

            ResetTimeout();

            _clients.Clear();
            _clients.AddRange(clients);

            ClientListReceived.Do(x => x(clients));
        }

        public void OnGameClientListReceived(List<ClientData> clients)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client in game received");
            
            ResetTimeout();

            _gameClients.Clear();
            _gameClients.AddRange(clients);

            GameClientListReceived.Do(x => x(clients));
        }

        public void OnClientConnected(Guid clientId, string name, string team)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client connected {0} {1} {2}", clientId, name, team);

            ResetTimeout();

            ClientData client = _clients.FirstOrDefault(x => x.Id == clientId);
            if (client == null)
               client = new ClientData
                {
                    Id = clientId,
                    Name = name,
                    Team = team
                };
            else
            {
                client.Id = clientId;
                client.Name = name;
                client.Team = team;
            }

            ClientConnected.Do(x => x(client, name, team));
        }

        public void OnClientDisconnected(Guid clientId, LeaveReasons reason)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client disconnected {0} {1}", clientId, reason);

            ResetTimeout();

            ClientData clientInGame = _gameClients.FirstOrDefault(x => x.Id == clientId);

            // If client was in game, remove it
            if (clientInGame != null)
            {
                Log.Default.WriteLine(LogLevels.Info, "Client disconnected {0} and removed from game", clientId);

                _gameClients.RemoveAll(x => x.Id == clientId);
                ClientGameLeft.Do(x => x(clientInGame));
            }

            // Remove client
            ClientData client = _clients.FirstOrDefault(x => x.Id == clientId);
            if (client != null)
                _clients.RemoveAll(x => x.Id == clientId);

            ClientDisconnected.Do(x => x(client, reason));
        }

        public void OnClientGameCreated(Guid clientId, GameData game)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client {0} creates game {1}", clientId, game?.Id ?? Guid.Empty);

            ResetTimeout();

            _games.Add(game);
            
            ClientData client = _clients.FirstOrDefault(x => x.Id == clientId);
            ClientGameCreated.Do(x => x(client, game));
        }

        public void OnServerGameCreated(GameData game)
        {
            Log.Default.WriteLine(LogLevels.Info, "Server creates game {0}", game?.Id ?? Guid.Empty);

            ResetTimeout();

            _games.Add(game);

            ServerGameCreated.Do(x => x(game));
        }

        public void OnServerGameDeleted(Guid gameId)
        {
            Log.Default.WriteLine(LogLevels.Info, "Server deletes game {0}", gameId);

            ResetTimeout();

            GameData game = _games.FirstOrDefault(x => x.Id == gameId);
            if (game != null)
                _games.RemoveAll(x => x.Id == gameId);

            ServerGameDeleted.Do(x => x(game));
        }

        public void OnServerMessageReceived(string message)
        {
            Log.Default.WriteLine(LogLevels.Info, "Server message {0}", message);

            ResetTimeout();

            ServerMessageReceived.Do(x => x(message));
        }

        public void OnBroadcastMessageReceived(Guid clientId, string message)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client {0} broadcasts message {1}", clientId, message);

            ResetTimeout();

            ClientData client = _clients.FirstOrDefault(x => x.Id == clientId);
            BroadcastMessageReceived.Do(x => x(client, message));
        }

        public void OnPrivateMessageReceived(Guid clientId, string message)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client {0} sends message {1}", clientId, message);

            ResetTimeout();

            ClientData client = _clients.FirstOrDefault(x => x.Id == clientId);
            PrivateMessageReceived.Do(x => x(client, message));
        }

        public void OnTeamChanged(Guid clientId, string team)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client {0} changes team {1}", clientId, team);

            ResetTimeout();

            ClientData client = _clients.FirstOrDefault(x => x.Id == clientId);
            if (client != null)
                client.Team = team;

            TeamChanged.Do(x => x(client, team));
        }

        public void OnGameCreated(GameCreateResults result, GameData game)
        {
            Log.Default.WriteLine(LogLevels.Info, "Game created {0} {1}", result, game?.Id ?? Guid.Empty);

            ResetTimeout();

            if (result == GameCreateResults.Successfull)
            {
                Log.Default.WriteLine(LogLevels.Info, "Game {0} created successfully", game?.Id ?? Guid.Empty);

                _games.Add(game);
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Warning, "Failed to create game {0}", result);
            }

            GameCreated.Do(x => x(result, game));
        }

        public void OnGameJoined(GameJoinResults result, GameData game, bool isGameMaster)
        {
            Log.Default.WriteLine(LogLevels.Info, "Game jointed {0} {1} {2}", result, game?.Id ?? Guid.Empty, isGameMaster);

            ResetTimeout();

            GameData innerGame = game == null ? null : _games.FirstOrDefault(x => x.Id == game.Id);
            if (result == GameJoinResults.Successfull && game != null)
            {
                Log.Default.WriteLine(LogLevels.Info, "Game {0} joined successfully. Master {1}", game.Id, isGameMaster);

                _state = States.WaitInGame;
                _isGameMaster = isGameMaster;

                _gameClients.Clear();
                foreach(ClientData client in game.Clients)
                {
                    ClientData innerClient = _clients.FirstOrDefault(x => x.Id == client.Id);
                    if (innerClient != null)
                        _gameClients.Add(innerClient);
                }
                if (innerGame != null)
                {
                    innerGame.Name = game.Name;
                    innerGame.Options = game.Options;
                    innerGame.Rule = game.Rule;
                }
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Warning, "Failed to join game {0} {1}", game?.Id ?? Guid.Empty, result);
            }

            GameJoined.Do(x => x(result, innerGame, isGameMaster));
        }

        public void OnGameLeft()
        {
            Log.Default.WriteLine(LogLevels.Info, "Game left");

            ResetTimeout();

            _state = States.Connected;

            GameLeft.Do(x => x());
        }

        public void OnClientGameJoined(Guid clientId, bool asSpectator)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client {0} join game spectator: {1}", clientId, asSpectator);

            ResetTimeout();

            if (_gameClients.All(x => x.Id != clientId))
            {
                ClientData data = _clients.FirstOrDefault(x => x.Id == clientId);
                if (data != null)
                {
                    Log.Default.WriteLine(LogLevels.Info, "Client {0} joined game. Spectator {1}", clientId, asSpectator);

                    _gameClients.Add(data);
                }
                else
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Client {0} joining game but not found in client list");

                    // Get clients
                    _proxy.Do(x => x.ClientGetClientList());
                    // Get clients in game
                    _proxy.Do(x => x.ClientGetGameClientList());
                }
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Warning, "Client {0} already in game", clientId);

                // Get clients in game
                _proxy.Do(x => x.ClientGetGameClientList());
            }

            ClientData player = _gameClients.FirstOrDefault(x => x.Id == clientId);
            if (player != null)
            {
                player.IsPlayer = !asSpectator;
                player.IsSpectator = asSpectator;
            }
            ClientGameJoined.Do(x => x(player, asSpectator));
        }

        public void OnClientGameLeft(Guid clientId)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client {0} left", clientId);

            ResetTimeout();

            if (_gameClients.Any(x => x.Id == clientId))
            {
                Log.Default.WriteLine(LogLevels.Info, "Client {0} left game", clientId);

                _gameClients.RemoveAll(x => x.Id == clientId);
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Warning, "Client {0} not in game", clientId);

                // Get clients in game
                _proxy.Do(x => x.ClientGetGameClientList());
            }

            ClientData player = _gameClients.FirstOrDefault(x => x.Id == clientId);
            ClientGameLeft.Do(x => x(player));
        }

        public void OnGameMasterModified(Guid playerId)
        {
            Log.Default.WriteLine(LogLevels.Info, "Game master modified {1}", playerId);

            ResetTimeout();

            foreach (ClientData client in _gameClients)
                client.IsGameMaster = client.Id == playerId;
            
            if (playerId == _clientId)
            {
                Log.Default.WriteLine(LogLevels.Info, "New game master !!!");
                _isGameMaster = true;
            }

            ClientData newMaster = _gameClients.FirstOrDefault(x => x.Id == playerId);
            GameMasterModified.Do(x => x(newMaster));
        }

        public void OnGameStarted(List<Pieces> pieces)
        {
            ResetTimeout();

            if (_state == States.WaitInGame)
            {
                _gameStartTime = DateTime.Now;
                _state = States.Playing;

                // TODO: handle spectator mode

                // TODO: add piece to queue, start action queue, start various timer, initialize play variables
                _actionQueue.Clear();
                _pieceBag.Reset();
                for (int i = 0; i < pieces.Count; i++)
                    _pieceBag[i] = pieces[i];
                _pieceIndex = 0;
                _inventory.Reset(10); // TODO: get inventory size from options
                LinesCleared = 0;
                Level = 0; // TODO: get starting level from options
                Score = 0;
                //_gameTimer.Interval = TODO: compute interval from level
                // TODO: reset player/opponents board/state

                _gameTimer.Start();
                
                GameStarted.Do(x => x());
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, "Cannot start game, wrong state {0}", _state);
        }

        public void OnGamePaused()
        {
            ResetTimeout();

            if (_state == States.Playing)
            {
                _gamePausedTime = DateTime.Now;
                _state = States.GamePaused;

                // TODO
                GamePaused.Do(x => x());
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, "Cannot pause game, wrong state {0}", _state);
        }

        public void OnGameResumed()
        {
            ResetTimeout();

            if (_state == States.GamePaused)
            {
                _state = States.Playing;

                // TODO
                GameResumed.Do(x => x());
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, "Cannot resume game, wrong state {0}", _state);
        }

        public void OnGameFinished(GameFinishedReasons reason, GameStatistics statistics)
        {
            ResetTimeout();

            if (_state == States.Playing || _state == States.GamePaused || _state == States.GameLost)
            {
                _state = States.WaitInGame;

                _actionQueue.Clear();
                _gameTimer.Stop();

                // TODO: update statistics, reset play variables
                GameFinished.Do(x => x(reason, statistics));
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, "Cannot finish game, wrong state {0}", _state);
        }

        public void OnWinListModified(List<WinEntry> winEntries)
        {
            ResetTimeout();

            throw new NotImplementedException();
        }

        public void OnGameOptionsChanged(GameOptions gameOptions)
        {
            ResetTimeout();

            throw new NotImplementedException();
        }

        public void OnVoteKickAsked(Guid sourceId, Guid targetId, string reason)
        {
            ResetTimeout();

            throw new NotImplementedException();
        }

        public void OnAchievementEarned(Guid playerId, int achievementId, string achievementTitle)
        {
            ResetTimeout();

            throw new NotImplementedException();
        }

        public void OnPiecePlaced(int firstIndex, List<Pieces> nextPieces)
        {
            ResetTimeout();

            throw new NotImplementedException();
        }

        public void OnPlayerWon(Guid playerId)
        {
            ResetTimeout();

            throw new NotImplementedException();
        }

        public void OnPlayerLost(Guid playerId)
        {
            ResetTimeout();

            throw new NotImplementedException();
        }

        public void OnServerLinesAdded(int count)
        {
            ResetTimeout();

            throw new NotImplementedException();
        }

        public void OnPlayerLinesAdded(Guid playerId, int specialId, int count)
        {
            ResetTimeout();

            throw new NotImplementedException();
        }

        public void OnSpecialUsed(Guid playerId, Guid targetId, int specialId, Specials special)
        {
            ResetTimeout();

            throw new NotImplementedException();
        }

        public void OnGridModified(Guid playerId, byte[] grid)
        {
            ResetTimeout();

            throw new NotImplementedException();
        }

        public void OnContinuousSpecialFinished(Guid playerId, Specials special)
        {
            ResetTimeout();

            throw new NotImplementedException();
        }

        #endregion

        public string Name { get; private set; }

        public string Team { get; private set; }

        public Versioning Version { get; private set; }

        public int LinesCleared { get; private set; }

        public int Score { get; private set; }

        public int Level { get; private set; }

        public bool IsPlaying => _state == States.Playing;

        public IReadOnlyCollection<ClientData> Clients => _clients;

        public IReadOnlyCollection<ClientData> GameClients => _gameClients;

        public IReadOnlyCollection<GameData> Games => _games;

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
        public event GameListReceivedEventHandler GameListReceived;
        public event ClientListReceivedEventHandler ClientListReceived;
        public event GameClientListReceivedEventHandler GameClientListReceived;
        public event ClientConnectedEventHandler ClientConnected;
        public event ClientDisconnectedEventHandler ClientDisconnected;
        public event ClientGameCreatedEventHandler ClientGameCreated;
        public event ServerGameCreatedEventHandler ServerGameCreated;
        public event ServerGameDeletedEventHandler ServerGameDeleted;
        public event ServerMessageReceivedEventHandler ServerMessageReceived;
        public event BroadcastMessageReceivedEventHandler BroadcastMessageReceived;
        public event PrivateMessageReceivedEventHandler PrivateMessageReceived;
        public event TeamChangedEventHandler TeamChanged;
        public event GameCreatedEventHandler GameCreated;
        public event GameJoinedEventHandler GameJoined;
        public event GameLeftEventHandler GameLeft;
        public event ClientGameJoinedEventHandler ClientGameJoined;
        public event ClientGameLeftEventHandler ClientGameLeft;
        public event GameMasterModifiedEventHandler GameMasterModified;
        public event GameStartedEventHandler GameStarted;
        public event GamePausedEventHandler GamePaused;
        public event GameResumedEventHandler GameResumed;
        public event GameFinishedEventHandler GameFinished;
        public event WinListModifiedEventHandler WinListModified;
        public event GameOptionsChangedEventHandler GameOptionsChanged;
        public event VoteKickAskedEventHandler VoteKickAsked;
        public event AchievementEarnedEventHandler AchievementEarned;
        public event PiecePlacedEventHandler PiecePlaced;
        public event PlayerWonEventHandler PlayerWon;
        public event PlayerLostEventHandler PlayerLost;
        public event ServerLinesAddedEventHandler ServerLinesAdded;
        public event PlayerLinesAddedEventHandler PlayerLinesAdded;
        public event SpecialUsedEventHandler SpecialUsed;
        public event GridModifiedEventHandler GridModified;
        public event ContinuousSpecialFinishedEventHandler ContinuousSpecialFinished;

        public bool Connect(string address, string name, string team)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));
            if (name == null)
                throw new ArgumentNullException(nameof(name));

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
                Team = team;

                _state = States.Connecting;

                _proxy.Do(x => x.ClientConnect(Version, name, team));

                return true;
            }
            catch (Exception ex)
            {
                Log.Default.WriteLine(LogLevels.Error, "Problem in Connect. Exception:{0}", ex.ToString());
                return false;
            }
        }

        public bool Disconnect()
        {
            _proxy.Do(x => x.ClientDisconnect());

            InternalDisconnect();
            return true;
        }

        public bool SendPrivateMessage(ClientData target, string message)
        {
            if (target == null || _clients.All(x => x != target))
                return false;

            _proxy.Do(x => x.ClientSendPrivateMessage(target.Id, message));
            return true;
        }

        public bool SendBroadcastMessage(string message)
        {
            _proxy.Do(x => x.ClientSendBroadcastMessage(message));
            return true;
        }

        public bool ChangeTeam(string team)
        {
            _proxy.Do(x => x.ClientChangeTeam(team));
            return true;
        }

        public bool JoinGame(GameData game, string password, bool asSpectator)
        {
            if (game == null || _games.All(x => x != game))
                return false;
            _proxy.Do(x => x.ClientJoinGame(game.Id, password, asSpectator));
            return true;
        }

        public bool JoinRandomGame(bool asSpectator)
        {
            _proxy.Do(x => x.ClientJoinRandomGame(asSpectator));
            return true;
        }

        public bool CreateAndJoinGame(string name, string password, GameRules rule, bool asSpectator)
        {
            _proxy.Do(x => x.ClientCreateAndJoinGame(name, password, rule, asSpectator));
            return true;
        }

        public bool GetGameList()
        {
            _proxy.Do(x => x.ClientGetGameList());
            return true;
        }

        public bool GetClientList()
        {
            _proxy.Do(x => x.ClientGetClientList());
            return true;
        }

        public bool StartGame()
        {
            _proxy.Do(x => x.ClientStartGame());
            return true;
        }

        public bool StopGame()
        {
            _proxy.Do(x => x.ClientStopGame());
            return true;
        }

        public bool PauseGame()
        {
            _proxy.Do(x => x.ClientPauseGame());
            return true;
        }

        public bool ResumeGame()
        {
            _proxy.Do(x => x.ClientResumeGame());
            return true;
        }

        public bool ChangeOptions(GameOptions options)
        {
            _proxy.Do(x => x.ClientChangeOptions(options));
            return true;
        }

        public bool VoteKick(ClientData target, string reason)
        {
            if (target == null || _clients.All(x => x != target))
                return false;

            _proxy.Do(x => x.ClientVoteKick(target.Id, reason));
            return true;
        }

        public bool VoteKickResponse(bool accepted)
        {
            _proxy.Do(x => x.ClientVoteKickResponse(accepted));
            return true;
        }

        public bool ResetWinList()
        {
            _proxy.Do(x => x.ClientResetWinList());
            return true;
        }

        public bool LeaveGame()
        {
            _proxy.Do(x => x.ClientLeaveGame());
            return true;
        }

        public bool GetGameClientList()
        {
            _proxy.Do(x => x.ClientGetGameClientList());
            return true;
        }

        public void Hold()
        {
            throw new NotImplementedException();
        }

        public void Drop()
        {
            throw new NotImplementedException();
        }

        public void MoveDown(bool automatic = false)
        {
            throw new NotImplementedException();
        }

        public void MoveLeft()
        {
            throw new NotImplementedException();
        }

        public void MoveRight()
        {
            throw new NotImplementedException();
        }

        public void RotateClockwise()
        {
            throw new NotImplementedException();
        }

        public void RotateCounterClockwise()
        {
            throw new NotImplementedException();
        }

        public void DiscardFirstSpecial()
        {
            throw new NotImplementedException();
        }

        public bool UseFirstSpecial(int targetId)
        {
            throw new NotImplementedException();
        }

        public bool UseFirstSpecialOnSelf()
        {
            throw new NotImplementedException();
        }

        public bool UseFirstSpecialOnRandomOpponent()
        {
            throw new NotImplementedException();
        }

        public void ResetAchievements()
        {
            throw new NotImplementedException();
        }

        #endregion

        private void GameTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (AutomaticallyMoveDown && _state == States.Playing)
            {
                MoveDown(true);
            }
        }

        private void InternalDisconnect()
        {
            Log.Default.WriteLine(LogLevels.Info, "Disconnected");
            if (_proxy != null)
            {
                _proxy.ConnectionLost -= OnConnectionLost;
                _proxy.Disconnect();
                _proxy = null;
            }
        }

        private void OnConnectionLost()
        {
            InternalDisconnect();

            ConnectionLost.Do(x => x());
        }

        private void ResetTimeout()
        {
            _timeoutCount = 0;
            _lastActionFromServer = DateTime.Now;
        }

        private void SetTimeout()
        {
            _timeoutCount++;
            _lastActionFromServer = DateTime.Now;
        }

        private void TimeoutTask()
        {
            while (true)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    Log.Default.WriteLine(LogLevels.Info, "Stop background task event raised");
                    break;
                }

                if (_state == States.Connected || _state == States.WaitInGame || _state == States.Playing || _state == States.GamePaused || _state == States.GameLost)
                {
                    // Check server timeout
                    TimeSpan timespan = DateTime.Now - _lastActionFromServer;
                    if (IsTimeoutDetectionActive && timespan.TotalMilliseconds > TimeoutDelay)
                    {
                        Log.Default.WriteLine(LogLevels.Debug, "Timeout++");
                        // Update timeout count
                        SetTimeout();
                        if (_timeoutCount >= MaxTimeoutCountBeforeDisconnection)
                            OnConnectionLost(); // timeout
                    }

                    // Send heartbeat if needed
                    if (_proxy != null)
                    {
                        TimeSpan delaySinceLastActionToServer = DateTime.Now - _proxy.LastActionToServer;
                        if (delaySinceLastActionToServer.TotalMilliseconds > HeartbeatDelay)
                            _proxy.ClientHeartbeat();
                    }
                }

                //if (_proxy != null && _isConfusionActive)
                //{
                //    if (DateTime.Now > _confusionEndTime)
                //    {
                //        _isConfusionActive = false;
                //        ContinuousEffectToggled.Do(x => x(Specials.Confusion, false, 0));
                //        _proxy.FinishContinuousSpecial(this, Specials.Confusion);
                //    }
                //}

                //if (_proxy != null && _isDarknessActive)
                //{
                //    if (DateTime.Now > _darknessEndTime)
                //    {
                //        _isDarknessActive = false;
                //        ContinuousEffectToggled.Do(x => x(Specials.Darkness, false, 0));
                //        _proxy.FinishContinuousSpecial(this, Specials.Darkness);
                //    }
                //}

                //if (_proxy != null && _isImmunityActive)
                //{
                //    if (DateTime.Now > _immunityEndTime)
                //    {
                //        _isImmunityActive = false;
                //        ContinuousEffectToggled.Do(x => x(Specials.Immunity, false, 0));
                //        _proxy.FinishContinuousSpecial(this, Specials.Immunity);
                //    }
                //}

                // Stop task if stop event is raised
                bool signaled = _cancellationTokenSource.Token.WaitHandle.WaitOne(10);
                if (signaled)
                {
                    Log.Default.WriteLine(LogLevels.Info, "Stop background task event raised");
                    break;
                }
            }
        }
    }
}
