using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Logger;
using TetriNET2.Common.Occurancy;
using TetriNET2.Server.Interfaces;
using TetriNET2.Server.Tests.Mocking;

namespace TetriNET2.Server.Tests
{
    [TestClass]
    public abstract class AbstractPieceProviderUnitTest
    {
        protected abstract IPieceProvider CreatePieceProvider();
        
        protected virtual void Reset(IPieceProvider provider)
        {
            provider.Reset();
        }

        [TestInitialize]
        public void Initialize()
        {
            Log.Default.Logger = new LogMock();
        }

        [TestCategory("Server")]
        [TestCategory("Server.IPieceProvider")]
        [TestCategory("Server.IPieceProvider")]
        [TestMethod]
        public void TestExceptionIfOccuranciesIsNull()
        {
            IPieceProvider pieceProvider = CreatePieceProvider();

            try
            {
                Pieces piece = pieceProvider[0];

                Assert.Fail("No Exception raised");
            }
            catch(Exception ex)
            {
                Assert.IsTrue(true);
            }
        }

        [TestCategory("Server")]
        [TestCategory("Server.IPieceProvider")]
        [TestCategory("Server.IPieceProvider")]
        [TestMethod]
        public void TestGetFirstPieceIsValid()
        {
            IPieceProvider pieceProvider = CreatePieceProvider();
            pieceProvider.Occurancies = () => new[] {new PieceOccurancy
                {
                    Occurancy = 100,
                    Value = Pieces.TetriminoI
                }};

            Pieces piece = pieceProvider[0];

            Assert.AreEqual(Pieces.TetriminoI, piece);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IPieceProvider")]
        [TestCategory("Server.IPieceProvider")]
        [TestMethod]
        public void TestGetMultiplePiecesAreValid()
        {
            IPieceProvider pieceProvider = CreatePieceProvider();
            pieceProvider.Occurancies = () => new[] {
                new PieceOccurancy
                {
                    Occurancy = 25,
                    Value = Pieces.TetriminoI
                },
            new PieceOccurancy
                {
                    Occurancy = 25,
                    Value = Pieces.TetriminoJ
                },
            new PieceOccurancy
                {
                    Occurancy = 25,
                    Value = Pieces.TetriminoL
                },
            new PieceOccurancy
                {
                    Occurancy = 25,
                    Value = Pieces.TetriminoO
                }};

            Pieces piece1 = pieceProvider[0];
            Pieces piece2 = pieceProvider[1];
            Pieces piece3 = pieceProvider[2];
            Pieces piece4 = pieceProvider[3];

            Assert.AreEqual(Pieces.TetriminoI, piece1);
            Assert.AreEqual(Pieces.TetriminoJ, piece2);
            Assert.AreEqual(Pieces.TetriminoL, piece3);
            Assert.AreEqual(Pieces.TetriminoO, piece4);
        }

        [TestCategory("Server")]
        [TestCategory("Server.IPieceProvider")]
        [TestCategory("Server.IPieceProvider")]
        [TestMethod]
        public void TestReset()
        {
            IPieceProvider pieceProvider = CreatePieceProvider();
            pieceProvider.Occurancies = () => new[] {
                new PieceOccurancy
                {
                    Occurancy = 25,
                    Value = Pieces.TetriminoI
                },
            new PieceOccurancy
                {
                    Occurancy = 25,
                    Value = Pieces.TetriminoJ
                },
            new PieceOccurancy
                {
                    Occurancy = 25,
                    Value = Pieces.TetriminoL
                },
            new PieceOccurancy
                {
                    Occurancy = 25,
                    Value = Pieces.TetriminoO
                }};
            Pieces p1 = pieceProvider[0];
            Pieces p2 = pieceProvider[1];
            Pieces p3 = pieceProvider[2];
            Pieces p4 = pieceProvider[3];

            Reset(pieceProvider);
            Pieces piece1 = pieceProvider[0];
            Pieces piece2 = pieceProvider[1];
            Pieces piece3 = pieceProvider[2];
            Pieces piece4 = pieceProvider[3];

            Assert.AreEqual(Pieces.TetriminoI, piece1);
            Assert.AreEqual(Pieces.TetriminoJ, piece2);
            Assert.AreEqual(Pieces.TetriminoL, piece3);
            Assert.AreEqual(Pieces.TetriminoO, piece4);
        }
    }

    [TestClass]
    public class PieceBagUnitTest : AbstractPieceProviderUnitTest
    {
        private const int HistorySize = 4;

        // Always get first available
        protected Pieces PseudoRandom(IEnumerable<IOccurancy<Pieces>> occurancies, IEnumerable<Pieces> history)
        {
            var available = (occurancies as IList<IOccurancy<Pieces>> ?? occurancies.ToList()).Where(x => !history.Contains(x.Value)).ToList();
            if (available.Any())
            {
                Pieces piece = available[0].Value;
                return piece;
            }
            return Pieces.Invalid;
        }

        protected override IPieceProvider CreatePieceProvider()
        {
            return new PieceBag(PseudoRandom, HistorySize);
        }

        [TestCategory("Server")]
        [TestCategory("Server.PieceBag")]
        [TestCategory("Server.PieceBag.History")]
        [TestMethod]
        public void TestHistory()
        {
            IPieceProvider pieceProvider = CreatePieceProvider();
            pieceProvider.Occurancies = () => new[] {
                new PieceOccurancy
                {
                    Occurancy = 25,
                    Value = Pieces.TetriminoI
                },
            new PieceOccurancy
                {
                    Occurancy = 25,
                    Value = Pieces.TetriminoJ
                },
            new PieceOccurancy
                {
                    Occurancy = 25,
                    Value = Pieces.TetriminoL
                },
            new PieceOccurancy
                {
                    Occurancy = 25,
                    Value = Pieces.TetriminoO
                }};

            Pieces piece1 = pieceProvider[0];
            Pieces piece2 = pieceProvider[1];
            Pieces piece3 = pieceProvider[2];
            Pieces piece4 = pieceProvider[3];
            Pieces piece5 = pieceProvider[4];

            Assert.AreEqual(Pieces.Invalid, piece5);
        }
    }
}
