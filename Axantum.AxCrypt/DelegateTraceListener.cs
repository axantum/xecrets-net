using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt
{
    public class DelegateTraceListener : TraceListener
    {
        private Action<string> _trace;

        public DelegateTraceListener(Action<string> trace)
        {
            _trace = trace;
        }

        public override void Write(string message)
        {
            _trace(message);
        }

        public override void WriteLine(string message)
        {
            Write(message + Environment.NewLine);
        }
    }
}