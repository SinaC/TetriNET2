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
    // TODO: invert parameters in Assert.AreEqual and Assert.AreNotEqual

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

        [TestMethod]
        public void TestExceptionFreeAction()
        {
            IClient client = CreateClient("Client1", IPAddress.Parse("127.0.0.1"), new RaiseExceptionTetriNETCallback());

            client.OnDisconnected();

            Assert.IsTrue(true, "No exception occured");
        }

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

        [TestMethod]
        public void TestLastActionToClientUpdate()
        {
            IClient client = CreateClient("Client1", IPAddress.Parse("127.0.0.1"), new CountCallTetriNETCallback());
            DateTime lastActionToClient = client.LastActionToClient;

            Thread.Sleep(1);
            client.OnHeartbeatReceived();

            Assert.AreNotEqual(lastActionToClient, client.LastActionToClient);
        }

        [TestMethod]
        public void TestSetTimeout()
        {
            IClient client = CreateClient("Client1", IPAddress.Parse("127.0.0.1"), new CountCallTetriNETCallback());
            DateTime lastActionFromClient = DateTime.Now;

            Thread.Sleep(1);
            client.SetTimeout();

            Assert.AreNotEqual(lastActionFromClient, client.LastActionFromClient);
            Assert.AreEqual(client.TimeoutCount, 1);
        }

        [TestMethod]
        public void TestResetTimeout()
        {
            IClient client = CreateClient("Client1", IPAddress.Parse("127.0.0.1"), new CountCallTetriNETCallback());
            client.SetTimeout();
            DateTime lastActionFromClient = DateTime.Now;

            Thread.Sleep(1);
            client.ResetTimeout();

            Assert.AreNotEqual(lastActionFromClient, client.LastActionFromClient);
            Assert.AreEqual(client.TimeoutCount, 0);
        }

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
                Assert.AreEqual(ex.ParamName, "name");
            }
        }

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
                Assert.AreEqual(ex.ParamName, "address");
            }
        }

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
                Assert.AreEqual(ex.ParamName, "callback");
            }
        }

        [TestMethod]
        public void TestConstructorSetProperties()
        {
            const string name = "Client1";
            IPAddress address = IPAddress.Parse("127.0.0.1");
            ITetriNETCallback callback = new CountCallTetriNETCallback();
            const string team = "Team1";

            IClient client = CreateClient(name, address, callback, team);

            Assert.AreEqual(client.Name, name);
            Assert.AreEqual(client.Address, address);
            Assert.AreEqual(client.Callback, callback);
            Assert.AreEqual(client.Team, team);
            Assert.AreNotEqual(client.ConnectTime, default(DateTime));
            Assert.IsFalse(client.Id.Equals(default(Guid)));
        }

        #endregion
    }
}
