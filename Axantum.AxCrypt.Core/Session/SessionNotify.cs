#region Coypright and License

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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Session
{
    public class SessionNotify
    {
        private List<Func<SessionNotification, Task>> _priorityCommands = new List<Func<SessionNotification, Task>>();

        private List<Func<SessionNotification, Task>> _commands = new List<Func<SessionNotification, Task>>();

        public void AddPriorityCommand(Func<SessionNotification, Task> priorityCommand)
        {
            lock (_priorityCommands)
            {
                _priorityCommands.Add(priorityCommand);
            }
        }

        public void RemovePriorityCommand(Func<SessionNotification, Task> priorityCommand)
        {
            lock (_priorityCommands)
            {
                _priorityCommands.Remove(priorityCommand);
            }
        }

        public void AddCommand(Func<SessionNotification, Task> command)
        {
            lock (_commands)
            {
                _commands.Add(command);
            }
        }

        public void RemoveCommand(Func<SessionNotification, Task> command)
        {
            lock (_commands)
            {
                _commands.Remove(command);
            }
        }

        private readonly Queue<SessionNotification> _notificationQueue = new Queue<SessionNotification>();

        public virtual async Task NotifyAsync(SessionNotification notification)
        {
            lock (_notificationQueue)
            {
                _notificationQueue.Enqueue(notification);
                if (_notificationQueue.Count > 1)
                {
                    return;
                }
            }
            while (true)
            {
                lock (_notificationQueue)
                {
                    if (_notificationQueue.Count == 0)
                    {
                        return;
                    }
                    OptimizeQueue();
                    notification = _notificationQueue.Peek();
                }
                await NotifyInternal(notification);
                lock (_notificationQueue)
                {
                    _notificationQueue.Dequeue();
                }
            }
        }

        private void OptimizeQueue()
        {
            if (_notificationQueue.Count == 1)
            {
                return;
            }
            SessionNotification[] notifications = _notificationQueue.ToArray();
            _notificationQueue.Clear();
            SessionNotification currentNotification = null;
            foreach (SessionNotification notification in notifications)
            {
                if (currentNotification == null)
                {
                    currentNotification = notification;
                    continue;
                }
                if (notification.NotificationType == currentNotification.NotificationType && notification.Capabilities == currentNotification.Capabilities && notification.Identity == currentNotification.Identity)
                {
                    currentNotification = MergeNotificationFullNames(currentNotification, notification);
                    continue;
                }
                _notificationQueue.Enqueue(currentNotification);
                currentNotification = notification;
            }
            if (currentNotification != null)
            {
                _notificationQueue.Enqueue(currentNotification);
            }
        }

        private SessionNotification MergeNotificationFullNames(SessionNotification currentNotification, SessionNotification nextNotification)
        {
            IEnumerable<string> allFullNames = currentNotification.FullNames.Union(nextNotification.FullNames);

            return new SessionNotification(currentNotification.NotificationType, currentNotification.Identity, allFullNames, currentNotification.Capabilities);
        }

        private async Task NotifyInternal(SessionNotification notification)
        {
            try
            {
                foreach (Func<SessionNotification, Task> priorityCommand in SafeCopy(_priorityCommands))
                {
                    await priorityCommand(notification).Free();
                }

                foreach (Func<SessionNotification, Task> command in SafeCopy(_commands))
                {
                    await command(notification).Free();
                }
                if (notification.NotificationType != SessionNotificationType.SessionChange)
                {
                    New<InactivitySignOut>().RestartInactivityTimer();
                }
            }
            catch (Exception ex)
            {
                ex.ReportAndDisplay();
            }
        }

        private static IEnumerable<Func<SessionNotification, Task>> SafeCopy(List<Func<SessionNotification, Task>> notifications)
        {
            Func<SessionNotification, Task>[] copy;
            lock (notifications)
            {
                copy = notifications.ToArray();
            }
            return copy;
        }
    }
}