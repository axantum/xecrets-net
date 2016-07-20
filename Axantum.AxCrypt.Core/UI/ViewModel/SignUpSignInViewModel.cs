using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public sealed class SignUpSignInViewModel : ViewModelBase
    {
        public string UserEmail { get { return GetProperty<string>(nameof(UserEmail)); } set { SetProperty(nameof(UserEmail), value); } }

        public bool StopAndExit { get { return GetProperty<bool>(nameof(StopAndExit)); } set { SetProperty(nameof(StopAndExit), value); } }

        public IAsyncAction DoDialogs { get { return new AsyncDelegateAction<object>((o) => DoDialogsActionAsync()); } }

        public SignUpSignInViewModel()
        {
            InitializePropertyValues();
            BindPropertyChangedEvents();
            SubscribeToModelEvents();
        }

        private void InitializePropertyValues()
        {
        }

        private void BindPropertyChangedEvents()
        {
        }

        private void SubscribeToModelEvents()
        {
        }

        private Task DoDialogsActionAsync()
        {
            throw new NotImplementedException();
        }
    }
}