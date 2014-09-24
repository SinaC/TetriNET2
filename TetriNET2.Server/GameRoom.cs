using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TetriNET2.Common.ActionQueue;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Logger;
using TetriNET2.Server.Interfaces;

namespace TetriNET2.Server
{
    public sealed class GameRoom : IGameRoom, IDisposable
    {
        private const int PiecesSendOnGameStarted = 5;
        private const int PiecesSendOnPlacePiece = 4;
        private const int VoteKickTimeout = 10*1000; // in ms

        private readonly IPieceProvider _pieceProvider;
        private readonly List<IClient> _clients = new List<IClient>();
        private readonly object _lockObject = new object();
        private readonly IActionQueue _actionQueue;
        private readonly Dictionary<string, GameStatisticsByPlayer> _gameStatistics; // By player (cannot be stored in IClient because IClient is lost when disconnected during a game)
        private readonly List<WinEntry> _winList;
        private readonly Timer _suddenDeathTimer;
        private readonly Timer _voteKickTimer;

        private int _specialId;
        private bool _isSuddenDeathActive;
        private IClient _voteKickTarget;

        public GameRoom(IActionQueue actionQueue, IPieceProvider pieceProvider, string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password = null)
        {
            if (actionQueue == null)
                throw new ArgumentNullException("actionQueue");
            if (pieceProvider == null)
                throw new ArgumentNullException("pieceProvider");
            if (name == null)
                throw new ArgumentNullException("name");
            if (maxPlayers <= 0)
                throw new ArgumentOutOfRangeException("maxPlayers", "maxPlayers must be strictly positive");
            if (maxSpectators <= 0)
                throw new ArgumentOutOfRangeException("maxSpectators", "maxSpectators must be strictly positive");
            if (options == null)
                throw new ArgumentNullException("options");

            Id = Guid.NewGuid();
            _actionQueue = actionQueue;
            _pieceProvider = pieceProvider;
            _pieceProvider.Occurancies = () => options.PieceOccurancies;
            Name = name;
            CreationTime = DateTime.Now;
            MaxPlayers = maxPlayers;
            MaxSpectators = maxSpectators;
            Rule = rule;
            Options = options;
            Password = password;
            State = GameRoomStates.Created;

            _specialId = 0;
            _isSuddenDeathActive = false;
            _gameStatistics = new Dictionary<string, GameStatisticsByPlayer>();
            _winList = new List<WinEntry>();
            _suddenDeathTimer = new Timer(SuddenDeathCallback, null, Timeout.Infinite, 0);
            _voteKickTimer = new Timer(VoteKickCallback, null, Timeout.Infinite, 0);
        }

        #region IGameRoom

        public Guid Id { get; private set; }

        public string Name { get; private set; }

        public DateTime CreationTime { get; private set; }

        public string Password { get; private set; }

        public GameRoomStates State { get; private set; }

        public int MaxPlayers { get; private set; }

        public int MaxSpectators { get; private set; }

        public int ClientCount
        {
            get { return _clients.Count; }
        }

        public int PlayerCount
        {
            get { return _clients.Count(x => x.IsPlayer); }
        }

        public int SpectatorCount
        {
            get { return _clients.Count(x => x.IsSpectator); }
        }

        public object LockObject
        {
            get { return _lockObject; }
        }

        public DateTime GameStartTime { get; private set; }

        public GameOptions Options { get; private set; }

        public GameRules Rule { get; private set; }

        public IEnumerable<IClient> Clients
        {
            get { return _clients; }
        }

        public IEnumerable<IClient> Players
        {
            get { return _clients.Where(x => x.IsPlayer); }
        }

        public IEnumerable<IClient> Spectators
        {
            get { return _clients.Where(x => x.IsSpectator); }
        }

        public bool Start(CancellationTokenSource cancellationTokenSource)
        {
            if (State != GameRoomStates.Created)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Game {0} task already started", Name);
                return false;
            }
            //
            State = GameRoomStates.WaitStartGame;
            _actionQueue.Start(cancellationTokenSource);

            Log.Default.WriteLine(LogLevels.Info, "Game room {0}: Started", Name);
            return true;
        }

        public bool Stop()
        {
            if (State == GameRoomStates.Created)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Game {0} task not yet started", Name);
                return false;
            }

            State = GameRoomStates.Stopping;

            // Disable sudden death
            _isSuddenDeathActive = false;
            _suddenDeathTimer.Change(Timeout.Infinite, Timeout.Infinite);

            // If game was running, inform about game stop
            if (State == GameRoomStates.GameStarted || State == GameRoomStates.GamePaused)
            {
                foreach (IClient client in Clients)
                    client.OnGameFinished(GameFinishedReasons.Stopped, null);
            }

            // Remove clients from game
            foreach (IClient client in Clients)
            {
                client.State = ClientStates.Connected;
                client.Game = null;
                client.OnGameLeft();
            }

            // Clear clients
            _clients.Clear();

            // Clear win list
            _winList.Clear();

            // Clear game statistics
            _gameStatistics.Clear();

            // Clear action queue
            _actionQueue.Clear();

            // Reset piece
            _pieceProvider.Reset();

            // Wait action queue stopped
            _actionQueue.Wait(2000);

            // Reset special id
            _specialId = 0;

            // Change state
            State = GameRoomStates.Created;

            Log.Default.WriteLine(LogLevels.Info, "Game room {0}: Stopped", Name);
            return true;
        }

        public bool Join(IClient client, bool asSpectator)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            if (State == GameRoomStates.Created || State == GameRoomStates.Stopping)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot join, game room {0} is not started(or stopping)", Name);
                return false;
            }
            if (!asSpectator && PlayerCount >= MaxPlayers)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Too many players in game room {0}", Name);
                return false;
            }
            if (asSpectator && SpectatorCount >= MaxSpectators)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Too many spectators in game room {0}", Name);
                return false;
            }
            if (Clients.Any(x => x == client))
            {
                Log.Default.WriteLine(LogLevels.Warning, "Client already in game room {0}", Name);
                return false;
            }

            IClient previousMaster = Players.FirstOrDefault();

            //
            _clients.Add(client);

            // Change role, state and game
            client.Roles &= ~(ClientRoles.Player | ClientRoles.Spectator); // remove player+spectator
            if (asSpectator)
                client.Roles |= ClientRoles.Spectator;
            else
                client.Roles |= ClientRoles.Player;
            client.State = ClientStates.WaitInGameRoom;
            client.Game = this;
            client.LastVoteKickAnswer = null;

            IClient newMaster = Players.FirstOrDefault();
            if (client.IsPlayer && client == newMaster) // set game master
            {
                // Clear previous game master
                if (previousMaster != null)
                    previousMaster.Roles &= ~ClientRoles.GameMaster;
                // Set game master
                client.Roles |= ClientRoles.GameMaster;
            }

            // Inform client
            client.OnGameJoined(GameJoinResults.Successfull, Id, Options, client.IsGameMaster);

            // Inform other clients in game
            foreach (IClient target in Clients.Where(c => c != client))
                target.OnClientGameJoined(client.Id, asSpectator);
            // Inform other clients about game master modification
            if (client.IsGameMaster)
            {
                foreach (IClient target in Clients.Where(c => c != client))
                    target.OnGameMasterModified(client.Id);
                Log.Default.WriteLine(LogLevels.Info, "Game room {0}: Game master modified: {1}", Name, client.Id);
            }

            Log.Default.WriteLine(LogLevels.Info, "Game room {0}: client {1} joined", Name, client.Name);
            return true;
        }

        public bool Leave(IClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            if (client.Game != this)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot leave, {0} is not in game room {1}", client.Name, Name);
                return false;
            }

            bool removed = _clients.Remove(client);
            if (!removed)
                return false;

            // Clear vote kick target and timer
            if (_voteKickTarget == client)
            {
                _voteKickTarget = null;
                _voteKickTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }

            // Save play state
            bool wasPlaying = client.State == ClientStates.Playing;

            // Change role, state and game
            client.Roles &= ~(ClientRoles.Player | ClientRoles.Spectator | ClientRoles.GameMaster); // remove Player+Spectator+GameMaster
            client.State = ClientStates.Connected;
            client.Game = null;
            client.LastVoteKickAnswer = null;

            // Search new game master
            bool gameMasterModified = false;
            IClient newMaster = Players.FirstOrDefault();
            if (newMaster != null && !newMaster.IsGameMaster)
            {
                gameMasterModified = true;
                newMaster.Roles |= ClientRoles.GameMaster;
            }

            // Inform client
            client.OnGameLeft(); // TODO: is this statement really usefull ???

            // Inform other clients in game
            foreach (IClient target in Clients.Where(c => c != client))
                target.OnClientGameLeft(client.Id);
            // Inform other clients about game master modification
            if (gameMasterModified)
            {
                foreach (IClient target in Clients.Where(c => c != client))
                    target.OnGameMasterModified(newMaster.Id);
                Log.Default.WriteLine(LogLevels.Info, "Game room {0}: Game master modified: {1}", Name, newMaster.Id);
            }

            // If game was running and player was playing, check if only one player left
            if ((State == GameRoomStates.GameStarted || State == GameRoomStates.GamePaused) && wasPlaying)
            {
                int playingCount = Players.Count(p => p.State == ClientStates.Playing);
                if (playingCount == 0 || playingCount == 1)
                {
                    Log.Default.WriteLine(LogLevels.Info, "Game finished by forfeit no winner");
                    State = GameRoomStates.GameFinished;
                    GameStatistics statistics = PrepareGameStatistics();
                    // Send game finished (no winner)
                    foreach(IClient target in Clients.Where(c => c != client))
                        target.OnGameFinished(GameFinishedReasons.NotEnoughPlayers, statistics);
                    // Reset last player if any
                    if (playingCount == 1)
                    {
                        IClient last = Players.Single(p => p.State == ClientStates.Playing);
                        last.State = ClientStates.WaitInGameRoom;
                    }
                    State = GameRoomStates.WaitStartGame;
                }
            }

            Log.Default.WriteLine(LogLevels.Info, "Game room {0}: client {1} left", Name, client.Name);
            return true;
        }

        public void Clear()
        {
            _clients.Clear();
        }

        public bool VoteKick(IClient client, IClient target, string reason)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            if (target == null)
                throw new ArgumentNullException("target");
            if (client.Game != this)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot vote kick, {0} is not in game room {1}", client.Name, Name);
                return false;
            }
            if (target.Game != this)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot vote kick, target {0} is not in game room {1}", target.Name, Name);
                return false;
            }
            if (!client.IsPlayer)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot vote kick, {0} is not flagged as player", client.Name);
                return false;
            }
            if (!target.IsPlayer)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot vote kick, target {0} is not flagged as player", target.Name);
                return false;
            }
            if (PlayerCount < 3)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot vote kick, not enough players (min 3)");
                return false;
            }
            if (_voteKickTarget != null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot vote kick, {0}, a vote kick is already running on room {1}", client.Name, Name);
                return false;
            }
            // Set vote kick target
            _voteKickTarget = target;
            // Set player vote
            client.LastVoteKickAnswer = true;
            // Inform players
            foreach(IClient other in Players.Where(p => p != client && p != target))
                other.OnVoteKickAsked(client.Id, target.Id, reason);
            // Start timeout timer
            _voteKickTimer.Change(TimeSpan.FromMilliseconds(VoteKickTimeout), Timeout.InfiniteTimeSpan);
            
            Log.Default.WriteLine(LogLevels.Warning, "Game room {0}: vote kick started on {1} from {2}", Name, target.Name, client.Name);
            return true;
        }

        public bool VoteKickAnswer(IClient client, bool accepted)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            if (!client.IsPlayer)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot handle vote kick answer, {0} is not flagged as player", client.Name);
                return false;
            }
            if (client.Game != this)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot handle vote kick answer, {0} is not in game room {1}", client.Name, Name);
                return false;
            }
            if (_voteKickTarget == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot handle vote kick answer from {0}, no vote kick has been started", client.Name);
                return false;
            }
            if (client == _voteKickTarget)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot handle vote kick answer from {0}, he/she is vote target", client.Name);
                return false;
            }
            if (client.LastVoteKickAnswer.HasValue)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot handle vote kick answer, {0} has already voted", client.Name);
                return false;
            }
            // Set player vote
            client.LastVoteKickAnswer = accepted;
            // Check if everyone (excepted kicked one) has replied
            if (Players.Where(x => x != _voteKickTarget).All(p => p.LastVoteKickAnswer.HasValue))
            {
                // Check unanimity
                bool unanimity = Players.Where(x => x != _voteKickTarget).All(p => p.LastVoteKickAnswer.HasValue && p.LastVoteKickAnswer.Value);

                // Store target and reset it
                IClient target = _voteKickTarget;
                _voteKickTarget = null;

                // Reset timer
                _voteKickTimer.Change(Timeout.Infinite, Timeout.Infinite);

                if (unanimity) // Unanimity (excepted kicked one)
                {
                    Log.Default.WriteLine(LogLevels.Info, "Vote accepted on {0}", target.Name);

                    // Kick player out of game
                    Leave(target);
                }
                else // Vote rejected (at least one player has answered false)
                {
                    Log.Default.WriteLine(LogLevels.Info, "Vote rejected on {0}", target.Name);
                }

                // Reset answers
                foreach (IClient other in Players)
                    other.LastVoteKickAnswer = null;

            }

            Log.Default.WriteLine(LogLevels.Info, "Game room {0}: vote kick answer {0} from {1}", accepted, client.Name);
            return true;
        }

        public bool PlacePiece(IClient client, int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            if (State != GameRoomStates.GameStarted)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot place piece, game {0} is not started", Name);
                return false;
            }
            if (client.Game != this)
            {
                Log.Default.WriteLine(LogLevels.Error, "Cannot place piece, client {0} is not in this game {1} but in game {2}", client.Name, Name, client.Game == null ? "???" : client.Game.Name);
                return false;
            }
            if (client.State != ClientStates.Playing)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot place piece, client {0} is not playing", client.Name);
                return false;
            }
            //
            _actionQueue.Enqueue(() => PlacePieceAction(client, pieceIndex, highestIndex, piece, orientation, posX, posY, grid));
            return true;
        }

        public bool ModifyGrid(IClient client, byte[] grid)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            if (State != GameRoomStates.GameStarted)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot modify grid, game {0} is not started", Name);
                return false;
            }
            if (client.Game != this)
            {
                Log.Default.WriteLine(LogLevels.Error, "Cannot modify grid, client {0} is not in this game {1} but in game {2}", client.Name, Name, client.Game == null ? "???" : client.Game.Name);
                return false;
            }
            if (client.State != ClientStates.Playing)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot modify grid, client {0} is not playing", client.Name);
                return false;
            }
            //
            _actionQueue.Enqueue(() => ModifyGridAction(client, grid));
            return true;
        }

        public bool UseSpecial(IClient client, IClient target, Specials special)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            if (target == null)
                throw new ArgumentNullException("target");
            if (State != GameRoomStates.GameStarted)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot use special, game {0} is not started", Name);
                return false;
            }
            if (client.Game != this)
            {
                Log.Default.WriteLine(LogLevels.Error, "Cannot use special, client {0} is not in this game {1} but in game {2}", client.Name, Name, client.Game == null ? "???" : client.Game.Name);
                return false;
            }
            if (target.Game != this)
            {
                Log.Default.WriteLine(LogLevels.Error, "Cannot use special, target {0} is not in this game {1} but in game {2}", client.Name, Name, client.Game == null ? "???" : client.Game.Name);
                return false;
            }
            if (client.State != ClientStates.Playing)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot use special, client {0} is not playing", client.Name);
                return false;
            }
            if (target.State != ClientStates.Playing)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot use special, target {0} is not playing", target.Name);
                return false;
            }
            //
            _actionQueue.Enqueue(() => UseSpecialAction(client, target, special));
            return true;
        }

        public bool ClearLines(IClient client, int count)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            if (State != GameRoomStates.GameStarted)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot send lines, game {0} is not started", Name);
                return false;
            }
            if (client.Game != this)
            {
                Log.Default.WriteLine(LogLevels.Error, "Cannot clear lines, client {0} is not in this game {1} but in game {2}", client.Name, Name, client.Game == null ? "???" : client.Game.Name);
                return false;
            }
            if (client.State != ClientStates.Playing)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot clear lines, client {0} is not playing", client.Name);
                return false;
            }
            //
            UpdateStatistics(client.Name, count);
            if (Options.ClassicStyleMultiplayerRules && count > 1)
            {
                int addLines = count - 1;
                if (addLines >= 4)
                    // special case for Tetris and above
                    addLines = 4;
                _actionQueue.Enqueue(() => SendLinesAction(client, addLines));
            }
            return true;
        }

        public bool GameLost(IClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            if (client.Game != this)
            {
                Log.Default.WriteLine(LogLevels.Error, "Cannot lose game, client {0} is not in this game {1} but in game {2}", client.Name, Name, client.Game == null ? "???" : client.Game.Name);
                return false;
            } 
            if (client.State != ClientStates.Playing)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot lose game, client {0} is not playing", client.Name);
                return false;
            }
            //
            _actionQueue.Enqueue(() => GameLostAction(client));
            return true;
        }

        public bool FinishContinuousSpecial(IClient client, Specials special)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            if (client.Game != this)
            {
                Log.Default.WriteLine(LogLevels.Error, "Cannot finish continuous special, client {0} is not in this game {1} but in game {2}", client.Name, Name, client.Game == null ? "???" : client.Game.Name);
                return false;
            }
            //
            _actionQueue.Enqueue(() => FinishContinuousSpecialAction(client, special)); // Must be handled even if game is not started
            return true;
        }

        public bool EarnAchievement(IClient client, int achievementId, string achievementTitle)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            if (client.Game != this)
            {
                Log.Default.WriteLine(LogLevels.Error, "Cannot earn achievement grid, client {0} is not in this game {1} but in game {2}", client.Name, Name, client.Game == null ? "???" : client.Game.Name);
                return false;
            }
            //
            foreach (IClient target in Clients.Where(x => x != client))
                target.OnAchievementEarned(client.Id, achievementId, achievementTitle);

            Log.Default.WriteLine(LogLevels.Info, "Game room {0}: EarnAchievement:{1} {2} {3}", Name, client.Name, achievementId, achievementTitle);
            return true;
        }

        public bool StartGame(IClient client)
        {
            Log.Default.WriteLine(LogLevels.Info, "Starting game");
            if (State != GameRoomStates.WaitStartGame)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Game {0} already started", Name);
                return false;
            }
            if (client != null)
            {
                if (!client.IsPlayer)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Cannot start game, {0} is not flagged as player", client.Name);
                    return false;
                }
                if (client.Game != this)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Cannot start game, {0} is not in game room {1}", client.Name, Name);
                    return false;
                }
                if (!client.IsGameMaster)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Cannot start game, client {0} is not game master", client.Name);
                    return false;
                }
            }
            if (PlayerCount <= 0)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Game {0} cannot be started, no players", Name);
                return false;
            }

            // Reset action queue
            _actionQueue.Clear();

            // Reset piece
            _pieceProvider.Reset();

            // Reset special id
            _specialId = 0;

            // Create first pieces
            List<Pieces> pieces = new List<Pieces>();
            for (int i = 0; i < PiecesSendOnGameStarted; i++)
                pieces.Add(_pieceProvider[i]);
            Log.Default.WriteLine(LogLevels.Info, "Starting game with {0}", pieces.Select(x => x.ToString()).Aggregate((n, i) => n + "," + i));

            GameStartTime = DateTime.Now;

            // Reset sudden death
            _isSuddenDeathActive = false;
            if (Options.DelayBeforeSuddenDeath > 0)
            {
                _suddenDeathTimer.Change(Options.DelayBeforeSuddenDeath*60*1000, Options.SuddenDeathTick*1000);
                _isSuddenDeathActive = true;
                Log.Default.WriteLine(LogLevels.Info, "Sudden death will be activated after {0} minutes and send lines every {1} seconds", Options.DelayBeforeSuddenDeath, Options.SuddenDeathTick);
            }

            // Reset statistics
            ResetStatistics();

            // Send game started to players
            foreach (IClient player in Players)
            {
                player.PieceIndex = 0;
                player.State = ClientStates.Playing;
                player.LossTime = DateTime.MaxValue;
                player.OnGameStarted(pieces);
            }
            // Send game started to spectators
            foreach (IClient spectator in Spectators)
                spectator.OnGameStarted(null);
            
            State = GameRoomStates.GameStarted;

            Log.Default.WriteLine(LogLevels.Info, "Game room {0}: game started by {1}", Name, client == null ? "[SERVER]" : client.Name);

            return true;
        }

        public bool StopGame(IClient client)
        {
            if (State != GameRoomStates.GameStarted && State != GameRoomStates.GamePaused)
            {
                Log.Default.WriteLine(LogLevels.Info, "Cannot stop game");
                return false;
            }
            if (client != null)
            {
                if (!client.IsPlayer)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Cannot stop game, {0} is not flagged as player", client.Name);
                    return false;
                }
                if (client.Game != this)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Cannot stop game, {0} is not in game room {1}", client.Name, Name);
                    return false;
                }
                if (!client.IsGameMaster)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Cannot stop game, client {0} is not game master", client.Name);
                    return false;
                }
            }
            //
            State = GameRoomStates.GameFinished;

            // Stop sudden death timer
            if (_isSuddenDeathActive)
            {
                _isSuddenDeathActive = false;
                _suddenDeathTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }

            GameStatistics statistics = PrepareGameStatistics();

            // Send game finished to players
            foreach (IClient player in Players)
            {
                player.State = ClientStates.WaitInGameRoom;
                player.OnGameFinished(GameFinishedReasons.Stopped, statistics);
            }
            // Send game finished to spectators
            foreach (IClient spectator in Spectators)
                spectator.OnGameFinished(GameFinishedReasons.Stopped, statistics);

            State = GameRoomStates.WaitStartGame;

            Log.Default.WriteLine(LogLevels.Info, "Game room {0}: game stopped by {1}", Name, client == null ? "[SERVER]" : client.Name);
            return true;
        }

        public bool PauseGame(IClient client)
        {
            Log.Default.WriteLine(LogLevels.Info, "Pausing game");

            if (State != GameRoomStates.GameStarted)
            {
                Log.Default.WriteLine(LogLevels.Info, "Cannot pause game");
                return false;
            }
            if (client != null)
            {
                if (!client.IsPlayer)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Cannot pause game, {0} is not flagged as player", client.Name);
                    return false;
                }
                if (client.Game != this)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Cannot pause game, {0} is not in game room {1}", client.Name, Name);
                    return false;
                }
                if (!client.IsGameMaster)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Cannot pause game, client {0} is not game master", client.Name);
                    return false;
                }
            }
            State = GameRoomStates.GamePaused;

            // Send game paused to players and spectators
            foreach (IClient target in Clients)
                target.OnGamePaused();

            Log.Default.WriteLine(LogLevels.Info, "Game room {0}: game paused by {1}", Name, client == null ? "[SERVER]" : client.Name);
            return true;
        }

        public bool ResumeGame(IClient client)
        {
            Log.Default.WriteLine(LogLevels.Info, "Resuming game");
            if (State != GameRoomStates.GamePaused)
            {
                Log.Default.WriteLine(LogLevels.Info, "Cannot resume game");
                return false;
            }
            if (client != null)
            {
                if (!client.IsPlayer)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Cannot pause game, {0} is not flagged as player", client.Name);
                    return false;
                }
                if (client.Game != this)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Cannot pause game, {0} is not in game room {1}", client.Name, Name);
                    return false;
                }
                if (!client.IsGameMaster)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Cannot pause game, client {0} is not game master", client.Name);
                    return false;
                }
            }
            //
            State = GameRoomStates.GameStarted;

            // Send game resumed to players and spectators
            foreach (IClient target in Clients)
                target.OnGameResumed();

            Log.Default.WriteLine(LogLevels.Info, "Game room {0}: game resumed by {1}", Name, client == null ? "[SERVER]" : client.Name);
            return true;
        }

        public bool ChangeOptions(IClient client, GameOptions options)
        {
            if (State != GameRoomStates.WaitStartGame)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot change options, game {0} is started", Name);
                return false;
            }
            if (client != null)
            {
                if (!client.IsPlayer)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Cannot change options, {0} is not flagged as player", client.Name);
                    return false;
                }
                if (client.Game != this)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Cannot change options, {0} is not in game room {1}", client.Name, Name);
                    return false;
                }
                if (!client.IsGameMaster)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Cannot change options, client {0} is not game master", client.Name);
                    return false;
                }
            }
            if (!options.IsValid)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot change options, options are not valid");
                return false;
            }
            //
            Options = options;
            _pieceProvider.Occurancies = () => Options.PieceOccurancies;
            // Inform clients
            foreach (IClient target in Clients)
                target.OnGameOptionsChanged(options);

            Log.Default.WriteLine(LogLevels.Info, "Game room {0}: game options changed by {1}", Name, client == null ? "[SERVER]" : client.Name);
            return true;
        }

        public bool ResetWinList(IClient client)
        {
            if (State != GameRoomStates.WaitStartGame)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot reset winlist, game {0} is started", Name);
                return false;
            }
            if (client != null)
            {
                if (!client.IsPlayer)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Cannot reset winlist, {0} is not flagged as player", client.Name);
                    return false;
                }
                if (client.Game != this)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Cannot reset winlist, {0} is not in game room {1}", client.Name, Name);
                    return false;
                }
                if (!client.IsGameMaster)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Cannot reset winlist, client {0} is not game master", client.Name);
                    return false;
                }
            }
            //
            _winList.Clear();
            // Inform clients
            foreach (IClient target in Clients)
                target.OnWinListModified(_winList);

            Log.Default.WriteLine(LogLevels.Info, "Game room {0}: win list resetted by {1}", Name, client == null ? "[SERVER]" : client.Name);
            return true;
        }

        #endregion

        #region Game statistics

        private void ResetStatistics()
        {
            // Delete previous stats
            _gameStatistics.Clear();
            // Create GameStatisticsByPlayer for each player
            foreach (IClient player in Players)
            {
                // Init stats
                GameStatisticsByPlayer stats = new GameStatisticsByPlayer
                {
                    PlayerName = player.Name,
                    SpecialsUsed = new Dictionary<Specials, Dictionary<string, int>>()
                };
                foreach (SpecialOccurancy occurancy in Options.SpecialOccurancies.Where(x => x.Occurancy > 0))
                {
                    Dictionary<string, int> specialsByPlayer = Players.Select(p => p.Name).ToDictionary(x => x, x => 0);
                    stats.SpecialsUsed.Add(occurancy.Value, specialsByPlayer);
                }
                // Add stats
                _gameStatistics.Add(player.Name, stats);
            }
        }

        private void UpdateStatistics(string playerName, int linesCount)
        {
            if (linesCount == 1)
                _gameStatistics[playerName].SingleCount++;
            else if (linesCount == 2)
                _gameStatistics[playerName].DoubleCount++;
            else if (linesCount == 3)
                _gameStatistics[playerName].TripleCount++;
            else if (linesCount >= 4)
                _gameStatistics[playerName].TetrisCount++;
        }

        private void UpdateStatistics(string playerName, string targetName, Specials special)
        {
            _gameStatistics[playerName].SpecialsUsed[special][targetName]++;
        }

        private void UpdateStatistics(string playerName, DateTime gameStartTime, DateTime lossTime)
        {
            _gameStatistics[playerName].PlayingTime = (lossTime - gameStartTime).TotalSeconds;
        }

        private GameStatistics PrepareGameStatistics()
        {
            GameStatistics statistics = new GameStatistics
            {
                GameStarted = GameStartTime,
                GameFinished = DateTime.Now,
                Players = _gameStatistics.Select(x => x.Value).ToList()
            };
            return statistics;
        }

        #endregion

        #region Game actions

        private void PlacePieceAction(IClient client, int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Game room {0}: PlacePieceAction[{1}]{2}:{3} {4} {5} at {6},{7} {8}", Name, client.Name, pieceIndex, highestIndex, piece, orientation, posX, posY, grid == null ? -1 : grid.Count(x => x > 0));

            //if (index != player.PieceIndex)
            //    Log.Default.WriteLine(LogLevels.Error, "!!!! piece index different for player {0} local {1} remote {2}", player.Name, player.PieceIndex, index);

            bool sendNextPieces = false;
            // Set grid
            client.Grid = grid;
            // Get next piece
            client.PieceIndex = pieceIndex;
            List<Pieces> nextPiecesToSend = new List<Pieces>();
            Log.Default.WriteLine(LogLevels.Debug, "{0} {1} indexes: {2} {3}", client.Id, client.Name, highestIndex, pieceIndex);
            if (highestIndex < pieceIndex)
                Log.Default.WriteLine(LogLevels.Error, "PROBLEM WITH INDEXES!!!!!");
            if (highestIndex < pieceIndex + PiecesSendOnPlacePiece) // send many pieces when needed
            {
                for (int i = 0; i < 2 * PiecesSendOnPlacePiece; i++)
                    nextPiecesToSend.Add(_pieceProvider[highestIndex + i]);
                sendNextPieces = true;
            }
            else if (highestIndex < pieceIndex + 2 * PiecesSendOnPlacePiece) // send next pieces only if needed
            {
                for (int i = 0; i < PiecesSendOnPlacePiece; i++)
                    nextPiecesToSend.Add(_pieceProvider[highestIndex + i]);
                sendNextPieces = true;
            }

            // Send grid to other playing players and spectators
            foreach (IClient target in Clients.Where(x => x != client))
                target.OnGridModified(client.Id, grid);

            if (sendNextPieces)
            {
                Log.Default.WriteLine(LogLevels.Debug, "Send next piece {0} {1} {2}", highestIndex, pieceIndex, nextPiecesToSend.Any() ? nextPiecesToSend.Select(x => x.ToString()).Aggregate((n, i) => n + "," + i) : String.Empty);
                // Send next pieces
                client.OnPiecePlaced(highestIndex, nextPiecesToSend);
            }
        }

        private void ModifyGridAction(IClient client, byte[] grid)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Game room {0}: ModifyGridAction[{1}]", Name, client.Name);

            // Set grid
            client.Grid = grid;
            // Send grid modification to everyone except sender
            foreach (IClient target in Clients.Where(x => x != client))
                target.OnGridModified(client.Id, client.Grid);
        }

        private void UseSpecialAction(IClient client, IClient target, Specials special)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Game room {0}: UseSpecial[{1}][{2}]:{3}", Name, client.Name, target.Name, special);

            if (target.State != ClientStates.Playing)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Cannot use special on a non-playing client");
                return;
            }

            // Update statistics
            UpdateStatistics(client.Name, target.Name, special);
            // Store special id locally
            int specialId = _specialId;
            // Increment special
            _specialId++;
            // If special is Switch, call OnGridModified with switched grids
            if (special == Specials.SwitchFields)
            {
                // Switch locally
                byte[] tmp = target.Grid;
                target.Grid = client.Grid;
                client.Grid = tmp;

                // Send switched grid to player and target
                client.OnGridModified(client.Id, client.Grid);
                if (client != target)
                    target.OnGridModified(target.Id, target.Grid);
                // Client and target will send their grid when receiving them (with an optional capping)
            }
            // Inform about special use
            foreach (IClient c in Clients)
                c.OnSpecialUsed(client.Id, target.Id, specialId, special);
        }

        private void SendLinesAction(IClient client, int count)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Game room {0}: SendLines[{1}]: {2}", Name, client.Name, count);

            // Store special id locally
            int specialId = _specialId;
            // Increment special id
            _specialId++;
            // Send lines to everyone including sender (so attack msg can be displayed)
            foreach (IClient target in Clients)
                target.OnPlayerLinesAdded(client.Id, specialId, count);
        }
        
        private void GameLostAction(IClient client)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Game room {0}: GameLost[{1}]: {2}", Name, client.Name, State);

            if (client.State != ClientStates.Playing)
            {
                Log.Default.WriteLine(LogLevels.Info, "Game lost from non-playing player {0} {1}", client.Name, client.State);
                return;
            }
            // Set player state
            client.State = ClientStates.GameLost;
            client.LossTime = DateTime.Now;

            // Inform other players and spectators
            foreach (IClient target in Clients.Where(x => x != client))
                target.OnPlayerLost(client.Id);

            UpdateStatistics(client.Name, GameStartTime, client.LossTime);

            //
            int playingCount = Players.Count(p => p.State == ClientStates.Playing);
            if (playingCount == 0) // there were only one playing player
            {
                Log.Default.WriteLine(LogLevels.Info, "Game finished with only one player playing, no winner");
                State = GameRoomStates.GameFinished;
                // Send game finished (no winner) to players and spectators
                GameStatistics statistics = PrepareGameStatistics();
                foreach (IClient target in Clients)
                    target.OnGameFinished(GameFinishedReasons.NotEnoughPlayers, statistics);
                State = GameRoomStates.WaitStartGame;
            }
            else if (playingCount == 1) // only one playing left
            {
                Log.Default.WriteLine(LogLevels.Info, "Game finished checking winner");
                State = GameRoomStates.GameFinished;
                // Game won
                IClient winner = Players.Single(p => p.State == ClientStates.Playing);
                winner.State = ClientStates.WaitInGameRoom;
                Log.Default.WriteLine(LogLevels.Info, "Winner: {0}[{1}]", winner.Name, winner.Id);

                // Update win list
                UpdateWinList(winner.Name, winner.Team, 3);
                int points = 2;
                foreach (IClient player in Players.Where(x => x.State == ClientStates.GameLost).OrderByDescending(x => x.LossTime))
                {
                    UpdateWinList(player.Name, player.Team, points);
                    points--;
                    if (points == 0)
                        break;
                }

                GameStatistics statistics = PrepareGameStatistics();
                // Send winner, game finished and win list
                foreach (IClient target in Clients)
                {
                    target.OnPlayerWon(winner.Id);
                    target.OnGameFinished(GameFinishedReasons.Won, statistics);
                    target.OnWinListModified(_winList);
                }
                State = GameRoomStates.WaitStartGame;
            }
        }

        private void FinishContinuousSpecialAction(IClient client, Specials special)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Game room {0}: FinishContinuousSpecial[{1}]: {2}", Name, client.Name, special);

            // Send to everyone except sender
            foreach (IClient target in Clients.Where(x => x != client))
                target.OnContinuousSpecialFinished(client.Id, special);
        }

        #endregion

        #region IDisposable

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_suddenDeathTimer != null)
                    _suddenDeathTimer.Dispose();
                if (_voteKickTimer != null)
                    _voteKickTimer.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        private void SuddenDeathCallback(object state)
        {
            if (State == GameRoomStates.GameStarted && _isSuddenDeathActive)
            {
                Log.Default.WriteLine(LogLevels.Info, "Game room {0}: Sudden death tick", Name);
                // Delay elapsed, send lines
                foreach (IClient player in Players.Where(p => p.State == ClientStates.Playing))
                    player.OnServerLinesAdded(1);
            }
        }

        private void VoteKickCallback(object state)
        {
            if (_voteKickTarget != null)
            {
                Log.Default.WriteLine(LogLevels.Info, "Game room {0}: vote kick timeout reached, cancel vote", Name);

                // Reset target
                _voteKickTarget = null;

                // Reset timer
                _voteKickTimer.Change(Timeout.Infinite, Timeout.Infinite);

                // Reset answers
                foreach (IClient other in Players)
                    other.LastVoteKickAnswer = null;
            }
        }

        private void UpdateWinList(string playerName, string team, int score)
        {
            WinEntry entry = _winList.SingleOrDefault(x => x.PlayerName == playerName && x.Team == team);
            if (entry == null)
            {
                entry = new WinEntry
                {
                    PlayerName = playerName,
                    Team = team,
                    Score = 0
                };
                _winList.Add(entry);
            }
            entry.Score += score;
        }
    }
}
