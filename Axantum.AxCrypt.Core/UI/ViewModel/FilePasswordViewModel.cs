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

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class FilePasswordViewModel : ViewModelBase, IPasswordEntry
    {
        private string _encryptedFileFullName;

        public FilePasswordViewModel(string encryptedFileFullName)
        {
            _encryptedFileFullName = encryptedFileFullName;
            InitializePropertyValues();
            BindPropertyChangedEvents();
        }

        private void InitializePropertyValues()
        {
            ShowPassword = New<UserSettings>().DisplayDecryptPassphrase;
            PasswordText = string.Empty;
            FileName = string.IsNullOrEmpty(_encryptedFileFullName) ? string.Empty : New<IDataStore>(_encryptedFileFullName).Name;
            IsLegacyFile = IsLegacyFileInternal(_encryptedFileFullName);
            KeyFileName = string.Empty;
        }

        private void BindPropertyChangedEvents()
        {
            BindPropertyChangedInternal(nameof(ShowPassword), (bool show) => New<UserSettings>().DisplayDecryptPassphrase = show);
        }

        public bool ShowPassword { get { return GetProperty<bool>(nameof(ShowPassword)); } set { SetProperty(nameof(ShowPassword), value); } }

        public string PasswordText { get { return GetProperty<string>(nameof(PasswordText)); } set { SetProperty(nameof(PasswordText), value); } }

        public string FileName { get { return GetProperty<string>(nameof(FileName)); } set { SetProperty(nameof(FileName), value); } }

        public bool IsLegacyFile { get { return GetProperty<bool>(nameof(IsLegacyFile)); } set { SetProperty(nameof(IsLegacyFile), value); } }

        public string KeyFileName { get { return GetProperty<string>(nameof(KeyFileName)); } set { SetProperty(nameof(KeyFileName), value); } }

        public Passphrase Passphrase
        {
            get
            {
                if (string.IsNullOrEmpty(KeyFileName))
                {
                    return Passphrase.Create(PasswordText);
                }

                IDataStore keyFile = New<IDataStore>(KeyFileName);
                if (!keyFile.IsAvailable)
                {
                    return Passphrase.Empty;
                }

                byte[] extra;
                using (Stream stream = New<IDataStore>(KeyFileName).OpenRead())
                {
                    extra = stream.ToArray();
                }
                return Passphrase.Create(PasswordText, extra);
            }
        }

        protected override Task<bool> ValidateAsync(string columnName)
        {
            return Task.FromResult(ValidateInternal(columnName));
        }

        private bool ValidateInternal(string columnName)
        {
            switch (columnName)
            {
                case nameof(KeyFileName):
                    if (string.IsNullOrEmpty(KeyFileName))
                    {
                        return true;
                    }
                    return ValidateKeyFile();

                case nameof(PasswordText):
                    if (!string.IsNullOrEmpty(KeyFileName) && !ValidateKeyFile())
                    {
                        return false;
                    }

                    if (New<KnownIdentities>().DefaultEncryptionIdentity.Passphrase == Passphrase)
                    {
                        ValidationError = (int)ViewModel.ValidationError.SamePasswordAlreadySignedIn;
                        return false;
                    }
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

        private bool ValidateKeyFile()
        {
            if (!New<IDataStore>(KeyFileName).IsAvailable)
            {
                ValidationError = (int)ViewModel.ValidationError.KeyFileInaccessible;
                return false;
            }
            try
            {
                using (Stream stream = New<IDataStore>(KeyFileName).OpenRead())
                {
                }
            }
            catch (IOException ioex)
            {
                New<IReport>().Exception(ioex);
                ValidationError = (int)ViewModel.ValidationError.KeyFileInaccessible;
                return false;
            }
            return true;
        }

        private static bool IsLegacyFileInternal(string encryptedFileFullName)
        {
            if (string.IsNullOrEmpty(encryptedFileFullName))
            {
                return false;
            }

            return New<IDataStore>(encryptedFileFullName).IsLegacyV1();
        }

        private static bool IsPassphraseValidForFileIfAny(Passphrase passphrase, string encryptedFileFullName)
        {
            if (string.IsNullOrEmpty(encryptedFileFullName))
            {
                return true;
            }
            return New<AxCryptFactory>().IsPassphraseValid(passphrase, encryptedFileFullName);
        }

        private bool IsKnownIdentity()
        {
            SymmetricKeyThumbprint thumbprint = Passphrase.Thumbprint;
            Passphrase passphrase = Resolve.FileSystemState.KnownPassphrases.FirstOrDefault(id => id.Thumbprint == thumbprint);
            if (passphrase != null)
            {
                return true;
            }
            return false;
        }
    }
}