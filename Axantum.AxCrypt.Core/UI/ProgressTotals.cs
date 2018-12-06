using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Core.UI
{
    public class ProgressTotals
    {
        private ITiming _stopwatch = OS.Current.StartTiming();

        public int NumberOfFiles { get; set; }

        public TimeSpan Elapsed
        {
            get
            {
                return _stopwatch.Elapsed;
            }
        }

        public void Done()
        {
            _stopwatch.Stop();
        }

        public void Reset()
        {
            _stopwatch.Stop();
        }
    }
}