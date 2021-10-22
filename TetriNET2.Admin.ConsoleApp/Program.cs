using System;
using TetriNET2.Admin.ConsoleApp.UI;
using TetriNET2.Admin.Interfaces;
using TetriNET2.Common.Contracts;
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
            Console.WriteLine("z: Disconnect");
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

            ConsoleUI ui = new ConsoleUI(_admin);

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
                            _admin.Connect("net.tcp://localhost:7788/TetriNET2Admin", "admin1", "123456");
                            break;
                        case ConsoleKey.Z:
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

        public class Factory : IFactory
        {
            public IProxy CreateProxy(ITetriNETAdminCallback callback, string address)
            {
                return new WCFProxy.WCFProxy(callback, address);
            }
        }
    }
}
