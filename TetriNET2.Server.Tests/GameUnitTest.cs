using System;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET2.Common.ActionQueue;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Logger;
using TetriNET2.Server.Interfaces;
using TetriNET2.Server.Tests.Mocking;

namespace TetriNET2.Server.Tests
{
    [TestClass]
    public abstract class AbstractGameUnitTest
    {
        protected abstract IClient CreateClient(string name, ITetriNETClientCallback callback);
        protected abstract IGame CreateGame(string name, int maxPlayers, int maxSpectators);
        protected abstract IGame CreateGame(string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password = null);
        protected abstract void FlushActionQueue();

        [TestInitialize]
        public void Initialize()
        {
            Log.Default.Logger = new LogMock();
        }

        #region Start

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Start")]
        [TestMethod]
        public void TestStartChangeState()
        {
            IGame game = CreateGame("game1", 10, 5);

            game.Start(new CancellationTokenSource());

            Assert.AreEqual(GameStates.WaitStartGame, game.State);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Start")]
        [TestMethod]
        public void TestStartOnlyIfCreated()
        {
            IGame game = CreateGame("game1", 10, 5);
            game.Start(new CancellationTokenSource());

            bool succeed = game.Start(new CancellationTokenSource());

            Assert.AreEqual(GameStates.WaitStartGame, game.State);
            Assert.IsFalse(succeed);
        }

        #endregion

        #region Stop

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Stop")]
        [TestMethod]
        public void TestStopOnlyIfStarted()
        {
            IGame game = CreateGame("game1", 10, 5);

            bool succeed = game.Stop();

            Assert.AreEqual(GameStates.Created, game.State);
            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Stop")]
        [TestMethod]
        public void TestStopClientsRemovedAndStatusChanged()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, true);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, true);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);

            game.Stop();

            Assert.AreEqual(GameStates.Created, game.State);
            Assert.AreEqual(0, game.ClientCount);
            Assert.IsNull(client1.Game);
            Assert.IsNull(client2.Game);
            Assert.IsNull(client3.Game);
            Assert.AreEqual(ClientStates.Connected, client1.State);
            Assert.AreEqual(ClientStates.Connected, client2.State);
            Assert.AreEqual(ClientStates.Connected, client3.State);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Stop")]
        [TestMethod]
        public void TestStopCallbackCalled()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, true);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, true);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);

            game.Stop();

            Assert.AreEqual(game.State, GameStates.Created);
            Assert.AreEqual(1, callback1.GetCallCount("OnGameLeft"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGameLeft"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGameLeft"));
        }

        #endregion

        #region Join

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Join")]
        [TestMethod]
        public void TestJoinNullClient()
        {
            IGame game = CreateGame("game1", 5, 10);

            try
            {
                game.Join(null, false);

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("client", ex.ParamName);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Join")]
        [TestMethod]
        public void TestJoinGameNoStarted()
        {
            IGame game = CreateGame("game1", 5, 10);
            IClient client = CreateClient("client1", new CountCallTetriNETCallback());

            bool succeed = game.Join(client, false);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Join")]
        [TestMethod]
        public void TestJoinPlayerNoMaxPlayers()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client = CreateClient("client1", new CountCallTetriNETCallback());

            bool succeed = game.Join(client, false);

            Assert.IsTrue(succeed);
            Assert.AreEqual(1, game.PlayerCount);
            Assert.AreEqual(0, game.SpectatorCount);
            Assert.AreEqual(1, game.Players.Count());
            Assert.AreEqual(0, game.Spectators.Count());
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Join")]
        [TestMethod]
        public void TestJoinSpectatorNoMaxSpectators()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client = CreateClient("client1", new CountCallTetriNETCallback());

            bool succeed = game.Join(client, true);

            Assert.IsTrue(succeed);
            Assert.AreEqual(0, game.PlayerCount);
            Assert.AreEqual(1, game.SpectatorCount);
            Assert.AreEqual(0, game.Players.Count());
            Assert.AreEqual(1, game.Spectators.Count());
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Join")]
        [TestMethod]
        public void TestJoinPlayerMaxPlayers()
        {
            IGame game = CreateGame("game1", 1, 10);
            game.Start(new CancellationTokenSource());
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

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Join")]
        [TestMethod]
        public void TestJoinSpectatorMaxSpectators()
        {
            IGame game = CreateGame("game1", 5, 1);
            game.Start(new CancellationTokenSource());
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

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Join")]
        [TestMethod]
        public void TestJoinSameClient()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, true);

            bool succeed = game.Join(client1, false);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Join")]
        [TestMethod]
        public void TestJoinPlayerModifyPlayerProperties()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client = CreateClient("client1", new CountCallTetriNETCallback());
            
            game.Join(client, false);

            Assert.AreEqual(ClientStates.WaitInGame, client.State);
            Assert.IsTrue(client.IsPlayer);
            Assert.IsTrue(client.IsGameMaster);
            Assert.AreEqual(game, client.Game);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Join")]
        [TestMethod]
        public void TestJoinSpectatorModifySpectatorProperties()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client = CreateClient("client1", new CountCallTetriNETCallback());

            game.Join(client, true);

            Assert.AreEqual(ClientStates.WaitInGame, client.State);
            Assert.IsTrue(client.IsSpectator);
            Assert.IsFalse(client.IsGameMaster);
            Assert.AreEqual(game, client.Game);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Join")]
        [TestMethod]
        public void TestJoinPlayerCallbackCalled()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback = new CountCallTetriNETCallback();
            IClient client = CreateClient("client1", callback);

            game.Join(client, false);

            Assert.AreEqual(1, callback.GetCallCount("OnGameJoined"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Join")]
        [TestMethod]
        public void TestJoinSpectatorCallbackCalled()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback = new CountCallTetriNETCallback();
            IClient client = CreateClient("client1", callback);

            game.Join(client, true);

            Assert.AreEqual(1, callback.GetCallCount("OnGameJoined"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Join")]
        [TestMethod]
        public void TestJoinPlayerOtherClientsInformed()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
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
            Assert.AreEqual(1, callback1.GetCallCount("OnGameMasterModified"));
            Assert.AreEqual(0, callback2.GetCallCount("OnGameMasterModified"));
            Assert.AreEqual(0, callback3.GetCallCount("OnGameMasterModified"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Join")]
        [TestMethod]
        public void TestJoinSpectatorOtherClientsInformed()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
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
            Assert.AreEqual(1, callback1.GetCallCount("OnGameMasterModified"));
            Assert.AreEqual(0, callback2.GetCallCount("OnGameMasterModified"));
            Assert.AreEqual(0, callback3.GetCallCount("OnGameMasterModified"));
        }

        #endregion

        #region Leave

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Leave")]
        [TestMethod]
        public void TestLeaveNullClient()
        {
            IGame game = CreateGame("game1", 5, 10);

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

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Leave")]
        [TestMethod]
        public void TestLeaveNonExistingClient()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, false);

            IClient client2 = CreateClient("client2", new CountCallTetriNETCallback());
            bool succeed = game.Leave(client2);

            Assert.IsFalse(succeed);
            Assert.AreEqual(1, game.ClientCount);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Leave")]
        [TestMethod]
        public void TestLeaveClientNotInGame()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            
            bool succeed = game.Leave(client1);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Leave")]
        [TestMethod]
        public void TestLeaveExistingClient()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
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

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Leave")]
        [TestMethod]
        public void TestLeavePlayerChangeGameMaster()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client1, false);
            game.Join(client2, false);
            game.Join(client3, true);

            bool succeed = game.Leave(client1);

            Assert.IsTrue(succeed);
            Assert.AreEqual(2, game.ClientCount);
            Assert.AreEqual(1, game.PlayerCount);
            Assert.AreEqual(1, game.SpectatorCount);
            Assert.AreEqual(client2, game.Clients.First());
            Assert.IsFalse(client1.IsGameMaster);
            Assert.IsTrue(client2.IsGameMaster);
            Assert.AreEqual(0, callback1.GetCallCount("OnGameMasterModified"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGameMasterModified"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGameMasterModified"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Leave")]
        [TestMethod]
        public void TestLeaveSpectatorNoChangeGameMaster()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            IClient client2 = CreateClient("client2", new CountCallTetriNETCallback());
            IClient client3 = CreateClient("client3", new CountCallTetriNETCallback());
            game.Join(client1, true);
            game.Join(client2, false);
            game.Join(client3, false);

            bool succeed = game.Leave(client1);

            Assert.IsTrue(succeed);
            Assert.AreEqual(2, game.ClientCount);
            Assert.AreEqual(2, game.PlayerCount);
            Assert.AreEqual(0, game.SpectatorCount);
            Assert.AreEqual(client2, game.Clients.First());
            Assert.IsTrue(client2.IsGameMaster);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Leave")]
        [TestMethod]
        public void TestLeavePlayerModifyPlayerProperties()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client, false);

            game.Leave(client);

            Assert.AreEqual(ClientStates.Connected, client.State);
            Assert.AreEqual(ClientRoles.NoRole, client.Roles);
            Assert.IsFalse(client.LastVoteKickAnswer.HasValue);
            Assert.IsNull(client.Game);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Leave")]
        [TestMethod]
        public void TestLeaveSpectatorModifySpectatorProperties()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client, true);

            game.Leave(client);

            Assert.AreEqual(ClientStates.Connected, client.State);
            Assert.AreEqual(ClientRoles.NoRole, client.Roles);
            Assert.IsFalse(client.LastVoteKickAnswer.HasValue);
            Assert.IsNull(client.Game);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Leave")]
        [TestMethod]
        public void TestLeaveClientCallbackCalled()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client = CreateClient("client1", callback1);
            game.Join(client, true);

            game.Leave(client);

            Assert.AreEqual(1, callback1.GetCallCount("OnGameLeft"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Leave")]
        [TestMethod]
        public void TestLeaveClientOtherClientsInformed()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
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

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Clear")]
        [TestMethod]
        public void TestClearNoClients()
        {
            IGame game = CreateGame("game1", 5, 10);

            game.Clear();

            Assert.AreEqual(0, game.ClientCount);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Clear")]
        [TestMethod]
        public void TestClearMultipleClients()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            game.Join(CreateClient("client1", new CountCallTetriNETCallback()), true);
            game.Join(CreateClient("client2", new CountCallTetriNETCallback()), true);
            game.Join(CreateClient("client3", new CountCallTetriNETCallback()), true);

            game.Clear();

            Assert.AreEqual(0, game.ClientCount);
        }

        #endregion

        #region VoteKick

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.VoteKick")]
        [TestMethod]
        public void TestVoteKickNullClient()
        {
            IGame game = CreateGame("game1", 5, 10);

            try
            {
                game.VoteKick(null, null, null);

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("client", ex.ParamName);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.VoteKick")]
        [TestMethod]
        public void TestVoteKickNullTarget()
        {
            IGame game = CreateGame("game1", 5, 10);
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());

            try
            {
                game.VoteKick(client1, null, null);

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("target", ex.ParamName);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.VoteKick")]
        [TestMethod]
        public void TestVoteKickClientNotInGame()
        {
            IGame game = CreateGame("game1", 5, 10);
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            IClient client2 = CreateClient("client2", new CountCallTetriNETCallback());

            bool succeed = game.VoteKick(client1, client2, "reason");

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.VoteKick")]
        [TestMethod]
        public void TestVoteKickTargetNotInGame()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, false);
            IClient client2 = CreateClient("client2", new CountCallTetriNETCallback());

            bool succeed = game.VoteKick(client1, client2, "reason");

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.VoteKick")]
        [TestMethod]
        public void TestVoteKickClientNotPlayer()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, true);
            IClient client2 = CreateClient("client2", new CountCallTetriNETCallback());
            game.Join(client2, false);

            bool succeed = game.VoteKick(client1, client2, "reason");

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.VoteKick")]
        [TestMethod]
        public void TestVoteKickTargetNotPlayer()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, false);
            IClient client2 = CreateClient("client2", new CountCallTetriNETCallback());
            game.Join(client2, true);

            bool succeed = game.VoteKick(client1, client2, "reason");

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.VoteKick")]
        [TestMethod]
        public void TestVoteKickNotEnoughPlayers()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, false);
            IClient client2 = CreateClient("client2", new CountCallTetriNETCallback());
            game.Join(client2, false);

            bool succeed = game.VoteKick(client1, client2, "reason");

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.VoteKick")]
        [TestMethod]
        public void TestVoteKickNoSimultaneousVoteKick()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, false);
            IClient client2 = CreateClient("client2", new CountCallTetriNETCallback());
            game.Join(client2, false);
            IClient client3 = CreateClient("client3", new CountCallTetriNETCallback());
            game.Join(client3, false);
            game.VoteKick(client1, client2, "reason");

            bool succeed = game.VoteKick(client1, client2, "reason");

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.VoteKick")]
        [TestMethod]
        public void TestVoteKickPropertiesModified()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);

            bool succeed = game.VoteKick(client1, client2, "reason");

            Assert.IsTrue(succeed);
            Assert.IsNotNull(client1.LastVoteKickAnswer);
            Assert.IsTrue(client1.LastVoteKickAnswer.Value);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.VoteKick")]
        [TestMethod]
        public void TestVoteKickCallbackCalled()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);
            CountCallTetriNETCallback callback4 = new CountCallTetriNETCallback();
            IClient client4 = CreateClient("client4", callback4);
            game.Join(client4, true);

            bool succeed = game.VoteKick(client1, client2, "reason");

            Assert.IsTrue(succeed);
            Assert.AreEqual(0, callback1.GetCallCount("OnVoteKickAsked")); // vote initiator automatically answers yes
            Assert.AreEqual(0, callback2.GetCallCount("OnVoteKickAsked")); // target is not warned
            Assert.AreEqual(1, callback3.GetCallCount("OnVoteKickAsked")); // other players are asked about vote
            Assert.AreEqual(0, callback4.GetCallCount("OnVoteKickAsked")); // spectators are not asked about vote
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.VoteKick")]
        [TestMethod]
        public void TestVoteKickTimeout()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);

            game.VoteKick(client1, client2, "reason");
            Thread.Sleep(10500);

            Assert.IsNull(client1.LastVoteKickAnswer);
            Assert.IsNull(client2.LastVoteKickAnswer);
            Assert.IsNull(client3.LastVoteKickAnswer);
            Assert.AreEqual(0, callback1.GetCallCount("OnClientGameLeft"));
            Assert.AreEqual(0, callback2.GetCallCount("OnGameLeft"));
            Assert.AreEqual(0, callback3.GetCallCount("OnClientGameLeft"));
        }

        #endregion

        #region VoteKickAnswer

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.VoteKickAnswer")]
        [TestMethod]
        public void TestVoteKickAnswerNullClient()
        {
            IGame game = CreateGame("game1", 5, 10);

            try
            {
                game.VoteKickAnswer(null, false);

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("client", ex.ParamName);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.VoteKickAnswer")]
        [TestMethod]
        public void TestVoteKickAnswerClientNotPlayer()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, true);

            bool succeed = game.VoteKickAnswer(client1, false);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.VoteKickAnswer")]
        [TestMethod]
        public void TestVoteKickAnswerNoVoteKickStarted()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, false);

            bool succeed = game.VoteKickAnswer(client1, false);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.VoteKickAnswer")]
        [TestMethod]
        public void TestVoteKickAnswerVoteTargetCannotVote()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);

            bool succeed = game.VoteKickAnswer(client2, false);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.VoteKickAnswer")]
        [TestMethod]
        public void TestVoteKickAnswerNoMultipleAnswerForSamePlayer()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);
            CountCallTetriNETCallback callback4 = new CountCallTetriNETCallback();
            IClient client4 = CreateClient("client4", callback4);
            game.Join(client4, false);
            game.VoteKick(client1, client2, "reason");
            game.VoteKickAnswer(client3, false);

            bool succeed = game.VoteKickAnswer(client3, false);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.VoteKickAnswer")]
        [TestMethod]
        public void TestVoteKickAnswerNotLastToAnswer()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, false);
            IClient client2 = CreateClient("client2", new CountCallTetriNETCallback());
            game.Join(client2, false);
            IClient client3 = CreateClient("client3", new CountCallTetriNETCallback());
            game.Join(client3, false);
            IClient client4 = CreateClient("client4", new CountCallTetriNETCallback());
            game.Join(client4, false);
            game.VoteKick(client1, client2, "reason");

            bool succeed = game.VoteKickAnswer(client3, false);

            Assert.IsTrue(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.VoteKickAnswer")]
        [TestMethod]
        public void TestVoteKickAnswerLastToAnswerNotKicked()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);
            CountCallTetriNETCallback callback4 = new CountCallTetriNETCallback();
            IClient client4 = CreateClient("client4", callback4);
            game.Join(client4, false);
            game.VoteKick(client1, client2, "reason");
            game.VoteKickAnswer(client4, false);

            bool succeed = game.VoteKickAnswer(client3, false);

            Assert.IsTrue(succeed);
            Assert.IsNull(client1.LastVoteKickAnswer);
            Assert.IsNull(client2.LastVoteKickAnswer);
            Assert.IsNull(client3.LastVoteKickAnswer);
            Assert.IsNull(client4.LastVoteKickAnswer);
            Assert.AreEqual(0, callback1.GetCallCount("OnClientGameLeft"));
            Assert.AreEqual(0, callback2.GetCallCount("OnGameLeft"));
            Assert.AreEqual(0, callback3.GetCallCount("OnClientGameLeft"));
            Assert.AreEqual(0, callback4.GetCallCount("OnClientGameLeft"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.VoteKickAnswer")]
        [TestMethod]
        public void TestVoteKickAnswerLastToAnswerKicked()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);
            CountCallTetriNETCallback callback4 = new CountCallTetriNETCallback();
            IClient client4 = CreateClient("client4", callback4);
            game.Join(client4, false);
            game.VoteKick(client1, client2, "reason");
            game.VoteKickAnswer(client4, true);

            bool succeed = game.VoteKickAnswer(client3, true);

            Assert.IsTrue(succeed);
            Assert.IsNull(client1.LastVoteKickAnswer);
            Assert.IsNull(client2.LastVoteKickAnswer);
            Assert.IsNull(client3.LastVoteKickAnswer);
            Assert.IsNull(client4.LastVoteKickAnswer);
            Assert.AreEqual(1, callback1.GetCallCount("OnClientGameLeft"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGameLeft"));
            Assert.AreEqual(1, callback3.GetCallCount("OnClientGameLeft"));
            Assert.AreEqual(1, callback4.GetCallCount("OnClientGameLeft"));
        }

        #endregion

        #region PlacePiece

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.PlacePiece")]
        [TestMethod]
        public void TestPlacePieceExceptionIfNullClient()
        {
            IGame game = CreateGame("game1", 5, 10);

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

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.PlacePiece")]
        [TestMethod]
        public void TestPlacePieceFailedIfClientNotInGame()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.StartGame(client1);

            bool succeed = game.PlacePiece(client2, 0, 0, Pieces.TetriminoO, 0, 0, 0, null);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.PlacePiece")]
        [TestMethod]
        public void TestPlacePieceFailedIfGameNotStarted()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, true);

            bool succeed = game.PlacePiece(client1, 0, 0, Pieces.TetriminoO, 0, 0, 0, null);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.PlacePiece")]
        [TestMethod]
        public void TestPlacePieceFailedIfClientNotPlaying()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.StartGame(client1);
            client1.State = ClientStates.GameLost; // simulate game loss

            bool succeed = game.PlacePiece(client1, 0, 0, Pieces.TetriminoO, 0, 0, 0, null);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.PlacePiece")]
        [TestMethod]
        public void TestPlacePieceOkIfGameStarted()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.StartGame(client1);

            bool succeed = game.PlacePiece(client1, 0, 0, Pieces.TetriminoO, 0, 0, 0, null);

            Assert.IsTrue(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.PlacePiece")]
        [TestMethod]
        public void TestPlacePieceActionCallbacksCalled()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, true);
            game.StartGame(client1);

            game.PlacePiece(client1, 0, 0, Pieces.TetriminoO, 0, 0, 0, null);
            FlushActionQueue();

            Assert.AreEqual(1, callback1.GetCallCount("OnPiecePlaced"));
            Assert.AreEqual(0, callback2.GetCallCount("OnPiecePlaced"));
            Assert.AreEqual(0, callback3.GetCallCount("OnPiecePlaced"));
            Assert.AreEqual(0, callback1.GetCallCount("OnGridModified"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGridModified"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGridModified"));
        }

        #endregion

        #region ModifyGrid

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ModifyGrid")]
        [TestMethod]
        public void TestModifyGridExceptionIfNullClient()
        {
            IGame game = CreateGame("game1", 5, 10);

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

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ModifyGrid")]
        [TestMethod]
        public void TestModifyGridFailedIfClientNotInGame()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.StartGame(client1);

            bool succeed = game.ModifyGrid(client2, null);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ModifyGrid")]
        [TestMethod]
        public void TestModifyGridFailedIfGameNotStarted()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, true);

            bool succeed = game.ModifyGrid(client1, null);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ModifyGrid")]
        [TestMethod]
        public void TestModifyGridFailedIfClientNotPlaying()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.StartGame(client1);
            client1.State = ClientStates.GameLost; // simulate game loss

            bool succeed = game.ModifyGrid(client1, null);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ModifyGrid")]
        [TestMethod]
        public void TestModifyGridOkIfGameStarted()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.StartGame(client1);

            bool succeed = game.ModifyGrid(client1, null);

            Assert.IsTrue(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ModifyGrid")]
        [TestMethod]
        public void TestModifyGridActionCallbacksCalled()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, true);
            game.StartGame(client1);

            game.ModifyGrid(client1, null);
            FlushActionQueue();

            Assert.AreEqual(0, callback1.GetCallCount("OnGridModified"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGridModified"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGridModified"));
        }

        #endregion

        #region UseSpecial

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.UseSpecial")]
        [TestMethod]
        public void TestUseSpecialExceptionIfNullClient()
        {
            IGame game = CreateGame("game1", 5, 10);

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

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.UseSpecial")]
        [TestMethod]
        public void TestUseSpecialExceptionIfNullTarget()
        {
            IGame game = CreateGame("game1", 5, 10);
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

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.UseSpecial")]
        [TestMethod]
        public void TestUseSpecialFailedIfClientNotInGame()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.StartGame(client1);

            bool succeed = game.UseSpecial(client2, client1, Specials.BlockBomb);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.UseSpecial")]
        [TestMethod]
        public void TestUseSpecialFailedIfTargetNotInGame()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.StartGame(client1);

            bool succeed = game.UseSpecial(client1, client2, Specials.BlockBomb);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.UseSpecial")]
        [TestMethod]
        public void TestUseSpecialFailedIfGameNotStarted()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);

            bool succeed = game.UseSpecial(client1, client2, Specials.AddLines);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.UseSpecial")]
        [TestMethod]
        public void TestUseSpecialFailedIfClientNotPlaying()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            game.StartGame(client1);
            client1.State = ClientStates.GameLost; // simulate game loss

            bool succeed = game.UseSpecial(client1, client2, Specials.AddLines);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.UseSpecial")]
        [TestMethod]
        public void TestUseSpecialFailedIfTargetNotPlaying()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            game.StartGame(client1);
            client2.State = ClientStates.GameLost; // simulate game loss

            bool succeed = game.UseSpecial(client1, client2, Specials.AddLines);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.UseSpecial")]
        [TestMethod]
        public void TestUseSpecialOkIfGameStarted()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            game.StartGame(client1);

            bool succeed = game.UseSpecial(client1, client2, Specials.AddLines);

            Assert.IsTrue(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.UseSpecial")]
        [TestMethod]
        public void TestUseSpecialActionCallbacksCalled()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);
            game.StartGame(client1);

            game.UseSpecial(client1, client2, Specials.AddLines);
            FlushActionQueue();

            Assert.AreEqual(1, callback1.GetCallCount("OnSpecialUsed"));
            Assert.AreEqual(1, callback2.GetCallCount("OnSpecialUsed"));
            Assert.AreEqual(1, callback3.GetCallCount("OnSpecialUsed"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.UseSpecial")]
        [TestMethod]
        public void TestUseSpecialSwitchFieldsActionCallbacksCalled()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);
            game.StartGame(client1);

            game.UseSpecial(client1, client2, Specials.SwitchFields);
            FlushActionQueue();

            Assert.AreEqual(1, callback1.GetCallCount("OnSpecialUsed"));
            Assert.AreEqual(1, callback2.GetCallCount("OnSpecialUsed"));
            Assert.AreEqual(1, callback3.GetCallCount("OnSpecialUsed"));
            Assert.AreEqual(1, callback1.GetCallCount("OnGridModified"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGridModified"));
            Assert.AreEqual(0, callback3.GetCallCount("OnGridModified"));
        }

        #endregion

        #region ClearLines

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ClearLines")]
        [TestMethod]
        public void TestClearLinesExceptionIfNullClient()
        {
            IGame game = CreateGame("game1", 5, 10);

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

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ClearLines")]
        [TestMethod]
        public void TestClearLinesFailedIfClientNotInGame()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.StartGame(client1);

            bool succeed = game.ClearLines(client2, 4);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ClearLines")]
        [TestMethod]
        public void TestClearLinesFailedIfGameNotStarted()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, true);

            bool succeed = game.ClearLines(client1, 4);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ClearLines")]
        [TestMethod]
        public void TestClearLinesFailedIfClientNotPlaying()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.StartGame(client1);
            client1.State = ClientStates.GameLost; // simulate game loss

            bool succeed = game.ClearLines(client1, 4);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ClearLines")]
        [TestMethod]
        public void TestClearLinesOkIfGameStartedAndClassicMultiplayerRules()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.Options.ClassicStyleMultiplayerRules = true;
            game.StartGame(client1);

            bool succeed = game.ClearLines(client1, 4);

            Assert.IsTrue(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ClearLines")]
        [TestMethod]
        public void TestClearLinesActionCallbacksCalled()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, true);
            game.StartGame(client1);

            game.ModifyGrid(client1, null);
            FlushActionQueue();

            Assert.AreEqual(0, callback1.GetCallCount("OnGridModified"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGridModified"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGridModified"));
        }

        #endregion

        #region GameLost

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.GameLost")]
        [TestMethod]
        public void TestGameLostExceptionIfNullClient()
        {
            IGame game = CreateGame("game1", 5, 10);

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

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.GameLost")]
        [TestMethod]
        public void TestGameLostFailedIfClientNotInGame()
        {
            IGame game = CreateGame("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);

            bool succeed = game.GameLost(client1);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.GameLost")]
        [TestMethod]
        public void TestGameLostFailedIfClientNotPlaying()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.StartGame(client1);
            client1.State = ClientStates.GameLost; // simulate game loss

            bool succeed = game.ClearLines(client1, 4);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.GameLost")]
        [TestMethod]
        public void TestGameLostOkIfGameStarted()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.StartGame(client1);

            bool succeed = game.GameLost(client1);

            Assert.IsTrue(succeed);
        }

        #endregion

        #region FinishContinuousSpecial

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.FinishContinuousSpecial")]
        [TestMethod]
        public void TestFinishContinuousSpecialExceptionIfNullClient()
        {
            IGame game = CreateGame("game1", 5, 10);

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

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.FinishContinuousSpecial")]
        [TestMethod]
        public void TestFinishContinuousSpecialFailedIfClientNotInGame()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.StartGame(client1);

            bool succeed = game.FinishContinuousSpecial(client2, Specials.Darkness);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.FinishContinuousSpecial")]
        [TestMethod]
        public void TestFinishContinuousSpecialOkIfGameStarted()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.StartGame(client1);

            bool succeed = game.FinishContinuousSpecial(client1, Specials.Darkness);

            Assert.IsTrue(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.FinishContinuousSpecial")]
        [TestMethod]
        public void TestFinishContinuousSpecialActionCallbacksCalled()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, true);
            game.StartGame(client1);

            game.FinishContinuousSpecial(client1, Specials.Darkness);
            FlushActionQueue();

            Assert.AreEqual(0, callback1.GetCallCount("OnContinuousSpecialFinished"));
            Assert.AreEqual(1, callback2.GetCallCount("OnContinuousSpecialFinished"));
            Assert.AreEqual(1, callback3.GetCallCount("OnContinuousSpecialFinished"));
        }

        #endregion

        #region EarnAchievement

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.EarnAchievement")]
        [TestMethod]
        public void TestEarnAchievementSpecialExceptionIfNullClient()
        {
            IGame game = CreateGame("game1", 5, 10);

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

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.EarnAchievement")]
        [TestMethod]
        public void TestEarnAchievementSpecialFailedIfClientNotInGame()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.StartGame(client1);

            bool succeed = game.EarnAchievement(client2, 5, "achievement");

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.EarnAchievement")]
        [TestMethod]
        public void TestEarnAchievement()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, true);
            game.StartGame(client1);

            bool succeed = game.EarnAchievement(client1, 5, "achievement");

            Assert.IsTrue(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.EarnAchievement")]
        [TestMethod]
        public void TestEarnAchievementCallbacksCalled()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, true);
            game.StartGame(client1);

            game.EarnAchievement(client1, 5, "achievement");
            FlushActionQueue();

            Assert.AreEqual(0, callback1.GetCallCount("OnAchievementEarned"));
            Assert.AreEqual(1, callback2.GetCallCount("OnAchievementEarned"));
            Assert.AreEqual(1, callback3.GetCallCount("OnAchievementEarned"));
        }

        #endregion

        #region StartGame

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.StartGame")]
        [TestMethod]
        public void TestStartGameFailedIfNotInGame()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());

            bool succeed = game.StartGame(client1);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.StartGame")]
        [TestMethod]
        public void TestStartGameFailedIfNotPlayer()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, true);

            bool succeed = game.StartGame(client1);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.StartGame")]
        [TestMethod]
        public void TestStartGameFailedIfNotGameMaster()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, false);
            IClient client2 = CreateClient("client2", new CountCallTetriNETCallback());
            game.Join(client2, false);

            bool succeed = game.StartGame(client2);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.StartGame")]
        [TestMethod]
        public void TestStartGameFailedIfNotWaitingGameStart()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, true);
            game.Stop();

            bool succeed = game.StartGame(client1);

            Assert.IsFalse(succeed);
            Assert.AreEqual(GameStates.Created, game.State);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.StartGame")]
        [TestMethod]
        public void TestStartGameFailedIfNoPlayers()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, true);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, true);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, true);

            bool succeed = game.StartGame(client1);

            Assert.IsFalse(succeed);
            Assert.AreEqual(GameStates.WaitStartGame, game.State);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.StartGame")]
        [TestMethod]
        public void TestStartGameStatusUpdatedAndCallbackCalled()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, true);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);

            bool succeed = game.StartGame(client1);

            Assert.IsTrue(succeed);
            Assert.AreEqual(GameStates.GameStarted, game.State);
            Assert.AreEqual(1, callback1.GetCallCount("OnGameStarted"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGameStarted"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGameStarted"));
            Assert.AreEqual(ClientStates.Playing, client1.State);
            Assert.AreNotEqual(ClientStates.Playing, client2.State);
            Assert.AreEqual(ClientStates.Playing, client3.State);
        }

        #endregion

        #region StopGame

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.StopGame")]
        [TestMethod]
        public void TestStopGameFailedIfGameNotStarted()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, true);

            bool succeed = game.StopGame(client1);

            Assert.IsFalse(succeed);
            Assert.AreEqual(GameStates.WaitStartGame, game.State);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.StopGame")]
        [TestMethod]
        public void TestStopGameStatusUpdatedAndCallbackCalledIfGameStarted()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, true);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);
            game.StartGame(client1);

            bool succeed = game.StopGame(client1);

            Assert.IsTrue(succeed);
            Assert.AreEqual(GameStates.WaitStartGame, game.State);
            Assert.AreEqual(1, callback1.GetCallCount("OnGameFinished"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGameFinished"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGameFinished"));
            Assert.AreEqual(ClientStates.WaitInGame, client1.State);
            Assert.AreEqual(ClientStates.WaitInGame, client2.State);
            Assert.AreEqual(ClientStates.WaitInGame, client3.State);
        }

        #endregion

        #region PauseGame

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.PauseGame")]
        [TestMethod]
        public void TestPauseGameFailedIfNotInGame()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());

            bool succeed = game.PauseGame(client1);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.PauseGame")]
        [TestMethod]
        public void TestPauseGameFailedIfNotPlayer()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, true);

            bool succeed = game.PauseGame(client1);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.PauseGame")]
        [TestMethod]
        public void TestPauseGameFailedIfNotGameMaster()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, false);
            IClient client2 = CreateClient("client2", new CountCallTetriNETCallback());
            game.Join(client2, false);

            bool succeed = game.PauseGame(client2);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.PauseGame")]
        [TestMethod]
        public void TestPauseGameFailedIfGameNotStarted()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, true);

            bool succeed = game.PauseGame(client1);

            Assert.IsFalse(succeed);
            Assert.AreEqual(GameStates.WaitStartGame, game.State);
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.PauseGame")]
        [TestMethod]
        public void TestPauseGameStatusUpdatedAndCallbacksCalled()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, true);
            game.StartGame(client1);

            bool succeed = game.PauseGame(client1);

            Assert.IsTrue(succeed);
            Assert.AreEqual(GameStates.GamePaused, game.State);
            Assert.AreEqual(1, callback1.GetCallCount("OnGamePaused"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGamePaused"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGamePaused"));
        }

        #endregion

        #region ResumeGame

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ResumeGame")]
        [TestMethod]
        public void TestResumeGameFailedIfNotInGame()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());

            bool succeed = game.ResumeGame(client1);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ResumeGame")]
        [TestMethod]
        public void TestResumeGameFailedIfNotPlayer()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, true);

            bool succeed = game.ResumeGame(client1);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ResumeGame")]
        [TestMethod]
        public void TestResumeGameFailedIfNotGameMaster()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, false);
            IClient client2 = CreateClient("client2", new CountCallTetriNETCallback());
            game.Join(client2, false);

            bool succeed = game.ResumeGame(client2);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ResumeGame")]
        [TestMethod]
        public void TestResumeGameFailedIfGameNotStarted()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, true);

            bool succeed = game.ResumeGame(client1);

            Assert.IsFalse(succeed);
            Assert.AreEqual(GameStates.WaitStartGame, game.State);
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ResumeGame")]
        [TestMethod]
        public void TestResumeGameFailedIfGameNotPaused()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, true);

            bool succeed = game.ResumeGame(client1);

            Assert.IsFalse(succeed);
            Assert.AreEqual(GameStates.WaitStartGame, game.State);
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ResumeGame")]
        [TestMethod]
        public void TestResumeGameStatusUpdatedAndCallbacksCalled()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, true);
            game.StartGame(client1);
            game.PauseGame(client1);

            bool succeed = game.ResumeGame(client1);

            Assert.IsTrue(succeed);
            Assert.AreEqual(GameStates.GameStarted, game.State);
            Assert.AreEqual(1, callback1.GetCallCount("OnGameResumed"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGameResumed"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGameResumed"));
        }

        #endregion

        #region ChangeOptions

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ChangeOptions")]
        [TestMethod]
        public void TestChangeOptionsFailedIfNotWaitingGameStart()
        {
            GameOptions originalOptions = new GameOptions();
            originalOptions.Initialize(GameRules.Custom);
            IGame game = CreateGame("game1", 5, 10, GameRules.Custom, originalOptions);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, false);
            game.StartGame(client1);

            GameOptions newOptions = new GameOptions();
            newOptions.Initialize(GameRules.Custom);
            bool succeed = game.ChangeOptions(client1, newOptions);

            Assert.AreEqual(originalOptions, game.Options);
            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ChangeOptions")]
        [TestMethod]
        public void TestChangeOptionsFailedIfNotInGame()
        {
            GameOptions originalOptions = new GameOptions();
            originalOptions.Initialize(GameRules.Custom);
            IGame game = CreateGame("game1", 5, 10, GameRules.Custom, originalOptions);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());

            GameOptions newOptions = new GameOptions();
            newOptions.Initialize(GameRules.Custom);
            bool succeed = game.ChangeOptions(client1, newOptions);

            Assert.AreEqual(originalOptions, game.Options);
            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ChangeOptions")]
        [TestMethod]
        public void TestChangeOptionsFailedIfNotPlayer()
        {
            GameOptions originalOptions = new GameOptions();
            originalOptions.Initialize(GameRules.Custom);
            IGame game = CreateGame("game1", 5, 10, GameRules.Custom, originalOptions);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, true);

            GameOptions newOptions = new GameOptions();
            newOptions.Initialize(GameRules.Custom);
            bool succeed = game.ChangeOptions(client1, newOptions);

            Assert.AreEqual(originalOptions, game.Options);
            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ChangeOptions")]
        [TestMethod]
        public void TestChangeOptionsFailedIfNotGameMaster()
        {
            GameOptions originalOptions = new GameOptions();
            originalOptions.Initialize(GameRules.Custom);
            IGame game = CreateGame("game1", 5, 10, GameRules.Custom, originalOptions);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, false);
            IClient client2 = CreateClient("client2", new CountCallTetriNETCallback());
            game.Join(client2, false);

            GameOptions newOptions = new GameOptions();
            newOptions.Initialize(GameRules.Custom);
            bool succeed = game.ChangeOptions(client2, newOptions);

            Assert.AreEqual(originalOptions, game.Options);
            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ChangeOptions")]
        [TestMethod]
        public void TestChangeOptionsOkWhenWaitingGameStart()
        {
            GameOptions originalOptions = new GameOptions();
            originalOptions.Initialize(GameRules.Custom);
            IGame game = CreateGame("game1", 5, 10, GameRules.Custom, originalOptions);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, false);

            GameOptions newOptions = new GameOptions();
            newOptions.Initialize(GameRules.Custom);
            game.ChangeOptions(client1, newOptions);

            Assert.AreEqual(newOptions, game.Options);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ChangeOptions")]
        [TestMethod]
        public void TestChangeOptionsClientsInformed()
        {
            GameOptions originalOptions = new GameOptions();
            originalOptions.Initialize(GameRules.Custom);
            IGame game = CreateGame("game1", 5, 10, GameRules.Custom, originalOptions);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, true);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);

            GameOptions newOptions = new GameOptions();
            newOptions.Initialize(GameRules.Custom);
            game.ChangeOptions(client1, newOptions);

            Assert.AreEqual(1, callback1.GetCallCount("OnGameOptionsChanged"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGameOptionsChanged"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGameOptionsChanged"));
        }

        #endregion

        #region ResetWinList

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ResetWinList")]
        [TestMethod]
        public void TestResetWinListFailedIfNotPlayer()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, true);

            bool succeed = game.ResetWinList(client1);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ResetWinList")]
        [TestMethod]
        public void TestResetWinListFailedIfNotInGame()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);

            bool succeed = game.ResetWinList(client1);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ResetWinList")]
        [TestMethod]
        public void TestResetWinListFailedIfNotGameMaster()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);

            bool succeed = game.ResetWinList(client2);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ResetWinList")]
        [TestMethod]
        public void TestResetWinListClientsInformed()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);

            game.ResetWinList(client1);

            Assert.AreEqual(1, callback1.GetCallCount("OnWinListModified"));
            Assert.AreEqual(1, callback2.GetCallCount("OnWinListModified"));
            Assert.AreEqual(1, callback3.GetCallCount("OnWinListModified"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ResetWinList")]
        [TestMethod]
        public void TestResetWinListFailedIfNotWaitingGameStart()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);
            game.StartGame(client1);

            bool succeed = game.ResetWinList(client1);

            Assert.AreEqual(0, callback1.GetCallCount("OnWinListModified"));
            Assert.IsFalse(succeed);
        }

        #endregion

        #region Join while game started

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Join")]
        [TestMethod]
        public void TestJoinPlayerWhileGameStarted()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, true);
            game.StartGame(client1);
            callback1.Reset();
            callback2.Reset();
            callback3.Reset();

            CountCallTetriNETCallback callback4 = new CountCallTetriNETCallback();
            IClient client4 = CreateClient("client4", callback4);
            game.Join(client4, false);

            Assert.AreEqual(ClientStates.WaitInGame, client4.State);
            Assert.AreEqual(ClientRoles.Player, client4.Roles);
            Assert.AreEqual(1, callback1.GetCallCount("OnClientGameJoined"));
            Assert.AreEqual(1, callback2.GetCallCount("OnClientGameJoined"));
            Assert.AreEqual(1, callback3.GetCallCount("OnClientGameJoined"));
            Assert.AreEqual(1, callback4.GetCallCount("OnGameJoined"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Join")]
        [TestMethod]
        public void TestJoinSpectatorWhileGameStarted()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource()); 
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, true);
            game.StartGame(client1);
            callback1.Reset();
            callback2.Reset();
            callback3.Reset();

            CountCallTetriNETCallback callback4 = new CountCallTetriNETCallback();
            IClient client4 = CreateClient("client4", callback4);
            game.Join(client4, true);

            Assert.AreEqual(ClientStates.WaitInGame, client4.State);
            Assert.AreEqual(ClientRoles.Spectator, client4.Roles);
            Assert.AreEqual(1, callback1.GetCallCount("OnClientGameJoined"));
            Assert.AreEqual(1, callback2.GetCallCount("OnClientGameJoined"));
            Assert.AreEqual(1, callback3.GetCallCount("OnClientGameJoined"));
            Assert.AreEqual(1, callback4.GetCallCount("OnGameJoined"));
        }

        #endregion

        #region Leave while game started

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Leave")]
        [TestMethod]
        public void TestLeavePlayingPlayerWhileGameStartedNoPlayingPlayerLeft()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource()); 
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            game.StartGame(client1);
            callback1.Reset();
            callback2.Reset();

            game.Leave(client2);

            Assert.AreEqual(GameStates.WaitStartGame, game.State);
            Assert.AreEqual(ClientStates.WaitInGame, client1.State);
            Assert.AreEqual(1, callback1.GetCallCount("OnGameFinished"));
            Assert.AreEqual(0, callback2.GetCallCount("OnGameFinished"));
            Assert.AreEqual(0, callback1.GetCallCount("OnWinListModified"));
            Assert.AreEqual(0, callback2.GetCallCount("OnWinListModified"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Leave")]
        [TestMethod]
        public void TestLeavePlayingPlayerWhileGameStartedOnePlayingPlayerLeft()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, true);
            game.StartGame(client1);
            callback1.Reset();
            callback2.Reset();
            callback3.Reset();

            game.Leave(client2);

            Assert.AreEqual(GameStates.WaitStartGame, game.State);
            Assert.AreEqual(ClientStates.WaitInGame, client1.State);
            Assert.AreEqual(1, callback1.GetCallCount("OnGameFinished"));
            Assert.AreEqual(0, callback2.GetCallCount("OnGameFinished"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGameFinished"));
            Assert.AreEqual(0, callback1.GetCallCount("OnWinListModified"));
            Assert.AreEqual(0, callback2.GetCallCount("OnWinListModified"));
            Assert.AreEqual(0, callback3.GetCallCount("OnWinListModified"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.Leave")]
        [TestMethod]
        public void TestLeavePlayingPlayerWhileGameStartedMultiplePlayingPlayerLeft()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource()); 
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);
            game.StartGame(client1);
            callback1.Reset();
            callback2.Reset();
            callback3.Reset();

            game.Leave(client2);

            Assert.AreEqual(GameStates.GameStarted, game.State);
            Assert.AreEqual(ClientStates.Playing, client1.State);
            Assert.AreEqual(ClientStates.Playing, client3.State);
            Assert.AreEqual(0, callback1.GetCallCount("OnGameFinished"));
            Assert.AreEqual(0, callback2.GetCallCount("OnGameFinished"));
            Assert.AreEqual(0, callback3.GetCallCount("OnGameFinished"));
            Assert.AreEqual(0, callback1.GetCallCount("OnWinListModified"));
            Assert.AreEqual(0, callback2.GetCallCount("OnWinListModified"));
            Assert.AreEqual(0, callback3.GetCallCount("OnWinListModified"));
        }

        #endregion

        #region Game lost (most complex scenario)

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.GameLost")]
        [TestMethod]
        public void TestGameLostWithOnePlayingPlayer()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.StartGame(client1);
            callback1.Reset();

            game.GameLost(client1);
            FlushActionQueue();

            Assert.AreEqual(GameStates.WaitStartGame, game.State);
            Assert.AreEqual(ClientStates.GameLost, client1.State);
            Assert.AreEqual(0, callback1.GetCallCount("OnPlayerLost"));
            Assert.AreEqual(1, callback1.GetCallCount("OnGameFinished"));
            Assert.AreEqual(0, callback1.GetCallCount("OnWinListModified"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.GameLost")]
        [TestMethod]
        public void TestGameLostWithTwoPlayingPlayers()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, true);
            game.StartGame(client1);
            callback1.Reset();
            callback2.Reset();
            callback3.Reset();

            game.GameLost(client2);
            FlushActionQueue();

            Assert.AreEqual(GameStates.WaitStartGame, game.State);
            Assert.AreEqual(ClientStates.WaitInGame, client1.State);
            Assert.AreEqual(ClientStates.GameLost, client2.State);
            Assert.AreEqual(ClientStates.WaitInGame, client3.State);
            Assert.AreEqual(1, callback1.GetCallCount("OnPlayerLost"));
            Assert.AreEqual(0, callback2.GetCallCount("OnPlayerLost"));
            Assert.AreEqual(1, callback3.GetCallCount("OnPlayerLost"));
            Assert.AreEqual(1, callback1.GetCallCount("OnGameFinished"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGameFinished"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGameFinished"));
            Assert.AreEqual(1, callback1.GetCallCount("OnWinListModified"));
            Assert.AreEqual(1, callback2.GetCallCount("OnWinListModified"));
            Assert.AreEqual(1, callback3.GetCallCount("OnWinListModified"));
            Assert.AreEqual(1, callback1.GetCallCount("OnPlayerWon"));
            Assert.AreEqual(1, callback2.GetCallCount("OnPlayerWon"));
            Assert.AreEqual(1, callback3.GetCallCount("OnPlayerWon"));
            //Assert.AreEqual(client1.Id, callback1.GetCallParameters("OnPlayerWon", 0)[0]);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.GameLost")]
        [TestMethod]
        public void TestGameLostWithMultiplePlayingPlayer()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            CountCallTetriNETCallback callback3 = new CountCallTetriNETCallback();
            IClient client3 = CreateClient("client3", callback3);
            game.Join(client3, false);
            game.StartGame(client1);
            callback1.Reset();
            callback2.Reset();
            callback3.Reset();

            game.GameLost(client2);
            FlushActionQueue();

            Assert.AreEqual(GameStates.GameStarted, game.State);
            Assert.AreEqual(ClientStates.Playing, client1.State);
            Assert.AreEqual(ClientStates.GameLost, client2.State);
            Assert.AreEqual(ClientStates.Playing, client3.State);
            Assert.AreEqual(1, callback1.GetCallCount("OnPlayerLost"));
            Assert.AreEqual(0, callback2.GetCallCount("OnPlayerLost"));
            Assert.AreEqual(1, callback3.GetCallCount("OnPlayerLost"));
        }

        #endregion

        // TODO: 
        //  Reset win list after a few win, win list updated after a win, ...
    }

    [TestClass]
    public class GameUnitTest : AbstractGameUnitTest
    {
        protected PieceProviderMock PieceProvider;
        protected ActionQueueMock ActionQueue;

        protected override IClient CreateClient(string name, ITetriNETClientCallback callback)
        {
            return new Client(name, IPAddress.Any, callback);
        }

        protected override IGame CreateGame(string name, int maxPlayers, int maxSpectators)
        {
            GameOptions options = new GameOptions();
            options.Initialize(GameRules.Standard);
            return CreateGame(name, maxPlayers, maxSpectators, GameRules.Classic, options);
        }

        protected override IGame CreateGame(string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password = null)
        {
            return CreateGame(new ActionQueueMock(), new PieceProviderMock(), name, maxPlayers, maxSpectators, rule, options, password);
        }

        protected IGame CreateGame(IActionQueue actionQueue, IPieceProvider pieceProvider, string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password = null)
        {
            ActionQueue = actionQueue as ActionQueueMock;
            PieceProvider = pieceProvider as PieceProviderMock;

            return new Game(actionQueue, pieceProvider, name, maxPlayers, maxSpectators, rule, options, password);
        }

        protected override void FlushActionQueue()
        {
            while(ActionQueue.ActionCount > 0)
                ActionQueue.DequeueAndExecuteFirstAction();
        }

        #region Constructor

        [TestCategory("Server")]
        [TestCategory("Server.Game")]
        [TestCategory("Server.Game.ctor")]
        [TestMethod]
        public void TestConstructorNullActionQueue()
        {
            try
            {
                IGame fgame = new Game(null, new PieceProviderMock(), "game1", 5, 5, GameRules.Classic, new GameOptions(), "password");

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("actionQueue", ex.ParamName);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.Game")]
        [TestCategory("Server.Game.ctor")]
        [TestMethod]
        public void TestConstructorNullPieceProvider()
        {
            try
            {
                IGame game = new Game(new ActionQueueMock(), null, "game1", 5, 5, GameRules.Classic, new GameOptions(), "password");

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("pieceProvider", ex.ParamName);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.Game")]
        [TestCategory("Server.Game.ctor")]
        [TestMethod]
        public void TestConstructorNullName()
        {
            try
            {
                IGame game = new Game(new ActionQueueMock(), new PieceProviderMock(), null, 5, 5, GameRules.Classic, new GameOptions(), "password");

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("name", ex.ParamName);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.Game")]
        [TestCategory("Server.Game.ctor")]
        [TestMethod]
        public void TestConstructorStrictlyPositiveMaxPlayers()
        {
            try
            {
                IGame game = new Game(new ActionQueueMock(), new PieceProviderMock(), "game1", 0, 5, GameRules.Classic, new GameOptions(), "password");

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.AreEqual("maxPlayers", ex.ParamName);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.Game")]
        [TestCategory("Server.Game.ctor")]
        [TestMethod]
        public void TestConstructorStrictlyPositiveMaxSpectators()
        {
            try
            {
                IGame game = new Game(new ActionQueueMock(), new PieceProviderMock(), "game1", 5, 0, GameRules.Classic, new GameOptions(), "password");

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.AreEqual("maxSpectators", ex.ParamName);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.Game")]
        [TestCategory("Server.Game.ctor")]
        [TestMethod]
        public void TestConstructorNullOptions()
        {
            try
            {
                IGame game = new Game(new ActionQueueMock(), new PieceProviderMock(), "game1", 5, 5, GameRules.Classic, null, "password");

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("options", ex.ParamName);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.Game")]
        [TestCategory("Server.Game.ctor")]
        [TestMethod]
        public void TestConstructorSetProperties()
        {
            const string name = "game1";
            const int maxPlayers = 5;
            const int maxSpectators = 10;
            const GameRules rule = GameRules.Extended;
            const string password = "password";
            GameOptions options = new GameOptions();

            IGame game = new Game(new ActionQueueMock(), new PieceProviderMock(), name, maxPlayers, maxSpectators, rule, options, password);

            Assert.IsNotNull(game);
            Assert.AreEqual(name, game.Name);
            Assert.AreEqual(maxPlayers, game.MaxPlayers);
            Assert.AreEqual(maxSpectators, game.MaxSpectators);
            Assert.AreEqual(rule, game.Rule);
            Assert.AreEqual(options, game.Options);
            Assert.AreEqual(password, game.Password);
            Assert.AreNotEqual(Guid.Empty, game.Id);
            Assert.AreEqual(GameStates.Created, game.State);
        }

        [TestCategory("Server")]
        [TestCategory("Server.Game")]
        [TestCategory("Server.Game.ctor")]
        [TestMethod]
        public void TestConstructorLockObjectNotNull()
        {
            IGame game = new Game(new ActionQueueMock(), new PieceProviderMock(), "game1", 5, 10, GameRules.Custom, new GameOptions());

            Assert.IsNotNull(game.LockObject);
        }

        #endregion

        #region ClearLines

        [TestCategory("Server")]
        [TestCategory("Server.Game")]
        [TestCategory("Server.Game.ClearLines")]
        [TestMethod]
        public void TestClearLinesActionNotEnqueuedIfClassicMultiplayerRulesNotSet()
        {
            IGame game = CreateGame("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.Options.ClassicStyleMultiplayerRules = false;
            game.StartGame(client1);

            bool succeed = game.ClearLines(client1, 4);

            Assert.IsTrue(succeed);
            Assert.AreEqual(0, ActionQueue.ActionCount);
        }

        #endregion

        #region ChangeOptions

        [TestCategory("Server")]
        [TestCategory("Server.IGame")]
        [TestCategory("Server.IGame.ChangeOptions")]
        [TestMethod]
        public void TestChangeOptionsPieceProviderOccuranciesModified()
        {
            GameOptions originalOptions = new GameOptions();
            originalOptions.Initialize(GameRules.Custom);
            IGame game = CreateGame("game1", 5, 10, GameRules.Custom, originalOptions);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, false);

            GameOptions newOptions = new GameOptions();
            newOptions.Initialize(GameRules.Standard);
            game.ChangeOptions(client1, newOptions);

            Assert.IsNotNull(PieceProvider.Occurancies);
            Assert.AreEqual(newOptions.PieceOccurancies, PieceProvider.Occurancies());
        }

        #endregion
    }
}
