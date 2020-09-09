using AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AxCrypt.Core.UI
{
    public class ProgressTotals
    {
        private ITiming _stopwatch = OS.Current.StartTiming();

        private int _numberOfFiles;

        public int NumberOfFiles { get { return _numberOfFiles; } set { _numberOfFiles = value; } }

        public void AddFileCount(int count)
        {
            Interlocked.Add(ref _numberOfFiles, count);
        }

        public void Pause()
        {
            _stopwatch.Pause();
        }

        public void Resume()
        {
            _stopwatch.Resume();
        }

        public TimeSpan Elapsed
        {
            get
            {
                return _stopwatch.Elapsed;
            }
        }
    }
}