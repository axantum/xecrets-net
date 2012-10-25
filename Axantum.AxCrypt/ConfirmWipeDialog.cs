using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    public partial class ConfirmWipeDialog : Form
    {
        public ConfirmWipeDialog()
        {
            InitializeComponent();
        }

        private void ConfirmWipeDialog_Load(object sender, EventArgs e)
        {
            iconPictureBox.Image = SystemIcons.Warning.ToBitmap();
        }

        private void promptLabel_Click(object sender, EventArgs e)
        {

        }
    }
}