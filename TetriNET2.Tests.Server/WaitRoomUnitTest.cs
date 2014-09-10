using System;
using System.Linq;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.Logger;
using TetriNET2.Server;
using TetriNET2.Server.Interfaces;
using TetriNET2.Tests.Server.Mocking;

namespace TetriNET2.Tests.Server
{
    [TestClass]
    public abstract class AbstractWaitRoomUnitTest
    {
        protected abstract IWaitRoom CreateWaitRoom(int maxClients);
        protected abstract IClient CreateClient(string name, ITetriNETCallback callback);

        [TestInitialize]
        public void Initialize()
        {
            Log.SetLogger(new LogMock());
        }

        [TestMethod]
        public void TestStrictlyPositiveMaxClients()
        {
            try
            {
                IWaitRoom waitRoom = CreateWaitRoom(0);
                Assert.Fail("ArgumentOutOfRange exception not raised");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.AreEqual(ex.ParamName, "maxClients");
            }
        }

        [TestMethod]
        public void TestConstructorsSetProperties()
        {
            const int maxClients = 10;
            IWaitRoom waitRoom = CreateWaitRoom(maxClients);

            Assert.AreEqual(waitRoom.MaxClients, maxClients);
        }

        [TestMethod]
        public void TestLockObjectNotNull()
        {
            IWaitRoom waitRoom = CreateWaitRoom(10);

            Assert.IsNotNull(waitRoom.LockObject);
        }

        [TestMethod]
        public void TestJoinNullClient()
        {
            IWaitRoom waitRoom = CreateWaitRoom(10);

            try
            {
                waitRoom.Join(null);

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual(ex.ParamName, "client");
            }

            Assert.AreEqual(waitRoom.ClientCount, 0);
            Assert.AreEqual(waitRoom.Clients.Count(), 0);
        }

        [TestMethod]
        public void TestJoinNoMaxClients()
        {
            IWaitRoom waitRoom = CreateWaitRoom(10);

            bool inserted1 = waitRoom.Join(CreateClient("client1", new CountCallTetriNETCallback()));
            bool inserted2 = waitRoom.Join(CreateClient("client2", new CountCallTetriNETCallback()));

            Assert.IsTrue(inserted1);
            Assert.IsTrue(inserted2);
            Assert.AreEqual(waitRoom.ClientCount, 2);
            Assert.AreEqual(waitRoom.Clients.Count(), 2);
            Assert.IsTrue(waitRoom.Clients.Any(x => x.Name == "client1") && waitRoom.Clients.Any(x => x.Name == "client2"));
        }

        [TestMethod]
        public void TestJoinWithMaxClients()
        {
            IWaitRoom waitRoom = CreateWaitRoom(1);
            waitRoom.Join(CreateClient("client1", new CountCallTetriNETCallback()));

            bool inserted = waitRoom.Join(CreateClient("client2", new CountCallTetriNETCallback()));

            Assert.IsFalse(inserted);
            Assert.AreEqual(waitRoom.ClientCount, 1);
            Assert.IsTrue(waitRoom.Clients.First().Name == "client1");
        }

        [TestMethod]
        public void TestJoinSameClient()
        {
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            IWaitRoom waitRoom = CreateWaitRoom(10);
            waitRoom.Join(client1);

            bool inserted = waitRoom.Join(client1);

            Assert.IsFalse(inserted);
            Assert.AreEqual(waitRoom.ClientCount, 1);
            Assert.AreEqual(waitRoom.Clients.Count(), 1);
        }

        [TestMethod]
        public void TestLeaveExistingClient()
        {
            IWaitRoom waitRoom = CreateWaitRoom(10);
            IClient client = CreateClient("client1", new CountCallTetriNETCallback());
            waitRoom.Join(client);

            bool removed = waitRoom.Leave(client);

            Assert.IsTrue(removed);
            Assert.AreEqual(waitRoom.ClientCount, 0);
            Assert.AreEqual(waitRoom.Clients.Count(), 0);
        }

        [TestMethod]
        public void TestLeaveNonExistingClient()
        {
            IWaitRoom waitRoom = CreateWaitRoom(10);
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            IClient client2 = CreateClient("client2", new CountCallTetriNETCallback());
            waitRoom.Join(client1);

            bool removed = waitRoom.Leave(client2);

            Assert.IsFalse(removed);
            Assert.AreEqual(waitRoom.ClientCount, 1);
            Assert.AreEqual(waitRoom.Clients.Count(), 1);
        }

        [TestMethod]
        public void TestLeaveNullClient()
        {
            IWaitRoom waitRoom = CreateWaitRoom(10);
            waitRoom.Join(CreateClient("client1", new CountCallTetriNETCallback()));

            try
            {
                waitRoom.Leave(null);
                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual(ex.ParamName, "client");
            }

            Assert.AreEqual(waitRoom.ClientCount, 1);
            Assert.AreEqual(waitRoom.Clients.Count(), 1);
        }

        [TestMethod]
        public void TestClearNoClients()
        {
            IWaitRoom waitRoom = CreateWaitRoom(10);

            waitRoom.Clear();

            Assert.AreEqual(waitRoom.ClientCount, 0);
        }

        [TestMethod]
        public void TestClearSomeClients()
        {
            IWaitRoom waitRoom = CreateWaitRoom(10);
            waitRoom.Join(CreateClient("client1", new CountCallTetriNETCallback()));
            waitRoom.Join(CreateClient("client2", new CountCallTetriNETCallback()));
            waitRoom.Join(CreateClient("client3", new CountCallTetriNETCallback()));

            waitRoom.Clear();

            Assert.AreEqual(waitRoom.ClientCount, 0);
        }
    }

    [TestClass]
    public class WaitRoomUnitTest : AbstractWaitRoomUnitTest
    {
        protected override IWaitRoom CreateWaitRoom(int maxClients)
        {
            return new WaitRoom(maxClients);
        }

        protected override IClient CreateClient(string name, ITetriNETCallback callback)
        {
            return new Client(name, IPAddress.Any, callback);
        }
    }
}
