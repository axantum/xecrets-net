#region Coypright and License

/*
 * AxCrypt - Copyright 2013, Svante Seleborg, All Rights Reserved
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
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class NewPassphraseViewModel : ViewModelBase
    {
        public NewPassphraseViewModel(string defaultIdentityName)
        {
            bool defaultIdentityKnown = Instance.FileSystemState.Identities.Any(identity => String.Compare(identity.Name, Environment.UserName, StringComparison.OrdinalIgnoreCase) == 0);
            IdentityName = defaultIdentityKnown ? defaultIdentityName : String.Empty;
        }

        public string IdentityName { get { return GetProperty<string>("IdentityName"); } set { SetProperty("IdentityName", value); } }

        public bool ShowPassphrase { get { return GetProperty<bool>("ShowPassphrase"); } set { SetProperty("ShowPassphrase", value); } }

        public string Passphrase { get { return GetProperty<string>("Passphrase"); } set { SetProperty("Passphrase", value); } }

        public string Verification { get { return GetProperty<string>("Verification"); } set { SetProperty("Verification", value); } }

        public ValidationError ValidationError { get { return GetProperty<ValidationError>("ValidationError"); } set { SetProperty("ValidationError", value); } }

        public override string this[string propertyName]
        {
            get
            {
                string error = base[propertyName];
                if (!String.IsNullOrEmpty(error))
                {
                    return error;
                }
                switch (propertyName)
                {
                    case "Passphrase":
                    case "Verification":
                        if (String.Compare(Passphrase, Verification, StringComparison.Ordinal) == 0)
                        {
                            return String.Empty;
                        }
                        ValidationError = ValidationError.VerificationPassphraseWrong;
                        break;

                    case "IdentityName":
                        if (!Instance.FileSystemState.Identities.Any(i => i.Name == IdentityName))
                        {
                            return String.Empty;
                        }
                        ValidationError = ValidationError.IdentityExistsAlready;
                        break;

                    default:
                        return String.Empty;
                }
                return ValidationError.ToString();
            }
        }
    }
}