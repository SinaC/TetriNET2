using TetriNET2.Common.Logger;

namespace TetriNET2.Admin.Tests.Mocking
{
    public interface ILogMock
    {
        LogLevels LastLogLevel { get; }
        string LastLogLine { get; }
        void Clear();
    }
}
