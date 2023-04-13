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
 * The source is maintained at http://bitbucket.org/AxCrypt.Desktop.Window-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.IO;
using AxCrypt.Core.Session;
using AxCrypt.Core.UI;
using AxCrypt.Core.UI.ViewModel;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestKnownFoldersViewModel
    {
        private class TestKnownImageProvider : IKnownFolderImageProvider
        {
            public static object Image { get; } = new byte[137];

            public object GetImage(KnownFolderKind folderKind)
            {
                return Image;
            }
        }

        private CryptoImplementation _cryptoImplementation;

        public TestKnownFoldersViewModel(CryptoImplementation cryptoImplementation)
        {
            _cryptoImplementation = cryptoImplementation;
        }

        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup();
            SetupAssembly.AssemblySetupCrypto(_cryptoImplementation);

            TypeMap.Register.Singleton<IKnownFolderImageProvider>(() => new TestKnownImageProvider());
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public void TestConstructor()
        {
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            KnownFoldersViewModel vm = new KnownFoldersViewModel(Resolve.FileSystemState, Resolve.SessionNotify, knownIdentities);

            Assert.That(vm.KnownFolders.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task TestSettingKnownFoldersAndLoggingOnAndOff()
        {
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            KnownFoldersViewModel vm = new KnownFoldersViewModel(Resolve.FileSystemState, Resolve.SessionNotify, knownIdentities);

            Assert.That(vm.KnownFolders.Count(), Is.EqualTo(0));

            IDataContainer betterCloudInfo = New<IDataContainer>(@"C:\BetterCloud");
            IDataContainer fasterCloudInfo = New<IDataContainer>(@"C:\FasterCloud");
            KnownFolder folder1 = new KnownFolder(betterCloudInfo, @"My AxCrypt", KnownFolderKind.WindowsMyDocuments, null);
            KnownFolder folder2 = new KnownFolder(fasterCloudInfo, @"My AxCrypt", KnownFolderKind.Dropbox, null);
            FakeDataStore.AddFolder(folder1.My.FullName);
            FakeDataStore.AddFolder(folder2.My.FullName);

            vm.KnownFolders = new KnownFolder[] { folder1, folder2 };
            Assert.That(vm.KnownFolders.Count(), Is.EqualTo(2));
            Assert.That(vm.KnownFolders.First().Enabled, Is.False, "We are not signed in so the folder should not be enabled");
            Assert.That(vm.KnownFolders.Last().Enabled, Is.False, "We are not signed in so the folder should not be enabled");

            await knownIdentities.SetDefaultEncryptionIdentity(new LogOnIdentity("aaa"));
            Assert.That(vm.KnownFolders.Count(), Is.EqualTo(2));
            Assert.That(vm.KnownFolders.First().Enabled, Is.True, "We are signed in so the folder should be enabled");
            Assert.That(vm.KnownFolders.Last().Enabled, Is.True, "We are signed in so the folder should be enabled");

            await knownIdentities.SetDefaultEncryptionIdentity(LogOnIdentity.Empty);
            Assert.That(vm.KnownFolders.Count(), Is.EqualTo(2));
            Assert.That(vm.KnownFolders.First().Enabled, Is.False, "We are no longer signed in so the folder should not be enabled");
            Assert.That(vm.KnownFolders.Last().Enabled, Is.False, "We are no longer signed in so the folder should not be enabled");
        }

        [Test]
        public async Task TestAlreadyKnownFoldersAndLoggingOn()
        {
            IDataContainer betterCloudInfo = New<IDataContainer>(@"C:\BetterCloud");
            IDataContainer fasterCloudInfo = New<IDataContainer>(@"C:\FasterCloud");
            KnownFolder folder1 = new KnownFolder(betterCloudInfo, @"My AxCrypt", KnownFolderKind.OneDrive, null);
            KnownFolder folder2 = new KnownFolder(fasterCloudInfo, @"My AxCrypt", KnownFolderKind.WindowsMyDocuments, null);
            FakeDataStore.AddFolder(folder1.My.FullName);
            FakeDataStore.AddFolder(folder2.My.FullName);

            await Resolve.FileSystemState.AddWatchedFolderAsync(new WatchedFolder(folder1.My.FullName, new LogOnIdentity("PassPhrase").Tag));
            await Resolve.FileSystemState.AddWatchedFolderAsync(new WatchedFolder(folder2.My.FullName, new LogOnIdentity(new Passphrase("aaa")).Tag));

            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            KnownFoldersViewModel vm = new KnownFoldersViewModel(Resolve.FileSystemState, Resolve.SessionNotify, knownIdentities);

            Assert.That(vm.KnownFolders.Count(), Is.EqualTo(0));

            vm.KnownFolders = new KnownFolder[] { folder1, folder2 };
            Assert.That(vm.KnownFolders.Count(), Is.EqualTo(2));
            Assert.That(vm.KnownFolders.First().Enabled, Is.False);
            Assert.That(vm.KnownFolders.Last().Enabled, Is.False);

            await knownIdentities.SetDefaultEncryptionIdentity(new LogOnIdentity("aaa"));
            Assert.That(vm.KnownFolders.Count(), Is.EqualTo(2));
            Assert.That(vm.KnownFolders.First().Enabled, Is.False, "This folder should not be enabled, because it's not watched by the signed in identity.");
            Assert.That(vm.KnownFolders.Last().Enabled, Is.True, "This folder should be enabled, since it is watched by the signed in identity.");
        }

        [Test]
        public async Task TestFileWasCreatedWhereAKnownFolderWasExpected()
        {
            IDataContainer betterCloudInfo = New<IDataContainer>(@"C:\BetterCloud");
            IDataContainer fasterCloudInfo = New<IDataContainer>(@"C:\FasterCloud");
            KnownFolder folder1 = new KnownFolder(betterCloudInfo, @"My AxCrypt", KnownFolderKind.GoogleDrive, null);
            KnownFolder folder2 = new KnownFolder(fasterCloudInfo, @"My AxCrypt", KnownFolderKind.Dropbox, null);
            FakeDataStore.AddFile(@"C:\BetterCloud\My AxCrypt", Stream.Null);
            FakeDataStore.AddFolder(folder2.My.FullName);

            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            KnownFoldersViewModel vm = new KnownFoldersViewModel(Resolve.FileSystemState, Resolve.SessionNotify, knownIdentities);

            Assert.That(vm.KnownFolders.Count(), Is.EqualTo(0));

            vm.KnownFolders = new KnownFolder[] { folder1, folder2 };
            Assert.That(vm.KnownFolders.Count(), Is.EqualTo(2));
            Assert.That(vm.KnownFolders.First().Enabled, Is.False);
            Assert.That(vm.KnownFolders.Last().Enabled, Is.False);

            await knownIdentities.SetDefaultEncryptionIdentity(new LogOnIdentity("aaa"));
            Assert.That(Resolve.FileSystemState.WatchedFolders.Count(), Is.EqualTo(1));
            Assert.That(vm.KnownFolders.Count(), Is.EqualTo(2));
            Assert.That(vm.KnownFolders.First().Enabled, Is.False);
            Assert.That(vm.KnownFolders.Last().Enabled, Is.True);
        }
    }
}
