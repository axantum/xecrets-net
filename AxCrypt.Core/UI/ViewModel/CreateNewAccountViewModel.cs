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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.Service;
using AxCrypt.Core.Session;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.UI.ViewModel
{
    /// <summary>
    /// Request user name and passphrase, get a public key pair from the server, or generate
    /// locally, and store the result locally.
    /// </summary>
    public class CreateNewAccountViewModel : ViewModelBase, INewPassword
    {
        public string PasswordText { get { return GetProperty<string>(nameof(PasswordText)); } set { SetProperty(nameof(PasswordText), value); } }

        public string Verification { get { return GetProperty<string>(nameof(Verification)); } set { SetProperty(nameof(Verification), value); } }

        public bool ShowPassword { get { return GetProperty<bool>(nameof(ShowPassword)); } set { SetProperty(nameof(ShowPassword), value); } }

        public string UserEmail { get { return GetProperty<string>(nameof(UserEmail)); } set { SetProperty(nameof(UserEmail), value); } }

        public IAsyncAction CreateAccount { get { return new AsyncDelegateAction<object>((o) => CreateAccountAction()); } }

        public CreateNewAccountViewModel(string passphrase, EmailAddress email)
        {
            InitializePropertyValues(passphrase, email);
            BindPropertyChangedEvents();
        }

        private void InitializePropertyValues(string passphrase, EmailAddress email)
        {
            PasswordText = passphrase ?? String.Empty;
            Verification = passphrase ?? String.Empty;

            UserEmail = email.Address;
            ShowPassword = Resolve.UserSettings.DisplayEncryptPassphrase;
        }

        private void BindPropertyChangedEvents()
        {
            BindPropertyChangedInternal(nameof(ShowPassword), (bool show) => Resolve.UserSettings.DisplayEncryptPassphrase = show);
            BindPropertyChangedInternal(nameof(UserEmail), async (string userEmail) => { if (await ValidateAsync(nameof(UserEmail))) { Resolve.UserSettings.UserEmail = userEmail; } });
        }

        protected override Task<bool> ValidateAsync(string columnName)
        {
            switch (columnName)
            {
                case nameof(UserEmail):
                    if (!UserEmail.IsValidEmail())
                    {
                        ValidationError = (int)ViewModel.ValidationError.InvalidEmail;
                        return Task.FromResult(false);
                    }
                    return Task.FromResult(true);

                case nameof(Verification):
                    if (String.IsNullOrEmpty(Verification))
                    {
                        return Task.FromResult(false);
                    }
                    return Task.FromResult(String.Compare(PasswordText, Verification, StringComparison.Ordinal) == 0);

                case nameof(PasswordText):
                    return Task.FromResult(ValidatePassphrasePolicy(PasswordText));

                default:
                    return Task.FromResult(true);
            }
        }

        private static bool ValidatePassphrasePolicy(string passphrase)
        {
            return New<PasswordStrengthEvaluator>().Evaluate(passphrase).Strength > PasswordStrength.Unacceptable;
        }

        private async Task<object?> CreateAccountAction()
        {
            if (string.IsNullOrEmpty(UserEmail))
            {
                return null;
            }

            AccountStorage accountStorage = new AccountStorage(New<LogOnIdentity, IAccountService>(new LogOnIdentity(EmailAddress.Parse(UserEmail), new Passphrase(PasswordText))));
            UserKeyPair userKeys = new UserKeyPair(EmailAddress.Parse(UserEmail), New<INow>().Utc, New<KeyPairService>().New());
            await accountStorage.ImportAsync(userKeys);

            return null;
        }
    }
}
