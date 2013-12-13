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
            get { return FactoryRegistry.Instance.Singleton<KnownKeys>(); }
        }

        public static IUIThread UIThread
        {
            get { return FactoryRegistry.Instance.Singleton<IUIThread>(); }
        }

        public static IProgressBackground BackgroundWork
        {
            get { return FactoryRegistry.Instance.Singleton<IProgressBackground>(); }
        }

        public static ParallelBackground ParallelBackground
        {
            get { return FactoryRegistry.Instance.Singleton<ParallelBackground>(); }
        }

        public static IRuntimeEnvironment Environment
        {
            get { return FactoryRegistry.Instance.Singleton<IRuntimeEnvironment>(); }
        }

        public static FileSystemState FileSystemState
        {
            get { return FactoryRegistry.Instance.Singleton<FileSystemState>(); }
        }

        public static ProcessState ProcessState
        {
            get { return FactoryRegistry.Instance.Singleton<ProcessState>(); }
        }

        public static CommandService CommandService
        {
            get { return FactoryRegistry.Instance.Singleton<CommandService>(); }
        }

        public static IStatusChecker StatusChecker
        {
            get { return FactoryRegistry.Instance.Singleton<IStatusChecker>(); }
        }

        public static IUserSettings UserSettings
        {
            get { return FactoryRegistry.Instance.Singleton<IUserSettings>(); }
        }

        public static ISleep Sleep
        {
            get { return FactoryRegistry.Instance.Singleton<ISleep>(); }
        }

        public static ILogging Log
        {
            get { return FactoryRegistry.Instance.Singleton<ILogging>(); }
        }
    }
}