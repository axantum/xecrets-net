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
using Axantum.AxCrypt.Core.UI.ViewModel;
using NUnit.Framework;
using System;
using System.Linq;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
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

            Passphrase id = passphrase;
            Resolve.FileSystemState.KnownPassphrases.Add(id);

            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "p";
            };

            ivm.LogOnLogOff.Execute(Guid.Empty);

            Assert.That(Resolve.KnownIdentities.IsLoggedOn);
        }

        [Test]
        public void TestLogOnExistingIdentityWithCancel()
        {
            Passphrase passphrase = new Passphrase("p");

            Passphrase id = passphrase;
            Resolve.FileSystemState.KnownPassphrases.Add(id);

            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.LoggingOn += (sender, e) =>
            {
                e.Cancel = true;
            };

            ivm.LogOnLogOff.Execute(Guid.Empty);

            Assert.That(ivm.LogOnIdentity == LogOnIdentity.Empty);
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.False);
        }

        [Test]
        public void TestLogOnLogOffWhenLoggedOn()
        {
            LogOnIdentity passphrase = new LogOnIdentity("p");

            LogOnIdentity id = passphrase;
            Resolve.FileSystemState.KnownPassphrases.Add(id.Passphrase);
            Resolve.KnownIdentities.DefaultEncryptionIdentity = id;

            bool wasLoggingOn = false;
            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.LoggingOn += (sender, e) =>
            {
                wasLoggingOn = true;
            };

            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.True);

            ivm.LogOnLogOff.Execute(Guid.Empty);

            Assert.That(wasLoggingOn, Is.False);
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.False);
        }

        [Test]
        public void TestLogOnLogOffWhenLoggedOffAndNoIdentities()
        {
            bool wasCreateNew = false;
            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.LoggingOn += (sender, e) =>
            {
                wasCreateNew = e.IsAskingForPreviouslyUnknownPassphrase;
                e.Passphrase = "ccc";
                e.Name = "New User Passphrase";
            };

            ivm.LogOnLogOff.Execute(Guid.Empty);

            Assert.That(wasCreateNew, Is.True, "Logging on event should be with Create New set.");
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.True, "Should be logged on.");
            Assert.That(Resolve.KnownIdentities.DefaultEncryptionIdentity.Passphrase.Thumbprint, Is.EqualTo(new Passphrase("ccc").Thumbprint));
            Assert.That(Resolve.FileSystemState.KnownPassphrases.Count(), Is.EqualTo(1));
        }

        [Test]
        public void TestLogOnLogOffWhenLoggedOffAndNoIdentitiesWithCancel()
        {
            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.LoggingOn += (sender, e) =>
            {
                e.Cancel = true;
            };

            ivm.LogOnIdentity = new LogOnIdentity("testing");
            ivm.LogOnLogOff.Execute(Guid.Empty);

            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.False, "Not logged on.");
            Assert.That(Resolve.FileSystemState.KnownPassphrases.Count(), Is.EqualTo(0));
            Assert.That(ivm.LogOnIdentity == LogOnIdentity.Empty);
        }

        [Test]
        public void AskForLogOnOrDecryptPassphraseActionNotActiveFile()
        {
            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.CryptoId = new V1Aes128CryptoFactory().Id; ;
            LogOnIdentity id = null;
            ivm.LoggingOn += (sender, e) =>
            {
                id = e.Identity;
                e.Passphrase = "p";
            };

            ivm.AskForDecryptPassphrase.Execute(@"C:\Folder\File1-txt.axx");

            Assert.That(ivm.LogOnIdentity.Passphrase.Text, Is.EqualTo("p"));
            Assert.That(id.Passphrase.Thumbprint, Is.EqualTo(Passphrase.Empty.Thumbprint));
        }

        [Test]
        public void AskForLogOnOrDecryptPassphraseActionActiveFile()
        {
            LogOnIdentity key = new LogOnIdentity("p");

            ActiveFile activeFile = new ActiveFile(TypeMap.Resolve.New<IDataStore>(@"C:\Folder\File1-txt.axx"), TypeMap.Resolve.New<IDataStore>(@"C:\Folder\File1.txt"), key, ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().Id);
            Resolve.FileSystemState.Add(activeFile);

            LogOnIdentity id = null;
            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.CryptoId = new V1Aes128CryptoFactory().Id;
            ivm.LoggingOn += (sender, e) =>
            {
                id = e.Identity;
                e.Passphrase = "p";
            };

            ivm.AskForDecryptPassphrase.Execute(@"C:\Folder\File1-txt.axx");

            Assert.That(ivm.LogOnIdentity.Passphrase.Text, Is.EqualTo("p"));
            Assert.That(id.Passphrase.Thumbprint, Is.EqualTo(Passphrase.Empty.Thumbprint));
        }

        [Test]
        public void AskForLogOnOrDecryptPassphraseActionActiveFileWithExistingIdentity()
        {
            LogOnIdentity key = new LogOnIdentity("p");

            ActiveFile activeFile = new ActiveFile(TypeMap.Resolve.New<IDataStore>(@"C:\Folder\File1-txt.axx"), TypeMap.Resolve.New<IDataStore>(@"C:\Folder\File1.txt"), key, ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().Id);
            Resolve.FileSystemState.Add(activeFile);

            LogOnIdentity id = key;
            Resolve.FileSystemState.KnownPassphrases.Add(id.Passphrase);

            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.CryptoId = new V1Aes128CryptoFactory().Id;
            ivm.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "p";
            };

            ivm.AskForDecryptPassphrase.Execute(@"C:\Folder\File1-txt.axx");

            Assert.That(ivm.LogOnIdentity.Passphrase.Text, Is.EqualTo("p"));
        }

        [Test]
        public void AskForLogOnPassphraseAction()
        {
            LogOnIdentity key = new LogOnIdentity("ppp");

            LogOnIdentity id = key;

            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.CryptoId = new V1Aes128CryptoFactory().Id;
            ivm.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "ppp";
                e.IsAskingForPreviouslyUnknownPassphrase = true;
            };

            ivm.AskForLogOnPassphrase.Execute(id);

            Assert.That(ivm.LogOnIdentity.Passphrase.Text, Is.EqualTo("ppp"));
            Assert.That(Resolve.KnownIdentities.DefaultEncryptionIdentity.Passphrase.Thumbprint, Is.EqualTo(id.Passphrase.Thumbprint));
        }

        [Test]
        public void AskForLogOnPassphraseActionWithCancel()
        {
            LogOnIdentity key = new LogOnIdentity("ppp");

            LogOnIdentity id = key;

            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.LoggingOn += (sender, e) =>
            {
                e.Cancel = true;
            };

            ivm.AskForLogOnPassphrase.Execute(id);

            Assert.That(ivm.LogOnIdentity == LogOnIdentity.Empty);
            Assert.That(Resolve.KnownIdentities.DefaultEncryptionIdentity == LogOnIdentity.Empty);
        }

        [Test]
        public void AskForNewLogOnPassphrase()
        {
            string defaultPassphrase = null;
            Resolve.FileSystemState.KnownPassphrases.Add(new Passphrase());
            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.CryptoId = new V1Aes128CryptoFactory().Id;
            ivm.LoggingOn += (sender, e) =>
            {
                if (!e.IsAskingForPreviouslyUnknownPassphrase)
                {
                    e.IsAskingForPreviouslyUnknownPassphrase = true;
                    e.Passphrase = "xxx";
                    return;
                }
                defaultPassphrase = e.Passphrase;
                e.Passphrase = "aaa";
            };

            ivm.AskForLogOnPassphrase.Execute(null);

            Assert.That(defaultPassphrase, Is.EqualTo("xxx"));
            Assert.That(ivm.LogOnIdentity.Passphrase.Text, Is.EqualTo("aaa"));
            Assert.That(Resolve.KnownIdentities.DefaultEncryptionIdentity.Passphrase.Thumbprint, Is.EqualTo(new Passphrase("aaa").Thumbprint));

            Passphrase id = Resolve.FileSystemState.KnownPassphrases.FirstOrDefault(i => i.Thumbprint == new Passphrase("aaa").Thumbprint);
            Assert.That(id.Thumbprint, Is.EqualTo(Resolve.KnownIdentities.DefaultEncryptionIdentity.Passphrase.Thumbprint));
        }

        [Test]
        public void AskForNewLogOnPassphraseAutomaticallyBecauseNoIdentitiesExists()
        {
            string defaultPassphrase = null;
            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.CryptoId = new V1Aes128CryptoFactory().Id;
            ivm.LoggingOn += (sender, e) =>
            {
                if (e.IsAskingForPreviouslyUnknownPassphrase)
                {
                    defaultPassphrase = e.Passphrase;
                    e.Passphrase = "aaa";
                    e.Name = "New User Passphrase";
                }
            };

            ivm.AskForLogOnPassphrase.Execute(null);

            Assert.That(defaultPassphrase, Is.EqualTo(String.Empty));
            Assert.That(ivm.LogOnIdentity.Passphrase.Text, Is.EqualTo("aaa"));
            Assert.That(Resolve.KnownIdentities.DefaultEncryptionIdentity.Passphrase.Thumbprint, Is.EqualTo(new Passphrase("aaa").Thumbprint));

            Passphrase id = Resolve.FileSystemState.KnownPassphrases.FirstOrDefault(i => i.Thumbprint == new Passphrase("aaa").Thumbprint);
            Assert.That(id.Thumbprint, Is.EqualTo(Resolve.KnownIdentities.DefaultEncryptionIdentity.Passphrase.Thumbprint));
        }

        [Test]
        public void AskForNewLogOnPassphraseWithCancel()
        {
            string defaultPassphrase = null;
            Resolve.FileSystemState.KnownPassphrases.Add(new Passphrase());
            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            bool isCancelling = false;
            ivm.LoggingOn += (sender, e) =>
            {
                if (isCancelling)
                {
                    e.Cancel = true;
                    return;
                }
                if (!e.IsAskingForPreviouslyUnknownPassphrase)
                {
                    e.IsAskingForPreviouslyUnknownPassphrase = true;
                    e.Passphrase = "xxx";
                    return;
                }
                defaultPassphrase = e.Passphrase;
                e.Cancel = isCancelling = true;
            };

            ivm.AskForLogOnPassphrase.Execute(null);

            Assert.That(defaultPassphrase, Is.EqualTo("xxx"));
            Assert.That(Resolve.KnownIdentities.DefaultEncryptionIdentity == LogOnIdentity.Empty);
            Assert.That(ivm.LogOnIdentity == LogOnIdentity.Empty);

            Passphrase id = Resolve.FileSystemState.KnownPassphrases.FirstOrDefault(i => i.Thumbprint == new Passphrase("xxx").Thumbprint);
            Assert.That(id, Is.Null);
        }

        [Test]
        public void AskForNewLogOnPassphraseWithKnownIdentity()
        {
            Passphrase passphrase = new Passphrase("aaa");
            Passphrase id = passphrase;
            Resolve.FileSystemState.KnownPassphrases.Add(id);

            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.LoggingOn += (sender, e) =>
            {
                if (!e.IsAskingForPreviouslyUnknownPassphrase)
                {
                    e.IsAskingForPreviouslyUnknownPassphrase = true;
                }
                e.Passphrase = "aaa";
            };

            ivm.AskForLogOnPassphrase.Execute(null);

            Assert.That(ivm.LogOnIdentity.Passphrase.Text, Is.EqualTo("aaa"));
            Assert.That(Resolve.KnownIdentities.DefaultEncryptionIdentity.Passphrase.Thumbprint, Is.EqualTo(passphrase.Thumbprint));
            Assert.That(Resolve.FileSystemState.KnownPassphrases.Count(), Is.EqualTo(1));
        }

        [Test]
        public void AskForLogOnPassphraseWithKnownIdentity()
        {
            Passphrase passphrase = new Passphrase("aaa");
            Passphrase id = passphrase;
            Resolve.FileSystemState.KnownPassphrases.Add(id);

            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "aaa";
            };

            ivm.AskForLogOnPassphrase.Execute(null);

            Assert.That(ivm.LogOnIdentity.Passphrase.Text, Is.EqualTo("aaa"));
            Assert.That(Resolve.KnownIdentities.DefaultEncryptionIdentity.Passphrase.Thumbprint, Is.EqualTo(passphrase.Thumbprint));
            Assert.That(Resolve.FileSystemState.KnownPassphrases.Count(), Is.EqualTo(1));
        }

        [Test]
        public void AskForLogOnPassphraseWithKnownIdentityButWrongPassphraseEntered()
        {
            Passphrase passphrase = new Passphrase("aaa");
            Passphrase id = passphrase;
            Resolve.FileSystemState.KnownPassphrases.Add(id);

            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "bbb";
            };

            ivm.AskForLogOnPassphrase.Execute(null);

            Assert.That(ivm.LogOnIdentity == LogOnIdentity.Empty);
            Assert.That(Resolve.FileSystemState.KnownPassphrases.Count(), Is.EqualTo(1));
        }
    }
}