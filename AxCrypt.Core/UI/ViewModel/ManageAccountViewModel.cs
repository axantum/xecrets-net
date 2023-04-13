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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using AxCrypt.Common;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Core.UI.ViewModel
{
    /// <summary>
    /// List the available UserKeys for the currently logged on user, and make it possible to change the password
    /// for them all.
    /// </summary>
    public class ManageAccountViewModel : ViewModelBase
    {
        public IEnumerable<AccountProperties> AccountProperties { get { return GetProperty<IEnumerable<AccountProperties>>(nameof(AccountProperties)); } private set { SetProperty(nameof(AccountProperties), value); } }

        public bool LastChangeStatus { get { return GetProperty<bool>(nameof(LastChangeStatus)); } private set { SetProperty(nameof(LastChangeStatus), value); } }

        [AllowNull]
        public IAsyncAction ChangePassphraseAsync { get; private set; }

        public Func<bool, Task> ChangePasswordCompleteAsync { get; set; } = (success) => Constant.CompletedTask;

        [AllowNull]
        private AccountStorage _accountStorage;

        public static async Task<ManageAccountViewModel> CreateAsync(AccountStorage accountStorage)
        {
            var vm = new ManageAccountViewModel
            {
                _accountStorage = accountStorage
            };

            await vm.InitializePropertyValuesAsync();

            BindPropertyChangedEvents();
            SubscribeToModelEvents();

            return vm;
        }

        private async Task InitializePropertyValuesAsync()
        {
            AccountProperties = (await _accountStorage.AllKeyPairsAsync()).Select(key => new AccountProperties(key.UserEmail, key.Timestamp));

            ChangePassphraseAsync = new AsyncDelegateAction<string>(async (password) => await ChangePassphraseActionAsync(password), async (password) => (await _accountStorage.AllKeyPairsAsync()).Any());
        }

        private static void BindPropertyChangedEvents()
        {
        }

        private static void SubscribeToModelEvents()
        {
        }

        private async Task ChangePassphraseActionAsync(string passphrase)
        {
            LastChangeStatus = await _accountStorage.ChangePassphraseAsync(new Passphrase(passphrase));
            await ChangePasswordCompleteAsync(LastChangeStatus);
        }
    }
}
