#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions copyright © 2022-2024, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets Cli, but is derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * The changes and additions are separately copyrighted and only licensed under GPL v3 or later as detailed below,
 * unless explicitly licensed otherwise. If you use any part of these changes and additions in your software,
 * please see https://www.gnu.org/licenses/ for details of what this means for you.
 * 
 * Warning: If you are using the original AxCrypt code under a non-GPL v3 or later license, these changes and additions
 * are not included in that license. If you use these changes under those circumstances, all your code becomes subject to
 * the GPL v3 or later license, according to the principle of strong copyleft as applied to GPL v3 or later.
 *
 * Xecrets Cli is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets Cli is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets Cli. If not, see
 * https://www.gnu.org/licenses/.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions, as well for commit history detailing changes and additions that fall under the strong
 * copyleft provisions mentioned above. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Xecrets Cli Copyright and GPL License notice
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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

using System.Reflection;

#endregion Coypright and License

using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Ipc;
using AxCrypt.Core.Portable;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.Session;
using AxCrypt.Core.UI;
using System.Diagnostics.CodeAnalysis;
using AxCrypt.Abstractions;
using AxCrypt.Abstractions.Rest;
using AxCrypt.Core.IO;
using static AxCrypt.Abstractions.TypeResolve;
using Xecrets.Net.Api.Implementation;
using Xecrets.Net.Core;
using Xecrets.Net.Core.Crypto.Asymmetric;

namespace AxCrypt.Core
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
            TypeMap.Register.Singleton<IAsymmetricFactory>(() => new NetAsymmetricFactory());
            TypeMap.Register.Singleton<CryptoFactory>(() => new CryptoFactory(assemblies));
            TypeMap.Register.Singleton<CryptoPolicy>(() => new CryptoPolicy(assemblies));
            TypeMap.Register.Singleton<UserPublicKeyUpdateStatus>(() => new UserPublicKeyUpdateStatus());
            TypeMap.Register.Singleton<FileFilter>(() => new FileFilter());
            TypeMap.Register.Singleton<CanOpenEncryptedFile>(() => new CanOpenEncryptedFile());

            TypeMap.Register.New<AxCryptFactory>(() => new AxCryptFactory());
            TypeMap.Register.New<AxCryptFile>(() => new AxCryptFile());
            TypeMap.Register.New<ActiveFileAction>(() => new ActiveFileAction());
            TypeMap.Register.New<FileOperation>(() => new FileOperation(Resolve.FileSystemState, Resolve.SessionNotify));
            TypeMap.Register.New<int, Salt>((size) => new Salt(size));
            TypeMap.Register.New<AxCryptUpdateCheck>(() => new AxCryptUpdateCheck(New<IVersion>().Current));
            TypeMap.Register.New<IterationCalculator>(() => new IterationCalculator());
            TypeMap.Register.Singleton<IStringSerializer>(() => new SystemTextJsonStringSerializer(JsonSourceGenerationContext.CreateJsonSerializerContext(New<IAsymmetricFactory>().GetConverters())));
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
