using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using TetriNET2.Common.Logger;
using TetriNET2.Server.Interfaces;

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
                throw new NotImplementedException();
            }

            public void ReadXml(XmlReader reader)
            {
                Name = reader.GetAttribute("Name");
                Reason = reader.GetAttribute("Reason");
                string address = reader.GetAttribute("Address");
                Address = address == null ? IPAddress.None : IPAddress.Parse(address);
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteAttributeString("Name", Name);
                writer.WriteAttributeString("Reason", Reason);
                writer.WriteAttributeString("Address", Address.ToString());
            }
        }

        private readonly string _banFilename;
        private readonly Dictionary<IPAddress, BanEntry> _banList = new Dictionary<IPAddress, BanEntry>();

        public BanManager(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException("filename");
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

        public IEnumerable<Common.DataContracts.BanEntry> Entries
        {
            get
            {
                return _banList.Values.Select(x => new Common.DataContracts.BanEntry
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
                //using(XmlReader reader = XmlReader.Create(_banFilename))
                //{
                //    while(reader.Read())
                //    {
                //        if (reader.NodeType == XmlNodeType.Element)
                //        {
                //            if (reader.Name == "Entry")
                //            {
                //            }
                //        }
                //        else if (reader.NodeType == XmlNodeType.EndElement)
                //        {
                //            if (reader.Name == "Entry")
                //            {
                //            }
                //        }
                //    }
                //}
                XmlSerializer serializer = new XmlSerializer(typeof(BanEntry));
                using (StreamReader sr = new StreamReader(_banFilename))
                {
                    //TODO: serializer.Deserialize(sr, kv.Value);
                }
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
                //using (XmlWriter writer = XmlWriter.Create(_banFilename))
                //{
                //    writer.WriteStartDocument();
                //    writer.WriteStartElement("Entries");
                //    foreach (KeyValuePair<IPAddress, BanEntry> kv in _banList)
                //    {
                //        writer.WriteStartElement("Entry");
                //        writer.WriteElementString("Name", kv.Value.Name);
                //        writer.WriteElementString("Address", kv.Value.Address.ToString());
                //        writer.WriteElementString("Reason", kv.Value.Reason);
                //        writer.WriteEndElement();
                //    }
                //    writer.WriteEndElement();
                //    writer.WriteEndDocument();
                //}
                XmlSerializer serializer = new XmlSerializer(typeof(BanEntry));
                using (StreamWriter wr = new StreamWriter(_banFilename, false))
                {
                    foreach(KeyValuePair<IPAddress, BanEntry> kv in _banList)
                        serializer.Serialize(wr, kv.Value);
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
