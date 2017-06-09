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

using Axantum.AxCrypt.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Session
{
    public class SessionNotify
    {
        private List<Func<SessionNotification, Task>> _priorityCommands = new List<Func<SessionNotification, Task>>();

        public event EventHandler<SessionNotificationEventArgs> Notification;

        protected virtual void OnNotification(SessionNotificationEventArgs e)
        {
            Notification?.Invoke(this, e);
        }

        public void AddPriorityCommand(Func<SessionNotification, Task> priorityCommand)
        {
            _priorityCommands.Add(priorityCommand);
        }

        public void RemovePriorityCommand(Func<SessionNotification, Task> priorityCommand)
        {
            _priorityCommands.Remove(priorityCommand);
        }

        public virtual void Notify(SessionNotification notification)
        {
            Task.Run(async () =>
            {
                try
                {
                    foreach (Func<SessionNotification, Task> priorityCommand in _priorityCommands)
                    {
                        await priorityCommand(notification);
                    }

                    OnNotification(new SessionNotificationEventArgs(notification));
                }
                catch (Exception ex)
                {
                    ex.ReportAndDisplay();
                }
            });
        }
    }
}