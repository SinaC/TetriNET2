using System;
using System.Collections.Generic;

namespace TetriNET2.Server.Interfaces
{
    public interface IGameManager
    {
        int MaxGames { get; }
        int GameCount { get; }
        object LockObject { get; }

        IReadOnlyCollection<IGame> Games { get; }

        IGame this[Guid guid] { get; }
        IGame this[string name] { get; }

        bool Add(IGame game);
        bool Remove(IGame game);
        void Clear();
    }
}
