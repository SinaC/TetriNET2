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
            Log.Default.Logger = new LogMock();
        }

        #region Add

        [TestCategory("Server")]
        [TestCategory("Server.IClientManager")]
        [TestCategory("Server.IClientManager.Add")]
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
                Assert.AreEqual("client", ex.ParamName);
            }

            Assert.AreEqual(0, clientManager.ClientCount);
            Assert.AreEqual(0, clientManager.Clients.Count());
        }

        [TestCategory("Server")]
        [TestCategory("Server.IClientManager")]
        [TestCategory("Server.IClientManager.Add")]
        [TestMethod]
        public void TestAddNoMaxClients()
        {
            IClientManager clientManager = CreateClientManager(10);

            bool inserted1 = clientManager.Add(CreateClient("client1", IPAddress.Any, new CountCallTetriNETCallback()));
            bool inserted2 = clientManager.Add(CreateClient("client2", IPAddress.Any, new CountCallTetriNETCallback()));

            Assert.IsTrue(inserted1);
            Assert.IsTrue(inserted2);
            Assert.AreEqual(2, clientManager.ClientCount);
            Assert.AreEqual(2, clientManager.Clients.Count());
            Assert.IsTrue(clientManager.Clients.Any(x => x.Name == "client1") && clientManager.Clients.Any(x => x.Name == "client2"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IClientManager")]
        [TestCategory("Server.IClientManager.Add")]
        [TestMethod]
        public void TestAddWithMaxClients()
        {
            IClientManager clientManager = CreateClientManager(1);
            clientManager.Add(CreateClient("client1", IPAddress.Any, new CountCallTetriNETCallback()));

            bool inserted = clientManager.Add(CreateClient("client2", IPAddress.Any, new CountCallTetriNETCallback()));

            Assert.IsFalse(inserted);
            Assert.AreEqual(1, clientManager.ClientCount);
            Assert.IsTrue(clientManager.Clients.First().Name == "client1");
        }

        [TestCategory("Server")]
        [TestCategory("Server.IClientManager")]
        [TestCategory("Server.IClientManager.Add")]
        [TestMethod]
        public void TestAddSameClient()
        {
            IClient client1 = CreateClient("client1", IPAddress.Any, new CountCallTetriNETCallback());
            IClientManager clientManager = CreateClientManager(10);
            clientManager.Add(client1);

            bool inserted = clientManager.Add(client1);

            Assert.IsFalse(inserted);
            Assert.AreEqual(1, clientManager.ClientCount);
            Assert.AreEqual(1, clientManager.Clients.Count());
        }

        [TestCategory("Server")]
        [TestCategory("Server.IClientManager")]
        [TestCategory("Server.IClientManager.Add")]
        [TestMethod]
        public void TestAddSameName()
        {
            IClientManager clientManager = CreateClientManager(10);
            clientManager.Add(CreateClient("client1", IPAddress.Any, new CountCallTetriNETCallback()));

            bool inserted = clientManager.Add(CreateClient("client1", IPAddress.Any, new CountCallTetriNETCallback()));

            Assert.IsFalse(inserted);
            Assert.AreEqual(1, clientManager.ClientCount);
            Assert.AreEqual(1, clientManager.Clients.Count());
        }

        [TestCategory("Server")]
        [TestCategory("Server.IClientManager")]
        [TestCategory("Server.IClientManager.Add")]
        [TestMethod]
        public void TestAddSameCallback()
        {
            IClientManager clientManager = CreateClientManager(10);
            ITetriNETCallback callback = new CountCallTetriNETCallback();
            clientManager.Add(CreateClient("client1", IPAddress.Any, callback));

            bool inserted = clientManager.Add(CreateClient("client2", IPAddress.Any, callback));

            Assert.IsFalse(inserted);
            Assert.AreEqual(1, clientManager.ClientCount);
            Assert.AreEqual(1, clientManager.Clients.Count());
        }

        #endregion

        #region Remove

        [TestCategory("Server")]
        [TestCategory("Server.IClientManager")]
        [TestCategory("Server.IClientManager.Remove")]
        [TestMethod]
        public void TestRemoveExistingClient()
        {
            IClientManager clientManager = CreateClientManager(10);
            IClient client = CreateClient("client1", IPAddress.Any, new CountCallTetriNETCallback());
            clientManager.Add(client);

            bool removed = clientManager.Remove(client);

            Assert.IsTrue(removed);
            Assert.AreEqual(0, clientManager.ClientCount);
            Assert.AreEqual(0, clientManager.Clients.Count());
        }

        [TestCategory("Server")]
        [TestCategory("Server.IClientManager")]
        [TestCategory("Server.IClientManager.Remove")]
        [TestMethod]
        public void TestRemoveNonExistingClient()
        {
            IClientManager clientManager = CreateClientManager(10);
            IClient client1 = CreateClient("client1", IPAddress.Any, new CountCallTetriNETCallback());
            IClient client2 = CreateClient("client2", IPAddress.Any, new CountCallTetriNETCallback());
            clientManager.Add(client1);

            bool removed = clientManager.Remove(client2);

            Assert.IsFalse(removed);
            Assert.AreEqual(1, clientManager.ClientCount);
            Assert.AreEqual(1, clientManager.Clients.Count());
        }

        [TestCategory("Server")]
        [TestCategory("Server.IClientManager")]
        [TestCategory("Server.IClientManager.Remove")]
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
                Assert.AreEqual("client", ex.ParamName);
            }

            Assert.AreEqual(1, clientManager.ClientCount);
            Assert.AreEqual(1, clientManager.Clients.Count());
        }

        #endregion
        
        #region Clear

        [TestCategory("Server")]
        [TestCategory("Server.IClientManager")]
        [TestCategory("Server.IClientManager.Clear")]
        [TestMethod]
        public void TestClearNoClients()
        {
            IClientManager clientManager = CreateClientManager(10);

            clientManager.Clear();

            Assert.AreEqual(0, clientManager.ClientCount);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IClientManager")]
        [TestCategory("Server.IClientManager.Clear")]
        [TestMethod]
        public void TestClearSomeClients()
        {
            IClientManager clientManager = CreateClientManager(10);
            clientManager.Add(CreateClient("client1", IPAddress.Any, new CountCallTetriNETCallback()));
            clientManager.Add(CreateClient("client2", IPAddress.Any, new CountCallTetriNETCallback()));
            clientManager.Add(CreateClient("client3", IPAddress.Any, new CountCallTetriNETCallback()));

            clientManager.Clear();

            Assert.AreEqual(0, clientManager.ClientCount);
        }

        #endregion

        #region Contains

        [TestCategory("Server")]
        [TestCategory("Server.IClientManager")]
        [TestCategory("Server.IClientManager.Contains")]
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

        [TestCategory("Server")]
        [TestCategory("Server.IClientManager")]
        [TestCategory("Server.IClientManager.Contains")]
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

        #endregion
        
        #region Indexers

        [TestCategory("Server")]
        [TestCategory("Server.IClientManager")]
        [TestCategory("Server.IClientManager.Indexers")]
        [TestMethod]
        public void TestIndexerGuidFindExistingClient()
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
            Assert.AreEqual(client2, searched);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IClientManager")]
        [TestCategory("Server.IClientManager.Indexers")]
        [TestMethod]
        public void TestIndexerGuidFindNonExistingClient()
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

        [TestCategory("Server")]
        [TestCategory("Server.IClientManager")]
        [TestCategory("Server.IClientManager.Indexers")]
        [TestMethod]
        public void TestIndexerNameFindExistingClient()
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
            Assert.AreEqual(client2, searched);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IClientManager")]
        [TestCategory("Server.IClientManager.Indexers")]
        [TestMethod]
        public void TestIndexerNameFindNonExistingClient()
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

        [TestCategory("Server")]
        [TestCategory("Server.IClientManager")]
        [TestCategory("Server.IClientManager.Indexers")]
        [TestMethod]
        public void TestIndexerCallbackFindExistingClient()
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
            Assert.AreEqual(client2, searched);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IClientManager")]
        [TestCategory("Server.IClientManager.Indexers")]
        [TestMethod]
        public void TestIndexerCallbackFindNonExistingClient()
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

        [TestCategory("Server")]
        [TestCategory("Server.IClientManager")]
        [TestCategory("Server.IClientManager.Indexers")]
        [TestMethod]
        public void TestIndexerAddressFindExistingClient()
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
            Assert.AreEqual(client2, searched);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IClientManager")]
        [TestCategory("Server.IClientManager.Indexers")]
        [TestMethod]
        public void TestIndexerAddressFindNonExistingClient()
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

        #endregion
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

        #region Constructor

        [TestCategory("Server")]
        [TestCategory("Server.ClientManager")]
        [TestCategory("Server.ClientManager.ctor")]
        [TestMethod]
        public void TestConstructorStrictlyPositiveMaxClients()
        {
            try
            {
                IClientManager clientManager = CreateClientManager(0);
                Assert.Fail("ArgumentOutOfRange exception not raised");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.AreEqual("maxClients", ex.ParamName);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.ClientManager")]
        [TestCategory("Server.ClientManager.ctor")]
        [TestMethod]
        public void TestConstructorSetProperties()
        {
            const int maxClients = 10;

            IClientManager clientManager = CreateClientManager(maxClients);

            Assert.AreEqual(maxClients, clientManager.MaxClients);
        }

        [TestCategory("Server")]
        [TestCategory("Server.ClientManager")]
        [TestCategory("Server.ClientManager.ctor")]
        [TestMethod]
        public void TestConstructorLockObjectNotNull()
        {
            IClientManager clientManager = CreateClientManager(10);

            Assert.IsNotNull(clientManager.LockObject);
        }

        #endregion
    }
}
