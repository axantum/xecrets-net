#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
 *
 * This file is part of AxCrypt.
 *
 * AxCrypt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AxCrypt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AxCrypt.  If not, see <http://www.gnu.org/licenses/>.
 *
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using System;
using System.Diagnostics;
using System.Threading;
using Axantum.AxCrypt.Core.Runtime;

namespace Axantum.AxCrypt.Core.UI
{
    /// <summary>
    /// Coordinate progress reporting, marshaling reports to the original instantiating thread (if
    /// it has a SynchronizationContext) and throttle the amount of calls based on a timer.
    /// </summary>
    public class ProgressContext
    {
        private static readonly TimeSpan TimeToFirstProgress = TimeSpan.FromMilliseconds(500);

        private static readonly TimeSpan ProgressTimeInterval = TimeSpan.FromMilliseconds(100);

        private TimeSpan _nextProgressTime;

        private SynchronizationContext _synchronizationContext;

        private ITiming _stopwatch = OS.Current.StartTiming();

        private static readonly object _progressLock = new object();

        private long _current = 0;

        private long _total = -1;

        private static readonly object _progressLevelLock = new object();

        private int _progressLevel = 0;

        public ProgressContext()
            : this(TimeToFirstProgress)
        {
        }

        public ProgressContext(TimeSpan timeToFirstProgress)
        {
            _nextProgressTime = timeToFirstProgress;
            if (SynchronizationContext.Current == null)
            {
                _synchronizationContext = new SynchronizationContext();
            }
            else
            {
                _synchronizationContext = SynchronizationContext.Current;
            }
        }

        public bool Cancel { get; set; }

        /// <summary>
        /// Progress has occurred. The actual number of events are throttled, so not all reports
        /// of progress will result in an event being raised. Only if NotifyLevelStart() / NotifyLevelFinished()
        /// are used, will a percentage of 100 be reported, and then only exactly once.
        /// </summary>
        public event EventHandler<ProgressEventArgs> Progressing;

        public void AddTotal(long partTotal)
        {
            Invariant();
            if (partTotal <= 0)
            {
                return;
            }
            lock (_progressLock)
            {
                if (_total < 0)
                {
                    _total = partTotal;
                }
                else
                {
                    _total += partTotal;
                }
            }
        }

        private bool Finished
        {
            get
            {
                return _progressLevel < 0;
            }
        }

        public void AddCount(long count)
        {
            Invariant();
            if (count <= 0)
            {
                return;
            }
            lock (_progressLock)
            {
                _current += count;
                if (_stopwatch.Elapsed < _nextProgressTime)
                {
                    return;
                }
                _nextProgressTime = _stopwatch.Elapsed.Add(ProgressTimeInterval);
            }
            ProgressEventArgs e = new ProgressEventArgs(Percent);
            OnProgressing(e);
        }

        private int Percent
        {
            get
            {
                lock (_progressLock)
                {
                    if (_total < 0)
                    {
                        return 0;
                    }
                    long current100 = _current * 100;
                    int percent = (int)(current100 / _total);
                    if (percent >= 100)
                    {
                        percent = 99;
                    }
                    return percent;
                }
            }
        }

        /// <summary>
        /// Start a new progress tracking level. Use this to indicate the start of a (sub-)operation
        /// that tracks progress.
        /// </summary>
        public void NotifyLevelStart()
        {
            Invariant();
            lock (_progressLevelLock)
            {
                ++_progressLevel;
            }
        }

        /// <summary>
        /// End a progress tracking level. When a transition to zero active levels occurs, then and only then,
        /// is a Progressing event raised with a percent value of 100.
        /// Calls to NotifyLevelStart() and NotifyLevelFinished() must be balanced.
        /// </summary>
        public void NotifyLevelFinished()
        {
            Invariant();
            lock (_progressLevelLock)
            {
                if (_progressLevel == 0)
                {
                    throw new InvalidOperationException("Call to NotifyLevelFinished() without prior call to NotifyLevelStart().");
                }
                if (--_progressLevel > 0)
                {
                    return;
                }
                --_progressLevel;
            }
            ProgressEventArgs e = new ProgressEventArgs(100);
            OnProgressing(e);
        }

        private void Invariant()
        {
            if (Finished)
            {
                throw new InvalidOperationException("Out-of-sequence call, cannot call after being finished.");
            }
            if (Cancel)
            {
                throw new OperationCanceledException("Operation canceled on request.");
            }
        }

        protected virtual void OnProgressing(ProgressEventArgs e)
        {
            EventHandler<ProgressEventArgs> handler = Progressing;
            if (handler != null)
            {
                _synchronizationContext.Send(
                    (object state) =>
                    {
                        handler(this, (ProgressEventArgs)e);
                    },
                    e);
            }
        }
    }
}