using System.Collections.Generic;
using System.Net;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Server.Interfaces
{
    public interface IBanManager
    {
        bool IsBanned(IPAddress address);
        string BannedReason(IPAddress address);
        void Ban(string name, IPAddress address, string reason);
        void Unban(IPAddress address);
        void Clear();

        IEnumerable<BanEntryData> Entries { get; }
    }
}
