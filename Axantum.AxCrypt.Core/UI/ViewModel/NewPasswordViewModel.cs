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
using Axantum.AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class NewPasswordViewModel : ViewModelBase, INewPassword, IPasswordEntry
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
            PasswordText = passphrase ?? String.Empty;
            Verification = passphrase ?? String.Empty;
            FileName = String.IsNullOrEmpty(_encryptedFileFullName) ? String.Empty : New<IDataStore>(_encryptedFileFullName).Name;
            ShowPassword = New<UserSettings>().DisplayEncryptPassphrase;
        }

        private void BindPropertyChangedEvents()
        {
            BindPropertyChangedInternal(nameof(ShowPassword), (bool show) => New<UserSettings>().DisplayEncryptPassphrase = show);
        }

        public bool ShowPassword { get { return GetProperty<bool>(nameof(ShowPassword)); } set { SetProperty(nameof(ShowPassword), value); } }

        public string PasswordText { get { return GetProperty<string>(nameof(PasswordText)); } set { SetProperty(nameof(PasswordText), value); } }

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
                case nameof(PasswordText):
                    if (!IsPassphraseValidForFileIfAny(PasswordText, _encryptedFileFullName))
                    {
                        ValidationError = (int)ViewModel.ValidationError.WrongPassphrase;
                        return false;
                    }

                    if (!IsPassphrasePolicyValid(PasswordText))
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
            return String.Compare(PasswordText, Verification, StringComparison.Ordinal) == 0;
        }

        private static bool IsPassphraseValidForFileIfAny(string passphrase, string encryptedFileFullName)
        {
            if (String.IsNullOrEmpty(encryptedFileFullName))
            {
                return true;
            }
            return New<AxCryptFactory>().IsPassphraseValid(new Passphrase(passphrase), encryptedFileFullName);
        }

        private static bool IsPassphrasePolicyValid(string passphrase)
        {
            return New<PasswordStrengthEvaluator>().Evaluate(passphrase, New<UserSettings>().UserEmail).Strength > PasswordStrength.Unacceptable;
        }
    }
}