using System.Collections.Generic;
using System.Linq;
using TetriNET2.Client.Interfaces;
using TetriNET2.Common.DataContracts;

namespace TetriNET2.Client
{
    public sealed class Inventory : IInventory
    {
        private readonly object _lock;
        private readonly List<Specials> _queue;
        private int _size;

        public Inventory(int size)
        {
            _lock = new object();
            _size = size;
            _queue = new List<Specials>();
        }

        #region IInventory

        public void Reset(int size)
        {
            lock (_lock)
            {
                _size = size;
                _queue.Clear();
            }
        }

        public bool Enqueue(Specials special)
        {
            bool result = false;
            lock (_lock)
            {
                if (_queue.Count < _size)
                {
                    _queue.Add(special);
                    result = true;
                }
            }
            return result;
        }

        public void Enqueue(List<Specials> specials)
        {
            lock (_lock)
            {
                foreach (Specials special in specials)
                {
                    bool enqueued = Enqueue(special);
                    if (!enqueued)
                        break;
                }
            }
        }

        public bool Dequeue(out Specials special)
        {
            special = 0;
            bool result = false;
            lock (_lock)
            {
                if (_queue.Count > 0)
                {
                    special = _queue[0];
                    _queue.RemoveAt(0);
                    result = true;
                }
            }
            return result;
        }

        public IReadOnlyCollection<Specials> Specials()
        {
            //http://stackoverflow.com/questions/491375/readonlycollection-or-ienumerable-for-exposing-member-collections
            IReadOnlyCollection<Specials> specials;
            lock (_lock)
            {
                specials = _queue.Skip(0).ToList();
            }
            return specials;
        }

        #endregion
    }
}
