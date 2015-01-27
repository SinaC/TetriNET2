using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Logger;
using TetriNET2.Server.Interfaces;
using TetriNET2.Server.Tests.Mocking;

namespace TetriNET2.Server.Tests
{
    [TestClass]
    public abstract class AbstractGameManagerUnitTest
    {
        protected abstract IGameManager CreateGameManager(int maxGames);
        protected abstract IGame CreateGame(string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password);

        [TestInitialize]
        public void Initialize()
        {
            Log.Default.Logger = new LogMock();
        }

        #region Add

        [TestCategory("Server")]
        [TestCategory("Server.IGameManager")]
        [TestCategory("Server.IGameManager.Add")]
        [TestMethod]
        public void TestAddNullAdmin()
        {
            IGameManager gameManager = CreateGameManager(10);

            try
            {
                gameManager.Add(null);

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("game", ex.ParamName);
            }

            Assert.AreEqual(0, gameManager.GameCount);
            Assert.AreEqual(0, gameManager.Games.Count());
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameManager")]
        [TestCategory("Server.IGameManager.Add")]
        [TestMethod]
        public void TestAddNoMaxGames()
        {
            IGameManager gameManager = CreateGameManager(10);

            bool inserted1 = gameManager.Add(CreateGame("game1", 5, 5, GameRules.Custom, new GameOptions(), null));
            bool inserted2 = gameManager.Add(CreateGame("game2", 5, 5, GameRules.Custom, new GameOptions(), null));

            Assert.IsTrue(inserted1);
            Assert.IsTrue(inserted2);
            Assert.AreEqual(2, gameManager.GameCount);
            Assert.AreEqual(2, gameManager.Games.Count());
            Assert.IsTrue(gameManager.Games.Any(x => x.Name == "game1") && gameManager.Games.Any(x => x.Name == "game2"));
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameManager")]
        [TestCategory("Server.IGameManager.Add")]
        [TestMethod]
        public void TestAddWithMaxGames()
        {
            IGameManager gameManager = CreateGameManager(1);
            gameManager.Add(CreateGame("game1", 5, 5, GameRules.Custom, new GameOptions(), null));

            bool inserted = gameManager.Add(CreateGame("game2", 5, 5, GameRules.Custom, new GameOptions(), null));

            Assert.IsFalse(inserted);
            Assert.AreEqual(1, gameManager.GameCount);
            Assert.AreEqual("game1", gameManager.Games.First().Name);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameManager")]
        [TestCategory("Server.IGameManager.Add")]
        [TestMethod]
        public void TestAddSameGame()
        {
            IGame game1 = CreateGame("game1", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGameManager gameManager = CreateGameManager(10);
            gameManager.Add(game1);

            bool inserted = gameManager.Add(game1);

            Assert.IsFalse(inserted);
            Assert.AreEqual(1, gameManager.GameCount);
            Assert.AreEqual(1, gameManager.Games.Count());
        }

        #endregion

        #region Remove

        [TestCategory("Server")]
        [TestCategory("Server.IGameManager")]
        [TestCategory("Server.IGameManager.Remove")]
        [TestMethod]
        public void TestRemoveExistingGame()
        {
            IGameManager gameManager = CreateGameManager(10);
            IGame game = CreateGame("game1", 5, 5, GameRules.Custom, new GameOptions(), null);
            gameManager.Add(game);

            bool removed = gameManager.Remove(game);

            Assert.IsTrue(removed);
            Assert.AreEqual(0, gameManager.GameCount);
            Assert.AreEqual(0, gameManager.Games.Count());
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameManager")]
        [TestCategory("Server.IGameManager.Remove")]
        [TestMethod]
        public void TestRemoveNonExistingGame()
        {
            IGameManager gameManager = CreateGameManager(10);
            IGame game1 = CreateGame("game1", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGame game2 = CreateGame("game2", 5, 5, GameRules.Custom, new GameOptions(), null);
            gameManager.Add(game1);

            bool removed = gameManager.Remove(game2);

            Assert.IsFalse(removed);
            Assert.AreEqual(1, gameManager.GameCount);
            Assert.AreEqual(1, gameManager.Games.Count());
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameManager")]
        [TestCategory("Server.IGameManager.Remove")]
        [TestMethod]
        public void TestRemoveNullGame()
        {
            IGameManager gameManager = CreateGameManager(10);
            gameManager.Add(CreateGame("game1", 5, 5, GameRules.Custom, new GameOptions(), null));

            try
            {
                gameManager.Remove(null);
                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("game", ex.ParamName);
            }

            Assert.AreEqual(1, gameManager.GameCount);
            Assert.AreEqual(1, gameManager.Games.Count());
        }

        #endregion

        #region Clear

        [TestCategory("Server")]
        [TestCategory("Server.IGameManager")]
        [TestCategory("Server.IGameManager.Clear")]
        [TestMethod]
        public void TestClearNoGames()
        {
            IGameManager gameManager = CreateGameManager(10);

            gameManager.Clear();

            Assert.AreEqual(0, gameManager.GameCount);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameManager")]
        [TestCategory("Server.IGameManager.Clear")]
        [TestMethod]
        public void TestClearSomeGames()
        {
            IGameManager gameManager = CreateGameManager(10);
            gameManager.Add(CreateGame("game1", 5, 5, GameRules.Custom, new GameOptions(), null));
            gameManager.Add(CreateGame("game2", 5, 5, GameRules.Custom, new GameOptions(), null));
            gameManager.Add(CreateGame("game3", 5, 5, GameRules.Custom, new GameOptions(), null));

            gameManager.Clear();

            Assert.AreEqual(0, gameManager.GameCount);
        }

        #endregion

        #region Indexers

        [TestCategory("Server")]
        [TestCategory("Server.IGameManager")]
        [TestCategory("Server.IGameManager.Indexers")]
        [TestMethod]
        public void TestIndexerGuidFindExistingGame()
        {
            IGame game1 = CreateGame("game1", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGame game2 = CreateGame("game2", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGame game3 = CreateGame("game3", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGameManager gameManager = CreateGameManager(10);
            gameManager.Add(game1);
            gameManager.Add(game2);
            gameManager.Add(game3);

            IGame searched = gameManager[game2.Id];

            Assert.IsNotNull(searched);
            Assert.AreEqual(game2, searched);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameManager")]
        [TestCategory("Server.IGameManager.Indexers")]
        [TestMethod]
        public void TestIndexerGuidFindNonExistingAdmin()
        {
            IGame game1 = CreateGame("game1", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGame game2 = CreateGame("game2", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGame game3 = CreateGame("game3", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGameManager gameManager = CreateGameManager(10);
            gameManager.Add(game1);
            gameManager.Add(game2);
            gameManager.Add(game3);

            IGame searched = gameManager[Guid.Empty];

            Assert.IsNull(searched);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameManager")]
        [TestCategory("Server.IGameManager.Indexers")]
        [TestMethod]
        public void TestIndexerNameFindExistingGame()
        {
            IGame game1 = CreateGame("game1", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGame game2 = CreateGame("game2", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGame game3 = CreateGame("game3", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGameManager gameManager = CreateGameManager(10);
            gameManager.Add(game1);
            gameManager.Add(game2);
            gameManager.Add(game3);

            IGame searched = gameManager[game2.Name];

            Assert.IsNotNull(searched);
            Assert.AreEqual(game2, searched);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IGameManager")]
        [TestCategory("Server.IGameManager.Indexers")]
        [TestMethod]
        public void TestIndexerNameFindNonExistingAdmin()
        {
            IGame game1 = CreateGame("game1", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGame game2 = CreateGame("game2", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGame game3 = CreateGame("game3", 5, 5, GameRules.Custom, new GameOptions(), null);
            IGameManager gameManager = CreateGameManager(10);
            gameManager.Add(game1);
            gameManager.Add(game2);
            gameManager.Add(game3);

            IGame searched = gameManager["admin4"];

            Assert.IsNull(searched);
        }

        #endregion
    }

    [TestClass]
    public class GameManagerUnitTest : AbstractGameManagerUnitTest
    {
        protected override IGameManager CreateGameManager(int maxGames)
        {
            return new GameManager(maxGames);
        }

        protected override IGame CreateGame(string name, int maxPlayers, int maxSpectators, GameRules rule, GameOptions options, string password)
        {
            return new Game(new ActionQueueMock(), new PieceProviderMock(), name, maxPlayers, maxSpectators, rule, options, password);
        }

        #region Constructor

        [TestCategory("Server")]
        [TestCategory("Server.GameManager")]
        [TestCategory("Server.GameManager.ctor")]
        [TestMethod]
        public void TestConstructorStrictlyPositiveMaxGames()
        {
            try
            {
                IGameManager gameManager = CreateGameManager(0);
                Assert.Fail("ArgumentOutOfRange exception not raised");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.AreEqual("maxGames", ex.ParamName);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.GameManager")]
        [TestCategory("Server.GameManager.ctor")]
        [TestMethod]
        public void TestConstructorSetProperties()
        {
            const int maxGames = 10;

            IGameManager gameManager = CreateGameManager(maxGames);

            Assert.AreEqual(maxGames, gameManager.MaxGames);
        }

        [TestCategory("Server")]
        [TestCategory("Server.GameManager")]
        [TestCategory("Server.GameManager.ctor")]
        [TestMethod]
        public void TestConstructorLockObjectNotNull()
        {
            IGameManager gameManager = CreateGameManager(10);

            Assert.IsNotNull(gameManager.LockObject);
        }

        #endregion
    }
}
