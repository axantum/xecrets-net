using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Axantum.AxCrypt.Forms.Implementation
{
    internal class Popup : IPopup
    {
        private Form _parent;

        public Popup(Form parent)
        {
            _parent = parent;
        }

        public PopupButtons Show(PopupButtons buttons, string title, string message)
        {
            DialogResult result;
            using (MessageDialog dialog = new MessageDialog(_parent))
            {
                if (!buttons.HasFlag(PopupButtons.Cancel))
                {
                    dialog.HideCancel();
                }
                if (!buttons.HasFlag(PopupButtons.Exit))
                {
                    dialog.HideExit();
                }
                dialog.HideDontShowAgain();

                dialog.Text = title;
                dialog.Message.Text = message;
                result = dialog.ShowDialog(_parent);
            }

            switch (result)
            {
                case DialogResult.OK:
                    return PopupButtons.Ok;

                case DialogResult.Cancel:
                    return PopupButtons.Cancel;

                case DialogResult.Abort:
                    return PopupButtons.Exit;

                default:
                    throw new InvalidOperationException($"Unexpected result from dialog: {result}");
            }
        }

        public PopupButtons Show(PopupButtons buttons, string title, string message, out bool dontShowAgainStatus)
        {
            DialogResult result;
            using (MessageDialog dialog = new MessageDialog(_parent))
            {
                if (!buttons.HasFlag(PopupButtons.Cancel))
                {
                    dialog.HideCancel();
                }
                if (!buttons.HasFlag(PopupButtons.Exit))
                {
                    dialog.HideExit();
                }

                dialog.Text = title;
                dialog.Message.Text = message;
                
                result = dialog.ShowDialog(_parent);
                dontShowAgainStatus = dialog.dontShowThisAgain.Checked;
            }

            switch (result)
            {
                case DialogResult.OK:
                    return PopupButtons.Ok;

                case DialogResult.Cancel:
                    return PopupButtons.Cancel;

                case DialogResult.Abort:
                    return PopupButtons.Exit;

                default:
                    throw new InvalidOperationException($"Unexpected result from dialog: {result}");
            }
        }

        public Task<PopupButtons> ShowAsync(PopupButtons buttons, string title, string message)
        {
            return Task.FromResult(Show(buttons, title, message));
        }

        public Task<string> ShowAsync(string[] buttons, string title, string message)
        {
            throw new NotImplementedException("Popup doesn't support custom buttons.");
        }
    }
}