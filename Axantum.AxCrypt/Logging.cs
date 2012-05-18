using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt
{
    public static class Logging
    {
        private static TraceSwitch _switch = InitializeTraceSwitch();

        public static bool IsWarningEnabled
        {
            get
            {
                return _switch.Level >= TraceLevel.Warning;
            }
        }

        public static bool IsErrorEnabled
        {
            get
            {
                return _switch.Level >= TraceLevel.Error;
            }
        }

        public static bool IsInfoEnabled
        {
            get
            {
                return _switch.Level >= TraceLevel.Info;
            }
        }

        public static bool IsDebugEnabled
        {
            get
            {
                return _switch.Level >= TraceLevel.Verbose;
            }
        }

        public static void Warning(string message)
        {
            Trace.TraceWarning(message);
        }

        public static void Error(string message)
        {
            Trace.TraceError(message);
        }

        public static void Info(string message)
        {
            Trace.TraceInformation(message);
        }

        public static void Verbose(string message)
        {
            Trace.Write(message);
        }

        private static TraceSwitch InitializeTraceSwitch()
        {
            TraceSwitch traceSwitch = new TraceSwitch("axCryptSwitch", "Logging levels for AxCrypt");
#if DEBUG
            traceSwitch.Level = TraceLevel.Verbose;
#else
            traceSwitch.Level = TraceLevel.Error;
#endif
            return traceSwitch;
        }
    }
}