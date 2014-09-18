using System;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Logger;
using TetriNET2.Server;
using TetriNET2.Server.Interfaces;
using TetriNET2.Tests.Server.ClientSide;
using TetriNET2.Tests.Server.Helpers;
using TetriNET2.Tests.Server.Mocking;

namespace TetriNET2.Tests.Server
{
    [TestClass]
    public abstract class AbstractServerUnitTest
    {
        protected IBanManager BanManager;
        protected IClientManager ClientManager;
        protected IAdminManager AdminManager;
        protected IGameRoomManager GameRoomManager;

        protected abstract IServer CreateServer();
        protected abstract HostMock CreateHost();
        protected abstract IGameRoom CreateGameRoom(string name);
        protected abstract ClientFake CreateClientFake(IHost host, string name, Versioning version, IPAddress address = null, string team = null);
        protected abstract AdminFake CreateAdminFake(IHost host, string name, Versioning version);

        [TestInitialize]
        public void Initialize()
        {
            Log.Default.Logger = new LogMock();
        }

        #region AddHost

        [TestCategory("Server")]
        [TestCategory("Server.IServer")]
        [TestCategory("Server.IServer.AddHost")]
        [TestMethod]
        public void TestAddHostNullHost()
        {
            IServer server = CreateServer();

            try
            {
                server.AddHost(null);

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("host", ex.ParamName);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.IServer")]
        [TestCategory("Server.IServer.AddHost")]
        [TestMethod]
        public void TestAddHostNoDuplicate()
        {
            IServer server = CreateServer();
            IHost host = CreateHost();
            server.AddHost(host);

            bool succeed = server.AddHost(host);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IServer")]
        [TestCategory("Server.IServer.AddHost")]
        [TestMethod]
        public void TestAddHostFailedIfServerStarted()
        {
            IServer server = CreateServer();
            server.SetVersion(1, 1);
            server.AddHost(CreateHost()); // need to be able to start
            server.Start();

            bool succeed = server.AddHost(CreateHost());

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IServer")]
        [TestCategory("Server.IServer.AddHost")]
        [TestMethod]
        public void TestAddHostOk()
        {
            IServer server = CreateServer();

            bool succeed = server.AddHost(CreateHost());

            Assert.IsTrue(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IServer")]
        [TestCategory("Server.IServer.AddHost")]
        [TestMethod]
        public void TestAddHostEventsHandled()
        {
            IServer server = CreateServer();
            IHost host = CreateHost();
            
            server.AddHost(host);

            Assert.IsTrue(EventChecker.CheckEvents(host));
        }

        #endregion

        #region SetVersion

        [TestCategory("Server")]
        [TestCategory("Server.IServer")]
        [TestCategory("Server.IServer.SetVersion")]
        [TestMethod]
        public void TestSetVersionFailedIfServerStarted()
        {
            IServer server = CreateServer();
            server.SetVersion(1, 1);
            server.AddHost(CreateHost());
            server.Start();

            bool succeed = server.SetVersion(10, 20);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IServer")]
        [TestCategory("Server.IServer.SetVersion")]
        [TestMethod]
        public void TestSetVersionIsVersionSet()
        {
            IServer server = CreateServer();

            bool succeed = server.SetVersion(10, 20);

            Assert.IsTrue(succeed);
            Assert.AreEqual(10, server.Version.Major);
            Assert.AreEqual(20, server.Version.Minor);
        }

        #endregion

        #region Start

        [TestCategory("Server")]
        [TestCategory("Server.IServer")]
        [TestCategory("Server.IServer.Start")]
        [TestMethod]
        public void TestStartFailedIfNoVersion()
        {
            IServer server = CreateServer();
            server.AddHost(CreateHost());

            bool succeed = server.Start();

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IServer")]
        [TestCategory("Server.IServer.Start")]
        [TestMethod]
        public void TestStartFailedIfNoHost()
        {
            IServer server = CreateServer();
            server.SetVersion(1, 1);

            bool succeed = server.Start();

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IServer")]
        [TestCategory("Server.IServer.Start")]
        [TestMethod]
        public void TestStartFailedIfAlreadyStarted()
        {
            IServer server = CreateServer();
            server.SetVersion(1, 1);
            server.AddHost(CreateHost());
            server.Start();

            bool succeed = server.Start();

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IServer")]
        [TestCategory("Server.IServer.Start")]
        [TestMethod]
        public void TestStartOk()
        {
            IServer server = CreateServer();
            server.SetVersion(1, 1);
            server.AddHost(CreateHost());

            bool succeed = server.Start();

            Assert.IsTrue(succeed);
            Assert.AreEqual(ServerStates.Started, server.State);
        }

        #endregion

        #region Stop

        [TestCategory("Server")]
        [TestCategory("Server.IServer")]
        [TestCategory("Server.IServer.Stop")]
        [TestMethod]
        public void TestStopFailedIfNotStarted()
        {
            IServer server = CreateServer();

            bool succeed = server.Stop();

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IServer")]
        [TestCategory("Server.IServer.Stop")]
        [TestMethod]
        public void TestStopOkIfStarted()
        {
            IServer server = CreateServer();
            server.SetVersion(1, 1);
            server.AddHost(CreateHost());
            server.Start();

            bool succeed = server.Stop();

            Assert.IsTrue(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IServer")]
        [TestCategory("Server.IServer.Stop")]
        [TestMethod]
        public void TestStopCallbackCalled()
        {
            IServer server = CreateServer();
            IHost host = CreateHost();
            IGameRoom gameRoom = CreateGameRoom("room1");
            gameRoom.Start(new CancellationTokenSource());
            GameRoomManager.Add(gameRoom);
            server.SetVersion(1, 1);
            server.AddHost(host);
            server.Start();
            ClientFake clientFake1 = CreateClientFake(host, "client1", server.Version);
            ClientFake clientFake2 = CreateClientFake(host, "client2", server.Version);
            AdminFake adminFake1 = CreateAdminFake(host, "admin1", server.Version);
            AdminFake adminFake2 = CreateAdminFake(host, "admin2", server.Version);
            clientFake1.ClientConnect();
            clientFake2.ClientConnect();
            adminFake1.AdminConnect(null);
            adminFake2.AdminConnect(null);

            bool succeed = server.Stop();

            Assert.IsTrue(succeed);
            Assert.AreEqual(ServerStates.Waiting, server.State);
            Assert.AreEqual(GameRoomStates.Created, gameRoom.State);
            Assert.AreEqual(0, GameRoomManager.RoomCount);
            Assert.AreEqual(0, AdminManager.AdminCount);
            Assert.AreEqual(0, ClientManager.ClientCount);
            Assert.AreEqual(1, clientFake1.GetCallCount("OnServerStopped"));
            Assert.AreEqual(1, clientFake2.GetCallCount("OnServerStopped"));
            Assert.AreEqual(1, adminFake1.GetCallCount("OnServerStopped"));
            Assert.AreEqual(1, adminFake2.GetCallCount("OnServerStopped"));
        }

        #endregion

        #region ClientConnect

        [TestCategory("Server")]
        [TestCategory("Server.IServer")]
        [TestCategory("Server.IServer.ClientConnect")]
        [TestMethod]
        public void TestClientConnectOk()
        {
            IServer server = CreateServer();
            IHost host = CreateHost();
            IGameRoom gameRoom = CreateGameRoom("room1");
            gameRoom.Start(new CancellationTokenSource());
            GameRoomManager.Add(gameRoom);
            server.SetVersion(1, 1);
            server.AddHost(host);
            server.Start();

            ClientFake clientFake1 = CreateClientFake(host, "client1", server.Version);
            clientFake1.ClientConnect();

            Assert.AreEqual(1, clientFake1.GetCallCount("OnConnected"));
            Assert.AreEqual(ConnectResults.Successfull, clientFake1.GetCallParameters("OnConnected", 0)[0]);
            Assert.AreEqual(1, ClientManager.ClientCount);
            Assert.IsNotNull(ClientManager.Clients.FirstOrDefault());
        }

        [TestCategory("Server")]
        [TestCategory("Server.IServer")]
        [TestCategory("Server.IServer.ClientConnect")]
        [TestMethod]
        public void TestClientConnectOkOtherInformed()
        {
            IServer server = CreateServer();
            IHost host = CreateHost();
            IGameRoom gameRoom = CreateGameRoom("room1");
            gameRoom.Start(new CancellationTokenSource());
            GameRoomManager.Add(gameRoom);
            server.SetVersion(1, 1);
            server.AddHost(host);
            server.Start();
            ClientFake clientFake1 = CreateClientFake(host, "client1", server.Version);
            AdminFake adminFake1 = CreateAdminFake(host, "admin1", server.Version);
            AdminFake adminFake2 = CreateAdminFake(host, "admin2", server.Version);
            clientFake1.ClientConnect();
            adminFake1.AdminConnect(null);
            adminFake2.AdminConnect(null);
            clientFake1.ResetCallInfo();
            adminFake1.ResetCallInfo();
            adminFake2.ResetCallInfo();

            ClientFake clientFake2 = CreateClientFake(host, "client2", server.Version);
            clientFake2.ClientConnect();

            Assert.AreEqual(1, clientFake1.GetCallCount("OnClientConnected"));
            Assert.AreEqual(1, adminFake1.GetCallCount("OnClientConnected"));
            Assert.AreEqual(1, adminFake2.GetCallCount("OnClientConnected"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IServer")]
        [TestCategory("Server.IServer.ClientConnect")]
        [TestMethod]
        public void TestClientConnectFailedIncompatibleVersion()
        {
            IServer server = CreateServer();
            IHost host = CreateHost();
            IGameRoom gameRoom = CreateGameRoom("room1");
            gameRoom.Start(new CancellationTokenSource());
            GameRoomManager.Add(gameRoom);
            server.SetVersion(1, 1);
            server.AddHost(host);
            server.Start();

            ClientFake clientFake1 = CreateClientFake(host, "client1", new Versioning {Major = 1, Minor = 2});
            clientFake1.ClientConnect();

            Assert.AreEqual(1, clientFake1.GetCallCount("OnConnected"));
            Assert.AreEqual(ConnectResults.FailedIncompatibleVersion, clientFake1.GetCallParameters("OnConnected", 0)[0]);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IServer")]
        [TestCategory("Server.IServer.ClientConnect")]
        [TestMethod]
        public void TestClientConnectFailedIfDuplicateName()
        {
            IServer server = CreateServer();
            IHost host = CreateHost();
            IGameRoom gameRoom = CreateGameRoom("room1");
            gameRoom.Start(new CancellationTokenSource());
            GameRoomManager.Add(gameRoom);
            server.SetVersion(1, 1);
            server.AddHost(host);
            server.Start();
            ClientFake clientFake1 = CreateClientFake(host, "client1", server.Version);
            clientFake1.ClientConnect();

            ClientFake clientFake2 = CreateClientFake(host, "client1", server.Version);
            clientFake2.ClientConnect();

            Assert.AreEqual(1, clientFake2.GetCallCount("OnConnected"));
            Assert.AreEqual(ConnectResults.FailedClientAlreadyExists, clientFake2.GetCallParameters("OnConnected", 0)[0]);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IServer")]
        [TestCategory("Server.IServer.ClientConnect")]
        [TestMethod]
        public void TestClientConnectFailedInvalidName()
        {
            IServer server = CreateServer();
            IHost host = CreateHost();
            IGameRoom gameRoom = CreateGameRoom("room1");
            gameRoom.Start(new CancellationTokenSource());
            GameRoomManager.Add(gameRoom);
            server.SetVersion(1, 1);
            server.AddHost(host);
            server.Start();

            ClientFake clientFake1 = CreateClientFake(host, "012345678901234567890123456789", server.Version);
            clientFake1.ClientConnect();

            Assert.AreEqual(1, clientFake1.GetCallCount("OnConnected"));
            Assert.AreEqual(ConnectResults.FailedInvalidName, clientFake1.GetCallParameters("OnConnected", 0)[0]);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IServer")]
        [TestCategory("Server.IServer.ClientConnect")]
        [TestMethod]
        public void TestClientConnectFailedIfBanned()
        {
            IServer server = CreateServer();
            IHost host = CreateHost();
            IGameRoom gameRoom = CreateGameRoom("room1");
            gameRoom.Start(new CancellationTokenSource());
            GameRoomManager.Add(gameRoom);
            server.SetVersion(1, 1);
            server.AddHost(host);
            server.Start();
            BanManager.Ban("client1", IPAddress.Parse("127.0.0.2"), "test");

            ClientFake clientFake1 = CreateClientFake(host, "client1", server.Version, IPAddress.Parse("127.0.0.2"));
            clientFake1.ClientConnect();

            Assert.AreEqual(1, clientFake1.GetCallCount("OnConnected"));
            Assert.AreEqual(ConnectResults.FailedBanned, clientFake1.GetCallParameters("OnConnected", 0)[0]);
        }

        // TODO: check maxClients

        #endregion

        // TODO: test remaining IHost event handlers
    }

    [TestClass]
    public class ServerUnitTest : AbstractServerUnitTest
    {
        private class Factory : IFactory
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
                return new GameRoom(new ActionQueueMock(), new PieceProviderMock(), name, maxPlayers, maxSpectators, rule, options, password);
            }
        }

        protected override IServer CreateServer()
        {
            BanManager = new BanManager(@"d:\temp\banmanagerunittest.lst");
            ClientManager = new ClientManager(10);
            AdminManager = new AdminManager(10);
            GameRoomManager = new GameRoomManager(10);
            return new TetriNET2.Server.Server(new Factory(), BanManager, ClientManager, AdminManager, GameRoomManager);
        }

        protected override HostMock CreateHost()
        {
            return new HostMock(ClientManager, AdminManager, GameRoomManager);
        }

        protected override ClientFake CreateClientFake(IHost host, string name, Versioning version, IPAddress address = null, string team = null)
        {
            ClientFake client = new ClientFake(name, team, version, address ?? IPAddress.Any)
            {
                Host = host
            };
            return client;
        }

        protected override AdminFake CreateAdminFake(IHost host, string name, Versioning version)
        {
            AdminFake admin = new AdminFake(name, version, IPAddress.Any)
            {
                Host = host
            };
            return admin;
        }

        protected override IGameRoom CreateGameRoom(string name)
        {
            GameOptions options = new GameOptions();
            options.Initialize(GameRules.Standard);
            return new GameRoom(new ActionQueueMock(), new PieceProviderMock(), name, 10, 10, GameRules.Standard, options);
        }

        #region Constructors

        [TestCategory("Server")]
        [TestCategory("Server.Server")]
        [TestCategory("Server.Server.ctor")]
        [TestMethod]
        public void TestNullFactory()
        {
            try
            {
                IServer server = new TetriNET2.Server.Server(null, new BanManager(@"d:\temp\banmanagerunittest.lst"), new ClientManager(10), new AdminManager(10), new GameRoomManager(10));

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("factory", ex.ParamName);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.Server")]
        [TestCategory("Server.Server.ctor")]
        [TestMethod]
        public void TestNullBanManager()
        {
            try
            {
                IServer server = new TetriNET2.Server.Server(new Factory(), null, new ClientManager(10), new AdminManager(10), new GameRoomManager(10));

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("banManager", ex.ParamName);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.Server")]
        [TestCategory("Server.Server.ctor")]
        [TestMethod]
        public void TestNullClientManager()
        {
            try
            {
                IServer server = new TetriNET2.Server.Server(new Factory(), new BanManager(@"d:\temp\banmanagerunittest.lst"), null, new AdminManager(10), new GameRoomManager(10));

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("clientManager", ex.ParamName);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.Server")]
        [TestCategory("Server.Server.ctor")]
        [TestMethod]
        public void TestNullAdminManager()
        {
            try
            {
                IServer server = new TetriNET2.Server.Server(new Factory(), new BanManager(@"d:\temp\banmanagerunittest.lst"), new ClientManager(10), null, new GameRoomManager(10));

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("adminManager", ex.ParamName);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.Server")]
        [TestCategory("Server.Server.ctor")]
        [TestMethod]
        public void TestNullGameRoomManager()
        {
            try
            {
                IServer server = new TetriNET2.Server.Server(new Factory(), new BanManager(@"d:\temp\banmanagerunittest.lst"), new ClientManager(10), new AdminManager(10), null);

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("gameRoomManager", ex.ParamName);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.Server")]
        [TestCategory("Server.Server.ctor")]
        [TestMethod]
        public void TestConstructorSetProperties()
        {
            IServer server = new TetriNET2.Server.Server(new Factory(), new BanManager(@"d:\temp\banmanagerunittest.lst"), new ClientManager(10), new AdminManager(10), new GameRoomManager(10));

            Assert.AreEqual(ServerStates.Waiting, server.State);
        }

        #endregion
    }
}
