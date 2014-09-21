﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TetriNET2.Common.ActionQueue;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Logger;
using TetriNET2.Common.Randomizer;
using TetriNET2.Server.Interfaces;
using TetriNET2.Server.Interfaces.IHost;

namespace TetriNET2.Server.ConsoleApp
{
    internal class Program
    {
        static void DisplayHelp()
        {
            Console.WriteLine("Commands:");
            Console.WriteLine("x: Stop server");
            Console.WriteLine("s: Start game");
            Console.WriteLine("t: Stop game");
            Console.WriteLine("p: Pause game");
            Console.WriteLine("r: Resume game");
            Console.WriteLine("+: Add dummy player");
            Console.WriteLine("-: Remove dummy player");
            Console.WriteLine("d: Dump client list");
        }

        private static void Main(string[] args)
        {
            Log.Default.Logger = new NLogger();
            Log.Default.Initialize(@"D:\TEMP\LOGS\", "TETRINET2_SERVER.LOG");

            IFactory factory = new Factory();
            IBanManager banManager = new BanManager(@"D:\TEMP\ban.lst");
            IClientManager clientManager = new ClientManager(50);
            IAdminManager adminManager = new AdminManager(5);
            IGameRoomManager gameRoomManager = new GameRoomManager(10);

            IHost dummyHost = new DummyHost(clientManager, adminManager, gameRoomManager);
            List<DummyClient> dummyClients = new List<DummyClient>();


            IServer server = new Server(factory, banManager, clientManager, adminManager, gameRoomManager);
            server.AddHost(dummyHost);
            server.PerformRestartServer += ServerOnPerformRestartServer;

            //
            try
            {
                server.Start();
            }
            catch (Exception ex)
            {
                Log.Default.WriteLine(LogLevels.Error, "Cannot start server. Exception: {0}", ex);
                return;
            }

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
                            server.Stop();
                            stopped = true;
                            break;
                        case ConsoleKey.S:
                            //
                            break;
                        case ConsoleKey.T:
                            //
                            break;
                        case ConsoleKey.P:
                            //
                            break;
                        case ConsoleKey.R:
                            //
                            break;
                        case ConsoleKey.Add:
                        {
                            DummyClient dummyClient = new DummyClient(dummyHost, "BuiltIn-" + Guid.NewGuid().ToString().Substring(0, 5), "DUMMY", new Versioning { Major = 1, Minor = 0}, IPAddress.Any);
                            dummyHost.ClientConnect(dummyClient, dummyClient.Versioning, dummyClient.Name, dummyClient.Team);
                            dummyClients.Add(dummyClient);
                            break;
                        }
                        case ConsoleKey.Subtract:
                        {
                            IClient client = clientManager.Clients.LastOrDefault(x => x.Name.Contains("BuiltIn-"));
                            if (client != null)
                            {
                                DummyClient dummyClient = dummyClients.FirstOrDefault(x => x == client.Callback);
                                if (dummyClient != null)
                                {
                                    dummyClient.ClientDisconnect();
                                    dummyClients.Remove(dummyClient);
                                }
                            }
                            break;
                        }
                        case ConsoleKey.D:
                            Console.WriteLine("Clients:");
                            foreach (IClient client in clientManager.Clients)
                                Console.WriteLine("{0}) {1} [{2}] {3} {4} {5:HH:mm:ss.fff} {6:HH:mm:ss.fff}", client.Id, client.Name, client.Team, client.State, client.PieceIndex, client.LastActionFromClient, client.LastActionToClient);
                            Console.WriteLine("Admin:");
                            foreach (IAdmin admin in adminManager.Admins)
                                Console.WriteLine("{0}) {1}", admin.Id, admin.Name);
                            break;
                    }
                }
                else
                    System.Threading.Thread.Sleep(100);
            }
        }

        private static void ServerOnPerformRestartServer(IServer server)
        {
            // TODO
            server.Stop();
        }
    }

    public class Factory : IFactory
    {
        public IClient CreateClient(string name, string team, IPAddress address, ITetriNETCallback callback)
        {
            return new Client(name, address, callback, team);
        }

        public IAdmin CreateAdmin(string name, IPAddress address, ITetriNETAdminCallback callback)
        {
            return new Admin(name, address, callback);
        }

        public IGameRoom CreateGameRoom(string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password)
        {
            return new GameRoom(new BlockingActionQueue(), new PieceBag(RangeRandom.Random, 4), name, maxPlayers, maxSpectators, rule, options, password);
        }
    }
}
