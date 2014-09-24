using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.Logger;
using TetriNET2.Server.Interfaces;

namespace TetriNET2.Server
{
    public sealed class ClientManager : IClientManager
    {
        private readonly Dictionary<ITetriNETCallback, IClient> _clients = new Dictionary<ITetriNETCallback, IClient>();
        private readonly object _lockObject = new object();

        public ClientManager(int maxClients)
        {
            if (maxClients <= 0)
                throw new ArgumentOutOfRangeException("maxClients", "maxClients must be strictly positive");
            MaxClients = maxClients;
        }

        #region IClientManager

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
            get { return _clients.Select(x => x.Value); }
        }

        public IClient this[Guid guid]
        {
            get
            {
                KeyValuePair<ITetriNETCallback, IClient> kv = _clients.FirstOrDefault(x => x.Value.Id == guid);
                if (kv.Equals(default(KeyValuePair<ITetriNETCallback, IClient>)))
                    return null;
                return kv.Value;
            }
        }

        public IClient this[string name]
        {
            get
            {
                KeyValuePair<ITetriNETCallback, IClient> kv = _clients.FirstOrDefault(x => x.Value.Name == name);
                if (kv.Equals(default(KeyValuePair<ITetriNETCallback, IClient>)))
                    return null;
                return kv.Value;
            }
        }

        public IClient this[ITetriNETCallback callback]
        {
            get
            {
                IClient client;
                _clients.TryGetValue(callback, out client);
                return client;
            }
        }

        public IClient this[IPAddress address]
        {
            get
            {
                KeyValuePair<ITetriNETCallback, IClient> kv = _clients.FirstOrDefault(x => x.Value.Address.Equals(address));
                if (kv.Equals(default(KeyValuePair<ITetriNETCallback, IClient>)))
                    return null;
                return kv.Value;
            }
        }

        public bool Add(IClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            if (ClientCount >= MaxClients)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Too many clients");
                return false;
            }

            if (_clients.ContainsKey(client.Callback))
            {
                Log.Default.WriteLine(LogLevels.Warning, "{0} already connected", client.Name);
                return false;
            }

            if (_clients.Any(x => x.Value.Name == client.Name))
            {
                Log.Default.WriteLine(LogLevels.Warning, "{0} already connected", client.Name);
                return false;
            }

            //
            _clients.Add(client.Callback, client);

            //
            return true;
        }

        public bool Remove(IClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            bool removed = _clients.Remove(client.Callback);
            return removed;
        }

        public void Clear()
        {
            _clients.Clear();
        }

        public bool Contains(string name, ITetriNETCallback callback)
        {
            bool found = _clients.Any(x => x.Value.Name == name || x.Key == callback);
            return found;
        }

        #endregion
    }
}
