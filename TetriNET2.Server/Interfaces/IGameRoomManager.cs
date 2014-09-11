using System;
using System.Collections.Generic;

namespace TetriNET2.Server.Interfaces
{
    public interface IGameRoomManager
    {
        int MaxRooms { get; }
        int RoomCount { get; }
        object LockObject { get; }

        IEnumerable<IGameRoom> Rooms { get; }

        IGameRoom this[Guid guid] { get; }
        IGameRoom this[string name] { get; }

        bool Add(IGameRoom client);
        bool Remove(IGameRoom client);
        void Clear();
    }
}
