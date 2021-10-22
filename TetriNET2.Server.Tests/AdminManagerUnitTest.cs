using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.Logger;
using TetriNET2.Server.Interfaces;
using TetriNET2.Server.Tests.Mocking;

namespace TetriNET2.Server.Tests
{
    [TestClass]
    public abstract class AbstractAdminManagerUnitTest
    {
        public class Settings : ISettings
        {
            public int MaxAdmins { get; }
            public int MaxClients { get; }
            public int MaxGames { get; }
            public string BanFilename { get; }

            public Settings(int maxAdmins)
            {
                MaxAdmins = maxAdmins;
            }
        }

        protected abstract IAdminManager CreateAdminManager(ISettings settings);
        protected abstract IAdmin CreateAdmin(string name, ITetriNETAdminCallback callback, string address = null);

        [TestInitialize]
        public void Initialize()
        {
            Log.Default.Logger = new LogMock();
        }

        #region Add

        [TestCategory("Server")]
        [TestCategory("Server.IAdminManager")]
        [TestCategory("Server.IAdminManager.Add")]
        [TestMethod]
        public void TestAddNullAdmin()
        {
            IAdminManager adminManager = CreateAdminManager(new Settings(10));

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
            Assert.AreEqual(0, adminManager.Admins.Count);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IAdminManager")]
        [TestCategory("Server.IAdminManager.Add")]
        [TestMethod]
        public void TestAddNoMaxAdmins()
        {
            IAdminManager adminManager = CreateAdminManager(new Settings(10));

            bool inserted1 = adminManager.Add(CreateAdmin("admin1", new CountCallTetriNETAdminCallback()));
            bool inserted2 = adminManager.Add(CreateAdmin("admin2", new CountCallTetriNETAdminCallback()));

            Assert.IsTrue(inserted1);
            Assert.IsTrue(inserted2);
            Assert.AreEqual(2, adminManager.AdminCount);
            Assert.AreEqual(2, adminManager.Admins.Count);
            Assert.IsTrue(adminManager.Admins.Any(x => x.Name == "admin1") && adminManager.Admins.Any(x => x.Name == "admin2"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IAdminManager")]
        [TestCategory("Server.IAdminManager.Add")]
        [TestMethod]
        public void TestAddWithMaxAdmins()
        {
            IAdminManager adminManager = CreateAdminManager(new Settings(1));
            adminManager.Add(CreateAdmin("admin1", new CountCallTetriNETAdminCallback()));

            bool inserted = adminManager.Add(CreateAdmin("admin2", new CountCallTetriNETAdminCallback()));

            Assert.IsFalse(inserted);
            Assert.AreEqual(1, adminManager.AdminCount);
            Assert.IsTrue(adminManager.Admins.First().Name == "admin1");
        }

        [TestCategory("Server")]
        [TestCategory("Server.IAdminManager")]
        [TestCategory("Server.IAdminManager.Add")]
        [TestMethod]
        public void TestAddSameAdmin()
        {
            IAdmin admin1 = CreateAdmin("admin1", new CountCallTetriNETAdminCallback());
            IAdminManager adminManager = CreateAdminManager(new Settings(10));
            adminManager.Add(admin1);

            bool inserted = adminManager.Add(admin1);

            Assert.IsFalse(inserted);
            Assert.AreEqual(1, adminManager.AdminCount);
            Assert.AreEqual(1, adminManager.Admins.Count);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IAdminManager")]
        [TestCategory("Server.IAdminManager.Add")]
        [TestMethod]
        public void TestAddSameName()
        {
            IAdminManager adminManager = CreateAdminManager(new Settings(10));
            adminManager.Add(CreateAdmin("admin1", new CountCallTetriNETAdminCallback()));

            bool inserted = adminManager.Add(CreateAdmin("admin1", new CountCallTetriNETAdminCallback()));

            Assert.IsFalse(inserted);
            Assert.AreEqual(1, adminManager.AdminCount);
            Assert.AreEqual(1, adminManager.Admins.Count);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IAdminManager")]
        [TestCategory("Server.IAdminManager.Add")]
        [TestMethod]
        public void TestAddSameCallback()
        {
            IAdminManager adminManager = CreateAdminManager(new Settings(10));
            ITetriNETAdminCallback callback = new CountCallTetriNETAdminCallback();
            adminManager.Add(CreateAdmin("admin1", callback));

            bool inserted = adminManager.Add(CreateAdmin("admin2", callback));

            Assert.IsFalse(inserted);
            Assert.AreEqual(1, adminManager.AdminCount);
            Assert.AreEqual(1, adminManager.Admins.Count);
        }

        #endregion

        #region Remove

        [TestCategory("Server")]
        [TestCategory("Server.IAdminManager")]
        [TestCategory("Server.IAdminManager.Remove")]
        [TestMethod]
        public void TestRemoveExistingAdmin()
        {
            IAdminManager adminManager = CreateAdminManager(new Settings(10));
            IAdmin admin = CreateAdmin("admin1", new CountCallTetriNETAdminCallback());
            adminManager.Add(admin);

            bool removed = adminManager.Remove(admin);

            Assert.IsTrue(removed);
            Assert.AreEqual(0, adminManager.AdminCount);
            Assert.AreEqual(0, adminManager.Admins.Count);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IAdminManager")]
        [TestCategory("Server.IAdminManager.Remove")]
        [TestMethod]
        public void TestRemoveNonExistingAdmin()
        {
            IAdminManager adminManager = CreateAdminManager(new Settings(10));
            IAdmin admin1 = CreateAdmin("admin1", new CountCallTetriNETAdminCallback());
            IAdmin admin2 = CreateAdmin("admin2", new CountCallTetriNETAdminCallback());
            adminManager.Add(admin1);

            bool removed = adminManager.Remove(admin2);

            Assert.IsFalse(removed);
            Assert.AreEqual(1, adminManager.AdminCount);
            Assert.AreEqual(1, adminManager.Admins.Count);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IAdminManager")]
        [TestCategory("Server.IAdminManager.Remove")]
        [TestMethod]
        public void TestRemoveNullAdmin()
        {
            IAdminManager adminManager = CreateAdminManager(new Settings(10));
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
            Assert.AreEqual(1, adminManager.Admins.Count);
        }

        #endregion

        #region Clear

        [TestCategory("Server")]
        [TestCategory("Server.IAdminManager")]
        [TestCategory("Server.IAdminManager.Clear")]
        [TestMethod]
        public void TestClearNoAdmins()
        {
            IAdminManager adminManager = CreateAdminManager(new Settings(10));

            adminManager.Clear();

            Assert.AreEqual(0, adminManager.AdminCount);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IAdminManager")]
        [TestCategory("Server.IAdminManager.Clear")]
        [TestMethod]
        public void TestClearSomeAdmins()
        {
            IAdminManager adminManager = CreateAdminManager(new Settings(10));
            adminManager.Add(CreateAdmin("admin1", new CountCallTetriNETAdminCallback()));
            adminManager.Add(CreateAdmin("admin2", new CountCallTetriNETAdminCallback()));
            adminManager.Add(CreateAdmin("admin3", new CountCallTetriNETAdminCallback()));

            adminManager.Clear();

            Assert.AreEqual(0, adminManager.AdminCount);
        }

        #endregion

        #region Contains

        [TestCategory("Server")]
        [TestCategory("Server.IAdminManager")]
        [TestCategory("Server.IAdminManager.Contains")]
        [TestMethod]
        public void TestContainsExistingAdmin()
        {
            IAdminManager adminManager = CreateAdminManager(new Settings(10));
            IAdmin admin = CreateAdmin("admin1", new CountCallTetriNETAdminCallback());
            adminManager.Add(admin);

            bool containsOnName = adminManager.Contains(admin.Name, null);
            bool containsOnCallback = adminManager.Contains(null, admin.Callback);

            Assert.IsTrue(containsOnName);
            Assert.IsTrue(containsOnCallback);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IAdminManager")]
        [TestCategory("Server.IAdminManager.Contains")]
        [TestMethod]
        public void TestContainsNonExistingAdmin()
        {
            IAdminManager adminManager = CreateAdminManager(new Settings(10));
            IAdmin admin = CreateAdmin("admin1", new CountCallTetriNETAdminCallback());
            adminManager.Add(admin);

            bool containsOnName = adminManager.Contains("admin2", null);
            bool containsOnCallback = adminManager.Contains(null, new CountCallTetriNETAdminCallback());

            Assert.IsFalse(containsOnName);
            Assert.IsFalse(containsOnCallback);
        }

        #endregion

        #region Indexers

        [TestCategory("Server")]
        [TestCategory("Server.IAdminManager")]
        [TestCategory("Server.IAdminManager.Indexers")]
        [TestMethod]
        public void TestIndexerGuidFindExistingAdmin()
        {
            IAdmin admin1 = CreateAdmin("admin1", new CountCallTetriNETAdminCallback());
            IAdmin admin2 = CreateAdmin("admin2", new CountCallTetriNETAdminCallback());
            IAdmin admin3 = CreateAdmin("admin3", new CountCallTetriNETAdminCallback());
            IAdminManager adminManager = CreateAdminManager(new Settings(10));
            adminManager.Add(admin1);
            adminManager.Add(admin2);
            adminManager.Add(admin3);

            IAdmin searched = adminManager[admin2.Id];

            Assert.IsNotNull(searched);
            Assert.AreEqual(searched, admin2);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IAdminManager")]
        [TestCategory("Server.IAdminManager.Indexers")]
        [TestMethod]
        public void TestIndexerGuidFindNonExistingAdmin()
        {
            IAdmin admin1 = CreateAdmin("admin1", new CountCallTetriNETAdminCallback());
            IAdmin admin2 = CreateAdmin("admin2", new CountCallTetriNETAdminCallback());
            IAdmin admin3 = CreateAdmin("admin3", new CountCallTetriNETAdminCallback());
            IAdminManager adminManager = CreateAdminManager(new Settings(10));
            adminManager.Add(admin1);
            adminManager.Add(admin2);
            adminManager.Add(admin3);

            IAdmin searched = adminManager[Guid.Empty];

            Assert.IsNull(searched);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IAdminManager")]
        [TestCategory("Server.IAdminManager.Indexers")]
        [TestMethod]
        public void TestIndexerNameFindExistingAdmin()
        {
            IAdmin admin1 = CreateAdmin("admin1", new CountCallTetriNETAdminCallback());
            IAdmin admin2 = CreateAdmin("admin2", new CountCallTetriNETAdminCallback());
            IAdmin admin3 = CreateAdmin("admin3", new CountCallTetriNETAdminCallback());
            IAdminManager adminManager = CreateAdminManager(new Settings(10));
            adminManager.Add(admin1);
            adminManager.Add(admin2);
            adminManager.Add(admin3);

            IAdmin searched = adminManager["admin2"];

            Assert.IsNotNull(searched);
            Assert.AreEqual(searched, admin2);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IAdminManager")]
        [TestCategory("Server.IAdminManager.Indexers")]
        [TestMethod]
        public void TesIndexerNameFindNonExistingAdmin()
        {
            IAdmin admin1 = CreateAdmin("admin1", new CountCallTetriNETAdminCallback());
            IAdmin admin2 = CreateAdmin("admin2", new CountCallTetriNETAdminCallback());
            IAdmin admin3 = CreateAdmin("admin3", new CountCallTetriNETAdminCallback());
            IAdminManager adminManager = CreateAdminManager(new Settings(10));
            adminManager.Add(admin1);
            adminManager.Add(admin2);
            adminManager.Add(admin3);

            IAdmin searched = adminManager["admin4"];

            Assert.IsNull(searched);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IAdminManager")]
        [TestCategory("Server.IAdminManager.Indexers")]
        [TestMethod]
        public void TestIndexerCallbackFindExistingAdmin()
        {
            IAdmin admin1 = CreateAdmin("admin1", new CountCallTetriNETAdminCallback());
            IAdmin admin2 = CreateAdmin("admin2", new CountCallTetriNETAdminCallback());
            IAdmin admin3 = CreateAdmin("admin3", new CountCallTetriNETAdminCallback());
            IAdminManager adminManager = CreateAdminManager(new Settings(10));
            adminManager.Add(admin1);
            adminManager.Add(admin2);
            adminManager.Add(admin3);

            IAdmin searched = adminManager[admin2.Callback];

            Assert.IsNotNull(searched);
            Assert.AreEqual(searched, admin2);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IAdminManager")]
        [TestCategory("Server.IAdminManager.Indexers")]
        [TestMethod]
        public void TestIndexerCallbackFindNonExistingAdmin()
        {
            IAdmin admin1 = CreateAdmin("admin1", new CountCallTetriNETAdminCallback());
            IAdmin admin2 = CreateAdmin("admin2", new CountCallTetriNETAdminCallback());
            IAdmin admin3 = CreateAdmin("admin3", new CountCallTetriNETAdminCallback());
            IAdminManager adminManager = CreateAdminManager(new Settings(10));
            adminManager.Add(admin1);
            adminManager.Add(admin2);
            adminManager.Add(admin3);

            IAdmin searched = adminManager[new CountCallTetriNETAdminCallback()];

            Assert.IsNull(searched);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IAdminManager")]
        [TestCategory("Server.IAdminManager.Indexers")]
        [TestMethod]
        public void TestIndexerAddressFindExistingAdmin()
        {
            IAdmin admin1 = CreateAdmin("admin1", new CountCallTetriNETAdminCallback(), "127.0.0.1");
            IAdmin admin2 = CreateAdmin("admin2", new CountCallTetriNETAdminCallback(), "127.0.0.2");
            IAdmin admin3 = CreateAdmin("admin3", new CountCallTetriNETAdminCallback(), "127.0.0.3");
            IAdminManager adminManager = CreateAdminManager(new Settings(10));
            adminManager.Add(admin1);
            adminManager.Add(admin2);
            adminManager.Add(admin3);

            IAdmin searched = adminManager[admin2.Address];

            Assert.IsNotNull(searched);
            Assert.AreEqual(searched, admin2);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IAdminManager")]
        [TestCategory("Server.IAdminManager.Indexers")]
        [TestMethod]
        public void TestIndexerAddressFindNonExistingAdmin()
        {
            IAdmin admin1 = CreateAdmin("admin1", new CountCallTetriNETAdminCallback());
            IAdmin admin2 = CreateAdmin("admin2", new CountCallTetriNETAdminCallback());
            IAdmin admin3 = CreateAdmin("admin3", new CountCallTetriNETAdminCallback());
            IAdminManager adminManager = CreateAdminManager(new Settings(10));
            adminManager.Add(admin1);
            adminManager.Add(admin2);
            adminManager.Add(admin3);

            IAdmin searched = adminManager[AddressMock.Any];

            Assert.IsNull(searched);
        }

        #endregion
    }

    [TestClass]
    public class AdminManagerUnitTest : AbstractAdminManagerUnitTest
    {
        protected override IAdminManager CreateAdminManager(ISettings settings)
        {
            return new AdminManager(settings);
        }

        protected override IAdmin CreateAdmin(string name, ITetriNETAdminCallback callback, string address = null)
        {
            return new Admin(name, new AddressMock(address), callback);
        }

        #region Constructor

        [TestCategory("Server")]
        [TestCategory("Server.AdminManager")]
        [TestCategory("Server.AdminManager.ctor")]
        [TestMethod]
        public void TestConstructorStrictlyPositiveMaxAdmins()
        {
            try
            {
                IAdminManager adminManager = CreateAdminManager(new Settings(0));
                Assert.Fail("ArgumentOutOfRange exception not raised");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.AreEqual("maxAdmins", ex.ParamName);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.AdminManager")]
        [TestCategory("Server.AdminManager.ctor")]
        [TestMethod]
        public void TestConstructorSetProperties()
        {
            const int maxAdmins = 10;

            IAdminManager adminManager = CreateAdminManager(new Settings(maxAdmins));

            Assert.AreEqual(maxAdmins, adminManager.MaxAdmins);
        }

        [TestCategory("Server")]
        [TestCategory("Server.AdminManager")]
        [TestCategory("Server.AdminManager.ctor")]
        [TestMethod]
        public void TestConstructorLockObjectNotNull()
        {
            IAdminManager adminManager = CreateAdminManager(new Settings(10));

            Assert.IsNotNull(adminManager.LockObject);
        }

        #endregion
    }
}
