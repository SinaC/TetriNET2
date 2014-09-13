using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Logger;
using TetriNET2.Server;
using TetriNET2.Server.Interfaces;
using TetriNET2.Tests.Server.Mocking;

namespace TetriNET2.Tests.Server
{
    // TODO: invert parameters in Assert.AreEqual and Assert.AreNotEqual

    [TestClass]
    public abstract class AbstractBanManagerUnitTest
    {
        protected abstract IBanManager CreateBanManager(string filename = @"d:\temp\banmanagerunittest.lst");

        [TestInitialize]
        public void Initialize()
        {
            Log.Default.Logger = new LogMock();
        }

        #region Constructor

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
                Assert.AreEqual(ex.ParamName, "filename");
            }
        }

        #endregion

        #region IsBanned

        [TestMethod]
        public void TestIsBannedFalseWhenNoBannedPlayers()
        {
            IBanManager banManager = CreateBanManager();
            banManager.Clear();

            bool isBanned = banManager.IsBanned(IPAddress.Parse("127.0.0.1"));

            Assert.IsFalse(isBanned);
        }

        [TestMethod]
        public void TestIsBannedTrueWhenBannedPlayers()
        {
            IBanManager banManager = CreateBanManager();
            banManager.Clear();
            banManager.Ban("player1", IPAddress.Parse("127.0.0.1"), "spam");
            banManager.Ban("player2", IPAddress.Parse("127.0.0.2"), "spam");

            bool isBanned = banManager.IsBanned(IPAddress.Parse("127.0.0.1"));

            Assert.IsTrue(isBanned);
        }

        [TestMethod]
        public void TestIsBannedFalseOnUnknownAddress()
        {
            IBanManager banManager = CreateBanManager();
            banManager.Clear();
            banManager.Ban("player1", IPAddress.Parse("127.0.0.1"), "spam");
            banManager.Ban("player2", IPAddress.Parse("127.0.0.2"), "spam");

            bool isBanned = banManager.IsBanned(IPAddress.Parse("127.1.1.1"));

            Assert.IsFalse(isBanned);
        }

        #endregion

        #region BannedReason

        [TestMethod]
        public void TestBannedReasonOnBannedPlayers()
        {
            const string reason = "spam";
            IBanManager banManager = CreateBanManager();
            banManager.Clear();
            banManager.Ban("player1", IPAddress.Parse("127.0.0.1"), reason);

            string bannedReason = banManager.BannedReason(IPAddress.Parse("127.0.0.1"));

            Assert.AreEqual(bannedReason, reason);
        }

        [TestMethod]
        public void TestBannedReasonOnUnknownAddress()
        {
            IBanManager banManager = CreateBanManager();
            banManager.Clear();
            banManager.Ban("player1", IPAddress.Parse("127.0.0.1"), "spam");

            string bannedReason = banManager.BannedReason(IPAddress.Parse("127.1.1.1"));

            Assert.IsNull(bannedReason);
        }

        #endregion

        [TestMethod]
        public void TestEntries()
        {
            IBanManager banManager = CreateBanManager();
            banManager.Clear();
            banManager.Ban("player1", IPAddress.Parse("127.0.0.1"), "spam");
            banManager.Ban("player2", IPAddress.Parse("127.0.0.2"), "spam");

            List<BanEntryData> entries = banManager.Entries.ToList();

            Assert.IsNotNull(entries);
            Assert.AreEqual(entries.Count, 2);
        }

        [TestMethod]
        public void TestBan2IdenticalAddress()
        {
            IBanManager banManager = CreateBanManager();
            banManager.Clear();
            banManager.Ban("player1", IPAddress.Parse("127.0.0.1"), "spam");
            banManager.Ban("player2", IPAddress.Parse("127.0.0.1"), "spam");

            List<BanEntryData> entries = banManager.Entries.ToList();

            Assert.AreEqual(entries.Count, 1);
        }

        [TestMethod]
        public void TestClear()
        {
            IBanManager banManager = CreateBanManager();
            banManager.Ban("player1", IPAddress.Parse("127.0.0.1"), "spam");
            banManager.Ban("player2", IPAddress.Parse("127.0.0.2"), "spam");
            banManager.Clear();

            List<BanEntryData> entries = banManager.Entries.ToList();

            Assert.AreEqual(entries.Count, 0);
        }

        [TestMethod]
        public void TestInternalLoadSave()
        {
            IBanManager banManager = CreateBanManager();
            banManager.Ban("player1", IPAddress.Parse("127.0.0.1"), "spam");
            banManager.Ban("player2", IPAddress.Parse("127.0.0.2"), "spam");

            IBanManager banManager2 = CreateBanManager();
            List<BanEntryData> entries = banManager2.Entries.ToList();

            Assert.AreEqual(entries.Count, banManager.Entries.Count());
        }
    }

    [TestClass]
    public class BanManagerUnitTest : AbstractBanManagerUnitTest
    {
        protected override IBanManager CreateBanManager(string filename = @"d:\temp\banmanagerunittest.lst")
        {
            return new BanManager(filename);
        }
    }
}
