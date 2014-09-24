using System.Collections.Generic;
using System.Linq;
using TetriNET2.Server.Interfaces;

namespace TetriNET2.Server
{
    public sealed class PasswordManager : IPasswordManager
    {
        private sealed class PasswordEntry
        {
            public DomainTypes DomainType { get; set; }
            public string Name { get; set; }
            public string CryptedPassword { get; set; }
        }

        private readonly List<PasswordEntry> _entries;

        public PasswordManager()
        {
            _entries = new List<PasswordEntry>();
        }

        #region IPasswordManager

        public bool CheckSucceedIfNotFound { get; set; }

        public bool Add(DomainTypes domainType, string name, string cryptedPassword)
        {
            if (_entries.Any(x => x.DomainType == domainType && x.Name == name))
                return false;
            _entries.Add(new PasswordEntry
                {
                    DomainType = domainType,
                    Name = name,
                    CryptedPassword = cryptedPassword
                });
            return true;
        }

        public bool Remove(DomainTypes domainType, string name)
        {
            PasswordEntry entry = _entries.FirstOrDefault(x => x.DomainType == domainType && x.Name == name);
            if (entry == null)
                return false;
            _entries.Remove(entry);
            return true;
        }

        public bool Check(DomainTypes domainType, string name, string cryptedPassword)
        {
            PasswordEntry entry = _entries.FirstOrDefault(x => x.DomainType == domainType && x.Name == name);
            if (entry == null)
                return CheckSucceedIfNotFound;
            return entry.CryptedPassword == cryptedPassword;
        }

        #endregion
    }
}
