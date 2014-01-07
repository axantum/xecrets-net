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

using Axantum.AxCrypt.Core.Ipc;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core
{
    public static class Instance
    {
        public static KnownKeys KnownKeys
        {
            get { return Factory.Instance.Singleton<KnownKeys>(); }
        }

        public static IUIThread UIThread
        {
            get { return Factory.Instance.Singleton<IUIThread>(); }
        }

        public static IProgressBackground BackgroundWork
        {
            get { return Factory.Instance.Singleton<IProgressBackground>(); }
        }

        public static ParallelFileOperation ParallelBackground
        {
            get { return Factory.Instance.Singleton<ParallelFileOperation>(); }
        }

        public static IRuntimeEnvironment Environment
        {
            get { return Factory.Instance.Singleton<IRuntimeEnvironment>(); }
        }

        public static FileSystemState FileSystemState
        {
            get { return Factory.Instance.Singleton<FileSystemState>(); }
        }

        public static ProcessState ProcessState
        {
            get { return Factory.Instance.Singleton<ProcessState>(); }
        }

        public static CommandService CommandService
        {
            get { return Factory.Instance.Singleton<CommandService>(); }
        }

        public static IStatusChecker StatusChecker
        {
            get { return Factory.Instance.Singleton<IStatusChecker>(); }
        }

        public static IUserSettings UserSettings
        {
            get { return Factory.Instance.Singleton<IUserSettings>(); }
        }

        public static ILogging Log
        {
            get { return Factory.Instance.Singleton<ILogging>(); }
        }

        public static SessionNotificationMonitor SessionNotification
        {
            get { return Factory.Instance.Singleton<SessionNotificationMonitor>(); }
        }

        public static WorkFolder WorkFolder
        {
            get { return Factory.Instance.Singleton<WorkFolder>(); }
        }
    }
}