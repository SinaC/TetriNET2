using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET2.Admin.Interfaces;
using TetriNET2.Admin.Tests.Mocking;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Logger;

namespace TetriNET2.Admin.Tests
{
    [TestClass]
    public abstract class AbstractAdminUnitTest
    {
        protected abstract IAdmin CreateAdmin();
        protected abstract ICallCount ProxyCallCount { get; }
        protected ILogMock LogMock { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            LogMock log = new LogMock();
            Log.Default.Logger = log;
            LogMock = log;
        }

        [TestCategory("Admin")]
        [TestCategory("Admin.IAdmin")]
        [TestMethod]
        public void TestSetVersion()
        {
            IAdmin admin = CreateAdmin();

            admin.SetVersion(5, 2);

            Assert.AreEqual(admin.Version.Major, 5);
            Assert.AreEqual(admin.Version.Minor, 2);
        }

        [TestCategory("Admin")]
        [TestCategory("Admin.IAdmin")]
        [TestCategory("Admin.IAdmin.Connect")]
        [TestMethod]
        public void TestConnectNoAddress()
        {
            IAdmin admin = CreateAdmin();

            try
            {
                admin.Connect(null, "admin1", "password1");

                Assert.Fail("Exception not thrown");
            }
            catch(ArgumentNullException ex)
            {
                Assert.AreEqual(ex.ParamName, "address");
            }
        }

        [TestCategory("Admin")]
        [TestCategory("Admin.IAdmin")]
        [TestCategory("Admin.IAdmin.Connect")]
        [TestMethod]
        public void TestConnectNoName()
        {
            IAdmin admin = CreateAdmin();

            try
            {
                admin.Connect("127.0.0.1", null, "password1");

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual(ex.ParamName, "name");
            }
        }

        [TestCategory("Admin")]
        [TestCategory("Admin.IAdmin")]
        [TestCategory("Admin.IAdmin.Connect")]
        [TestMethod]
        public void TestConnectNoVersion()
        {
            IAdmin admin = CreateAdmin();

            bool connected = admin.Connect("127.0.0.1", "admin1", "password1");

            Assert.IsFalse(connected);
            Assert.AreEqual(LogMock.LastLogLevel, LogLevels.Error);
            Assert.IsTrue(LogMock.LastLogLine.Contains("Cannot connect, version is not set"));
        }

        [TestCategory("Admin")]
        [TestCategory("Admin.IAdmin")]
        [TestCategory("Admin.IAdmin.Connect")]
        [TestMethod]
        public void TestConnectDoubleCall()
        {
            IAdmin admin = CreateAdmin();
            admin.SetVersion(1, 0);
            admin.Connect("127.0.0.1", "admin1", "password1");

            bool connected = admin.Connect("127.0.0.1", "admin1", "password1");

            Assert.IsFalse(connected);
            Assert.AreEqual(LogMock.LastLogLevel, LogLevels.Error);
            Assert.IsTrue(LogMock.LastLogLine.Contains("Proxy already created, must disconnect before reconnecting"));
        }

        [TestCategory("Admin")]
        [TestCategory("Admin.IAdmin")]
        [TestCategory("Admin.IAdmin.Connect")]
        [TestMethod]
        public void TestConnect()
        {
            IAdmin admin = CreateAdmin();
            admin.SetVersion(1, 0);

            bool connected = admin.Connect("127.0.0.1", "admin1", "password1");

            Assert.IsTrue(connected);
            Assert.AreEqual(ProxyCallCount.GetCallCount("AdminConnect"), 1);
        }

        [TestCategory("Admin")]
        [TestCategory("Admin.IAdmin")]
        [TestCategory("Admin.IAdmin.Disconnect")]
        [TestMethod]
        public void TestDisconnectNotConnected()
        {
            IAdmin admin = CreateAdmin();

            bool disconnected = admin.Disconnect();

            Assert.IsFalse(disconnected);
        }

        [TestCategory("Admin")]
        [TestCategory("Admin.IAdmin")]
        [TestCategory("Admin.IAdmin.Disconnect")]
        [TestMethod]
        public void TestDisconnect()
        {
            IAdmin admin = CreateAdmin();
            admin.SetVersion(1, 0);
            admin.Connect("127.0.0.1", "admin1", "password1");

            bool disconnected = admin.Disconnect();

            Assert.IsTrue(disconnected);
            Assert.AreEqual(ProxyCallCount.GetCallCount("AdminDisconnect"), 1);
        }

        [TestCategory("Admin")]
        [TestCategory("Admin.IAdmin")]
        [TestCategory("Admin.IAdmin.SendPrivateAdminMessage")]
        [TestMethod]
        public void TestSendPrivateAdminMessageNotConnected()
        {
            IAdmin admin = CreateAdmin();

            bool sent = admin.SendPrivateAdminMessage(Guid.NewGuid(), "msg");

            Assert.IsFalse(sent);
            Assert.AreEqual(LogMock.LastLogLevel, LogLevels.Error);
            Assert.IsTrue(LogMock.LastLogLine.Contains("Proxy"));
        }

        [TestCategory("Admin")]
        [TestCategory("Admin.IAdmin")]
        [TestCategory("Admin.IAdmin.SendPrivateAdminMessage")]
        [TestMethod]
        public void TestSendPrivateAdminMessageEmptyGuid()
        {
            IAdmin admin = CreateAdmin();
            admin.SetVersion(1, 0);
            admin.Connect("127.0.0.1", "admin1", "password1");

            bool sent = admin.SendPrivateAdminMessage(Guid.Empty, "msg");

            Assert.IsFalse(sent);
            Assert.AreEqual(LogMock.LastLogLevel, LogLevels.Error);
            Assert.IsTrue(LogMock.LastLogLine.Contains("unknown target"));
        }

        [TestCategory("Admin")]
        [TestCategory("Admin.IAdmin")]
        [TestCategory("Admin.IAdmin.SendPrivateAdminMessage")]
        [TestMethod]
        public void TestSendPrivateAdminMessageNoMessage()
        {
            IAdmin admin = CreateAdmin();
            admin.SetVersion(1, 0);
            admin.Connect("127.0.0.1", "admin1", "password1");

            bool sent = admin.SendPrivateAdminMessage(Guid.NewGuid(), null);

            Assert.IsFalse(sent);
            Assert.AreEqual(LogMock.LastLogLevel, LogLevels.Error);
            Assert.IsTrue(LogMock.LastLogLine.Contains("empty message"));
        }

        [TestCategory("Admin")]
        [TestCategory("Admin.IAdmin")]
        [TestCategory("Admin.IAdmin.SendPrivateAdminMessage")]
        [TestMethod]
        public void TestSendPrivateAdminMessage()
        {
            IAdmin admin = CreateAdmin();
            admin.SetVersion(1, 0);
            admin.Connect("127.0.0.1", "admin1", "password1");

            bool sent = admin.SendPrivateAdminMessage(Guid.NewGuid(), "msg");

            Assert.IsTrue(sent);
            Assert.AreEqual(ProxyCallCount.GetCallCount("AdminSendPrivateAdminMessage"), 1);
        }

        // TODO: remaining APIs
    }

    [TestClass]
    public class AdminUnitTest : AbstractAdminUnitTest
    {
        private Factory _factory;

        protected override ICallCount ProxyCallCount
        {
            get { return _factory == null ? null : _factory.ProxyCallCount; }
        }

        private class Factory : IFactory
        {
            public ICallCount ProxyCallCount { get; private set; }

            public IProxy CreateProxy(ITetriNETAdminCallback callback, string address)
            {
                ProxyMock proxy = new ProxyMock(callback, address, new Versioning
                    {
                        Major = 1,
                        Minor = 0
                    });
                ProxyCallCount = proxy;
                return proxy;
            }
        }

        protected override IAdmin CreateAdmin()
        {
            _factory = new Factory();
            return new Admin(_factory);
        }
    }
}
