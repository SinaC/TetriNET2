﻿using System;

namespace TetriNET2.Common.Logger
{
    public sealed class NLogger : ILog
    {
        private readonly NLog.Logger _logger = NLog.LogManager.GetLogger("TetriNET2");

        #region ILog

        public void Initialize(string path, string file, string fileTarget = "logfile")
        {
            string logfile = System.IO.Path.Combine(path, file);
            NLog.Targets.FileTarget target = NLog.LogManager.Configuration.FindTargetByName(fileTarget) as NLog.Targets.FileTarget;
            if (target == null)
                throw new ApplicationException($"Couldn't find target {fileTarget} in NLog config");
            target.FileName = logfile;
        }

        public void WriteLine(LogLevels level, string format, params object[] args)
        {
            switch (level)
            {
                case LogLevels.Debug:
                    _logger.Debug(format, args);
                    break;
                case LogLevels.Info:
                    _logger.Info(format, args);
                    break;
                case LogLevels.Warning:
                    _logger.Warn(format, args);
                    break;
                case LogLevels.Error:
                    _logger.Error(format, args);
                    break;
            }
        }

        #endregion
    }
}
