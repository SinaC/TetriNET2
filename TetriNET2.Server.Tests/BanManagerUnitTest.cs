using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Logger;
using TetriNET2.Server.Interfaces;
using TetriNET2.Server.Tests.Mocking;

namespace TetriNET2.Server.Tests
{
    [TestClass]
    public abstract class AbstractBanManagerUnitTest
    {
        public class Settings : ISettings
        {
            public int MaxAdmins { get; }
            public int MaxClients { get; }
            public int MaxGames { get; }
            public string BanFilename => @"c:\temp\banmanagerunittest.lst";
        }

        protected abstract IBanManager CreateBanManager(ISettings settings);

        [TestInitialize]
        public void Initialize()
        {
            Log.Default.Logger = new LogMock();
        }

        #region IsBanned

        [TestCategory("Server")]
        [TestCategory("Server.IBanManager")]
        [TestCategory("Server.IBanManager.IsBanned")]
        [TestMethod]
        public void TestIsBannedFalseWhenNoBannedPlayers()
        {
            IBanManager banManager = CreateBanManager(new Settings());
            banManager.Clear();

            bool isBanned = banManager.IsBanned(new AddressMock("127.0.0.1"));

            Assert.IsFalse(isBanned);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IBanManager")]
        [TestCategory("Server.IBanManager.IsBanned")]
        [TestMethod]
        public void TestIsBannedTrueWhenBannedPlayers()
        {
            IBanManager banManager = CreateBanManager(new Settings());
            banManager.Clear();
            banManager.Ban("player1", new AddressMock("127.0.0.1"), "spam");
            banManager.Ban("player2", new AddressMock("127.0.0.2"), "spam");

            bool isBanned = banManager.IsBanned(new AddressMock("127.0.0.1"));

            Assert.IsTrue(isBanned);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IBanManager")]
        [TestCategory("Server.IBanManager.IsBanned")]
        [TestMethod]
        public void TestIsBannedFalseOnUnknownAddress()
        {
            IBanManager banManager = CreateBanManager(new Settings());
            banManager.Clear();
            banManager.Ban("player1", new AddressMock("127.0.0.1"), "spam");
            banManager.Ban("player2", new AddressMock("127.0.0.2"), "spam");

            bool isBanned = banManager.IsBanned(new AddressMock("127.1.1.1"));

            Assert.IsFalse(isBanned);
        }

        #endregion

        #region BannedReason

        [TestCategory("Server")]
        [TestCategory("Server.IBanManager")]
        [TestCategory("Server.IBanManager.BannedReason")]
        [TestMethod]
        public void TestBannedReasonOnBannedPlayers()
        {
            const string reason = "spam";
            IBanManager banManager = CreateBanManager(new Settings());
            banManager.Clear();
            banManager.Ban("player1", new AddressMock("127.0.0.1"), reason);

            string bannedReason = banManager.BannedReason(new AddressMock("127.0.0.1"));

            Assert.AreEqual(reason, bannedReason);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IBanManager")]
        [TestCategory("Server.IBanManager.BannedReason")]
        [TestMethod]
        public void TestBannedReasonOnUnknownAddress()
        {
            IBanManager banManager = CreateBanManager(new Settings());
            banManager.Clear();
            banManager.Ban("player1", new AddressMock("127.0.0.1"), "spam");

            string bannedReason = banManager.BannedReason(new AddressMock("127.1.1.1"));

            Assert.IsNull(bannedReason);
        }

        #endregion

        [TestCategory("Server")]
        [TestCategory("Server.IBanManager")]
        [TestCategory("Server.IBanManager.Entries")]
        [TestMethod]
        public void TestEntries()
        {
            IBanManager banManager = CreateBanManager(new Settings());
            banManager.Clear();
            banManager.Ban("player1", new AddressMock("127.0.0.1"), "spam");
            banManager.Ban("player2", new AddressMock("127.0.0.2"), "spam");

            List<BanEntryData> entries = banManager.Entries.ToList();

            Assert.IsNotNull(entries);
            Assert.AreEqual(2, entries.Count);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IBanManager")]
        [TestCategory("Server.IBanManager.Ban")]
        [TestMethod]
        public void TestBan2IdenticalAddress()
        {
            IBanManager banManager = CreateBanManager(new Settings());
            banManager.Clear();
            banManager.Ban("player1", new AddressMock("127.0.0.1"), "spam");
            banManager.Ban("player2", new AddressMock("127.0.0.1"), "spam");

            List<BanEntryData> entries = banManager.Entries.ToList();

            Assert.AreEqual(1, entries.Count);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IBanManager")]
        [TestCategory("Server.IBanManager.Clear")]
        [TestMethod]
        public void TestClear()
        {
            IBanManager banManager = CreateBanManager(new Settings());
            banManager.Ban("player1", new AddressMock("127.0.0.1"), "spam");
            banManager.Ban("player2", new AddressMock("127.0.0.2"), "spam");
            banManager.Clear();

            List<BanEntryData> entries = banManager.Entries.ToList();

            Assert.AreEqual(0, entries.Count);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IBanManager")]
        [TestCategory("Server.IBanManager.Load/Save")]
        [TestMethod]
        public void TestInternalLoadSave()
        {
            IBanManager banManager = CreateBanManager(new Settings());
            banManager.Ban("player1", new AddressMock("127.0.0.1"), "spam");
            banManager.Ban("player2", new AddressMock("127.0.0.2"), "spam");

            IBanManager banManager2 = CreateBanManager(new Settings());
            List<BanEntryData> entries = banManager2.Entries.ToList();

            Assert.AreEqual(banManager.Entries.Count, entries.Count);
        }
    }

    [TestClass]
    public class BanManagerUnitTest : AbstractBanManagerUnitTest
    {
        protected override IBanManager CreateBanManager(ISettings settings)
        {
            return new BanManager(settings);
        }

        #region Constructor

        [TestCategory("Server")]
        [TestCategory("Server.BanManager")]
        [TestCategory("Server.BanManager.ctor")]
        [TestMethod]
        public void TestConstructorNullFilename()
        {
            try
            {
                IBanManager banManager = CreateBanManager(null);

                Assert.Fail("ArgumentNullException on name not raised");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("settings", ex.ParamName);
            }
        }

        #endregion
    }
}
