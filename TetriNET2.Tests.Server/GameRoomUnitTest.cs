using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET2.Common.ActionQueue;
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
        protected IPieceProvider PieceProvider;
        protected IActionQueue ActionQueue;

        protected abstract IClient CreateClient(string name, ITetriNETCallback callback);
        protected abstract IGameRoom CreateGameRoom(string name, int maxPlayers, int maxSpectators);
        protected abstract IGameRoom CreateGameRoom(string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password = null);
        protected abstract IGameRoom CreateGameRoom(IActionQueue actionQueue, IPieceProvider pieceProvider, string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password = null);

        [TestInitialize]
        public void Initialize()
        {
            Log.Default.Logger = new LogMock();
        }

        #region Constructor

        [TestMethod]
        public void TestNullActionQueue()
        {
            try
            {
                IGameRoom game = CreateGameRoom(null, new PieceProviderMock(), "game1", 5, 5, GameRules.Classic, new GameOptions(), "password");

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual(ex.ParamName, "actionQueue");
            }
        }

        [TestMethod]
        public void TestNullPieceProvider()
        {
            try
            {
                IGameRoom game = CreateGameRoom(new ActionQueueMock(), null, "game1", 5, 5, GameRules.Classic, new GameOptions(), "password");

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual(ex.ParamName, "pieceProvider");
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

        [TestMethod]
        public void TestLockObjectNotNull()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10, GameRules.Custom, new GameOptions());

            Assert.IsNotNull(game.LockObject);
        }

        #endregion

        #region Join

        [TestMethod]
        public void TestJoinPlayerNoMaxPlayers()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            IClient client = CreateClient("client1", new CountCallTetriNETCallback());

            bool joined = game.Join(client, false);

            Assert.IsTrue(joined);
            Assert.AreEqual(game.PlayerCount, 1);
            Assert.AreEqual(game.SpectatorCount, 0);
            Assert.AreEqual(game.Players.Count(), 1);
            Assert.AreEqual(game.Spectators.Count(), 0);
        }

        [TestMethod]
        public void TestJoinSpectatorNoMaxSpectators()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            IClient client = CreateClient("client1", new CountCallTetriNETCallback());

            bool joined = game.Join(client, true);

            Assert.IsTrue(joined);
            Assert.AreEqual(game.PlayerCount, 0);
            Assert.AreEqual(game.SpectatorCount, 1);
            Assert.AreEqual(game.Players.Count(), 0);
            Assert.AreEqual(game.Spectators.Count(), 1);
        }

        [TestMethod]
        public void TestJoinPlayerMaxPlayers()
        {
            IGameRoom game = CreateGameRoom("game1", 1, 10);
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            IClient client2 = CreateClient("client2", new CountCallTetriNETCallback());
            game.Join(client1, false);

            bool joined = game.Join(client2, false);

            Assert.IsFalse(joined);
            Assert.AreEqual(game.PlayerCount, 1);
            Assert.AreEqual(game.SpectatorCount, 0);
            Assert.AreEqual(game.Players.Count(), 1);
            Assert.AreEqual(game.Spectators.Count(), 0);
        }

        [TestMethod]
        public void TestJoinSpectatorMaxSpectators()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 1);
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            IClient client2 = CreateClient("client2", new CountCallTetriNETCallback());
            game.Join(client1, true);

            bool joined = game.Join(client2, true);

            Assert.IsFalse(joined);
            Assert.AreEqual(game.PlayerCount, 0);
            Assert.AreEqual(game.SpectatorCount, 1);
            Assert.AreEqual(game.Players.Count(), 0);
            Assert.AreEqual(game.Spectators.Count(), 1);
        }

        [TestMethod]
        public void TestJoinSameClient()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, true);

            bool joined = game.Join(client1, false);

            Assert.IsFalse(joined);
        }

        [TestMethod]
        public void TestJoinPlayerModifyPlayerProperties()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            IClient client = CreateClient("client1", new CountCallTetriNETCallback());
            
            game.Join(client, false);

            Assert.AreEqual(client.State, ClientStates.WaitInGameRoom);
            Assert.AreEqual(client.Roles, ClientRoles.Player);
            Assert.AreEqual(client.Game, game);
        }

        [TestMethod]
        public void TestJoinSpectatorModifySpectatorProperties()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            IClient client = CreateClient("client1", new CountCallTetriNETCallback());

            game.Join(client, true);

            Assert.AreEqual(client.State, ClientStates.WaitInGameRoom);
            Assert.AreEqual(client.Roles, ClientRoles.Spectator);
            Assert.AreEqual(client.Game, game);
        }

        [TestMethod]
        public void TestJoinPlayerCallbackCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback = new CountCallTetriNETCallback();
            IClient client = CreateClient("client1", callback);

            game.Join(client, false);

            Assert.AreEqual(callback.GetCallCount("OnGameJoined"), 1);
        }

        [TestMethod]
        public void TestJoinSpectatorCallbackCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback = new CountCallTetriNETCallback();
            IClient client = CreateClient("client1", callback);

            game.Join(client, true);

            Assert.AreEqual(callback.GetCallCount("OnGameJoined"), 1);
        }

        [TestMethod]
        public void TestJoinPlayerOtherClientsInformed()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client1, true);
            game.Join(client2, false);

            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);

            Assert.AreEqual(callback1.GetCallCount("OnClientGameJoined"), 2);
            Assert.AreEqual(callback2.GetCallCount("OnClientGameJoined"), 1);
            Assert.AreEqual(callback3.GetCallCount("OnClientGameJoined"), 0);
        }

        [TestMethod]
        public void TestJoinSpectatorOtherClientsInformed()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client1, true);
            game.Join(client2, false);

            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, true);

            Assert.AreEqual(callback1.GetCallCount("OnClientGameJoined"), 2);
            Assert.AreEqual(callback2.GetCallCount("OnClientGameJoined"), 1);
            Assert.AreEqual(callback3.GetCallCount("OnClientGameJoined"), 0);
        }

        #endregion

        #region Leave

        [TestMethod]
        public void TestLeaveNullClient()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);

            try
            {
                game.Leave(null);

                Assert.Fail("Exception not thrown");
            }
            catch(ArgumentNullException ex)
            {
                Assert.AreEqual(ex.ParamName, "client");
            }
        }

        [TestMethod]
        public void TestLeaveNonExistingClient()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, false);

            IClient client2 = CreateClient("client2", new CountCallTetriNETCallback());
            bool left = game.Leave(client2);

            Assert.IsFalse(left);
            Assert.AreEqual(game.ClientCount, 1);
        }

        [TestMethod]
        public void TestLeaveExistingClient()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            IClient client2 = CreateClient("client2", new CountCallTetriNETCallback());
            game.Join(client1, false);
            game.Join(client2, true);

            bool left = game.Leave(client1);

            Assert.IsTrue(left);
            Assert.AreEqual(game.ClientCount, 1);
            Assert.AreEqual(game.PlayerCount, 0);
            Assert.AreEqual(game.SpectatorCount, 1);
            Assert.AreEqual(game.Clients.First(), client2);
        }

        [TestMethod]
        public void TestLeavePlayerModifyPlayerProperties()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            IClient client = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client, false);

            game.Leave(client);

            Assert.AreEqual(client.State, ClientStates.Connected);
            Assert.AreEqual(client.Roles, ClientRoles.NoRole);
            Assert.IsNull(client.Game);
        }

        [TestMethod]
        public void TestLeaveSpectatorModifySpectatorProperties()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            IClient client = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client, true);

            game.Leave(client);

            Assert.AreEqual(client.State, ClientStates.Connected);
            Assert.AreEqual(client.Roles, ClientRoles.NoRole);
            Assert.IsNull(client.Game);
        }

        [TestMethod]
        public void TestLeaveClientCallbackCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client = CreateClient("client1", callback1);
            game.Join(client, true);

            game.Leave(client);

            Assert.AreEqual(callback1.GetCallCount("OnGameLeft"), 1);
        }

        [TestMethod]
        public void TestLeaveClientOtherClientsInformed()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, true);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, true);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);

            game.Leave(client1);

            Assert.AreEqual(callback2.GetCallCount("OnClientGameLeft"), 1);
            Assert.AreEqual(callback3.GetCallCount("OnClientGameLeft"), 1);
        }

        #endregion

        #region Clear

        [TestMethod]
        public void TestClearNoClients()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);

            game.Clear();

            Assert.AreEqual(game.ClientCount, 0);
        }

        [TestMethod]
        public void TestClearMultipleClients()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Join(CreateClient("client1", new CountCallTetriNETCallback()), true);
            game.Join(CreateClient("client2", new CountCallTetriNETCallback()), true);
            game.Join(CreateClient("client3", new CountCallTetriNETCallback()), true);

            game.Clear();

            Assert.AreEqual(game.ClientCount, 0);
        }

        #endregion

        #region Start/Stop

        [TestMethod]
        public void TestStartChangeState()
        {
            IGameRoom game = CreateGameRoom("game1", 10, 5);

            game.Start(new CancellationTokenSource());

            Assert.AreEqual(game.State, GameRoomStates.WaitStartGame);
        }

        [TestMethod]
        public void TestStartOnlyIfCreated()
        {
            IGameRoom game = CreateGameRoom("game1", 10, 5);
            game.Start(new CancellationTokenSource());

            game.Start(new CancellationTokenSource());

            Assert.AreEqual(game.State, GameRoomStates.WaitStartGame);
            Assert.AreEqual(((LogMock)Log.Default.Logger).LastLogLevel, LogLevels.Warning);
        }

        [TestMethod]
        public void TestStopOnlyIfStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 10, 5);

            game.Stop();

            Assert.AreEqual(game.State, GameRoomStates.Created);
            Assert.AreEqual(((LogMock)Log.Default.Logger).LastLogLevel, LogLevels.Warning);
        }

        [TestMethod]
        public void TestStopClientsRemovedAndStatusChanged()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, true);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, true);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);
            game.Start(new CancellationTokenSource());

            game.Stop();

            Assert.AreEqual(game.State, GameRoomStates.Created);
            Assert.AreEqual(game.ClientCount, 0);
            Assert.IsNull(client1.Game);
            Assert.IsNull(client2.Game);
            Assert.IsNull(client3.Game);
            Assert.AreEqual(client1.State, ClientStates.Connected);
            Assert.AreEqual(client2.State, ClientStates.Connected);
            Assert.AreEqual(client3.State, ClientStates.Connected);
        }

        [TestMethod]
        public void TestStopCallbackCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, true);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, true);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);
            game.Start(new CancellationTokenSource());

            game.Stop();

            Assert.AreEqual(game.State, GameRoomStates.Created);
            Assert.AreEqual(callback1.GetCallCount("OnGameLeft"), 1);
            Assert.AreEqual(callback2.GetCallCount("OnGameLeft"), 1);
            Assert.AreEqual(callback3.GetCallCount("OnGameLeft"), 1);
        }

        #endregion

        [TestMethod]
        public void TestChangeOptionsFailedIfNotWaitingGameStart()
        {
            GameOptions originalOptions = new GameOptions();
            IGameRoom game = CreateGameRoom("game1", 5, 10, GameRules.Custom, originalOptions);

            GameOptions newOptions = new GameOptions();
            game.ChangeOptions(newOptions);

            Assert.AreEqual(game.Options, originalOptions);
        }

        [TestMethod]
        public void TestChangeOptionsOkWhenWaitingGameStart()
        {
            GameOptions originalOptions = new GameOptions();
            IGameRoom game = CreateGameRoom("game1", 5, 10, GameRules.Custom, originalOptions);
            game.Start(new CancellationTokenSource());

            GameOptions newOptions = new GameOptions();
            game.ChangeOptions(newOptions);

            Assert.AreEqual(game.Options, newOptions);
        }


        [TestMethod]
        public void TestChangeOptionsClientsInformed()
        {
            GameOptions originalOptions = new GameOptions();
            IGameRoom game = CreateGameRoom("game1", 5, 10, GameRules.Custom, originalOptions);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, true);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, true);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);
            game.Start(new CancellationTokenSource());

            GameOptions newOptions = new GameOptions();
            game.ChangeOptions(newOptions);

            Assert.AreEqual(callback1.GetCallCount("OnGameOptionsChanged"), 1);
            Assert.AreEqual(callback2.GetCallCount("OnGameOptionsChanged"), 1);
            Assert.AreEqual(callback3.GetCallCount("OnGameOptionsChanged"), 1);
        }

        // TODO: 
        //  test every GameRoom API
        //  game started + join/leave/stop
    }

    [TestClass]
    public class GameRoomUnitTest : AbstractGameRoomUnitTest
    {
        protected override IClient CreateClient(string name, ITetriNETCallback callback)
        {
            return new TetriNET2.Server.Client(name, IPAddress.Any, callback);
        }

        protected override IGameRoom CreateGameRoom(string name, int maxPlayers, int maxSpectators)
        {
            return CreateGameRoom(name, maxPlayers, maxSpectators, GameRules.Classic, new GameOptions());
        }

        protected override IGameRoom CreateGameRoom(string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password = null)
        {
            return CreateGameRoom(new ActionQueueMock(), new PieceProviderMock(), name, maxPlayers, maxSpectators, rule, options, password);
        }

        protected override IGameRoom CreateGameRoom(IActionQueue actionQueue, IPieceProvider pieceProvider, string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password = null)
        {
            ActionQueue = actionQueue;
            PieceProvider = pieceProvider;

            return new GameRoom(actionQueue, pieceProvider, name, maxPlayers, maxSpectators, rule, options, password);
        }
    }
}
