using System;
using TetriNET2.Common.Logger;

namespace TetriNET2.Admin.Tests.Mocking
{
    public class LogMock : ILog, ILogMock
    {
        #region ILogMock

        public LogLevels LastLogLevel { get; private set; }
        public string LastLogLine { get; private set; }
        
        public void Clear()
        {
            LastLogLevel = LogLevels.Debug;
            LastLogLine = null;
        }
        #endregion

        #region ILog

        public void Initialize(string path, string file, string fileTarget = "logfile")
        {
            // NOP
        }

        public void WriteLine(LogLevels level, string format, params object[] args)
        {
            try
            {
                LastLogLevel = level;
                LastLogLine = string.Format(format, args);
            }
            catch
            {
                // ignored
            }
        }

        #endregion
    }
}
