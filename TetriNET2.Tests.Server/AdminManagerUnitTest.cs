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
        protected abstract IAdmin CreateAdmin(string name, ITetriNETAdminCallback callback, string address = null);

        [TestInitialize]
        public void Initialize()
        {
            Log.Default.Logger = new LogMock();
        }

        #region Add

        [TestMethod]
        public void TestAddNullAdmin()
        {
            IAdminManager adminManager = CreateAdminManager(10);

            try
            {
                adminManager.Add(null);

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("admin", ex.ParamName);
            }

            Assert.AreEqual(0, adminManager.AdminCount);
            Assert.AreEqual(0, adminManager.Admins.Count());
        }

        [TestMethod]
        public void TestAddNoMaxAdmins()
        {
            IAdminManager adminManager = CreateAdminManager(10);

            bool inserted1 = adminManager.Add(CreateAdmin("admin1", new CountCallTetriNETAdminCallback()));
            bool inserted2 = adminManager.Add(CreateAdmin("admin2", new CountCallTetriNETAdminCallback()));

            Assert.IsTrue(inserted1);
            Assert.IsTrue(inserted2);
            Assert.AreEqual(2, adminManager.AdminCount);
            Assert.AreEqual(2, adminManager.Admins.Count());
            Assert.IsTrue(adminManager.Admins.Any(x => x.Name == "admin1") && adminManager.Admins.Any(x => x.Name == "admin2"));
        }

        [TestMethod]
        public void TestAddWithMaxAdmins()
        {
            IAdminManager adminManager = CreateAdminManager(1);
            adminManager.Add(CreateAdmin("admin1", new CountCallTetriNETAdminCallback()));

            bool inserted = adminManager.Add(CreateAdmin("admin2", new CountCallTetriNETAdminCallback()));

            Assert.IsFalse(inserted);
            Assert.AreEqual(1, adminManager.AdminCount);
            Assert.IsTrue(adminManager.Admins.First().Name == "admin1");
        }

        [TestMethod]
        public void TestAddSameAdmin()
        {
            IAdmin admin1 = CreateAdmin("admin1", new CountCallTetriNETAdminCallback());
            IAdminManager adminManager = CreateAdminManager(10);
            adminManager.Add(admin1);

            bool inserted = adminManager.Add(admin1);

            Assert.IsFalse(inserted);
            Assert.AreEqual(1, adminManager.AdminCount);
            Assert.AreEqual(1, adminManager.Admins.Count());
        }

        [TestMethod]
        public void TestAddSameName()
        {
            IAdminManager adminManager = CreateAdminManager(10);
            adminManager.Add(CreateAdmin("admin1", new CountCallTetriNETAdminCallback()));

            bool inserted = adminManager.Add(CreateAdmin("admin1", new CountCallTetriNETAdminCallback()));

            Assert.IsFalse(inserted);
            Assert.AreEqual(1, adminManager.AdminCount);
            Assert.AreEqual(1, adminManager.Admins.Count());
        }

        [TestMethod]
        public void TestAddSameCallback()
        {
            IAdminManager adminManager = CreateAdminManager(10);
            ITetriNETAdminCallback callback = new CountCallTetriNETAdminCallback();
            adminManager.Add(CreateAdmin("admin1", callback));

            bool inserted = adminManager.Add(CreateAdmin("admin2", callback));

            Assert.IsFalse(inserted);
            Assert.AreEqual(1, adminManager.AdminCount);
            Assert.AreEqual(1, adminManager.Admins.Count());
        }

        #endregion

        #region Remove

        [TestMethod]
        public void TestRemoveExistingAdmin()
        {
            IAdminManager adminManager = CreateAdminManager(10);
            IAdmin admin = CreateAdmin("admin1", new CountCallTetriNETAdminCallback());
            adminManager.Add(admin);

            bool removed = adminManager.Remove(admin);

            Assert.IsTrue(removed);
            Assert.AreEqual(0, adminManager.AdminCount);
            Assert.AreEqual(0, adminManager.Admins.Count());
        }

        [TestMethod]
        public void TestRemoveNonExistingAdmin()
        {
            IAdminManager adminManager = CreateAdminManager(10);
            IAdmin admin1 = CreateAdmin("admin1", new CountCallTetriNETAdminCallback());
            IAdmin admin2 = CreateAdmin("admin2", new CountCallTetriNETAdminCallback());
            adminManager.Add(admin1);

            bool removed = adminManager.Remove(admin2);

            Assert.IsFalse(removed);
            Assert.AreEqual(1, adminManager.AdminCount);
            Assert.AreEqual(1, adminManager.Admins.Count());
        }

        [TestMethod]
        public void TestRemoveNullAdmin()
        {
            IAdminManager adminManager = CreateAdminManager(10);
            adminManager.Add(CreateAdmin("admin1", new CountCallTetriNETAdminCallback()));

            try
            {
                adminManager.Remove(null);
                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("admin", ex.ParamName);
            }

            Assert.AreEqual(1, adminManager.AdminCount);
            Assert.AreEqual(1, adminManager.Admins.Count());
        }

        #endregion

        #region Clear

        [TestMethod]
        public void TestClearNoAdmins()
        {
            IAdminManager adminManager = CreateAdminManager(10);

            adminManager.Clear();

            Assert.AreEqual(0, adminManager.AdminCount);
        }

        [TestMethod]
        public void TestClearSomeAdmins()
        {
            IAdminManager adminManager = CreateAdminManager(10);
            adminManager.Add(CreateAdmin("admin1", new CountCallTetriNETAdminCallback()));
            adminManager.Add(CreateAdmin("admin2", new CountCallTetriNETAdminCallback()));
            adminManager.Add(CreateAdmin("admin3", new CountCallTetriNETAdminCallback()));

            adminManager.Clear();

            Assert.AreEqual(0, adminManager.AdminCount);
        }

        #endregion

        #region Indexers

        [TestMethod]
        public void TestIndexerGuidFindExistingAdmin()
        {
            IAdmin admin1 = CreateAdmin("admin1", new CountCallTetriNETAdminCallback());
            IAdmin admin2 = CreateAdmin("admin2", new CountCallTetriNETAdminCallback());
            IAdmin admin3 = CreateAdmin("admin3", new CountCallTetriNETAdminCallback());
            IAdminManager adminManager = CreateAdminManager(10);
            adminManager.Add(admin1);
            adminManager.Add(admin2);
            adminManager.Add(admin3);

            IAdmin searched = adminManager[admin2.Id];

            Assert.IsNotNull(searched);
            Assert.AreEqual(searched, admin2);
        }

        [TestMethod]
        public void TestIndexerGuidFindNonExistingAdmin()
        {
            IAdmin admin1 = CreateAdmin("admin1", new CountCallTetriNETAdminCallback());
            IAdmin admin2 = CreateAdmin("admin2", new CountCallTetriNETAdminCallback());
            IAdmin admin3 = CreateAdmin("admin3", new CountCallTetriNETAdminCallback());
            IAdminManager adminManager = CreateAdminManager(10);
            adminManager.Add(admin1);
            adminManager.Add(admin2);
            adminManager.Add(admin3);

            IAdmin searched = adminManager[Guid.Empty];

            Assert.IsNull(searched);
        }

        [TestMethod]
        public void TestIndexertNameFindExistingAdmin()
        {
            IAdmin admin1 = CreateAdmin("admin1", new CountCallTetriNETAdminCallback());
            IAdmin admin2 = CreateAdmin("admin2", new CountCallTetriNETAdminCallback());
            IAdmin admin3 = CreateAdmin("admin3", new CountCallTetriNETAdminCallback());
            IAdminManager adminManager = CreateAdminManager(10);
            adminManager.Add(admin1);
            adminManager.Add(admin2);
            adminManager.Add(admin3);

            IAdmin searched = adminManager["admin2"];

            Assert.IsNotNull(searched);
            Assert.AreEqual(searched, admin2);
        }

        [TestMethod]
        public void TesIndexertNameFindNonExistingAdmin()
        {
            IAdmin admin1 = CreateAdmin("admin1", new CountCallTetriNETAdminCallback());
            IAdmin admin2 = CreateAdmin("admin2", new CountCallTetriNETAdminCallback());
            IAdmin admin3 = CreateAdmin("admin3", new CountCallTetriNETAdminCallback());
            IAdminManager adminManager = CreateAdminManager(10);
            adminManager.Add(admin1);
            adminManager.Add(admin2);
            adminManager.Add(admin3);

            IAdmin searched = adminManager["admin4"];

            Assert.IsNull(searched);
        }

        [TestMethod]
        public void TestIndexerCallbackFindExistingAdmin()
        {
            IAdmin admin1 = CreateAdmin("admin1", new CountCallTetriNETAdminCallback());
            IAdmin admin2 = CreateAdmin("admin2", new CountCallTetriNETAdminCallback());
            IAdmin admin3 = CreateAdmin("admin3", new CountCallTetriNETAdminCallback());
            IAdminManager adminManager = CreateAdminManager(10);
            adminManager.Add(admin1);
            adminManager.Add(admin2);
            adminManager.Add(admin3);

            IAdmin searched = adminManager[admin2.Callback];

            Assert.IsNotNull(searched);
            Assert.AreEqual(searched, admin2);
        }

        [TestMethod]
        public void TestIndexerCallbackFindNonExistingAdmin()
        {
            IAdmin admin1 = CreateAdmin("admin1", new CountCallTetriNETAdminCallback());
            IAdmin admin2 = CreateAdmin("admin2", new CountCallTetriNETAdminCallback());
            IAdmin admin3 = CreateAdmin("admin3", new CountCallTetriNETAdminCallback());
            IAdminManager adminManager = CreateAdminManager(10);
            adminManager.Add(admin1);
            adminManager.Add(admin2);
            adminManager.Add(admin3);

            IAdmin searched = adminManager[new CountCallTetriNETAdminCallback()];

            Assert.IsNull(searched);
        }

        [TestMethod]
        public void TestIndexerAddressFindExistingAdmin()
        {
            IAdmin admin1 = CreateAdmin("admin1", new CountCallTetriNETAdminCallback(), "127.0.0.1");
            IAdmin admin2 = CreateAdmin("admin2", new CountCallTetriNETAdminCallback(), "127.0.0.2");
            IAdmin admin3 = CreateAdmin("admin3", new CountCallTetriNETAdminCallback(), "127.0.0.3");
            IAdminManager adminManager = CreateAdminManager(10);
            adminManager.Add(admin1);
            adminManager.Add(admin2);
            adminManager.Add(admin3);

            IAdmin searched = adminManager[admin2.Address];

            Assert.IsNotNull(searched);
            Assert.AreEqual(searched, admin2);
        }

        [TestMethod]
        public void TestIndexerAddressFindNonExistingAdmin()
        {
            IAdmin admin1 = CreateAdmin("admin1", new CountCallTetriNETAdminCallback());
            IAdmin admin2 = CreateAdmin("admin2", new CountCallTetriNETAdminCallback());
            IAdmin admin3 = CreateAdmin("admin3", new CountCallTetriNETAdminCallback());
            IAdminManager adminManager = CreateAdminManager(10);
            adminManager.Add(admin1);
            adminManager.Add(admin2);
            adminManager.Add(admin3);

            IAdmin searched = adminManager[IPAddress.None];

            Assert.IsNull(searched);
        }

        #endregion
    }

    [TestClass]
    public class AdminManagerUnitTest : AbstractAdminManagerUnitTest
    {
        protected override IAdminManager CreateAdminManager(int maxAdmins)
        {
            return new AdminManager(maxAdmins);
        }

        protected override IAdmin CreateAdmin(string name, ITetriNETAdminCallback callback, string address = null)
        {
            return new Admin(name, address == null ? IPAddress.Any : IPAddress.Parse(address), callback);
        }

        #region Constructor

        [TestMethod]
        public void TestConstructorStrictlyPositiveMaxAdmins()
        {
            try
            {
                IAdminManager adminManager = CreateAdminManager(0);
                Assert.Fail("ArgumentOutOfRange exception not raised");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.AreEqual("maxAdmins", ex.ParamName);
            }
        }

        [TestMethod]
        public void TestConstructorSetProperties()
        {
            const int maxAdmins = 10;

            IAdminManager adminManager = CreateAdminManager(maxAdmins);

            Assert.AreEqual(maxAdmins, adminManager.MaxAdmins);
        }

        [TestMethod]
        public void TestConstructorLockObjectNotNull()
        {
            IAdminManager adminManager = CreateAdminManager(10);

            Assert.IsNotNull(adminManager.LockObject);
        }

        #endregion

    }
}
