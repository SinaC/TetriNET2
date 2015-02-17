using TetriNET2.Common.ActionQueue;
using TetriNET2.Common.Contracts;

namespace TetriNET2.Client.Interfaces
{
    public interface IFactory
    {
        IActionQueue CreateActionQueue();
        IProxy CreateProxy(ITetriNETClientCallback callback, string address);
        IInventory CreateInventory(int size);
        IPieceBag CreatePieceBag(int size);
    }
}
