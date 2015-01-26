using System;
using System.Collections.Generic;
using System.Linq;
using TetriNET2.Client.Interfaces;
using TetriNET2.Common.ActionQueue;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Logger;

namespace TetriNET2.Client.ConsoleApp
{
    internal class Program
    {
        private static IClient _client;

        static void DisplayHelp()
        {
            Console.WriteLine("Commands:");
            Console.WriteLine("x: Stop client");
            Console.WriteLine("r: Get room list");
            Console.WriteLine("c: Get client list");
            Console.WriteLine("l: Get game client list");
            Console.WriteLine("j: Create and join game as player");
            Console.WriteLine("g: Join random game");
            Console.WriteLine("s: Start game");
            Console.WriteLine("t: Stop game");
        }

        public class Factory : IFactory
        {
            public IProxy CreateProxy(ITetriNETClientCallback callback, string address)
            {
                return new WCFProxy(callback, address);
            }
        }

        private static void Main(string[] args)
        {
            string clientName = "Console"+Guid.NewGuid().ToString().Substring(0,5);

            Log.Default.Logger = new NLogger();
            Log.Default.Initialize(@"D:\TEMP\LOG\",
                                   String.Format("TETRINET2_CLIENT_{0}.LOG", clientName));

            IFactory factory = new Factory();
            IActionQueue actionQueue = new BlockingActionQueue();

            _client = new Client(factory, actionQueue);
            _client.SetVersion(1, 0);
            _client.Connect("net.tcp://localhost:7788/TetriNET2Client", clientName, "team1");

            //_client.ConnectionLost += OnConnectionLost;

            _client.Connected += OnConnected;
            _client.Disconnected += OnDisconnected;

            _client.ServerStopped += OnServerStopped;

            _client.RoomListReceived += OnRoomListReceived;
            _client.ClientListReceived += OnClientListReceived;
            _client.GameClientListReceived += OnGameClientListReceived;

            _client.ClientConnected += OnClientConnected;
            _client.ClientDisconnected += OnClientDisconnected;
            _client.ClientGameCreated += OnClientGameCreated;
            _client.ServerGameCreated += OnServerGameCreated;
            _client.ServerGameDeleted += OnServerGameDeleted;

            _client.ServerMessageReceived += OnServerMessageReceived;
            _client.BroadcastMessageReceived += OnBroadcastMessageReceived;
            _client.PrivateMessageReceived += OnPrivateMessageReceived;
            _client.TeamChanged += OnTeamChanged;
            _client.GameCreated += OnGameCreated;

            _client.GameJoined += OnGameJoined;
            _client.GameLeft += OnGameLeft;
            _client.ClientGameJoined += OnClientGameJoined;
            _client.ClientGameLeft += OnClientGameLeft;
            _client.GameMasterModified += OnGameMasterModified;
            _client.GameStarted += OnGameStarted;
            _client.GamePaused += OnGamePaused;
            _client.GameResumed += OnGameResumed;
            _client.GameFinished += OnGameFinished;
            _client.WinListModified += OnWinListModified;
            _client.GameOptionsChanged += OnGameOptionsChanged;
            _client.VoteKickAsked += OnVoteKickAsked;
            _client.AchievementEarned += OnAchievementEarned;

            _client.PiecePlaced += OnPiecePlaced;
            _client.PlayerWon += OnPlayerWon;
            _client.PlayerLost += OnPlayerLost;
            _client.ServerLinesAdded += OnServerLinesAdded;
            _client.PlayerLinesAdded += OnPlayerLinesAdded;
            _client.SpecialUsed += OnSpecialUsed;
            _client.GridModified += OnGridModified;
            _client.ContinuousSpecialFinished += OnContinuousSpecialFinished;

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
                        case ConsoleKey.O:
                            _client.Connect(
                                "net.tcp://localhost:7788/TetriNET2Admin",
                                clientName, "team1");
                            break;
                        case ConsoleKey.D:
                            _client.Disconnect();
                            break;
                        case ConsoleKey.X:
                            _client.Disconnect();
                            stopped = true;
                            break;
                        case ConsoleKey.C:
                            _client.GetClientList();
                            break;
                        case ConsoleKey.R:
                            _client.GetRoomList();
                            break;
                        case ConsoleKey.L:
                            _client.GetGameClientList();
                            break;
                        case ConsoleKey.J:
                            _client.CreateAndJoinGame("GAME1" + Guid.NewGuid().ToString().Substring(0, 5), null, GameRules.Standard, false);
                            break;
                        case ConsoleKey.G:
                            _client.JoinRandomGame(false);
                            break;
                        case ConsoleKey.S:
                            _client.StartGame();
                            break;
                        case ConsoleKey.T:
                            _client.StopGame();
                            break;
                    }
                }
                else
                    System.Threading.Thread.Sleep(250);
            }
        }

        private static void DisplayClientList()
        {
            Console.WriteLine("Client list: {0}", _client.Clients.Count());
            foreach (ClientData client in _client.Clients)
                Console.WriteLine("Client: {0} {1} {2} {3} {4} {5} ", client.Id, client.Name, client.IsGameMaster, client.GameId, client.IsPlayer, client.IsSpectator);
        }

        private static void DisplayRoomList()
        {
            Console.WriteLine("Rooms: {0}", _client.Rooms.Count());
            foreach (GameRoomData room in _client.Rooms)
            {
                Console.WriteLine("Room: {0} {1} {2}", room.Id, room.Name, room.Rule);
                Console.WriteLine("\tClients: {0}", room.Clients == null ? 0 : room.Clients.Count);
                if (room.Clients != null)
                    foreach (ClientData client in room.Clients)
                        Console.WriteLine("Client: {0} {1} {2} {3} {4} {5} ", client.Id, client.Name, client.IsGameMaster, client.GameId, client.IsPlayer, client.IsSpectator);
            }
        }

        private static void OnContinuousSpecialFinished(Guid playerId, Specials special)
        {
            Console.WriteLine("OnContinuousSpecialFinished: {0} {1}", playerId, special);
        }

        private static void OnGridModified(Guid playerId, byte[] grid)
        {
            Console.WriteLine("OnGridModified: {0}", playerId);
        }

        private static void OnSpecialUsed(Guid playerId, Guid targetId, int specialId, Specials special)
        {
            Console.WriteLine("OnSpecialUsed: {0} {1} {2} {3}", playerId, targetId, specialId, special);
        }

        private static void OnPlayerLinesAdded(Guid playerId, int specialId, int count)
        {
            Console.WriteLine("OnPlayerLinesAdded: {0} {1} {2}", playerId, specialId, count);
        }

        private static void OnServerLinesAdded(int count)
        {
            Console.WriteLine("OnServerLinesAdded: {0}", count);
        }

        private static void OnPlayerLost(Guid playerId)
        {
            Console.WriteLine("OnPlayerLost: {0}", playerId);
        }

        private static void OnPlayerWon(Guid playerId)
        {
            Console.WriteLine("OnPlayerWon: {0}", playerId);
        }

        private static void OnPiecePlaced(int firstIndex, List<Pieces> nextPieces)
        {
            Console.WriteLine("OnPiecePlaced: {0} {1}", firstIndex, nextPieces == null ? 0 : nextPieces.Count);
        }

        private static void OnAchievementEarned(Guid playerId, int achievementId, string achievementTitle)
        {
            Console.WriteLine("OnAchievementEarned: {0} {1} {2}", playerId, achievementId, achievementTitle);
        }

        private static void OnVoteKickAsked(Guid sourceClient, Guid targetClient, string reason)
        {
            Console.WriteLine("OnVoteKickAsked: {0} {1} {2}", sourceClient, targetClient, reason);
        }

        private static void OnGameOptionsChanged(GameOptions gameOptions)
        {
            Console.WriteLine("OnGameOptionsChanged");
        }

        private static void OnWinListModified(List<WinEntry> winEntries)
        {
            Console.WriteLine("OnWinListModified");
        }

        private static void OnGameFinished(GameFinishedReasons reason, GameStatistics statistics)
        {
            Console.WriteLine("OnGameFinished: {0}", reason);
        }

        private static void OnGameResumed()
        {
            Console.WriteLine("OnGameResumed");
        }

        private static void OnGamePaused()
        {
            Console.WriteLine("OnGamePaused");
        }

        private static void OnGameStarted(List<Pieces> pieces)
        {
            Console.WriteLine("OnGameFinished: {0}", pieces == null ? 0 : pieces.Count);
        }

        private static void OnGameMasterModified(Guid playerId)
        {
            Console.WriteLine("OnGameMasterModified: {0}", playerId);
        }

        private static void OnClientGameLeft(Guid clientId)
        {
            Console.WriteLine("OnClientGameLeft: {0}", clientId);
        }

        private static void OnClientGameJoined(Guid clientId, bool asSpectator)
        {
            Console.WriteLine("OnClientGameJoined: {0} {1}", clientId, asSpectator);
        }

        private static void OnGameLeft()
        {
            Console.WriteLine("OnGameLeft");
        }

        private static void OnGameJoined(GameJoinResults result, Guid gameId, GameOptions options, bool isGameMaster)
        {
            Console.WriteLine("OnGameJoined: {0} {1} {2}", result, gameId, isGameMaster);
        }

        private static void OnGameCreated(GameCreateResults result, GameRoomData game)
        {
            Console.WriteLine("OnGameCreated: {0} {1} {2} {3}", result, game.Id, game.Name, game.Rule);
        }

        private static void OnTeamChanged(Guid clientId, string team)
        {
            Console.WriteLine("OnTeamChanged: {0} {1}", clientId, team);
        }

        private static void OnPrivateMessageReceived(Guid clientId, string message)
        {
            Console.WriteLine("OnPrivateMessageReceived: {0} {1}", clientId, message);
        }

        private static void OnBroadcastMessageReceived(Guid clientId, string message)
        {
            Console.WriteLine("OnBroadcastMessageReceived: {0} {1}", clientId, message);
        }

        private static void OnServerMessageReceived(string message)
        {
            Console.WriteLine("OnServerMessageReceived: {0}", message);
        }

        private static void OnServerGameDeleted(Guid gameId)
        {
            Console.WriteLine("OnGameDeleted: {0}", gameId);

            DisplayRoomList();
        }

        private static void OnServerGameCreated(GameRoomData game)
        {
            Console.WriteLine("OnServerGameCreated: {0} {1} {2}", game.Id, game.Name, game.Rule);
            if (game.Clients != null)
            {
                Console.WriteLine("\tClients: {0}", game.Clients.Count);
                foreach (ClientData client in game.Clients)
                    Console.WriteLine("Client: {0} {1} {2} {3} {4} {5}", client.Id, client.Name, client.GameId, client.IsGameMaster, client.IsPlayer, client.IsSpectator);
            }

            DisplayRoomList();
        }

        private static void OnClientGameCreated(Guid clientId, GameRoomData game)
        {
            Console.WriteLine("OnClientGameCreated: {0} {1} {2} {3}", clientId, game.Id, game.Name, game.Rule);
            if (game.Clients != null)
            {
                Console.WriteLine("\tClients: {0}", game.Clients.Count);
                foreach (ClientData client in game.Clients)
                    Console.WriteLine("\tClient: {0} {1} {2} {3} {4} {5}", client.Id, client.Name, client.GameId, client.IsGameMaster, client.IsPlayer, client.IsSpectator);
            }

            DisplayRoomList();
        }

        private static void OnClientDisconnected(Guid clientId, LeaveReasons reason)
        {
            Console.WriteLine("OnClientDisconnected: {0} {1}", clientId, reason);

            DisplayClientList();
        }

        private static void OnClientConnected(Guid clientId, string name, string team)
        {
            Console.WriteLine("OnClientConnected: {0} {1} {2}", clientId, name, team);

            DisplayClientList();
        }

        private static void OnGameClientListReceived(List<ClientData> clients)
        {
            Console.WriteLine("Clients in game: {0}", clients == null ? 0 : clients.Count);
            if (clients != null)
                foreach (ClientData client in clients)
                    Console.WriteLine("Client: {0} {1} {2} {3} {4} {5}", client.Id, client.Name, client.GameId, client.IsGameMaster, client.IsPlayer, client.IsSpectator);
        }

        private static void OnClientListReceived(List<ClientData> clients)
        {
            Console.WriteLine("Clients: {0}", clients == null ? 0 : clients.Count);
            if (clients != null)
                foreach (ClientData client in clients)
                    Console.WriteLine("Client: {0} {1} {2} {3} {4} {5}", client.Id, client.Name, client.GameId, client.IsGameMaster, client.IsPlayer, client.IsSpectator);
        }

        private static void OnRoomListReceived(List<GameRoomData> rooms)
        {
            Console.WriteLine("Rooms: {0}", rooms == null ? 0 : rooms.Count);
            if (rooms != null)
                foreach (GameRoomData room in rooms)
                {
                    Console.WriteLine("Room: {0} {1} {2}", room.Id, room.Name, room.Rule);
                    Console.WriteLine("\tClients: {0}", room.Clients == null ? 0 : room.Clients.Count);
                    if (room.Clients != null)
                        foreach (ClientData client in room.Clients)
                            Console.WriteLine("\tClient: {0} {1} {2} {3} {4} {5}", client.Id, client.Name, client.GameId, client.IsGameMaster, client.IsPlayer, client.IsSpectator);
                }
        }

        private static void OnServerStopped()
        {
            Console.WriteLine("OnServerStopped");
        }

        private static void OnDisconnected()
        {
            Console.WriteLine("OnDisconnected");
        }

        private static void OnConnected(ConnectResults result, Versioning serverVersion, Guid clientId, List<GameRoomData> games)
        {
            Console.WriteLine("OnConnected: {0} {1}.{2} {3}", result, serverVersion == null ? -1 : serverVersion.Major, serverVersion == null ? -1 : serverVersion.Minor, clientId);
            Console.WriteLine("Game list: {0}", games == null ? 0 : games.Count);
            if (games != null)
                foreach (GameRoomData game in games)
                {
                    Console.WriteLine("Room: {0} {1} {2}", game.Id, game.Name, game.Rule);
                    Console.WriteLine("\tClients: {0}", game.Clients == null ? 0 : game.Clients.Count);
                    if (game.Clients != null)
                        foreach (ClientData client in game.Clients)
                            Console.WriteLine("\tClient: {0} {1} {2} {3} {4} {5}", client.Id, client.Name, client.GameId, client.IsGameMaster, client.IsPlayer, client.IsSpectator);
                }
            }
        }

    //    private static void Main2(string[] args)
    //    {
    //        CountCallTetriNETCallback callbackInstance = new CountCallTetriNETCallback();

    //        DuplexChannelFactory<ITetriNETClient> factory;
    //        ITetriNETClient proxy;

    //        EndpointAddress endpointAddress = new EndpointAddress("net.tcp://localhost:7788/TetriNET2Client");
    //        Binding binding = new NetTcpBinding(SecurityMode.None);
    //        InstanceContext instanceContext = new InstanceContext(callbackInstance);
    //        factory = new DuplexChannelFactory<ITetriNETClient>(instanceContext, binding, endpointAddress);
    //        proxy = factory.CreateChannel(instanceContext);

    //        proxy.ClientConnect(
    //            new Versioning
    //                {
    //                    Major = 1,
    //                    Minor = 0
    //                },
    //            "client1" + Guid.NewGuid().ToString().Substring(0, 5), "team1");

    //        bool stopped = false;
    //        while (!stopped)
    //        {
    //            if (Console.KeyAvailable)
    //            {
    //                ConsoleKeyInfo cki = Console.ReadKey(true);
    //                switch (cki.Key)
    //                {
    //                    default:
    //                        DisplayHelp();
    //                        break;
    //                    case ConsoleKey.X:
    //                        proxy.ClientDisconnect();
    //                        stopped = true;
    //                        break;
    //                    case ConsoleKey.R:
    //                        proxy.ClientGetRoomList();
    //                        break;
    //                    case ConsoleKey.C:
    //                        proxy.ClientGetClientList();
    //                        break;
    //                    case ConsoleKey.L:
    //                        proxy.ClientGetGameClientList();
    //                        break;
    //                    case ConsoleKey.J:
    //                        proxy.ClientCreateAndJoinGame("GAME1" + Guid.NewGuid().ToString().Substring(0, 5), null, GameRules.Standard, false);
    //                        break;
    //                    case ConsoleKey.G:
    //                        proxy.ClientJoinRandomGame(false);
    //                        break;
    //                    case ConsoleKey.S:
    //                        proxy.ClientStartGame();
    //                        break;
    //                    case ConsoleKey.T:
    //                        proxy.ClientStopGame();
    //                        break;
    //                }
    //            }
    //            else
    //            {
    //                proxy.ClientHeartbeat();

    //                System.Threading.Thread.Sleep(250);
    //            }
    //        }
    //    }
    //}

    //public class CountCallTetriNETCallback : ITetriNETClientCallback
    //{
    //    private class CallInfo
    //    {
    //        public static readonly CallInfo NullObject = new CallInfo
    //            {
    //                Count = 0,
    //                ParametersPerCall = null
    //            };

    //        public int Count { get; set; }
    //        public List<List<object>> ParametersPerCall { get; set; }
    //    }

    //    private readonly Dictionary<string, CallInfo> _callInfos = new Dictionary<string, CallInfo>();

    //    private void UpdateCallInfo(string callbackName, params object[] parameters)
    //    {
    //        Console.WriteLine("Callback: {0} {1}", callbackName, parameters == null || parameters.Length == 0 ? "(none)" : parameters.Select(x => x == null ? "[null]" : x.ToString()).Aggregate((n, i) => n + "[" + i + "]"));

    //        List<object> paramList = parameters == null ? new List<object>() : parameters.ToList();
    //        if (!_callInfos.ContainsKey(callbackName))
    //            _callInfos.Add(callbackName, new CallInfo
    //                {
    //                    Count = 1,
    //                    ParametersPerCall = new List<List<object>>
    //                        {
    //                            paramList
    //                        }
    //                });
    //        else
    //        {
    //            CallInfo callInfo = _callInfos[callbackName];
    //            callInfo.Count++;
    //            callInfo.ParametersPerCall.Add(paramList);
    //        }
    //    }

    //    public int GetCallCount(string callbackName)
    //    {
    //        CallInfo value;
    //        _callInfos.TryGetValue(callbackName, out value);
    //        return (value ?? CallInfo.NullObject).Count;
    //    }

    //    public List<object> GetCallParameters(string callbackName, int callId)
    //    {
    //        CallInfo value;
    //        if (!_callInfos.TryGetValue(callbackName, out value))
    //            return null;
    //        if (callId >= value.Count)
    //            return null;
    //        return value.ParametersPerCall[callId];
    //    }

    //    public void Reset()
    //    {
    //        _callInfos.Clear();
    //    }

    //    #region ITetriNETCallback

    //    public void OnConnected(ConnectResults result, Versioning serverVersion, Guid clientId, List<GameRoomData> games)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, result, serverVersion, clientId, games);
    //    }

    //    public void OnDisconnected()
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
    //    }

    //    public void OnHeartbeatReceived()
    //    {
    //        //UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
    //    }

    //    public void OnServerStopped()
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
    //    }

    //    public void OnRoomListReceived(List<GameRoomData> rooms)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, rooms);
    //        Console.WriteLine("Rooms: {0}", rooms == null ? 0 : rooms.Count);
    //        if (rooms != null)
    //            foreach (GameRoomData room in rooms)
    //            {
    //                Console.WriteLine("Room: {0} {1} {2}", room.Id, room.Name, room.Rule);
    //                Console.WriteLine("\tClients: {0}", room.Clients == null ? 0 : room.Clients.Count);
    //                if (room.Clients != null)
    //                    foreach (ClientData client in room.Clients)
    //                        Console.WriteLine("\tClient: {0} {1} {2} {3} {4} {5}", client.Id, client.Name, client.GameId, client.IsPlayer, client.IsSpectator, client.IsGameMaster);
    //            }
    //    }

    //    public void OnClientListReceived(List<ClientData> clients)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clients);
    //        Console.WriteLine("Clients: {0}", clients == null ? 0 : clients.Count);
    //        if (clients != null)
    //            foreach (ClientData client in clients)
    //                Console.WriteLine("Client: {0} {1} {2} {3} {4} {5}", client.Id, client.Name, client.GameId, client.IsPlayer, client.IsSpectator, client.IsGameMaster);
    //    }

    //    public void OnGameClientListReceived(List<ClientData> clients)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clients);
    //        Console.WriteLine("Clients in game: {0}", clients == null ? 0 : clients.Count);
    //        if (clients != null)
    //            foreach (ClientData client in clients)
    //                Console.WriteLine("Client: {0} {1} {2} {3} {4} {5}", client.Id, client.Name, client.GameId, client.IsPlayer, client.IsSpectator, client.IsGameMaster);
    //    }

    //    public void OnClientConnected(Guid clientId, string name, string team)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId, name, team);
    //    }

    //    public void OnClientDisconnected(Guid clientId, LeaveReasons reason)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId, reason);
    //    }

    //    public void OnClientGameCreated(Guid clientId, GameRoomData game)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId, game);
    //    }

    //    public void OnServerGameCreated(GameRoomData game)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, game);
    //    }

    //    public void OnServerGameDeleted(Guid gameId)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, gameId);
    //    }

    //    public void OnServerMessageReceived(string message)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, message);
    //    }

    //    public void OnBroadcastMessageReceived(Guid clientId, string message)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId, message);
    //    }

    //    public void OnPrivateMessageReceived(Guid clientId, string message)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId, message);
    //    }

    //    public void OnTeamChanged(Guid clientId, string team)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId, team);
    //    }

    //    public void OnGameCreated(GameCreateResults result, GameRoomData game)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, result, game);
    //    }

    //    public void OnGameJoined(GameJoinResults result, Guid gameId, GameOptions options, bool isGameMaster)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, result, gameId, options, isGameMaster);
    //    }

    //    public void OnGameLeft()
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
    //    }

    //    public void OnClientGameJoined(Guid clientId, bool asSpectator)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId, asSpectator);
    //    }

    //    public void OnClientGameLeft(Guid clientId)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, clientId);
    //    }

    //    public void OnGameMasterModified(Guid playerId)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, playerId);
    //    }

    //    public void OnGameStarted(List<Pieces> pieces)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, pieces);
    //    }

    //    public void OnGamePaused()
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
    //    }

    //    public void OnGameResumed()
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name);
    //    }

    //    public void OnGameFinished(GameFinishedReasons reason, GameStatistics statistics)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, reason, statistics);
    //    }

    //    public void OnWinListModified(List<WinEntry> winEntries)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, winEntries);
    //    }

    //    public void OnGameOptionsChanged(GameOptions gameOptions)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, gameOptions);
    //    }

    //    public void OnVoteKickAsked(Guid sourceClient, Guid targetClient, string reason)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, sourceClient, targetClient, reason);
    //    }

    //    public void OnAchievementEarned(Guid playerId, int achievementId, string achievementTitle)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, playerId, achievementId, achievementTitle);
    //    }

    //    public void OnPiecePlaced(int firstIndex, List<Pieces> nextPieces)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, firstIndex, nextPieces);
    //    }

    //    public void OnPlayerWon(Guid playerId)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, playerId);
    //    }

    //    public void OnPlayerLost(Guid playerId)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, playerId);
    //    }

    //    public void OnServerLinesAdded(int count)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, count);
    //    }

    //    public void OnPlayerLinesAdded(Guid playerId, int specialId, int count)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, playerId, specialId, count);
    //    }

    //    public void OnSpecialUsed(Guid playerId, Guid targetId, int specialId, Specials special)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, playerId, targetId, specialId, specialId);
    //    }

    //    public void OnGridModified(Guid playerId, byte[] grid)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, playerId, grid);
    //    }

    //    public void OnContinuousSpecialFinished(Guid playerId, Specials special)
    //    {
    //        UpdateCallInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, playerId, special);
    //    }

    //    #endregion
    //}
}
