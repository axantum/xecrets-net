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
using Axantum.AxCrypt.Core.Ipc;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Mono;
using System;
using System.Globalization;
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

            FactoryRegistry.Instance.Clear();
        }

        private static void RegisterTypeFactories()
        {
            FactoryRegistry.Instance.Singleton<ILogging>(() => new Logging());
            FactoryRegistry.Instance.Singleton<IRuntimeEnvironment>(() => new RuntimeEnvironment());
            FactoryRegistry.Instance.Singleton<CommandService>(() => new CommandService(new HttpRequestServer(), new HttpRequestClient()));
            FactoryRegistry.Instance.Singleton<IUserSettings>(() => new UserSettings(UserSettings.DefaultPathInfo));
            FactoryRegistry.Instance.Singleton<FileSystemState>(() => FileSystemState.Create(FileSystemState.DefaultPathInfo));
            FactoryRegistry.Instance.Singleton<KnownKeys>(() => new KnownKeys());
            FactoryRegistry.Instance.Singleton<ParallelBackground>(() => new ParallelBackground());
            FactoryRegistry.Instance.Singleton<ProcessState>(() => new ProcessState());
            FactoryRegistry.Instance.Singleton<SessionNotificationMonitor>(() => new SessionNotificationMonitor());

            FactoryRegistry.Instance.Register<IDelayTimer>(() => new DelayTimer());
            FactoryRegistry.Instance.Register<AxCryptFile>(() => new AxCryptFile());
            FactoryRegistry.Instance.Register<ActiveFileAction>(() => new ActiveFileAction());

            Instance.SessionNotification.Notification += new SessionNotificationHandler(Instance.FileSystemState, Factory.ActiveFileAction, Factory.AxCryptFile).HandleNotification;
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