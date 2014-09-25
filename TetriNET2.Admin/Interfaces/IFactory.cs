using TetriNET2.Common.Contracts;

namespace TetriNET2.Admin.Interfaces
{
    public interface IFactory
    {
        IProxy CreateProxy(ITetriNETAdminCallback callback, string address);
    }
}
