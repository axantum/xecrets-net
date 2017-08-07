using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using AxCrypt.Content;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Forms
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
    }
}