﻿#region Coypright and License

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
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class NewPasswordViewModel : ViewModelBase, INewPassword
    {
        private string _encryptedFileFullName;

        public NewPasswordViewModel(string passphrase, string encryptedFileFullName)
        {
            _encryptedFileFullName = encryptedFileFullName;
            InitializePropertyValues(passphrase);
            BindPropertyChangedEvents();
        }

        private void InitializePropertyValues(string passphrase)
        {
            Password = passphrase ?? String.Empty;
            Verification = passphrase ?? String.Empty;
            FileName = String.IsNullOrEmpty(_encryptedFileFullName) ? String.Empty : New<IDataStore>(_encryptedFileFullName).Name;
            ShowPassword = New<UserSettings>().DisplayEncryptPassphrase;
        }

        private void BindPropertyChangedEvents()
        {
            BindPropertyChangedInternal(nameof(ShowPassword), (bool show) => New<UserSettings>().DisplayEncryptPassphrase = show);
        }

        public bool ShowPassword { get { return GetProperty<bool>(nameof(ShowPassword)); } set { SetProperty(nameof(ShowPassword), value); } }

        public string Password { get { return GetProperty<string>(nameof(Password)); } set { SetProperty(nameof(Password), value); } }

        public string Verification { get { return GetProperty<string>(nameof(Verification)); } set { SetProperty(nameof(Verification), value); } }

        public string FileName { get { return GetProperty<string>(nameof(FileName)); } set { SetProperty(nameof(FileName), value); } }

        protected override Task<bool> ValidateAsync(string columnName)
        {
            return Task.FromResult(ValidateInternal(columnName));
        }

        private bool ValidateInternal(string columnName)
        {
            switch (columnName)
            {
                case nameof(Password):
                    if (!IsPassphraseValidForFileIfAny(Password, _encryptedFileFullName))
                    {
                        ValidationError = (int)ViewModel.ValidationError.WrongPassphrase;
                        return false;
                    }

                    if (!IsPassphrasePolicyValid(Password))
                    {
                        ValidationError = (int)ViewModel.ValidationError.WrongPassphrase;
                        return false;
                    }

                    break;

                case nameof(Verification):
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
            return String.Compare(Password, Verification, StringComparison.Ordinal) == 0;
        }

        private static bool IsPassphraseValidForFileIfAny(string passphrase, string encryptedFileFullName)
        {
            if (String.IsNullOrEmpty(encryptedFileFullName))
            {
                return true;
            }
            IDataStore encryptedStore = New<IDataStore>(encryptedFileFullName);
            IEnumerable<DecryptionParameter> decryptionParameters = encryptedStore.DecryptionParameters(new Passphrase(passphrase), new IAsymmetricPrivateKey[0]);
            return New<AxCryptFactory>().FindDecryptionParameter(decryptionParameters, encryptedStore) != null;
        }

        private static bool IsPassphrasePolicyValid(string passphrase)
        {
            return New<PasswordStrengthEvaluator>().Evaluate(passphrase).Strength > PasswordStrength.Unacceptable;
        }
    }
}