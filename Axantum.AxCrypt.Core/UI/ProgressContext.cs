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
        private static readonly TimeSpan DefaultFirstProgressing = TimeSpan.FromMilliseconds(500);
        private static readonly TimeSpan DefaultInterval = TimeSpan.FromMilliseconds(100);

        private object _context;

        private SynchronizationContext _synchronizationContext;

        private ITiming _stopwatch = OS.Current.StartTiming();

        public TimeSpan NextProgressing { get; set; }

        public ProgressContext()
            : this(DefaultFirstProgressing)
        {
        }

        public ProgressContext(TimeSpan firstProgressing)
            : this(null, firstProgressing)
        {
        }

        public ProgressContext(object context, TimeSpan firstProgressing)
        {
            _context = context;
            NextProgressing = firstProgressing;
            Finished = false;
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

        private static readonly object _lock = new object();

        private long _total = -1;

        public void AddTotal(long partTotal)
        {
            lock (_lock)
            {
                if (Finished)
                {
                    throw new InvalidOperationException("Out-of-sequence call, cannot call AddTotal() after call to Finished()");
                }
                if (partTotal <= 0)
                {
                    return;
                }
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

        private long _current = 0;

        public bool Finished { get; private set; }

        public void AddCount(long count)
        {
            ProgressEventArgs e;
            lock (_lock)
            {
                if (Finished)
                {
                    throw new InvalidOperationException("Out-of-sequence call, cannot call AddCount() after call to Finished()");
                }
                if (Cancel)
                {
                    throw new OperationCanceledException("Operation canceled on request");
                }
                if (count <= 0)
                {
                    return;
                }
                _current += count;
                if (_stopwatch.Elapsed < NextProgressing)
                {
                    return;
                }
                e = new ProgressEventArgs(Percent, _context);
                NextProgressing = _stopwatch.Elapsed.Add(DefaultInterval);
            }
            OnProgressing(e);
        }

        private static readonly object _progressLevelLock = new object();

        private int _progressLevel = 0;

        /// <summary>
        /// Start a new progress tracking level. Use this to indicate the start of a (sub-)operation
        /// that tracks progress.
        /// </summary>
        public void NotifyLevelStart()
        {
            lock (_progressLevelLock)
            {
                if (_progressLevel < 0)
                {
                    throw new InvalidOperationException("Call to NotifyLevelStart() after having already finished.");
                }
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
            lock (_progressLevelLock)
            {
                if (_progressLevel == 0)
                {
                    throw new InvalidOperationException("Too many calls to NotifyLevelFinished()");
                }
                if (--_progressLevel > 0)
                {
                    return;
                }
                --_progressLevel;
            }
            ProgressEventArgs e;
            lock (_lock)
            {
                if (Finished)
                {
                    return;
                }
                Finished = true;
                e = new ProgressEventArgs(100, _context);
            }
            OnProgressing(e);
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

        private int Percent
        {
            get
            {
                lock (_lock)
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
    }
}