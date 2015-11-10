using Axantum.AxCrypt.Core.Extensions;
using System;
using System.Collections.Generic;
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
            BindPropertyChangedInternal(nameof(UserEmail), (string userEmail) => { if (Validate(nameof(UserEmail))) { Resolve.UserSettings.UserEmail = userEmail; } });
        }

        protected override bool Validate(string columnName)
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