using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Logger;
using TetriNET2.Common.Occurancy;
using TetriNET2.Server;
using TetriNET2.Server.Interfaces;
using TetriNET2.Tests.Server.Mocking;

namespace TetriNET2.Tests.Server
{
    [TestClass]
    public abstract class AbstractGameRoomUnitTest
    {
        protected abstract IGameRoom CreateGameRoom(string name, int maxPlayers, int maxSpectators);
        protected abstract IGameRoom CreateGameRoom(string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password = null);
        protected abstract IGameRoom CreateGameRoom(IFactory factory, string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password = null);

        [TestInitialize]
        public void Initialize()
        {
            Log.SetLogger(new LogMock());
        }

        [TestMethod]
        public void TestNullFactory()
        {
            try
            {
                IGameRoom game = CreateGameRoom(null, "game1", 5, 5, GameRules.Classic, new GameOptions(), "password");

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual(ex.ParamName, "factory");
            }
        }

        [TestMethod]
        public void TestNullName()
        {
            try
            {
                IGameRoom game = CreateGameRoom(null, 5, 5, GameRules.Classic, new GameOptions(), "password");

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual(ex.ParamName, "name");
            }
        }

        [TestMethod]
        public void TestStrictlyPositiveMaxPlayers()
        {
            try
            {
                IGameRoom game = CreateGameRoom("game1", 0, 5, GameRules.Classic, new GameOptions(), "password");

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.AreEqual(ex.ParamName, "maxPlayers");
            }
        }

        [TestMethod]
        public void TestStrictlyPositiveMaxSpectators()
        {
            try
            {
                IGameRoom game = CreateGameRoom("game1", 5, 0, GameRules.Classic, new GameOptions(), "password");

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.AreEqual(ex.ParamName, "maxSpectators");
            }
        }

        [TestMethod]
        public void TestNullOptions()
        {
            try
            {
                IGameRoom game = CreateGameRoom("game1", 5, 5, GameRules.Classic, null, "password");

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual(ex.ParamName, "options");
            }
        }

        [TestMethod]
        public void TestConstructorSetProperties()
        {
            const string name = "game1";
            const int maxPlayers = 5;
            const int maxSpectators = 10;
            const GameRules rule = GameRules.Extended;
            const string password = "password";
            GameOptions options = new GameOptions();

            IGameRoom game = CreateGameRoom(name, maxPlayers, maxSpectators, rule, options, password);

            Assert.IsNotNull(game);
            Assert.AreEqual(game.Name, name);
            Assert.AreEqual(game.MaxPlayers, maxPlayers);
            Assert.AreEqual(game.MaxSpectators, maxSpectators);
            Assert.AreEqual(game.Rule, rule);
            Assert.AreEqual(game.Options, options);
            Assert.AreEqual(game.Password, password);
            Assert.AreNotEqual(game.Id, Guid.Empty);
            Assert.AreEqual(game.State, GameRoomStates.Created);
        }

        // TODO: 
        //  test every GameRoom API
        //  game started + join/leave/stop
    }

    [TestClass]
    public class GameRoomUnitTest : AbstractGameRoomUnitTest
    {
        private class Factory : IFactory
        {
            // Always get first available
            private Pieces PseudoRandom(IEnumerable<IOccurancy<Pieces>> occurancies, IEnumerable<Pieces> history)
            {
                var available = (occurancies as IList<IOccurancy<Pieces>> ?? occurancies.ToList()).Where(x => !history.Contains(x.Value)).ToList();
                if (available.Any())
                {
                    Pieces piece = available[0].Value;
                    return piece;
                }
                return Pieces.Invalid;
            }

            #region IFactory

            public IClient CreateClient(string name, string team, IPAddress address, ITetriNETCallback callback)
            {
                throw new System.NotImplementedException();
            }

            public IGameRoom CreateGameRoom(string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password)
            {
                throw new System.NotImplementedException();
            }

            public IPieceProvider CreatePieceProvider()
            {
                return new PieceBag(PseudoRandom, 4);
            }

            #endregion
        }

        private readonly IFactory _factory = new Factory();

        protected override IGameRoom CreateGameRoom(string name, int maxPlayers, int maxSpectators)
        {
            return new TetriNET2.Server.GameRoom(_factory, name, maxPlayers, maxSpectators, GameRules.Classic, new GameOptions());
        }

        protected override IGameRoom CreateGameRoom(string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password = null)
        {
            return new TetriNET2.Server.GameRoom(_factory, name, maxPlayers, maxSpectators, rule, options, password);
        }

        protected override IGameRoom CreateGameRoom(IFactory factory, string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password = null)
        {
            return new TetriNET2.Server.GameRoom(factory, name, maxPlayers, maxSpectators, rule, options, password);
        }
    }
}
