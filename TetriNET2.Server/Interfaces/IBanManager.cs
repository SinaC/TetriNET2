using System.Collections.Generic;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Server.Interfaces
{
    public interface IBanManager
    {
        bool IsBanned(IAddress address);
        string BannedReason(IAddress address);
        void Ban(string name, IAddress address, string reason);
        void Unban(IAddress address);
        void Clear();

        IReadOnlyCollection<BanEntryData> Entries { get; }
    }
}
