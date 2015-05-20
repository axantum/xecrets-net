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

using System.Collections.Generic;
using System.Reflection;

#endregion Coypright and License

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Ipc;
using Axantum.AxCrypt.Core.Portable;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Linq;
using Axantum.AxCrypt.Core.IO;
using System.Diagnostics.CodeAnalysis;

namespace Axantum.AxCrypt.Core
{
    /// <summary>
    /// Syntactic convenience methods for accessing well-known application singleton instances.
    /// </summary>
    public static class Resolve
    {
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This is not really complex.")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "This is not really complex.")]
        public static void RegisterTypeFactories(string workFolderPath, IEnumerable<Assembly> assemblies)
        {
            TypeMap.Register.Singleton<KnownKeys>(() => new KnownKeys(Resolve.FileSystemState, Resolve.SessionNotify));
            TypeMap.Register.Singleton<UserAsymmetricKeysStore>(() => new UserAsymmetricKeysStore(Resolve.WorkFolder.FileInfo));
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new ParallelFileOperation());
            TypeMap.Register.Singleton<FileSystemState>(() => FileSystemState.Create(Resolve.WorkFolder.FileInfo.FileItemInfo("FileSystemState.xml")));
            TypeMap.Register.Singleton<ProcessState>(() => new ProcessState());
            TypeMap.Register.Singleton<IUserSettings>(() => new UserSettings(Resolve.WorkFolder.FileInfo.FileItemInfo("UserSettings.txt"), TypeMap.Resolve.New<IterationCalculator>()));
            TypeMap.Register.Singleton<SessionNotify>(() => new SessionNotify());
            TypeMap.Register.Singleton<WorkFolderWatcher>(() => new WorkFolderWatcher());
            TypeMap.Register.Singleton<WorkFolder>(() => new WorkFolder(workFolderPath), () => TypeMap.Resolve.Singleton<WorkFolderWatcher>());
            TypeMap.Register.Singleton<IRandomGenerator>(() => new RandomGenerator());
            TypeMap.Register.Singleton<CommandHandler>(() => new CommandHandler());
            TypeMap.Register.Singleton<ActiveFileWatcher>(() => new ActiveFileWatcher());
            TypeMap.Register.Singleton<IAsymmetricFactory>(() => new BouncyCastleAsymmetricFactory());

            TypeMap.Register.Singleton<CryptoFactory>(() => new CryptoFactory(assemblies));
            TypeMap.Register.Singleton<CryptoPolicy>(() => new CryptoPolicy(assemblies));
            TypeMap.Register.Singleton<ICryptoPolicy>(() => TypeMap.Resolve.Singleton<CryptoPolicy>().CreateDefault());

            TypeMap.Register.New<AxCryptFactory>(() => new AxCryptFactory());
            TypeMap.Register.New<AxCryptFile>(() => new AxCryptFile());
            TypeMap.Register.New<ActiveFileAction>(() => new ActiveFileAction());
            TypeMap.Register.New<FileOperation>(() => new FileOperation(Resolve.FileSystemState, Resolve.SessionNotify));
            TypeMap.Register.New<int, Salt>((size) => new Salt(size));
            TypeMap.Register.New<Version, UpdateCheck>((version) => new UpdateCheck(version));
            TypeMap.Register.New<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));
            TypeMap.Register.New<IterationCalculator>(() => new IterationCalculator());
            TypeMap.Register.New<IStringSerializer>(() => new StringSerializer(TypeMap.Resolve.Singleton<IAsymmetricFactory>().GetConverters()));
        }

        public static KnownKeys KnownKeys
        {
            get { return TypeMap.Resolve.Singleton<KnownKeys>(); }
        }

        public static UserAsymmetricKeysStore AsymmetricKeysStore
        {
            get { return TypeMap.Resolve.Singleton<UserAsymmetricKeysStore>(); }
        }

        public static IUIThread UIThread
        {
            get { return TypeMap.Resolve.Singleton<IUIThread>(); }
        }

        public static IProgressBackground ProgressBackground
        {
            get { return TypeMap.Resolve.Singleton<IProgressBackground>(); }
        }

        public static ParallelFileOperation ParallelFileOperation
        {
            get { return TypeMap.Resolve.Singleton<ParallelFileOperation>(); }
        }

        public static IRuntimeEnvironment Environment
        {
            get { return TypeMap.Resolve.Singleton<IRuntimeEnvironment>(); }
        }

        public static FileSystemState FileSystemState
        {
            get { return TypeMap.Resolve.Singleton<FileSystemState>(); }
        }

        public static ProcessState ProcessState
        {
            get { return TypeMap.Resolve.Singleton<ProcessState>(); }
        }

        public static CommandService CommandService
        {
            get { return TypeMap.Resolve.Singleton<CommandService>(); }
        }

        public static IStatusChecker StatusChecker
        {
            get { return TypeMap.Resolve.Singleton<IStatusChecker>(); }
        }

        public static IUserSettings UserSettings
        {
            get { return TypeMap.Resolve.Singleton<IUserSettings>(); }
        }

        public static ILogging Log
        {
            get { return TypeMap.Resolve.Singleton<ILogging>(); }
        }

        public static SessionNotify SessionNotify
        {
            get { return TypeMap.Resolve.Singleton<SessionNotify>(); }
        }

        public static WorkFolder WorkFolder
        {
            get { return TypeMap.Resolve.Singleton<WorkFolder>(); }
        }

        public static IRandomGenerator RandomGenerator
        {
            get { return TypeMap.Resolve.Singleton<IRandomGenerator>(); }
        }

        public static CryptoFactory CryptoFactory
        {
            get { return TypeMap.Resolve.Singleton<CryptoFactory>(); }
        }

        public static IPortableFactory Portable
        {
            get { return TypeMap.Resolve.Singleton<IPortableFactory>(); }
        }

        public static IStringSerializer Serializer
        {
            get { return TypeMap.Resolve.New<IStringSerializer>(); }
        }
    }
}