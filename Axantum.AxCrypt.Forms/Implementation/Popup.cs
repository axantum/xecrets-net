using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.UI;
using AxCrypt.Content;
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

        public async Task<PopupButtons> ShowAsync(PopupButtons buttons, string title, string message, DoNotShowAgainOptions dontShowAgainFlag, string doNotShowAgainCustomText)
        {
            string[] stringButtons = GetStringButtons(buttons);
            string popupResult = await ShowAsync(stringButtons, title, message, dontShowAgainFlag, doNotShowAgainCustomText);

            return GetPopupResult(popupResult);
        }

        private string[] GetStringButtons(PopupButtons buttons)
        {
            switch (buttons)
            {
                case PopupButtons.Ok:
                    return new string[] { Texts.ButtonOkText };

                case PopupButtons.Cancel:
                    return new string[] { Texts.ButtonCancelText };

                case PopupButtons.Exit:
                    return new string[] { Texts.ButtonExitText };

                case PopupButtons.OkCancel:
                    return new string[] { Texts.ButtonOkText, Texts.ButtonCancelText };

                case PopupButtons.OkExit:
                    return new string[] { Texts.ButtonOkText, Texts.ButtonExitText };

                case PopupButtons.OkCancelExit:
                    return new string[] { Texts.ButtonOkText, Texts.ButtonCancelText, Texts.ButtonExitText };

                default:
                    throw new InvalidOperationException($"Unexpected popup buttons {buttons}.");
            }
        }

        private PopupButtons GetPopupResult(string popupResult)
        {
            if (popupResult == Texts.ButtonOkText)
            {
                return PopupButtons.Ok;
            }
            if (popupResult == Texts.ButtonCancelText)
            {
                return PopupButtons.Cancel;
            }
            if (popupResult == Texts.ButtonExitText)
            {
                return PopupButtons.Exit;
            }
            if (popupResult == string.Empty)
            {
                return PopupButtons.None;
            }

            throw new InvalidOperationException($"Unexpected popup button {popupResult} clicked.");
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
            if (buttons.Length > 3)
            {
                throw new InvalidOperationException($"More than 3 buttons are not supported in a popup dialog.");
            }

            string result = string.Empty;
            if (dontShowAgainFlag != DoNotShowAgainOptions.None && New<UserSettings>().DoNotShowAgain.HasFlag(dontShowAgainFlag))
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
                        messageDialog.InitializeButtonTexts(buttons[0], null, null);
                        messageDialog.HideButton1();
                        messageDialog.HideButton2();
                        break;

                    case 2:
                        messageDialog.InitializeButtonTexts(buttons[0], buttons[1], null);
                        messageDialog.HideButton2();
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
                    return buttons.Length > 1 ? buttons[1] : Texts.ButtonCancelText;

                case DialogResult.Abort:
                    return buttons[2];

                default:
                    throw new InvalidOperationException($"Unexpected result from dialog: {result}");
            }
        }
    }
}