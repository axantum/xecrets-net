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
    public partial class DebugLogOutputDialog : Form
    {
        public DebugLogOutputDialog()
        {
            InitializeComponent();
        }

        private void DebugLogOutputDialog_Load(object sender, EventArgs e)
        {
            FormClosing += (fsender, fe) => { if (!AllowClose) { Visible = false; fe.Cancel = true; } };
        }

        public void AppendText(string text)
        {
            _logOutputTextBox.AppendText(text);
        }

        public bool AllowClose { get; set; }
    }
}
