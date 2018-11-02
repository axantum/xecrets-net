using AxCrypt.Content;
using System.Linq;
using System.Windows.Forms;

namespace Axantum.AxCrypt.Forms
{
    internal partial class MessageDialog : StyledMessageBase
    {
        private string _button1Text;
        private string _button2Text;
        private string _button3Text;
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
            _button0.Text = "&" + _button1Text ?? Texts.ButtonOkText;
            _button1.Text = "&" + _button2Text ?? Texts.ButtonCancelText;
            _button2.Text = "&" + _button3Text ?? Texts.ButtonExitText;
            dontShowThisAgain.Text = _doNotShowAgainCustomText ?? Texts.DontShowAgainCheckBoxText;
        }

        public void InitializeButtonTexts(string button1Text, string button2Text, string button3Text)
        {
            _button1Text = button1Text;
            _button2Text = button2Text;
            _button3Text = button3Text;
        }

        public MessageDialog HideButton2()
        {
            _button2.Visible = false;
            ReSizeButtonsPanel();
            return this;
        }

        public MessageDialog HideButton1()
        {
            _button1.Visible = false;
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
                messageDialog.HideButton2();
                messageDialog.HideButton1();
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
                messageDialog.HideButton2();
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
                messageDialog.HideButton1();
                messageDialog.Text = caption;
                messageDialog.Message.Text = message;

                return messageDialog.ShowDialog(parent);
            }
        }
    }
}