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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    /// <summary>
    /// Request user name and passphrase, get a public key pair from the server, or generate
    /// locally, and store the result locally.
    /// </summary>
    public class CreateNewAccountViewModel : ViewModelBase
    {
        public string Passphrase { get { return GetProperty<string>(nameof(Passphrase)); } set { SetProperty(nameof(Passphrase), value); } }

        public string Verification { get { return GetProperty<string>(nameof(Verification)); } set { SetProperty(nameof(Verification), value); } }

        public bool ShowPassphrase { get { return GetProperty<bool>(nameof(ShowPassphrase)); } set { SetProperty(nameof(ShowPassphrase), value); } }

        public string UserEmail { get { return GetProperty<string>(nameof(UserEmail)); } set { SetProperty(nameof(UserEmail), value); } }

        public IAction CreateAccount { get { return new DelegateAction<object>((o) => CreateAccountAction()); } }

        public CreateNewAccountViewModel(string passphrase, EmailAddress email)
        {
            InitializePropertyValues(passphrase, email);
            BindPropertyChangedEvents();
        }

        private void InitializePropertyValues(string passphrase, EmailAddress email)
        {
            Passphrase = passphrase ?? String.Empty;
            Verification = passphrase ?? String.Empty;

            UserEmail = email.Address;
            ShowPassphrase = Resolve.UserSettings.DisplayEncryptPassphrase;
        }

        private void BindPropertyChangedEvents()
        {
            BindPropertyChangedInternal(nameof(ShowPassphrase), (bool show) => Resolve.UserSettings.DisplayEncryptPassphrase = show);
            BindPropertyChangedInternal(nameof(UserEmail), async (string userEmail) => { if (await ValidateAsync(nameof(UserEmail))) { Resolve.UserSettings.UserEmail = userEmail; } });
        }

        protected override Task<bool> ValidateAsync(string columnName)
        {
            switch (columnName)
            {
                case nameof(UserEmail):
                    if (!UserEmail.IsValidEmail())
                    {
                        ValidationError = (int)ViewModel.ValidationError.InvalidEmail;
                        return Task.FromResult(false);
                    }
                    return Task.FromResult(true);

                case nameof(Verification):
                    if (String.IsNullOrEmpty(Verification))
                    {
                        return Task.FromResult(false);
                    }
                    return Task.FromResult(String.Compare(Passphrase, Verification, StringComparison.Ordinal) == 0);

                case nameof(Passphrase):
                    return Task.FromResult(!String.IsNullOrEmpty(Passphrase));

                default:
                    return Task.FromResult(true);
            }
        }

        private object CreateAccountAction()
        {
            if (String.IsNullOrEmpty(UserEmail))
            {
                return null;
            }

            AccountStorage accountStorage = new AccountStorage(New<LogOnIdentity, IAccountService>(new LogOnIdentity(EmailAddress.Parse(UserEmail), new Passphrase(Passphrase))));
            UserKeyPair userKeys = new UserKeyPair(EmailAddress.Parse(UserEmail), Resolve.Environment.UtcNow, New<KeyPairService>().New());
            accountStorage.ImportAsync(userKeys).Wait();

            return null;
        }
    }
}