using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Logger;
using TetriNET2.Server;
using TetriNET2.Server.Interfaces;
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
        protected abstract IHost CreateHost();

        // TODO

        [TestInitialize]
        public void Initialize()
        {
            Log.Default.Logger = new LogMock();
        }

        #region SetVersion
        
        [TestCategory("Server")]
        [TestCategory("Server.IServer")]
        [TestCategory("Server.IServer.SetVersion")]
        [TestMethod]
        public void TestSetVersionFailedIfServerStarted()
        {
            IServer server = CreateServer();
            server.SetVersion(1, 1);
            IHost host = CreateHost();
            server.AddHost(host);
            server.Start();

            bool succeed = server.SetVersion(10, 20);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IServer")]
        [TestCategory("Server.IServer.SetVersion")]
        [TestMethod]
        public void TestSetVersionOkVersionSet()
        {
            IServer server = CreateServer();
            
            bool succeed = server.SetVersion(10, 20);

            Assert.IsTrue(succeed);
            Assert.AreEqual(10, server.Version.Major);
            Assert.AreEqual(20, server.Version.Minor);
        }

        #endregion
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

        protected override IHost CreateHost()
        {
            return new HostMock(ClientManager, AdminManager, GameRoomManager);
        }
    }
}
