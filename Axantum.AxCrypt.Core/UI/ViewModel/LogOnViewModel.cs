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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Axantum.AxCrypt.Core.Crypto;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class LogOnViewModel : ViewModelBase
    {
        public LogOnViewModel(string identityName)
        {
            InitializePropertyValues(identityName);
        }

        private void InitializePropertyValues(string identityName)
        {
            IdentityName = identityName;
            Passphrase = String.Empty;
        }

        public string IdentityName { get { return GetProperty<string>("IdentityName"); } set { SetProperty("IdentityName", value); } }

        public bool ShowPassphrase { get { return GetProperty<bool>("ShowPassphrase"); } set { SetProperty("ShowPassphrase", value); } }

        public string Passphrase { get { return GetProperty<string>("Passphrase"); } set { SetProperty("Passphrase", value); } }

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
            switch (columnName)
            {
                case "Passphrase":
                    SymmetricKeyThumbprint thumbprint = new V1Passphrase(Passphrase).DerivedKey.Thumbprint;
                    if (Instance.FileSystemState.Identities.Any(identity => (String.IsNullOrEmpty(IdentityName) || IdentityName == identity.Name) && identity.Thumbprint == thumbprint))
                    {
                        return String.Empty;
                    }
                    ValidationError = (int)ViewModel.ValidationError.WrongPassphrase;
                    break;

                default:
                    throw new ArgumentException("Cannot validate property.", columnName);
            }
            return ValidationError.ToString(CultureInfo.InvariantCulture);
        }
    }
}