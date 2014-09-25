using System;
using System.Collections.Generic;
using System.Net;
using TetriNET2.Common.Contracts;

namespace TetriNET2.Server.Interfaces
{
    public interface IClientManager
    {
        int MaxClients { get; }
        int ClientCount { get; }
        object LockObject { get; }

        IEnumerable<IClient> Clients { get; }

        IClient this[Guid guid] { get; }
        IClient this[string name] { get; }
        IClient this[ITetriNETClientCallback callback] { get; }
        IClient this[IPAddress address] { get; }

        bool Add(IClient client);
        bool Remove(IClient client);
        void Clear();
        bool Contains(string name, ITetriNETClientCallback callback);
    }
}
