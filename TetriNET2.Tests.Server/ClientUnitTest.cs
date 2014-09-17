using System;
using System.Net;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.Logger;
using TetriNET2.Server;
using TetriNET2.Server.Interfaces;
using TetriNET2.Tests.Server.Mocking;

namespace TetriNET2.Tests.Server
{
    [TestClass]
    public abstract class AbstractClientUnitTest
    {
        protected abstract IClient CreateClient(string name, IPAddress address, ITetriNETCallback callback, string team = null);

        [TestInitialize]
        public void Initialize()
        {
            Log.Default.Logger = new LogMock();
        }

        #region Exception

        [TestCategory("Server")]
        [TestCategory("Server.IClient")]
        [TestCategory("Server.IClient.Exception")]
        [TestMethod]
        public void TestExceptionFreeAction()
        {
            IClient client = CreateClient("Client1", IPAddress.Parse("127.0.0.1"), new RaiseExceptionTetriNETCallback());

            client.OnDisconnected();

            Assert.IsTrue(true, "No exception occured");
        }

        [TestCategory("Server.IClient.Exception")]
        [TestMethod]
        public void TestConnectionLostCalledOnException()
        {
            bool called = false;
            IClient client = CreateClient("Client1", IPAddress.Parse("127.0.0.1"), new RaiseExceptionTetriNETCallback());
            client.ConnectionLost += entity => called = true;

            client.OnDisconnected();

            Assert.IsTrue(called);
        }

        #endregion

        #region Timeout

        [TestCategory("Server")]
        [TestCategory("Server.IClient")]
        [TestCategory("Server.IClient.Timeout")]
        [TestMethod]
        public void TestLastActionToClientUpdate()
        {
            IClient client = CreateClient("Client1", IPAddress.Parse("127.0.0.1"), new CountCallTetriNETCallback());
            DateTime lastActionToClient = client.LastActionToClient;

            Thread.Sleep(1);
            client.OnHeartbeatReceived();

            Assert.AreNotEqual(lastActionToClient, client.LastActionToClient);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IClient")]
        [TestCategory("Server.IClient.Timeout")]
        [TestMethod]
        public void TestSetTimeout()
        {
            IClient client = CreateClient("Client1", IPAddress.Parse("127.0.0.1"), new CountCallTetriNETCallback());
            DateTime lastActionFromClient = DateTime.Now;

            Thread.Sleep(1);
            client.SetTimeout();

            Assert.AreNotEqual(lastActionFromClient, client.LastActionFromClient);
            Assert.AreEqual(1, client.TimeoutCount);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IClient")]
        [TestCategory("Server.IClient.Timeout")]
        [TestMethod]
        public void TestResetTimeout()
        {
            IClient client = CreateClient("Client1", IPAddress.Parse("127.0.0.1"), new CountCallTetriNETCallback());
            client.SetTimeout();
            DateTime lastActionFromClient = DateTime.Now;

            Thread.Sleep(1);
            client.ResetTimeout();

            Assert.AreNotEqual(lastActionFromClient, client.LastActionFromClient);
            Assert.AreEqual(0, client.TimeoutCount);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IClient")]
        [TestCategory("Server.IClient.Timeout")]
        [TestMethod]
        public void TestLastActionToClientNotUpdatedOnException()
        {
            IClient client = CreateClient("Client1", IPAddress.Parse("127.0.0.1"), new RaiseExceptionTetriNETCallback());
            DateTime lastActionToClient = client.LastActionToClient;

            Thread.Sleep(1);
            client.OnHeartbeatReceived();

            Assert.AreEqual(lastActionToClient, client.LastActionToClient);
        }

        #endregion
    }

    [TestClass]
    public class ClientUnitTest : AbstractClientUnitTest
    {
        protected override IClient CreateClient(string name, IPAddress address, ITetriNETCallback callback, string team = null)
        {
            return new Client(name, address, callback, team);
        }

        #region Constructor

        [TestCategory("Server")]
        [TestCategory("Server.Client")]
        [TestCategory("Server.Client.ctor")]
        [TestMethod]
        public void TestConstructorNullName()
        {
            try
            {
                IClient client = CreateClient(null, IPAddress.Parse("127.0.0.1"), new CountCallTetriNETCallback());

                Assert.Fail("ArgumentNullException on name not raised");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("name", ex.ParamName);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.Client")]
        [TestCategory("Server.Client.ctor")]
        [TestMethod]
        public void TestConstructorNullAddress()
        {
            try
            {
                IClient client = CreateClient("Client1", null, new CountCallTetriNETCallback());

                Assert.Fail("ArgumentNullException on name not raised");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("address", ex.ParamName);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.Client")]
        [TestCategory("Server.Client.ctor")]
        [TestMethod]
        public void TestConstructorNullCallback()
        {
            try
            {
                IClient client = CreateClient("Client1", IPAddress.Parse("127.0.0.1"), null);

                Assert.Fail("ArgumentNullException on callback not raised");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("callback", ex.ParamName);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.Client")]
        [TestCategory("Server.Client.ctor")]
        [TestMethod]
        public void TestConstructorSetProperties()
        {
            const string name = "Client1";
            IPAddress address = IPAddress.Parse("127.0.0.1");
            ITetriNETCallback callback = new CountCallTetriNETCallback();
            const string team = "Team1";

            IClient client = CreateClient(name, address, callback, team);

            Assert.AreEqual(name, client.Name);
            Assert.AreEqual(address, client.Address);
            Assert.AreEqual(callback, client.Callback);
            Assert.AreEqual(team, client.Team);
            Assert.AreNotEqual(default(DateTime), client.ConnectTime);
            Assert.IsFalse(client.Id.Equals(default(Guid)));
        }

        #endregion
    }
}
