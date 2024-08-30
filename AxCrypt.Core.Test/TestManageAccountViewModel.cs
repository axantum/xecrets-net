#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions copyright © 2022-2024, Svante Seleborg, All Rights Reserved.
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

using AxCrypt.Common;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Service;
using AxCrypt.Core.Session;
using AxCrypt.Core.UI;
using AxCrypt.Core.UI.ViewModel;
using AxCrypt.Fake;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace AxCrypt.Core.Test
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
            SetupAssembly.AssemblySetup(_cryptoImplementation);
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public async Task TestManageAccountViewModelChangePassword()
        {
            UserKeyPair key1 = new UserKeyPair(EmailAddress.Parse("svante@axantum.com"), 1024);
            UserKeyPair key2 = new UserKeyPair(EmailAddress.Parse("svante@axantum.com"), 1024);

            var mockUserAsymmetricKeysStore = new Mock<AccountStorage>((IAccountService)null);
            mockUserAsymmetricKeysStore.Setup<Task<IEnumerable<UserKeyPair>>>(f => f.AllKeyPairsAsync()).Returns(Task.FromResult((IEnumerable<UserKeyPair>)new UserKeyPair[] { key1, key2 }));
            string passphraseUsed = String.Empty;
            mockUserAsymmetricKeysStore.Setup(f => f.ChangePassphraseAsync(It.IsAny<Passphrase>()))
                .Callback<Passphrase>((passphrase) =>
                {
                    passphraseUsed = passphrase.Text;
                }).Returns(Task.FromResult(true));

            ManageAccountViewModel viewModel = await ManageAccountViewModel.CreateAsync(mockUserAsymmetricKeysStore.Object);
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
