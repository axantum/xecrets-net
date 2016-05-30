using Axantum.AxCrypt.Forms.Style;
using Axantum.AxCrypt.Properties;
using AxCrypt.Content;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    public partial class ConfirmWipeDialog : StyledMessageBase
    {
        public ConfirmWipeDialog()
        {
            InitializeComponent();
            new Styling(Resources.axcrypticon).Style(this);
        }

        protected override void InitializeContentResources()
        {
            Text = Texts.SecureDeleteDialogTitle;
            _cancelButton.Text = Texts.ButtonCancelText;
            _noButton.Text = Texts.ButtonNoText;
            _promptLabel.Text = Texts.PromptLabelText;
            _yesButton.Text = Texts.ButtonYesText;
            _confirmAllCheckBox.Text = Texts.ConfirmAllCheckBoxText;
        }

        private void ConfirmWipeDialog_Load(object sender, EventArgs e)
        {
            _iconPictureBox.Image = SystemIcons.Warning.ToBitmap();
        }

        private void ConfirmWipeDialog_Shown(object sender, EventArgs e)
        {
            Activate();
            Focus();
            BringToFront();
        }
    }
}