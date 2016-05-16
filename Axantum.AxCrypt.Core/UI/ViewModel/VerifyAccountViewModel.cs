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
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class VerifyAccountViewModel : ViewModelBase
    {
        public string UserEmail { get { return GetProperty<string>(nameof(UserEmail)); } set { SetProperty(nameof(UserEmail), value); } }

        public string VerificationCode { get { return GetProperty<string>(nameof(VerificationCode)); } set { SetProperty(nameof(VerificationCode), value); } }

        public string Passphrase { get { return GetProperty<string>(nameof(Passphrase)); } set { SetProperty(nameof(Passphrase), value); } }

        public string VerificationPassphrase { get { return GetProperty<string>(nameof(VerificationPassphrase)); } set { SetProperty(nameof(VerificationPassphrase), value); } }

        public bool ShowPassphrase { get { return GetProperty<bool>(nameof(ShowPassphrase)); } set { SetProperty(nameof(ShowPassphrase), value); } }

        public string ErrorMessage { get { return GetProperty<string>(nameof(ErrorMessage)); } set { SetProperty(nameof(ErrorMessage), value); } }

        public IAsyncAction VerifyAccount { get { return new AsyncDelegateAction<object>((o) => VerifyAccountActionAsync()); } }

        public VerifyAccountViewModel(EmailAddress emailAddress)
        {
            InitializePropertyValues(emailAddress);
            BindPropertyChangedEvents();
        }

        private void InitializePropertyValues(EmailAddress emailAddress)
        {
            UserEmail = emailAddress.Address;
            VerificationCode = String.Empty;
            Passphrase = String.Empty;
            VerificationPassphrase = String.Empty;
            ShowPassphrase = Resolve.UserSettings.DisplayEncryptPassphrase;
            ErrorMessage = String.Empty;
        }

        private void BindPropertyChangedEvents()
        {
            BindPropertyChangedInternal(nameof(ShowPassphrase), (bool show) => Resolve.UserSettings.DisplayEncryptPassphrase = show);
            BindPropertyChangedInternal(nameof(VerificationCode), (string code) => VerificationCode = code.Replace(" ", String.Empty));
        }

        protected override bool Validate(string columnName)
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

                case nameof(Passphrase):
                    return ValidatePassphrasePolicy(Passphrase);

                case nameof(VerificationPassphrase):
                    return String.Compare(Passphrase, VerificationPassphrase, StringComparison.Ordinal) == 0;

                case nameof(VerificationCode):
                    return VerificationCode.Length == 6 && VerificationCode.ToCharArray().All(c => Char.IsDigit(c));

                default:
                    return true;
            }
        }

        private static bool ValidatePassphrasePolicy(string passphrase)
        {
            if (passphrase.Length < 10)
            {
                return false;
            }

            return true;
        }

        private async Task VerifyAccountActionAsync()
        {
            LogOnIdentity identity = new LogOnIdentity(EmailAddress.Parse(UserEmail), Crypto.Passphrase.Create(Passphrase));
            IAccountService accountService = New<LogOnIdentity, IAccountService>(identity);

            try
            {
                await accountService.PasswordResetAsync(VerificationCode).Free();
                ErrorMessage = String.Empty;
            }
            catch (Exception ex)
            {
                New<IReport>().Exception(ex);
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }
                ErrorMessage = ex.Message;
            }
        }
    }
}