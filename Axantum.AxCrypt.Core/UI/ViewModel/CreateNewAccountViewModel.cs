#region Coypright and License

/*
 * AxCrypt - Copyright 2015w, Svante Seleborg, All Rights Reserved
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
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

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
            BindPropertyChangedInternal(nameof(UserEmail), (string userEmail) => { if (String.IsNullOrEmpty(Validate(nameof(UserEmail)))) { Resolve.UserSettings.UserEmail = userEmail; } });
        }

        public override string this[string columnName]
        {
            get
            {
                string error = base[columnName];
                if (String.IsNullOrEmpty(error))
                {
                    error = Validate(columnName);
                }
                return error;
            }
        }

        private string Validate(string columnName)
        {
            if (ValidateInternal(columnName))
            {
                return String.Empty;
            }
            return ValidationError.ToString(CultureInfo.InvariantCulture);
        }

        private bool ValidateInternal(string columnName)
        {
            switch (columnName)
            {
                case nameof(UserEmail):
                    if (!UserEmail.IsValidEmail())
                    {
                        ValidationError = (int)ViewModel.ValidationError.InvalidEmail;
                        return false;
                    }
                    return true;

                case nameof(Verification):
                    if (String.IsNullOrEmpty(Verification))
                    {
                        return false;
                    }
                    return String.Compare(Passphrase, Verification, StringComparison.Ordinal) == 0;

                case nameof(Passphrase):
                    return !String.IsNullOrEmpty(Passphrase);

                default:
                    return true;
            }
        }

        private object CreateAccountAction()
        {
            if (String.IsNullOrEmpty(UserEmail))
            {
                return null;
            }

            AccountStorage store = new AccountStorage(New<LogOnIdentity, IAccountService>(new LogOnIdentity(EmailAddress.Parse(UserEmail), new Passphrase(Passphrase))));
            UserKeyPair userKeys = new UserKeyPair(EmailAddress.Parse(UserEmail), DateTime.UtcNow, New<KeyPairService>().New());
            store.ImportAsync(userKeys);

            return null;
        }
    }
}