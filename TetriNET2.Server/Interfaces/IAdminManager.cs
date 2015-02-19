using System;
using System.Collections.Generic;
using System.Net;
using TetriNET2.Common.Contracts;

namespace TetriNET2.Server.Interfaces
{
    public interface IAdminManager
    {
        int MaxAdmins { get; }
        int AdminCount { get; }
        object LockObject { get; }

        IReadOnlyCollection<IAdmin> Admins { get; }

        IAdmin this[Guid guid] { get; }
        IAdmin this[string name] { get; }
        IAdmin this[ITetriNETAdminCallback callback] { get; }
        IAdmin this[IPAddress address] { get; }

        bool Add(IAdmin admin);
        bool Remove(IAdmin admin);
        void Clear();
        bool Contains(string name, ITetriNETAdminCallback callback);
    }
}
