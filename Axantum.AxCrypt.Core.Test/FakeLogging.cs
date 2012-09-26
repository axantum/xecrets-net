using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.System;

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
            get { return false; }
        }

        public bool IsErrorEnabled
        {
            get { return false; }
        }

        public bool IsWarningEnabled
        {
            get { return false; }
        }

        public bool IsInfoEnabled
        {
            get { return false; }
        }

        public bool IsDebugEnabled
        {
            get { return false; }
        }

        public void Fatal(string message)
        {
        }

        public void Error(string message)
        {
        }

        public void Warning(string message)
        {
        }

        public void Info(string message)
        {
        }

        public void Debug(string message)
        {
        }

        #endregion ILogging Members
    }
}