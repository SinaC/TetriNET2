using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;
using TetriNET2.Server.Interfaces;

namespace TetriNET2.Server.ConsoleApp
{
    public class DummyClient : ITetriNETCallback
    {
        private class CallInfo
        {
            public static readonly CallInfo NullObject = new CallInfo
            {
                Count = 0,
                ParametersPerCall = null
            };

            public int Count { get; set; }
            public List<List<object>> ParametersPerCall { get; set; }
        }

        public readonly string Name;
        public readonly string Team;
        public readonly Versioning Versioning;
        public readonly IPAddress Address;

        private readonly IHost _host;
        private readonly Dictionary<string, CallInfo> _callInfos = new Dictionary<string, CallInfo>();

        public DummyClient(IHost host, string name, string team, Versioning version, IPAddress address)
        {
            _host = host;
            Name = name;
            Team = team;
            Versioning = version;
            Address = address;
        }

        private void UpdateCallInfo(string callbackName, params object[] parameters)
        {
            List<object> paramList = parameters == null ? new List<object>() : parameters.ToList();
            if (!_callInfos.ContainsKey(callbackName))
                _callInfos.Add(callbackName, new CallInfo
                {
                    Count = 1,
                    ParametersPerCall = new List<List<object>> { paramList }
                });
            else
            {
                CallInfo callInfo = _callInfos[callbackName];
                callInfo.Count++;
                callInfo.ParametersPerCall.Add(paramList);
            }
        }

        public int GetCallCount(string callbackName)
        {
            CallInfo value;
            _callInfos.TryGetValue(callbackName, out value);
            return (value ?? CallInfo.NullObject).Count;
        }

        public List<object> GetCallParameters(string callbackName, int callId)
        {
            CallInfo value;
            if (!_callInfos.TryGetValue(callbackName, out value))
                return null;
            if (callId >= value.Count)
                return null;
            return value.ParametersPerCall[callId];
        }

        public void Reset()
        {
            _callInfos.Clear();
        }

        #region ITetriNET

        public void ClientConnect(Versioning version, string name, string team)
        {
            _host.ClientConnect(this, Versioning, Name, Team);
        }

        public void ClientDisconnect()
        {
            _host.ClientDisconnect(this);
        }

        public void ClientHeartbeat()
        {
            _host.ClientHeartbeat(this);
        }

        public void ClientSendPrivateMessage(Guid targetId, string message)
        {
            _host.ClientSendPrivateMessage(this, targetId, message);
        }

        public void ClientSendBroadcastMessage(string message)
        {
            _host.ClientSendBroadcastMessage(this, message);
        }

        public void ClientChangeTeam(string team)
        {
            _host.ClientChangeTeam(this, team);
        }

        public void ClientJoinGame(Guid gameId, string password, bool asSpectator)
        {
            _host.ClientJoinGame(this, gameId, password, asSpectator);
        }

        public void ClientJoinRandomGame(bool asSpectator)
        {
            _host.ClientJoinRandomGame(this, asSpectator);
        }

        public void ClientCreateAndJoinGame(string name, string password, GameRules rule, bool asSpectator)
        {
            _host.ClientCreateAndJoinGame(this, name, password, rule, asSpectator);
        }

        public void ClientStartGame()
        {
            _host.ClientStartGame(this);
        }

        public void ClientStopGame()
        {
            _host.ClientStopGame(this);
        }

        public void ClientPauseGame()
        {
            _host.ClientPauseGame(this);
        }

        public void ClientResumeGame()
        {
            _host.ClientResumeGame(this);
        }

        public void ClientChangeOptions(GameOptions options)
        {
            _host.ClientChangeOptions(this, options);
        }

        public void ClientVoteKick(Guid targetId)
        {
            _host.ClientVoteKick(this, targetId);
        }

        public void ClientVoteKickResponse(bool accepted)
        {
            _host.ClientVoteKickResponse(this, accepted);
        }

        public void ClientResetWinList()
        {
            _host.ClientResetWinList(this);
        }

        public void ClientLeaveGame()
        {
            _host.ClientLeaveGame(this);
        }

        public void ClientPlacePiece(int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid)
        {
            _host.ClientPlacePiece(this, pieceIndex, highestIndex, piece, orientation, posX, posX, grid);
        }

        public void ClientModifyGrid(byte[] grid)
        {
            _host.ClientModifyGrid(this, grid);
        }

        public void ClientUseSpecial(Guid targetId, Specials special)
        {
            _host.ClientUseSpecial(this, targetId, special);
        }

        public void ClientClearLines(int count)
        {
            _host.ClientClearLines(this, count);
        }

        public void ClientGameLost()
        {
            _host.ClientGameLost(this);
        }

        public void ClientFinishContinuousSpecial(Specials special)
        {
            _host.ClientFinishContinuousSpecial(this, special);
        }

        public void ClientEarnAchievement(int achievementId, string achievementTitle)
        {
            _host.ClientEarnAchievement(this, achievementId, achievementTitle);
        }

        #endregion

        #region ITetriNETCallback

        public void OnConnected(ConnectResults result, Versioning serverVersion, Guid clientId, List<GameDescription> games)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, result, serverVersion, clientId, games);
        }

        public void OnDisconnected()
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnHeartbeatReceived()
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnServerStopped()
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnClientConnected(Guid clientId, string name, string team)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId, name, team);
        }

        public void OnClientDisconnected(Guid clientId, LeaveReasons reason)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId, reason);
        }

        public void OnClientGameCreated(Guid clientId, GameDescription game)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId, game);
        }

        public void OnServerMessageReceived(string message)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, message);
        }

        public void OnBroadcastMessageReceived(Guid clientId, string message)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId, message);
        }

        public void OnPrivateMessageReceived(Guid clientId, string message)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId, message);
        }

        public void OnTeamChanged(Guid clientId, string team)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId, team);
        }

        public void OnGameCreated(GameCreateResults result, GameDescription game)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, result, game);
        }

        public void OnGameJoined(GameJoinResults result, Guid gameId, GameOptions options)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, result, gameId, options);
        }

        public void OnGameLeft()
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnClientGameJoined(Guid clientId)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId);
        }

        public void OnClientGameLeft(Guid clientId)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId);
        }

        public void OnGameStarted(List<Pieces> pieces)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, pieces);
        }

        public void OnGamePaused()
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnGameResumed()
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnGameFinished(GameFinishedReasons reason, GameStatistics statistics)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, reason, statistics);
        }

        public void OnWinListModified(List<WinEntry> winEntries)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, winEntries);
        }

        public void OnGameOptionsChanged(GameOptions gameOptions)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, gameOptions);
        }

        public void OnVoteKickAsked(Guid sourceClient, Guid targetClient)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, sourceClient, targetClient);
        }

        public void OnAchievementEarned(Guid playerId, int achievementId, string achievementTitle)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, playerId, achievementId, achievementTitle);
        }

        public void OnPiecePlaced(int firstIndex, List<Pieces> nextPieces)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, firstIndex, nextPieces);
        }

        public void OnPlayerWon(Guid playerId)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, playerId);
        }

        public void OnPlayerLost(Guid playerId)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, playerId);
        }

        public void OnServerLinesAdded(int count)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, count);
        }

        public void OnPlayerLinesAdded(Guid playerId, int specialId, int count)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, playerId, specialId, count);
        }

        public void OnSpecialUsed(Guid playerId, Guid targetId, int specialId, Specials special)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, playerId, targetId, specialId, specialId);
        }

        public void OnGridModified(Guid playerId, byte[] grid)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, playerId, grid);
        }

        public void OnContinuousSpecialFinished(Guid playerId, Specials special)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, playerId, special);
        }

        #endregion
    }
}
