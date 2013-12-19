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

using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Runtime
{
    /// <summary>
    /// Delay an action for a minimum time, if action request occur while in a delay, the delay is extended.
    /// In effect, delay an action until a minimum idle time has passed.
    /// </summary>
    public class DelayedAction : IDisposable
    {
        private IDelayTimer _timer;

        /// <summary>
        /// The action to perform after the specified idle time.
        /// </summary>
        public event EventHandler Action;

        protected virtual void OnAction()
        {
            EventHandler handler = Action;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        /// <summary>
        /// Create an instance bound to an action delegate and a minimum idle time.
        /// </summary>
        /// <param name="minimumIdleTime">The minium time of idle before actually performing the action.</param>
        public DelayedAction(IDelayTimer timer, TimeSpan minimumIdleTime)
        {
            _timer = timer;
            _timer.SetInterval(minimumIdleTime);
            _timer.Elapsed += HandleTimerElapsedEvent;
        }

        private void HandleTimerElapsedEvent(object sender, EventArgs e)
        {
            if (_timer != null)
            {
                OnAction();
            }
        }

        /// <summary>
        /// (Re)Start the idle timeer.
        /// </summary>
        public void StartIdleTimer()
        {
            if (_timer == null)
            {
                throw new ObjectDisposedException("_timer");
            }
            _timer.Start();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
            }
        }
    }
}