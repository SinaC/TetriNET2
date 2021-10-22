using System;

namespace TetriNET2.Server.Interfaces
{
    public interface IAddress : IEquatable<IAddress>
    {
        string Serialize();
    }
}
