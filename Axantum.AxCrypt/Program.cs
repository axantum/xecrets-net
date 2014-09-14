﻿#region Coypright and License

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

using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Ipc;
using Axantum.AxCrypt.Core.Portable;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Desktop;
using Axantum.AxCrypt.Forms;
using Axantum.AxCrypt.Mono;
using Axantum.AxCrypt.Mono.Portable;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
            string[] commandLineArgs = Environment.GetCommandLineArgs();

            RegisterTypeFactories(commandLineArgs[0]);
            WireupEvents();
            SetCulture();

            if (commandLineArgs.Length == 1)
            {
                RunInteractive();
            }
            else
            {
                new CommandLine(commandLineArgs[0], commandLineArgs.Skip(1)).Execute();
            }

            Instance.CommandService.Dispose();
            Factory.Instance.Clear();
        }

        private static void RegisterTypeFactories(string startPath)
        {
            string workFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"AxCrypt" + Path.DirectorySeparatorChar);

            Instance.RegisterTypeFactories(workFolderPath);
            RuntimeEnvironment.RegisterTypeFactories();

            Factory.Instance.Singleton<CryptoFactory>(() => CreateCryptoFactory(startPath));
            Factory.Instance.Singleton<CryptoPolicy>(() => CreateCryptoPolicy(startPath));
            Factory.Instance.Singleton<ICryptoPolicy>(() => Factory.Instance.Singleton<CryptoPolicy>().CreateDefault());

            Factory.Instance.Register<IDataProtection>(() => new DataProtection());
        }

        private static CryptoFactory CreateCryptoFactory(string startPath)
        {
            IEnumerable<Assembly> extraAssemblies = LoadFromFiles(new DirectoryInfo(Path.GetDirectoryName(startPath)).GetFiles("*.dll"));
            IEnumerable<Type> types = TypeDiscovery.Interface(typeof(ICryptoFactory), extraAssemblies);

            CryptoFactory factory = new CryptoFactory();
            foreach (Type type in types)
            {
                factory.Add(() => Activator.CreateInstance(type) as ICryptoFactory);
            }
            return factory;
        }

        private static CryptoPolicy CreateCryptoPolicy(string startPath)
        {
            IEnumerable<Assembly> extraAssemblies = LoadFromFiles(new DirectoryInfo(Path.GetDirectoryName(startPath)).GetFiles("*.dll"));
            return new CryptoPolicy(extraAssemblies);
        }

        private static IEnumerable<Assembly> LoadFromFiles(IEnumerable<FileInfo> files)
        {
            List<Assembly> assemblies = new List<Assembly>();
            foreach (FileInfo file in files)
            {
                try
                {
                    assemblies.Add(Assembly.LoadFrom(file.FullName));
                }
                catch (BadImageFormatException)
                {
                    continue;
                }
                catch (FileLoadException)
                {
                    continue;
                }
            }
            return assemblies;
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
                Instance.CommandService.Call(CommandVerb.Show, -1);
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new AxCryptMainForm());
        }
    }
}