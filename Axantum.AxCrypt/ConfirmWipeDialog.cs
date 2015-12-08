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
        }

        public ConfirmWipeDialog(Form parent)
            : this()
        {
            InitializeStyle(parent);
        }

        protected override void InitializeContentResources()
        {
            Text = Content.SecureDeleteDialogTitle;
            _cancelButton.Text = Content.ButtonCancelText;
            _noButton.Text = Content.ButtonNoText;
            _promptLabel.Text = Content.PromptLabelText;
            _yesButton.Text = Content.ButtonYesText;
            _confirmAllCheckBox.Text = Content.ConfirmAllCheckBoxText;
        }

        private void ConfirmWipeDialog_Load(object sender, EventArgs e)
        {
            _iconPictureBox.Image = SystemIcons.Warning.ToBitmap();
        }

        private void promptLabel_Click(object sender, EventArgs e)
        {
        }
    }
}