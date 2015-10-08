using Axantum.AxCrypt.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class AccountEmailViewModel : ViewModelBase
    {
        public string UserEmail { get { return GetProperty<string>(nameof(UserEmail)); } set { SetProperty(nameof(UserEmail), value); } }

        public AccountEmailViewModel()
        {
            InitializePropertyValues();
            BindPropertyChangedEvents();
        }

        private void InitializePropertyValues()
        {
            UserEmail = String.Empty;
        }

        private void BindPropertyChangedEvents()
        {
            BindPropertyChangedInternal(nameof(UserEmail), (string userEmail) => { if (String.IsNullOrEmpty(Validate(nameof(UserEmail)))) { Resolve.UserSettings.UserEmail = userEmail; } });
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
                case nameof(UserEmail):
                    if (String.IsNullOrEmpty(UserEmail) || !UserEmail.IsValidEmail())
                    {
                        ValidationError = (int)ViewModel.ValidationError.InvalidEmail;
                        return false;
                    }
                    return true;

                default:
                    return true;
            }
        }
    }
}