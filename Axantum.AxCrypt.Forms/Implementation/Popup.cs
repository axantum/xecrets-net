using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Forms.Implementation
{
    internal class Popup : IPopup
    {
        private Form _parent;

        public Popup(Form parent)
        {
            _parent = parent;
        }

        public Task<PopupButtons> ShowAsync(PopupButtons buttons, string title, string message)
        {
            return ShowAsync(buttons, title, message, DoNotShowAgainOptions.None, null);
        }

        public Task<PopupButtons> ShowAsync(PopupButtons buttons, string title, string message, DoNotShowAgainOptions dontShowAgainFlag)
        {
            return Task.FromResult(ShowSyncInternal(buttons, title, message, dontShowAgainFlag, null));
        }

        public Task<PopupButtons> ShowAsync(PopupButtons buttons, string title, string message, DoNotShowAgainOptions dontShowAgainFlag, string doNotShowAgainCustomText)
        {
            return Task.FromResult(ShowSyncInternal(buttons, title, message, dontShowAgainFlag, doNotShowAgainCustomText));
        }

        private PopupButtons ShowSyncInternal(PopupButtons buttons, string title, string message, DoNotShowAgainOptions dontShowAgainFlag, string doNotShowAgainCustomText)
        {
            PopupButtons result = PopupButtons.None;
            if (dontShowAgainFlag != DoNotShowAgainOptions.None && New<UserSettings>().DoNotShowAgain.HasFlag(dontShowAgainFlag))
            {
                return result;
            }

            New<IUIThread>().SendTo(() => result = ShowSyncInternalAssumingUiThread(buttons, title, message, dontShowAgainFlag, doNotShowAgainCustomText));
            return result;
        }

        private PopupButtons ShowSyncInternalAssumingUiThread(PopupButtons buttons, string title, string message, DoNotShowAgainOptions dontShowAgainFlag, string doNotShowAgainCustomText)
        {
            DialogResult result;
            using (MessageDialog dialog = new MessageDialog(_parent, doNotShowAgainCustomText))
            {
                if (!buttons.HasFlag(PopupButtons.Cancel))
                {
                    dialog.HideCancel();
                }
                if (!buttons.HasFlag(PopupButtons.Exit))
                {
                    dialog.HideExit();
                }
                if (dontShowAgainFlag == DoNotShowAgainOptions.None)
                {
                    dialog.HideDontShowAgain();
                }

                dialog.Text = title;
                dialog.Message.Text = message;

                result = dialog.ShowDialog(_parent);

                if (dontShowAgainFlag != DoNotShowAgainOptions.None && dialog.dontShowThisAgain.Checked)
                {
                    New<UserSettings>().DoNotShowAgain = New<UserSettings>().DoNotShowAgain | dontShowAgainFlag;
                }
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

        public Task<string> ShowAsync(string[] buttons, string title, string message)
        {
            return ShowAsync(buttons, title, message, DoNotShowAgainOptions.None);
        }

        public Task<string> ShowAsync(string[] buttons, string title, string message, DoNotShowAgainOptions dontShowAgain)
        {
            return Task.FromResult(ShowSyncInternal(buttons, title, message, dontShowAgain, null));
        }

        public Task<string> ShowAsync(string[] buttons, string title, string message, DoNotShowAgainOptions dontShowAgainFlag, string doNotShowAgainCustomText)
        {
            return Task.FromResult(ShowSyncInternal(buttons, title, message, dontShowAgainFlag, doNotShowAgainCustomText));
        }

        private string ShowSyncInternal(string[] buttons, string title, string message, DoNotShowAgainOptions dontShowAgainFlag, string doNotShowAgainCustomText)
        {
            string result = string.Empty;
            if (buttons.Length > 3)
            {
                return result;
            }
            if (dontShowAgainFlag != DoNotShowAgainOptions.None && New<UserSettings>().DoNotShowAgain.HasFlag(dontShowAgainFlag))
            {
                return result;
            }

            New<IUIThread>().SendTo(() => result = ShowSyncInternalAssumingUiThread(buttons, title, message, dontShowAgainFlag, doNotShowAgainCustomText));
            return result;
        }

        private string ShowSyncInternalAssumingUiThread(string[] buttons, string title, string message, DoNotShowAgainOptions dontShowAgainFlag, string doNotShowAgainCustomText)
        {
            CustomMessageDialog customMessageDialog;
            switch (buttons.Length)
            {
                case 1:
                    customMessageDialog = new CustomMessageDialog(_parent, doNotShowAgainCustomText, buttons[0]);
                    break;

                case 2:
                    customMessageDialog = new CustomMessageDialog(_parent, doNotShowAgainCustomText, buttons[0], buttons[1]);
                    break;

                case 3:
                    customMessageDialog = new CustomMessageDialog(_parent, doNotShowAgainCustomText, buttons[0], buttons[1], buttons[2]);
                    break;

                default:
                    throw new NotSupportedException("Can display alerts with 1 to 3 buttons only");
            }

            DialogResult result;
            using (customMessageDialog)
            {
                customMessageDialog.Text = title;
                customMessageDialog.Message.Text = message;

                result = customMessageDialog.ShowDialog(_parent);

                if (dontShowAgainFlag != DoNotShowAgainOptions.None && customMessageDialog.dontShowThisAgain.Checked)
                {
                    New<UserSettings>().DoNotShowAgain = New<UserSettings>().DoNotShowAgain | dontShowAgainFlag;
                }
            }

            switch (result)
            {
                case DialogResult.OK:
                    return buttons[0];

                case DialogResult.Cancel:
                    return buttons[1];

                case DialogResult.Abort:
                    return buttons[2];

                default:
                    throw new InvalidOperationException($"Unexpected result from dialog: {result}");
            }
        }
    }
}