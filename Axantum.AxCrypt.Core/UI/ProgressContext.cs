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
using Axantum.AxCrypt.Core.System;

namespace Axantum.AxCrypt.Core.UI
{
    public class ProgressContext
    {
        private static readonly TimeSpan DefaultFirstDelay = TimeSpan.FromMilliseconds(500);
        private static readonly TimeSpan DefaultInterval = TimeSpan.FromMilliseconds(100);

        private object _context;

        private ITiming _stopwatch = OS.Current.StartTiming();

        private TimeSpan _nextElapsed;

        public ProgressContext()
            : this(DefaultFirstDelay)
        {
        }

        public ProgressContext(string displayText)
            : this(displayText, null)
        {
        }

        public ProgressContext(string displayText, object context)
            : this(displayText, context, DefaultFirstDelay)
        {
        }

        public ProgressContext(TimeSpan firstElapsed)
            : this(String.Empty, null, firstElapsed)
        {
        }

        public ProgressContext(string displayText, object context, TimeSpan firstElapsed)
        {
            _context = context;
            DisplayText = displayText;
            _nextElapsed = firstElapsed;
            Max = -1;
        }

        public event EventHandler<ProgressEventArgs> Progressing;

        public string DisplayText { get; set; }

        public long Max { get; set; }

        private long _current = 0;

        private bool _finished = false;

        public long Current
        {
            get
            {
                return _current;
            }
            set
            {
                if (_finished)
                {
                    return;
                }
                _current = value;
                if (_stopwatch.Elapsed < _nextElapsed && Percent != 100)
                {
                    return;
                }
                ProgressEventArgs e;
                e = new ProgressEventArgs(Percent, _context);
                OnProgressing(e);
                if (Percent == 100)
                {
                    _finished = true;
                    return;
                }
                _nextElapsed = _stopwatch.Elapsed.Add(DefaultInterval);
            }
        }

        public void Finished()
        {
            Current = Max;
        }

        protected virtual void OnProgressing(ProgressEventArgs e)
        {
            EventHandler<ProgressEventArgs> handler = Progressing;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public int Percent
        {
            get
            {
                if (Current == Max)
                {
                    return 100;
                }
                if (Max >= 0)
                {
                    long current100 = _current * 100;
                    return (int)(current100 / Max);
                }
                return 0;
            }
        }
    }
}