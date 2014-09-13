using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
            public string Name { get; private set; }
            public IPAddress Address { get; private set; }
            public string Reason { get; private set; }

            public BanEntry()
            {
            }

            public BanEntry(string name, IPAddress address, string reason)
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
                string address = reader.GetAttribute("Address");
                Address = address == null ? IPAddress.None : IPAddress.Parse(address);
                reader.Read();
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteAttributeString("Name", Name);
                writer.WriteAttributeString("Reason", Reason);
                writer.WriteAttributeString("Address", Address.ToString());
            }
        }

        private readonly string _banFilename;
        private Dictionary<IPAddress, BanEntry> _banList;

        public BanManager(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException("filename");
            _banList = new Dictionary<IPAddress, BanEntry>();
            lock (_banList)
            {
                _banFilename = filename;
                Load();
            }
        }

        #region IBanManager

        public bool IsBanned(IPAddress address)
        {
            address = FixAddress(address);

            return _banList.ContainsKey(address);
        }

        public string BannedReason(IPAddress address)
        {
            address = FixAddress(address);

            BanEntry entry;
            if (_banList.TryGetValue(address, out entry))
                return entry.Reason;
            return null;
        }

        public void Ban(string name, IPAddress address, string reason)
        {
            address = FixAddress(address);

            lock (_banList)
            {
                if (_banList.ContainsKey(address))
                    return;
                BanEntry banEntry = new BanEntry(name, address, reason);
                _banList.Add(address, banEntry);

                Save();
            }
        }

        public void Unban(IPAddress address)
        {
            address = FixAddress(address);

            lock (_banList)
            {
                if (!_banList.ContainsKey(address))
                    return;
                _banList.Remove(address);

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

        public IEnumerable<Common.DataContracts.BanEntryData> Entries
        {
            get
            {
                return _banList.Values.Select(x => new Common.DataContracts.BanEntryData
                    {
                        Name = x.Name,
                        Address = x.Address.ToString(),
                        Reason = x.Reason
                    });
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

        private static IPAddress FixAddress(IPAddress address)
        {
            if (address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            {
                IPAddress addressIPV4 = GetIPv4Address(address);
                if (addressIPV4 != null)
                    address = addressIPV4;
                else
                    Log.Default.WriteLine(LogLevels.Error, "IPv4 supported only. {0} Address family: {1}", address, address.AddressFamily);
            }
            return address;
        }

        private static IPAddress GetIPv4Address(IPAddress address)
        {
            if (IPAddress.IPv6Loopback.Equals(address))
            {
                return new IPAddress(0x0100007F);
            }
            IPAddress[] addresses = Dns.GetHostAddresses(address.ToString());
            return addresses.FirstOrDefault(i => i.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        }
    }
}
