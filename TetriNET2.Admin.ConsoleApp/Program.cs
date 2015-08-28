using System;
using System.Collections.Generic;
using System.Linq;
using TetriNET2.Admin.Interfaces;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Logger;

namespace TetriNET2.Admin.ConsoleApp
{
    internal class Program
    {
        private static IAdmin _admin;

        private static void DisplayHelp()
        {
            Console.WriteLine("Commands:");
            Console.WriteLine("x: Stop admin");
            Console.WriteLine("o: Connect");
            Console.WriteLine("d: Disconnect");
            Console.WriteLine("a: Get admin list");
            Console.WriteLine("c: Get client list");
            Console.WriteLine("g: Get game list");
            Console.WriteLine("b: Get banned list");
            Console.WriteLine("s: Restart server");
            // TODO: 
            //  get client in game
            //  create/delete game
            //  kick/ban
        }

        private static void Main(string[] args)
        {
            Log.Default.Logger = new NLogger();
            Log.Default.Initialize(@"D:\TEMP\LOG\", "TETRINET2_ADMIN.LOG");

            IFactory factory = new Factory();

            _admin = new Admin(factory);
            _admin.SetVersion(1, 0);
            _admin.Connect(
                "net.tcp://localhost:7788/TetriNET2Admin",
                "admin1", "123456");

            //_admin.ConnectionLost += AdminOnConnectionLost;

            _admin.Connected += OnConnected;
            _admin.Disconnected += OnDisconnected;

            _admin.ServerStopped += OnServerStopped;

            _admin.ClientConnected += OnClientConnected;
            _admin.ClientDisconnected += OnClientDisconnected;

            _admin.AdminConnected += OnAdminConnected;
            _admin.AdminDisconnected += OnAdminDisconnected;

            _admin.GameCreated += OnGameCreated;
            _admin.GameDeleted += OnGameDeleted;

            _admin.ServerMessageReceived += OnServerMessageReceived;
            _admin.BroadcastMessageReceived += OnBroadcastMessageReceived;
            _admin.PrivateMessageReceived += OnPrivateMessageReceived;

            _admin.AdminListReceived += OnAdminListReceived;
            _admin.ClientListReceived += OnClientListReceived;
            _admin.ClientListInGameReceived += OnClientListInGameReceived;
            _admin.GameListReceived += OnGameListReceived;
            _admin.BannedListReceived += OnBannedListReceived;

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
                            _admin.Connect(
                                "net.tcp://localhost:7788/TetriNET2Admin",
                                "admin1", "123456");
                            break;
                        case ConsoleKey.D:
                            _admin.Disconnect();
                            break;
                        case ConsoleKey.X:
                            _admin.Disconnect();
                            stopped = true;
                            break;
                        case ConsoleKey.A:
                            _admin.GetAdminList();
                            break;
                        case ConsoleKey.C:
                            _admin.GetClientList();
                            break;
                        case ConsoleKey.G:
                            _admin.GetGameList();
                            break;
                        case ConsoleKey.B:
                            _admin.GetBannedList();
                            break;
                        case ConsoleKey.S:
                            _admin.RestartServer(30);
                            break;
                    }
                }
                else
                    System.Threading.Thread.Sleep(250);
            }
        }
        
        private static void DisplayAdminList()
        {
            Console.WriteLine("Admin list: {0}", _admin.Admins.Count());
            foreach (AdminData admin in _admin.Admins)
                Console.WriteLine("Admin: {0} {1} {2:dd-MM-yyyy HH:mm:ss.fff} {3}", admin.Id, admin.Name, admin.ConnectTime, admin.Address);
        }

        private static void DisplayClientList()
        {
            Console.WriteLine("Client list: {0}", _admin.Clients.Count());
            foreach (ClientAdminData client in _admin.Clients)
                Console.WriteLine("Client: {0} {1} {2} {3:HH:mm:ss.fff} {4} {5} {6}", client.Id, client.Name, client.Team, client.ConnectTime, client.Address, client.Roles, client.State);
        }

        private static void DisplayGameList()
        {
            Console.WriteLine("Games: {0}", _admin.Games.Count());
            foreach (GameAdminData game in _admin.Games)
            {
                Console.WriteLine("Game: {0} {1} {2}", game.Id, game.Name, game.Rule);
                Console.WriteLine("\tClients: {0}", game.Clients == null ? 0 : game.Clients.Count);
                if (game.Clients != null)
                    foreach (ClientAdminData client in game.Clients)
                        Console.WriteLine("Client: {0} {1} {2} {3:HH:mm:ss.fff} {4} {5} {6}", client.Id, client.Name, client.Team, client.ConnectTime, client.Address, client.Roles, client.State);
            }
        }

        private static void OnConnected(ConnectResults result, Versioning serverVersion, Guid adminId)
        {
            Console.WriteLine("OnConnected: {0} {1}.{2} {3}", result, serverVersion == null ? -1 : serverVersion.Major, serverVersion == null ? -1 : serverVersion.Minor, adminId);
        }

        private static void OnDisconnected()
        {
            Console.WriteLine("OnDisconnected");
        }

        private static void OnServerStopped()
        {
            Console.WriteLine("OnServerStopped");
        }

        private static void OnClientConnected(Guid clientId, string name, string team)
        {
            Console.WriteLine("OnClientConnected: {0} {1} {2}", clientId, name, team);

            DisplayClientList();
        }

        private static void OnClientDisconnected(Guid clientId, LeaveReasons reason)
        {
            Console.WriteLine("OnClientDisconnected: {0} {1}", clientId, reason);

            DisplayClientList();
        }

        private static void OnAdminConnected(Guid adminId, string name)
        {
            Console.WriteLine("OnAdminConnected: {0} {1}", adminId, name);

            DisplayAdminList();
        }

        private static void OnAdminDisconnected(Guid adminId, LeaveReasons reason)
        {
            Console.WriteLine("OnAdminDisconnected: {0} {1}", adminId, reason);

            DisplayAdminList();
        }

        private static void OnGameCreated(bool createdByClient, Guid clientOrAdminId, GameAdminData game)
        {
            Console.WriteLine("OnGameCreated: {0} {1} {2} {3} {4}", createdByClient, clientOrAdminId, game.Id, game.Name, game.Rule);
            if (game.Clients != null)
            {
                Console.WriteLine("\tClients: {0}", game.Clients.Count);
                foreach (ClientAdminData client in game.Clients)
                    Console.WriteLine("\tClient: {0} {1} {2} {3:HH:mm:ss.fff} {4} {5} {6}", client.Id, client.Name, client.Team, client.ConnectTime, client.Address, client.Roles, client.State);
            }

            DisplayGameList();
        }

        private static void OnGameDeleted(Guid adminId, Guid gameId)
        {
            Console.WriteLine("OnGameDeleted: {0} {1}", adminId, gameId);

            DisplayGameList();
        }

        private static void OnServerMessageReceived(string message)
        {
            Console.WriteLine("OnServerMessageReceived: {0}", message);
        }

        private static void OnBroadcastMessageReceived(Guid clientId, string message)
        {
            Console.WriteLine("OnBroadcastMessageReceived: {0} {1}", clientId, message);
        }

        private static void OnPrivateMessageReceived(Guid adminId, string message)
        {
            Console.WriteLine("OnPrivateMessageReceived: {0} {1}", adminId, message);
        }

        private static void OnAdminListReceived(List<AdminData> admins)
        {
            Console.WriteLine("OnAdminListReceived: {0}", admins == null ? 0 : admins.Count);
            if (admins != null)
                foreach (AdminData admin in admins)
                    Console.WriteLine("Admin: {0} {1} {2:HH:mm:ss.fff} {3}", admin.Id, admin.Name, admin.ConnectTime, admin.Address);

            DisplayAdminList();
        }

        private static void OnClientListReceived(List<ClientAdminData> clients)
        {
            Console.WriteLine("OnClientListReceived: {0}", clients == null ? 0 : clients.Count);
            if (clients != null)
                foreach (ClientAdminData client in clients)
                    Console.WriteLine("Client: {0} {1} {2} {3:HH:mm:ss.fff} {4} {5} {6}", client.Id, client.Name, client.Team, client.ConnectTime, client.Address, client.Roles, client.State);
        }

        private static void OnClientListInGameReceived(Guid gameId, List<ClientAdminData> clients)
        {
            Console.WriteLine("OnClientListInGameReceived {0}: {1}", gameId, clients == null ? 0 : clients.Count);
            if (clients != null)
                foreach (ClientAdminData client in clients)
                    Console.WriteLine("Client: {0} {1} {2} {3:HH:mm:ss.fff} {4} {5} {6}", client.Id, client.Name, client.Team, client.ConnectTime, client.Address, client.Roles, client.State);
        }

        private static void OnGameListReceived(List<GameAdminData> games)
        {
            Console.WriteLine("OnGameListReceived: {0}", games == null ? 0 : games.Count);
            if (games != null)
                foreach (GameAdminData game in games)
                {
                    Console.WriteLine("Game: {0} {1} {2} {3}", game.Id, game.Name, game.Rule, game.State);
                    Console.WriteLine("\tClients: {0}", game.Clients == null ? 0 : game.Clients.Count);
                    if (game.Clients != null)
                        foreach (ClientAdminData client in game.Clients)
                            Console.WriteLine("\tClient: {0} {1} {2} {3:HH:mm:ss.fff} {4} {5} {6}", client.Id, client.Name, client.Team, client.ConnectTime, client.Address, client.Roles, client.State);
                }
        }

        private static void OnBannedListReceived(List<BanEntryData> entries)
        {
            Console.WriteLine("OnBannedListReceived: {0}", entries == null ? 0 : entries.Count);
            if (entries != null)
                foreach (BanEntryData ban in entries)
                    Console.WriteLine("Ban: {0} {1} {2}", ban.Name, ban.Address, ban.Reason);
        }

        public class Factory : IFactory
        {
            public IProxy CreateProxy(ITetriNETAdminCallback callback, string address)
            {
                return new WCFProxy(callback, address);
            }
        }
    }
}
