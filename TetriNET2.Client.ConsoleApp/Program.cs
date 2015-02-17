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
            Console.WriteLine("g: Get game list");
            Console.WriteLine("c: Get client list");
            Console.WriteLine("l: Get game client list");
            Console.WriteLine("j: Create and join game as player");
            Console.WriteLine("r: Join random game");
            Console.WriteLine("s: Start game");
            Console.WriteLine("t: Stop game");
        }

        public class Factory : IFactory
        {
            public IActionQueue CreateActionQueue()
            {
                return new BlockingActionQueue();
            }

            public IProxy CreateProxy(ITetriNETClientCallback callback, string address)
            {
                return new WCFProxy(callback, address);
            }

            public IInventory CreateInventory(int size)
            {
                return new Inventory(size);
            }

            public IPieceBag CreatePieceBag(int size)
            {
                return new PieceArray(size);
            }
        }

        private static void Main(string[] args)
        {
            string clientName = "Console"+Guid.NewGuid().ToString().Substring(0,5);

            Log.Default.Logger = new NLogger();
            Log.Default.Initialize(@"D:\TEMP\LOG\",
                                   String.Format("TETRINET2_CLIENT_{0}.LOG", clientName));

            IFactory factory = new Factory();

            _client = new Client(factory);
            _client.SetVersion(1, 0);
            _client.Connect("net.tcp://localhost:7788/TetriNET2Client", clientName, "team1");

            //_client.ConnectionLost += OnConnectionLost;

            _client.Connected += OnConnected;
            _client.Disconnected += OnDisconnected;

            _client.ServerStopped += OnServerStopped;

            _client.GameListReceived += OnGameListReceived;
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
                        case ConsoleKey.G:
                            _client.GetGameList();
                            break;
                        case ConsoleKey.L:
                            _client.GetGameClientList();
                            break;
                        case ConsoleKey.J:
                            _client.CreateAndJoinGame("GAME1" + Guid.NewGuid().ToString().Substring(0, 5), null, GameRules.Standard, false);
                            break;
                        case ConsoleKey.R:
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

        private static void DisplayGameList()
        {
            Console.WriteLine("Games: {0}", _client.Games.Count());
            foreach (GameData game in _client.Games)
            {
                Console.WriteLine("Game: {0} {1} {2}", game.Id, game.Name, game.Rule);
                Console.WriteLine("\tClients: {0}", game.Clients == null ? 0 : game.Clients.Count);
                if (game.Clients != null)
                    foreach (ClientData client in game.Clients)
                        Console.WriteLine("Client: {0} {1} {2} {3} {4} {5} ", client.Id, client.Name, client.IsGameMaster, client.GameId, client.IsPlayer, client.IsSpectator);
            }
        }

        private static void OnContinuousSpecialFinished(ClientData client, Specials special)
        {
            Console.WriteLine("OnContinuousSpecialFinished: {0} {1}", client == null ? Guid.Empty : client.Id, special);
        }

        private static void OnGridModified(ClientData client, byte[] grid)
        {
            Console.WriteLine("OnGridModified: {0}", client == null ? Guid.Empty : client.Id);
        }

        private static void OnSpecialUsed(ClientData client, ClientData target, int specialId, Specials special)
        {
            Console.WriteLine("OnSpecialUsed: {0} {1} {2} {3}", client == null ? Guid.Empty : client.Id, target == null ? Guid.Empty : target.Id, specialId, special);
        }

        private static void OnPlayerLinesAdded(ClientData client, int specialId, int count)
        {
            Console.WriteLine("OnPlayerLinesAdded: {0} {1} {2}", client == null ? Guid.Empty : client.Id, specialId, count);
        }

        private static void OnServerLinesAdded(int count)
        {
            Console.WriteLine("OnServerLinesAdded: {0}", count);
        }

        private static void OnPlayerLost(ClientData client)
        {
            Console.WriteLine("OnPlayerLost: {0}", client == null ? Guid.Empty : client.Id);
        }

        private static void OnPlayerWon(ClientData client)
        {
            Console.WriteLine("OnPlayerWon: {0}", client == null ? Guid.Empty : client.Id);
        }

        private static void OnPiecePlaced(int firstIndex, List<Pieces> nextPieces)
        {
            Console.WriteLine("OnPiecePlaced: {0} {1}", firstIndex, nextPieces == null ? 0 : nextPieces.Count);
        }

        private static void OnAchievementEarned(ClientData client, int achievementId, string achievementTitle)
        {
            Console.WriteLine("OnAchievementEarned: {0} {1} {2}", client == null ? Guid.Empty : client.Id, achievementId, achievementTitle);
        }

        private static void OnVoteKickAsked(ClientData sourceClient, ClientData targetClient, string reason)
        {
            Console.WriteLine("OnVoteKickAsked: {0} {1} {2}", sourceClient == null ? Guid.Empty : sourceClient.Id, targetClient == null ? Guid.Empty : targetClient.Id, reason);
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

        private static void OnGameStarted()
        {
            Console.WriteLine("OnGameStarted");
        }

        private static void OnGameMasterModified(ClientData client)
        {
            Console.WriteLine("OnGameMasterModified: {0}", client == null ? Guid.Empty : client.Id);
        }

        private static void OnClientGameLeft(ClientData client)
        {
            Console.WriteLine("OnClientGameLeft: {0}", client == null ? Guid.Empty : client.Id);
        }

        private static void OnClientGameJoined(ClientData client, bool asSpectator)
        {
            Console.WriteLine("OnClientGameJoined: {0} {1}", client == null ? Guid.Empty : client.Id, asSpectator);
        }

        private static void OnGameLeft()
        {
            Console.WriteLine("OnGameLeft");
        }

        private static void OnGameJoined(GameJoinResults result, GameData game, bool isGameMaster)
        {
            Console.WriteLine("OnGameJoined: {0} {1} {2}", result, game == null ? Guid.Empty : game.Id, isGameMaster);
        }

        private static void OnGameCreated(GameCreateResults result, GameData game)
        {
            Console.WriteLine("OnGameCreated: {0} {1} {2} {3}", result, game.Id, game.Name, game.Rule);
        }

        private static void OnTeamChanged(ClientData client, string team)
        {
            Console.WriteLine("OnTeamChanged: {0} {1}", client == null ? Guid.Empty : client.Id, team);
        }

        private static void OnPrivateMessageReceived(ClientData client, string message)
        {
            Console.WriteLine("OnPrivateMessageReceived: {0} {1}", client == null ? Guid.Empty : client.Id, message);
        }

        private static void OnBroadcastMessageReceived(ClientData client, string message)
        {
            Console.WriteLine("OnBroadcastMessageReceived: {0} {1}", client == null ? Guid.Empty : client.Id, message);
        }

        private static void OnServerMessageReceived(string message)
        {
            Console.WriteLine("OnServerMessageReceived: {0}", message);
        }

        private static void OnServerGameDeleted(GameData game)
        {
            Console.WriteLine("OnGameDeleted: {0}", game == null ? Guid.Empty : game.Id);

            DisplayGameList();
        }

        private static void OnServerGameCreated(GameData game)
        {
            Console.WriteLine("OnServerGameCreated: {0} {1} {2}", game.Id, game.Name, game.Rule);
            if (game.Clients != null)
            {
                Console.WriteLine("\tClients: {0}", game.Clients.Count);
                foreach (ClientData client in game.Clients)
                    Console.WriteLine("Client: {0} {1} {2} {3} {4} {5}", client.Id, client.Name, client.GameId, client.IsGameMaster, client.IsPlayer, client.IsSpectator);
            }

            DisplayGameList();
        }

        private static void OnClientGameCreated(ClientData client, GameData game)
        {
            Console.WriteLine("OnClientGameCreated: {0} {1} {2} {3}", client == null ? Guid.Empty : client.Id, game.Id, game.Name, game.Rule);
            if (game.Clients != null)
            {
                Console.WriteLine("\tClients: {0}", game.Clients.Count);
                foreach (ClientData gameClient in game.Clients)
                    Console.WriteLine("\tClient: {0} {1} {2} {3} {4} {5}", gameClient.Id, gameClient.Name, gameClient.GameId, gameClient.IsGameMaster, gameClient.IsPlayer, gameClient.IsSpectator);
            }

            DisplayGameList();
        }

        private static void OnClientDisconnected(ClientData client, LeaveReasons reason)
        {
            Console.WriteLine("OnClientDisconnected: {0} {1}", client == null ? Guid.Empty : client.Id, reason);

            DisplayClientList();
        }

        private static void OnClientConnected(ClientData client, string name, string team)
        {
            Console.WriteLine("OnClientConnected: {0} {1} {2}", client == null ? Guid.Empty : client.Id, name, team);

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

        private static void OnGameListReceived(List<GameData> games)
        {
            Console.WriteLine("Games: {0}", games == null ? 0 : games.Count);
            if (games != null)
                foreach (GameData game in games)
                {
                    Console.WriteLine("Game: {0} {1} {2}", game.Id, game.Name, game.Rule);
                    Console.WriteLine("\tClients: {0}", game.Clients == null ? 0 : game.Clients.Count);
                    if (game.Clients != null)
                        foreach (ClientData client in game.Clients)
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

        private static void OnConnected(ConnectResults result, Versioning serverVersion, ClientData client, List<GameData> games)
        {
            Console.WriteLine("OnConnected: {0} {1}.{2} {3}", result, serverVersion == null ? -1 : serverVersion.Major, serverVersion == null ? -1 : serverVersion.Minor, client.Id);
            Console.WriteLine("Game list: {0}", games == null ? 0 : games.Count);
            if (games != null)
                foreach (GameData game in games)
                {
                    Console.WriteLine("Game: {0} {1} {2}", game.Id, game.Name, game.Rule);
                    Console.WriteLine("\tClients: {0}", game.Clients == null ? 0 : game.Clients.Count);
                    if (game.Clients != null)
                        foreach (ClientData gameClient in game.Clients)
                            Console.WriteLine("\tClient: {0} {1} {2} {3} {4} {5}", gameClient.Id, gameClient.Name, gameClient.GameId, gameClient.IsGameMaster, gameClient.IsPlayer, gameClient.IsSpectator);
                }
            }
        }
}
