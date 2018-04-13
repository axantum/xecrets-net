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
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class VerifyAccountViewModel : ViewModelBase, INewPassword, IPasswordEntry
    {
        public string UserEmail { get { return GetProperty<string>(nameof(UserEmail)); } set { SetProperty(nameof(UserEmail), value); } }

        public string VerificationCode { get { return GetProperty<string>(nameof(VerificationCode)); } set { SetProperty(nameof(VerificationCode), value); } }

        public string PasswordText { get { return GetProperty<string>(nameof(PasswordText)); } set { SetProperty(nameof(PasswordText), value); } }

        public string Verification { get { return GetProperty<string>(nameof(Verification)); } set { SetProperty(nameof(Verification), value); } }

        public bool ShowPassword { get { return GetProperty<bool>(nameof(ShowPassword)); } set { SetProperty(nameof(ShowPassword), value); } }

        public bool AlreadyVerified { get { return GetProperty<bool>(nameof(AlreadyVerified)); } set { SetProperty(nameof(AlreadyVerified), value); } }

        public string ErrorMessage { get { return GetProperty<string>(nameof(ErrorMessage)); } set { SetProperty(nameof(ErrorMessage), value); } }

        public IAsyncAction CheckAccountStatus { get { return new AsyncDelegateAction<object>((o) => CheckAccountActionAsync()); } }

        public IAsyncAction VerifyAccount { get { return new AsyncDelegateAction<object>((o) => VerifyAccountActionAsync()); } }

        public VerifyAccountViewModel(EmailAddress emailAddress)
        {
            InitializePropertyValues(emailAddress);
            BindPropertyChangedEvents();
        }

        private void InitializePropertyValues(EmailAddress emailAddress)
        {
            UserEmail = emailAddress.Address;
            VerificationCode = string.Empty;
            PasswordText = string.Empty;
            Verification = string.Empty;
            ShowPassword = Resolve.UserSettings.DisplayEncryptPassphrase;
            ErrorMessage = string.Empty;
        }

        private void BindPropertyChangedEvents()
        {
            BindPropertyChangedInternal(nameof(ShowPassword), (bool show) => Resolve.UserSettings.DisplayEncryptPassphrase = show);
            BindPropertyChangedInternal(nameof(VerificationCode), (string code) => VerificationCode = code.Replace(" ", String.Empty));
        }

        protected override Task<bool> ValidateAsync(string columnName)
        {
            return Task.FromResult(ValidateInternal(columnName));
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

                case nameof(PasswordText):
                    return ValidatePassphrasePolicy(PasswordText);

                case nameof(Verification):
                    return String.Compare(PasswordText, Verification, StringComparison.Ordinal) == 0;

                case nameof(VerificationCode):
                    return VerificationCode.Length == 6 && VerificationCode.ToCharArray().All(c => Char.IsDigit(c));

                case nameof(ErrorMessage):
                    return ErrorMessage.Length == 0;

                default:
                    return true;
            }
        }

        private static bool ValidatePassphrasePolicy(string passphrase)
        {
            return New<PasswordStrengthEvaluator>().Evaluate(passphrase).Strength > PasswordStrength.Unacceptable;
        }

        private async Task CheckAccountActionAsync()
        {
            try
            {
                AccountStatus status = await New<LogOnIdentity, IAccountService>(LogOnIdentity.Empty).Refresh().StatusAsync(EmailAddress.Parse(UserEmail));
                if (status == AccountStatus.Verified)
                {
                    AlreadyVerified = true;
                }
            }
            catch (Exception ex)
            {
                New<IReport>().Exception(ex);
                throw;
            }
        }

        private async Task VerifyAccountActionAsync()
        {
            LogOnIdentity identity = new LogOnIdentity(EmailAddress.Parse(UserEmail), Crypto.Passphrase.Create(PasswordText));
            IAccountService accountService = New<LogOnIdentity, IAccountService>(identity);

            try
            {
                await accountService.PasswordResetAsync(VerificationCode);
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                New<IReport>().Exception(ex);
                ErrorMessage = ex.Innermost().Message;
            }
        }
    }
}