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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    /// <summary>
    /// Request user name and passphrase, get a public key pair from the server, or generate
    /// locally, and store the result locally.
    /// </summary>
    public class CreateNewAccountViewModel : ViewModelBase
    {
        public string Passphrase { get { return GetProperty<string>("Passphrase"); } set { SetProperty("Passphrase", value); } }

        public string Verification { get { return GetProperty<string>("Verification"); } set { SetProperty("Verification", value); } }

        public bool ShowPassphrase { get { return GetProperty<bool>("ShowPassphrase"); } set { SetProperty("ShowPassphrase", value); } }

        public string UserEmail { get { return GetProperty<string>("UserEmail"); } set { SetProperty("UserEmail", value); } }

        public IAction CreateAccount { get { return new DelegateAction<object>((o) => CreateAccountAction()); } }

        private UserAsymmetricKeysStore _keysStore;

        public CreateNewAccountViewModel(UserAsymmetricKeysStore keysStore, string passphrase)
        {
            _keysStore = keysStore;

            InitializePropertyValues(passphrase);
            BindPropertyChangedEvents();
        }

        private void InitializePropertyValues(string passphrase)
        {
            Passphrase = passphrase ?? String.Empty;
            Verification = passphrase ?? String.Empty;

            UserEmail = String.Empty;
            ShowPassphrase = Resolve.UserSettings.DisplayEncryptPassphrase;
        }

        private void BindPropertyChangedEvents()
        {
            BindPropertyChangedInternal("ShowPassphrase", (bool show) => Resolve.UserSettings.DisplayEncryptPassphrase = show);
            BindPropertyChangedInternal("UserEmail", (string userEmail) => { if (String.IsNullOrEmpty(Validate("UserEmail"))) { Resolve.UserSettings.UserEmail = userEmail; } });
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
                case "UserEmail":
                    if (!UserEmail.IsValidEmail())
                    {
                        ValidationError = (int)ViewModel.ValidationError.InvalidEmail;
                        return false;
                    }
                    return true;

                case "Verification":
                    if (String.IsNullOrEmpty(Verification))
                    {
                        return false;
                    }
                    return String.Compare(Passphrase, Verification, StringComparison.Ordinal) == 0;

                case "Passphrase":
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
            _keysStore.Create(EmailAddress.Parse(UserEmail), new Passphrase(Passphrase));

            return null;
        }
    }
}