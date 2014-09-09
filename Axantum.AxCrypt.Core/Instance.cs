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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Ipc;
using Axantum.AxCrypt.Core.Portable;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core
{
    /// <summary>
    /// Syntactic convenience methods for accessing well-known application singleton instances.
    /// </summary>
    public static class Instance
    {
        public static KnownKeys KnownKeys
        {
            get { return Factory.Instance.Singleton<KnownKeys>(); }
        }

        public static UserAsymmetricKeysStore AsymmetricKeysStore
        {
            get { return Factory.Instance.Singleton<UserAsymmetricKeysStore>(); }
        }

        public static IUIThread UIThread
        {
            get { return Factory.Instance.Singleton<IUIThread>(); }
        }

        public static IProgressBackground ProgressBackground
        {
            get { return Factory.Instance.Singleton<IProgressBackground>(); }
        }

        public static ParallelFileOperation ParallelFileOperation
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

        public static SessionNotify SessionNotify
        {
            get { return Factory.Instance.Singleton<SessionNotify>(); }
        }

        public static WorkFolder WorkFolder
        {
            get { return Factory.Instance.Singleton<WorkFolder>(); }
        }

        public static IRandomGenerator RandomGenerator
        {
            get { return Factory.Instance.Singleton<IRandomGenerator>(); }
        }

        public static CryptoFactory CryptoFactory
        {
            get { return Factory.Instance.Singleton<CryptoFactory>(); }
        }

        public static IPortableFactory Portable
        {
            get { return Factory.Instance.Singleton<IPortableFactory>(); }
        }
    }
}