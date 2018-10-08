using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Forms.Style;
using AxCrypt.Content;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt
{
    internal class SignUpSignIn
    {
        public string UserEmail { get; set; }

        public bool StopAndExit { get; set; }

        public ApiVersion Version { get; set; }

        public SignUpSignIn()
        {
        }

        public async Task DialogsAsync(Form parent, ISignIn signingInState)
        {
            SignupSignInViewModel viewModel = new SignupSignInViewModel(signingInState, new NameOf(nameof(Texts.WelcomeToAxCrypt)), new NameOf(nameof(Texts.MessageAskAboutStartTrialPremium)))
            {
                UserEmail = UserEmail,
                Version = Version,
            };

            viewModel.BindPropertyChanged(nameof(viewModel.UserEmail), (string email) => UserEmail = email);
            viewModel.BindPropertyChanged(nameof(viewModel.StopAndExit), (bool stop) => StopAndExit = stop);
            viewModel.BindPropertyChanged(nameof(viewModel.TopControlsEnabled), (bool enabled) => SetTopControlsEnabled(parent, enabled));

            viewModel.CreateAccount = (e) =>
            {
                using (CreateNewAccountDialog dialog = new CreateNewAccountDialog(parent, String.Empty, EmailAddress.Parse(UserEmail)))
                {
                    DialogResult result = dialog.ShowDialog();
                    if (result != DialogResult.OK)
                    {
                        e.Cancel = true; ;
                    }
                }
                return Task.FromResult<object>(null);
            };

            viewModel.RequestEmail = (e) =>
            {
                using (EmailDialog dialog = new EmailDialog(parent))
                {
                    dialog.EmailTextBox.Text = viewModel.UserEmail;
                    DialogResult result = dialog.ShowDialog(parent);
                    if (result != DialogResult.OK)
                    {
                        e.Cancel = true;
                        return Task.FromResult<object>(null);
                    }
                    viewModel.UserEmail = dialog.EmailTextBox.Text;
                }
                return Task.FromResult<object>(null);
            };

            viewModel.VerifyAccount = (e) =>
            {
                VerifyAccountViewModel vav = new VerifyAccountViewModel(EmailAddress.Parse(viewModel.UserEmail));
                vav.BindPropertyChanged<bool>(nameof(vav.AlreadyVerified), (verified) => viewModel.AlreadyVerified = verified);

                using (VerifyAccountDialog dialog = new VerifyAccountDialog(parent, vav))
                {
                    DialogResult result = dialog.ShowDialog(parent);
                    if (result != DialogResult.OK)
                    {
                        e.Cancel = true;
                    }
                }
                return Task.FromResult<object>(null);
            };

            viewModel.SignInCommandAsync = signingInState.SignIn;

            viewModel.RestoreWindow = () =>
            {
                if (New<UserSettings>().RestoreFullWindow)
                {
                    Styling.RestoreWindowWithFocus(parent);
                }
                return Task.FromResult<object>(null);
            };

            await viewModel.DoAll.ExecuteAsync(null);
        }

        private static void SetTopControlsEnabled(Form parent, bool enabled)
        {
            foreach (Control control in parent.Controls)
            {
                control.Enabled = enabled;
            }
            parent.Cursor = enabled ? Cursors.Default : Cursors.WaitCursor;
        }
    }
}