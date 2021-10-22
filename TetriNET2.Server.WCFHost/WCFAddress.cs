using System.Linq;
using System.Net;
using TetriNET2.Common.Logger;
using TetriNET2.Server.Interfaces;

namespace TetriNET2.Server.WCFHost
{
    public class WCFAddress : IAddress
    {
        private IPAddress IPAddress { get; }

        public WCFAddress(IPAddress address)
        {
            IPAddress = FixAddress(address);
        }

        public bool Equals(IAddress other)
        {
            if (other == null)
                return false;
            if (other is WCFAddress wcfAddress)
                return IPAddress.Equals(wcfAddress.IPAddress);
            return Serialize().Equals(other.Serialize());
        }

        public string Serialize()
        {
            return IPAddress.ToString();
        }

        public override string ToString()
        {
            return $"IP:{IPAddress}";
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
                return new IPAddress(0x0100007F);
            IPAddress[] addresses = Dns.GetHostAddresses(address.ToString());
            return addresses.FirstOrDefault(i => i.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        }
    }
}
