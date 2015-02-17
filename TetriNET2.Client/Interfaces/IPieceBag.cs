using TetriNET2.Common.DataContracts;

namespace TetriNET2.Client.Interfaces
{
    public interface IPieceBag
    {
        void Reset();
        Pieces this[int index] { get; set; }
        string Dump(int size);
    }
}
