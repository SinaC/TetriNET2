using System.Collections.Generic;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Client.Interfaces
{
    public interface IInventory
    {
        void Reset(int size);
        bool Enqueue(Specials special);
        void Enqueue(List<Specials> specials);
        bool Dequeue(out Specials special);
        IEnumerable<Specials> Specials();
    }
}
