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
    public abstract class AbstractClientManagerUnitTest
    {
        protected abstract IClientManager CreateClientManager(int maxClients);
        protected abstract IClient CreateClient(string name, IPAddress address, ITetriNETCallback callback, string team = null);

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
                IClientManager clientManager = CreateClientManager(0);
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
            IClientManager clientManager = CreateClientManager(maxClients);

            Assert.AreEqual(clientManager.MaxClients, maxClients);
        }

        [TestMethod]
        public void TestLockObjectNotNull()
        {
            IClientManager clientManager = CreateClientManager(10);

            Assert.IsNotNull(clientManager.LockObject);
        }

        [TestMethod]
        public void TestAddNullClient()
        {
            IClientManager clientManager = CreateClientManager(10);

            try
            {
                clientManager.Add(null);

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual(ex.ParamName, "client");
            }

            Assert.AreEqual(clientManager.ClientCount, 0);
            Assert.AreEqual(clientManager.Clients.Count(), 0);
        }

        [TestMethod]
        public void TestAddNoMaxClients()
        {
            IClientManager clientManager = CreateClientManager(10);

            bool inserted1 = clientManager.Add(CreateClient("client1", IPAddress.Any, new CountCallTetriNETCallback()));
            bool inserted2 = clientManager.Add(CreateClient("client2", IPAddress.Any, new CountCallTetriNETCallback()));

            Assert.IsTrue(inserted1);
            Assert.IsTrue(inserted2);
            Assert.AreEqual(clientManager.ClientCount, 2);
            Assert.AreEqual(clientManager.Clients.Count(), 2);
            Assert.IsTrue(clientManager.Clients.Any(x => x.Name == "client1") && clientManager.Clients.Any(x => x.Name == "client2"));
        }

        [TestMethod]
        public void TestAddWithMaxClients()
        {
            IClientManager clientManager = CreateClientManager(1);
            clientManager.Add(CreateClient("client1", IPAddress.Any, new CountCallTetriNETCallback()));

            bool inserted = clientManager.Add(CreateClient("client2", IPAddress.Any, new CountCallTetriNETCallback()));

            Assert.IsFalse(inserted);
            Assert.AreEqual(clientManager.ClientCount, 1);
            Assert.IsTrue(clientManager.Clients.First().Name == "client1");
        }

        [TestMethod]
        public void TestAddSameClient()
        {
            IClient client1 = CreateClient("client1", IPAddress.Any, new CountCallTetriNETCallback());
            IClientManager clientManager = CreateClientManager(10);
            clientManager.Add(client1);

            bool inserted = clientManager.Add(client1);

            Assert.IsFalse(inserted);
            Assert.AreEqual(clientManager.ClientCount, 1);
            Assert.AreEqual(clientManager.Clients.Count(), 1);
        }

        [TestMethod]
        public void TestAddSameName()
        {
            IClientManager clientManager = CreateClientManager(10);
            clientManager.Add(CreateClient("client1", IPAddress.Any, new CountCallTetriNETCallback()));

            bool inserted = clientManager.Add(CreateClient("client1", IPAddress.Any, new CountCallTetriNETCallback()));

            Assert.IsFalse(inserted);
            Assert.AreEqual(clientManager.ClientCount, 1);
            Assert.AreEqual(clientManager.Clients.Count(), 1);
        }

        [TestMethod]
        public void TestAddSameCallback()
        {
            IClientManager clientManager = CreateClientManager(10);
            ITetriNETCallback callback = new CountCallTetriNETCallback();
            clientManager.Add(CreateClient("client1", IPAddress.Any, callback));

            bool inserted = clientManager.Add(CreateClient("client2", IPAddress.Any, callback));

            Assert.IsFalse(inserted);
            Assert.AreEqual(clientManager.ClientCount, 1);
            Assert.AreEqual(clientManager.Clients.Count(), 1);
        }

        [TestMethod]
        public void TestRemoveExistingClient()
        {
            IClientManager clientManager = CreateClientManager(10);
            IClient client = CreateClient("client1", IPAddress.Any, new CountCallTetriNETCallback());
            clientManager.Add(client);

            bool removed = clientManager.Remove(client);

            Assert.IsTrue(removed);
            Assert.AreEqual(clientManager.ClientCount, 0);
            Assert.AreEqual(clientManager.Clients.Count(), 0);
        }

        [TestMethod]
        public void TestRemoveNonExistingClient()
        {
            IClientManager clientManager = CreateClientManager(10);
            IClient client1 = CreateClient("client1", IPAddress.Any, new CountCallTetriNETCallback());
            IClient client2 = CreateClient("client2", IPAddress.Any, new CountCallTetriNETCallback());
            clientManager.Add(client1);

            bool removed = clientManager.Remove(client2);

            Assert.IsFalse(removed);
            Assert.AreEqual(clientManager.ClientCount, 1);
            Assert.AreEqual(clientManager.Clients.Count(), 1);
        }

        [TestMethod]
        public void TestRemoveNullClient()
        {
            IClientManager clientManager = CreateClientManager(10);
            clientManager.Add(CreateClient("client1", IPAddress.Any, new CountCallTetriNETCallback()));

            try
            {
                clientManager.Remove(null);
                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual(ex.ParamName, "client");
            }

            Assert.AreEqual(clientManager.ClientCount, 1);
            Assert.AreEqual(clientManager.Clients.Count(), 1);
        }

        [TestMethod]
        public void TestClearNoClients()
        {
            IClientManager clientManager = CreateClientManager(10);

            clientManager.Clear();

            Assert.AreEqual(clientManager.ClientCount, 0);
        }

        [TestMethod]
        public void TestClearSomeClients()
        {
            IClientManager clientManager = CreateClientManager(10);
            clientManager.Add(CreateClient("client1", IPAddress.Any, new CountCallTetriNETCallback()));
            clientManager.Add(CreateClient("client2", IPAddress.Any, new CountCallTetriNETCallback()));
            clientManager.Add(CreateClient("client3", IPAddress.Any, new CountCallTetriNETCallback()));

            clientManager.Clear();

            Assert.AreEqual(clientManager.ClientCount, 0);
        }

        [TestMethod]
        public void TestContainsExistingClient()
        {
            IClientManager clientManager = CreateClientManager(10);
            IClient client = CreateClient("client1", IPAddress.Any, new CountCallTetriNETCallback());
            clientManager.Add(client);

            bool containsOnName = clientManager.Contains(client.Name, null);
            bool containsOnCallback = clientManager.Contains(null, client.Callback);

            Assert.IsTrue(containsOnName);
            Assert.IsTrue(containsOnCallback);
        }

        [TestMethod]
        public void TestContainsNonExistingClient()
        {
            IClientManager clientManager = CreateClientManager(10);
            IClient client = CreateClient("client1", IPAddress.Any, new CountCallTetriNETCallback());
            clientManager.Add(client);

            bool containsOnName = clientManager.Contains("client2", null);
            bool containsOnCallback = clientManager.Contains(null, new CountCallTetriNETCallback());

            Assert.IsFalse(containsOnName);
            Assert.IsFalse(containsOnCallback);
        }

        [TestMethod]
        public void TestGuidIndexerFindExistingClient()
        {
            IClient client1 = CreateClient("client1", IPAddress.Any, new CountCallTetriNETCallback());
            IClient client2 = CreateClient("client2", IPAddress.Any, new CountCallTetriNETCallback());
            IClient client3 = CreateClient("client3", IPAddress.Any, new CountCallTetriNETCallback());
            IClientManager clientManager = CreateClientManager(10);
            clientManager.Add(client1);
            clientManager.Add(client2);
            clientManager.Add(client3);

            IClient searched = clientManager[client2.Id];

            Assert.IsNotNull(searched);
            Assert.AreEqual(searched, client2);
        }

        [TestMethod]
        public void TestGuidIndexerFindNonExistingClient()
        {
            IClient client1 = CreateClient("client1", IPAddress.Any, new CountCallTetriNETCallback());
            IClient client2 = CreateClient("client2", IPAddress.Any, new CountCallTetriNETCallback());
            IClient client3 = CreateClient("client3", IPAddress.Any, new CountCallTetriNETCallback());
            IClientManager clientManager = CreateClientManager(10);
            clientManager.Add(client1);
            clientManager.Add(client2);
            clientManager.Add(client3);

            IClient searched = clientManager[Guid.Empty];

            Assert.IsNull(searched);
        }

        [TestMethod]
        public void TestNameIndexerFindExistingClient()
        {
            IClient client1 = CreateClient("client1", IPAddress.Any, new CountCallTetriNETCallback());
            IClient client2 = CreateClient("client2", IPAddress.Any, new CountCallTetriNETCallback());
            IClient client3 = CreateClient("client3", IPAddress.Any, new CountCallTetriNETCallback());
            IClientManager clientManager = CreateClientManager(10);
            clientManager.Add(client1);
            clientManager.Add(client2);
            clientManager.Add(client3);

            IClient searched = clientManager[client2.Name];

            Assert.IsNotNull(searched);
            Assert.AreEqual(searched, client2);
        }

        [TestMethod]
        public void TestNameIndexerFindNonExistingClient()
        {
            IClient client1 = CreateClient("client1", IPAddress.Any, new CountCallTetriNETCallback());
            IClient client2 = CreateClient("client2", IPAddress.Any, new CountCallTetriNETCallback());
            IClient client3 = CreateClient("client3", IPAddress.Any, new CountCallTetriNETCallback());
            IClientManager clientManager = CreateClientManager(10);
            clientManager.Add(client1);
            clientManager.Add(client2);
            clientManager.Add(client3);

            IClient searched = clientManager["client4"];

            Assert.IsNull(searched);
        }

        [TestMethod]
        public void TestCallbackIndexerFindExistingClient()
        {
            IClient client1 = CreateClient("client1", IPAddress.Any, new CountCallTetriNETCallback());
            IClient client2 = CreateClient("client2", IPAddress.Any, new CountCallTetriNETCallback());
            IClient client3 = CreateClient("client3", IPAddress.Any, new CountCallTetriNETCallback());
            IClientManager clientManager = CreateClientManager(10);
            clientManager.Add(client1);
            clientManager.Add(client2);
            clientManager.Add(client3);

            IClient searched = clientManager[client2.Callback];

            Assert.IsNotNull(searched);
            Assert.AreEqual(searched, client2);
        }

        [TestMethod]
        public void TestCallbackIndexerFindNonExistingClient()
        {
            IClient client1 = CreateClient("client1", IPAddress.Any, new CountCallTetriNETCallback());
            IClient client2 = CreateClient("client2", IPAddress.Any, new CountCallTetriNETCallback());
            IClient client3 = CreateClient("client3", IPAddress.Any, new CountCallTetriNETCallback());
            IClientManager clientManager = CreateClientManager(10);
            clientManager.Add(client1);
            clientManager.Add(client2);
            clientManager.Add(client3);

            IClient searched = clientManager[new CountCallTetriNETCallback()];

            Assert.IsNull(searched);
        }

        [TestMethod]
        public void TestAddressIndexerFindExistingClient()
        {
            IClient client1 = CreateClient("client1", IPAddress.Parse("127.0.0.1"), new CountCallTetriNETCallback());
            IClient client2 = CreateClient("client2", IPAddress.Parse("127.0.0.2"), new CountCallTetriNETCallback());
            IClient client3 = CreateClient("client3", IPAddress.Parse("127.0.0.3"), new CountCallTetriNETCallback());
            IClientManager clientManager = CreateClientManager(10);
            clientManager.Add(client1);
            clientManager.Add(client2);
            clientManager.Add(client3);

            IClient searched = clientManager[client2.Address];

            Assert.IsNotNull(searched);
            Assert.AreEqual(searched, client2);
        }

        [TestMethod]
        public void TestAddressIndexerFindNonExistingClient()
        {
            IClient client1 = CreateClient("client1", IPAddress.Any, new CountCallTetriNETCallback());
            IClient client2 = CreateClient("client2", IPAddress.Any, new CountCallTetriNETCallback());
            IClient client3 = CreateClient("client3", IPAddress.Any, new CountCallTetriNETCallback());
            IClientManager clientManager = CreateClientManager(10);
            clientManager.Add(client1);
            clientManager.Add(client2);
            clientManager.Add(client3);

            IClient searched = clientManager[IPAddress.None];

            Assert.IsNull(searched);
        }
    }

    [TestClass]
    public class ClientManagerUnitTest : AbstractClientManagerUnitTest
    {
        protected override IClientManager CreateClientManager(int maxClients)
        {
            return new ClientManager(maxClients);
        }

        protected override IClient CreateClient(string name, IPAddress address, ITetriNETCallback callback, string team = null)
        {
            return new Client(name, address, callback, team);
        }
    }
}
