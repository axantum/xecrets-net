using Axantum.AxCrypt.Forms;
using AxCrypt.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Axantum.AxCrypt.Forms
{
    internal partial class MessageDialog : StyledMessageBase
    {
        public MessageDialog()
        {
            InitializeComponent();
        }

        public MessageDialog(Form parent)
            : this()
        {
            InitializeStyle(parent);
        }

        protected override void InitializeContentResources()
        {
            _buttonOk.Text = "&" + Texts.ButtonOkText;
            _buttonCancel.Text = "&" + Texts.ButtonCancelText;
            _buttonExit.Text = "&" + Texts.ButtonExitText;
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

        private void ReSizeButtonsPanel()
        {
            flowLayoutPanel1.PerformLayout();
            flowLayoutPanel1.Left = (flowLayoutPanel1.Parent.ClientRectangle.Width - flowLayoutPanel1.Width) / 2;
        }

        public static DialogResult ShowOk(Form parent, string caption, string message)
        {
            return ShowOkAsync(parent, caption, message).Result;
        }

        public static Task<DialogResult> ShowOkAsync(Form parent, string caption, string message)
        {
            using (MessageDialog messageDialog = new MessageDialog(parent))
            {
                messageDialog.HideExit();
                messageDialog.HideCancel();
                messageDialog.Text = caption;
                messageDialog.Message.Text = message;

                return Task.FromResult(messageDialog.ShowDialog(parent));
            }
        }

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

        public static DialogResult ShowOkCancelExit(Form parent, string caption, string message)
        {
            using (MessageDialog messageDialog = new MessageDialog(parent))
            {
                messageDialog.Text = caption;
                messageDialog.Message.Text = message;

                return messageDialog.ShowDialog(parent);
            }
        }

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