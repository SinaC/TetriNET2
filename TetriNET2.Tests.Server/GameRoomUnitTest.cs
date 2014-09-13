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
        protected PieceProviderMock PieceProvider;

        protected abstract IClient CreateClient(string name, ITetriNETCallback callback);
        protected abstract IGameRoom CreateGameRoom(string name, int maxPlayers, int maxSpectators);
        protected abstract IGameRoom CreateGameRoom(string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password = null);

        [TestInitialize]
        public void Initialize()
        {
            Log.Default.Logger = new LogMock();
        }

        #region Join

        [TestMethod]
        public void TestJoinPlayerNoMaxPlayers()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            IClient client = CreateClient("client1", new CountCallTetriNETCallback());

            bool succeed = game.Join(client, false);

            Assert.IsTrue(succeed);
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

            bool succeed = game.Join(client, true);

            Assert.IsTrue(succeed);
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

            bool succeed = game.Join(client2, false);

            Assert.IsFalse(succeed);
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

            bool succeed = game.Join(client2, true);

            Assert.IsFalse(succeed);
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

            bool succeed = game.Join(client1, false);

            Assert.IsFalse(succeed);
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
            bool succeed = game.Leave(client2);

            Assert.IsFalse(succeed);
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

            bool succeed = game.Leave(client1);

            Assert.IsTrue(succeed);
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

            bool succeed = game.Start(new CancellationTokenSource());

            Assert.AreEqual(GameRoomStates.WaitStartGame, game.State);
            Assert.IsFalse(succeed);
        }

        [TestMethod]
        public void TestStopOnlyIfStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 10, 5);

            bool succeed = game.Stop();

            Assert.AreEqual(GameRoomStates.Created, game.State);
            Assert.IsFalse(succeed);
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

            GameOptions newOptions = new GameOptions();
            bool succeed = game.ChangeOptions(newOptions);

            Assert.AreEqual(originalOptions, game.Options);
            Assert.IsFalse(succeed);
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

            bool succeed = game.ResetWinList();

            Assert.AreEqual(0, callback1.GetCallCount("OnWinListModified"));
            Assert.IsFalse(succeed);
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
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Start(new CancellationTokenSource());
            game.StartGame();

            bool succeed = game.PlacePiece(client2, 0, 0, Pieces.TetriminoO, 0, 0, 0, null);

            Assert.IsFalse(succeed);
        }

        [TestMethod]
        public void TestPlacePieceFailedIfGameNotStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, true);
            game.Start(new CancellationTokenSource());

            bool succeed = game.PlacePiece(client1, 0, 0, Pieces.TetriminoO, 0, 0, 0, null);

            Assert.IsFalse(succeed);
        }

        [TestMethod]
        public void TestPlacePieceFailedIfClientNotPlaying()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.Start(new CancellationTokenSource());
            game.StartGame();
            client1.State = ClientStates.GameLost; // simulate game loss

            bool succeed = game.PlacePiece(client1, 0, 0, Pieces.TetriminoO, 0, 0, 0, null);

            Assert.IsFalse(succeed);
        }

        [TestMethod]
        public void TestPlacePieceOkIfGameStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.Start(new CancellationTokenSource());
            game.StartGame();

            bool succeed = game.PlacePiece(client1, 0, 0, Pieces.TetriminoO, 0, 0, 0, null);

            Assert.IsTrue(succeed);
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
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Start(new CancellationTokenSource());
            game.StartGame();

            bool succeed = game.ModifyGrid(client2, null);

            Assert.IsFalse(succeed);
        }

        [TestMethod]
        public void TestModifyGridFailedIfGameNotStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, true);
            game.Start(new CancellationTokenSource());

            bool succeed = game.ModifyGrid(client1, null);

            Assert.IsFalse(succeed);
        }

        [TestMethod]
        public void TestModifyGridFailedIfClientNotPlaying()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.Start(new CancellationTokenSource());
            game.StartGame();
            client1.State = ClientStates.GameLost; // simulate game loss

            bool succeed = game.ModifyGrid(client1, null);

            Assert.IsFalse(succeed);
        }

        [TestMethod]
        public void TestModifyGridOkIfGameStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.Start(new CancellationTokenSource());
            game.StartGame();

            bool succeed = game.ModifyGrid(client1, null);

            Assert.IsTrue(succeed);
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
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Start(new CancellationTokenSource());
            game.StartGame();

            bool succeed = game.UseSpecial(client2, client1, Specials.BlockBomb);

            Assert.IsFalse(succeed);
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
            game.Start(new CancellationTokenSource());
            game.StartGame();

            bool succeed = game.UseSpecial(client1, client2, Specials.BlockBomb);

            Assert.IsFalse(succeed);
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

            bool succeed = game.UseSpecial(client1, client2, Specials.AddLines);

            Assert.IsFalse(succeed);
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
            game.StartGame();
            client1.State = ClientStates.GameLost; // simulate game loss

            bool succeed = game.UseSpecial(client1, client2, Specials.AddLines);

            Assert.IsFalse(succeed);
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
            game.StartGame();
            client2.State = ClientStates.GameLost; // simulate game loss

            bool succeed = game.UseSpecial(client1, client2, Specials.AddLines);

            Assert.IsFalse(succeed);
        }

        [TestMethod]
        public void TestUseSpecialOkIfGameStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            game.Start(new CancellationTokenSource());
            game.StartGame();

            bool succeed = game.UseSpecial(client1, client2, Specials.AddLines);

            Assert.IsTrue(succeed);
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
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Start(new CancellationTokenSource());
            game.StartGame();

            bool succeed = game.ClearLines(client2, 4);

            Assert.IsFalse(succeed);
        }

        [TestMethod]
        public void TestClearLinesFailedIfGameNotStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, true);
            game.Start(new CancellationTokenSource());

            bool succeed = game.ClearLines(client1, 4);

            Assert.IsFalse(succeed);
        }

        [TestMethod]
        public void TestClearLinesFailedIfClientNotPlaying()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.Start(new CancellationTokenSource());
            game.StartGame();
            client1.State = ClientStates.GameLost; // simulate game loss

            bool succeed = game.ClearLines(client1, 4);

            Assert.IsFalse(succeed);
        }

        [TestMethod]
        public void TestClearLinesOkIfGameStartedAndClassicMultiplayerRules()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.Start(new CancellationTokenSource());
            game.Options.ClassicStyleMultiplayerRules = true;
            game.StartGame();

            bool succeed = game.ClearLines(client1, 4);

            Assert.IsTrue(succeed);
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

            bool succeed = game.GameLost(client1);

            Assert.IsFalse(succeed);
        }

        [TestMethod]
        public void TestGameLostFailedIfClientNotPlaying()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.Start(new CancellationTokenSource());
            game.StartGame();
            client1.State = ClientStates.GameLost; // simulate game loss

            bool succeed = game.ClearLines(client1, 4);

            Assert.IsFalse(succeed);
        }

        [TestMethod]
        public void TestGameLostOkIfGameStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.Start(new CancellationTokenSource());
            game.StartGame();

            bool succeed = game.GameLost(client1);

            Assert.IsTrue(succeed);
        }

        #endregion

        #region FinishContinuousSpecial

        [TestMethod]
        public void TestFinishContinuousSpecialExceptionIfNullClient()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);

            try
            {
                game.FinishContinuousSpecial(null, Specials.Darkness);

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("client", ex.ParamName);
            }
        }

        [TestMethod]
        public void TestFinishContinuousSpecialFailedIfClientNotInGame()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Start(new CancellationTokenSource());
            game.StartGame();

            bool succeed = game.FinishContinuousSpecial(client2, Specials.Darkness);

            Assert.IsFalse(succeed);
        }

        [TestMethod]
        public void TestFinishContinuousSpecialOkIfGameStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.Start(new CancellationTokenSource());
            game.StartGame();

            bool succeed = game.FinishContinuousSpecial(client1, Specials.Darkness);

            Assert.IsTrue(succeed);
        }

        #endregion

        #region EarnAchievement

        [TestMethod]
        public void TestEarnAchievementSpecialExceptionIfNullClient()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);

            try
            {
                game.EarnAchievement(null, 5, "achievement");

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("client", ex.ParamName);
            }
        }

        [TestMethod]
        public void TestEarnAchievementSpecialFailedIfClientNotInGame()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Start(new CancellationTokenSource());
            game.StartGame();

            bool succeed = game.EarnAchievement(client2, 5, "achievement");

            Assert.IsFalse(succeed);
        }
        
        [TestMethod]
        public void TestEarnAchievement()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
            game.StartGame();

            bool succeed = game.EarnAchievement(client1, 5, "achievement");

            Assert.IsTrue(succeed);
        }

        #endregion

        #region StartGame

        [TestMethod]
        public void TestStartGameFailedIfNotWaitingGameStart()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, true);

            bool succeed = game.StartGame();

            Assert.IsFalse(succeed);
            Assert.AreEqual(GameRoomStates.Created, game.State);
        }

        [TestMethod]
        public void TestStartGameFailedIfNoPlayers()
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
            game.Join(client3, true);
            game.Start(new CancellationTokenSource());

            bool succeed = game.StartGame();

            Assert.IsFalse(succeed);
            Assert.AreEqual(GameRoomStates.WaitStartGame, game.State);
        }

        [TestMethod]
        public void TestStartGameStatusUpdatedAndCallbackCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, true);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);
            game.Start(new CancellationTokenSource());

            bool succeed = game.StartGame();

            Assert.IsTrue(succeed);
            Assert.AreEqual(GameRoomStates.GameStarted, game.State);
            Assert.AreEqual(1, callback1.GetCallCount("OnGameStarted"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGameStarted"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGameStarted"));
            Assert.AreEqual(ClientStates.Playing, client1.State);
            Assert.AreNotEqual(ClientStates.Playing, client2.State);
            Assert.AreEqual(ClientStates.Playing, client3.State);
        }

        #endregion

        #region StopGame

        [TestMethod]
        public void TestStopGameFailedIfGameNotStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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

            bool succeed = game.StopGame();

            Assert.IsFalse(succeed);
            Assert.AreEqual(GameRoomStates.WaitStartGame, game.State);
        }

        [TestMethod]
        public void TestStopGameStatusUpdatedAndCallbackCalledIfGameStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, true);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);
            game.Start(new CancellationTokenSource());
            game.StartGame();

            bool succeed = game.StopGame();

            Assert.IsTrue(succeed);
            Assert.AreEqual(GameRoomStates.WaitStartGame, game.State);
            Assert.AreEqual(1, callback1.GetCallCount("OnGameFinished"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGameFinished"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGameFinished"));
            Assert.AreEqual(ClientStates.WaitInGameRoom, client1.State);
            Assert.AreEqual(ClientStates.WaitInGameRoom, client3.State);
        }

        #endregion

        #region PauseGame

        [TestMethod]
        public void TestPauseGameFailedIfGameNotStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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

            bool succeed = game.PauseGame();

            Assert.IsFalse(succeed);
            Assert.AreEqual(GameRoomStates.WaitStartGame, game.State);
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
        }

        [TestMethod]
        public void TestPauseGameStatusUpdatedAndCallbacksCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
            game.StartGame();

            bool succeed = game.PauseGame();

            Assert.IsTrue(succeed);
            Assert.AreEqual(GameRoomStates.GamePaused, game.State);
            Assert.AreEqual(1, callback1.GetCallCount("OnGamePaused"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGamePaused"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGamePaused"));
        }

        #endregion

        #region ResumeGame

        [TestMethod]
        public void TestResumeGameFailedIfGameNotStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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

            bool succeed = game.ResumeGame();

            Assert.IsFalse(succeed);
            Assert.AreEqual(GameRoomStates.WaitStartGame, game.State);
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
        }

        [TestMethod]
        public void TestResumeGameFailedIfGameNotPaused()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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

            bool succeed = game.ResumeGame();

            Assert.IsFalse(succeed);
            Assert.AreEqual(GameRoomStates.WaitStartGame, game.State);
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
        }

        [TestMethod]
        public void TestResumeGameStatusUpdatedAndCallbacksCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
            game.StartGame();
            game.PauseGame();

            bool succeed = game.ResumeGame();

            Assert.IsTrue(succeed);
            Assert.AreEqual(GameRoomStates.GameStarted, game.State);
            Assert.AreEqual(1, callback1.GetCallCount("OnGameResumed"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGameResumed"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGameResumed"));
        }

        #endregion

        // TODO: 
        //  StartGame, StopGame
        //  game started + join/leave/stop/game lost
        //  reset win list after a few win, win list updated after a win, ...
    }

    [TestClass]
    public class GameRoomUnitTest : AbstractGameRoomUnitTest
    {
        protected ActionQueueMock ActionQueue;

        protected override IClient CreateClient(string name, ITetriNETCallback callback)
        {
            return new Client(name, IPAddress.Any, callback);
        }

        protected override IGameRoom CreateGameRoom(string name, int maxPlayers, int maxSpectators)
        {
            GameOptions options = new GameOptions();
            options.ResetToDefault();
            return CreateGameRoom(name, maxPlayers, maxSpectators, GameRules.Classic, options);
        }

        protected override IGameRoom CreateGameRoom(string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password = null)
        {
            return CreateGameRoom(new ActionQueueMock(), new PieceProviderMock(), name, maxPlayers, maxSpectators, rule, options, password);
        }

        protected IGameRoom CreateGameRoom(IActionQueue actionQueue, IPieceProvider pieceProvider, string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password = null)
        {
            ActionQueue = actionQueue as ActionQueueMock;
            PieceProvider = pieceProvider as PieceProviderMock;

            return new GameRoom(actionQueue, pieceProvider, name, maxPlayers, maxSpectators, rule, options, password);
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

        #region PlacePiece

        [TestMethod]
        public void TestPlacePieceActionCallbacksCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
        public void TestModifyGridActionCallbacksCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
        public void TestUseSpecialActionCallbacksCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
        public void TestClearLinesActionCallbacksCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
            game.StartGame();

            game.ModifyGrid(client1, null);
            ActionQueue.DequeueAndExecuteFirstAction();

            Assert.AreEqual(0, callback1.GetCallCount("OnGridModified"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGridModified"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGridModified"));
        }

        [TestMethod]
        public void TestClearLinesActionNotEnqueuedIfClassicMultiplayerRulesNotSet()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.Start(new CancellationTokenSource());
            game.Options.ClassicStyleMultiplayerRules = false;
            game.StartGame();

            bool succeed = game.ClearLines(client1, 4);

            Assert.IsTrue(succeed);
            Assert.AreEqual(0, ActionQueue.ActionCount);
        }

        #endregion

        #region FinishContinuousSpecial

        [TestMethod]
        public void TestFinishContinuousSpecialActionCallbacksCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
            game.StartGame();

            game.FinishContinuousSpecial(client1, Specials.Darkness);
            ActionQueue.DequeueAndExecuteFirstAction();

            Assert.AreEqual(0, callback1.GetCallCount("OnContinuousSpecialFinished"));
            Assert.AreEqual(1, callback2.GetCallCount("OnContinuousSpecialFinished"));
            Assert.AreEqual(1, callback3.GetCallCount("OnContinuousSpecialFinished"));
        }

        #endregion

        #region EarnAchievement

        [TestMethod]
        public void TestEarnAchievementCallbacksCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
            game.StartGame();

            game.EarnAchievement(client1, 5, "achievement");

            Assert.AreEqual(0, callback1.GetCallCount("OnAchievementEarned"));
            Assert.AreEqual(1, callback2.GetCallCount("OnAchievementEarned"));
            Assert.AreEqual(1, callback3.GetCallCount("OnAchievementEarned"));
        }

        #endregion
    }
}
