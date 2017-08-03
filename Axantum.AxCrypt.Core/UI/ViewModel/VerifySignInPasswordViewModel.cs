using Axantum.AxCrypt.Core.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class VerifySignInPasswordViewModel : ViewModelBase
    {
        private readonly LogOnIdentity _identity;

        public bool ShowPassword { get { return GetProperty<bool>(nameof(ShowPassword)); } set { SetProperty(nameof(ShowPassword), value); } }

        public string Password { get { return GetProperty<string>(nameof(Password)); } set { SetProperty(nameof(Password), value); } }

        public VerifySignInPasswordViewModel(LogOnIdentity identity)
        {
            _identity = identity;

            BindPropertyChangedEvents();
            InitializePropertyValues();
        }

        private void BindPropertyChangedEvents()
        {
            BindPropertyChangedInternal(nameof(ShowPassword), (bool show) => New<UserSettings>().DisplayDecryptPassphrase = show);
        }

        private void InitializePropertyValues()
        {
            Password = String.Empty;
            ShowPassword = New<UserSettings>().DisplayDecryptPassphrase;
        }

        protected override Task<bool> ValidateAsync(string columnName)
        {
            return Task.FromResult(ValidateInternal(columnName));
        }

        private bool ValidateInternal(string columnName)
        {
            switch (columnName)
            {
                case nameof(Password):
                    return ValidatePassphrase();

                case nameof(ShowPassword):
                    return true;

                default:
                    throw new ArgumentException("Cannot validate property.", columnName);
            }
        }

        private bool ValidatePassphrase()
        {
            Passphrase passphrase = new Passphrase(Password);
            if (passphrase == _identity.Passphrase)
            {
                return true;
            }

            ValidationError = (int)ViewModel.ValidationError.WrongPassphrase;
            return false;
        }
    }
}