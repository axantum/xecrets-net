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

using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Fake;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestManageAccountViewModel
    {
        private CryptoImplementation _cryptoImplementation;

        public TestManageAccountViewModel(CryptoImplementation cryptoImplementation)
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
        public async Task TestManageAccountViewModelChangePassword()
        {
            UserKeyPair key1 = new UserKeyPair(EmailAddress.Parse("svante@axantum.com"), 512);
            UserKeyPair key2 = new UserKeyPair(EmailAddress.Parse("svante@axantum.com"), 512);

            var mockUserAsymmetricKeysStore = new Mock<AccountStorage>((IAccountService)null);
            mockUserAsymmetricKeysStore.Setup<Task<IEnumerable<UserKeyPair>>>(f => f.AllKeyPairsAsync()).Returns(Task.FromResult((IEnumerable<UserKeyPair>)new UserKeyPair[] { key1, key2 }));
            string passphraseUsed = String.Empty;
            mockUserAsymmetricKeysStore.Setup(f => f.ChangePassphraseAsync(It.IsAny<Passphrase>()))
                .Callback<Passphrase>((passphrase) =>
                {
                    passphraseUsed = passphrase.Text;
                }).Returns(Task.FromResult(true));

            ManageAccountViewModel viewModel = new ManageAccountViewModel(mockUserAsymmetricKeysStore.Object);
            IEnumerable<AccountProperties> emailsList = null;
            viewModel.BindPropertyChanged(nameof(ManageAccountViewModel.AccountProperties), (IEnumerable<AccountProperties> emails) => emailsList = emails);
            Assert.That(emailsList.Count(), Is.EqualTo(2), "There should be two accounts now.");
            Assert.That(emailsList.First().EmailAddress, Is.EqualTo("svante@axantum.com"), "The first should be 'svante@axantum.com'");
            Assert.That(emailsList.Last().EmailAddress, Is.EqualTo("svante@axantum.com"), "The last should be 'svante@axantum.com'");

            await viewModel.ChangePassphraseAsync.ExecuteAsync("allan").Free();
            Assert.That(passphraseUsed, Is.EqualTo("allan"));
        }
    }
}