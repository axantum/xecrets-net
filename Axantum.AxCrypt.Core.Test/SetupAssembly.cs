#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

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
            Factory.Instance.Singleton<WorkFolder>(() => new WorkFolder(@"C:\WorkFolder\"));
            Factory.Instance.Singleton<IRuntimeEnvironment>(() => new FakeRuntimeEnvironment());
            Factory.Instance.Singleton<ILogging>(() => new FakeLogging());
            Factory.Instance.Singleton<IUserSettings>(() => new UserSettings(Instance.WorkFolder.FileInfo.Combine("UserSettings.txt"), new KeyWrapIterationCalculator()));
            Factory.Instance.Singleton<KnownKeys>(() => new KnownKeys(Instance.FileSystemState, Instance.SessionNotify));
            Factory.Instance.Singleton<ProcessState>(() => new ProcessState());
            Factory.Instance.Singleton<IUIThread>(() => new FakeUIThread());
            Factory.Instance.Singleton<IProgressBackground>(() => new FakeProgressBackground());
            Factory.Instance.Singleton<SessionNotify>(() => new SessionNotify());
            Factory.Instance.Singleton<FileSystemState>(() => FileSystemState.Create(Instance.WorkFolder.FileInfo.Combine("FileSystemState.xml")));
            Factory.Instance.Singleton<IStatusChecker>(() => new FakeStatusChecker());

            Factory.Instance.Register<AxCryptFile>(() => new AxCryptFile());
            Factory.Instance.Register<ActiveFileAction>(() => new ActiveFileAction());
            Factory.Instance.Register<FileOperation>(() => new FileOperation(Instance.FileSystemState, Instance.SessionNotify));
            Factory.Instance.Register<IdentityViewModel>(() => new IdentityViewModel(Instance.FileSystemState, Instance.KnownKeys, Instance.UserSettings));
            Factory.Instance.Register<FileOperationViewModel>(() => new FileOperationViewModel(Instance.FileSystemState, Instance.KnownKeys, Instance.ParallelFileOperation, Factory.Instance.Singleton<IStatusChecker>(), Factory.New<IdentityViewModel>()));
            Factory.Instance.Register<MainViewModel>(() => new MainViewModel(Instance.FileSystemState, Factory.New<FileOperationViewModel>()));
            Factory.Instance.Register<string, IRuntimeFileInfo>((path) => new FakeRuntimeFileInfo(path));
            Factory.Instance.Register<string, IFileWatcher>((path) => new FakeFileWatcher(path));

            Instance.UserSettings.KeyWrapIterations = 1234;
            Instance.UserSettings.ThumbprintSalt = KeyWrapSalt.Zero;
            Instance.Log.SetLevel(LogLevel.Debug);
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