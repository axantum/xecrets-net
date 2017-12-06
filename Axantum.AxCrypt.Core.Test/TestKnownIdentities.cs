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

#endregion Coypright and License

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestKnownIdentities
    {
        private CryptoImplementation _cryptoImplementation;

        public TestKnownIdentities(CryptoImplementation cryptoImplementation)
        {
            _cryptoImplementation = cryptoImplementation;
        }

        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup();
            SetupAssembly.AssemblySetupCrypto(_cryptoImplementation);

            TypeMap.Register.Singleton<FileSystemState>(() => new FileSystemState());
        }

        [TearDown]
        public void Teardown()
        {
            TypeMap.Register.Clear();
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public async Task TestAddNewKnownKey()
        {
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            LogOnIdentity passphrase = new LogOnIdentity("a");
            await knownIdentities.AddAsync(passphrase);
            Assert.That(knownIdentities.Identities.First(), Is.EqualTo(passphrase), "The first and only key should be the one just added.");
        }

        [Test]
        public async Task TestAddTwoNewKnownIdentities()
        {
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            LogOnIdentity key1 = new LogOnIdentity("key1");
            await knownIdentities.AddAsync(key1);
            LogOnIdentity key2 = new LogOnIdentity("key2");
            await knownIdentities.AddAsync(key2);
            Assert.That(knownIdentities.Identities.First(), Is.EqualTo(key2), "The first key should be the last one added.");
            Assert.That(knownIdentities.Identities.Last(), Is.EqualTo(key1), "The last key should be the first one added.");
        }

        [Test]
        public async Task TestAddEmptyKey()
        {
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            LogOnIdentity key = new LogOnIdentity(String.Empty);
            await knownIdentities.AddAsync(key);
            await knownIdentities.AddAsync(key);
            Assert.That(knownIdentities.Identities.Count(), Is.EqualTo(0), "No key should be in the collection even if added twice, since it is empty.");
        }

        [Test]
        public async Task TestAddSameKeyTwice()
        {
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            LogOnIdentity key = new LogOnIdentity("abc");
            await knownIdentities.AddAsync(key);
            await knownIdentities.AddAsync(key);
            Assert.That(knownIdentities.Identities.Count(), Is.EqualTo(1), "Only one key should be in the collection even if added twice.");
            Assert.That(knownIdentities.Identities.First(), Is.EqualTo(key), "The first and only key should be the one just added.");
        }

        [Test]
        public async Task TestDefaultEncryptionKey()
        {
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            LogOnIdentity key = new LogOnIdentity("a");
            await knownIdentities.SetDefaultEncryptionIdentity(key);
            Assert.That(knownIdentities.DefaultEncryptionIdentity, Is.EqualTo(key), "The DefaultEncryptionKey should be the one just set as it.");
            Assert.That(knownIdentities.Identities.Count(), Is.EqualTo(1), "Only one key should be in the collection.");
            Assert.That(knownIdentities.Identities.First(), Is.EqualTo(key), "The first and only key should be the one just set as DefaultEncryptionKey.");
        }

        [Test]
        public async Task TestClear()
        {
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            LogOnIdentity key1 = new LogOnIdentity("key1");
            await knownIdentities.AddAsync(key1);
            LogOnIdentity key2 = new LogOnIdentity("key2");
            await knownIdentities.AddAsync(key2);
            Assert.That(knownIdentities.Identities.Count(), Is.EqualTo(2), "There should be two keys in the collection.");

            await knownIdentities.SetDefaultEncryptionIdentity(LogOnIdentity.Empty);
            Assert.That(knownIdentities.Identities.Count(), Is.EqualTo(0), "There should be zero keys in the collection after Clear().");

            await knownIdentities.SetDefaultEncryptionIdentity(LogOnIdentity.Empty);
            Assert.That(knownIdentities.Identities.Count(), Is.EqualTo(0), "There should be zero keys in the collection after Clear() with zero keys to start with.");
        }

        [Test]
        public async Task TestSettingNullDefaultEncryptionKey()
        {
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            LogOnIdentity key1 = new LogOnIdentity("a");
            await knownIdentities.AddAsync(key1);
            LogOnIdentity key2 = new LogOnIdentity("B");
            await knownIdentities.SetDefaultEncryptionIdentity(key2);

            Assert.That(knownIdentities.Identities.Count(), Is.EqualTo(2), "Setting the DefaultEncryptionKey should also add it as a known key.");

            await knownIdentities.SetDefaultEncryptionIdentity(LogOnIdentity.Empty);
            Assert.That(knownIdentities.Identities.Count(), Is.EqualTo(0), "Setting the DefaultEncryptionKey to empty should clear the known keys.");
        }

        [Test]
        public async Task TestChangedEventWhenAddingEmptyIdentity()
        {
            bool wasChanged = false;
            SessionNotify notificationMonitor = new SessionNotify();
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, notificationMonitor);
            notificationMonitor.AddCommand((SessionNotification notification) =>
            {
                wasChanged |= notification.NotificationType == SessionNotificationType.KnownKeyChange;
                return Constant.CompletedTask;
            });
            LogOnIdentity key1 = new LogOnIdentity(String.Empty);
            await knownIdentities.AddAsync(key1);
            Assert.That(wasChanged, Is.False, "A new key should not trigger the Changed event.");
        }

        [Test]
        public async Task TestChangedEvent()
        {
            bool wasChanged = false;
            SessionNotify notificationMonitor = new SessionNotify();
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, notificationMonitor);
            notificationMonitor.AddCommand((SessionNotification notification) =>
            {
                wasChanged |= notification.NotificationType == SessionNotificationType.KnownKeyChange;
                return Constant.CompletedTask;
            });
            LogOnIdentity key1 = new LogOnIdentity("abc");
            await knownIdentities.AddAsync(key1);
            Assert.That(wasChanged, Is.True, "A new key should trigger the Changed event.");
            wasChanged = false;
            await knownIdentities.AddAsync(key1);
            Assert.That(wasChanged, Is.False, "Re-adding an existing key should not trigger the Changed event.");
        }

        [Test]
        public async Task TestLoggingOffWhenLoggingOnWhenAlreadyLoggedOn()
        {
            int wasLoggedOnCount = 0;
            int wasLoggedOffCount = 0;
            SessionNotify notificationMonitor = new SessionNotify();
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, notificationMonitor);
            notificationMonitor.AddCommand((SessionNotification notification) =>
            {
                if (notification.NotificationType == SessionNotificationType.SignIn)
                {
                    Assert.That(knownIdentities.IsLoggedOn, Is.True, "The state of the IsLoggedOn property should be consistent with the event.");
                    ++wasLoggedOnCount;
                }
                if (notification.NotificationType == SessionNotificationType.SignOut)
                {
                    Assert.That(knownIdentities.IsLoggedOn, Is.False, "The state of the IsLoggedOn property should be consistent with the event.");
                    ++wasLoggedOffCount;
                }
                return Constant.CompletedTask;
            });

            await knownIdentities.SetDefaultEncryptionIdentity(new LogOnIdentity("passphrase1"));
            Assert.That(wasLoggedOnCount, Is.EqualTo(1));
            Assert.That(wasLoggedOffCount, Is.EqualTo(0));
            Assert.That(knownIdentities.IsLoggedOn, Is.True);

            await knownIdentities.SetDefaultEncryptionIdentity(new LogOnIdentity("passphrase"));
            Assert.That(wasLoggedOnCount, Is.EqualTo(2));
            Assert.That(wasLoggedOffCount, Is.EqualTo(1));
            Assert.That(knownIdentities.IsLoggedOn, Is.True);
        }

        [Test]
        public async Task TestAddKeyForKnownIdentity()
        {
            Resolve.FileSystemState.KnownPassphrases.Add(new Passphrase("a"));
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            await knownIdentities.AddAsync(new LogOnIdentity("a"));

            Assert.That(knownIdentities.DefaultEncryptionIdentity.Equals(LogOnIdentity.Empty), "When adding a key that is for a known identity it should not be set as the default.");
        }

        [Test]
        public async Task TestWatchedFoldersNotLoggedOn()
        {
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            FakeDataStore.AddFolder(@"C:\WatchedFolder\");
            await Resolve.FileSystemState.AddWatchedFolderAsync(new WatchedFolder(@"C:\WatchedFolder\", IdentityPublicTag.Empty));
            IEnumerable<WatchedFolder> watchedFolders = knownIdentities.LoggedOnWatchedFolders;

            Assert.That(watchedFolders.Count(), Is.EqualTo(0), "When not logged on, no watched folders should be known.");
        }

        [Test]
        public async Task TestWatchedFoldersWhenLoggedOn()
        {
            Passphrase key1 = new Passphrase("a");
            LogOnIdentity key2 = new LogOnIdentity("b");
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            FakeDataStore.AddFolder(@"C:\WatchedFolder1\");
            FakeDataStore.AddFolder(@"C:\WatchedFolder2\");
            await Resolve.FileSystemState.AddWatchedFolderAsync(new WatchedFolder(@"C:\WatchedFolder1\", new LogOnIdentity(key1).Tag));
            await Resolve.FileSystemState.AddWatchedFolderAsync(new WatchedFolder(@"C:\WatchedFolder2\", key2.Tag));
            await knownIdentities.SetDefaultEncryptionIdentity(key2);
            IEnumerable<WatchedFolder> watchedFolders = knownIdentities.LoggedOnWatchedFolders;

            Assert.That(watchedFolders.Count(), Is.EqualTo(1), "Only one of the two watched folders should be shown.");
            Assert.That(watchedFolders.First().Tag.Matches(key2.Tag), "The returned watched folder should be number 2.");
        }
    }
}