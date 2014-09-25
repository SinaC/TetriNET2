using System;
using System.Collections.Generic;
using TetriNET2.Admin.Interfaces;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Logger;

namespace TetriNET2.Admin.ConsoleApp
{
    class Program
    {
        static void DisplayHelp()
        {
            Console.WriteLine("Commands:");
            Console.WriteLine("x: Stop admin");
            Console.WriteLine("a: Get admin list");
            Console.WriteLine("c: Get client list");
            Console.WriteLine("r: Get room list");
            Console.WriteLine("b: Get banned list");
            Console.WriteLine("s: Restart server");
            // TODO: 
            //  get client in room
            //  kick/ban
        }

        static void Main(string[] args)
        {
            Log.Default.Logger = new NLogger();
            Log.Default.Initialize(@"D:\TEMP\LOG\", "TETRINET2_ADMIN.LOG");

            IFactory factory = new Factory();

            IAdmin admin = new Admin(factory);
            admin.SetVersion(1, 0);
            admin.Connect(
                "net.tcp://localhost:7788/TetriNET2Admin", 
                "admin1", "123456");

            admin.AdminOnConnected += OnConnected;
            admin.AdminOnDisconnected += OnDisconnected;
            admin.AdminOnGameCreated += OnGameCreated;
            admin.AdminOnClientConnected += OnClientConnected;
            admin.AdminOnAdminConnected += OnAdminConnected;
            admin.AdminOnAdminListReceived += OnAdminListReceived;
            admin.AdminOnClientListReceived += OnClientListReceived;
            admin.AdminOnClientListInRoomReceived += OnClientListInRoomReceived;
            admin.AdminOnRoomListReceived += OnRoomListReceived;
            admin.AdminOnBannedListReceived += OnBannedListReceived;

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
                            admin.Disconnect();
                            stopped = true;
                            break;
                        case ConsoleKey.A:
                            admin.GetAdminList();
                            break;
                        case ConsoleKey.C:
                            admin.GetClientList();
                            break;
                        case ConsoleKey.R:
                            admin.GetRoomList();
                            break;
                        case ConsoleKey.B:
                            admin.GetBannedList();
                            break;
                        case ConsoleKey.S:
                            admin.RestartServer(90);
                            break;
                    }
                }
                else
                    System.Threading.Thread.Sleep(250);
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

        private static void OnClientConnected(Guid clientId, string name, string team)
        {
            Console.WriteLine("OnClientConnected: {0} {1} {2}", clientId, name, team);
        }

        private static void OnAdminConnected(Guid adminId, string name)
        {
            Console.WriteLine("OnAdminConnected: {0} {1}", adminId, name);
        }

        private static void OnGameCreated(Guid clientId, GameDescription game)
        {
            Console.WriteLine("Game: {0} {1} {2}", game.Id, game.Name, game.Rule);
            if (game.Players != null)
            {
                Console.WriteLine("\tPlayers: {0}", game.Players.Count);
                foreach (string clientName in game.Players)
                    Console.WriteLine("\tPlayer: {0}", clientName);
            }
        }

        private static void OnAdminListReceived(List<AdminData> admins)
        {
            Console.WriteLine("Admins: {0}", admins == null ? 0 : admins.Count);
            if (admins != null)
                foreach (AdminData admin in admins)
                    Console.WriteLine("Admin: {0} {1} {2:dd-MM-yyyy HH:mm:ss.fff} {3}", admin.Id, admin.Name, admin.ConnectTime, admin.Address);
              
        }

        private static void OnClientListReceived(List<ClientAdminData> clients)
        {
            Console.WriteLine("Clients: {0}", clients == null ? 0 : clients.Count);
            if (clients != null)
                foreach (ClientAdminData client in clients)
                    Console.WriteLine("Client: {0} {1} {2:dd-MM-yyyy HH:mm:ss.fff} {3}", client.Id, client.Name, client.ConnectTime, client.Address);
        }

        private static void OnClientListInRoomReceived(Guid roomId, List<ClientAdminData> clients)
        {
            Console.WriteLine("Clients in room {0}: {1}", roomId, clients == null ? 0 : clients.Count);
            if (clients != null)
                foreach (ClientAdminData client in clients)
                    Console.WriteLine("Client: {0} {1} {2:dd-MM-yyyy HH:mm:ss.fff} {3}", client.Id, client.Name, client.ConnectTime, client.Address);
        }

        private static void OnRoomListReceived(List<GameRoomAdminData> rooms)
        {
            Console.WriteLine("Rooms: {0}", rooms == null ? 0 : rooms.Count);
            if (rooms != null)
                foreach (GameRoomAdminData room in rooms)
                {
                    Console.WriteLine("Room: {0} {1} {2}", room.Id, room.Name, room.Rule);
                    Console.WriteLine("\tClients: {0}", room.Clients == null ? 0 : room.Clients.Count);
                    if (room.Clients != null)
                        foreach (ClientAdminData client in room.Clients)
                            Console.WriteLine("\tClient: {0} {1} {2:dd-MM-yyyy HH:mm:ss.fff} {3}", client.Id, client.Name, client.ConnectTime, client.Address);
                }
        }

        private static void OnBannedListReceived(List<BanEntryData> entries)
        {
            Console.WriteLine("Banned: {0}", entries == null ? 0 : entries.Count);
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
