using System;
using System.Collections.Generic;
using System.Linq;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.Logger;
using TetriNET2.Server.Interfaces;

namespace TetriNET2.Server
{
    public sealed class ClientManager : IClientManager
    {
        private readonly Dictionary<ITetriNETClientCallback, IClient> _clients = new Dictionary<ITetriNETClientCallback, IClient>();

        public ClientManager(ISettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            int maxClients = settings.MaxClients;
            if (maxClients <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxClients), "maxClients must be strictly positive");
            MaxClients = maxClients;
        }

        #region IClientManager

        public int MaxClients { get; }

        public int ClientCount => _clients.Count;

        public object LockObject { get; } = new object();

        public IReadOnlyCollection<IClient> Clients => _clients.Values.ToList();

        public IClient this[Guid guid]
        {
            get
            {
                KeyValuePair<ITetriNETClientCallback, IClient> kv = _clients.FirstOrDefault(x => x.Value.Id == guid);
                if (kv.Equals(default(KeyValuePair<ITetriNETClientCallback, IClient>)))
                    return null;
                return kv.Value;
            }
        }

        public IClient this[string name]
        {
            get
            {
                KeyValuePair<ITetriNETClientCallback, IClient> kv = _clients.FirstOrDefault(x => x.Value.Name == name);
                if (kv.Equals(default(KeyValuePair<ITetriNETClientCallback, IClient>)))
                    return null;
                return kv.Value;
            }
        }

        public IClient this[ITetriNETClientCallback callback]
        {
            get
            {
                _clients.TryGetValue(callback, out var client);
                return client;
            }
        }

        public IClient this[IAddress address]
        {
            get
            {
                KeyValuePair<ITetriNETClientCallback, IClient> kv = _clients.FirstOrDefault(x => x.Value.Address.Equals(address));
                if (kv.Equals(default(KeyValuePair<ITetriNETClientCallback, IClient>)))
                    return null;
                return kv.Value;
            }
        }

        public bool Add(IClient client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

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
                throw new ArgumentNullException(nameof(client));

            bool removed = _clients.Remove(client.Callback);
            return removed;
        }

        public void Clear()
        {
            _clients.Clear();
        }

        public bool Contains(string name, ITetriNETClientCallback callback)
        {
            bool found = _clients.Any(x => x.Value.Name == name || x.Key == callback);
            return found;
        }

        #endregion
    }
}
