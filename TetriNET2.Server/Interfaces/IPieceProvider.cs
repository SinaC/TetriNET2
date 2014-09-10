using System;
using System.Collections.Generic;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Server.Interfaces
{
    public interface IPieceProvider
    {
        void Reset();
        Func<IEnumerable<PieceOccurancy>> Occurancies { get; set; }
        Pieces this[int index] { get; }
    }
}
