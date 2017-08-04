﻿using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            return ShowAsync(buttons, title, message, DontShowAgain.None);
        }

        public Task<PopupButtons> ShowAsync(PopupButtons buttons, string title, string message, DontShowAgain dontShowAgainFlag)
        {
            return Task.FromResult(ShowSyncInternal(buttons, title, message, dontShowAgainFlag));
        }

        private PopupButtons ShowSyncInternal(PopupButtons buttons, string title, string message, DontShowAgain dontShowAgainFlag)
        {
            PopupButtons result = PopupButtons.None;
            if (dontShowAgainFlag != DontShowAgain.None && New<UserSettings>().DontShowAgain.HasFlag(dontShowAgainFlag))
            {
                return result;
            }

            New<IUIThread>().SendTo(() => result = ShowSyncInternalAssumingUiThread(buttons, title, message, dontShowAgainFlag));
            return result;
        }

        private PopupButtons ShowSyncInternalAssumingUiThread(PopupButtons buttons, string title, string message, DontShowAgain dontShowAgainFlag)
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
                if (dontShowAgainFlag == DontShowAgain.None)
                {
                    dialog.HideDontShowAgain();
                }

                dialog.Text = title;
                dialog.Message.Text = message;

                result = dialog.ShowDialog(_parent);

                if (dontShowAgainFlag != DontShowAgain.None && dialog.dontShowThisAgain.Checked)
                {
                    New<UserSettings>().DontShowAgain = New<UserSettings>().DontShowAgain | dontShowAgainFlag;
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
            throw new NotImplementedException("Popup doesn't support custom buttons.");
        }

        public Task<string> ShowAsync(string[] buttons, string title, string message, DontShowAgain dontShowAgain)
        {
            throw new NotImplementedException("Popup doesn't support custom buttons.");
        }
    }
}