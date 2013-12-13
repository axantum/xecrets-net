using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Runtime
{
    public class LoggingEventArgs : EventArgs
    {
        public string Message { get; private set; }

        public LoggingEventArgs(string message)
        {
            Message = message;
        }
    }
}