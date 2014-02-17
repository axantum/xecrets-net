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

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestIdentityViewModel
    {
        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestLogOnExistingIdentity()
        {
            Passphrase passphrase = new Passphrase("p");

            PassphraseIdentity id = new PassphraseIdentity("Test", passphrase.DerivedPassphrase);
            Instance.FileSystemState.Identities.Add(id);

            IdentityViewModel ivm = new IdentityViewModel(Instance.FileSystemState, Instance.KnownKeys, Instance.UserSettings);
            ivm.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "p";
            };

            ivm.LogOnLogOff.Execute(null);

            Assert.That(Instance.KnownKeys.IsLoggedOn);
        }

        [Test]
        public static void TestLogOnExistingIdentityWithCancel()
        {
            Passphrase passphrase = new Passphrase("p");

            PassphraseIdentity id = new PassphraseIdentity("Test", passphrase.DerivedPassphrase);
            Instance.FileSystemState.Identities.Add(id);

            IdentityViewModel ivm = new IdentityViewModel(Instance.FileSystemState, Instance.KnownKeys, Instance.UserSettings);
            ivm.LoggingOn += (sender, e) =>
            {
                e.Cancel = true;
            };

            ivm.LogOnLogOff.Execute(null);

            Assert.That(ivm.PassphraseText, Is.EqualTo(""));
            Assert.That(Instance.KnownKeys.IsLoggedOn, Is.False);
        }

        [Test]
        public static void TestLogOnLogOffWhenLoggedOn()
        {
            Passphrase passphrase = new Passphrase("p");

            PassphraseIdentity id = new PassphraseIdentity("Test", passphrase.DerivedPassphrase);
            Instance.FileSystemState.Identities.Add(id);
            Instance.KnownKeys.DefaultEncryptionKey = id.Key;

            bool wasLoggingOn = false;
            IdentityViewModel ivm = new IdentityViewModel(Instance.FileSystemState, Instance.KnownKeys, Instance.UserSettings);
            ivm.LoggingOn += (sender, e) =>
            {
                wasLoggingOn = true;
            };

            Assert.That(Instance.KnownKeys.IsLoggedOn, Is.True);

            ivm.LogOnLogOff.Execute(null);

            Assert.That(wasLoggingOn, Is.False);
            Assert.That(Instance.KnownKeys.IsLoggedOn, Is.False);
        }

        [Test]
        public static void TestLogOnLogOffWhenLoggedOffAndNoIdentities()
        {
            bool wasCreateNew = false;
            IdentityViewModel ivm = new IdentityViewModel(Instance.FileSystemState, Instance.KnownKeys, Instance.UserSettings);
            ivm.LoggingOn += (sender, e) =>
            {
                wasCreateNew = e.CreateNew;
                e.Passphrase = "ccc";
                e.Name = "New User Passphrase";
            };

            ivm.LogOnLogOff.Execute(null);

            Assert.That(wasCreateNew, Is.True, "Logging on event should be with Create New set.");
            Assert.That(Instance.KnownKeys.IsLoggedOn, Is.True, "Should be logged on.");
            Assert.That(Instance.KnownKeys.DefaultEncryptionKey.Thumbprint, Is.EqualTo(new Passphrase("ccc").DerivedPassphrase.Thumbprint));
            Assert.That(Instance.FileSystemState.Identities.Count(), Is.EqualTo(1));
            Assert.That(Instance.FileSystemState.Identities.First().Name, Is.EqualTo("New User Passphrase"));
        }

        [Test]
        public static void TestLogOnLogOffWhenLoggedOffAndNoIdentitiesWithCancel()
        {
            IdentityViewModel ivm = new IdentityViewModel(Instance.FileSystemState, Instance.KnownKeys, Instance.UserSettings);
            ivm.LoggingOn += (sender, e) =>
            {
                e.Cancel = true;
            };

            ivm.PassphraseText = "testing";
            ivm.LogOnLogOff.Execute(null);

            Assert.That(Instance.KnownKeys.IsLoggedOn, Is.False, "Not logged on.");
            Assert.That(Instance.FileSystemState.Identities.Count(), Is.EqualTo(0));
            Assert.That(ivm.PassphraseText, Is.EqualTo(""));
        }

        [Test]
        public static void AskForLogOnOrDecryptPassphraseActionNotActiveFile()
        {
            IdentityViewModel ivm = new IdentityViewModel(Instance.FileSystemState, Instance.KnownKeys, Instance.UserSettings);
            PassphraseIdentity id = null;
            ivm.LoggingOn += (sender, e) =>
            {
                id = e.Identity;
                e.Passphrase = "p";
            };

            ivm.AskForLogOnOrDecryptPassphrase.Execute(@"C:\Folder\File1-txt.axx");

            Assert.That(ivm.PassphraseText, Is.EqualTo("p"));
            Assert.That(Instance.KnownKeys.DefaultEncryptionKey.Thumbprint, Is.EqualTo(new Passphrase("p").DerivedPassphrase.Thumbprint));
            Assert.That(id.Thumbprint, Is.EqualTo(SymmetricKey.Zero128.Thumbprint));
        }

        [Test]
        public static void AskForLogOnOrDecryptPassphraseActionActiveFile()
        {
            SymmetricKey key = new Passphrase("p").DerivedPassphrase;

            ActiveFile activeFile = new ActiveFile(Factory.New<IRuntimeFileInfo>(@"C:\Folder\File1-txt.axx"), Factory.New<IRuntimeFileInfo>(@"C:\Folder\File1.txt"), key, ActiveFileStatus.NotDecrypted);
            Instance.FileSystemState.Add(activeFile);

            PassphraseIdentity id = null;
            IdentityViewModel ivm = new IdentityViewModel(Instance.FileSystemState, Instance.KnownKeys, Instance.UserSettings);
            ivm.LoggingOn += (sender, e) =>
            {
                id = e.Identity;
                e.Passphrase = "p";
            };

            ivm.AskForLogOnOrDecryptPassphrase.Execute(@"C:\Folder\File1-txt.axx");

            Assert.That(ivm.PassphraseText, Is.EqualTo("p"));
            Assert.That(Instance.KnownKeys.DefaultEncryptionKey.Thumbprint, Is.EqualTo(key.Thumbprint));
            Assert.That(id.Thumbprint, Is.EqualTo(SymmetricKey.Zero128.Thumbprint));
        }

        [Test]
        public static void AskForLogOnOrDecryptPassphraseActionActiveFileWithExistingIdentity()
        {
            SymmetricKey key = new Passphrase("p").DerivedPassphrase;

            ActiveFile activeFile = new ActiveFile(Factory.New<IRuntimeFileInfo>(@"C:\Folder\File1-txt.axx"), Factory.New<IRuntimeFileInfo>(@"C:\Folder\File1.txt"), key, ActiveFileStatus.NotDecrypted);
            Instance.FileSystemState.Add(activeFile);

            PassphraseIdentity id = new PassphraseIdentity("Test User", key);
            Instance.FileSystemState.Identities.Add(id);

            string name = null;
            IdentityViewModel ivm = new IdentityViewModel(Instance.FileSystemState, Instance.KnownKeys, Instance.UserSettings);
            ivm.LoggingOn += (sender, e) =>
            {
                name = e.Identity.Name;
                e.Passphrase = "p";
            };

            ivm.AskForLogOnOrDecryptPassphrase.Execute(@"C:\Folder\File1-txt.axx");

            Assert.That(ivm.PassphraseText, Is.EqualTo("p"));
            Assert.That(Instance.KnownKeys.DefaultEncryptionKey.Thumbprint, Is.EqualTo(key.Thumbprint));
            Assert.That(name, Is.EqualTo("Test User"));
        }

        [Test]
        public static void AskForLogOnPassphraseAction()
        {
            SymmetricKey key = new Passphrase("ppp").DerivedPassphrase;

            PassphraseIdentity id = new PassphraseIdentity("Test User", key);

            IdentityViewModel ivm = new IdentityViewModel(Instance.FileSystemState, Instance.KnownKeys, Instance.UserSettings);
            ivm.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "ppp";
            };

            ivm.AskForLogOnPassphrase.Execute(id);

            Assert.That(ivm.PassphraseText, Is.EqualTo("ppp"));
            Assert.That(Instance.KnownKeys.DefaultEncryptionKey.Thumbprint, Is.EqualTo(id.Thumbprint));
        }

        [Test]
        public static void AskForLogOnPassphraseActionWithCancel()
        {
            SymmetricKey key = new Passphrase("ppp").DerivedPassphrase;

            PassphraseIdentity id = new PassphraseIdentity("Test User", key);

            IdentityViewModel ivm = new IdentityViewModel(Instance.FileSystemState, Instance.KnownKeys, Instance.UserSettings);
            ivm.LoggingOn += (sender, e) =>
            {
                e.Cancel = true;
            };

            ivm.AskForLogOnPassphrase.Execute(id);

            Assert.That(ivm.PassphraseText.Length, Is.EqualTo(0));
            Assert.That(Instance.KnownKeys.DefaultEncryptionKey, Is.Null);
        }

        [Test]
        public static void AskForNewLogOnPassphrase()
        {
            string defaultPassphrase = null;
            IdentityViewModel ivm = new IdentityViewModel(Instance.FileSystemState, Instance.KnownKeys, Instance.UserSettings);
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
                e.Name = "New User Passphrase";
            };

            ivm.AskForLogOnPassphrase.Execute(null);

            Assert.That(defaultPassphrase, Is.EqualTo("xxx"));
            Assert.That(ivm.PassphraseText, Is.EqualTo("aaa"));
            Assert.That(Instance.KnownKeys.DefaultEncryptionKey.Thumbprint, Is.EqualTo(new Passphrase("aaa").DerivedPassphrase.Thumbprint));

            PassphraseIdentity id = Instance.FileSystemState.Identities.FirstOrDefault(i => i.Name == "New User Passphrase");
            Assert.That(id.Thumbprint, Is.EqualTo(Instance.KnownKeys.DefaultEncryptionKey.Thumbprint));
        }

        [Test]
        public static void AskForNewLogOnPassphraseWithCancel()
        {
            string defaultPassphrase = null;
            IdentityViewModel ivm = new IdentityViewModel(Instance.FileSystemState, Instance.KnownKeys, Instance.UserSettings);
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
            Assert.That(Instance.KnownKeys.DefaultEncryptionKey, Is.Null);
            Assert.That(ivm.PassphraseText, Is.EqualTo(""));

            PassphraseIdentity id = Instance.FileSystemState.Identities.FirstOrDefault(i => i.Name == "New User Passphrase");
            Assert.That(id, Is.Null);
        }

        [Test]
        public static void AskForNewLogOnPassphraseWithKnownIdentity()
        {
            Passphrase passphrase = new Passphrase("aaa");
            PassphraseIdentity id = new PassphraseIdentity("Test User", passphrase.DerivedPassphrase);
            Instance.FileSystemState.Identities.Add(id);

            IdentityViewModel ivm = new IdentityViewModel(Instance.FileSystemState, Instance.KnownKeys, Instance.UserSettings);
            ivm.LoggingOn += (sender, e) =>
            {
                if (!e.CreateNew)
                {
                    e.CreateNew = true;
                }
                e.Passphrase = "aaa";
            };

            ivm.AskForLogOnPassphrase.Execute(null);

            Assert.That(ivm.PassphraseText, Is.EqualTo("aaa"));
            Assert.That(Instance.KnownKeys.DefaultEncryptionKey.Thumbprint, Is.EqualTo(passphrase.DerivedPassphrase.Thumbprint));
            Assert.That(Instance.FileSystemState.Identities.Count(), Is.EqualTo(1));
        }
    }
}