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
        public void TestAddNewKnownKey()
        {
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            LogOnIdentity passphrase = new LogOnIdentity("a");
            knownIdentities.Add(passphrase);
            Assert.That(knownIdentities.Identities.First(), Is.EqualTo(passphrase), "The first and only key should be the one just added.");
        }

        [Test]
        public void TestAddTwoNewKnownIdentities()
        {
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            LogOnIdentity key1 = new LogOnIdentity("key1");
            knownIdentities.Add(key1);
            LogOnIdentity key2 = new LogOnIdentity("key2");
            knownIdentities.Add(key2);
            Assert.That(knownIdentities.Identities.First(), Is.EqualTo(key2), "The first key should be the last one added.");
            Assert.That(knownIdentities.Identities.Last(), Is.EqualTo(key1), "The last key should be the first one added.");
        }

        [Test]
        public void TestAddEmptyKey()
        {
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            LogOnIdentity key = new LogOnIdentity(String.Empty);
            knownIdentities.Add(key);
            knownIdentities.Add(key);
            Assert.That(knownIdentities.Identities.Count(), Is.EqualTo(0), "No key should be in the collection even if added twice, since it is empty.");
        }

        [Test]
        public void TestAddSameKeyTwice()
        {
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            LogOnIdentity key = new LogOnIdentity("abc");
            knownIdentities.Add(key);
            knownIdentities.Add(key);
            Assert.That(knownIdentities.Identities.Count(), Is.EqualTo(1), "Only one key should be in the collection even if added twice.");
            Assert.That(knownIdentities.Identities.First(), Is.EqualTo(key), "The first and only key should be the one just added.");
        }

        [Test]
        public void TestDefaultEncryptionKey()
        {
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            LogOnIdentity key = new LogOnIdentity("a");
            knownIdentities.DefaultEncryptionIdentity = key;
            Assert.That(knownIdentities.DefaultEncryptionIdentity, Is.EqualTo(key), "The DefaultEncryptionKey should be the one just set as it.");
            Assert.That(knownIdentities.Identities.Count(), Is.EqualTo(1), "Only one key should be in the collection.");
            Assert.That(knownIdentities.Identities.First(), Is.EqualTo(key), "The first and only key should be the one just set as DefaultEncryptionKey.");
        }

        [Test]
        public void TestClear()
        {
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            LogOnIdentity key1 = new LogOnIdentity("key1");
            knownIdentities.Add(key1);
            LogOnIdentity key2 = new LogOnIdentity("key2");
            knownIdentities.Add(key2);
            Assert.That(knownIdentities.Identities.Count(), Is.EqualTo(2), "There should be two keys in the collection.");

            knownIdentities.Clear();
            Assert.That(knownIdentities.Identities.Count(), Is.EqualTo(0), "There should be zero keys in the collection after Clear().");

            knownIdentities.Clear();
            Assert.That(knownIdentities.Identities.Count(), Is.EqualTo(0), "There should be zero keys in the collection after Clear() with zero keys to start with.");
        }

        [Test]
        public void TestSettingNullDefaultEncryptionKey()
        {
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            LogOnIdentity key1 = new LogOnIdentity("a");
            knownIdentities.Add(key1);
            LogOnIdentity key2 = new LogOnIdentity("B");
            knownIdentities.DefaultEncryptionIdentity = key2;

            Assert.That(knownIdentities.Identities.Count(), Is.EqualTo(2), "Setting the DefaultEncryptionKey should also add it as a known key.");

            knownIdentities.DefaultEncryptionIdentity = LogOnIdentity.Empty;
            Assert.That(knownIdentities.Identities.Count(), Is.EqualTo(2), "Setting the DefaultEncryptionKey to null should not affect the known keys.");
        }

        [Test]
        public void TestChangedEventWhenAddingEmptyIdentity()
        {
            bool wasChanged = false;
            SessionNotify notificationMonitor = new SessionNotify();
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, notificationMonitor);
            notificationMonitor.Notification += (object sender, SessionNotificationEventArgs e) =>
            {
                wasChanged |= e.Notification.NotificationType == SessionNotificationType.KnownKeyChange;
            };
            LogOnIdentity key1 = new LogOnIdentity(String.Empty);
            knownIdentities.Add(key1);
            Assert.That(wasChanged, Is.False, "A new key should not trigger the Changed event.");
        }

        [Test]
        public void TestChangedEvent()
        {
            bool wasChanged = false;
            SessionNotify notificationMonitor = new SessionNotify();
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, notificationMonitor);
            notificationMonitor.Notification += (object sender, SessionNotificationEventArgs e) =>
            {
                wasChanged |= e.Notification.NotificationType == SessionNotificationType.KnownKeyChange;
            };
            LogOnIdentity key1 = new LogOnIdentity("abc");
            knownIdentities.Add(key1);
            Assert.That(wasChanged, Is.True, "A new key should trigger the Changed event.");
            wasChanged = false;
            knownIdentities.Add(key1);
            Assert.That(wasChanged, Is.False, "Re-adding an existing key should not trigger the Changed event.");
        }

        [Test]
        public void TestLoggingOffWhenLoggingOnWhenAlreadyLoggedOn()
        {
            int wasLoggedOnCount = 0;
            int wasLoggedOffCount = 0;
            SessionNotify notificationMonitor = new SessionNotify();
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, notificationMonitor);
            notificationMonitor.Notification += (object sender, SessionNotificationEventArgs e) =>
            {
                if (e.Notification.NotificationType == SessionNotificationType.LogOn)
                {
                    Assert.That(knownIdentities.IsLoggedOn, Is.True, "The state of the IsLoggedOn property should be consistent with the event.");
                    ++wasLoggedOnCount;
                }
                if (e.Notification.NotificationType == SessionNotificationType.LogOff)
                {
                    Assert.That(knownIdentities.IsLoggedOn, Is.False, "The state of the IsLoggedOn property should be consistent with the event.");
                    ++wasLoggedOffCount;
                }
            };

            knownIdentities.DefaultEncryptionIdentity = new LogOnIdentity("passphrase1");
            Assert.That(wasLoggedOnCount, Is.EqualTo(1));
            Assert.That(wasLoggedOffCount, Is.EqualTo(0));
            Assert.That(knownIdentities.IsLoggedOn, Is.True);

            knownIdentities.DefaultEncryptionIdentity = new LogOnIdentity("passphrase");
            Assert.That(wasLoggedOnCount, Is.EqualTo(2));
            Assert.That(wasLoggedOffCount, Is.EqualTo(1));
            Assert.That(knownIdentities.IsLoggedOn, Is.True);
        }

        [Test]
        public void TestAddKeyForKnownIdentity()
        {
            Resolve.FileSystemState.KnownPassphrases.Add(new Passphrase("a"));
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            knownIdentities.Add(new LogOnIdentity("a"));

            Assert.That(knownIdentities.DefaultEncryptionIdentity.Equals(LogOnIdentity.Empty), "When adding a key that is for a known identity it should not be set as the default.");
        }

        [Test]
        public void TestWatchedFoldersNotLoggedOn()
        {
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            FakeDataStore.AddFolder(@"C:\WatchedFolder\");
            Resolve.FileSystemState.AddWatchedFolder(new WatchedFolder(@"C:\WatchedFolder\", IdentityPublicTag.Empty));
            IEnumerable<WatchedFolder> watchedFolders = knownIdentities.LoggedOnWatchedFolders;

            Assert.That(watchedFolders.Count(), Is.EqualTo(0), "When not logged on, no watched folders should be known.");
        }

        [Test]
        public void TestWatchedFoldersWhenLoggedOn()
        {
            Passphrase key1 = new Passphrase("a");
            LogOnIdentity key2 = new LogOnIdentity("b");
            KnownIdentities knownIdentities = new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify);
            FakeDataStore.AddFolder(@"C:\WatchedFolder1\");
            FakeDataStore.AddFolder(@"C:\WatchedFolder2\");
            Resolve.FileSystemState.AddWatchedFolder(new WatchedFolder(@"C:\WatchedFolder1\", new LogOnIdentity(key1).Tag));
            Resolve.FileSystemState.AddWatchedFolder(new WatchedFolder(@"C:\WatchedFolder2\", key2.Tag));
            knownIdentities.DefaultEncryptionIdentity = key2;
            IEnumerable<WatchedFolder> watchedFolders = knownIdentities.LoggedOnWatchedFolders;

            Assert.That(watchedFolders.Count(), Is.EqualTo(1), "Only one of the two watched folders should be shown.");
            Assert.That(watchedFolders.First().Tag.Matches(key2.Tag), "The returned watched folder should be number 2.");
        }
    }
}