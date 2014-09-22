using System;
using System.Collections.Generic;
using System.ServiceModel;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Common.Contracts
{
    public interface ITetriNETCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnConnected(ConnectResults result, Versioning serverVersion, Guid clientId, List<GameDescription> games);
        [OperationContract(IsOneWay = true)]
        void OnDisconnected();
        [OperationContract(IsOneWay = true)]
        void OnHeartbeatReceived();

        [OperationContract(IsOneWay = true)]
        void OnServerStopped();

        [OperationContract(IsOneWay = true)]
        void OnClientConnected(Guid clientId, string name, string team);
        [OperationContract(IsOneWay = true)]
        void OnClientDisconnected(Guid clientId, LeaveReasons reason);
        [OperationContract(IsOneWay = true)]
        void OnClientGameCreated(Guid clientId, GameDescription game);

        [OperationContract(IsOneWay = true)]
        void OnServerMessageReceived(string message);
        [OperationContract(IsOneWay = true)]
        void OnBroadcastMessageReceived(Guid clientId, string message);
        [OperationContract(IsOneWay = true)]
        void OnPrivateMessageReceived(Guid clientId, string message);
        [OperationContract(IsOneWay = true)]
        void OnTeamChanged(Guid clientId, string team);
        [OperationContract(IsOneWay = true)]
        void OnGameCreated(GameCreateResults result, GameDescription game);

        [OperationContract(IsOneWay = true)]
        void OnGameJoined(GameJoinResults result, Guid gameId, GameOptions options);
        [OperationContract(IsOneWay = true)]
        void OnGameLeft();
        [OperationContract(IsOneWay = true)]
        void OnClientGameJoined(Guid clientId, bool asSpectator);
        [OperationContract(IsOneWay = true)]
        void OnClientGameLeft(Guid clientId);
        [OperationContract(IsOneWay = true)]
        void OnGameStarted(List<Pieces> pieces);
        [OperationContract(IsOneWay = true)]
        void OnGamePaused();
        [OperationContract(IsOneWay = true)]
        void OnGameResumed();
        [OperationContract(IsOneWay = true)]
        void OnGameFinished(GameFinishedReasons reason, GameStatistics statistics);
        [OperationContract(IsOneWay = true)]
        void OnWinListModified(List<WinEntry> winEntries);
        [OperationContract(IsOneWay = true)]
        void OnGameOptionsChanged(GameOptions gameOptions);
        [OperationContract(IsOneWay = true)]
        void OnVoteKickAsked(Guid sourceClient, Guid targetClient, string reason);
        [OperationContract(IsOneWay = true)]
        void OnAchievementEarned(Guid playerId, int achievementId, string achievementTitle);

        [OperationContract(IsOneWay = true)]
        void OnPiecePlaced(int firstIndex, List<Pieces> nextPieces);
        [OperationContract(IsOneWay = true)]
        void OnPlayerWon(Guid playerId);
        [OperationContract(IsOneWay = true)]
        void OnPlayerLost(Guid playerId);
        [OperationContract(IsOneWay = true)]
        void OnServerLinesAdded(int count);
        [OperationContract(IsOneWay = true)]
        void OnPlayerLinesAdded(Guid playerId, int specialId, int count);
        [OperationContract(IsOneWay = true)]
        void OnSpecialUsed(Guid playerId, Guid targetId, int specialId, Specials special);
        [OperationContract(IsOneWay = true)]
        void OnGridModified(Guid playerId, byte[] grid);
        [OperationContract(IsOneWay = true)]
        void OnContinuousSpecialFinished(Guid playerId, Specials special);
    }
}
