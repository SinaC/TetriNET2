using TetriNET2.Common.Contracts;

namespace TetriNET2.Client.Interfaces
{
    public interface IFactory
    {
        IProxy CreateProxy(ITetriNETClientCallback callback, string address);
    }
}
