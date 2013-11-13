using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Timers;

namespace Axantum.AxCrypt.Core.Runtime
{
    /// <summary>
    /// Delay an action for a minimum time, if action request occur while in a delay, the delay is extended.
    /// In effect, delay an action until a minimum idle time has passed.
    /// </summary>
    public class DelayedAction : IDisposable
    {
        private Action _action;

        private Timer _timer;

        /// <summary>
        /// Create an instance bound to an action delegate, a minimum idle time and an option synchronizingObject.
        /// </summary>
        /// <param name="action">The action to perform after the specified idle time.</param>
        /// <param name="minimumIdleTime">The minium time of idle before actually performing the action.</param>
        public DelayedAction(Action action, TimeSpan minimumIdleTime)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            _action = action;
            _timer = new Timer();
            _timer.AutoReset = false;
            _timer.Interval = minimumIdleTime.TotalMilliseconds;
            _timer.Elapsed += HandleTimerElapsedEvent;
        }

        private void HandleTimerElapsedEvent(object sender, ElapsedEventArgs e)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _action();
            }
        }

        /// <summary>
        /// Restart the idle timeer.
        /// </summary>
        public void RestartIdleTimer()
        {
            if (_timer == null)
            {
                throw new ObjectDisposedException("_timer");
            }
            _timer.Stop();
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