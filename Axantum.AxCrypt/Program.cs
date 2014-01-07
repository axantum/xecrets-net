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

using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Ipc;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Mono;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            RegisterTypeFactories();
            WireupEvents();
            SetCulture();

            string[] commandLineArgs = Environment.GetCommandLineArgs();
            if (commandLineArgs.Length == 1)
            {
                RunInteractive();
            }
            else
            {
                new CommandLine(commandLineArgs[0], commandLineArgs.Skip(1)).Execute();
            }

            Factory.Instance.Clear();
        }

        private static void RegisterTypeFactories()
        {
            string workFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"AxCrypt" + Path.DirectorySeparatorChar);

            Factory.Instance.Singleton<ILogging>(() => new Logging());
            Factory.Instance.Singleton<CommandService>(() => new CommandService(new HttpRequestServer(), new HttpRequestClient()));
            Factory.Instance.Singleton<IUserSettings>(() => new UserSettings(Factory.New<IRuntimeFileInfo>(Path.Combine(workFolderPath, "UserSettings.txt")), new KeyWrapIterationCalculator()));
            Factory.Instance.Singleton<FileSystemState>(() => FileSystemState.Create(Factory.New<IRuntimeFileInfo>(Path.Combine(workFolderPath, "FileSystemState.xml"))));
            Factory.Instance.Singleton<KnownKeys>(() => new KnownKeys(Instance.FileSystemState, Instance.SessionNotification));
            Factory.Instance.Singleton<ParallelFileOperation>(() => new ParallelFileOperation(Instance.UIThread));
            Factory.Instance.Singleton<ProcessState>(() => new ProcessState());
            Factory.Instance.Singleton<SessionNotificationMonitor>(() => new SessionNotificationMonitor());

            Factory.Instance.Register<AxCryptFile>(() => new AxCryptFile());
            Factory.Instance.Register<ActiveFileAction>(() => new ActiveFileAction());
            Factory.Instance.Register<ISleep>(() => new Sleep());
            Factory.Instance.Register<FileOperation>(() => new FileOperation(Instance.FileSystemState, Instance.SessionNotification));
            Factory.Instance.Register<SessionNotificationHandler>(() => new SessionNotificationHandler(Instance.FileSystemState, Factory.New<ActiveFileAction>(), Factory.New<AxCryptFile>()));
            Factory.Instance.Register<int, KeyWrapSalt>((length) => new KeyWrapSalt(length));
            Factory.Instance.Register<Version, UpdateCheck>((version) => new UpdateCheck(version));
            Factory.Instance.Register<IdentityViewModel>(() => new IdentityViewModel(Instance.FileSystemState, Instance.KnownKeys, Instance.UserSettings));
            Factory.Instance.Register<MainViewModel>(() => new MainViewModel(Instance.FileSystemState, Factory.New<IdentityViewModel>()));

            Factory.Instance.Singleton<IRuntimeEnvironment>(() => new RuntimeEnvironment(".axx"));
            Factory.Instance.Register<string, IFileWatcher>((path) => new FileWatcher(path, new DelayedAction(new DelayTimer(), Instance.UserSettings.SessionNotificationMinimumIdle)));
            Factory.Instance.Register<string, IRuntimeFileInfo>((path) => new RuntimeFileInfo(path));
        }

        private static void WireupEvents()
        {
            Instance.SessionNotification.Notification += (sender, e) => Factory.New<SessionNotificationHandler>().HandleNotification(e.Notification);
        }

        private static void SetCulture()
        {
            if (String.IsNullOrEmpty(Instance.UserSettings.CultureName))
            {
                return;
            }
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(Instance.UserSettings.CultureName);
        }

        private static void RunInteractive()
        {
            if (!OS.Current.IsFirstInstance)
            {
                Instance.CommandService.Call(CommandVerb.Show);
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new AxCryptMainForm());
        }
    }
}