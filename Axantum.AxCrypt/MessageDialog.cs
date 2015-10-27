using Axantum.AxCrypt.Forms.Style;
using Axantum.AxCrypt.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    public partial class MessageDialog : Form
    {
        public MessageDialog()
        {
            InitializeComponent();
            new Styling(Resources.axcrypticon).Style(this);
        }

        public MessageDialog(Form parent)
            : this()
        {
            Owner = parent;
            Owner.Activated += (sender, e) => Activate();
            StartPosition = FormStartPosition.CenterParent;
        }

        public static void Show(Form parent, string caption, string message)
        {
            using (MessageDialog messageDialog = new MessageDialog(parent))
            {
                messageDialog.Text = caption;
                messageDialog._text.Text = message;

                messageDialog.ShowDialog(parent);
            }
        }

        private void _buttonOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}