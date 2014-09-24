using System;
using System.Collections.Generic;
using TetriNET2.Common.DataContracts;
using TetriNET2.Server.Interfaces;

namespace TetriNET2.Server.Tests.Mocking
{
    public class PieceProviderMock : IPieceProvider
    {
        private Func<IEnumerable<PieceOccurancy>> _occurancies = () => new [] { 
            new PieceOccurancy
            {
                Value = Pieces.TetriminoO,
                Occurancy = 20
            },  
            new PieceOccurancy
            {
                Value = Pieces.TetriminoL,
                Occurancy = 20
            },
            new PieceOccurancy
            {
                Value = Pieces.TetriminoI,
                Occurancy = 20
            },
            new PieceOccurancy
            {
                Value = Pieces.TetriminoJ,
                Occurancy = 20
            },
            new PieceOccurancy
            {
                Value = Pieces.TetriminoS,
                Occurancy = 20
            },
        };
        
        public void Reset()
        {
        }

        public Func<IEnumerable<PieceOccurancy>> Occurancies
        {
            get { return _occurancies; }
            set { _occurancies = value; }
        }

        public Pieces this[int index]
        {
            get {return Pieces.TetriminoO;}
        }
    }

    //    // Always get first available
    //private static Pieces PseudoRandom(IEnumerable<IOccurancy<Pieces>> occurancies, IEnumerable<Pieces> history)
    //{
    //    var available = (occurancies as IList<IOccurancy<Pieces>> ?? occurancies.ToList()).Where(x => !history.Contains(x.Value)).ToList();
    //    if (available.Any())
    //    {
    //        Pieces piece = available[0].Value;
    //        return piece;
    //    }
    //    return Pieces.Invalid;
    //}
}
