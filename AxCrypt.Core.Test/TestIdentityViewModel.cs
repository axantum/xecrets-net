#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions Copyright © 2022-2025, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets Cli, but is derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * The changes and additions are separately copyrighted and only licensed under GPL v3 or later as detailed below,
 * unless explicitly licensed otherwise. If you use any part of these changes and additions in your software,
 * please see https://www.gnu.org/licenses/ for details of what this means for you.
 * 
 * Warning: If you are using the original AxCrypt code under a non-GPL v3 or later license, these changes and additions
 * are not included in that license. If you use these changes under those circumstances, all your code becomes subject to
 * the GPL v3 or later license, according to the principle of strong copyleft as applied to GPL v3 or later.
 *
 * Xecrets Cli is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets Cli is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets Cli. If not, see
 * https://www.gnu.org/licenses/.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions, as well for commit history detailing changes and additions that fall under the strong
 * copyleft provisions mentioned above. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Xecrets Cli Copyright and GPL License notice
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

using AxCrypt.Core.Crypto;
using AxCrypt.Core.IO;
using AxCrypt.Core.Session;
using AxCrypt.Core.UI.ViewModel;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace AxCrypt.Core.Test
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
            SetupAssembly.AssemblySetup(_cryptoImplementation);
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public async Task TestLogOnExistingIdentity()
        {
            Passphrase passphrase = new Passphrase("p");

            Passphrase id = passphrase;
            Resolve.FileSystemState.KnownPassphrases.Add(id);

            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.LoggingOnAsync = (e) =>
            {
                e.Passphrase = new Passphrase("p");
                return Task.FromResult<object>(null);
            };

            await ivm.LogOnLogOff.ExecuteAsync(null);

            Assert.That(Resolve.KnownIdentities.IsLoggedOn);
        }

        [Test]
        public async Task TestLogOnExistingIdentityWithCancel()
        {
            Passphrase passphrase = new Passphrase("p");

            Passphrase id = passphrase;
            Resolve.FileSystemState.KnownPassphrases.Add(id);

            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.LoggingOnAsync = (e) =>
            {
                e.Cancel = true;
                return Task.FromResult<object>(null);
            };

            await ivm.LogOnLogOff.ExecuteAsync(null);

            Assert.That(ivm.LogOnIdentity == LogOnIdentity.Empty);
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.False);
        }

        [Test]
        public async Task TestLogOnLogOffWhenLoggedOn()
        {
            LogOnIdentity passphrase = new LogOnIdentity("p");

            LogOnIdentity id = passphrase;
            Resolve.FileSystemState.KnownPassphrases.Add(id.Passphrase);
            await Resolve.KnownIdentities.SetDefaultEncryptionIdentity(id);

            bool wasLoggingOn = false;
            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.LoggingOnAsync = (e) =>
            {
                wasLoggingOn = true;
                return Task.FromResult<object>(null);
            };

            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.True);

            await ivm.LogOnLogOff.ExecuteAsync(null);

            Assert.That(wasLoggingOn, Is.False);
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.False);
        }

        [Test]
        public async Task TestLogOnLogOffWhenLoggedOffAndNoIdentities()
        {
            bool wasCreateNew = true;
            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.LoggingOnAsync = (e) =>
            {
                wasCreateNew = e.IsAskingForPreviouslyUnknownPassphrase;
                e.Passphrase = new Passphrase("ccc");
                e.Name = "New User Passphrase";
                return Task.FromResult<object>(null);
            };

            Resolve.FileSystemState.KnownPassphrases.Add(Passphrase.Create("ccc"));
            await ivm.LogOnLogOff.ExecuteAsync(null);

            Assert.That(wasCreateNew, Is.False, "Logging on event should not be with Create New set.");
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.True, "Should be logged on.");
            Assert.That(Resolve.KnownIdentities.DefaultEncryptionIdentity.Passphrase.Thumbprint, Is.EqualTo(new Passphrase("ccc").Thumbprint));
            Assert.That(Resolve.FileSystemState.KnownPassphrases.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task TestLogOnLogOffWhenLoggedOffAndNoIdentitiesWithCancel()
        {
            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.LoggingOnAsync = (e) =>
            {
                e.Cancel = true;
                return Task.FromResult<object>(null);
            };

            ivm.LogOnIdentity = new LogOnIdentity("testing");
            await ivm.LogOnLogOff.ExecuteAsync(null);

            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.False, "Not logged on.");
            Assert.That(Resolve.FileSystemState.KnownPassphrases.Count(), Is.EqualTo(0));
            Assert.That(ivm.LogOnIdentity == LogOnIdentity.Empty);
        }

        [Test]
        public async Task AskForLogOnOrDecryptPassphraseActionNotActiveFile()
        {
            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            LogOnIdentity id = null;
            ivm.LoggingOnAsync = (e) =>
            {
                id = e.Identity;
                e.Passphrase = new Passphrase("p");
                return Task.FromResult<object>(null);
            };

            await ivm.AskForDecryptPassphrase.ExecuteAsync(@"C:\Folder\File1-txt.axx");

            Assert.That(ivm.LogOnIdentity.Passphrase.Text, Is.EqualTo("p"));
            Assert.That(id.Passphrase.Thumbprint, Is.EqualTo(Passphrase.Empty.Thumbprint));
        }

        [Test]
        public async Task AskForLogOnOrDecryptPassphraseActionActiveFile()
        {
            LogOnIdentity key = new LogOnIdentity("p");

            ActiveFile activeFile = new ActiveFile(New<IDataStore>(@"C:\Folder\File1-txt.axx"), New<IDataStore>(@"C:\Folder\File1.txt"), key, ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().CryptoId);
            Resolve.FileSystemState.Add(activeFile);

            LogOnIdentity id = null;
            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.LoggingOnAsync = (e) =>
            {
                id = e.Identity;
                e.Passphrase = new Passphrase("p");
                return Task.FromResult<object>(null);
            };

            await ivm.AskForDecryptPassphrase.ExecuteAsync(@"C:\Folder\File1-txt.axx");

            Assert.That(ivm.LogOnIdentity.Passphrase.Text, Is.EqualTo("p"));
            Assert.That(id.Passphrase.Thumbprint, Is.EqualTo(Passphrase.Empty.Thumbprint));
        }

        [Test]
        public async Task AskForLogOnOrDecryptPassphraseActionActiveFileWithExistingIdentity()
        {
            LogOnIdentity key = new LogOnIdentity("p");

            ActiveFile activeFile = new ActiveFile(New<IDataStore>(@"C:\Folder\File1-txt.axx"), New<IDataStore>(@"C:\Folder\File1.txt"), key, ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().CryptoId);
            Resolve.FileSystemState.Add(activeFile);

            LogOnIdentity id = key;
            Resolve.FileSystemState.KnownPassphrases.Add(id.Passphrase);

            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.LoggingOnAsync = (e) =>
            {
                e.Passphrase = new Passphrase("p");
                return Task.FromResult<object>(null);
            };

            await ivm.AskForDecryptPassphrase.ExecuteAsync(@"C:\Folder\File1-txt.axx");

            Assert.That(ivm.LogOnIdentity.Passphrase.Text, Is.EqualTo("p"));
        }

        [Test]
        public async Task AskForLogOnPassphraseAction()
        {
            LogOnIdentity key = new LogOnIdentity("ppp");

            LogOnIdentity id = key;

            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.LoggingOnAsync = (e) =>
            {
                e.Passphrase = new Passphrase("ppp");
                e.IsAskingForPreviouslyUnknownPassphrase = true;
                return Task.FromResult<object>(null);
            };

            await ivm.AskForLogOnPassphrase.ExecuteAsync(id);

            Assert.That(ivm.LogOnIdentity.Passphrase.Text, Is.EqualTo("ppp"));
            Assert.That(Resolve.KnownIdentities.DefaultEncryptionIdentity.Passphrase.Thumbprint, Is.EqualTo(id.Passphrase.Thumbprint));
        }

        [Test]
        public async Task AskForLogOnPassphraseActionWithCancel()
        {
            LogOnIdentity key = new LogOnIdentity("ppp");

            LogOnIdentity id = key;

            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.LoggingOnAsync = (e) =>
            {
                e.Cancel = true;
                return Task.FromResult<object>(null);
            };

            await ivm.AskForLogOnPassphrase.ExecuteAsync(id);

            Assert.That(ivm.LogOnIdentity == LogOnIdentity.Empty);
            Assert.That(Resolve.KnownIdentities.DefaultEncryptionIdentity == LogOnIdentity.Empty);
        }

        [Test]
        public async Task AskForNewLogOnPassphrase()
        {
            Passphrase defaultPassphrase = null;
            Resolve.FileSystemState.KnownPassphrases.Add(Passphrase.Empty);
            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.LoggingOnAsync = (e) =>
            {
                if (!e.IsAskingForPreviouslyUnknownPassphrase)
                {
                    e.IsAskingForPreviouslyUnknownPassphrase = true;
                    e.Passphrase = new Passphrase("xxx");
                    return Task.FromResult<object>(null);
                }
                defaultPassphrase = e.Passphrase;
                e.Passphrase = new Passphrase("aaa");
                return Task.FromResult<object>(null);
            };

            await ivm.AskForLogOnPassphrase.ExecuteAsync(null);

            Assert.That(defaultPassphrase, Is.EqualTo(new Passphrase("xxx")));
            Assert.That(ivm.LogOnIdentity.Passphrase.Text, Is.EqualTo("aaa"));
            Assert.That(Resolve.KnownIdentities.DefaultEncryptionIdentity.Passphrase.Thumbprint, Is.EqualTo(new Passphrase("aaa").Thumbprint));

            Passphrase id = Resolve.FileSystemState.KnownPassphrases.FirstOrDefault(i => i.Thumbprint == new Passphrase("aaa").Thumbprint);
            Assert.That(id.Thumbprint, Is.EqualTo(Resolve.KnownIdentities.DefaultEncryptionIdentity.Passphrase.Thumbprint));
        }

        [Test]
        public async Task AskForNewLogOnPassphraseAutomaticallyBecauseNoIdentitiesExists()
        {
            Resolve.FileSystemState.KnownPassphrases.Add(Passphrase.Create("aaa"));
            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.LoggingOnAsync = (e) =>
            {
                e.Passphrase = new Passphrase("aaa");
                e.Name = "New User Passphrase";
                return Task.FromResult<object>(null);
            };

            await ivm.AskForLogOnPassphrase.ExecuteAsync(LogOnIdentity.Empty);

            Assert.That(ivm.LogOnIdentity.Passphrase.Text, Is.EqualTo("aaa"));
            Assert.That(Resolve.KnownIdentities.DefaultEncryptionIdentity.Passphrase.Thumbprint, Is.EqualTo(new Passphrase("aaa").Thumbprint));

            Passphrase id = Resolve.FileSystemState.KnownPassphrases.FirstOrDefault(i => i.Thumbprint == new Passphrase("aaa").Thumbprint);
            Assert.That(id.Thumbprint, Is.EqualTo(Resolve.KnownIdentities.DefaultEncryptionIdentity.Passphrase.Thumbprint));
        }

        [Test]
        public async Task AskForNewLogOnPassphraseWithCancel()
        {
            Passphrase defaultPassphrase = null;
            Resolve.FileSystemState.KnownPassphrases.Add(Passphrase.Empty);
            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            bool isCancelling = false;
            ivm.LoggingOnAsync = (e) =>
            {
                if (isCancelling)
                {
                    e.Cancel = true;
                    return Task.FromResult<object>(null);
                }
                if (!e.IsAskingForPreviouslyUnknownPassphrase)
                {
                    e.IsAskingForPreviouslyUnknownPassphrase = true;
                    e.Passphrase = new Passphrase("xxx");
                    return Task.FromResult<object>(null);
                }
                defaultPassphrase = e.Passphrase;
                e.Cancel = isCancelling = true;
                return Task.FromResult<object>(null);
            };

            await ivm.AskForLogOnPassphrase.ExecuteAsync(null);

            Assert.That(defaultPassphrase, Is.EqualTo(new Passphrase("xxx")));
            Assert.That(Resolve.KnownIdentities.DefaultEncryptionIdentity == LogOnIdentity.Empty);
            Assert.That(ivm.LogOnIdentity == LogOnIdentity.Empty);

            Passphrase id = Resolve.FileSystemState.KnownPassphrases.FirstOrDefault(i => i.Thumbprint == new Passphrase("xxx").Thumbprint);
            Assert.That(id, Is.Null);
        }

        [Test]
        public async Task AskForNewLogOnPassphraseWithKnownIdentity()
        {
            Passphrase passphrase = new Passphrase("aaa");
            Passphrase id = passphrase;
            Resolve.FileSystemState.KnownPassphrases.Add(id);

            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.LoggingOnAsync = (e) =>
            {
                if (!e.IsAskingForPreviouslyUnknownPassphrase)
                {
                    e.IsAskingForPreviouslyUnknownPassphrase = true;
                }
                e.Passphrase = new Passphrase("aaa");
                return Task.FromResult<object>(null);
            };

            await ivm.AskForLogOnPassphrase.ExecuteAsync(null);

            Assert.That(ivm.LogOnIdentity.Passphrase.Text, Is.EqualTo("aaa"));
            Assert.That(Resolve.KnownIdentities.DefaultEncryptionIdentity.Passphrase.Thumbprint, Is.EqualTo(passphrase.Thumbprint));
            Assert.That(Resolve.FileSystemState.KnownPassphrases.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task AskForLogOnPassphraseWithKnownIdentity()
        {
            Passphrase passphrase = new Passphrase("aaa");
            Passphrase id = passphrase;
            Resolve.FileSystemState.KnownPassphrases.Add(id);

            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.LoggingOnAsync = (e) =>
            {
                e.Passphrase = new Passphrase("aaa");
                return Task.FromResult<object>(null);
            };

            await ivm.AskForLogOnPassphrase.ExecuteAsync(null);

            Assert.That(ivm.LogOnIdentity.Passphrase.Text, Is.EqualTo("aaa"));
            Assert.That(Resolve.KnownIdentities.DefaultEncryptionIdentity.Passphrase.Thumbprint, Is.EqualTo(passphrase.Thumbprint));
            Assert.That(Resolve.FileSystemState.KnownPassphrases.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task AskForLogOnPassphraseWithKnownIdentityButWrongPassphraseEntered()
        {
            Passphrase passphrase = new Passphrase("aaa");
            Passphrase id = passphrase;
            Resolve.FileSystemState.KnownPassphrases.Add(id);

            IdentityViewModel ivm = new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify);
            ivm.LoggingOnAsync = (e) =>
            {
                e.Passphrase = new Passphrase("bbb");
                return Task.FromResult<object>(null);
            };

            await ivm.AskForLogOnPassphrase.ExecuteAsync(null);

            Assert.That(ivm.LogOnIdentity == LogOnIdentity.Empty);
            Assert.That(Resolve.FileSystemState.KnownPassphrases.Count(), Is.EqualTo(1));
        }
    }
}
