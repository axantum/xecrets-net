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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Globalization;
using System.Linq;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class NewPassphraseViewModel : ViewModelBase
    {
        private string _encryptedFileFullName;

        public NewPassphraseViewModel(string passphrase, string encryptedFileFullName)
        {
            _encryptedFileFullName = encryptedFileFullName;
            InitializePropertyValues(passphrase);
        }

        private void InitializePropertyValues(string passphrase)
        {
            Passphrase = passphrase ?? String.Empty;
            Verification = passphrase ?? String.Empty;
            FileName = String.IsNullOrEmpty(_encryptedFileFullName) ? String.Empty : TypeMap.Resolve.New<IRuntimeFileInfo>(_encryptedFileFullName).Name;
        }

        public bool ShowPassphrase { get { return GetProperty<bool>("ShowPassphrase"); } set { SetProperty("ShowPassphrase", value); } }

        public string Passphrase { get { return GetProperty<string>("Passphrase"); } set { SetProperty("Passphrase", value); } }

        public string Verification { get { return GetProperty<string>("Verification"); } set { SetProperty("Verification", value); } }

        public string FileName { get { return GetProperty<string>("FileName"); } set { SetProperty("FileName", value); } }

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
                case "Passphrase":
                    if (!IsPassphraseValidForFileIfAny(Passphrase, _encryptedFileFullName))
                    {
                        ValidationError = (int)ViewModel.ValidationError.WrongPassphrase;
                        return false;
                    }
                    break;

                case "Verification":
                    if (!ValidateVerification())
                    {
                        ValidationError = (int)ViewModel.ValidationError.VerificationPassphraseWrong;
                        return false;
                    }
                    break;

                default:
                    throw new ArgumentException("Cannot validate property.", columnName);
            }
            return true;
        }

        private bool ValidateVerification()
        {
            return String.Compare(Passphrase, Verification, StringComparison.Ordinal) == 0;
        }

        private static bool IsPassphraseValidForFileIfAny(string passphrase, string encryptedFileFullName)
        {
            if (String.IsNullOrEmpty(encryptedFileFullName))
            {
                return true;
            }
            return TypeMap.Resolve.New<AxCryptFactory>().TryFindCryptoId(new Passphrase(passphrase), TypeMap.Resolve.New<IRuntimeFileInfo>(encryptedFileFullName), Resolve.CryptoFactory.OrderedIds) != Guid.Empty;
        }
    }
}