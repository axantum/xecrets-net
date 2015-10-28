#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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

namespace Axantum.AxCrypt.Fake
{
    public class FakeDelayTimer : IDelayTimer
    {
        private ISleep _sleep;

        public FakeDelayTimer(ISleep sleep)
        {
            _sleep = sleep;
            _sleep.Elapsed += Sleep_Elapsed;
        }

        private void Sleep_Elapsed(object sender, SleepEventArgs e)
        {
            if (_elapsed >= _interval)
            {
                return;
            }
            _elapsed += e.Time;
            if (_elapsed >= _interval)
            {
                OnElapsed();
            }
        }

        private bool disposed;

        private TimeSpan _elapsed = TimeSpan.MinValue;

        private TimeSpan _interval;

        public void SetInterval(TimeSpan interval)
        {
            _interval = interval;
        }

        public event EventHandler<EventArgs> Elapsed;

        public void Start()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("this");
            }
            _elapsed = TimeSpan.Zero;
        }

        protected virtual void OnElapsed()
        {
            EventHandler<EventArgs> handler = Elapsed;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                _sleep.Elapsed -= Sleep_Elapsed;
            }
        }
    }
}