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

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Ipc;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Mono;

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

            Factory.Instance.Singleton<WorkFolderWatcher>(() => new WorkFolderWatcher());
            Factory.Instance.Singleton<WorkFolder>(() => new WorkFolder(workFolderPath), () => Factory.Instance.Singleton<WorkFolderWatcher>());
            Factory.Instance.Singleton<ILogging>(() => new Logging());
            Factory.Instance.Singleton<CommandService>(() => new CommandService(new HttpRequestServer(), new HttpRequestClient()));
            Factory.Instance.Singleton<IUserSettings>(() => new UserSettings(Factory.Instance.Singleton<WorkFolder>().FileInfo.Combine("UserSettings.txt"), Factory.New<IterationCalculator>()));
            Factory.Instance.Singleton<FileSystemState>(() => FileSystemState.Create(Factory.Instance.Singleton<WorkFolder>().FileInfo.Combine("FileSystemState.xml")));
            Factory.Instance.Singleton<KnownKeys>(() => new KnownKeys(Instance.FileSystemState, Instance.SessionNotify));
            Factory.Instance.Singleton<ParallelFileOperation>(() => new ParallelFileOperation());
            Factory.Instance.Singleton<ProcessState>(() => new ProcessState());
            Factory.Instance.Singleton<SessionNotify>(() => new SessionNotify());
            Factory.Instance.Singleton<IRandomGenerator>(() => new RandomGenerator());
            Factory.Instance.Singleton<CryptoFactory>(() => CreateCryptoFactory());
            Factory.Instance.Singleton<ICryptoPolicy>(() => new ProCryptoPolicy());

            Factory.Instance.Register<AxCryptFactory>(() => new AxCryptFactory());
            Factory.Instance.Register<AxCryptFile>(() => new AxCryptFile());
            Factory.Instance.Register<ActiveFileAction>(() => new ActiveFileAction());
            Factory.Instance.Register<ISleep>(() => new Sleep());
            Factory.Instance.Register<FileOperation>(() => new FileOperation(Instance.FileSystemState, Instance.SessionNotify));
            Factory.Instance.Register<SessionNotificationHandler>(() => new SessionNotificationHandler(Instance.FileSystemState, Instance.KnownKeys, Factory.New<ActiveFileAction>(), Factory.New<AxCryptFile>()));
            Factory.Instance.Register<int, Salt>((size) => new Salt(size));
            Factory.Instance.Register<Version, UpdateCheck>((version) => new UpdateCheck(version));
            Factory.Instance.Register<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));
            Factory.Instance.Register<IterationCalculator>(() => new IterationCalculator());

            Factory.Instance.Singleton<IRuntimeEnvironment>(() => new RuntimeEnvironment(".axx"));
            Factory.Instance.Register<string, IFileWatcher>((path) => new FileWatcher(path, new DelayedAction(new DelayTimer(), Instance.UserSettings.SessionNotificationMinimumIdle)));
            Factory.Instance.Register<string, IRuntimeFileInfo>((path) => new RuntimeFileInfo(path));
        }

        private static CryptoFactory CreateCryptoFactory()
        {
            CryptoFactory factory = new CryptoFactory();
            foreach (Func<ICryptoFactory> activator in CryptoFactoryDiscovery.Discover(Assembly.GetAssembly(typeof(CryptoFactory))))
            {
                factory.Add(activator);
            }
            return factory;
        }

        private static void WireupEvents()
        {
            Instance.SessionNotify.Notification += (sender, e) => Factory.New<SessionNotificationHandler>().HandleNotification(e.Notification);
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