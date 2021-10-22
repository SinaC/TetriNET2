using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Helpers;
using TetriNET2.Common.Logger;
using TetriNET2.Server.Interfaces;

namespace TetriNET2.Server
{
    public sealed class Client : IClient
    {
        private bool _disconnected;

        public Client(string name, IAddress address, ITetriNETClientCallback callback, string team = null)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Address = address ?? throw new ArgumentNullException(nameof(address));
            Callback = callback ?? throw new ArgumentNullException(nameof(callback));
            ConnectTime = DateTime.Now;

            State = ClientStates.Connected;
            Roles = ClientRoles.NoRole;

            Team = team;
            PieceIndex = 0;

            LastActionToClient = DateTime.Now;
            LastActionFromClient = DateTime.Now;
            TimeoutCount = 0;
            _disconnected = false;
        }

        private void ExceptionFreeAction(Action action, [CallerMemberName]string actionName = null)
        {
            try
            {
                if (!_disconnected)
                    action();
                LastActionToClient = DateTime.Now;
            }
            catch (CommunicationObjectAbortedException)
            {
                _disconnected = true;
                ConnectionLost.Do(x => x(this));
            }
            catch (Exception ex)
            {
                _disconnected = true;
                Log.Default.WriteLine(LogLevels.Error, "Exception:{0} {1}", actionName, ex);
                ConnectionLost.Do(x => x(this));
            }
        }

        #region IClient

        public event ClientConnectionLostEventHandler ConnectionLost;

        public Guid Id { get; }
        public string Name { get; }
        public IAddress Address { get; }
        public ITetriNETClientCallback Callback { get; }
        public DateTime ConnectTime { get; }

        public ClientStates State { get; set; }
        public ClientRoles Roles { get; set; }

        //
        public string Team { get; set; }
        public int PieceIndex { get; set; }
        public byte[] Grid { get; set; }
        public DateTime LossTime { get; set; }
        public IGame Game { get; set; }
        public bool? LastVoteKickAnswer { get; set; }

        public bool IsGameMaster => (Roles & ClientRoles.GameMaster) == ClientRoles.GameMaster;

        public bool IsPlayer => (Roles & ClientRoles.Player) == ClientRoles.Player;

        public bool IsSpectator => (Roles & ClientRoles.Spectator) == ClientRoles.Spectator;

        public DateTime LastActionToClient { get; private set; }
        public DateTime LastActionFromClient { get; private set; }
        public int TimeoutCount { get; private set; }

        public void ResetTimeout()
        {
            TimeoutCount = 0;
            LastActionFromClient = DateTime.Now;
        }

        public void SetTimeout()
        {
            TimeoutCount++;
            LastActionFromClient = DateTime.Now;
        }

        #endregion IClient

        #region ITetriNETCallback

        public void OnConnected(ConnectResults result, Versioning serverVersion, Guid clientId, List<GameData> games)
        {
            ExceptionFreeAction(() => Callback.OnConnected(result, serverVersion, clientId, games));
        }

        public void OnDisconnected()
        {
            ExceptionFreeAction(() => Callback.OnDisconnected());
        }

        public void OnHeartbeatReceived()
        {
            ExceptionFreeAction(() => Callback.OnHeartbeatReceived());
        }

        public void OnServerStopped()
        {
            ExceptionFreeAction(() => Callback.OnServerStopped());
        }

        public void OnGameListReceived(List<GameData> games)
        {
            ExceptionFreeAction(() => Callback.OnGameListReceived(games));
        }

        public void OnClientListReceived(List<ClientData> clients)
        {
            ExceptionFreeAction(() => Callback.OnClientListReceived(clients));
        }

        public void OnGameClientListReceived(List<ClientData> clients)
        {
            ExceptionFreeAction(() => Callback.OnGameClientListReceived(clients));
        }

        public void OnClientConnected(Guid clientId, string name, string team)
        {
            ExceptionFreeAction(() => Callback.OnClientConnected(clientId, name, team));
        }

        public void OnClientDisconnected(Guid clientId, LeaveReasons reason)
        {
            ExceptionFreeAction(() => Callback.OnClientDisconnected(clientId, reason));
        }

        public void OnClientGameCreated(Guid clientId, GameData game)
        {
            ExceptionFreeAction(() => Callback.OnClientGameCreated(clientId, game));
        }

        public void OnServerGameCreated(GameData game)
        {
            ExceptionFreeAction(() => Callback.OnServerGameCreated(game));
        }

        public void OnServerGameDeleted(Guid gameId)
        {
            ExceptionFreeAction(() => Callback.OnServerGameDeleted(gameId));
        }

        public void OnServerMessageReceived(string message)
        {
            ExceptionFreeAction(() => Callback.OnServerMessageReceived(message));
        }

        public void OnBroadcastMessageReceived(Guid clientId, string message)
        {
            ExceptionFreeAction(() => Callback.OnBroadcastMessageReceived(clientId, message));
        }

        public void OnPrivateMessageReceived(Guid clientId, string message)
        {
            ExceptionFreeAction(() => Callback.OnPrivateMessageReceived(clientId, message));
        }

        public void OnTeamChanged(Guid clientId, string team)
        {
            ExceptionFreeAction(() => Callback.OnTeamChanged(clientId, team));
        }

        public void OnGameCreated(GameCreateResults result, GameData game)
        {
            ExceptionFreeAction(() => Callback.OnGameCreated(result, game));
        }

        public void OnGameJoined(GameJoinResults result, GameData game, bool isGameMaster)
        {
            ExceptionFreeAction(() => Callback.OnGameJoined(result, game, isGameMaster));
        }

        public void OnGameLeft()
        {
            ExceptionFreeAction(() => Callback.OnGameLeft());
        }

        public void OnClientGameJoined(Guid clientId, bool asSpectator)
        {
            ExceptionFreeAction(() => Callback.OnClientGameJoined(clientId, asSpectator));
        }

        public void OnClientGameLeft(Guid clientId)
        {
            ExceptionFreeAction(() => Callback.OnClientGameLeft(clientId));
        }

        public void OnGameMasterModified(Guid playerId)
        {
            ExceptionFreeAction(() => Callback.OnGameMasterModified(playerId));
        }

        public void OnGameStarted(List<Pieces> pieces)
        {
            ExceptionFreeAction(() => Callback.OnGameStarted(pieces));
        }

        public void OnGamePaused()
        {
            ExceptionFreeAction(() => Callback.OnGamePaused());
        }

        public void OnGameResumed()
        {
            ExceptionFreeAction(() => Callback.OnGameResumed());
        }

        public void OnGameFinished(GameFinishedReasons reason, GameStatistics statistics)
        {
            ExceptionFreeAction(() => Callback.OnGameFinished(reason, statistics));
        }

        public void OnWinListModified(List<WinEntry> winEntries)
        {
            ExceptionFreeAction(() => Callback.OnWinListModified(winEntries));
        }

        public void OnGameOptionsChanged(GameOptions gameOptions)
        {
            ExceptionFreeAction(() => Callback.OnGameOptionsChanged(gameOptions));
        }

        public void OnVoteKickAsked(Guid sourceClient, Guid targetClient, string reason)
        {
            ExceptionFreeAction(() => Callback.OnVoteKickAsked(sourceClient, targetClient, reason));
        }

        public void OnAchievementEarned(Guid playerId, int achievementId, string achievementTitle)
        {
            ExceptionFreeAction(() => Callback.OnAchievementEarned(playerId, achievementId, achievementTitle));
        }

        public void OnPiecePlaced(int firstIndex, List<Pieces> nextPieces)
        {
            ExceptionFreeAction(() => Callback.OnPiecePlaced(firstIndex, nextPieces));
        }

        public void OnPlayerWon(Guid playerId)
        {
            ExceptionFreeAction(() => Callback.OnPlayerWon(playerId));
        }

        public void OnPlayerLost(Guid playerId)
        {
            ExceptionFreeAction(() => Callback.OnPlayerLost(playerId));
        }

        public void OnServerLinesAdded(int count)
        {
            ExceptionFreeAction(() => Callback.OnServerLinesAdded(count));
        }

        public void OnPlayerLinesAdded(Guid playerId, int specialId, int count)
        {
            ExceptionFreeAction(() => Callback.OnPlayerLinesAdded(playerId, specialId, count));
        }

        public void OnSpecialUsed(Guid playerId, Guid targetId, int specialId, Specials special)
        {
            ExceptionFreeAction(() => Callback.OnSpecialUsed(playerId, targetId, specialId, special));
        }

        public void OnGridModified(Guid playerId, byte[] grid)
        {
            ExceptionFreeAction(() => Callback.OnGridModified(playerId, grid));
        }

        public void OnContinuousSpecialFinished(Guid playerId, Specials special)
        {
            ExceptionFreeAction(() => Callback.OnContinuousSpecialFinished(playerId, special));
        }

        #endregion
    }
}
