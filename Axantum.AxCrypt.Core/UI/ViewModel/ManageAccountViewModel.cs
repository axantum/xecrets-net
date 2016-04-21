#region Coypright and License

/*
 * AxCrypt - Copyright 2015, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    /// <summary>
    /// List the available UserKeys for the currently logged on user, and make it possible to change the password
    /// for them all.
    /// </summary>
    public class ManageAccountViewModel : ViewModelBase
    {
        public IEnumerable<AccountProperties> AccountProperties { get { return GetProperty<IEnumerable<AccountProperties>>(nameof(AccountProperties)); } private set { SetProperty(nameof(AccountProperties), value); } }

        public IAsyncAction ChangePassphraseAsync { get; private set; }

        private AccountStorage _accountStorage;

        public ManageAccountViewModel(AccountStorage accountStorage)
        {
            _accountStorage = accountStorage;

            InitializePropertyValues();
            BindPropertyChangedEvents();
            SubscribeToModelEvents();
        }

        private void InitializePropertyValues()
        {
            AccountProperties = _accountStorage.AllKeyPairsAsync().Result.Select(key => new AccountProperties(key.UserEmail, key.Timestamp));

            ChangePassphraseAsync = new AsyncDelegateAction<string>(async (password) => await ChangePassphraseActionAsync(password), (password) => _accountStorage.AllKeyPairsAsync().Result.Any());
        }

        private static void BindPropertyChangedEvents()
        {
        }

        private static void SubscribeToModelEvents()
        {
        }

        private async Task ChangePassphraseActionAsync(string passphrase)
        {
            await _accountStorage.ChangePassphraseAsync(new Passphrase(passphrase)).Free();
        }
    }
}