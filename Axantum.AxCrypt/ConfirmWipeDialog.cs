using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Forms.Style;
using Axantum.AxCrypt.Properties;
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
            new Styling(Resources.axcrypticon).Style(this);
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