using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using AxCrypt.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt
{
    internal class SignUpSignIn
    {
        public string UserEmail { get; private set; }

        public bool StopAndExit { get; private set; }

        public SignUpSignIn(string userEmail)
        {
            UserEmail = userEmail;
        }

        public async Task DialogsAsync(Form parent)
        {
            SignUpSignInViewModel viewModel = new SignUpSignInViewModel()
            {
                UserEmail = UserEmail,
            };

            viewModel.BindPropertyChanged(nameof(viewModel.UserEmail), (string email) => UserEmail = email);
            viewModel.BindPropertyChanged(nameof(viewModel.StopAndExit), (bool stop) => StopAndExit = stop);

            viewModel.CreateAccount += (sender, e) =>
            {
                using (CreateNewAccountDialog dialog = new CreateNewAccountDialog(parent, String.Empty, EmailAddress.Parse(UserEmail)))
                {
                    DialogResult result = dialog.ShowDialog();
                    if (result != DialogResult.OK)
                    {
                        e.Cancel = true; ;
                    }
                }
            };

            viewModel.RequestEmail += (sender, e) =>
            {
                using (EmailDialog dialog = new EmailDialog(parent))
                {
                    dialog.EmailTextBox.Text = viewModel.UserEmail;
                    DialogResult result = dialog.ShowDialog(parent);
                    if (result != DialogResult.OK)
                    {
                        e.Cancel = true;
                        return;
                    }
                    viewModel.UserEmail = dialog.EmailTextBox.Text;
                }
            };

            viewModel.VerifyAccount += (sender, e) =>
            {
                VerifyAccountViewModel vav = new VerifyAccountViewModel(EmailAddress.Parse(viewModel.UserEmail));
                using (VerifyAccountDialog dialog = new VerifyAccountDialog(parent, vav))
                {
                    DialogResult result = dialog.ShowDialog(parent);
                    if (result != DialogResult.OK)
                    {
                        e.Cancel = true;
                    }
                }
            };

            await viewModel.DoDialogs.ExecuteAsync(null);
        }
    }
}