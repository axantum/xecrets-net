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
using Axantum.AxCrypt.Core.Algorithm.Implementation;
using Axantum.AxCrypt.Mono;
using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Api.Implementation;
using Axantum.AxCrypt.Core.Service;

using static Axantum.AxCrypt.Abstractions.TypeResolve;
using Axantum.AxCrypt.Fake;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Api;
using System.Reflection;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Abstractions.Algorithm;

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
            IDataStore knownPublicKeysStore = new FakeInMemoryDataStoreItem("knownpublickeys.txt");

            TypeMap.Register.Singleton<INow>(() => new FakeNow());
            TypeMap.Register.Singleton<IReport>(() => new FakeReport());
            TypeMap.Register.Singleton<IPortableFactory>(() => new PortableFactory());
            TypeMap.Register.Singleton<WorkFolder>(() => new WorkFolder(Path.GetPathRoot(Environment.CurrentDirectory) + @"WorkFolder\"));
            TypeMap.Register.Singleton<IRuntimeEnvironment>(() => new FakeRuntimeEnvironment());
            TypeMap.Register.Singleton<ILogging>(() => new FakeLogging());
            TypeMap.Register.Singleton<ISettingsStore>(() => new SettingsStore(null));
            TypeMap.Register.Singleton<UserSettingsVersion>(() => new UserSettingsVersion());
            TypeMap.Register.Singleton<UserSettings>(() => (new FakeUserSettings(New<IterationCalculator>())).Initialize());
            TypeMap.Register.Singleton<KnownIdentities>(() => new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify));
            TypeMap.Register.Singleton<ProcessState>(() => new ProcessState());
            TypeMap.Register.Singleton<IUIThread>(() => new FakeUIThread());
            TypeMap.Register.Singleton<IProgressBackground>(() => new FakeProgressBackground());
            TypeMap.Register.Singleton<SessionNotify>(() => new SessionNotify());
            TypeMap.Register.Singleton<FileSystemState>(() => FileSystemState.Create(Resolve.WorkFolder.FileInfo.FileItemInfo("FileSystemState.txt")));
            TypeMap.Register.Singleton<IStatusChecker>(() => new FakeStatusChecker());
            TypeMap.Register.Singleton<IRandomGenerator>(() => new FakeRandomGenerator());
            TypeMap.Register.Singleton<CryptoFactory>(() => CreateCryptoFactory());
            TypeMap.Register.Singleton<ActiveFileWatcher>(() => new ActiveFileWatcher());
            TypeMap.Register.Singleton<IAsymmetricFactory>(() => new BouncyCastleAsymmetricFactory());
            TypeMap.Register.Singleton<IEmailParser>(() => new EmailParser());
            TypeMap.Register.Singleton<ICache>(() => new FakeCache());
            TypeMap.Register.Singleton<AxCryptOnlineState>(() => new AxCryptOnlineState());
            TypeMap.Register.Singleton<CryptoPolicy>(() => new CryptoPolicy(new Assembly[0]));
            TypeMap.Register.Singleton<IVersion>(() => new FakeVersion());
            TypeMap.Register.Singleton<IInternetState>(() => new FakeInternetState());
            TypeMap.Register.Singleton<ICryptoPolicy>(() => New<LicensePolicy>().Capabilities.CryptoPolicy);
            TypeMap.Register.Singleton<LicensePolicy>(() => new PremiumForcedLicensePolicy());
            TypeMap.Register.Singleton<InactivitySignOut>(() => new InactivitySignOut(TimeSpan.Zero));
            TypeMap.Register.Singleton<FileLocker>(() => new FileLocker());
            TypeMap.Register.Singleton<FileFilter>(() => new FileFilter());
            TypeMap.Register.Singleton<IVerifySignInPassword>(() => new FakeVerifySignInPassword());
            TypeMap.Register.Singleton<IPopup>(() => new FakePopup());
            TypeMap.Register.Singleton<IKnownFoldersDiscovery>(() => new FakeKnownFoldersDiscovery());
            TypeMap.Register.Singleton<IGlobalNotification>(() => new FakeGlobalNotification());
            TypeMap.Register.Singleton<CanOpenEncryptedFile>(() => new CanOpenEncryptedFile());

            TypeMap.Register.New<AxCryptFactory>(() => new AxCryptFactory());
            TypeMap.Register.New<AxCryptFile>(() => new AxCryptFile());
            TypeMap.Register.New<ActiveFileAction>(() => new ActiveFileAction());
            TypeMap.Register.New<FileOperation>(() => new FileOperation(Resolve.FileSystemState, Resolve.SessionNotify));
            TypeMap.Register.New<IdentityViewModel>(() => new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify));
            TypeMap.Register.New<FileOperationViewModel>(() => new FileOperationViewModel(Resolve.FileSystemState, Resolve.SessionNotify, Resolve.KnownIdentities, Resolve.ParallelFileOperation, New<IStatusChecker>(), New<IdentityViewModel>()));
            TypeMap.Register.New<KnownPublicKeys>(() => KnownPublicKeys.Load(knownPublicKeysStore, New<IStringSerializer>()));
            TypeMap.Register.New<MainViewModel>(() => new MainViewModel(Resolve.FileSystemState, Resolve.UserSettings));
            TypeMap.Register.New<string, IDataStore>((path) => new FakeDataStore(path));
            TypeMap.Register.New<string, IDataContainer>((path) => new FakeDataContainer(path));
            TypeMap.Register.New<string, IDataItem>((path) => CreateDataItem(path));
            TypeMap.Register.New<string, IFileWatcher>((path) => new FakeFileWatcher(path));
            TypeMap.Register.New<IterationCalculator>(() => new FakeIterationCalculator());
            TypeMap.Register.New<IProtectedData>(() => new FakeDataProtection());
            TypeMap.Register.New<IStringSerializer>(() => new StringSerializer(New<IAsymmetricFactory>().GetSerializers()));
            TypeMap.Register.New<LogOnIdentity, IAccountService>((LogOnIdentity identity) => new DeviceAccountService(new LocalAccountService(identity, Resolve.WorkFolder.FileInfo), new NullAccountService(identity)));
            TypeMap.Register.New<ISystemCryptoPolicy>(() => new ProCryptoPolicy());
            TypeMap.Register.New<GlobalApiClient>(() => new GlobalApiClient(Resolve.UserSettings.RestApiBaseUrl, Resolve.UserSettings.ApiTimeout));
            TypeMap.Register.New<AxCryptApiClient>(() => new AxCryptApiClient(Resolve.KnownIdentities.DefaultEncryptionIdentity.ToRestIdentity(), Resolve.UserSettings.RestApiBaseUrl, Resolve.UserSettings.ApiTimeout));
            TypeMap.Register.New<AxCryptUpdateCheck>(() => new AxCryptUpdateCheck(New<IVersion>().Current));
            TypeMap.Register.New<ISingleThread>(() => new SingleThread());

            Resolve.UserSettings.SetKeyWrapIterations(new V1Aes128CryptoFactory().CryptoId, 1234);
            Resolve.UserSettings.ThumbprintSalt = Salt.Zero;
            Resolve.Log.SetLevel(LogLevel.Debug);
        }

        public static void AssemblySetupCrypto(CryptoImplementation cryptoImplementation)
        {
            switch (cryptoImplementation)
            {
                case CryptoImplementation.Mono:
                    TypeMap.Register.New<AxCryptHMACSHA1>(() => PortableFactory.AxCryptHMACSHA1());
                    TypeMap.Register.New<HMACSHA512>(() => PortableFactory.HMACSHA512());
                    TypeMap.Register.New<Aes>(() => PortableFactory.AesManaged());
                    TypeMap.Register.New<CryptoStreamBase>(() => PortableFactory.CryptoStream());
                    TypeMap.Register.New<Sha1>(() => PortableFactory.SHA1Managed());
                    TypeMap.Register.New<Sha256>(() => PortableFactory.SHA256Managed());
                    break;

                case CryptoImplementation.WindowsDesktop:
                    TypeMap.Register.New<AxCryptHMACSHA1>(() => PortableFactory.AxCryptHMACSHA1());
                    TypeMap.Register.New<HMACSHA512>(() => new Mono.Cryptography.HMACSHA512Wrapper(new Axantum.AxCrypt.Desktop.Cryptography.HMACSHA512CryptoServiceProvider()));
                    TypeMap.Register.New<Aes>(() => new Mono.Cryptography.AesWrapper(new System.Security.Cryptography.AesCryptoServiceProvider()));
                    TypeMap.Register.New<CryptoStreamBase>(() => PortableFactory.CryptoStream());
                    TypeMap.Register.New<Sha1>(() => PortableFactory.SHA1Managed());
                    TypeMap.Register.New<Sha256>(() => PortableFactory.SHA256Managed());
                    break;

                case CryptoImplementation.BouncyCastle:
                    TypeMap.Register.New<AxCryptHMACSHA1>(() => BouncyCastleCryptoFactory.AxCryptHMACSHA1());
                    TypeMap.Register.New<HMACSHA512>(() => BouncyCastleCryptoFactory.HMACSHA512());
                    TypeMap.Register.New<Aes>(() => BouncyCastleCryptoFactory.Aes());
                    TypeMap.Register.New<CryptoStreamBase>(() => BouncyCastleCryptoFactory.CryptoStream());
                    TypeMap.Register.New<Sha1>(() => BouncyCastleCryptoFactory.Sha1());
                    TypeMap.Register.New<Sha256>(() => BouncyCastleCryptoFactory.Sha256());
                    break;
            }
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