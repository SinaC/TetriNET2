using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET2.Common.Logger;
using TetriNET2.Server;
using TetriNET2.Server.Interfaces;
using TetriNET2.Tests.Server.Mocking;

namespace TetriNET2.Tests.Server
{
    [TestClass]
    public abstract class AbstractBanManagerUnitTest
    {
        protected abstract IBanManager CreateBanManager();

        [TestInitialize]
        public void Initialize()
        {
            Log.SetLogger(new LogMock());
        }

        [TestMethod]
        public void TestIsBannedFalseWhenNoBannedPlayers()
        {
            IBanManager banManager = CreateBanManager();

            bool isBanned = banManager.IsBanned(IPAddress.Parse("127.0.0.1"));

            Assert.IsFalse(isBanned);
        }

        [TestMethod]
        public void TestIsBannedTrueWhenBannedPlayers()
        {
            IBanManager banManager = CreateBanManager();
            banManager.Ban("player1", IPAddress.Parse("127.0.0.1"), "spam");
            banManager.Ban("player2", IPAddress.Parse("127.0.0.2"), "spam");

            bool isBanned = banManager.IsBanned(IPAddress.Parse("127.0.0.1"));

            Assert.IsTrue(isBanned);
        }

        [TestMethod]
        public void TestIsBannedFalseOnUnknownAddress()
        {
            IBanManager banManager = CreateBanManager();
            banManager.Ban("player1", IPAddress.Parse("127.0.0.1"), "spam");
            banManager.Ban("player2", IPAddress.Parse("127.0.0.2"), "spam");

            bool isBanned = banManager.IsBanned(IPAddress.Parse("127.1.1.1"));

            Assert.IsFalse(isBanned);
        }

        [TestMethod]
        public void TestBannedReasonOnBannedPlayers()
        {
            const string reason = "spam";
            IBanManager banManager = CreateBanManager();
            banManager.Ban("player1", IPAddress.Parse("127.0.0.1"), reason);

            string bannedReason = banManager.BannedReason(IPAddress.Parse("127.0.0.1"));

            Assert.AreEqual(bannedReason, reason);
        }

        [TestMethod]
        public void TestBannedReasonOnUnknownAddress()
        {
            IBanManager banManager = CreateBanManager();
            banManager.Ban("player1", IPAddress.Parse("127.0.0.1"), "spam");

            string bannedReason = banManager.BannedReason(IPAddress.Parse("127.1.1.1"));

            Assert.IsNull(bannedReason);
        }
    }

    [TestClass]
    public class BanManagerUnitTest : AbstractBanManagerUnitTest
    {
        protected override IBanManager CreateBanManager()
        {
            return new BanManager(@"d:\\temp\\banmanagerunittest.log");
        }
    }
}
