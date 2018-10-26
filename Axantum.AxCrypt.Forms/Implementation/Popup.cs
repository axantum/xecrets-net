using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.UI;
using System;
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
            return ShowAsync(buttons, title, message, dontShowAgainFlag, null);
        }

        public Task<PopupButtons> ShowAsync(PopupButtons buttons, string title, string message, DoNotShowAgainOptions dontShowAgainFlag, string doNotShowAgainCustomText)
        {
            string[] stringButtons = GetStringButtons(buttons);
            string popupResult = ShowAsync(stringButtons, title, message, dontShowAgainFlag, doNotShowAgainCustomText).Result;

            return Task.FromResult(GetPopupResult(popupResult));
        }

        private string[] GetStringButtons(PopupButtons buttons)
        {
            string[] stringButtons = null;

            switch (buttons)
            {
                case PopupButtons.Ok:
                    stringButtons = new string[] { nameof(PopupButtons.Ok) };
                    break;

                case PopupButtons.Cancel:
                    stringButtons = new string[] { nameof(PopupButtons.Cancel) };
                    break;

                case PopupButtons.Exit:
                    stringButtons = new string[] { nameof(PopupButtons.Exit) };
                    break;

                case PopupButtons.OkCancel:
                    stringButtons = new string[] { nameof(PopupButtons.Ok), nameof(PopupButtons.Cancel) };
                    break;

                case PopupButtons.OkExit:
                    stringButtons = new string[] { nameof(PopupButtons.Ok), nameof(PopupButtons.Exit) };
                    break;

                case PopupButtons.OkCancelExit:
                    stringButtons = new string[] { nameof(PopupButtons.Ok), nameof(PopupButtons.Cancel), nameof(PopupButtons.Exit) };
                    break;
            }

            return stringButtons;
        }

        private PopupButtons GetPopupResult(string popupResult)
        {
            PopupButtons popupButtonResults = PopupButtons.None;
            switch (popupResult)
            {
                case nameof(PopupButtons.Ok):
                    popupButtonResults = PopupButtons.Ok;
                    break;

                case nameof(PopupButtons.Cancel):
                    popupButtonResults = PopupButtons.Cancel;
                    break;

                case nameof(PopupButtons.Exit):
                    popupButtonResults = PopupButtons.Exit;
                    break;
            }

            return popupButtonResults;
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
            if (dontShowAgainFlag != DoNotShowAgainOptions.None && New<UserSettings>().DoNotShowAgain.HasFlag(dontShowAgainFlag))
            {
                return result;
            }
            if (buttons.Length > 3)
            {
                return result;
            }

            New<IUIThread>().SendTo(() => result = ShowSyncInternalAssumingUiThread(buttons, title, message, dontShowAgainFlag, doNotShowAgainCustomText));
            return result;
        }

        private string ShowSyncInternalAssumingUiThread(string[] buttons, string title, string message, DoNotShowAgainOptions dontShowAgainFlag, string doNotShowAgainCustomText)
        {
            DialogResult result;
            using (MessageDialog messageDialog = new MessageDialog(_parent, doNotShowAgainCustomText))
            {
                switch (buttons.Length)
                {
                    case 1:
                        messageDialog.InitializeButtonTexts(buttons[0]);
                        messageDialog.HideCancel();
                        messageDialog.HideExit();
                        break;

                    case 2:
                        messageDialog.InitializeButtonTexts(buttons[0], buttons[1]);
                        messageDialog.HideExit();
                        break;

                    case 3:
                        messageDialog.InitializeButtonTexts(buttons[0], buttons[1], buttons[2]);
                        break;

                    default:
                        throw new NotSupportedException("Can't display alert dialog(s) with more than 3 buttons!");
                }

                if (dontShowAgainFlag == DoNotShowAgainOptions.None)
                {
                    messageDialog.HideDontShowAgain();
                }

                messageDialog.Text = title;
                messageDialog.Message.Text = message;

                result = messageDialog.ShowDialog(_parent);

                if (dontShowAgainFlag != DoNotShowAgainOptions.None && messageDialog.dontShowThisAgain.Checked)
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