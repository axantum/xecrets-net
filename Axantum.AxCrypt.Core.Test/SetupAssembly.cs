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

using System.IO;

#endregion Coypright and License

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;

namespace Axantum.AxCrypt.Core.Test
{
    /// <summary>
    /// Not using SetUpFixtureAttribute etc because MonoDevelop does not always honor.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors", Justification = "NUnit requires there to be a parameterless constructor.")]
    internal static class SetupAssembly
    {
        public static void AssemblySetup()
        {
            Factory.Instance.Singleton<WorkFolder>(() => new WorkFolder(Path.GetPathRoot(Environment.CurrentDirectory) + @"WorkFolder\"));
            Factory.Instance.Singleton<IRuntimeEnvironment>(() => new FakeRuntimeEnvironment());
            Factory.Instance.Singleton<ILogging>(() => new FakeLogging());
            Factory.Instance.Singleton<IUserSettings>(() => new UserSettings(Instance.WorkFolder.FileInfo.Combine("UserSettings.txt"), Factory.New<IterationCalculator>()));
            Factory.Instance.Singleton<KnownKeys>(() => new KnownKeys(Instance.FileSystemState, Instance.SessionNotify));
            Factory.Instance.Singleton<ProcessState>(() => new ProcessState());
            Factory.Instance.Singleton<IUIThread>(() => new FakeUIThread());
            Factory.Instance.Singleton<IProgressBackground>(() => new FakeProgressBackground());
            Factory.Instance.Singleton<SessionNotify>(() => new SessionNotify());
            Factory.Instance.Singleton<FileSystemState>(() => FileSystemState.Create(Instance.WorkFolder.FileInfo.Combine("FileSystemState.xml")));
            Factory.Instance.Singleton<IStatusChecker>(() => new FakeStatusChecker());
            Factory.Instance.Singleton<IRandomGenerator>(() => new FakeRandomGenerator());
            Factory.Instance.Singleton<CryptoFactory>(() => CreateCryptoFactory());
            Factory.Instance.Singleton<ICryptoPolicy>(() => new ProCryptoPolicy());
            Factory.Instance.Singleton<ActiveFileWatcher>(() => new ActiveFileWatcher());
            Factory.Instance.Singleton<UserAsymmetricKeysStore>(() => new UserAsymmetricKeysStore(Instance.WorkFolder.FileInfo, Instance.KnownKeys));

            Factory.Instance.Register<AxCryptFactory>(() => new AxCryptFactory());
            Factory.Instance.Register<AxCryptFile>(() => new AxCryptFile());
            Factory.Instance.Register<ActiveFileAction>(() => new ActiveFileAction());
            Factory.Instance.Register<FileOperation>(() => new FileOperation(Instance.FileSystemState, Instance.SessionNotify));
            Factory.Instance.Register<IdentityViewModel>(() => new IdentityViewModel(Instance.FileSystemState, Instance.KnownKeys, Instance.UserSettings));
            Factory.Instance.Register<FileOperationViewModel>(() => new FileOperationViewModel(Instance.FileSystemState, Instance.SessionNotify, Instance.KnownKeys, Instance.ParallelFileOperation, Factory.Instance.Singleton<IStatusChecker>(), Factory.New<IdentityViewModel>()));
            Factory.Instance.Register<MainViewModel>(() => new MainViewModel(Instance.FileSystemState));
            Factory.Instance.Register<string, IRuntimeFileInfo>((path) => new FakeRuntimeFileInfo(path));
            Factory.Instance.Register<string, IFileWatcher>((path) => new FakeFileWatcher(path));
            Factory.Instance.Register<IterationCalculator>(() => new FakeIterationCalculator());

            Instance.UserSettings.SetKeyWrapIterations(V1Aes128CryptoFactory.CryptoId, 1234);
            Instance.UserSettings.ThumbprintSalt = Salt.Zero;
            Instance.Log.SetLevel(LogLevel.Debug);
        }

        public static CryptoFactory CreateCryptoFactory()
        {
            CryptoFactory factory = new CryptoFactory();
            factory.Add(() => new V2Aes256CryptoFactory());
            factory.Add(() => new V2Aes128CryptoFactory());
            factory.Add(() => new V1Aes128CryptoFactory());

            return factory;
        }

        public static void AssemblyTeardown()
        {
            FakeRuntimeFileInfo.ClearFiles();
            Factory.Instance.Clear();
        }

        internal static FakeRuntimeEnvironment FakeRuntimeEnvironment
        {
            get
            {
                return (FakeRuntimeEnvironment)OS.Current;
            }
        }
    }
}