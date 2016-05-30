using Axantum.AxCrypt.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            BindPropertyChangedInternal(nameof(UserEmail), async (string userEmail) => { if (await ValidateAsync(nameof(UserEmail))) { Resolve.UserSettings.UserEmail = userEmail; } });
        }

        protected override Task<bool> ValidateAsync(string columnName)
        {
            switch (columnName)
            {
                case nameof(UserEmail):
                    if (String.IsNullOrEmpty(UserEmail) || !UserEmail.IsValidEmail())
                    {
                        ValidationError = (int)ViewModel.ValidationError.InvalidEmail;
                        return Task.FromResult(false);
                    }
                    return Task.FromResult(true);

                default:
                    return Task.FromResult(true);
            }
        }
    }
}