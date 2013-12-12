#region Coypright and License

/*
 * AxCrypt - Copyright 2013, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Linq;
using System.Timers;

namespace Axantum.AxCrypt.Mono
{
    public class DelayTimer : IDelayTimer
    {
        private Timer _timer = new Timer();

        public DelayTimer()
        {
            _timer.AutoReset = false;
            _timer.Elapsed += HandleTimerElapsedEvent;
        }

        private void HandleTimerElapsedEvent(object sender, ElapsedEventArgs e)
        {
            OnElapsed(new EventArgs());
        }

        protected virtual void OnElapsed(EventArgs e)
        {
            EventHandler<EventArgs> handler = Elapsed;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public TimeSpan Interval
        {
            set
            {
                _timer.Enabled = false;
                _timer.Interval = value.TotalMilliseconds;
            }
        }

        public event EventHandler<EventArgs> Elapsed;

        public void Start()
        {
            _timer.Enabled = false;
            _timer.Enabled = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeInternal();
            }
        }

        private void DisposeInternal()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }
    }
}