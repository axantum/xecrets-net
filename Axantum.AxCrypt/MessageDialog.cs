using Axantum.AxCrypt.Forms.Style;
using Axantum.AxCrypt.Properties;
using System;
using System.Collections.Generic;
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

        private Point? _lastLocation;

        public MessageDialog(Form parent)
            : this()
        {
            Owner = parent;
            Move += MessageDialog_Move;
            StartPosition = FormStartPosition.CenterParent;
        }

        private void MessageDialog_Move(object sender, EventArgs e)
        {
            if (_lastLocation == null)
            {
                return;
            }
            Owner.Location = new Point(Owner.Location.X - (_lastLocation.Value.X - Location.X), Owner.Location.Y - (_lastLocation.Value.Y - Location.Y));
            _lastLocation = Location;
        }

        public static DialogResult ShowOkCancel(Form parent, string caption, string message)
        {
            using (MessageDialog messageDialog = new MessageDialog(parent))
            {
                messageDialog.HideExit();
                messageDialog.Text = caption;
                messageDialog._text.Text = message;

                return messageDialog.ShowDialog(parent);
            }
        }

        public static DialogResult ShowOkCancelExit(Form parent, string caption, string message)
        {
            using (MessageDialog messageDialog = new MessageDialog(parent))
            {
                messageDialog.Text = caption;
                messageDialog._text.Text = message;

                return messageDialog.ShowDialog(parent);
            }
        }

        public static DialogResult ShowOkExit(Form parent, string caption, string message)
        {
            using (MessageDialog messageDialog = new MessageDialog(parent))
            {
                messageDialog.HideCancel();
                messageDialog.Text = caption;
                messageDialog._text.Text = message;

                return messageDialog.ShowDialog(parent);
            }
        }

        private void _buttonOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void _buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void _buttonExit_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Abort;
        }

        private void MessageDialog_Shown(object sender, EventArgs e)
        {
            _lastLocation = Location;
        }
    }
}