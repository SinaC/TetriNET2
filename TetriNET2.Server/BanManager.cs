using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using TetriNET2.Common.Logger;
using TetriNET2.Server.Interfaces;

//http://stackoverflow.com/questions/279534/proper-way-to-implement-ixmlserializable
//http://msdn.microsoft.com/en-us/library/system.xml.serialization.ixmlserializable.readxml.aspx
namespace TetriNET2.Server
{
    public sealed class BanManager : IBanManager
    {
        public sealed class BanEntry: IXmlSerializable
        {
            public string Name { get; set; }
            public string Address { get; set; }
            public string Reason { get; set; }

            public BanEntry() // needed for serialization
            {
            }

            public BanEntry(string name, string address, string reason)
            {
                Name = name;
                Address = address;
                Reason = reason;
            }

            public XmlSchema GetSchema()
            {
                return null;
            }

            public void ReadXml(XmlReader reader)
            {
                Name = reader.GetAttribute("Name");
                Reason = reader.GetAttribute("Reason");
                Address = reader.GetAttribute("Address");
                reader.Read();
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteAttributeString("Name", Name);
                writer.WriteAttributeString("Reason", Reason);
                writer.WriteAttributeString("Address", Address);
            }
        }

        private readonly string _banFilename;
        private Dictionary<string, BanEntry> _banList;

        public BanManager(ISettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            string filename = settings.BanFilename;
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));
            _banList = new Dictionary<string, BanEntry>();
            lock (_banList)
            {
                _banFilename = filename;
                Load();
            }
        }

        #region IBanManager

        public bool IsBanned(IAddress address)
        {
            lock (_banList)
                return _banList.ContainsKey(address.Serialize());
        }

        public string BannedReason(IAddress address)
        {
            lock (_banList)
            {
                if (_banList.TryGetValue(address.Serialize(), out var entry))
                    return entry.Reason;
            }
            return null;
        }

        public void Ban(string name, IAddress address, string reason)
        {
            lock (_banList)
            {
                var serialized = address.Serialize();
                if (_banList.ContainsKey(serialized))
                    return;
                BanEntry banEntry = new BanEntry(name, serialized, reason);
                _banList.Add(serialized, banEntry);

                Save();
            }
        }

        public void Unban(IAddress address)
        {
            lock (_banList)
            {
                var serialized = address.Serialize();
                if (!_banList.ContainsKey(serialized))
                    return;
                _banList.Remove(serialized);

                Save();
            }
        }

        public void Clear()
        {
            lock (_banList)
            {
                _banList.Clear();

                Save();
            }
        }

        public IReadOnlyCollection<Common.DataContracts.BanEntryData> Entries
        {
            get
            {
                lock (_banList)
                    return _banList.Values.Select(x => new Common.DataContracts.BanEntryData
                    {
                        Name = x.Name,
                        Address = x.Address.ToString(),
                        Reason = x.Reason
                    }).ToList();
            }
        }

        #endregion

        private void Load()
        {
            try
            {
                if (!File.Exists(_banFilename))
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Banned file {0} not found", _banFilename);
                    return;
                }

                BanEntry[] entries;
                XmlSerializer serializer = new XmlSerializer(typeof(BanEntry[]));
                using (StreamReader sr = new StreamReader(_banFilename))
                {
                    entries = (BanEntry[])serializer.Deserialize(sr);
                }
                lock(_banList)
                    _banList = entries.ToDictionary(x => x.Address, x => x);
            }
            catch(Exception ex)
            {
                Log.Default.WriteLine(LogLevels.Error, "Error while reading banned file {0}. Exception: {1}", _banFilename, ex);
            }
        }

        private void Save()
        {
            try
            {
                BanEntry[] entries;
                lock(_banList)
                    entries = _banList.Select(x => x.Value).ToArray();
                XmlSerializer serializer = new XmlSerializer(entries.GetType());
                using (StreamWriter wr = new StreamWriter(_banFilename, false))
                {
                    serializer.Serialize(wr, entries);
                }
            }
            catch (Exception ex)
            {
                Log.Default.WriteLine(LogLevels.Error, "Error while writing banned file {0}. Exception: {1}", _banFilename, ex);
            }
        }
    }
}
