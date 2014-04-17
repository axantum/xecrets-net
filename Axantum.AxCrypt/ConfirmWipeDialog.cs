using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    public partial class ConfirmWipeDialog : Form
    {
        public ConfirmWipeDialog()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterParent;
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