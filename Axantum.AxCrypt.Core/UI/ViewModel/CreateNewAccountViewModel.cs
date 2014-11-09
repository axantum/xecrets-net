using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    /// <summary>
    /// Request user name and passphrase, get a public key pair from the server, or generate
    /// locally, and store the result locally.
    /// </summary>
    public class CreateNewAccountViewModel : ViewModelBase
    {
        public string Passphrase { get { return GetProperty<string>("Passphrase"); } set { SetProperty("Passphrase", value); } }

        public string Verification { get { return GetProperty<string>("Verification"); } set { SetProperty("Verification", value); } }

        public bool ShowPassphrase { get { return GetProperty<bool>("ShowPassphrase"); } set { SetProperty("ShowPassphrase", value); } }

        public string UserEmail { get { return GetProperty<string>("UserEmail"); } set { SetProperty("UserEmail", value); } }

        public IAction CreateAccount { get { return new DelegateAction<object>((o) => CreateAccountAction()); } }

        private UserAsymmetricKeysStore _keysStore;

        public CreateNewAccountViewModel(UserAsymmetricKeysStore keysStore, string passphrase)
        {
            _keysStore = keysStore;

            InitializePropertyValues(passphrase);
            BindPropertyChangedEvents();
        }

        private void InitializePropertyValues(string passphrase)
        {
            Passphrase = passphrase ?? String.Empty;
            Verification = passphrase ?? String.Empty;

            UserEmail = String.Empty;
            ShowPassphrase = Resolve.UserSettings.DisplayEncryptPassphrase;
        }

        private void BindPropertyChangedEvents()
        {
            BindPropertyChangedInternal("ShowPassphrase", (bool show) => Resolve.UserSettings.DisplayEncryptPassphrase = show);
        }

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
                case "UserEmail":
                    if (!UserEmail.IsValidEmailOrEmpty())
                    {
                        ValidationError = (int)ViewModel.ValidationError.InvalidEmail;
                        return false;
                    }
                    return true;

                case "Verification":
                    if (String.IsNullOrEmpty(Verification))
                    {
                        return false;
                    }
                    return String.Compare(Passphrase, Verification, StringComparison.Ordinal) == 0;

                case "Passphrase":
                    return !String.IsNullOrEmpty(Passphrase);

                default:
                    return true;
            }
        }

        private object CreateAccountAction()
        {
            if (String.IsNullOrEmpty(UserEmail))
            {
                return null;
            }
            _keysStore.Create(new EmailAddress(UserEmail), new Passphrase(Passphrase));

            return null;
        }
    }
}