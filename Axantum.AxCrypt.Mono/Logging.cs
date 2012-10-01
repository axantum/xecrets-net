using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.System;

namespace Axantum.AxCrypt.Mono
{
    internal class Logging : ILogging
    {
        private TraceSwitch _switch = InitializeTraceSwitch();

        #region ILogging Members

        public void SetLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Fatal:
                    _switch.Level = TraceLevel.Off;
                    break;

                case LogLevel.Error:
                    _switch.Level = TraceLevel.Error;
                    break;

                case LogLevel.Warning:
                    _switch.Level = TraceLevel.Warning;
                    break;

                case LogLevel.Info:
                    _switch.Level = TraceLevel.Info;
                    break;

                case LogLevel.Debug:
                    _switch.Level = TraceLevel.Verbose;
                    break;
                default:
                    throw new ArgumentException("level must be a value form the LogLevel enumeration.");
            }
        }

        public bool IsFatalEnabled
        {
            get { return _switch.Level >= TraceLevel.Off; }
        }

        public bool IsErrorEnabled
        {
            get { return _switch.Level >= TraceLevel.Error; }
        }

        public bool IsWarningEnabled
        {
            get { return _switch.Level >= TraceLevel.Warning; }
        }

        public bool IsInfoEnabled
        {
            get { return _switch.Level >= TraceLevel.Info; }
        }

        public bool IsDebugEnabled
        {
            get { return _switch.Level >= TraceLevel.Verbose; }
        }

        public void LogFatal(string message)
        {
            if (IsFatalEnabled)
            {
                Trace.WriteLine("{1} Fatal: {0}".InvariantFormat(message, AppName));
            }
        }

        public void LogError(string message)
        {
            if (IsErrorEnabled)
            {
                Trace.TraceError(message);
            }
        }

        public void LogWarning(string message)
        {
            if (IsWarningEnabled)
            {
                Trace.TraceWarning(message);
            }
        }

        public void LogInfo(string message)
        {
            if (IsInfoEnabled)
            {
                Trace.TraceInformation(message);
            }
        }

        public void LogDebug(string message)
        {
            if (IsDebugEnabled)
            {
                Trace.WriteLine("{1} Debug: {0}".InvariantFormat(message, AppName));
            }
        }

        #endregion ILogging Members

        private static TraceSwitch InitializeTraceSwitch()
        {
            TraceSwitch traceSwitch = new TraceSwitch("axCryptSwitch", "Logging levels for AxCrypt");
            traceSwitch.Level = TraceLevel.Error;
            return traceSwitch;
        }

        private static string _appName;

        private static string AppName
        {
            get
            {
                if (_appName == null)
                {
                    _appName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
                }
                return _appName;
            }
        }
    }
}