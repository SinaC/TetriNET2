﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Logger;
using TetriNET2.Server.Interfaces;
using TetriNET2.Server.Tests.Mocking;

namespace TetriNET2.Server.Tests
{
    [TestClass]
    public abstract class AbstractGameRoomManagerUnitTest
    {
        protected abstract IGameRoomManager CreateGameRoomManager(int maxRooms);
        protected abstract IGameRoom CreateGameRoom(string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password);

        [TestInitialize]
        public void Initialize()
        {
            Log.Default.Logger = new LogMock();
        }

        #region Add

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoomManager")]
        [TestCategory("Server.IGameRoomManager.Add")]
        [TestMethod]
        public void TestAddNullAdmin()
        {
            IGameRoomManager gameRoomManager = CreateGameRoomManager(10);

            try
            {
                gameRoomManager.Add(null);

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("room", ex.ParamName);
            }

            Assert.AreEqual(0, gameRoomManager.RoomCount);
            Assert.AreEqual(0, gameRoomManager.Rooms.Count());
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoomManager")]
        [TestCategory("Server.IGameRoomManager.Add")]
        [TestMethod]
        public void TestAddNoMaxRooms()
        {
            IGameRoomManager gameRoomManager = CreateGameRoomManager(10);

            bool inserted1 = gameRoomManager.Add(CreateGameRoom("room1", 5, 5, GameRules.Custom, new GameOptions(), null));
            bool inserted2 = gameRoomManager.Add(CreateGameRoom("room2", 5, 5, GameRules.Custom, new GameOptions(), null));

            Assert.IsTrue(inserted1);
            Assert.IsTrue(inserted2);
            Assert.AreEqual(2, gameRoomManager.RoomCount);
            Assert.AreEqual(2, gameRoomManager.Rooms.Count());
            Assert.IsTrue(gameRoomManager.Rooms.Any(x => x.Name == "room1") && gameRoomManager.Rooms.Any(x => x.Name == "room2"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoomManager")]
        [TestCategory("Server.IGameRoomManager.Add")]
        [TestMethod]
        public void TestAddWithMaxRooms()
        {
            IGameRoomManager gameRoomManager = CreateGameRoomManager(1);
            gameRoomManager.Add(CreateGameRoom("room1", 5, 5, GameRules.Custom, new GameOptions(), null));

            bool inserted = gameRoomManager.Add(CreateGameRoom("room2", 5, 5, GameRules.Custom, new GameOptions(), null));

            Assert.IsFalse(inserted);
            Assert.AreEqual(1, gameRoomManager.RoomCount);
            Assert.AreEqual("room1", gameRoomManager.Rooms.First().Name);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoomManager")]
        [TestCategory("Server.IGameRoomManager.Add")]
        [TestMethod]
        public void TestAddSameGameRoom()
        {
            IGameRoom room1 = CreateGameRoom("room1", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGameRoomManager gameRoomManager = CreateGameRoomManager(10);
            gameRoomManager.Add(room1);

            bool inserted = gameRoomManager.Add(room1);

            Assert.IsFalse(inserted);
            Assert.AreEqual(1, gameRoomManager.RoomCount);
            Assert.AreEqual(1, gameRoomManager.Rooms.Count());
        }

        #endregion

        #region Remove

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoomManager")]
        [TestCategory("Server.IGameRoomManager.Remove")]
        [TestMethod]
        public void TestRemoveExistingRoom()
        {
            IGameRoomManager gameRoomManager = CreateGameRoomManager(10);
            IGameRoom room = CreateGameRoom("room1", 5, 5, GameRules.Custom, new GameOptions(), null);
            gameRoomManager.Add(room);

            bool removed = gameRoomManager.Remove(room);

            Assert.IsTrue(removed);
            Assert.AreEqual(0, gameRoomManager.RoomCount);
            Assert.AreEqual(0, gameRoomManager.Rooms.Count());
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoomManager")]
        [TestCategory("Server.IGameRoomManager.Remove")]
        [TestMethod]
        public void TestRemoveNonExistingRoom()
        {
            IGameRoomManager gameRoomManager = CreateGameRoomManager(10);
            IGameRoom room1 = CreateGameRoom("room1", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGameRoom room2 = CreateGameRoom("room2", 5, 5, GameRules.Custom, new GameOptions(), null);
            gameRoomManager.Add(room1);

            bool removed = gameRoomManager.Remove(room2);

            Assert.IsFalse(removed);
            Assert.AreEqual(1, gameRoomManager.RoomCount);
            Assert.AreEqual(1, gameRoomManager.Rooms.Count());
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoomManager")]
        [TestCategory("Server.IGameRoomManager.Remove")]
        [TestMethod]
        public void TestRemoveNullRoom()
        {
            IGameRoomManager gameRoomManager = CreateGameRoomManager(10);
            gameRoomManager.Add(CreateGameRoom("room1", 5, 5, GameRules.Custom, new GameOptions(), null));

            try
            {
                gameRoomManager.Remove(null);
                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("room", ex.ParamName);
            }

            Assert.AreEqual(1, gameRoomManager.RoomCount);
            Assert.AreEqual(1, gameRoomManager.Rooms.Count());
        }

        #endregion

        #region Clear

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoomManager")]
        [TestCategory("Server.IGameRoomManager.Clear")]
        [TestMethod]
        public void TestClearNoRooms()
        {
            IGameRoomManager gameRoomManager = CreateGameRoomManager(10);

            gameRoomManager.Clear();

            Assert.AreEqual(0, gameRoomManager.RoomCount);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoomManager")]
        [TestCategory("Server.IGameRoomManager.Clear")]
        [TestMethod]
        public void TestClearSomeRooms()
        {
            IGameRoomManager gameRoomManager = CreateGameRoomManager(10);
            gameRoomManager.Add(CreateGameRoom("room1", 5, 5, GameRules.Custom, new GameOptions(), null));
            gameRoomManager.Add(CreateGameRoom("room2", 5, 5, GameRules.Custom, new GameOptions(), null));
            gameRoomManager.Add(CreateGameRoom("room3", 5, 5, GameRules.Custom, new GameOptions(), null));

            gameRoomManager.Clear();

            Assert.AreEqual(0, gameRoomManager.RoomCount);
        }

        #endregion

        #region Indexers

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoomManager")]
        [TestCategory("Server.IGameRoomManager.Indexers")]
        [TestMethod]
        public void TestIndexerGuidFindExistingRoom()
        {
            IGameRoom room1 = CreateGameRoom("room1", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGameRoom room2 = CreateGameRoom("room2", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGameRoom room3 = CreateGameRoom("room3", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGameRoomManager gameRoomManager = CreateGameRoomManager(10);
            gameRoomManager.Add(room1);
            gameRoomManager.Add(room2);
            gameRoomManager.Add(room3);

            IGameRoom searched = gameRoomManager[room2.Id];

            Assert.IsNotNull(searched);
            Assert.AreEqual(room2, searched);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoomManager")]
        [TestCategory("Server.IGameRoomManager.Indexers")]
        [TestMethod]
        public void TestIndexerGuidFindNonExistingAdmin()
        {
            IGameRoom room1 = CreateGameRoom("room1", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGameRoom room2 = CreateGameRoom("room2", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGameRoom room3 = CreateGameRoom("room3", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGameRoomManager gameRoomManager = CreateGameRoomManager(10);
            gameRoomManager.Add(room1);
            gameRoomManager.Add(room2);
            gameRoomManager.Add(room3);

            IGameRoom searched = gameRoomManager[Guid.Empty];

            Assert.IsNull(searched);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoomManager")]
        [TestCategory("Server.IGameRoomManager.Indexers")]
        [TestMethod]
        public void TestIndexerNameFindExistingRoom()
        {
            IGameRoom room1 = CreateGameRoom("room1", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGameRoom room2 = CreateGameRoom("room2", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGameRoom room3 = CreateGameRoom("room3", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGameRoomManager gameRoomManager = CreateGameRoomManager(10);
            gameRoomManager.Add(room1);
            gameRoomManager.Add(room2);
            gameRoomManager.Add(room3);

            IGameRoom searched = gameRoomManager[room2.Name];

            Assert.IsNotNull(searched);
            Assert.AreEqual(room2, searched);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoomManager")]
        [TestCategory("Server.IGameRoomManager.Indexers")]
        [TestMethod]
        public void TestIndexerNameFindNonExistingAdmin()
        {
            IGameRoom room1 = CreateGameRoom("room1", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGameRoom room2 = CreateGameRoom("room2", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGameRoom room3 = CreateGameRoom("room3", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGameRoomManager gameRoomManager = CreateGameRoomManager(10);
            gameRoomManager.Add(room1);
            gameRoomManager.Add(room2);
            gameRoomManager.Add(room3);

            IGameRoom searched = gameRoomManager["admin4"];

            Assert.IsNull(searched);
        }

        #endregion
    }

    [TestClass]
    public class GameRoomManagerUnitTest : AbstractGameRoomManagerUnitTest
    {
        protected override IGameRoomManager CreateGameRoomManager(int maxRooms)
        {
            return new GameRoomManager(maxRooms);
        }

        protected override IGameRoom CreateGameRoom(string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password)
        {
            return new GameRoom(new ActionQueueMock(), new PieceProviderMock(), name, maxPlayers, maxSpectators, rule, options, password);
        }

        #region Constructor

        [TestCategory("Server")]
        [TestCategory("Server.GameRoomManager")]
        [TestCategory("Server.GameRoomManager.ctor")]
        [TestMethod]
        public void TestConstructorStrictlyPositiveMaxRooms()
        {
            try
            {
                IGameRoomManager gameRoomManager = CreateGameRoomManager(0);
                Assert.Fail("ArgumentOutOfRange exception not raised");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.AreEqual("maxRooms", ex.ParamName);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.GameRoomManager")]
        [TestCategory("Server.GameRoomManager.ctor")]
        [TestMethod]
        public void TestConstructorSetProperties()
        {
            const int maxRooms = 10;

            IGameRoomManager gameRoomManager = CreateGameRoomManager(maxRooms);

            Assert.AreEqual(maxRooms, gameRoomManager.MaxRooms);
        }

        [TestCategory("Server")]
        [TestCategory("Server.GameRoomManager")]
        [TestCategory("Server.GameRoomManager.ctor")]
        [TestMethod]
        public void TestConstructorLockObjectNotNull()
        {
            IGameRoomManager gameRoomManager = CreateGameRoomManager(10);

            Assert.IsNotNull(gameRoomManager.LockObject);
        }

        #endregion
    }
}