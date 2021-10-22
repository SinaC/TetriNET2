using System;
using TetriNET2.Client.ConsoleApp.UI;
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

        private static void DisplayHelp()
        {
            Console.WriteLine("Commands:");
            Console.WriteLine("x: Stop client");
            Console.WriteLine("o: Connect");
            Console.WriteLine("z: Disconnect");
            Console.WriteLine("g: Get game list");
            Console.WriteLine("c: Get client list");
            Console.WriteLine("l: Get game client list");
            Console.WriteLine("j: Create and join game as player");
            Console.WriteLine("r: Join random game");
            Console.WriteLine("w: Leave game");
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
                return new WCFProxy.WCFProxy(callback, address);
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
            string clientName = "Console" + Guid.NewGuid().ToString().Substring(0, 5);

            Log.Default.Logger = new NLogger();
            Log.Default.Initialize(@"D:\TEMP\LOG\", $"TETRINET2_CLIENT_{clientName}.LOG");

            IFactory factory = new Factory();

            _client = new Client(factory);
            _client.SetVersion(1, 0);

            ConsoleUI ui = new ConsoleUI(_client);
            IGameController controller = new GameController.GameController(_client);

            //_client.ConnectionLost += OnConnectionLost;

            _client.Connect("net.tcp://localhost:7788/TetriNET2Client", clientName, "team1");

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
                            _client.Connect("net.tcp://localhost:7788/TetriNET2Client", clientName, "team1");
                            break;
                        case ConsoleKey.Z:
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
                            _client.CreateAndJoinGame("GAME" + Guid.NewGuid().ToString().Substring(0, 5), null, GameRules.Standard, false);
                            break;
                        case ConsoleKey.R:
                            _client.JoinRandomGame(false);
                            break;
                        case ConsoleKey.W:
                            _client.LeaveGame();
                            break;
                        case ConsoleKey.S:
                            _client.StartGame();
                            break;
                        case ConsoleKey.T:
                            _client.StopGame();
                            break;

                        // Game controller
                        case ConsoleKey.LeftArrow:
                            controller.KeyDown(Commands.Left);
                            controller.KeyUp(Commands.Left);
                            break;
                        case ConsoleKey.RightArrow:
                            controller.KeyDown(Commands.Right);
                            controller.KeyUp(Commands.Right);
                            break;
                        case ConsoleKey.DownArrow:
                            controller.KeyDown(Commands.Down);
                            controller.KeyUp(Commands.Down);
                            break;
                        case ConsoleKey.H:
                            controller.KeyDown(Commands.Hold);
                            controller.KeyUp(Commands.Hold);
                            break;
                        case ConsoleKey.Spacebar:
                            controller.KeyDown(Commands.Drop);
                            controller.KeyUp(Commands.Drop);
                            break;
                        case ConsoleKey.UpArrow:
                            controller.KeyDown(Commands.RotateClockwise);
                            controller.KeyUp(Commands.RotateClockwise);
                            break;
                        case ConsoleKey.D:
                            controller.KeyDown(Commands.DiscardFirstSpecial);
                            controller.KeyUp(Commands.DiscardFirstSpecial);
                            break;
                        case ConsoleKey.NumPad1:
                        case ConsoleKey.D1:
                            controller.KeyDown(Commands.UseSpecialOn1);
                            controller.KeyUp(Commands.UseSpecialOn1);
                            break;
                        case ConsoleKey.NumPad2:
                        case ConsoleKey.D2:
                            controller.KeyDown(Commands.UseSpecialOn2);
                            controller.KeyUp(Commands.UseSpecialOn2);
                            break;
                        case ConsoleKey.NumPad3:
                        case ConsoleKey.D3:
                            controller.KeyDown(Commands.UseSpecialOn3);
                            controller.KeyUp(Commands.UseSpecialOn3);
                            break;
                        case ConsoleKey.NumPad4:
                        case ConsoleKey.D4:
                            controller.KeyDown(Commands.UseSpecialOn4);
                            controller.KeyUp(Commands.UseSpecialOn4);
                            break;
                        case ConsoleKey.NumPad5:
                        case ConsoleKey.D5:
                            controller.KeyDown(Commands.UseSpecialOn5);
                            controller.KeyUp(Commands.UseSpecialOn5);
                            break;
                        case ConsoleKey.NumPad6:
                        case ConsoleKey.D6:
                            controller.KeyDown(Commands.UseSpecialOn6);
                            controller.KeyUp(Commands.UseSpecialOn6);
                            break;
                        case ConsoleKey.Enter:
                            controller.KeyDown(Commands.UseSpecialOnSelf);
                            controller.KeyUp(Commands.UseSpecialOnSelf);
                            break;
                        case ConsoleKey.Tab:
                            controller.KeyDown(Commands.UseSpecialOnRandomOpponent);
                            controller.KeyUp(Commands.UseSpecialOnRandomOpponent);
                            break;
                    }
                }
                else
                    System.Threading.Thread.Sleep(250);
            }
        }
    }
}
