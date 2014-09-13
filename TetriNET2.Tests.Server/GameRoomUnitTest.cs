using System;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET2.Common.ActionQueue;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Logger;
using TetriNET2.Server;
using TetriNET2.Server.Interfaces;
using TetriNET2.Tests.Server.Mocking;

namespace TetriNET2.Tests.Server
{
    [TestClass]
    public abstract class AbstractGameRoomUnitTest
    {
        protected LogMock Logger;
        protected PieceProviderMock PieceProvider;
        protected ActionQueueMock ActionQueue;

        protected abstract IClient CreateClient(string name, ITetriNETCallback callback);
        protected abstract IGameRoom CreateGameRoom(string name, int maxPlayers, int maxSpectators);
        protected abstract IGameRoom CreateGameRoom(string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password = null);
        protected abstract IGameRoom CreateGameRoom(IActionQueue actionQueue, IPieceProvider pieceProvider, string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password = null);

        [TestInitialize]
        public void Initialize()
        {
            Logger = new LogMock();
            Log.Default.Logger = Logger;
        }

        #region Constructor

        [TestMethod]
        public void TestConstructorNullActionQueue()
        {
            try
            {
                IGameRoom game = CreateGameRoom(null, new PieceProviderMock(), "game1", 5, 5, GameRules.Classic, new GameOptions(), "password");

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("actionQueue", ex.ParamName);
            }
        }

        [TestMethod]
        public void TestConstructorNullPieceProvider()
        {
            try
            {
                IGameRoom game = CreateGameRoom(new ActionQueueMock(), null, "game1", 5, 5, GameRules.Classic, new GameOptions(), "password");

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("pieceProvider", ex.ParamName);
            }
        }

        [TestMethod]
        public void TestConstructorNullName()
        {
            try
            {
                IGameRoom game = CreateGameRoom(null, 5, 5, GameRules.Classic, new GameOptions(), "password");

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("name", ex.ParamName);
            }
        }

        [TestMethod]
        public void TestConstructorStrictlyPositiveMaxPlayers()
        {
            try
            {
                IGameRoom game = CreateGameRoom("game1", 0, 5, GameRules.Classic, new GameOptions(), "password");

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.AreEqual("maxPlayers", ex.ParamName);
            }
        }

        [TestMethod]
        public void TestConstructorStrictlyPositiveMaxSpectators()
        {
            try
            {
                IGameRoom game = CreateGameRoom("game1", 5, 0, GameRules.Classic, new GameOptions(), "password");

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.AreEqual("maxSpectators", ex.ParamName);
            }
        }

        [TestMethod]
        public void TestConstructorNullOptions()
        {
            try
            {
                IGameRoom game = CreateGameRoom("game1", 5, 5, GameRules.Classic, null, "password");

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("options", ex.ParamName);
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
            Assert.AreEqual(name, game.Name);
            Assert.AreEqual(maxPlayers, game.MaxPlayers);
            Assert.AreEqual(maxSpectators, game.MaxSpectators);
            Assert.AreEqual(rule, game.Rule);
            Assert.AreEqual(options, game.Options);
            Assert.AreEqual(password, game.Password);
            Assert.AreNotEqual(Guid.Empty, game.Id);
            Assert.AreEqual(GameRoomStates.Created, game.State);
        }

        [TestMethod]
        public void TestConstructorLockObjectNotNull()
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
            Assert.AreEqual(1, game.PlayerCount);
            Assert.AreEqual(0, game.SpectatorCount);
            Assert.AreEqual(1, game.Players.Count());
            Assert.AreEqual(0, game.Spectators.Count());
        }

        [TestMethod]
        public void TestJoinSpectatorNoMaxSpectators()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            IClient client = CreateClient("client1", new CountCallTetriNETCallback());

            bool joined = game.Join(client, true);

            Assert.IsTrue(joined);
            Assert.AreEqual(0, game.PlayerCount);
            Assert.AreEqual(1, game.SpectatorCount);
            Assert.AreEqual(0, game.Players.Count());
            Assert.AreEqual(1, game.Spectators.Count());
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
            Assert.AreEqual(1, game.PlayerCount);
            Assert.AreEqual(0, game.SpectatorCount);
            Assert.AreEqual(1, game.Players.Count());
            Assert.AreEqual(0, game.Spectators.Count());
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
            Assert.AreEqual(0, game.PlayerCount);
            Assert.AreEqual(1, game.SpectatorCount);
            Assert.AreEqual(0, game.Players.Count());
            Assert.AreEqual(1, game.Spectators.Count());
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

            Assert.AreEqual(ClientStates.WaitInGameRoom, client.State);
            Assert.AreEqual(ClientRoles.Player, client.Roles);
            Assert.AreEqual(game, client.Game);
        }

        [TestMethod]
        public void TestJoinSpectatorModifySpectatorProperties()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            IClient client = CreateClient("client1", new CountCallTetriNETCallback());

            game.Join(client, true);

            Assert.AreEqual(ClientStates.WaitInGameRoom, client.State);
            Assert.AreEqual(ClientRoles.Spectator, client.Roles);
            Assert.AreEqual(game, client.Game);
        }

        [TestMethod]
        public void TestJoinPlayerCallbackCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback = new CountCallTetriNETCallback();
            IClient client = CreateClient("client1", callback);

            game.Join(client, false);

            Assert.AreEqual(1, callback.GetCallCount("OnGameJoined"));
        }

        [TestMethod]
        public void TestJoinSpectatorCallbackCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback = new CountCallTetriNETCallback();
            IClient client = CreateClient("client1", callback);

            game.Join(client, true);

            Assert.AreEqual(1, callback.GetCallCount("OnGameJoined"));
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

            Assert.AreEqual(2, callback1.GetCallCount("OnClientGameJoined"));
            Assert.AreEqual(1, callback2.GetCallCount("OnClientGameJoined"));
            Assert.AreEqual(0, callback3.GetCallCount("OnClientGameJoined"));
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

            Assert.AreEqual(2, callback1.GetCallCount("OnClientGameJoined"));
            Assert.AreEqual(1, callback2.GetCallCount("OnClientGameJoined"));
            Assert.AreEqual(0, callback3.GetCallCount("OnClientGameJoined"));
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
                Assert.AreEqual("client", ex.ParamName);
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
            Assert.AreEqual(1, game.ClientCount);
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
            Assert.AreEqual(1, game.ClientCount);
            Assert.AreEqual(0, game.PlayerCount);
            Assert.AreEqual(1, game.SpectatorCount);
            Assert.AreEqual(client2, game.Clients.First());
        }

        [TestMethod]
        public void TestLeavePlayerModifyPlayerProperties()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            IClient client = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client, false);

            game.Leave(client);

            Assert.AreEqual(ClientStates.Connected, client.State);
            Assert.AreEqual(ClientRoles.NoRole, client.Roles);
            Assert.IsNull(client.Game);
        }

        [TestMethod]
        public void TestLeaveSpectatorModifySpectatorProperties()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            IClient client = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client, true);

            game.Leave(client);

            Assert.AreEqual(ClientStates.Connected, client.State);
            Assert.AreEqual(ClientRoles.NoRole, client.Roles);
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

            Assert.AreEqual(1, callback1.GetCallCount("OnGameLeft"));
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

            Assert.AreEqual(1, callback2.GetCallCount("OnClientGameLeft"));
            Assert.AreEqual(1, callback3.GetCallCount("OnClientGameLeft"));
        }

        #endregion

        #region Clear

        [TestMethod]
        public void TestClearNoClients()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);

            game.Clear();

            Assert.AreEqual(0, game.ClientCount);
        }

        [TestMethod]
        public void TestClearMultipleClients()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Join(CreateClient("client1", new CountCallTetriNETCallback()), true);
            game.Join(CreateClient("client2", new CountCallTetriNETCallback()), true);
            game.Join(CreateClient("client3", new CountCallTetriNETCallback()), true);

            game.Clear();

            Assert.AreEqual(0, game.ClientCount);
        }

        #endregion

        #region Start/Stop

        [TestMethod]
        public void TestStartChangeState()
        {
            IGameRoom game = CreateGameRoom("game1", 10, 5);

            game.Start(new CancellationTokenSource());

            Assert.AreEqual(GameRoomStates.WaitStartGame, game.State);
        }

        [TestMethod]
        public void TestStartOnlyIfCreated()
        {
            IGameRoom game = CreateGameRoom("game1", 10, 5);
            game.Start(new CancellationTokenSource());
            Logger.Clear();

            game.Start(new CancellationTokenSource());

            Assert.AreEqual(GameRoomStates.WaitStartGame, game.State);
            Assert.AreEqual(LogLevels.Warning, Logger.LastLogLevel);
        }

        [TestMethod]
        public void TestStopOnlyIfStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 10, 5);
            Logger.Clear();

            game.Stop();

            Assert.AreEqual(GameRoomStates.Created, game.State);
            Assert.AreEqual(LogLevels.Warning, Logger.LastLogLevel);
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

            Assert.AreEqual(GameRoomStates.Created, game.State);
            Assert.AreEqual(0, game.ClientCount);
            Assert.IsNull(client1.Game);
            Assert.IsNull(client2.Game);
            Assert.IsNull(client3.Game);
            Assert.AreEqual(ClientStates.Connected, client1.State);
            Assert.AreEqual(ClientStates.Connected,client2.State);
            Assert.AreEqual(ClientStates.Connected,client3.State);
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
            Assert.AreEqual(1,callback1.GetCallCount("OnGameLeft"));
            Assert.AreEqual(1,callback2.GetCallCount("OnGameLeft"));
            Assert.AreEqual(1,callback3.GetCallCount("OnGameLeft"));
        }

        #endregion

        #region ChangeOptions
        [TestMethod]
        public void TestChangeOptionsFailedIfNotWaitingGameStart()
        {
            GameOptions originalOptions = new GameOptions();
            IGameRoom game = CreateGameRoom("game1", 5, 10, GameRules.Custom, originalOptions);
            Logger.Clear();

            GameOptions newOptions = new GameOptions();
            game.ChangeOptions(newOptions);

            Assert.AreEqual(originalOptions, game.Options);
            Assert.AreEqual(LogLevels.Warning, Logger.LastLogLevel);
        }

        [TestMethod]
        public void TestChangeOptionsOkWhenWaitingGameStart()
        {
            GameOptions originalOptions = new GameOptions();
            IGameRoom game = CreateGameRoom("game1", 5, 10, GameRules.Custom, originalOptions);
            game.Start(new CancellationTokenSource());

            GameOptions newOptions = new GameOptions();
            game.ChangeOptions(newOptions);

            Assert.AreEqual(newOptions, game.Options);
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

            Assert.AreEqual(1, callback1.GetCallCount("OnGameOptionsChanged"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGameOptionsChanged"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGameOptionsChanged"));
        }

        #endregion

        #region ResetWinList

        [TestMethod]
        public void TestResetWinListClientsInformed()
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

            game.ResetWinList();

            Assert.AreEqual(1, callback1.GetCallCount("OnWinListModified"));
            Assert.AreEqual(1, callback2.GetCallCount("OnWinListModified"));
            Assert.AreEqual(1, callback3.GetCallCount("OnWinListModified"));
        }

        [TestMethod]
        public void TestResetWinListFailedIfNotWaitingGameStart()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, true);
            Logger.Clear();

            game.ResetWinList();

            Assert.AreEqual(0, callback1.GetCallCount("OnWinListModified"));
            Assert.AreEqual(LogLevels.Warning, Logger.LastLogLevel);
        }
            
        #endregion

        #region PlacePiece

        [TestMethod]
        public void TestPlacePieceExceptionIfNullClient()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);

            try
            {
                game.PlacePiece(null, 0, 0, Pieces.TetriminoO, 0, 0, 0, null);

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("client", ex.ParamName);
            }
        }

        [TestMethod]
        public void TestPlacePieceFailedIfClientNotInGame()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            Logger.Clear();

            game.PlacePiece(client1, 0, 0, Pieces.TetriminoO, 0, 0, 0, null);

            Assert.AreEqual(LogLevels.Warning, Logger.LastLogLevel);
            Assert.AreEqual(0, ActionQueue.ActionCount);
        }

        [TestMethod]
        public void TestPlacePieceFailedIfGameNotStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, true);
            game.Start(new CancellationTokenSource());
            Logger.Clear();
            
            game.PlacePiece(client1, 0, 0, Pieces.TetriminoO, 0, 0, 0, null);

            Assert.AreEqual(LogLevels.Warning, Logger.LastLogLevel);
            Assert.AreEqual(0, ActionQueue.ActionCount);
        }

        [TestMethod]
        public void TestPlacePieceFailedIfClientNotPlaying()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.Start(new CancellationTokenSource());
            game.Options.ResetToDefault(); // needed for reset statistics
            game.StartGame();
            client1.State = ClientStates.GameLost; // simulate game loss

            game.PlacePiece(client1, 0, 0, Pieces.TetriminoO, 0, 0, 0, null);

            Assert.AreEqual(LogLevels.Warning, Logger.LastLogLevel);
            Assert.AreEqual(0, ActionQueue.ActionCount);
        }

        [TestMethod]
        public void TestPlacePieceActionEnqueuedIfGameStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.Start(new CancellationTokenSource());
            game.Options.ResetToDefault(); // needed for reset statistics
            game.StartGame();

            game.PlacePiece(client1, 0, 0, Pieces.TetriminoO, 0, 0, 0, null);

            Assert.AreEqual(1, ActionQueue.ActionCount);
        }

        [TestMethod]
        public void TestPlacePieceActionCallbacksCalled()
        {
            GameOptions originalOptions = new GameOptions();
            IGameRoom game = CreateGameRoom("game1", 5, 10, GameRules.Custom, originalOptions);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, true);
            game.Start(new CancellationTokenSource());
            game.Options.ResetToDefault(); // needed for reset statistics
            game.StartGame();

            game.PlacePiece(client1, 0, 0, Pieces.TetriminoO, 0, 0, 0, null);
            ActionQueue.DequeueAndExecuteFirstAction();

            Assert.AreEqual(1, callback1.GetCallCount("OnPiecePlaced"));
            Assert.AreEqual(0, callback2.GetCallCount("OnPiecePlaced"));
            Assert.AreEqual(0, callback3.GetCallCount("OnPiecePlaced"));
            Assert.AreEqual(0, callback1.GetCallCount("OnGridModified"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGridModified"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGridModified"));
        }

        #endregion

        #region ModifyGrid

        [TestMethod]
        public void TestModifyGridExceptionIfNullClient()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);

            try
            {
                game.ModifyGrid(null, null);

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("client", ex.ParamName);
            }
        }

        [TestMethod]
        public void TestModifyGridFailedIfClientNotInGame()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            Logger.Clear();

            game.ModifyGrid(client1, null);

            Assert.AreEqual(LogLevels.Warning, Logger.LastLogLevel);
            Assert.AreEqual(0, ActionQueue.ActionCount);
        }

        [TestMethod]
        public void TestModifyGridFailedIfGameNotStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, true);
            game.Start(new CancellationTokenSource());
            Logger.Clear();

            game.ModifyGrid(client1, null);

            Assert.AreEqual(LogLevels.Warning, Logger.LastLogLevel);
            Assert.AreEqual(0, ActionQueue.ActionCount);
        }

        [TestMethod]
        public void TestModifyGridFailedIfClientNotPlaying()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.Start(new CancellationTokenSource());
            game.Options.ResetToDefault(); // needed for reset statistics
            game.StartGame();
            client1.State = ClientStates.GameLost; // simulate game loss

            game.ModifyGrid(client1, null);

            Assert.AreEqual(LogLevels.Warning, Logger.LastLogLevel);
            Assert.AreEqual(0, ActionQueue.ActionCount);
        }

        [TestMethod]
        public void TestModifyGridActionEnqueuedIfGameStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.Start(new CancellationTokenSource());
            game.Options.ResetToDefault(); // needed for reset statistics
            game.StartGame();

            game.ModifyGrid(client1, null);

            Assert.AreEqual(1, ActionQueue.ActionCount);
        }

        [TestMethod]
        public void TestModifyGridActionCallbacksCalled()
        {
            GameOptions originalOptions = new GameOptions();
            IGameRoom game = CreateGameRoom("game1", 5, 10, GameRules.Custom, originalOptions);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, true);
            game.Start(new CancellationTokenSource());
            game.Options.ResetToDefault(); // needed for reset statistics
            game.StartGame();

            game.ModifyGrid(client1, null);
            ActionQueue.DequeueAndExecuteFirstAction();

            Assert.AreEqual(0, callback1.GetCallCount("OnGridModified"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGridModified"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGridModified"));
        }

        #endregion

        #region UseSpecial

        [TestMethod]
        public void TestUseSpecialExceptionIfNullClient()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);

            try
            {
                game.UseSpecial(null, null, Specials.BlockBomb);

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("client", ex.ParamName);
            }
        }

        [TestMethod]
        public void TestUseSpecialExceptionIfNullTarget()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);

            try
            {
                game.UseSpecial(client1, null, Specials.BlockBomb);

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("target", ex.ParamName);
            }
        }

        [TestMethod]
        public void TestUseSpecialFailedIfClientNotInGame()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            Logger.Clear();

            game.UseSpecial(client1, client2, Specials.BlockBomb);

            Assert.AreEqual(LogLevels.Warning, Logger.LastLogLevel);
            Assert.AreEqual(0, ActionQueue.ActionCount);
        }

        [TestMethod]
        public void TestUseSpecialFailedIfTargetNotInGame()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            Logger.Clear();

            game.UseSpecial(client1, client2, Specials.BlockBomb);

            Assert.AreEqual(LogLevels.Warning, Logger.LastLogLevel);
            Assert.AreEqual(0, ActionQueue.ActionCount);
        }

        [TestMethod]
        public void TestUseSpecialFailedIfGameNotStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            game.Start(new CancellationTokenSource());
            Logger.Clear();

            game.UseSpecial(client1, client2, Specials.AddLines);

            Assert.AreEqual(LogLevels.Warning, Logger.LastLogLevel);
            Assert.AreEqual(0, ActionQueue.ActionCount);
        }

        [TestMethod]
        public void TestUseSpecialFailedIfClientNotPlaying()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            game.Start(new CancellationTokenSource());
            game.Options.ResetToDefault(); // needed for reset statistics
            game.StartGame();
            client1.State = ClientStates.GameLost; // simulate game loss

            game.UseSpecial(client1, client2, Specials.AddLines);

            Assert.AreEqual(LogLevels.Warning, Logger.LastLogLevel);
            Assert.AreEqual(0, ActionQueue.ActionCount);
        }

        [TestMethod]
        public void TestUseSpecialFailedIfTargetNotPlaying()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            game.Start(new CancellationTokenSource());
            game.Options.ResetToDefault(); // needed for reset statistics
            game.StartGame();
            client2.State = ClientStates.GameLost; // simulate game loss

            game.UseSpecial(client1, client2, Specials.AddLines);

            Assert.AreEqual(LogLevels.Warning, Logger.LastLogLevel);
            Assert.AreEqual(0, ActionQueue.ActionCount);
        }

        [TestMethod]
        public void TestUseSpecialActionEnqueuedIfGameStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            game.Start(new CancellationTokenSource());
            game.Options.ResetToDefault(); // needed for reset statistics
            game.StartGame();

            game.UseSpecial(client1, client2, Specials.AddLines);

            Assert.AreEqual(1, ActionQueue.ActionCount);
        }

        [TestMethod]
        public void TestUseSpecialActionCallbacksCalled()
        {
            GameOptions originalOptions = new GameOptions();
            IGameRoom game = CreateGameRoom("game1", 5, 10, GameRules.Custom, originalOptions);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);
            game.Start(new CancellationTokenSource());
            game.Options.ResetToDefault(); // needed for reset statistics
            game.StartGame();

            game.UseSpecial(client1, client2, Specials.AddLines);
            ActionQueue.DequeueAndExecuteFirstAction();

            Assert.AreEqual(1, callback1.GetCallCount("OnSpecialUsed"));
            Assert.AreEqual(1, callback2.GetCallCount("OnSpecialUsed"));
            Assert.AreEqual(1, callback3.GetCallCount("OnSpecialUsed"));
        }

        [TestMethod]
        public void TestUseSpecialSwitchFieldsActionCallbacksCalled()
        {
            GameOptions originalOptions = new GameOptions();
            IGameRoom game = CreateGameRoom("game1", 5, 10, GameRules.Custom, originalOptions);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);
            game.Start(new CancellationTokenSource());
            game.Options.ResetToDefault(); // needed for reset statistics
            game.StartGame();

            game.UseSpecial(client1, client2, Specials.SwitchFields);
            ActionQueue.DequeueAndExecuteFirstAction();

            Assert.AreEqual(1, callback1.GetCallCount("OnSpecialUsed"));
            Assert.AreEqual(1, callback2.GetCallCount("OnSpecialUsed"));
            Assert.AreEqual(1, callback3.GetCallCount("OnSpecialUsed"));
            Assert.AreEqual(1, callback1.GetCallCount("OnGridModified"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGridModified"));
            Assert.AreEqual(0, callback3.GetCallCount("OnGridModified"));
        }

        #endregion

        #region ClearLines

        [TestMethod]
        public void TestClearLinesExceptionIfNullClient()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);

            try
            {
                game.ClearLines(null, 4);

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("client", ex.ParamName);
            }
        }

        [TestMethod]
        public void TestClearLinesFailedIfClientNotInGame()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            Logger.Clear();

            game.ClearLines(client1, 4);

            Assert.AreEqual(LogLevels.Warning, Logger.LastLogLevel);
            Assert.AreEqual(0, ActionQueue.ActionCount);
        }

        [TestMethod]
        public void TestClearLinesFailedIfGameNotStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, true);
            game.Start(new CancellationTokenSource());
            Logger.Clear();

            game.ClearLines(client1, 4);

            Assert.AreEqual(LogLevels.Warning, Logger.LastLogLevel);
            Assert.AreEqual(0, ActionQueue.ActionCount);
        }

        [TestMethod]
        public void TestClearLinesFailedIfClientNotPlaying()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.Start(new CancellationTokenSource());
            game.Options.ResetToDefault(); // needed for reset statistics
            game.StartGame();
            client1.State = ClientStates.GameLost; // simulate game loss

            game.ClearLines(client1, 4);

            Assert.AreEqual(LogLevels.Warning, Logger.LastLogLevel);
            Assert.AreEqual(0, ActionQueue.ActionCount);
        }

        [TestMethod]
        public void TestClearLinesActionNotEnqueuedIfClassicMultiplayerRulesNotSet()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.Start(new CancellationTokenSource());
            game.Options.ResetToDefault(); // needed for reset statistics
            game.Options.ClassicStyleMultiplayerRules = false;
            game.StartGame();
            Logger.Clear();

            game.ClearLines(client1, 4);

            Assert.AreEqual(0, ActionQueue.ActionCount);
            Assert.IsNull(Logger.LastLogLine);
        }

        [TestMethod]
        public void TestClearLinesActionEnqueuedIfGameStartedAndClassicMultiplayerRules()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.Start(new CancellationTokenSource());
            game.Options.ResetToDefault(); // needed for reset statistics
            game.Options.ClassicStyleMultiplayerRules = true;
            game.StartGame();

            game.ClearLines(client1, 4);

            Assert.AreEqual(1, ActionQueue.ActionCount);
        }

        [TestMethod]
        public void TestClearLinesActionCallbacksCalled()
        {
            GameOptions originalOptions = new GameOptions();
            IGameRoom game = CreateGameRoom("game1", 5, 10, GameRules.Custom, originalOptions);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, true);
            game.Start(new CancellationTokenSource());
            game.Options.ResetToDefault(); // needed for reset statistics
            game.StartGame();

            game.ModifyGrid(client1, null);
            ActionQueue.DequeueAndExecuteFirstAction();

            Assert.AreEqual(0, callback1.GetCallCount("OnGridModified"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGridModified"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGridModified"));
        }

        #endregion

        #region GameLost

        [TestMethod]
        public void TestGameLostExceptionIfNullClient()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);

            try
            {
                game.GameLost(null);

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("client", ex.ParamName);
            }
        }

        [TestMethod]
        public void TestGameLostFailedIfClientNotInGame()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            Logger.Clear();

            game.GameLost(client1);

            Assert.AreEqual(LogLevels.Warning, Logger.LastLogLevel);
            Assert.AreEqual(0, ActionQueue.ActionCount);
        }

        [TestMethod]
        public void TestGameLostFailedIfClientNotPlaying()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.Start(new CancellationTokenSource());
            game.Options.ResetToDefault(); // needed for reset statistics
            game.StartGame();
            client1.State = ClientStates.GameLost; // simulate game loss

            game.ClearLines(client1, 4);

            Assert.AreEqual(LogLevels.Warning, Logger.LastLogLevel);
            Assert.AreEqual(0, ActionQueue.ActionCount);
        }

        [TestMethod]
        public void TestGameLostActionEnqueuedIfGameStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.Start(new CancellationTokenSource());
            game.Options.ResetToDefault(); // needed for reset statistics
            game.Options.ClassicStyleMultiplayerRules = true;
            game.StartGame();

            game.GameLost(client1);

            Assert.AreEqual(1, ActionQueue.ActionCount);
        }

        #endregion

        // TODO: 
        //  FinishContinuousSpecial, EarnAchievement, StartGame, StopGame, PauseGame, ResumeGame
        //  game started + join/leave/stop/game lost
        //  reset win list after a few win
    }

    [TestClass]
    public class GameRoomUnitTest : AbstractGameRoomUnitTest
    {
        protected override IClient CreateClient(string name, ITetriNETCallback callback)
        {
            return new Client(name, IPAddress.Any, callback);
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
            ActionQueue = actionQueue as ActionQueueMock;
            PieceProvider = pieceProvider as PieceProviderMock;

            return new GameRoom(actionQueue, pieceProvider, name, maxPlayers, maxSpectators, rule, options, password);
        }
    }
}
