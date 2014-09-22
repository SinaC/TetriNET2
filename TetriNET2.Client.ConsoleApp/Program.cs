using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.Contracts.WCF;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Client.ConsoleApp
{
    internal class Program
    {
        static void DisplayHelp()
        {
            Console.WriteLine("Commands:");
            Console.WriteLine("x: Stop client");
            Console.WriteLine("j: Create and join game as player");
            Console.WriteLine("s: Start game");
        }

        private static void Main(string[] args)
        {
            CountCallTetriNETCallback callbackInstance = new CountCallTetriNETCallback();

            DuplexChannelFactory<IWCFTetriNET> factory;
            IWCFTetriNET proxy;

            EndpointAddress endpointAddress = new EndpointAddress("net.tcp://localhost:7788/TetriNET2");
            Binding binding = new NetTcpBinding(SecurityMode.None);
            InstanceContext instanceContext = new InstanceContext(callbackInstance);
            factory = new DuplexChannelFactory<IWCFTetriNET>(instanceContext, binding, endpointAddress);
            proxy = factory.CreateChannel(instanceContext);

            proxy.ClientConnect(
                new Versioning
                    {
                        Major = 1,
                        Minor = 0
                    },
                "client1", "team1");

            bool stopped = false;
            while (!stopped)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo cki = Console.ReadKey(true);
                    switch (cki.Key)
                    {
                        default:
                            DisplayHelp();
                            break;
                        case ConsoleKey.X:
                            stopped = true;
                            break;
                        case ConsoleKey.J:
                            proxy.ClientCreateAndJoinGame("GAME1", null, GameRules.Standard, false);
                            break;
                        case ConsoleKey.S:
                            proxy.ClientStartGame();
                            break;
                    }
                }
                else
                {
                    proxy.ClientHeartbeat();

                    System.Threading.Thread.Sleep(250);
                }
            }
        }
    }

    public class CountCallTetriNETCallback : ITetriNETCallback
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

        private readonly Dictionary<string, CallInfo> _callInfos = new Dictionary<string, CallInfo>();

        private void UpdateCallInfo(string callbackName, params object[] parameters)
        {
            Console.WriteLine("Callback: {0} {1}", callbackName, parameters == null || parameters.Length == 0 ? "(none)" : parameters.Select(x => x == null ? "[null]" : x.ToString()).Aggregate((n, i) => n + "[" + i + "]"));

            List<object> paramList = parameters == null ? new List<object>() : parameters.ToList();
            if (!_callInfos.ContainsKey(callbackName))
                _callInfos.Add(callbackName, new CallInfo
                    {
                        Count = 1,
                        ParametersPerCall = new List<List<object>>
                            {
                                paramList
                            }
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

        public void OnClientGameJoined(Guid clientId, bool asSpectator)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId, asSpectator);
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

        public void OnVoteKickAsked(Guid sourceClient, Guid targetClient, string reason)
        {
            UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, sourceClient, targetClient, reason);
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
