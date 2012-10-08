using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Runtime;

namespace Axantum.AxCrypt.Core.Test
{
    internal class FakeLogging : ILogging
    {
        #region ILogging Members

        public void SetLevel(LogLevel level)
        {
        }

        public bool IsFatalEnabled
        {
            get { return true; }
        }

        public bool IsErrorEnabled
        {
            get { return true; }
        }

        public bool IsWarningEnabled
        {
            get { return true; }
        }

        public bool IsInfoEnabled
        {
            get { return true; }
        }

        public bool IsDebugEnabled
        {
            get { return true; }
        }

        public void LogFatal(string message)
        {
        }

        public void LogError(string message)
        {
        }

        public void LogWarning(string message)
        {
        }

        public void LogInfo(string message)
        {
        }

        public void LogDebug(string message)
        {
        }

        #endregion ILogging Members
    }
}