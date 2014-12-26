using System;
using TetriNET2.Common.Contracts;

namespace TetriNET2.Client.Interfaces
{
    public delegate void ProxyClientConnectionLostEventHandler();

    public interface IProxy : ITetriNETClient
    {
        DateTime LastActionToServer { get; } // used to check if heartbeat is needed

        event ProxyClientConnectionLostEventHandler ConnectionLost;

        bool Disconnect();
    }
}
