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

        public PopupButtons Show(PopupButtons buttons, string title, string message)
        {
            return Show(buttons, title, message, DontShowAgain.None);
        }

        public PopupButtons Show(PopupButtons buttons, string title, string message, DontShowAgain dontShowAgainFlag)
        {
            if (dontShowAgainFlag != DontShowAgain.None && New<UserSettings>().DontShowAgain.HasFlag(dontShowAgainFlag))
            {
                return PopupButtons.None;
            }

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
    }
}