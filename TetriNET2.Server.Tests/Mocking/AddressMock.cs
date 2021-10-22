using TetriNET2.Server.Interfaces;

namespace TetriNET2.Server.Tests.Mocking
{
    internal class AddressMock : IAddress
    {
        public static readonly AddressMock Any = new AddressMock("255.255.255.255");
        public static readonly AddressMock None = new AddressMock("0.0.0.0");

        private string Address { get; }

        public AddressMock(string address)
        {
            Address = address;
        }

        public bool Equals(IAddress other)
        {
            return other?.Serialize().Equals(Address) ?? false;
        }

        public string Serialize()
        {
            return Address;
        }
    }
}
