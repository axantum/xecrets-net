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

        public bool ShowPassphrase { get { return GetProperty<bool>(nameof(ShowPassphrase)); } set { SetProperty(nameof(ShowPassphrase), value); } }

        public string Passphrase { get { return GetProperty<string>(nameof(Passphrase)); } set { SetProperty(nameof(Passphrase), value); } }

        public VerifySignInPasswordViewModel(LogOnIdentity identity)
        {
            _identity = identity;

            BindPropertyChangedEvents();
            InitializePropertyValues();
        }

        private void BindPropertyChangedEvents()
        {
            BindPropertyChangedInternal(nameof(ShowPassphrase), (bool show) => New<IUserSettings>().DisplayDecryptPassphrase = show);
        }

        private void InitializePropertyValues()
        {
            Passphrase = String.Empty;
            ShowPassphrase = New<IUserSettings>().DisplayDecryptPassphrase;
        }

        protected override bool Validate(string columnName)
        {
            switch (columnName)
            {
                case nameof(Passphrase):
                    return ValidatePassphrase();

                case nameof(ShowPassphrase):
                    return true;

                default:
                    throw new ArgumentException("Cannot validate property.", columnName);
            }
        }

        private bool ValidatePassphrase()
        {
            Passphrase passphrase = new Passphrase(Passphrase);
            if (passphrase == _identity.Passphrase)
            {
                return true;
            }

            ValidationError = (int)ViewModel.ValidationError.WrongPassphrase;
            return false;
        }
    }
}