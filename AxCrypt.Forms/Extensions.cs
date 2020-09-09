using AxCrypt.Core.UI;
using AxCrypt.Core.UI.ViewModel;
using AxCrypt.Content;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Forms
{
    public static class Extensions
    {
        public static async Task WithWaitCursorAsync(this Control control, Func<Task> action, Action final)
        {
            try
            {
                control.UseWaitCursor = true;
                await action();
            }
            finally
            {
                control.UseWaitCursor = false;
                final();
            }
        }

        public static void WithWaitCursor(this Control control, Action action, Action final)
        {
            if (control == null)
            {
                throw new ArgumentNullException(nameof(control));
            }
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            if (final == null)
            {
                throw new ArgumentNullException(nameof(final));
            }

            try
            {
                control.UseWaitCursor = true;
                action();
            }
            finally
            {
                control.UseWaitCursor = false;
                final();
            }
        }

        public static Task<DialogResult> ShowDialogAsync(this Form self, Form parent)
        {
            if (self == null)
            {
                throw new ArgumentNullException("self");
            }

            TaskCompletionSource<DialogResult> completion = new TaskCompletionSource<DialogResult>();
            self.BeginInvoke(new Action(() => completion.SetResult(self.ShowDialog(parent))));

            return completion.Task;
        }

        public static async Task<bool> ChangePasswordDialogAsync(this Form parent, ManageAccountViewModel viewModel)
        {
            NewPasswordViewModel newPasswordviewModel = new NewPasswordViewModel(String.Empty, String.Empty);
            using (NewPassphraseDialog dialog = new NewPassphraseDialog(parent, Texts.ChangePassphraseDialogTitle, newPasswordviewModel))
            {
                DialogResult dialogResult = dialog.ShowDialog(parent);
                if (dialogResult != DialogResult.OK || newPasswordviewModel.PasswordText.Length == 0)
                {
                    return false;
                }
            }
            viewModel.ChangePasswordCompleteAsync = async (success) => { if (!success) await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.MessageErrorTitle, Texts.ChangePasswordError); };
            await viewModel.ChangePassphraseAsync.ExecuteAsync(newPasswordviewModel.PasswordText);
            return viewModel.LastChangeStatus;
        }

        public static bool ShowVerifySignInPasswordDialog(this Form parent, string description)
        {
            VerifySignInPasswordViewModel viewModel = new VerifySignInPasswordViewModel(New<KnownIdentities>().DefaultEncryptionIdentity);
            using (VerifySignInPasswordDialog dialog = new VerifySignInPasswordDialog(parent, viewModel, description))
            {
                DialogResult dr = dialog.ShowDialog(parent);
                return dr == DialogResult.OK;
            }
        }
    }
}