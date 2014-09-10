using System;
using System.Collections.Generic;
using System.Linq;
using TetriNET2.Common.DataContracts;
using TetriNET2.Common.Logger;
using TetriNET2.Server.Interfaces;

namespace TetriNET2.Server
{
    public class WaitRoom : IWaitRoom
    {
        private readonly Dictionary<Guid, IClient> _clients = new Dictionary<Guid, IClient>();
        private readonly object _lockObject = new object();

        public WaitRoom(int maxClients)
        {
            if (maxClients <= 0)
                throw new ArgumentOutOfRangeException("maxClients", "maxClients must be strictly positive");
            MaxClients = maxClients;
        }

        #region IWaitRoom
        
        public int MaxClients { get; private set; }

        public int ClientCount
        {
            get { return _clients.Count; }
        }

        public object LockObject
        {
            get { return _lockObject; }
        }

        public IEnumerable<IClient> Clients
        {
            get { return _clients.Select(x => x.Value).ToList(); }
        }

        public bool Join(IClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            if (ClientCount >= MaxClients)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Too many clients");
                return false;
            }

            if (_clients.ContainsKey(client.Id))
            {
                Log.Default.WriteLine(LogLevels.Warning, "{0} already added", client.Name);
                return false;
            }

            // Change state
            client.State = ClientStates.InWaitRoom;

            //
            _clients.Add(client.Id, client);

            //
            return true;
        }

        public bool Leave(IClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            bool removed = _clients.Remove(client.Id);
            return removed;
        }

        public void Clear()
        {
            _clients.Clear();
        }

        #endregion
    }
}
