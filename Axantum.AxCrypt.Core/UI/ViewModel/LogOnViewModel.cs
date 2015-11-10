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
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class LogOnViewModel : ViewModelBase
    {
        private string _encryptedFileFullName;

        public LogOnViewModel(string encryptedFileFullName)
        {
            _encryptedFileFullName = encryptedFileFullName;
            InitializePropertyValues();
        }

        private void InitializePropertyValues()
        {
            Passphrase = String.Empty;
            FileName = String.IsNullOrEmpty(_encryptedFileFullName) ? String.Empty : New<IDataStore>(_encryptedFileFullName).Name;
        }

        public bool ShowPassphrase { get { return GetProperty<bool>(nameof(ShowPassphrase)); } set { SetProperty(nameof(ShowPassphrase), value); } }

        public string Passphrase { get { return GetProperty<string>(nameof(Passphrase)); } set { SetProperty(nameof(Passphrase), value); } }

        public string FileName { get { return GetProperty<string>(nameof(FileName)); } set { SetProperty(nameof(FileName), value); } }

        protected override bool Validate(string columnName)
        {
            switch (columnName)
            {
                case nameof(Passphrase):
                    if (!IsPassphraseValidForFileIfAny(Passphrase, _encryptedFileFullName))
                    {
                        ValidationError = (int)ViewModel.ValidationError.WrongPassphrase;
                        return false;
                    }
                    bool isKnownIdentity = IsKnownIdentity();
                    if (String.IsNullOrEmpty(_encryptedFileFullName) && !isKnownIdentity)
                    {
                        ValidationError = (int)ViewModel.ValidationError.WrongPassphrase;
                        return false;
                    }
                    return true;

                default:
                    throw new ArgumentException("Cannot validate property.", columnName);
            }
        }

        private static bool IsPassphraseValidForFileIfAny(string passphrase, string encryptedFileFullName)
        {
            if (String.IsNullOrEmpty(encryptedFileFullName))
            {
                return true;
            }
            IEnumerable<DecryptionParameter> decryptionParameters = DecryptionParameter.CreateAll(new Passphrase[] { new Passphrase(passphrase) }, new IAsymmetricPrivateKey[0], Resolve.CryptoFactory.OrderedIds);
            return New<AxCryptFactory>().FindDecryptionParameter(decryptionParameters, New<IDataStore>(encryptedFileFullName)) != null;
        }

        private bool IsKnownIdentity()
        {
            SymmetricKeyThumbprint thumbprint = new Passphrase(Passphrase).Thumbprint;
            Passphrase passphrase = Resolve.FileSystemState.KnownPassphrases.FirstOrDefault(id => id.Thumbprint == thumbprint);
            if (passphrase != null)
            {
                return true;
            }
            return false;
        }
    }
}