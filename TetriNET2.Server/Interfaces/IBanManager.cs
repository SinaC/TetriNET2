using System.Net;

namespace TetriNET2.Server.Interfaces
{
    public interface IBanManager
    {
        bool IsBanned(IPAddress address);
        string BannedReason(IPAddress address);
        void Ban(string name, IPAddress address, string reason);
        void Unban(IPAddress address);
    }
}
