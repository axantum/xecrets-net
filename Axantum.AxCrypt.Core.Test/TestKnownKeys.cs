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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestKnownKeys
    {
        private CryptoImplementation _cryptoImplementation;

        public TestKnownKeys(CryptoImplementation cryptoImplementation)
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
            KnownKeys knownKeys = new KnownKeys(Resolve.FileSystemState, Resolve.SessionNotify);
            Passphrase passphrase = new Passphrase("a");
            knownKeys.Add(passphrase);
            Assert.That(knownKeys.Keys.First(), Is.EqualTo(passphrase), "The first and only key should be the one just added.");
        }

        [Test]
        public void TestAddTwoNewKnownKeys()
        {
            KnownKeys knownKeys = new KnownKeys(Resolve.FileSystemState, Resolve.SessionNotify);
            Passphrase key1 = new Passphrase("key1");
            knownKeys.Add(key1);
            Passphrase key2 = new Passphrase("key2");
            knownKeys.Add(key2);
            Assert.That(knownKeys.Keys.First(), Is.EqualTo(key2), "The first key should be the last one added.");
            Assert.That(knownKeys.Keys.Last(), Is.EqualTo(key1), "The last key should be the first one added.");
        }

        [Test]
        public void TestAddSameKeyTwice()
        {
            KnownKeys knownKeys = new KnownKeys(Resolve.FileSystemState, Resolve.SessionNotify);
            Passphrase key = new Passphrase(String.Empty);
            knownKeys.Add(key);
            knownKeys.Add(key);
            Assert.That(knownKeys.Keys.Count(), Is.EqualTo(1), "Only one key should be in the collection even if added twice.");
            Assert.That(knownKeys.Keys.First(), Is.EqualTo(key), "The first and only key should be the one just added.");
        }

        [Test]
        public void TestDefaultEncryptionKey()
        {
            KnownKeys knownKeys = new KnownKeys(Resolve.FileSystemState, Resolve.SessionNotify);
            Passphrase key = new Passphrase(String.Empty);
            knownKeys.DefaultEncryptionKey = key;
            Assert.That(knownKeys.DefaultEncryptionKey, Is.EqualTo(key), "The DefaultEncryptionKey should be the one just set as it.");
            Assert.That(knownKeys.Keys.Count(), Is.EqualTo(1), "Only one key should be in the collection.");
            Assert.That(knownKeys.Keys.First(), Is.EqualTo(key), "The first and only key should be the one just set as DefaultEncryptionKey.");
        }

        [Test]
        public void TestClear()
        {
            KnownKeys knownKeys = new KnownKeys(Resolve.FileSystemState, Resolve.SessionNotify);
            Passphrase key1 = new Passphrase("key1");
            knownKeys.Add(key1);
            Passphrase key2 = new Passphrase("key2");
            knownKeys.Add(key2);
            Assert.That(knownKeys.Keys.Count(), Is.EqualTo(2), "There should be two keys in the collection.");

            knownKeys.Clear();
            Assert.That(knownKeys.Keys.Count(), Is.EqualTo(0), "There should be zero keys in the collection after Clear().");

            knownKeys.Clear();
            Assert.That(knownKeys.Keys.Count(), Is.EqualTo(0), "There should be zero keys in the collection after Clear() with zero keys to start with.");
        }

        [Test]
        public void TestSettingNullDefaultEncryptionKey()
        {
            KnownKeys knownKeys = new KnownKeys(Resolve.FileSystemState, Resolve.SessionNotify);
            Passphrase key1 = new Passphrase("a");
            knownKeys.Add(key1);
            Passphrase key2 = new Passphrase("B");
            knownKeys.DefaultEncryptionKey = key2;

            Assert.That(knownKeys.Keys.Count(), Is.EqualTo(2), "Setting the DefaultEncryptionKey should also add it as a known key.");

            knownKeys.DefaultEncryptionKey = null;
            Assert.That(knownKeys.Keys.Count(), Is.EqualTo(2), "Setting the DefaultEncryptionKey to null should not affect the known keys.");
        }

        [Test]
        public void TestChangedEvent()
        {
            bool wasChanged = false;
            SessionNotify notificationMonitor = new SessionNotify();
            KnownKeys knownKeys = new KnownKeys(Resolve.FileSystemState, notificationMonitor);
            notificationMonitor.Notification += (object sender, SessionNotificationEventArgs e) =>
            {
                wasChanged |= e.Notification.NotificationType == SessionNotificationType.KnownKeyChange;
            };
            Passphrase key1 = Passphrase.Empty;
            knownKeys.Add(key1);
            Assert.That(wasChanged, Is.True, "A new key should trigger the Changed event.");
            wasChanged = false;
            knownKeys.Add(key1);
            Assert.That(wasChanged, Is.False, "Re-adding an existing key should not trigger the Changed event.");
        }

        [Test]
        public void TestLoggingOffWhenLoggingOnWhenAlreadyLoggedOn()
        {
            int wasLoggedOnCount = 0;
            int wasLoggedOffCount = 0;
            SessionNotify notificationMonitor = new SessionNotify();
            KnownKeys knownKeys = new KnownKeys(Resolve.FileSystemState, notificationMonitor);
            notificationMonitor.Notification += (object sender, SessionNotificationEventArgs e) =>
            {
                if (e.Notification.NotificationType == SessionNotificationType.LogOn)
                {
                    Assert.That(knownKeys.IsLoggedOn, Is.True, "The state of the IsLoggedOn property should be consistent with the event.");
                    ++wasLoggedOnCount;
                }
                if (e.Notification.NotificationType == SessionNotificationType.LogOff)
                {
                    Assert.That(knownKeys.IsLoggedOn, Is.False, "The state of the IsLoggedOn property should be consistent with the event.");
                    ++wasLoggedOffCount;
                }
            };

            knownKeys.DefaultEncryptionKey = new Passphrase("passphrase1");
            Assert.That(wasLoggedOnCount, Is.EqualTo(1));
            Assert.That(wasLoggedOffCount, Is.EqualTo(0));
            Assert.That(knownKeys.IsLoggedOn, Is.True);

            knownKeys.DefaultEncryptionKey = new Passphrase("passphrase");
            Assert.That(wasLoggedOnCount, Is.EqualTo(2));
            Assert.That(wasLoggedOffCount, Is.EqualTo(1));
            Assert.That(knownKeys.IsLoggedOn, Is.True);
        }

        [Test]
        public void TestAddKeyForKnownIdentity()
        {
            Resolve.FileSystemState.Identities.Add(new PassphraseIdentity(new Passphrase("a")));
            KnownKeys knownKeys = new KnownKeys(Resolve.FileSystemState, Resolve.SessionNotify);
            knownKeys.Add(new Passphrase("a"));

            Assert.That(knownKeys.DefaultEncryptionKey.Equals(new Passphrase("a")), "When adding a key that is for a known identity it should be set as the default.");
        }

        [Test]
        public void TestWatchedFoldersNotLoggedOn()
        {
            KnownKeys knownKeys = new KnownKeys(Resolve.FileSystemState, Resolve.SessionNotify);
            FakeDataStore.AddFolder(@"C:\WatchedFolder\");
            Resolve.FileSystemState.AddWatchedFolder(new WatchedFolder(@"C:\WatchedFolder\"));
            IEnumerable<WatchedFolder> watchedFolders = knownKeys.LoggedOnWatchedFolders;

            Assert.That(watchedFolders.Count(), Is.EqualTo(0), "When not logged on, no watched folders should be known.");
        }

        [Test]
        public void TestWatchedFoldersWhenLoggedOn()
        {
            Passphrase key1 = new Passphrase("a");
            Passphrase key2 = new Passphrase("b");
            KnownKeys knownKeys = new KnownKeys(Resolve.FileSystemState, Resolve.SessionNotify);
            FakeDataStore.AddFolder(@"C:\WatchedFolder1\");
            FakeDataStore.AddFolder(@"C:\WatchedFolder2\");
            Resolve.FileSystemState.AddWatchedFolder(new WatchedFolder(@"C:\WatchedFolder1\", key1.Thumbprint));
            Resolve.FileSystemState.AddWatchedFolder(new WatchedFolder(@"C:\WatchedFolder2\", key2.Thumbprint));
            knownKeys.DefaultEncryptionKey = key2;
            IEnumerable<WatchedFolder> watchedFolders = knownKeys.LoggedOnWatchedFolders;

            Assert.That(watchedFolders.Count(), Is.EqualTo(1), "Only one of the two watched folders should be shown.");
            Assert.That(watchedFolders.First().Thumbprint, Is.EqualTo(key2.Thumbprint), "The returned watched folder should be number 2.");
        }
    }
}