using System;

namespace TetriNET2.Common.Logger
{
    //public static class Log
    //{
    //    public static ILog Default { get; private set; }
    //
    //    static Log()
    //    {
    //        Default = new NLogger();
    //    }
    //
    //    public static void SetLogger(ILog log)
    //    {
    //        Default = log;
    //    }
    //}

    public class Log : ILog
    {
        #region Singleton

        private static readonly Lazy<Log> Lazy = new Lazy<Log>(() => new Log());

        public static Log Default
        {
            get { return Lazy.Value; }
        }

        private Log()
        {
        }

        #endregion

        public ILog Logger { get; set; }

        public void Initialize(string path, string file, string fileTarget = "logfile")
        {
            if (Logger == null)
                throw new InvalidOperationException("Logger has not been initialized");
            Logger.Initialize(path, file, fileTarget);
        }

        public void WriteLine(LogLevels level, string format, params object[] args)
        {
            if (Logger == null)
                throw new InvalidOperationException("Logger has not been initialized");
            Logger.WriteLine(level, format, args);
        }
    }
}
