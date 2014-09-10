using System;
using System.Collections.Generic;
using System.Linq;
using TetriNET2.Common.Logger;
using TetriNET2.Server.Interfaces;

namespace TetriNET2.Server
{
    public class GameRoomManager : IGameRoomManager
    {
        private readonly Dictionary<Guid, IGameRoom> _rooms = new Dictionary<Guid, IGameRoom>();
        private readonly object _lockObject = new object();

        public GameRoomManager(int maxRooms)
        {
            Id = Guid.NewGuid();
            MaxRooms = maxRooms;
        }

        public Guid Id { get; private set; }

        public int MaxRooms { get; private set; }

        public int RoomCount
        {
            get { return _rooms.Count; }
        }

        public object LockObject
        {
            get { return _lockObject; }
        }

        public IEnumerable<IGameRoom> Rooms
        {
            get { return _rooms.Select(x => x.Value); }
        }

        public IGameRoom this[Guid guid]
        {
            get
            {
                IGameRoom room;
                _rooms.TryGetValue(guid, out room);
                return room;
            }
        }

        public IGameRoom this[string name]
        {
            get
            {
                KeyValuePair<Guid, IGameRoom> kv = _rooms.FirstOrDefault(x => x.Value.Name == name);
                if (kv.Equals(default(KeyValuePair<Guid, IGameRoom>)))
                    return null;
                return kv.Value;
            }
        }

        public bool Add(IGameRoom room)
        {
            if (room == null)
                throw new ArgumentNullException("room");

            if (RoomCount >= MaxRooms)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Too many rooms");
                return false;
            }

            if (_rooms.ContainsKey(room.Id))
            {
                Log.Default.WriteLine(LogLevels.Warning, "{0} already added", room.Name);
                return false;
            }

            //
            _rooms.Add(room.Id, room);

            //
            return true;
        }

        public bool Remove(IGameRoom room)
        {
            if (room == null)
                throw new ArgumentNullException("room");

            bool removed = _rooms.Remove(room.Id);
            return removed;
        }

        public void Clear()
        {
            foreach(KeyValuePair<Guid, IGameRoom> room in _rooms)
                room.Value.Clear();
            _rooms.Clear();
        }
    }
}
