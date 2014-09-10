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
    public abstract class AbstractAdminManagerUnitTest
    {
        protected abstract IAdminManager CreateAdminManager(int maxAdmins);
        protected abstract IAdmin CreateAdmin(string name, ITetriNETAdminCallback callback);

        [TestInitialize]
        public void Initialize()
        {
            Log.SetLogger(new LogMock());
        }

        [TestMethod]
        public void TestStrictlyPositiveMaxAdmins()
        {
            try
            {
                IAdminManager adminManager = CreateAdminManager(0);
                Assert.Fail("ArgumentOutOfRange exception not raised");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.AreEqual(ex.ParamName, "maxAdmins");
            }
        }

        [TestMethod]
        public void TestConstructorsSetProperties()
        {
            const int maxAdmins = 10;
            IAdminManager adminManager = CreateAdminManager(maxAdmins);

            Assert.AreEqual(adminManager.MaxAdmins, maxAdmins);
        }

        [TestMethod]
        public void TestLockObjectNotNull()
        {
            IAdminManager adminManager = CreateAdminManager(10);

            Assert.IsNotNull(adminManager.LockObject);
        }

        [TestMethod]
        public void TestAddNullAdmin()
        {
            IAdminManager adminManager = CreateAdminManager(10);

            bool added = adminManager.Add(null);

            Assert.IsFalse(added);
            Assert.AreEqual(adminManager.AdminCount, 0);
            Assert.AreEqual(adminManager.Admins.Count(), 0);
        }

        [TestMethod]
        public void TestAddNoMaxAdmins()
        {
            IAdminManager adminManager = CreateAdminManager(10);

            bool inserted1 = adminManager.Add(CreateAdmin("admin1", new CountCallTetriNETAdminCallback()));
            bool inserted2 = adminManager.Add(CreateAdmin("admin2", new CountCallTetriNETAdminCallback()));

            Assert.IsTrue(inserted1);
            Assert.IsTrue(inserted2);
            Assert.AreEqual(adminManager.AdminCount, 2);
            Assert.AreEqual(adminManager.Admins.Count(), 2);
            Assert.IsTrue(adminManager.Admins.Any(x => x.Name == "admin1") && adminManager.Admins.Any(x => x.Name == "admin2"));
        }

        [TestMethod]
        public void TestAddWithMaxAdmins()
        {
            IAdminManager adminManager = CreateAdminManager(1);
            adminManager.Add(CreateAdmin("admin1", new CountCallTetriNETAdminCallback()));

            bool inserted = adminManager.Add(CreateAdmin("admin2", new CountCallTetriNETAdminCallback()));

            Assert.IsFalse(inserted);
            Assert.AreEqual(adminManager.AdminCount, 1);
            Assert.IsTrue(adminManager.Admins.Any(x => x.Name == "admin1"));
        }

        [TestMethod]
        public void TestAddSameName()
        {
            IAdminManager adminManager = CreateAdminManager(10);
            adminManager.Add(CreateAdmin("admin1", new CountCallTetriNETAdminCallback()));

            bool inserted = adminManager.Add(CreateAdmin("admin1", new CountCallTetriNETAdminCallback()));

            Assert.IsFalse(inserted);
            Assert.AreEqual(adminManager.AdminCount, 1);
        }

        [TestMethod]
        public void TestAddSameCallback()
        {
            IAdminManager adminManager = CreateAdminManager(10);
            ITetriNETAdminCallback callback = new CountCallTetriNETAdminCallback();
            adminManager.Add(CreateAdmin("admin1", callback));

            bool inserted = adminManager.Add(CreateAdmin("admin2", callback));

            Assert.IsFalse(inserted);
            Assert.AreEqual(adminManager.AdminCount, 1);
        }

        [TestMethod]
        public void TestRemoveExistingPlayer()
        {
            IPlayerManager playerManager = CreatePlayerManager(5);
            IPlayer player = new Player(0, "player1", new CountCallTetriNETCallback());
            playerManager.Add(player);

            bool removed = playerManager.Remove(player);

            Assert.IsTrue(removed);
            Assert.AreEqual(playerManager.PlayerCount, 0);
        }

        [TestMethod]
        public void TestRemoveNonExistingPlayer()
        {
            IPlayerManager playerManager = CreatePlayerManager(5);
            playerManager.Add(new Player(0, "player1", new CountCallTetriNETCallback()));

            bool removed = playerManager.Remove(new Player(0, "player2", new CountCallTetriNETCallback()));

            Assert.IsFalse(removed);
            Assert.AreEqual(playerManager.PlayerCount, 1);
            Assert.AreEqual(playerManager.Players.Count, 1);
        }

        [TestMethod]
        public void TestRemoveNullPlayer()
        {
            IPlayerManager playerManager = CreatePlayerManager(5);
            playerManager.Add(new Player(0, "player1", new CountCallTetriNETCallback()));

            bool removed = playerManager.Remove(null);

            Assert.IsFalse(removed);
            Assert.AreEqual(playerManager.PlayerCount, 1);
        }

        [TestMethod]
        public void TestClearNoPlayers()
        {
            IPlayerManager playerManager = CreatePlayerManager(5);

            playerManager.Clear();

            Assert.AreEqual(playerManager.PlayerCount, 0);
        }

        [TestMethod]
        public void TestClearSomePlayers()
        {
            IPlayerManager playerManager = CreatePlayerManager(5);
            playerManager.Add(new Player(0, "player1", new CountCallTetriNETCallback()));
            playerManager.Add(new Player(1, "player2", new CountCallTetriNETCallback()));
            playerManager.Add(new Player(2, "player3", new CountCallTetriNETCallback()));

            playerManager.Clear();

            Assert.AreEqual(playerManager.PlayerCount, 0);
        }
    }

    [TestClass]
    public class AdminManagerUnitTest : AbstractAdminManagerUnitTest
    {
        protected override IAdminManager CreateAdminManager(int maxAdmins)
        {
            return new AdminManager(maxAdmins);
        }

        protected override IAdmin CreateAdmin(string name, ITetriNETAdminCallback callback)
        {
            return new Admin(name, IPAddress.Any, callback);
        }
    }
}
