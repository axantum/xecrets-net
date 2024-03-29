﻿#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.UI
{
    /// <summary>
    /// Coordinate progress reporting, marshaling reports to the original instantiating thread (if
    /// it has a SynchronizationContext) and throttle the amount of calls based on a timer.
    /// </summary>
    public class ProgressContext : IProgressContext
    {
        private static readonly TimeSpan TimeToFirstProgress = TimeSpan.FromMilliseconds(500);

        private static readonly TimeSpan ProgressTimeInterval = TimeSpan.FromMilliseconds(100);

        private TimeSpan _nextProgressTime;

        private readonly TimeSpan _progressTimeInterval = ProgressTimeInterval;

        private static readonly object _progressLock = new object();

        private long _current = 0;

        private long _total = -1;

        private static readonly object _progressLevelLock = new object();

        private int _progressLevel = 0;

        private string _name = string.Empty;

        public ProgressContext()
            : this(TimeToFirstProgress)
        {
        }

        public ProgressContext(TimeSpan timeToFirstProgress)
        {
            _nextProgressTime = timeToFirstProgress;
        }

        public ProgressContext(TimeSpan timeToFirstProgress, TimeSpan progressTimeInterval)
            : this(timeToFirstProgress)
        {
            _progressTimeInterval = progressTimeInterval;
        }

        /// <summary>
        /// Set to true to have an OperationCanceledException being thrown in the progress reporting
        /// thread at the earliest opportunity.
        /// </summary>
        public bool Cancel
        {
            get;
            set;
        }

        public string Display
        {
            get
            {
                return _name;
            }

            set
            {
                if (_name != value)
                {
                    _name = value;
                    AddCount(0);
                }
            }
        }

        public bool AllItemsConfirmed { get; set; }

        private int _items;

        public int Items { get { return _items; } }

        public int AddItems(int count)
        {
            return Interlocked.Add(ref _items, count);
        }

        /// <summary>
        /// Progress has occurred. The actual number of events are throttled, so not all reports
        /// of progress will result in an event being raised. Only if NotifyLevelStart() / NotifyLevelFinished()
        /// are used, will a percentage of 100 be reported, and then only exactly once.
        /// </summary>
        public event EventHandler<ProgressEventArgs>? Progressing;

        protected virtual void OnProgressing(ProgressEventArgs e)
        {
            EventHandler<ProgressEventArgs>? handler = Progressing;
            if (handler != null)
            {
                New<IUIThread>().PostTo(() => handler(this, e));
            }
        }

        /// <summary>
        /// Add to the total work count.
        /// </summary>
        /// <param name="count">The amount of work to add.</param>
        public void AddTotal(long count)
        {
            Invariant();
            if (count <= 0)
            {
                return;
            }
            lock (_progressLock)
            {
                if (_total < 0)
                {
                    _total = count;
                }
                else
                {
                    _total += count;
                }
            }
        }

        /// <summary>
        /// Add to the count of work having been performed. May lead to a Progressing event.
        /// </summary>
        /// <param name="count">The amount of work having been performed in this step.</param>
        public void AddCount(long count)
        {
            Invariant();
            if (count < 0)
            {
                return;
            }
            lock (_progressLock)
            {
                _current += count;
                if (Totals.Elapsed < _nextProgressTime)
                {
                    return;
                }
                _nextProgressTime = Totals.Elapsed.Add(_progressTimeInterval);
            }
            ProgressEventArgs e = new ProgressEventArgs(Percent, Display);
            OnProgressing(e);
        }

        public void RemoveCount(long totalCount, long progressCount)
        {
            lock (_progressLock)
            {
                _total -= totalCount;
                _current -= progressCount;
            }
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

        public ProgressTotals Totals { get; } = new ProgressTotals();

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
                    throw new InvalidOperationException("Call to decrease notification level was called without prior call start a notification level.");
                }
                if (--_progressLevel > 0)
                {
                    return;
                }
                --_progressLevel;
            }
            ProgressEventArgs e = new ProgressEventArgs(100, Display);
            OnProgressing(e);
        }

        private void Invariant()
        {
            if (_progressLevel < 0)
            {
                throw new InvalidOperationException("Out-of-sequence call, cannot call after being finished.");
            }
        }

        public Task EnterSingleThread()
        {
            return Task.FromResult(default(object));
        }

        public void LeaveSingleThread()
        {
        }
    }
}
