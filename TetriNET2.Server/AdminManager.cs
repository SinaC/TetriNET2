using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TetriNET2.Common.Contracts;
using TetriNET2.Common.Logger;
using TetriNET2.Server.Interfaces;

namespace TetriNET2.Server
{
    public sealed class AdminManager : IAdminManager
    {
        private readonly Dictionary<ITetriNETAdminCallback, IAdmin> _admins = new Dictionary<ITetriNETAdminCallback, IAdmin>();
        private readonly object _lockObject = new object();

        public AdminManager(int maxAdmins)
        {
            if (maxAdmins <= 0)
                throw new ArgumentOutOfRangeException("maxAdmins", "maxAdmins must be strictly positive");
            MaxAdmins = maxAdmins;
        }

        #region IAdminManager

        public int MaxAdmins { get; private set; }

        public int AdminCount
        {
            get { return _admins.Count; }
        }

        public object LockObject
        {
            get { return _lockObject; }
        }

        public IReadOnlyCollection<IAdmin> Admins
        {
            get { return _admins.Values.ToList(); }
        }

        public IAdmin this[Guid guid]
        {
            get
            {
                KeyValuePair<ITetriNETAdminCallback, IAdmin> kv = _admins.FirstOrDefault(x => x.Value.Id == guid);
                if (kv.Equals(default(KeyValuePair<ITetriNETAdminCallback, IAdmin>)))
                    return null;
                return kv.Value;
            }
        }

        public IAdmin this[string name]
        {
            get
            {
                KeyValuePair<ITetriNETAdminCallback, IAdmin> kv = _admins.FirstOrDefault(x => x.Value.Name == name);
                if (kv.Equals(default(KeyValuePair<ITetriNETAdminCallback, IAdmin>)))
                    return null;
                return kv.Value;
            }
        }

        public IAdmin this[ITetriNETAdminCallback callback]
        {
            get
            {
                IAdmin admin;
                _admins.TryGetValue(callback, out admin);
                return admin;
            }
        }

        public IAdmin this[IPAddress address]
        {
            get
            {
                KeyValuePair<ITetriNETAdminCallback, IAdmin> kv = _admins.FirstOrDefault(x => x.Value.Address.Equals(address));
                if (kv.Equals(default(KeyValuePair<ITetriNETAdminCallback, IAdmin>)))
                    return null;
                return kv.Value;
            }
        }

        public bool Add(IAdmin admin)
        {
            if (admin == null)
                throw new ArgumentNullException("admin");

            if (AdminCount >= MaxAdmins)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Too many admins");
                return false;
            }

            if (_admins.ContainsValue(admin))
            {
                Log.Default.WriteLine(LogLevels.Warning, "{0} already connected", admin.Name);
                return false;
            }

            if (_admins.ContainsKey(admin.Callback))
            {
                Log.Default.WriteLine(LogLevels.Warning, "{0} already connected", admin.Name);
                return false;
            }

            if (_admins.Any(x => x.Value.Name == admin.Name))
            {
                Log.Default.WriteLine(LogLevels.Warning, "{0} already connected", admin.Name);
                return false;
            }

            //
            _admins.Add(admin.Callback, admin);

            //
            return true;
        }

        public bool Remove(IAdmin admin)
        {
            if (admin == null)
                throw new ArgumentNullException("admin");

            bool removed = _admins.Remove(admin.Callback);
            return removed;
        }

        public void Clear()
        {
            _admins.Clear();
        }

        public bool Contains(string name, ITetriNETAdminCallback callback)
        {
            bool found = _admins.Any(x => x.Value.Name == name || x.Key == callback);
            return found;
        }

        #endregion
    }
}
