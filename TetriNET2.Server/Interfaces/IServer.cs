﻿using TetriNET2.Common.DataContracts;

namespace TetriNET2.Server.Interfaces
{
    public enum ServerStates
    {
        Waiting,
        Starting,
        Started,
        Stopping
    }

    public delegate void PerformRestartServerEventHandler();

    public interface IServer
    {
        event PerformRestartServerEventHandler PerformRestartServer;

        ServerStates State { get; }
        Versioning Version { get; }

        bool AddHost(IHost host);

        void SetVersion(int major, int minor);

        void Start();
        void Stop();
    }
}
