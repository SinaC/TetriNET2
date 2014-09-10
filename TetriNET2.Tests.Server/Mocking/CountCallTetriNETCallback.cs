using System;
using System.Collections.Generic;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Tests.Server.Mocking
{
    public class CountCallTetriNETCallback : ITetriNETCallback
    {
        private readonly Dictionary<string, int> _callCount = new Dictionary<string, int>();

        private void UpdateCallCount(string callbackName)
        {
            if (!_callCount.ContainsKey(callbackName))
                _callCount.Add(callbackName, 1);
            else
                _callCount[callbackName]++;
        }

        public int GetCallCount(string callbackName)
        {
            int value;
            _callCount.TryGetValue(callbackName, out value);
            return value;
        }

        #region ITetriNETCallback

        public void OnConnected(ConnectResults result, Versioning serverVersion, Guid clientId, List<GameDescription> games)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnDisconnected()
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnHeartbeatReceived()
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnServerStopped()
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnClientConnected(Guid clientId, string name, string team)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnClientDisconnected(Guid clientId, LeaveReasons reason)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnClientGameCreated(Guid clientId, GameDescription game)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnServerMessageReceived(string message)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnBroadcastMessageReceived(Guid clientId, string message)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnPrivateMessageReceived(Guid clientId, string message)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnTeamChanged(Guid clientId, string team)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnGameCreated(GameCreateResults result, GameDescription game)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnGameJoined(GameJoinResults result, Guid gameId, GameOptions options)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnClientGameJoined(Guid clientId)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnClientGameLeft(Guid clientId)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnGameStarted(List<Pieces> pieces)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnGamePaused()
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnGameResumed()
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnGameFinished(GameFinishedReasons reason, GameStatistics statistics)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnWinListModified(List<WinEntry> winEntries)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnGameOptionsChanged(GameOptions gameOptions)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnVoteKickAsked(Guid sourceClient, Guid targetClient)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnAchievementEarned(Guid playerId, int achievementId, string achievementTitle)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnPiecePlaced(int firstIndex, List<Pieces> nextPieces)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnPlayerWon(Guid playerId)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnPlayerLost(Guid playerId)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnServerLinesAdded(int count)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnPlayerLinesAdded(Guid playerId, int specialId, int count)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnSpecialUsed(Guid playerId, Guid targetId, int specialId, Specials special)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnGridModified(Guid playerId, byte[] grid)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnContinuousSpecialFinished(Guid playerId, Specials special)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        #endregion
    }

}

