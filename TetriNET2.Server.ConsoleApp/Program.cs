using System;
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
            Console.WriteLine("d: Dump");
        }

        private static void Main()
        {
            Log.Default.Logger = new NLogger();
            Log.Default.Initialize(@"D:\TEMP\LOG\", "TETRINET2_SERVER.LOG");

            IFactory factory = new Factory();
            IPasswordManager passwordManager = new PasswordManager();
            IBanManager banManager = new BanManager(@"D:\TEMP\ban.lst");
            IClientManager clientManager = new ClientManager(50);
            IAdminManager adminManager = new AdminManager(5);
            IGameManager gameManager = new GameManager(10);

            IHost wcfHost = new WCFHost.WCFHost(banManager, clientManager, adminManager, gameManager)
                {
                    Port = 7788
                };

            IServer server = new Server(factory, passwordManager, banManager, clientManager, adminManager, gameManager);

            server.AddHost(wcfHost);

            server.SetVersion(1, 0);
            server.SetAdminPassword("admin1", "123456");

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
                        case ConsoleKey.D:
                            Console.WriteLine("Clients:");
                            foreach (IClient client in clientManager.Clients)
                                Console.WriteLine("{0}) {1} [{2}] {3} {4} {5} {6:HH:mm:ss.fff} {7:HH:mm:ss.fff}", client.Id, client.Name, client.Team, client.State, client.Game == null ? "no in game" : client.Game.Name, client.PieceIndex, client.LastActionFromClient, client.LastActionToClient);
                            Console.WriteLine("Admins:");
                            foreach (IAdmin admin in adminManager.Admins)
                                Console.WriteLine("{0}) {1}", admin.Id, admin.Name);
                            Console.WriteLine("Games:");
                            foreach (IGame game in gameManager.Games)
                                Console.WriteLine("{0}) {1} {2} {3} #players:{4} #spectators:{5}  password:{6} {7:HH:mm:ss}", game.Id, game.Name, game.State, game.Rule, game.PlayerCount, game.SpectatorCount, game.Password, game.CreationTime);
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
            Console.WriteLine("Waiting 5 seconds before restarting");
            System.Threading.Thread.Sleep(5000);
            server.Start();
        }
    }

    public class Factory : IFactory
    {
        public IClient CreateClient(string name, string team, IPAddress address, ITetriNETClientCallback callback)
        {
            return new Client(name, address, callback, team);
        }

        public IAdmin CreateAdmin(string name, IPAddress address, ITetriNETAdminCallback callback)
        {
            return new Admin(name, address, callback);
        }

        public IGame CreateGame(string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password)
        {
            return new Game(new BlockingActionQueue(), new PieceBag(RangeRandom.Random, 4), name, maxPlayers, maxSpectators, rule, options, password);
        }
    }
}
