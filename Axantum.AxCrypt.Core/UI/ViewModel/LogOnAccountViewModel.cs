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
    /// Log On (enable default encryption key). If an e-mail is provided, an attempt is made to log on to the Internet
    /// server and synchronize keys. If that fails, a locally accessible key-pair is required.
    /// If no e-mail is provided, classic mode encryption is attempted, if this is enabled by the current feature policy.
    /// </summary>
    public class LogOnAccountViewModel : ViewModelBase
    {
        public LogOnAccountViewModel(string userEmail)
        {
            InitializePropertyValues(userEmail);
            BindPropertyChangedEvents();
        }

        private void InitializePropertyValues(string userEmail)
        {
            UserEmail = userEmail;
            Passphrase = String.Empty;
            ShowPassphrase = Resolve.UserSettings.DisplayEncryptPassphrase;
            ShowEmail = Resolve.AsymmetricKeysStore.HasStore;
        }

        private void BindPropertyChangedEvents()
        {
            BindPropertyChangedInternal("ShowPassphrase", (bool show) => Resolve.UserSettings.DisplayEncryptPassphrase = show);
            BindPropertyChangedInternal("ShowEmail", (bool show) => { if (!ShowEmail) UserEmail = String.Empty; });
        }

        public bool ShowPassphrase { get { return GetProperty<bool>("ShowPassphrase"); } set { SetProperty("ShowPassphrase", value); } }

        public string Passphrase { get { return GetProperty<string>("Passphrase"); } set { SetProperty("Passphrase", value); } }

        public string UserEmail { get { return GetProperty<string>("UserEmail"); } set { SetProperty("UserEmail", value); } }

        public bool ShowEmail { get { return GetProperty<bool>("ShowEmail"); } private set { SetProperty("ShowEmail", value); } }

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
                    if (!ShowEmail)
                    {
                        return true;
                    }
                    if (!UserEmail.IsValidEmailOrEmpty())
                    {
                        ValidationError = (int)ViewModel.ValidationError.InvalidEmail;
                        return false;
                    }
                    return true;

                case "Passphrase":
                    if (ShowEmail && UserEmail.Length > 0 && UserEmail.IsValidEmail())
                    {
                        return IsValidAccountLogOn();
                    }
                    if (ShowEmail && UserEmail.Length > 0)
                    {
                        return true;
                    }
                    bool isKnownPassphrase = IsKnownPassphrase();
                    if (!isKnownPassphrase)
                    {
                        ValidationError = (int)ViewModel.ValidationError.WrongPassphrase;
                        return false;
                    }
                    return true;

                case "ShowPassphrase":
                case "ShowEmail":
                    return true;

                default:
                    throw new ArgumentException("Cannot validate property.", columnName);
            }
        }

        private bool IsValidAccountLogOn()
        {
            return Resolve.AsymmetricKeysStore.IsValidAccountLogOn(new EmailAddress(UserEmail), new Passphrase(Passphrase));
        }

        private bool IsKnownPassphrase()
        {
            SymmetricKeyThumbprint thumbprint = new Passphrase(Passphrase).Thumbprint;
            Passphrase knownPassphrase = Resolve.FileSystemState.KnownPassphrases.FirstOrDefault(id => id.Thumbprint == thumbprint);
            if (knownPassphrase != null)
            {
                return true;
            }
            return false;
        }
    }
}