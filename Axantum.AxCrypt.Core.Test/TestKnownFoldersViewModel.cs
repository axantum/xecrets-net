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
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using NUnit.Framework;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestKnownFoldersViewModel
    {
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
        public void TestSettingKnownFoldersAndLoggingOnAndOff()
        {
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            KnownFoldersViewModel vm = new KnownFoldersViewModel(Resolve.FileSystemState, Resolve.SessionNotify, knownIdentities);

            Assert.That(vm.KnownFolders.Count(), Is.EqualTo(0));

            IDataContainer betterCloudInfo = New<IDataContainer>(@"C:\BetterCloud");
            IDataContainer fasterCloudInfo = New<IDataContainer>(@"C:\FasterCloud");
            KnownFolder folder1 = new KnownFolder(betterCloudInfo, @"My AxCrypt", new Bitmap(10, 10), null);
            KnownFolder folder2 = new KnownFolder(fasterCloudInfo, @"My AxCrypt", new Bitmap(10, 10), null);
            FakeDataStore.AddFolder(folder1.My.FullName);
            FakeDataStore.AddFolder(folder2.My.FullName);

            vm.KnownFolders = new KnownFolder[] { folder1, folder2 };
            Assert.That(vm.KnownFolders.Count(), Is.EqualTo(2));
            Assert.That(vm.KnownFolders.First().Enabled, Is.False);
            Assert.That(vm.KnownFolders.Last().Enabled, Is.False);

            knownIdentities.DefaultEncryptionIdentity = new LogOnIdentity("aaa");
            Assert.That(vm.KnownFolders.Count(), Is.EqualTo(2));
            Assert.That(vm.KnownFolders.First().Enabled, Is.True);
            Assert.That(vm.KnownFolders.Last().Enabled, Is.True);

            knownIdentities.DefaultEncryptionIdentity = LogOnIdentity.Empty;
            Assert.That(vm.KnownFolders.Count(), Is.EqualTo(2));
            Assert.That(vm.KnownFolders.First().Enabled, Is.False);
            Assert.That(vm.KnownFolders.Last().Enabled, Is.False);
        }

        [Test]
        public void TestAlreadyKnownFoldersAndLoggingOn()
        {
            IDataContainer betterCloudInfo = New<IDataContainer>(@"C:\BetterCloud");
            IDataContainer fasterCloudInfo = New<IDataContainer>(@"C:\FasterCloud");
            KnownFolder folder1 = new KnownFolder(betterCloudInfo, @"My AxCrypt", new Bitmap(10, 10), null);
            KnownFolder folder2 = new KnownFolder(fasterCloudInfo, @"My AxCrypt", new Bitmap(10, 10), null);
            FakeDataStore.AddFolder(folder1.My.FullName);
            FakeDataStore.AddFolder(folder2.My.FullName);

            Resolve.FileSystemState.AddWatchedFolder(new WatchedFolder(folder1.My.FullName, new LogOnIdentity("PassPhrase").Tag));
            Resolve.FileSystemState.AddWatchedFolder(new WatchedFolder(folder2.My.FullName, new LogOnIdentity(new Passphrase("aaa")).Tag));

            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            KnownFoldersViewModel vm = new KnownFoldersViewModel(Resolve.FileSystemState, Resolve.SessionNotify, knownIdentities);

            Assert.That(vm.KnownFolders.Count(), Is.EqualTo(0));

            vm.KnownFolders = new KnownFolder[] { folder1, folder2 };
            Assert.That(vm.KnownFolders.Count(), Is.EqualTo(2));
            Assert.That(vm.KnownFolders.First().Enabled, Is.False);
            Assert.That(vm.KnownFolders.Last().Enabled, Is.False);

            knownIdentities.DefaultEncryptionIdentity = new LogOnIdentity("aaa");
            Assert.That(vm.KnownFolders.Count(), Is.EqualTo(2));
            Assert.That(vm.KnownFolders.First().Enabled, Is.False);
            Assert.That(vm.KnownFolders.Last().Enabled, Is.True);
        }

        [Test]
        public void TestFileWasCreatedWhereAKnownFolderWasExpected()
        {
            IDataContainer betterCloudInfo = New<IDataContainer>(@"C:\BetterCloud");
            IDataContainer fasterCloudInfo = New<IDataContainer>(@"C:\FasterCloud");
            KnownFolder folder1 = new KnownFolder(betterCloudInfo, @"My AxCrypt", new Bitmap(10, 10), null);
            KnownFolder folder2 = new KnownFolder(fasterCloudInfo, @"My AxCrypt", new Bitmap(10, 10), null);
            FakeDataStore.AddFile(@"C:\BetterCloud\My AxCrypt", Stream.Null);
            FakeDataStore.AddFolder(folder2.My.FullName);

            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            KnownFoldersViewModel vm = new KnownFoldersViewModel(Resolve.FileSystemState, Resolve.SessionNotify, knownIdentities);

            Assert.That(vm.KnownFolders.Count(), Is.EqualTo(0));

            vm.KnownFolders = new KnownFolder[] { folder1, folder2 };
            Assert.That(vm.KnownFolders.Count(), Is.EqualTo(2));
            Assert.That(vm.KnownFolders.First().Enabled, Is.False);
            Assert.That(vm.KnownFolders.Last().Enabled, Is.False);

            knownIdentities.DefaultEncryptionIdentity = new LogOnIdentity("aaa");
            Assert.That(Resolve.FileSystemState.WatchedFolders.Count(), Is.EqualTo(1));
            Assert.That(vm.KnownFolders.Count(), Is.EqualTo(2));
            Assert.That(vm.KnownFolders.First().Enabled, Is.False);
            Assert.That(vm.KnownFolders.Last().Enabled, Is.True);
        }
    }
}