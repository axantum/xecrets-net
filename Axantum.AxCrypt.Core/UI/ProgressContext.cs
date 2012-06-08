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

namespace Axantum.AxCrypt.Core.UI
{
    public class ProgressContext
    {
        private object _context;

        private Stopwatch _stopwatch = Stopwatch.StartNew();

        private long _nextElapsed = 500;

        public ProgressContext(string displayText, object context)
            : this()
        {
            _context = context;
            DisplayText = displayText;
        }

        public ProgressContext()
        {
            Max = -1;
        }

        public event EventHandler<ProgressEventArgs> Progressing;

        public string DisplayText { get; set; }

        public long Max { get; set; }

        private long _current = 0;

        public long Current
        {
            get
            {
                return _current;
            }
            set
            {
                _current = value;
                if (_stopwatch.ElapsedMilliseconds < _nextElapsed)
                {
                    return;
                }
                ProgressEventArgs e;
                e = new ProgressEventArgs(Percent, _context);
                OnProgressing(e);
                _nextElapsed = _stopwatch.ElapsedMilliseconds + 100;
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