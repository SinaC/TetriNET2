using System;
using System.Net;
using TetriNET2.Common.Contracts;

namespace TetriNET2.Server.Interfaces
{
    public delegate void AdminConnectionLostEventHandler(IAdmin entity);

    public interface IAdmin : ITetriNETAdminCallback
    {
        event AdminConnectionLostEventHandler ConnectionLost;

        //
        Guid Id { get; }
        string Name { get; }
        IPAddress Address { get; }
        ITetriNETAdminCallback Callback { get; }
        DateTime ConnectTime { get; }
    }
}
