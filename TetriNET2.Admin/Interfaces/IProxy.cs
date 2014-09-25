using TetriNET2.Common.Contracts;

namespace TetriNET2.Admin.Interfaces
{
    public delegate void ProxyAdminConnectionLostEventHandler();

    public interface IProxy : ITetriNETAdmin
    {
        event ProxyAdminConnectionLostEventHandler ConnectionLost;

        bool Disconnect();
    }
}
