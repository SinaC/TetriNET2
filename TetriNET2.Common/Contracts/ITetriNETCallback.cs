using System;
using System.Collections.Generic;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Common.Contracts
{
    public interface ITetriNETCallback
    {
        void OnConnected(ConnectResults result, Versioning serverVersion, Guid clientId, List<GameDescription> games);
        void OnDisconnected();
        void OnHeartbeatReceived();

        void OnServerStopped();

        void OnClientConnected(Guid clientId, string name, string team);
        void OnClientDisconnected(Guid clientId, LeaveReasons reason);
        void OnClientGameCreated(Guid clientId, GameDescription game);
        
        void OnServerMessageReceived(string message);
        void OnBroadcastMessageReceived(Guid clientId, string message);
        void OnPrivateMessageReceived(Guid clientId, string message);
        void OnTeamChanged(Guid clientId, string team);
        void OnGameCreated(GameCreateResults result, GameDescription game);

        void OnGameJoined(GameJoinResults result, Guid gameId, GameOptions options);
        void OnGameLeft();
        void OnClientGameJoined(Guid clientId, bool asSpectator);
        void OnClientGameLeft(Guid clientId);
        void OnGameStarted(List<Pieces> pieces);
        void OnGamePaused();
        void OnGameResumed();
        void OnGameFinished(GameFinishedReasons reason, GameStatistics statistics);
        void OnWinListModified(List<WinEntry> winEntries);
        void OnGameOptionsChanged(GameOptions gameOptions);
        void OnVoteKickAsked(Guid sourceClient, Guid targetClient, string reason);
        void OnAchievementEarned(Guid playerId, int achievementId, string achievementTitle);

        void OnPiecePlaced(int firstIndex, List<Pieces> nextPieces);
        void OnPlayerWon(Guid playerId);
        void OnPlayerLost(Guid playerId);
        void OnServerLinesAdded(int count);
        void OnPlayerLinesAdded(Guid playerId, int specialId, int count);
        void OnSpecialUsed(Guid playerId, Guid targetId, int specialId, Specials special);
        void OnGridModified(Guid playerId, byte[] grid);
        void OnContinuousSpecialFinished(Guid playerId, Specials special);
    }
}
