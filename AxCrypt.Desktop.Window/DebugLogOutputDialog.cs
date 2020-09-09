using AxCrypt.Core;
using AxCrypt.Forms;
using AxCrypt.Forms.Style;
using AxCrypt.Desktop.Window.Properties;
using AxCrypt.Content;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AxCrypt.Desktop.Window
{
    public partial class DebugLogOutputDialog : StyledMessageBase
    {
        public DebugLogOutputDialog()
        {
            InitializeComponent();
        }

        public DebugLogOutputDialog(Form parent)
            : this()
        {
            InitializeStyle(parent);
        }

        protected override void InitializeContentResources()
        {
            Text = Texts.DialogDebugLogTitle;
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