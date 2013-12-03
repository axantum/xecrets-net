using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class NewPassphraseViewModel : ViewModelBase
    {
        public NewPassphraseViewModel(string defaultIdentityName)
        {
            bool defaultIdentityKnown = Instance.FileSystemState.Identities.Any(identity => String.Compare(identity.Name, Environment.UserName, StringComparison.OrdinalIgnoreCase) == 0);
            IdentityName = defaultIdentityKnown ? defaultIdentityName : String.Empty;
        }

        public string IdentityName { get { return GetProperty<string>("IdentityName"); } set { SetProperty("IdentityName", value); } }

        public bool ShowPassphrase { get { return GetProperty<bool>("ShowPassphrase"); } set { SetProperty("ShowPassphrase", value); } }

        public string Passphrase { get { return GetProperty<string>("Passphrase"); } set { SetProperty("Passphrase", value); } }

        public string Verification { get { return GetProperty<string>("Verification"); } set { SetProperty("Verification", value); } }

        public ValidationError ValidationError { get { return GetProperty<ValidationError>("ValidationError"); } set { SetProperty("ValidationError", value); } }

        public override string this[string propertyName]
        {
            get
            {
                string error = base[propertyName];
                if (!String.IsNullOrEmpty(error))
                {
                    return error;
                }
                switch (propertyName)
                {
                    case "Passphrase":
                    case "Verification":
                        if (String.Compare(Passphrase, Verification, StringComparison.Ordinal) == 0)
                        {
                            return String.Empty;
                        }
                        ValidationError = ValidationError.VerificationPassphraseWrong;
                        break;

                    case "IdentityName":
                        if (!Instance.FileSystemState.Identities.Any(i => i.Name == IdentityName))
                        {
                            return String.Empty;
                        }
                        ValidationError = ValidationError.IdentityExistsAlready;
                        break;

                    default:
                        return String.Empty;
                }
                return ValidationError.ToString();
            }
        }
    }
}