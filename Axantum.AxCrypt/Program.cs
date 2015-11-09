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

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Api;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Algorithm;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Ipc;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Desktop;
using Axantum.AxCrypt.Forms.Implementation;
using Axantum.AxCrypt.Forms.Style;
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

using static Axantum.AxCrypt.Abstractions.TypeResolve;

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
                ExplorerRefresh.Notify();
            }

            Resolve.CommandService.Dispose();
            TypeMap.Register.Clear();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Dependency registration, not real complexity")]
        private static void RegisterTypeFactories(string startPath)
        {
            string workFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"AxCrypt" + Path.DirectorySeparatorChar);
            IEnumerable<Assembly> extraAssemblies = LoadFromFiles(new DirectoryInfo(Path.GetDirectoryName(startPath)).GetFiles("*.dll"));

            Resolve.RegisterTypeFactories(workFolderPath, extraAssemblies);
            RuntimeEnvironment.RegisterTypeFactories();
            DesktopFactory.RegisterTypeFactories();

            TypeMap.Register.New<IDataProtection>(() => new DataProtection());
            TypeMap.Register.New<ILauncher>(() => new Launcher());
            TypeMap.Register.New<AxCryptHMACSHA1>(() => PortableFactory.AxCryptHMACSHA1());
            TypeMap.Register.New<HMACSHA512>(() => PortableFactory.HMACSHA512());
            TypeMap.Register.New<Aes>(() => new Axantum.AxCrypt.Mono.Cryptography.AesWrapper(new System.Security.Cryptography.AesCryptoServiceProvider()));
            TypeMap.Register.New<Sha1>(() => PortableFactory.SHA1Managed());
            TypeMap.Register.New<Sha256>(() => PortableFactory.SHA256Managed());
            TypeMap.Register.New<CryptoStream>(() => PortableFactory.CryptoStream());
            TypeMap.Register.New<RandomNumberGenerator>(() => PortableFactory.RandomNumberGenerator());
            TypeMap.Register.New<LogOnIdentity, IAccountService>((LogOnIdentity identity) => new CachingAccountService(new DeviceAccountService(new LocalAccountService(identity, Resolve.WorkFolder.FileInfo), new ApiAccountService(new AxCryptApiClient(identity.ToRestIdentity(), Resolve.UserSettings.RestApiBaseUrl, Resolve.UserSettings.ApiTimeout)))));
            TypeMap.Register.New<GlobalApiClient>(() => new GlobalApiClient(Resolve.UserSettings.RestApiBaseUrl, Resolve.UserSettings.ApiTimeout));

            TypeMap.Register.Singleton<FontLoader>(() => new FontLoader());
            TypeMap.Register.Singleton<IEmailParser>(() => new EmailParser());
            TypeMap.Register.Singleton<KeyPairService>(() => new KeyPairService(1, 0, New<IUserSettings>().AsymmetricKeyBits));
            TypeMap.Register.Singleton<ICache>(() => new ItemCache());
            TypeMap.Register.Singleton<DummyReferencedType>(() => new DummyReferencedType());
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
            Resolve.SessionNotify.Notification += (sender, e) => New<SessionNotificationHandler>().HandleNotification(e.Notification);
        }

        private static void SetCulture()
        {
            if (String.IsNullOrEmpty(Resolve.UserSettings.CultureName))
            {
                return;
            }
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(Resolve.UserSettings.CultureName);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static void RunInteractive()
        {
            if (!OS.Current.IsFirstInstance)
            {
                Resolve.CommandService.Call(CommandVerb.Show, -1);
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            try
            {
                Application.Run(new AxCryptMainForm());
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }
                MessageBox.Show(ex.Message, "Unhandled Exception");
            }
            finally
            {
                AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
                Application.ThreadException -= Application_ThreadException;
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is ApplicationExitException)
            {
                Application.Exit();
            }
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            if (e.Exception is ApplicationExitException)
            {
                Application.Exit();
            }
        }
    }
}