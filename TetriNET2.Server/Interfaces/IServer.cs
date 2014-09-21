using TetriNET2.Common.DataContracts;

namespace TetriNET2.Server.Interfaces
{
    public enum ServerStates
    {
        Waiting,
        Starting,
        Started,
        Stopping
    }

    public delegate void PerformRestartServerEventHandler(IServer server);

    public interface IServer
    {
        event PerformRestartServerEventHandler PerformRestartServer;

        ServerStates State { get; }
        Versioning Version { get; }

        bool AddHost(IHost.IHost host);

        bool SetVersion(int major, int minor);

        bool Start();
        bool Stop();
    }
}
