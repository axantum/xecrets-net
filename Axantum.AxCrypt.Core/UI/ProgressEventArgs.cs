using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.UI
{
    public class ProgressEventArgs : EventArgs
    {
        internal ProgressEventArgs(int percent, object context)
        {
            Context = context;
            if (percent < 0 || percent > 100)
            {
                throw new ArgumentOutOfRangeException("percent");
            }
            Percent = percent;
        }

        public int Percent { get; private set; }

        public object Context { get; private set; }
    }
}