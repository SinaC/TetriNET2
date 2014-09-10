using System.Collections.Generic;

namespace TetriNET2.Server.Interfaces
{
    public interface IWaitRoom
    {
        int MaxClients { get; }
        int ClientCount { get; }
        object LockObject { get; }

        IEnumerable<IClient> Clients { get; }

        bool Join(IClient client);
        bool Leave(IClient client);
        void Clear();
    }
}
