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

        private StringBuilder _buffer = new StringBuilder();

        public DelegateTraceListener(Action<string> trace)
        {
            _trace = trace;
        }

        public override void Write(string message)
        {
            int i;
            while ((i = message.IndexOf(Environment.NewLine, StringComparison.Ordinal)) >= 0)
            {
                _buffer.Append(message.Substring(0, i + Environment.NewLine.Length));
                _trace(_buffer.ToString());
                _buffer.Clear();
                message = message.Substring(i + Environment.NewLine.Length);
            }
            _buffer.Append(message);
        }

        public override void WriteLine(string message)
        {
            Write(message + Environment.NewLine);
        }
    }
}