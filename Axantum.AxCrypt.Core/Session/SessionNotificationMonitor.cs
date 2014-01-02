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
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Core.Session
{
    public class SessionNotificationMonitor : IDisposable
    {
        private DelayedAction _delayedNotification;

        public SessionNotificationMonitor(DelayedAction delayedAction)
        {
            _notifications = new HashSet<SessionNotification>();
            _delayedNotification = delayedAction;
            _delayedNotification.Action += (sender, e) => { OnDelayedNotification(); };
        }

        private bool _handleSessionChangedInProgress = false;

        protected virtual void OnDelayedNotification()
        {
            IEnumerable<SessionNotification> notifications;
            lock (_notifications)
            {
                if (!_notifications.Any())
                {
                    return;
                }
                if (_handleSessionChangedInProgress)
                {
                    _delayedNotification.StartIdleTimer();
                    return;
                }
                notifications = new List<SessionNotification>(_notifications);
                _notifications.Clear();

                _handleSessionChangedInProgress = true;
            }
            DoDelayedNotificationsInBackground(notifications);
        }

        public event EventHandler<SessionNotificationEventArgs> Notification;

        protected virtual void OnNotification(SessionNotificationEventArgs e)
        {
            EventHandler<SessionNotificationEventArgs> handler = Notification;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private HashSet<SessionNotification> _notifications;

        public virtual void Notify(SessionNotification notification)
        {
            lock (_notifications)
            {
                _notifications.Add(notification);
            }
            _delayedNotification.StartIdleTimer();
        }

        public void DoAllNow()
        {
            WaitForIdle();
            OnDelayedNotification();
            WaitForIdle();
        }

        private void WaitForIdle()
        {
            while (_handleSessionChangedInProgress)
            {
                Factory.New<ISleep>().Time(TimeSpan.Zero);
            }
        }

        private void DoDelayedNotificationsInBackground(IEnumerable<SessionNotification> notifications)
        {
            if (Instance.Log.IsInfoEnabled)
            {
                Instance.Log.LogInfo("Tick");
            }

            Instance.BackgroundWork.Work(
                (IProgressContext progress) =>
                {
                    progress.NotifyLevelStart();
                    try
                    {
                        HandleSessionEvents(notifications, progress);
                    }
                    finally
                    {
                        progress.NotifyLevelFinished();
                    }
                    return FileOperationStatus.Success;
                },
                (FileOperationStatus status) =>
                {
                    _handleSessionChangedInProgress = false;
                });
        }

        private void HandleSessionEvents(IEnumerable<SessionNotification> notifications, IProgressContext progress)
        {
            foreach (SessionNotification notification in notifications)
            {
                OnNotification(new SessionNotificationEventArgs(notification, progress));
            }
            OnNotification(new SessionNotificationEventArgs(new SessionNotification(SessionNotificationType.ActiveFileChange), progress));
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
            if (_delayedNotification != null)
            {
                _delayedNotification.Dispose();
                _delayedNotification = null;
            }
        }
    }
}