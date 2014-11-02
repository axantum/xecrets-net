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
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI.ViewModel;
using NUnit.Framework;
using System;
using System.Linq;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestIdentityViewModel
    {
        private CryptoImplementation _cryptoImplementation;

        public TestIdentityViewModel(CryptoImplementation cryptoImplementation)
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
        public void TestLogOnExistingIdentity()
        {
            Passphrase passphrase = new Passphrase("p");

            PassphraseIdentity id = new PassphraseIdentity(passphrase);
            Resolve.FileSystemState.Identities.Add(id);

            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownKeys, Resolve.UserSettings);
            ivm.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "p";
            };

            ivm.LogOnLogOff.Execute(Guid.Empty);

            Assert.That(Resolve.KnownKeys.IsLoggedOn);
        }

        [Test]
        public void TestLogOnExistingIdentityWithCancel()
        {
            Passphrase passphrase = new Passphrase("p");

            PassphraseIdentity id = new PassphraseIdentity(passphrase);
            Resolve.FileSystemState.Identities.Add(id);

            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownKeys, Resolve.UserSettings);
            ivm.LoggingOn += (sender, e) =>
            {
                e.Cancel = true;
            };

            ivm.LogOnLogOff.Execute(Guid.Empty);

            Assert.That(ivm.Passphrase, Is.Null);
            Assert.That(Resolve.KnownKeys.IsLoggedOn, Is.False);
        }

        [Test]
        public void TestLogOnLogOffWhenLoggedOn()
        {
            Passphrase passphrase = new Passphrase("p");

            PassphraseIdentity id = new PassphraseIdentity(passphrase);
            Resolve.FileSystemState.Identities.Add(id);
            Resolve.KnownKeys.DefaultEncryptionKey = id.Key;

            bool wasLoggingOn = false;
            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownKeys, Resolve.UserSettings);
            ivm.LoggingOn += (sender, e) =>
            {
                wasLoggingOn = true;
            };

            Assert.That(Resolve.KnownKeys.IsLoggedOn, Is.True);

            ivm.LogOnLogOff.Execute(Guid.Empty);

            Assert.That(wasLoggingOn, Is.False);
            Assert.That(Resolve.KnownKeys.IsLoggedOn, Is.False);
        }

        [Test]
        public void TestLogOnLogOffWhenLoggedOffAndNoIdentities()
        {
            bool wasCreateNew = false;
            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownKeys, Resolve.UserSettings);
            ivm.LoggingOn += (sender, e) =>
            {
                wasCreateNew = e.CreateNew;
                e.Passphrase = "ccc";
                e.Name = "New User Passphrase";
            };

            ivm.LogOnLogOff.Execute(Guid.Empty);

            Assert.That(wasCreateNew, Is.True, "Logging on event should be with Create New set.");
            Assert.That(Resolve.KnownKeys.IsLoggedOn, Is.True, "Should be logged on.");
            Assert.That(Resolve.KnownKeys.DefaultEncryptionKey.Thumbprint, Is.EqualTo(new Passphrase("ccc").Thumbprint));
            Assert.That(Resolve.FileSystemState.Identities.Count(), Is.EqualTo(1));
        }

        [Test]
        public void TestLogOnLogOffWhenLoggedOffAndNoIdentitiesWithCancel()
        {
            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownKeys, Resolve.UserSettings);
            ivm.LoggingOn += (sender, e) =>
            {
                e.Cancel = true;
            };

            ivm.Passphrase = new Passphrase("testing");
            ivm.LogOnLogOff.Execute(Guid.Empty);

            Assert.That(Resolve.KnownKeys.IsLoggedOn, Is.False, "Not logged on.");
            Assert.That(Resolve.FileSystemState.Identities.Count(), Is.EqualTo(0));
            Assert.That(ivm.Passphrase, Is.Null);
        }

        [Test]
        public void AskForLogOnOrDecryptPassphraseActionNotActiveFile()
        {
            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownKeys, Resolve.UserSettings);
            ivm.CryptoId = new V1Aes128CryptoFactory().Id; ;
            PassphraseIdentity id = null;
            ivm.LoggingOn += (sender, e) =>
            {
                id = e.Identity;
                e.Passphrase = "p";
            };

            ivm.AskForDecryptPassphrase.Execute(@"C:\Folder\File1-txt.axx");

            Assert.That(ivm.Passphrase.Text, Is.EqualTo("p"));
            Assert.That(id.Thumbprint, Is.EqualTo(PassphraseIdentity.Empty.Thumbprint));
        }

        [Test]
        public void AskForLogOnOrDecryptPassphraseActionActiveFile()
        {
            Passphrase key = new Passphrase("p");

            ActiveFile activeFile = new ActiveFile(TypeMap.Resolve.New<IDataStore>(@"C:\Folder\File1-txt.axx"), TypeMap.Resolve.New<IDataStore>(@"C:\Folder\File1.txt"), key, ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().Id);
            Resolve.FileSystemState.Add(activeFile);

            PassphraseIdentity id = null;
            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownKeys, Resolve.UserSettings);
            ivm.CryptoId = new V1Aes128CryptoFactory().Id;
            ivm.LoggingOn += (sender, e) =>
            {
                id = e.Identity;
                e.Passphrase = "p";
            };

            ivm.AskForDecryptPassphrase.Execute(@"C:\Folder\File1-txt.axx");

            Assert.That(ivm.Passphrase.Text, Is.EqualTo("p"));
            Assert.That(id.Thumbprint, Is.EqualTo(PassphraseIdentity.Empty.Thumbprint));
        }

        [Test]
        public void AskForLogOnOrDecryptPassphraseActionActiveFileWithExistingIdentity()
        {
            Passphrase key = new Passphrase("p");

            ActiveFile activeFile = new ActiveFile(TypeMap.Resolve.New<IDataStore>(@"C:\Folder\File1-txt.axx"), TypeMap.Resolve.New<IDataStore>(@"C:\Folder\File1.txt"), key, ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().Id);
            Resolve.FileSystemState.Add(activeFile);

            PassphraseIdentity id = new PassphraseIdentity(key);
            Resolve.FileSystemState.Identities.Add(id);

            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownKeys, Resolve.UserSettings);
            ivm.CryptoId = new V1Aes128CryptoFactory().Id;
            ivm.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "p";
            };

            ivm.AskForDecryptPassphrase.Execute(@"C:\Folder\File1-txt.axx");

            Assert.That(ivm.Passphrase.Text, Is.EqualTo("p"));
        }

        [Test]
        public void AskForLogOnPassphraseAction()
        {
            Passphrase key = new Passphrase("ppp");

            PassphraseIdentity id = new PassphraseIdentity(key);

            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownKeys, Resolve.UserSettings);
            ivm.CryptoId = new V1Aes128CryptoFactory().Id;
            ivm.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "ppp";
            };

            ivm.AskForLogOnPassphrase.Execute(id);

            Assert.That(ivm.Passphrase.Text, Is.EqualTo("ppp"));
            Assert.That(Resolve.KnownKeys.DefaultEncryptionKey.Thumbprint, Is.EqualTo(id.Thumbprint));
        }

        [Test]
        public void AskForLogOnPassphraseActionWithCancel()
        {
            Passphrase key = new Passphrase("ppp");

            PassphraseIdentity id = new PassphraseIdentity(key);

            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownKeys, Resolve.UserSettings);
            ivm.LoggingOn += (sender, e) =>
            {
                e.Cancel = true;
            };

            ivm.AskForLogOnPassphrase.Execute(id);

            Assert.That(ivm.Passphrase, Is.Null);
            Assert.That(Resolve.KnownKeys.DefaultEncryptionKey, Is.Null);
        }

        [Test]
        public void AskForNewLogOnPassphrase()
        {
            string defaultPassphrase = null;
            Resolve.FileSystemState.Identities.Add(new PassphraseIdentity());
            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownKeys, Resolve.UserSettings);
            ivm.CryptoId = new V1Aes128CryptoFactory().Id;
            ivm.LoggingOn += (sender, e) =>
            {
                if (!e.CreateNew)
                {
                    e.CreateNew = true;
                    e.Passphrase = "xxx";
                    return;
                }
                defaultPassphrase = e.Passphrase;
                e.Passphrase = "aaa";
            };

            ivm.AskForLogOnPassphrase.Execute(null);

            Assert.That(defaultPassphrase, Is.EqualTo("xxx"));
            Assert.That(ivm.Passphrase.Text, Is.EqualTo("aaa"));
            Assert.That(Resolve.KnownKeys.DefaultEncryptionKey.Thumbprint, Is.EqualTo(new Passphrase("aaa").Thumbprint));

            PassphraseIdentity id = Resolve.FileSystemState.Identities.FirstOrDefault(i => i.Thumbprint == new Passphrase("aaa").Thumbprint);
            Assert.That(id.Thumbprint, Is.EqualTo(Resolve.KnownKeys.DefaultEncryptionKey.Thumbprint));
        }

        [Test]
        public void AskForNewLogOnPassphraseAutomaticallyBecauseNoIdentitiesExists()
        {
            string defaultPassphrase = null;
            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownKeys, Resolve.UserSettings);
            ivm.CryptoId = new V1Aes128CryptoFactory().Id;
            ivm.LoggingOn += (sender, e) =>
            {
                if (e.CreateNew)
                {
                    defaultPassphrase = e.Passphrase;
                    e.Passphrase = "aaa";
                    e.Name = "New User Passphrase";
                }
            };

            ivm.AskForLogOnPassphrase.Execute(null);

            Assert.That(defaultPassphrase, Is.EqualTo(String.Empty));
            Assert.That(ivm.Passphrase.Text, Is.EqualTo("aaa"));
            Assert.That(Resolve.KnownKeys.DefaultEncryptionKey.Thumbprint, Is.EqualTo(new Passphrase("aaa").Thumbprint));

            PassphraseIdentity id = Resolve.FileSystemState.Identities.FirstOrDefault(i => i.Thumbprint == new Passphrase("aaa").Thumbprint);
            Assert.That(id.Thumbprint, Is.EqualTo(Resolve.KnownKeys.DefaultEncryptionKey.Thumbprint));
        }

        [Test]
        public void AskForNewLogOnPassphraseWithCancel()
        {
            string defaultPassphrase = null;
            Resolve.FileSystemState.Identities.Add(new PassphraseIdentity());
            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownKeys, Resolve.UserSettings);
            ivm.LoggingOn += (sender, e) =>
            {
                if (!e.CreateNew)
                {
                    e.CreateNew = true;
                    e.Passphrase = "xxx";
                    return;
                }
                defaultPassphrase = e.Passphrase;
                e.Cancel = true;
            };

            ivm.AskForLogOnPassphrase.Execute(null);

            Assert.That(defaultPassphrase, Is.EqualTo("xxx"));
            Assert.That(Resolve.KnownKeys.DefaultEncryptionKey, Is.Null);
            Assert.That(ivm.Passphrase, Is.Null);

            PassphraseIdentity id = Resolve.FileSystemState.Identities.FirstOrDefault(i => i.Thumbprint == new Passphrase("xxx").Thumbprint);
            Assert.That(id, Is.Null);
        }

        [Test]
        public void AskForNewLogOnPassphraseWithKnownIdentity()
        {
            Passphrase passphrase = new Passphrase("aaa");
            PassphraseIdentity id = new PassphraseIdentity(passphrase);
            Resolve.FileSystemState.Identities.Add(id);

            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownKeys, Resolve.UserSettings);
            ivm.LoggingOn += (sender, e) =>
            {
                if (!e.CreateNew)
                {
                    e.CreateNew = true;
                }
                e.Passphrase = "aaa";
            };

            ivm.AskForLogOnPassphrase.Execute(null);

            Assert.That(ivm.Passphrase.Text, Is.EqualTo("aaa"));
            Assert.That(Resolve.KnownKeys.DefaultEncryptionKey.Thumbprint, Is.EqualTo(passphrase.Thumbprint));
            Assert.That(Resolve.FileSystemState.Identities.Count(), Is.EqualTo(1));
        }

        [Test]
        public void AskForLogOnPassphraseWithKnownIdentity()
        {
            Passphrase passphrase = new Passphrase("aaa");
            PassphraseIdentity id = new PassphraseIdentity(passphrase);
            Resolve.FileSystemState.Identities.Add(id);

            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownKeys, Resolve.UserSettings);
            ivm.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "aaa";
            };

            ivm.AskForLogOnPassphrase.Execute(null);

            Assert.That(ivm.Passphrase.Text, Is.EqualTo("aaa"));
            Assert.That(Resolve.KnownKeys.DefaultEncryptionKey.Thumbprint, Is.EqualTo(passphrase.Thumbprint));
            Assert.That(Resolve.FileSystemState.Identities.Count(), Is.EqualTo(1));
        }

        [Test]
        public void AskForLogOnPassphraseWithKnownIdentityButWrongPassphraseEntered()
        {
            Passphrase passphrase = new Passphrase("aaa");
            PassphraseIdentity id = new PassphraseIdentity(passphrase);
            Resolve.FileSystemState.Identities.Add(id);

            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownKeys, Resolve.UserSettings);
            ivm.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "bbb";
            };

            ivm.AskForLogOnPassphrase.Execute(null);

            Assert.That(ivm.Passphrase, Is.Null);
            Assert.That(Resolve.FileSystemState.Identities.Count(), Is.EqualTo(1));
        }
    }
}