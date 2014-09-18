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
        protected abstract IClient CreateClient(string name, ITetriNETCallback callback);
        protected abstract IGameRoom CreateGameRoom(string name, int maxPlayers, int maxSpectators);
        protected abstract IGameRoom CreateGameRoom(string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password = null);
        protected abstract void FlushActionQueue();

        [TestInitialize]
        public void Initialize()
        {
            Log.Default.Logger = new LogMock();
        }

        #region Start

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Start")]
        [TestMethod]
        public void TestStartChangeState()
        {
            IGameRoom game = CreateGameRoom("game1", 10, 5);

            game.Start(new CancellationTokenSource());

            Assert.AreEqual(GameRoomStates.WaitStartGame, game.State);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Start")]
        [TestMethod]
        public void TestStartOnlyIfCreated()
        {
            IGameRoom game = CreateGameRoom("game1", 10, 5);
            game.Start(new CancellationTokenSource());

            bool succeed = game.Start(new CancellationTokenSource());

            Assert.AreEqual(GameRoomStates.WaitStartGame, game.State);
            Assert.IsFalse(succeed);
        }

        #endregion

        #region Stop

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Stop")]
        [TestMethod]
        public void TestStopOnlyIfStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 10, 5);

            bool succeed = game.Stop();

            Assert.AreEqual(GameRoomStates.Created, game.State);
            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Stop")]
        [TestMethod]
        public void TestStopClientsRemovedAndStatusChanged()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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

            Assert.AreEqual(GameRoomStates.Created, game.State);
            Assert.AreEqual(0, game.ClientCount);
            Assert.IsNull(client1.Game);
            Assert.IsNull(client2.Game);
            Assert.IsNull(client3.Game);
            Assert.AreEqual(ClientStates.Connected, client1.State);
            Assert.AreEqual(ClientStates.Connected, client2.State);
            Assert.AreEqual(ClientStates.Connected, client3.State);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Stop")]
        [TestMethod]
        public void TestStopCallbackCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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

            Assert.AreEqual(game.State, GameRoomStates.Created);
            Assert.AreEqual(1, callback1.GetCallCount("OnGameLeft"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGameLeft"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGameLeft"));
        }

        #endregion

        #region Join

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Join")]
        [TestMethod]
        public void TestJoinNullClient()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);

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
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Join")]
        [TestMethod]
        public void TestJoinGameRoomNoStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            IClient client = CreateClient("client1", new CountCallTetriNETCallback());

            bool succeed = game.Join(client, false);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Join")]
        [TestMethod]
        public void TestJoinPlayerNoMaxPlayers()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Join")]
        [TestMethod]
        public void TestJoinSpectatorNoMaxSpectators()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Join")]
        [TestMethod]
        public void TestJoinPlayerMaxPlayers()
        {
            IGameRoom game = CreateGameRoom("game1", 1, 10);
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
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Join")]
        [TestMethod]
        public void TestJoinSpectatorMaxSpectators()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 1);
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
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Join")]
        [TestMethod]
        public void TestJoinSameClient()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, true);

            bool succeed = game.Join(client1, false);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Join")]
        [TestMethod]
        public void TestJoinPlayerModifyPlayerProperties()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client = CreateClient("client1", new CountCallTetriNETCallback());
            
            game.Join(client, false);

            Assert.AreEqual(ClientStates.WaitInGameRoom, client.State);
            Assert.AreEqual(ClientRoles.Player, client.Roles);
            Assert.AreEqual(game, client.Game);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Join")]
        [TestMethod]
        public void TestJoinSpectatorModifySpectatorProperties()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client = CreateClient("client1", new CountCallTetriNETCallback());

            game.Join(client, true);

            Assert.AreEqual(ClientStates.WaitInGameRoom, client.State);
            Assert.AreEqual(ClientRoles.Spectator, client.Roles);
            Assert.AreEqual(game, client.Game);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Join")]
        [TestMethod]
        public void TestJoinPlayerCallbackCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback = new CountCallTetriNETCallback();
            IClient client = CreateClient("client1", callback);

            game.Join(client, false);

            Assert.AreEqual(1, callback.GetCallCount("OnGameJoined"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Join")]
        [TestMethod]
        public void TestJoinSpectatorCallbackCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback = new CountCallTetriNETCallback();
            IClient client = CreateClient("client1", callback);

            game.Join(client, true);

            Assert.AreEqual(1, callback.GetCallCount("OnGameJoined"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Join")]
        [TestMethod]
        public void TestJoinPlayerOtherClientsInformed()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Join")]
        [TestMethod]
        public void TestJoinSpectatorOtherClientsInformed()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
        }

        #endregion

        #region Leave

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Leave")]
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

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Leave")]
        [TestMethod]
        public void TestLeaveNonExistingClient()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, false);

            IClient client2 = CreateClient("client2", new CountCallTetriNETCallback());
            bool succeed = game.Leave(client2);

            Assert.IsFalse(succeed);
            Assert.AreEqual(1, game.ClientCount);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Leave")]
        [TestMethod]
        public void TestLeaveClientNotInGameRoom()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            
            bool succeed = game.Leave(client1);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Leave")]
        [TestMethod]
        public void TestLeaveExistingClient()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Leave")]
        [TestMethod]
        public void TestLeavePlayerModifyPlayerProperties()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Leave")]
        [TestMethod]
        public void TestLeaveSpectatorModifySpectatorProperties()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Leave")]
        [TestMethod]
        public void TestLeaveClientCallbackCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client = CreateClient("client1", callback1);
            game.Join(client, true);

            game.Leave(client);

            Assert.AreEqual(1, callback1.GetCallCount("OnGameLeft"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Leave")]
        [TestMethod]
        public void TestLeaveClientOtherClientsInformed()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Clear")]
        [TestMethod]
        public void TestClearNoClients()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);

            game.Clear();

            Assert.AreEqual(0, game.ClientCount);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Clear")]
        [TestMethod]
        public void TestClearMultipleClients()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.VoteKick")]
        [TestMethod]
        public void TestVoteKickNullClient()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);

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
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.VoteKick")]
        [TestMethod]
        public void TestVoteKickNullTarget()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.VoteKick")]
        [TestMethod]
        public void TestVoteKickClientNotInGame()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            IClient client2 = CreateClient("client2", new CountCallTetriNETCallback());

            bool succeed = game.VoteKick(client1, client2, "reason");

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.VoteKick")]
        [TestMethod]
        public void TestVoteKickTargetNotInGame()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, false);
            IClient client2 = CreateClient("client2", new CountCallTetriNETCallback());

            bool succeed = game.VoteKick(client1, client2, "reason");

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.VoteKick")]
        [TestMethod]
        public void TestVoteKickClientNotPlayer()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, true);
            IClient client2 = CreateClient("client2", new CountCallTetriNETCallback());
            game.Join(client2, false);

            bool succeed = game.VoteKick(client1, client2, "reason");

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.VoteKick")]
        [TestMethod]
        public void TestVoteKickTargetNotPlayer()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, false);
            IClient client2 = CreateClient("client2", new CountCallTetriNETCallback());
            game.Join(client2, true);

            bool succeed = game.VoteKick(client1, client2, "reason");

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.VoteKick")]
        [TestMethod]
        public void TestVoteKickNotEnoughPlayers()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, false);
            IClient client2 = CreateClient("client2", new CountCallTetriNETCallback());
            game.Join(client2, false);

            bool succeed = game.VoteKick(client1, client2, "reason");

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.VoteKick")]
        [TestMethod]
        public void TestVoteKickNoSimultaneousVoteKick()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.VoteKick")]
        [TestMethod]
        public void TestVoteKickPropertiesModified()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.VoteKick")]
        [TestMethod]
        public void TestVoteKickCallbackCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.VoteKick")]
        [TestMethod]
        public void TestVoteKickTimeout()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.VoteKickAnswer")]
        [TestMethod]
        public void TestVoteKickAnswerNullClient()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);

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
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.VoteKickAnswer")]
        [TestMethod]
        public void TestVoteKickAnswerClientNotPlayer()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, true);

            bool succeed = game.VoteKickAnswer(client1, false);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.VoteKickAnswer")]
        [TestMethod]
        public void TestVoteKickAnswerNoVoteKickStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            IClient client1 = CreateClient("client1", new CountCallTetriNETCallback());
            game.Join(client1, false);

            bool succeed = game.VoteKickAnswer(client1, false);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.VoteKickAnswer")]
        [TestMethod]
        public void TestVoteKickAnswerVoteTargetCannotVote()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.VoteKickAnswer")]
        [TestMethod]
        public void TestVoteKickAnswerNoMultipleAnswerForSamePlayer()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.VoteKickAnswer")]
        [TestMethod]
        public void TestVoteKickAnswerNotLastToAnswer()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.VoteKickAnswer")]
        [TestMethod]
        public void TestVoteKickAnswerLastToAnswerNotKicked()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.VoteKickAnswer")]
        [TestMethod]
        public void TestVoteKickAnswerLastToAnswerKicked()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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

        #region ChangeOptions

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.ChangeOptions")]
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

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.ChangeOptions")]
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

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.ChangeOptions")]
        [TestMethod]
        public void TestChangeOptionsClientsInformed()
        {
            GameOptions originalOptions = new GameOptions();
            IGameRoom game = CreateGameRoom("game1", 5, 10, GameRules.Custom, originalOptions);
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

            GameOptions newOptions = new GameOptions();
            game.ChangeOptions(newOptions);

            Assert.AreEqual(1, callback1.GetCallCount("OnGameOptionsChanged"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGameOptionsChanged"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGameOptionsChanged"));
        }

        #endregion

        #region ResetWinList

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.ResetWinList")]
        [TestMethod]
        public void TestResetWinListClientsInformed()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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

            game.ResetWinList();

            Assert.AreEqual(1, callback1.GetCallCount("OnWinListModified"));
            Assert.AreEqual(1, callback2.GetCallCount("OnWinListModified"));
            Assert.AreEqual(1, callback3.GetCallCount("OnWinListModified"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.ResetWinList")]
        [TestMethod]
        public void TestResetWinListFailedIfNotWaitingGameStart()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
            game.StartGame();

            bool succeed = game.ResetWinList();

            Assert.AreEqual(0, callback1.GetCallCount("OnWinListModified"));
            Assert.IsFalse(succeed);
        }
            
        #endregion

        #region PlacePiece

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.PlacePiece")]
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

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.PlacePiece")]
        [TestMethod]
        public void TestPlacePieceFailedIfClientNotInGame()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.StartGame();

            bool succeed = game.PlacePiece(client2, 0, 0, Pieces.TetriminoO, 0, 0, 0, null);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.PlacePiece")]
        [TestMethod]
        public void TestPlacePieceFailedIfGameNotStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, true);

            bool succeed = game.PlacePiece(client1, 0, 0, Pieces.TetriminoO, 0, 0, 0, null);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.PlacePiece")]
        [TestMethod]
        public void TestPlacePieceFailedIfClientNotPlaying()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.StartGame();
            client1.State = ClientStates.GameLost; // simulate game loss

            bool succeed = game.PlacePiece(client1, 0, 0, Pieces.TetriminoO, 0, 0, 0, null);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.PlacePiece")]
        [TestMethod]
        public void TestPlacePieceOkIfGameStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.StartGame();

            bool succeed = game.PlacePiece(client1, 0, 0, Pieces.TetriminoO, 0, 0, 0, null);

            Assert.IsTrue(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.PlacePiece")]
        [TestMethod]
        public void TestPlacePieceActionCallbacksCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
            game.StartGame();

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
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.ModifyGrid")]
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

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.ModifyGrid")]
        [TestMethod]
        public void TestModifyGridFailedIfClientNotInGame()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.StartGame();

            bool succeed = game.ModifyGrid(client2, null);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.ModifyGrid")]
        [TestMethod]
        public void TestModifyGridFailedIfGameNotStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, true);

            bool succeed = game.ModifyGrid(client1, null);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.ModifyGrid")]
        [TestMethod]
        public void TestModifyGridFailedIfClientNotPlaying()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.StartGame();
            client1.State = ClientStates.GameLost; // simulate game loss

            bool succeed = game.ModifyGrid(client1, null);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.ModifyGrid")]
        [TestMethod]
        public void TestModifyGridOkIfGameStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.StartGame();

            bool succeed = game.ModifyGrid(client1, null);

            Assert.IsTrue(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.ModifyGrid")]
        [TestMethod]
        public void TestModifyGridActionCallbacksCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
            game.StartGame();

            game.ModifyGrid(client1, null);
            FlushActionQueue();

            Assert.AreEqual(0, callback1.GetCallCount("OnGridModified"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGridModified"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGridModified"));
        }

        #endregion

        #region UseSpecial

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.UseSpecial")]
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

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.UseSpecial")]
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

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.UseSpecial")]
        [TestMethod]
        public void TestUseSpecialFailedIfClientNotInGame()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.StartGame();

            bool succeed = game.UseSpecial(client2, client1, Specials.BlockBomb);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.UseSpecial")]
        [TestMethod]
        public void TestUseSpecialFailedIfTargetNotInGame()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.StartGame();

            bool succeed = game.UseSpecial(client1, client2, Specials.BlockBomb);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.UseSpecial")]
        [TestMethod]
        public void TestUseSpecialFailedIfGameNotStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.UseSpecial")]
        [TestMethod]
        public void TestUseSpecialFailedIfClientNotPlaying()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            game.StartGame();
            client1.State = ClientStates.GameLost; // simulate game loss

            bool succeed = game.UseSpecial(client1, client2, Specials.AddLines);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.UseSpecial")]
        [TestMethod]
        public void TestUseSpecialFailedIfTargetNotPlaying()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            game.StartGame();
            client2.State = ClientStates.GameLost; // simulate game loss

            bool succeed = game.UseSpecial(client1, client2, Specials.AddLines);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.UseSpecial")]
        [TestMethod]
        public void TestUseSpecialOkIfGameStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            game.StartGame();

            bool succeed = game.UseSpecial(client1, client2, Specials.AddLines);

            Assert.IsTrue(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.UseSpecial")]
        [TestMethod]
        public void TestUseSpecialActionCallbacksCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
            game.StartGame();

            game.UseSpecial(client1, client2, Specials.AddLines);
            FlushActionQueue();

            Assert.AreEqual(1, callback1.GetCallCount("OnSpecialUsed"));
            Assert.AreEqual(1, callback2.GetCallCount("OnSpecialUsed"));
            Assert.AreEqual(1, callback3.GetCallCount("OnSpecialUsed"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.UseSpecial")]
        [TestMethod]
        public void TestUseSpecialSwitchFieldsActionCallbacksCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
            game.StartGame();

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
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.ClearLines")]
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

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.ClearLines")]
        [TestMethod]
        public void TestClearLinesFailedIfClientNotInGame()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.StartGame();

            bool succeed = game.ClearLines(client2, 4);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.ClearLines")]
        [TestMethod]
        public void TestClearLinesFailedIfGameNotStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, true);

            bool succeed = game.ClearLines(client1, 4);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.ClearLines")]
        [TestMethod]
        public void TestClearLinesFailedIfClientNotPlaying()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.StartGame();
            client1.State = ClientStates.GameLost; // simulate game loss

            bool succeed = game.ClearLines(client1, 4);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.ClearLines")]
        [TestMethod]
        public void TestClearLinesOkIfGameStartedAndClassicMultiplayerRules()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.Options.ClassicStyleMultiplayerRules = true;
            game.StartGame();

            bool succeed = game.ClearLines(client1, 4);

            Assert.IsTrue(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.ClearLines")]
        [TestMethod]
        public void TestClearLinesActionCallbacksCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
            game.StartGame();

            game.ModifyGrid(client1, null);
            FlushActionQueue();

            Assert.AreEqual(0, callback1.GetCallCount("OnGridModified"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGridModified"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGridModified"));
        }

        #endregion

        #region GameLost

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.GameLost")]
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

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.GameLost")]
        [TestMethod]
        public void TestGameLostFailedIfClientNotInGame()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);

            bool succeed = game.GameLost(client1);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.GameLost")]
        [TestMethod]
        public void TestGameLostFailedIfClientNotPlaying()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.StartGame();
            client1.State = ClientStates.GameLost; // simulate game loss

            bool succeed = game.ClearLines(client1, 4);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.GameLost")]
        [TestMethod]
        public void TestGameLostOkIfGameStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.StartGame();

            bool succeed = game.GameLost(client1);

            Assert.IsTrue(succeed);
        }

        #endregion

        #region FinishContinuousSpecial

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.FinishContinuousSpecial")]
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

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.FinishContinuousSpecial")]
        [TestMethod]
        public void TestFinishContinuousSpecialFailedIfClientNotInGame()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.StartGame();

            bool succeed = game.FinishContinuousSpecial(client2, Specials.Darkness);

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.FinishContinuousSpecial")]
        [TestMethod]
        public void TestFinishContinuousSpecialOkIfGameStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.StartGame();

            bool succeed = game.FinishContinuousSpecial(client1, Specials.Darkness);

            Assert.IsTrue(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.FinishContinuousSpecial")]
        [TestMethod]
        public void TestFinishContinuousSpecialActionCallbacksCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
            game.StartGame();

            game.FinishContinuousSpecial(client1, Specials.Darkness);
            FlushActionQueue();

            Assert.AreEqual(0, callback1.GetCallCount("OnContinuousSpecialFinished"));
            Assert.AreEqual(1, callback2.GetCallCount("OnContinuousSpecialFinished"));
            Assert.AreEqual(1, callback3.GetCallCount("OnContinuousSpecialFinished"));
        }

        #endregion

        #region EarnAchievement

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.EarnAchievement")]
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

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.EarnAchievement")]
        [TestMethod]
        public void TestEarnAchievementSpecialFailedIfClientNotInGame()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.StartGame();

            bool succeed = game.EarnAchievement(client2, 5, "achievement");

            Assert.IsFalse(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.EarnAchievement")]
        [TestMethod]
        public void TestEarnAchievement()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
            game.StartGame();

            bool succeed = game.EarnAchievement(client1, 5, "achievement");

            Assert.IsTrue(succeed);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.EarnAchievement")]
        [TestMethod]
        public void TestEarnAchievementCallbacksCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
            game.StartGame();

            game.EarnAchievement(client1, 5, "achievement");
            FlushActionQueue();

            Assert.AreEqual(0, callback1.GetCallCount("OnAchievementEarned"));
            Assert.AreEqual(1, callback2.GetCallCount("OnAchievementEarned"));
            Assert.AreEqual(1, callback3.GetCallCount("OnAchievementEarned"));
        }

        #endregion

        #region StartGame

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.StartGame")]
        [TestMethod]
        public void TestStartGameFailedIfNotWaitingGameStart()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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

            bool succeed = game.StartGame();

            Assert.IsFalse(succeed);
            Assert.AreEqual(GameRoomStates.Created, game.State);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.StartGame")]
        [TestMethod]
        public void TestStartGameFailedIfNoPlayers()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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

            bool succeed = game.StartGame();

            Assert.IsFalse(succeed);
            Assert.AreEqual(GameRoomStates.WaitStartGame, game.State);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.StartGame")]
        [TestMethod]
        public void TestStartGameStatusUpdatedAndCallbackCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.StopGame")]
        [TestMethod]
        public void TestStopGameFailedIfGameNotStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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

            bool succeed = game.StopGame();

            Assert.IsFalse(succeed);
            Assert.AreEqual(GameRoomStates.WaitStartGame, game.State);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.StopGame")]
        [TestMethod]
        public void TestStopGameStatusUpdatedAndCallbackCalledIfGameStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
            game.StartGame();

            bool succeed = game.StopGame();

            Assert.IsTrue(succeed);
            Assert.AreEqual(GameRoomStates.WaitStartGame, game.State);
            Assert.AreEqual(1, callback1.GetCallCount("OnGameFinished"));
            Assert.AreEqual(1, callback2.GetCallCount("OnGameFinished"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGameFinished"));
            Assert.AreEqual(ClientStates.WaitInGameRoom, client1.State);
            Assert.AreEqual(ClientStates.WaitInGameRoom, client2.State);
            Assert.AreEqual(ClientStates.WaitInGameRoom, client3.State);
        }

        #endregion

        #region PauseGame

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.PauseGame")]
        [TestMethod]
        public void TestPauseGameFailedIfGameNotStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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

            bool succeed = game.PauseGame();

            Assert.IsFalse(succeed);
            Assert.AreEqual(GameRoomStates.WaitStartGame, game.State);
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.PauseGame")]
        [TestMethod]
        public void TestPauseGameStatusUpdatedAndCallbacksCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.ResumeGame")]
        [TestMethod]
        public void TestResumeGameFailedIfGameNotStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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

            bool succeed = game.ResumeGame();

            Assert.IsFalse(succeed);
            Assert.AreEqual(GameRoomStates.WaitStartGame, game.State);
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.ResumeGame")]
        [TestMethod]
        public void TestResumeGameFailedIfGameNotPaused()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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

            bool succeed = game.ResumeGame();

            Assert.IsFalse(succeed);
            Assert.AreEqual(GameRoomStates.WaitStartGame, game.State);
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
            Assert.AreEqual(0, callback1.GetCallCount("OnGamePaused"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.ResumeGame")]
        [TestMethod]
        public void TestResumeGameStatusUpdatedAndCallbacksCalled()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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

        #region Join while game started

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Join")]
        [TestMethod]
        public void TestJoinPlayerWhileGameStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
            game.StartGame();
            callback1.Reset();
            callback2.Reset();
            callback3.Reset();

            CountCallTetriNETCallback callback4 = new CountCallTetriNETCallback();
            IClient client4 = CreateClient("client4", callback4);
            game.Join(client4, false);

            Assert.AreEqual(ClientStates.WaitInGameRoom, client4.State);
            Assert.AreEqual(ClientRoles.Player, client4.Roles);
            Assert.AreEqual(1, callback1.GetCallCount("OnClientGameJoined"));
            Assert.AreEqual(1, callback2.GetCallCount("OnClientGameJoined"));
            Assert.AreEqual(1, callback3.GetCallCount("OnClientGameJoined"));
            Assert.AreEqual(1, callback4.GetCallCount("OnGameJoined"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Join")]
        [TestMethod]
        public void TestJoinSpectatorWhileGameStarted()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
            game.StartGame();
            callback1.Reset();
            callback2.Reset();
            callback3.Reset();

            CountCallTetriNETCallback callback4 = new CountCallTetriNETCallback();
            IClient client4 = CreateClient("client4", callback4);
            game.Join(client4, true);

            Assert.AreEqual(ClientStates.WaitInGameRoom, client4.State);
            Assert.AreEqual(ClientRoles.Spectator, client4.Roles);
            Assert.AreEqual(1, callback1.GetCallCount("OnClientGameJoined"));
            Assert.AreEqual(1, callback2.GetCallCount("OnClientGameJoined"));
            Assert.AreEqual(1, callback3.GetCallCount("OnClientGameJoined"));
            Assert.AreEqual(1, callback4.GetCallCount("OnGameJoined"));
        }

        #endregion

        #region Leave while game started

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Leave")]
        [TestMethod]
        public void TestLeavePlayingPlayerWhileGameStartedNoPlayingPlayerLeft()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource()); 
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            IClient client2 = CreateClient("client2", callback2);
            game.Join(client2, false);
            game.StartGame();
            callback1.Reset();
            callback2.Reset();

            game.Leave(client2);

            Assert.AreEqual(GameRoomStates.WaitStartGame, game.State);
            Assert.AreEqual(ClientStates.WaitInGameRoom, client1.State);
            Assert.AreEqual(1, callback1.GetCallCount("OnGameFinished"));
            Assert.AreEqual(0, callback2.GetCallCount("OnGameFinished"));
            Assert.AreEqual(0, callback1.GetCallCount("OnWinListModified"));
            Assert.AreEqual(0, callback2.GetCallCount("OnWinListModified"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Leave")]
        [TestMethod]
        public void TestLeavePlayingPlayerWhileGameStartedOnePlayingPlayerLeft()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
            game.StartGame();
            callback1.Reset();
            callback2.Reset();
            callback3.Reset();

            game.Leave(client2);

            Assert.AreEqual(GameRoomStates.WaitStartGame, game.State);
            Assert.AreEqual(ClientStates.WaitInGameRoom, client1.State);
            Assert.AreEqual(1, callback1.GetCallCount("OnGameFinished"));
            Assert.AreEqual(0, callback2.GetCallCount("OnGameFinished"));
            Assert.AreEqual(1, callback3.GetCallCount("OnGameFinished"));
            Assert.AreEqual(0, callback1.GetCallCount("OnWinListModified"));
            Assert.AreEqual(0, callback2.GetCallCount("OnWinListModified"));
            Assert.AreEqual(0, callback3.GetCallCount("OnWinListModified"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.Leave")]
        [TestMethod]
        public void TestLeavePlayingPlayerWhileGameStartedMultiplePlayingPlayerLeft()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
            game.StartGame();
            callback1.Reset();
            callback2.Reset();
            callback3.Reset();

            game.Leave(client2);

            Assert.AreEqual(GameRoomStates.GameStarted, game.State);
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
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.GameLost")]
        [TestMethod]
        public void TestGameLostWithOnePlayingPlayer()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.StartGame();
            callback1.Reset();

            game.GameLost(client1);
            FlushActionQueue();

            Assert.AreEqual(GameRoomStates.WaitStartGame, game.State);
            Assert.AreEqual(ClientStates.GameLost, client1.State);
            Assert.AreEqual(0, callback1.GetCallCount("OnPlayerLost"));
            Assert.AreEqual(1, callback1.GetCallCount("OnGameFinished"));
            Assert.AreEqual(0, callback1.GetCallCount("OnWinListModified"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.GameLost")]
        [TestMethod]
        public void TestGameLostWithTwoPlayingPlayers()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
            game.StartGame();
            callback1.Reset();
            callback2.Reset();
            callback3.Reset();

            game.GameLost(client2);
            FlushActionQueue();

            Assert.AreEqual(GameRoomStates.WaitStartGame, game.State);
            Assert.AreEqual(ClientStates.WaitInGameRoom, client1.State);
            Assert.AreEqual(ClientStates.GameLost, client2.State);
            Assert.AreEqual(ClientStates.WaitInGameRoom, client3.State);
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
        [TestCategory("Server.IGameRoom")]
        [TestCategory("Server.IGameRoom.GameLost")]
        [TestMethod]
        public void TestGameLostWithMultiplePlayingPlayer()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
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
            game.StartGame();
            callback1.Reset();
            callback2.Reset();
            callback3.Reset();

            game.GameLost(client2);
            FlushActionQueue();

            Assert.AreEqual(GameRoomStates.GameStarted, game.State);
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
    public class GameRoomUnitTest : AbstractGameRoomUnitTest
    {
        protected PieceProviderMock PieceProvider;
        protected ActionQueueMock ActionQueue;

        protected override IClient CreateClient(string name, ITetriNETCallback callback)
        {
            return new Client(name, IPAddress.Any, callback);
        }

        protected override IGameRoom CreateGameRoom(string name, int maxPlayers, int maxSpectators)
        {
            GameOptions options = new GameOptions();
            options.Initialize(GameRules.Standard);
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

        protected override void FlushActionQueue()
        {
            while(ActionQueue.ActionCount > 0)
                ActionQueue.DequeueAndExecuteFirstAction();
        }

        #region Constructor

        [TestCategory("Server")]
        [TestCategory("Server.GameRoom")]
        [TestCategory("Server.GameRoom.ctor")]
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

        [TestCategory("Server")]
        [TestCategory("Server.GameRoom")]
        [TestCategory("Server.GameRoom.ctor")]
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

        [TestCategory("Server")]
        [TestCategory("Server.GameRoom")]
        [TestCategory("Server.GameRoom.ctor")]
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

        [TestCategory("Server")]
        [TestCategory("Server.GameRoom")]
        [TestCategory("Server.GameRoom.ctor")]
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

        [TestCategory("Server")]
        [TestCategory("Server.GameRoom")]
        [TestCategory("Server.GameRoom.ctor")]
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

        [TestCategory("Server")]
        [TestCategory("Server.GameRoom")]
        [TestCategory("Server.GameRoom.ctor")]
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

        [TestCategory("Server")]
        [TestCategory("Server.GameRoom")]
        [TestCategory("Server.GameRoom.ctor")]
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

        [TestCategory("Server")]
        [TestCategory("Server.GameRoom")]
        [TestCategory("Server.GameRoom.ctor")]
        [TestMethod]
        public void TestConstructorLockObjectNotNull()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10, GameRules.Custom, new GameOptions());

            Assert.IsNotNull(game.LockObject);
        }

        #endregion

        #region ClearLines

        [TestCategory("Server")]
        [TestCategory("Server.GameRoom")]
        [TestCategory("Server.GameRoom.ClearLines")]
        [TestMethod]
        public void TestClearLinesActionNotEnqueuedIfClassicMultiplayerRulesNotSet()
        {
            IGameRoom game = CreateGameRoom("game1", 5, 10);
            game.Start(new CancellationTokenSource());
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            IClient client1 = CreateClient("client1", callback1);
            game.Join(client1, false);
            game.Options.ClassicStyleMultiplayerRules = false;
            game.StartGame();

            bool succeed = game.ClearLines(client1, 4);

            Assert.IsTrue(succeed);
            Assert.AreEqual(0, ActionQueue.ActionCount);
        }

        #endregion
    }
}
