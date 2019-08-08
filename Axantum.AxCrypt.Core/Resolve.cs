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
using System.Diagnostics.CodeAnalysis;
using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Abstractions.Rest;
using Axantum.AxCrypt.Api.Implementation;
using Axantum.AxCrypt.Core.IO;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

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
            RegisterTypeFactories(assemblies);

            TypeMap.Register.Singleton<WorkFolderWatcher>(() => new WorkFolderWatcher());
            TypeMap.Register.Singleton<WorkFolder>(() => new WorkFolder(workFolderPath), () => New<WorkFolderWatcher>());
            TypeMap.Register.New<KnownPublicKeys>(() => KnownPublicKeys.Load(Resolve.WorkFolder.FileInfo.FileItemInfo("UserPublicKeys.txt"), New<IStringSerializer>()));
            TypeMap.Register.Singleton<FileSystemState>(() => FileSystemState.Create(Resolve.WorkFolder.FileInfo.FileItemInfo("FileSystemState.txt")));
        }

        public static void RegisterTypeFactories(IEnumerable<Assembly> assemblies)
        {
            TypeMap.Register.Singleton<KnownIdentities>(() => new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify));
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new ParallelFileOperation());
            TypeMap.Register.Singleton<ProcessState>(() => new ProcessState());
            TypeMap.Register.Singleton<UserSettingsVersion>(() => new UserSettingsVersion());
            TypeMap.Register.Singleton<UserSettings>(() => new UserSettings(New<ISettingsStore>(), New<IterationCalculator>()));
            TypeMap.Register.Singleton<SessionNotify>(() => new SessionNotify());
            TypeMap.Register.Singleton<IRandomGenerator>(() => new RandomGenerator());
            TypeMap.Register.Singleton<CommandHandler>(() => new CommandHandler());
            TypeMap.Register.Singleton<ActiveFileWatcher>(() => new ActiveFileWatcher());
            TypeMap.Register.Singleton<IAsymmetricFactory>(() => new BouncyCastleAsymmetricFactory());
            TypeMap.Register.Singleton<CryptoFactory>(() => new CryptoFactory(assemblies));
            TypeMap.Register.Singleton<CryptoPolicy>(() => new CryptoPolicy(assemblies));
            TypeMap.Register.Singleton<UserPublicKeyUpdateStatus>(() => new UserPublicKeyUpdateStatus());
            TypeMap.Register.Singleton<FileFilter>(() => new FileFilter());
            TypeMap.Register.New<CanOpenEncryptedFile>(() => new CanOpenEncryptedFile());

            TypeMap.Register.New<AxCryptFactory>(() => new AxCryptFactory());
            TypeMap.Register.New<AxCryptFile>(() => new AxCryptFile());
            TypeMap.Register.New<ActiveFileAction>(() => new ActiveFileAction());
            TypeMap.Register.New<FileOperation>(() => new FileOperation(Resolve.FileSystemState, Resolve.SessionNotify));
            TypeMap.Register.New<int, Salt>((size) => new Salt(size));
            TypeMap.Register.New<AxCryptUpdateCheck>(() => new AxCryptUpdateCheck(New<IVersion>().Current));
            TypeMap.Register.New<IterationCalculator>(() => new IterationCalculator());
            TypeMap.Register.New<IStringSerializer>(() => new StringSerializer(New<IAsymmetricFactory>().GetSerializers()));
        }

        public static KnownIdentities KnownIdentities
        {
            get { return New<KnownIdentities>(); }
        }

        public static IUIThread UIThread
        {
            get { return New<IUIThread>(); }
        }

        public static ParallelFileOperation ParallelFileOperation
        {
            get { return New<ParallelFileOperation>(); }
        }

        public static IRuntimeEnvironment Environment
        {
            get { return New<IRuntimeEnvironment>(); }
        }

        public static FileSystemState FileSystemState
        {
            get { return New<FileSystemState>(); }
        }

        public static ProcessState ProcessState
        {
            get { return New<ProcessState>(); }
        }

        public static CommandService CommandService
        {
            get { return New<CommandService>(); }
        }

        public static IStatusChecker StatusChecker
        {
            get { return New<IStatusChecker>(); }
        }

        public static UserSettings UserSettings
        {
            get { return New<UserSettings>(); }
        }

        public static ILogging Log
        {
            get { return New<ILogging>(); }
        }

        public static SessionNotify SessionNotify
        {
            get { return New<SessionNotify>(); }
        }

        public static WorkFolder WorkFolder
        {
            get { return New<WorkFolder>(); }
        }

        public static IRandomGenerator RandomGenerator
        {
            get { return New<IRandomGenerator>(); }
        }

        public static CryptoFactory CryptoFactory
        {
            get { return New<CryptoFactory>(); }
        }

        public static IPortableFactory Portable
        {
            get { return New<IPortableFactory>(); }
        }

        public static IStringSerializer Serializer
        {
            get { return New<IStringSerializer>(); }
        }

        public static IRestCaller RestCaller
        {
            get { return New<IRestCaller>(); }
        }

        public static IAsymmetricFactory AsymmetricFactory
        {
            get { return New<IAsymmetricFactory>(); }
        }
    }
}