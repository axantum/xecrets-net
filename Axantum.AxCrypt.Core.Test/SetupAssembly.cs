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
using Axantum.AxCrypt.Core.Portable;
using Axantum.AxCrypt.Mono.Portable;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Algorithm;

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
            TypeMap.Register.Singleton<IPortableFactory>(() => new PortableFactory());
            TypeMap.Register.Singleton<WorkFolder>(() => new WorkFolder(Path.GetPathRoot(Environment.CurrentDirectory) + @"WorkFolder\"));
            TypeMap.Register.Singleton<IRuntimeEnvironment>(() => new FakeRuntimeEnvironment());
            TypeMap.Register.Singleton<ILogging>(() => new FakeLogging());
            TypeMap.Register.Singleton<IUserSettings>(() => new UserSettings(Resolve.WorkFolder.FileInfo.FileItemInfo("UserSettings.txt"), TypeMap.Resolve.New<IterationCalculator>()));
            TypeMap.Register.Singleton<KnownKeys>(() => new KnownKeys(Resolve.FileSystemState, Resolve.SessionNotify));
            TypeMap.Register.Singleton<ProcessState>(() => new ProcessState());
            TypeMap.Register.Singleton<IUIThread>(() => new FakeUIThread());
            TypeMap.Register.Singleton<IProgressBackground>(() => new FakeProgressBackground());
            TypeMap.Register.Singleton<SessionNotify>(() => new SessionNotify());
            TypeMap.Register.Singleton<FileSystemState>(() => FileSystemState.Create(Resolve.WorkFolder.FileInfo.FileItemInfo("FileSystemState.xml")));
            TypeMap.Register.Singleton<IStatusChecker>(() => new FakeStatusChecker());
            TypeMap.Register.Singleton<IRandomGenerator>(() => new FakeRandomGenerator());
            TypeMap.Register.Singleton<CryptoFactory>(() => CreateCryptoFactory());
            TypeMap.Register.Singleton<ICryptoPolicy>(() => new ProCryptoPolicy());
            TypeMap.Register.Singleton<ActiveFileWatcher>(() => new ActiveFileWatcher());
            TypeMap.Register.Singleton<UserAsymmetricKeysStore>(() => new UserAsymmetricKeysStore(Resolve.WorkFolder.FileInfo, Resolve.KnownKeys));
            TypeMap.Register.Singleton<IAsymmetricFactory>(() => new BouncyCastleAsymmetricFactory());

            TypeMap.Register.New<AxCryptFactory>(() => new AxCryptFactory());
            TypeMap.Register.New<AxCryptFile>(() => new AxCryptFile());
            TypeMap.Register.New<ActiveFileAction>(() => new ActiveFileAction());
            TypeMap.Register.New<FileOperation>(() => new FileOperation(Resolve.FileSystemState, Resolve.SessionNotify));
            TypeMap.Register.New<IdentityViewModel>(() => new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownKeys, Resolve.UserSettings));
            TypeMap.Register.New<FileOperationViewModel>(() => new FileOperationViewModel(Resolve.FileSystemState, Resolve.SessionNotify, Resolve.KnownKeys, Resolve.ParallelFileOperation, TypeMap.Resolve.Singleton<IStatusChecker>(), TypeMap.Resolve.New<IdentityViewModel>()));
            TypeMap.Register.New<MainViewModel>(() => new MainViewModel(Resolve.FileSystemState));
            TypeMap.Register.New<string, IDataStore>((path) => new FakeDataStore(path));
            TypeMap.Register.New<string, IDataContainer>((path) => new FakeDataContainer(path));
            TypeMap.Register.New<string, IDataItem>((path) => CreateDataItem(path));
            TypeMap.Register.New<string, IFileWatcher>((path) => new FakeFileWatcher(path));
            TypeMap.Register.New<IterationCalculator>(() => new FakeIterationCalculator());
            TypeMap.Register.New<IDataProtection>(() => new FakeDataProtection());
            TypeMap.Register.New<IStringSerializer>(() => new StringSerializer(TypeMap.Resolve.Singleton<IAsymmetricFactory>().GetConverters()));
            TypeMap.Register.New<AxCryptHMACSHA1>(() => PortableFactory.AxCryptHMACSHA1());
            TypeMap.Register.New<HMACSHA512>(() => PortableFactory.HMACSHA512());
            TypeMap.Register.New<AesManaged>(() => PortableFactory.AesManaged());
            TypeMap.Register.New<CryptoStream>(() => PortableFactory.CryptoStream());
            TypeMap.Register.New<Sha1>(() => PortableFactory.SHA1Managed());
            TypeMap.Register.New<Sha256>(() => PortableFactory.SHA256Managed());

            Resolve.UserSettings.SetKeyWrapIterations(V1Aes128CryptoFactory.CryptoId, 1234);
            Resolve.UserSettings.ThumbprintSalt = Salt.Zero;
            Resolve.Log.SetLevel(LogLevel.Debug);
        }

        private static IDataItem CreateDataItem(string location)
        {
            if (location.EndsWith(Path.PathSeparator.ToString()))
            {
                return new FakeDataContainer(location);
            }
            return new FakeDataStore(location);
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
            FakeDataStore.ClearFiles();
            TypeMap.Register.Clear();
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