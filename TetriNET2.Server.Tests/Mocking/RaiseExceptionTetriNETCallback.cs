using System;
using System.Collections.Generic;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Server.Tests.Mocking
{
    public class RaiseExceptionTetriNETCallback : ITetriNETClientCallback
    {
        public void OnConnected(ConnectResults result, Versioning serverVersion, Guid clientId, List<GameRoomData> games)
        {
            throw new NotImplementedException();
        }
        public void OnDisconnected()
        {
            throw new NotImplementedException();
        }
        public void OnHeartbeatReceived()
        {
            throw new NotImplementedException();
        }
        public void OnServerStopped()
        {
            throw new NotImplementedException();
        }
        public void OnRoomListReceived(List<GameRoomData> rooms)
        {
            throw new NotImplementedException();
        }
        public void OnClientListReceived(List<ClientData> clients)
        {
            throw new NotImplementedException();
        }
        public void OnGameClientListReceived(List<ClientData> clients)
        {
            throw new NotImplementedException();
        }
        public void OnClientConnected(Guid clientId, string name, string team)
        {
            throw new NotImplementedException();
        }
        public void OnClientDisconnected(Guid clientId, LeaveReasons reason)
        {
            throw new NotImplementedException();
        }
        public void OnClientGameCreated(Guid clientId, GameRoomData game)
        {
            throw new NotImplementedException();
        }
        public void OnServerMessageReceived(string message)
        {
            throw new NotImplementedException();
        }
        public void OnBroadcastMessageReceived(Guid clientId, string message)
        {
            throw new NotImplementedException();
        }
        public void OnPrivateMessageReceived(Guid clientId, string message)
        {
            throw new NotImplementedException();
        }
        public void OnTeamChanged(Guid clientId, string team)
        {
            throw new NotImplementedException();
        }
        public void OnGameCreated(GameCreateResults result, GameRoomData game)
        {
            throw new NotImplementedException();
        }
        public void OnGameJoined(GameJoinResults result, Guid gameId, GameOptions options, bool isGameMaster)
        {
            throw new NotImplementedException();
        }
        public void OnGameLeft()
        {
            throw new NotImplementedException();
        }
        public void OnClientGameJoined(Guid clientId, bool asSpectator)
        {
            throw new NotImplementedException();
        }
        public void OnClientGameLeft(Guid clientId)
        {
            throw new NotImplementedException();
        }
        public void OnGameMasterModified(Guid playerId)
        {
            throw new NotImplementedException();
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
    }
}
