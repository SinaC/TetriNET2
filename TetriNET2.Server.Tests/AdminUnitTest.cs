using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.Logger;
using TetriNET2.Server.Interfaces;
using TetriNET2.Server.Tests.Mocking;

namespace TetriNET2.Server.Tests
{
    [TestClass]
    public abstract class AbstractAdminUnitTest
    {
        protected abstract IAdmin CreateAdmin(string name, IPAddress address, ITetriNETAdminCallback callback);

        [TestInitialize]
        public void Initialize()
        {
            Log.Default.Logger = new LogMock();
        }

        #region Exception

        [TestCategory("Server")]
        [TestCategory("Server.IAdmin")]
        [TestCategory("Server.IAdmin.Exception")]
        [TestMethod]
        public void TestExceptionFreeAction()
        {
            IAdmin admin = CreateAdmin("admin1", IPAddress.Any, new RaiseExceptionTetriNETAdminCallback());

            admin.OnDisconnected();

            Assert.IsTrue(true, "No exception occured");
        }

        [TestCategory("Server")]
        [TestCategory("Server.IAdmin")]
        [TestCategory("Server.IAdmin.Exception")]
        [TestMethod]
        public void TestConnectionLostCalledOnException()
        {
            bool called = false;
            IAdmin admin = CreateAdmin("admin1", IPAddress.Any, new RaiseExceptionTetriNETAdminCallback());
            admin.ConnectionLost += entity => called = true;

            admin.OnDisconnected();

            Assert.IsTrue(called);
        }

        #endregion
    }

    [TestClass]
    public class AdminUnitTest : AbstractAdminUnitTest
    {
        protected override IAdmin CreateAdmin(string name, IPAddress address, ITetriNETAdminCallback callback)
        {
            return new Admin(name, address, callback);
        }

        #region Constructor

        [TestCategory("Server")]
        [TestCategory("Server.Admin")]
        [TestCategory("Server.Admin.ctor")]
        [TestMethod]
        public void TestNullName()
        {
            try
            {
                IAdmin admin = CreateAdmin(null, IPAddress.Any, new CountCallTetriNETAdminCallback());

                Assert.Fail("ArgumentNullException on name not raised");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("name", ex.ParamName);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.Admin")]
        [TestCategory("Server.Admin.ctor")]
        [TestMethod]
        public void TestNullAddress()
        {
            try
            {
                IAdmin admin = CreateAdmin("admin1", null, new CountCallTetriNETAdminCallback());

                Assert.Fail("ArgumentNullException on name not raised");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("address", ex.ParamName);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.Admin")]
        [TestCategory("Server.Admin.ctor")]
        [TestMethod]
        public void TestNullCallback()
        {
            try
            {
                IAdmin admin = CreateAdmin("admin1", IPAddress.Any, null);

                Assert.Fail("ArgumentNullException on callback not raised");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("callback", ex.ParamName);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.Admin")]
        [TestCategory("Server.Admin.ctor")]
        [TestMethod]
        public void TestConstructorSetProperties()
        {
            const string name = "admin1";
            IPAddress address = IPAddress.Any;
            ITetriNETAdminCallback callback = new CountCallTetriNETAdminCallback();

            IAdmin admin = CreateAdmin(name, address, callback);

            Assert.AreEqual(admin.Name, name);
            Assert.AreEqual(admin.Address, address);
            Assert.AreEqual(admin.Callback, callback);
            Assert.AreNotEqual(admin.ConnectTime, default(DateTime));
            Assert.IsFalse(admin.Id.Equals(default(Guid)));
        }

        #endregion
    }
}
