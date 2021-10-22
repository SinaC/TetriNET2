using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;
using TetriNET2.Server.Interfaces;
using TetriNET2.Server.Interfaces.IHost;
using TetriNET2.Server.Tests.Mocking;

namespace TetriNET2.Server.Tests.ClientSide
{
    public class ClientFake : ITetriNETClientCallback, IDisposable
    {
        public IHost Host { get; set; }
        
        public readonly string Name;
        public readonly string Team;
        public readonly Versioning Versioning;
        public readonly IAddress Address;

        private readonly Timer _timer;

        public ClientFake(string name, string team, Versioning version, IAddress address)
        {
            Name = name;
            Team = team;
            Versioning = version;
            Address = address;
            _timer = new Timer(TimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        private void TimerCallback(object state)
        {
            ClientHeartbeat();
        }

        private void SetCallbackAndAddress()
        {
            if (Host is HostMock hostMock)
            {
                hostMock.ClientCallback = this;
                hostMock.Address = Address;
            }
        }

        #region ITetriNET

        public void ClientConnect()
        {
            SetCallbackAndAddress();
            Host.ClientConnect(Versioning, Name, Team);
            _timer.Change(500, 350);
        }

        public void ClientDisconnect()
        {
            SetCallbackAndAddress();
            Host.ClientDisconnect();
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void ClientHeartbeat()
        {
            SetCallbackAndAddress();
            Host.ClientHeartbeat();
        }

        public void ClientSendPrivateMessage(Guid targetId, string message)
        {
            SetCallbackAndAddress();
            Host.ClientSendPrivateMessage(targetId, message);
        }

        public void ClientSendBroadcastMessage(string message)
        {
            SetCallbackAndAddress();
            Host.ClientSendBroadcastMessage(message);
        }

        public void ClientChangeTeam(string team)
        {
            SetCallbackAndAddress();
            Host.ClientChangeTeam(team);
        }

        public void ClientJoinGame(Guid gameId, string password, bool asSpectator)
        {
            SetCallbackAndAddress();
            Host.ClientJoinGame(gameId, password, asSpectator);
        }

        public void ClientJoinRandomGame(bool asSpectator)
        {
            SetCallbackAndAddress();
            Host.ClientJoinRandomGame(asSpectator);
        }

        public void ClientCreateAndJoinGame(string name, string password, GameRules rule, bool asSpectator)
        {
            SetCallbackAndAddress();
            Host.ClientCreateAndJoinGame(name, password, rule, asSpectator);
        }

        public void ClientGetGameList()
        {
            SetCallbackAndAddress();
            Host.ClientGetGameList();
        }

        public void ClientGetClientList()
        {
            SetCallbackAndAddress();
            Host.ClientGetClientList();
        }

        public void ClientStartGame()
        {
            SetCallbackAndAddress();
            Host.ClientStartGame();
        }

        public void ClientStopGame()
        {
            SetCallbackAndAddress();
            Host.ClientStopGame();
        }

        public void ClientPauseGame()
        {
            SetCallbackAndAddress();
            Host.ClientPauseGame();
        }

        public void ClientResumeGame()
        {
            SetCallbackAndAddress();
            Host.ClientResumeGame();
        }

        public void ClientChangeOptions(GameOptions options)
        {
            SetCallbackAndAddress();
            Host.ClientChangeOptions(options);
        }

        public void ClientVoteKick(Guid targetId, string reason)
        {
            SetCallbackAndAddress();
            Host.ClientVoteKick(targetId, reason);
        }

        public void ClientVoteKickResponse(bool accepted)
        {
            SetCallbackAndAddress();
            Host.ClientVoteKickResponse(accepted);
        }

        public void ClientResetWinList()
        {
            SetCallbackAndAddress();
            Host.ClientResetWinList();
        }

        public void ClientLeaveGame()
        {
            SetCallbackAndAddress();
            Host.ClientLeaveGame();
        }

        public void ClientGetGameClientList()
        {
            SetCallbackAndAddress();
            Host.ClientGetGameClientList();
        }

        public void ClientPlacePiece(int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid)
        {
            SetCallbackAndAddress();
            Host.ClientPlacePiece(pieceIndex, highestIndex, piece, orientation, posX, posX, grid);
        }

        public void ClientModifyGrid(byte[] grid)
        {
            SetCallbackAndAddress();
            Host.ClientModifyGrid(grid);
        }

        public void ClientUseSpecial(Guid targetId, Specials special)
        {
            SetCallbackAndAddress();
            Host.ClientUseSpecial(targetId, special);
        }

        public void ClientClearLines(int count)
        {
            SetCallbackAndAddress();
            Host.ClientClearLines(count);
        }

        public void ClientGameLost()
        {
            SetCallbackAndAddress();
            Host.ClientGameLost();
        }

        public void ClientFinishContinuousSpecial(Specials special)
        {
            SetCallbackAndAddress();
            Host.ClientFinishContinuousSpecial(special);
        }

        public void ClientEarnAchievement(int achievementId, string achievementTitle)
        {
            SetCallbackAndAddress();
            Host.ClientEarnAchievement(achievementId, achievementTitle);
        }

        #endregion

        #region ITetriNETCallback

        public void OnConnected(ConnectResults result, Versioning serverVersion, Guid clientId, List<GameData> games)
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

        public void OnGameListReceived(List<GameData> games)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, games);
        }

        public void OnClientListReceived(List<ClientData> clients)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clients);
        }

        public void OnGameClientListReceived(List<ClientData> clients)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clients);
        }

        public void OnClientConnected(Guid clientId, string name, string team)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId, name, team);
        }

        public void OnClientDisconnected(Guid clientId, LeaveReasons reason)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId, reason);
        }

        public void OnClientGameCreated(Guid clientId, GameData game)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId, game);
        }

        public void OnServerGameCreated(GameData game)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, game);
        }

        public void OnServerGameDeleted(Guid gameId)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, gameId);
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

        public void OnGameCreated(GameCreateResults result, GameData game)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, result, game);
        }

        public void OnGameJoined(GameJoinResults result, GameData game, bool isGameMaster)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, result, game, isGameMaster);
        }

        public void OnGameLeft()
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnClientGameJoined(Guid clientId, bool asSpectator)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId);
        }

        public void OnClientGameLeft(Guid clientId)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId);
        }

        public void OnGameMasterModified(Guid playerId)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, playerId);
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

        public void OnVoteKickAsked(Guid sourceClient, Guid targetClient, string reason)
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

        #region IDisposable

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region Call Info

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

        private readonly Dictionary<string, CallInfo> _callInfos = new Dictionary<string, CallInfo>();

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
            _callInfos.TryGetValue(callbackName, out var value);
            return (value ?? CallInfo.NullObject).Count;
        }

        public List<object> GetCallParameters(string callbackName, int callId)
        {
            if (!_callInfos.TryGetValue(callbackName, out var value))
                return null;
            if (callId >= value.Count)
                return null;
            return value.ParametersPerCall[callId];
        }

        public void ResetCallInfo()
        {
            _callInfos.Clear();
        }

        #endregion
    }
}
