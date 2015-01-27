using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TetriNET2.Client.Interfaces;
using TetriNET2.Common.ActionQueue;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Helpers;
using TetriNET2.Common.Logger;

// TODO: 
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

        private readonly IFactory _factory;
        private readonly IActionQueue _actionQueue;
        private readonly List<ClientData> _clients;
        private readonly List<ClientData> _gameClients;
        private readonly List<GameData> _games;

        private IProxy _proxy;

        private readonly Task _timeoutTask;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private States _state;
        private Guid _clientId;
        private bool _isGameMaster;
        private DateTime _lastActionFromServer;
        private int _timeoutCount;

        public Client(IFactory factory, IActionQueue actionQueue)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");
            if (actionQueue == null)
                throw new ArgumentNullException("actionQueue");

            _factory = factory;
            _actionQueue = actionQueue;
            _clients = new List<ClientData>();
            _gameClients = new List<ClientData>();
            _games = new List<GameData>();

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

            _cancellationTokenSource = new CancellationTokenSource();
            _timeoutTask = Task.Factory.StartNew(TimeoutTask, _cancellationTokenSource.Token);
            _actionQueue.Start(_cancellationTokenSource);
        }

        #region IClient
        
        #region ITetriNETClientCallback

        public void OnConnected(ConnectResults result, Versioning serverVersion, Guid clientId, List<GameData> games)
        {
            if (result == ConnectResults.Successfull)
            {
                Log.Default.WriteLine(LogLevels.Info, "Connected as player {0}", clientId);

                _clients.Clear();
                _gameClients.Clear();
                _games.Clear();

                _games.AddRange(games);

                _clientId = clientId;

                _clients.Add(new ClientData
                    {
                        Id = clientId,
                        Name = Name,
                    });

                _state = States.Connected;
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Warning, "Wrong id {0}", clientId);

                _state = States.Created;
            }

            Connected.Do(x => x(result, serverVersion, clientId, games));
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

            _games.Clear();
            _games.AddRange(games);

            GameListReceived.Do(x => x(games));
        }

        public void OnClientListReceived(List<ClientData> clients)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client list received");

            _clients.Clear();
            _clients.AddRange(clients);

            ClientListReceived.Do(x => x(clients));
        }

        public void OnGameClientListReceived(List<ClientData> clients)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client in game received");

            _gameClients.Clear();
            _gameClients.AddRange(clients);

            GameClientListReceived.Do(x => x(clients));
        }

        public void OnClientConnected(Guid clientId, string name, string team)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client connected {0} {1} {2}", clientId, name, team);

            ClientConnected.Do(x => x(clientId, name, team));
        }

        public void OnClientDisconnected(Guid clientId, LeaveReasons reason)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client disconnected {0} {1}", clientId, reason);

            // If client was in game, remove it
            if (_gameClients.Any(x => x.Id == clientId))
            {
                Log.Default.WriteLine(LogLevels.Info, "Client disconnected {0} and removed from game", clientId);

                _gameClients.RemoveAll(x => x.Id == clientId);
                ClientGameLeft.Do(x => x(clientId));
            }

            ClientDisconnected.Do(x => x(clientId, reason));
        }

        public void OnClientGameCreated(Guid clientId, GameData game)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client {0} creates game {1}", clientId, game == null ? Guid.Empty : game.Id);

            _games.Add(game);

            ClientGameCreated.Do(x => x(clientId, game));
        }

        public void OnServerGameCreated(GameData game)
        {
            Log.Default.WriteLine(LogLevels.Info, "Server creates game {0}", game == null ? Guid.Empty : game.Id);

            _games.Add(game);

            ServerGameCreated.Do(x => x(game));
        }

        public void OnServerGameDeleted(Guid gameId)
        {
            Log.Default.WriteLine(LogLevels.Info, "Server deletes game {0}", gameId);

            _games.RemoveAll(x => x.Id == gameId);

            ServerGameDeleted.Do(x => x(gameId));
        }

        public void OnServerMessageReceived(string message)
        {
            Log.Default.WriteLine(LogLevels.Info, "Server message {0}", message);

            ServerMessageReceived.Do(x => x(message));
        }

        public void OnBroadcastMessageReceived(Guid clientId, string message)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client {0} broadcasts message {1}", clientId, message);

            BroadcastMessageReceived.Do(x => x(clientId, message));
        }

        public void OnPrivateMessageReceived(Guid clientId, string message)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client {0} sends message {1}", clientId, message);

            PrivateMessageReceived.Do(x => x(clientId, message));
        }

        public void OnTeamChanged(Guid clientId, string team)
        {
            Log.Default.WriteLine(LogLevels.Info, "Client {0} changes team {1}", clientId, team);

            TeamChanged.Do(x => x(clientId, team));
        }

        public void OnGameCreated(GameCreateResults result, GameData game)
        {
            if (result == GameCreateResults.Successfull)
            {
                Log.Default.WriteLine(LogLevels.Info, "Game {0} created successfully", game == null ? Guid.Empty : game.Id);

                _games.Add(game);
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Warning, "Failed to create game {0}", result);
            }

            GameCreated.Do(x => x(result, game));
        }

        public void OnGameJoined(GameJoinResults result, Guid gameId, GameOptions options, bool isGameMaster)
        {
            if (result == GameJoinResults.Successfull)
            {
                Log.Default.WriteLine(LogLevels.Info, "Game {0} joined successfully. Master {1}", gameId, isGameMaster);

                _state = States.WaitInGame;
                _isGameMaster = isGameMaster;

                _gameClients.Clear();

                // Get clients in game  TODO: players in game as additional parameter ?
                _proxy.Do(x => x.ClientGetGameClientList());
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Warning, "Failed to join game {0} {1}", gameId, result);
            }

            GameJoined.Do(x => x(result, gameId, options, isGameMaster));
        }

        public void OnGameLeft()
        {
            Log.Default.WriteLine(LogLevels.Info, "Game left");

            _state = States.Connected;

            GameLeft.Do(x => x());
        }

        public void OnClientGameJoined(Guid clientId, bool asSpectator)
        {
            if (_gameClients.Any(x => x.Id == clientId))
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

            ClientGameJoined.Do(x => x(clientId, asSpectator));
        }

        public void OnClientGameLeft(Guid clientId)
        {
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

            ClientGameLeft.Do(x => x(clientId));
        }

        public void OnGameMasterModified(Guid playerId)
        {
            Log.Default.WriteLine(LogLevels.Info, "Game master modified {1}", playerId);

            foreach (ClientData client in _gameClients)
                client.IsGameMaster = client.Id == playerId;
            
            if (playerId == _clientId)
            {
                Log.Default.WriteLine(LogLevels.Info, "New game master !!!");
                _isGameMaster = true;
            }

            GameMasterModified.Do(x => x(playerId));
        }

        public void OnGameStarted(List<Pieces> pieces)
        {
            throw new NotImplementedException();
        }

        public void OnGamePaused()
        {
            throw new NotImplementedException();
        }

        public void OnGameResumed()
        {
            throw new NotImplementedException();
        }

        public void OnGameFinished(GameFinishedReasons reason, GameStatistics statistics)
        {
            throw new NotImplementedException();
        }

        public void OnWinListModified(List<WinEntry> winEntries)
        {
            throw new NotImplementedException();
        }

        public void OnGameOptionsChanged(GameOptions gameOptions)
        {
            throw new NotImplementedException();
        }

        public void OnVoteKickAsked(Guid sourceClient, Guid targetClient, string reason)
        {
            throw new NotImplementedException();
        }

        public void OnAchievementEarned(Guid playerId, int achievementId, string achievementTitle)
        {
            throw new NotImplementedException();
        }

        public void OnPiecePlaced(int firstIndex, List<Pieces> nextPieces)
        {
            throw new NotImplementedException();
        }

        public void OnPlayerWon(Guid playerId)
        {
            throw new NotImplementedException();
        }

        public void OnPlayerLost(Guid playerId)
        {
            throw new NotImplementedException();
        }

        public void OnServerLinesAdded(int count)
        {
            throw new NotImplementedException();
        }

        public void OnPlayerLinesAdded(Guid playerId, int specialId, int count)
        {
            throw new NotImplementedException();
        }

        public void OnSpecialUsed(Guid playerId, Guid targetId, int specialId, Specials special)
        {
            throw new NotImplementedException();
        }

        public void OnGridModified(Guid playerId, byte[] grid)
        {
            throw new NotImplementedException();
        }

        public void OnContinuousSpecialFinished(Guid playerId, Specials special)
        {
            throw new NotImplementedException();
        }

        #endregion

        public string Name { get; private set; }

        public string Team { get; private set; }

        public Versioning Version { get; private set; }

        public IEnumerable<ClientData> Clients { get { return _clients; } }

        public IEnumerable<ClientData> GameClients { get { return _gameClients; } }

        public IEnumerable<GameData> Games { get { return _games; } }

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

        public bool SendPrivateMessage(Guid targetId, string message)
        {
            _proxy.Do(x => x.ClientSendPrivateMessage(targetId, message));
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

        public bool JoinGame(Guid gameId, string password, bool asSpectator)
        {
            _proxy.Do(x => x.ClientJoinGame(gameId, password, asSpectator));
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

        public bool VoteKick(Guid targetId, string reason)
        {
            _proxy.Do(x => x.ClientVoteKick(targetId, reason));
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

        public void ResetAchievements()
        {
            throw new NotImplementedException();
        }

        #endregion

        private void InternalDisconnect()
        {
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
                    if (timespan.TotalMilliseconds > TimeoutDelay && IsTimeoutDetectionActive)
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
