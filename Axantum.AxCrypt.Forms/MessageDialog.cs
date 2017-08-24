using AxCrypt.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Axantum.AxCrypt.Forms
{
    internal partial class MessageDialog : StyledMessageBase
    {
        private string _doNotShowAgainCustomText;

        public MessageDialog()
        {
            InitializeComponent();
        }

        public MessageDialog(Form parent)
            : this(parent, null)
        {
        }

        public MessageDialog(Form parent, string doNotShowAgainCustomText)
            : this()
        {
            InitializeStyle(parent);
            _doNotShowAgainCustomText = doNotShowAgainCustomText;
        }

        protected override void InitializeContentResources()
        {
            _buttonOk.Text = "&" + Texts.ButtonOkText;
            _buttonCancel.Text = "&" + Texts.ButtonCancelText;
            _buttonExit.Text = "&" + Texts.ButtonExitText;
            dontShowThisAgain.Text = _doNotShowAgainCustomText ?? Texts.DontShowAgainCheckBoxText;
        }

        public MessageDialog HideExit()
        {
            _buttonExit.Visible = false;
            ReSizeButtonsPanel();
            return this;
        }

        public MessageDialog HideCancel()
        {
            _buttonCancel.Visible = false;
            ReSizeButtonsPanel();
            return this;
        }

        public MessageDialog HideDontShowAgain()
        {
            dontShowThisAgain.Visible = false;
            tableLayoutPanel1.RowCount = 2;
            return this;
        }

        private void ReSizeButtonsPanel()
        {
            flowLayoutPanel1.PerformLayout();
            flowLayoutPanel1.Left = (flowLayoutPanel1.Parent.ClientRectangle.Width - flowLayoutPanel1.Width) / 2;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static DialogResult ShowOk(Form parent, string caption, string message)
        {
            using (MessageDialog messageDialog = new MessageDialog(parent))
            {
                messageDialog.HideExit();
                messageDialog.HideCancel();
                messageDialog.Text = caption;
                messageDialog.Message.Text = message;

                return messageDialog.ShowDialog(parent);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static DialogResult ShowOkCancel(Form parent, string caption, string message)
        {
            using (MessageDialog messageDialog = new MessageDialog(parent))
            {
                messageDialog.HideExit();
                messageDialog.Text = caption;
                messageDialog.Message.Text = message;

                return messageDialog.ShowDialog(parent);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static DialogResult ShowOkCancelExit(Form parent, string caption, string message)
        {
            using (MessageDialog messageDialog = new MessageDialog(parent))
            {
                messageDialog.Text = caption;
                messageDialog.Message.Text = message;

                return messageDialog.ShowDialog(parent);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static DialogResult ShowOkExit(Form parent, string caption, string message)
        {
            using (MessageDialog messageDialog = new MessageDialog(parent))
            {
                messageDialog.HideCancel();
                messageDialog.Text = caption;
                messageDialog.Message.Text = message;

                return messageDialog.ShowDialog(parent);
            }
        }
    }
}