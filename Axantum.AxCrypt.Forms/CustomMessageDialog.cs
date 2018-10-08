using AxCrypt.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Axantum.AxCrypt.Forms
{
    internal partial class CustomMessageDialog : StyledMessageBase
    {
        private string _doNotShowAgainCustomText;

        public CustomMessageDialog()
        {
            InitializeComponent();

            _customButton1.Visible = false;
            _customButton2.Visible = false;
            _customButton3.Visible = false;
        }

        public CustomMessageDialog(Form parent)
            : this(parent, null, null)
        {
        }

        public CustomMessageDialog(Form parent, string doNotShowAgainCustomText, string button1)
            : this(parent, doNotShowAgainCustomText, button1, null)
        {
        }

        public CustomMessageDialog(Form parent, string doNotShowAgainCustomText, string button1, string button2)
           : this(parent, doNotShowAgainCustomText, button1, button2, null)
        {
        }

        public CustomMessageDialog(Form parent, string doNotShowAgainCustomText, string button1, string button2, string button3)
           : this()
        {
            InitializeStyle(parent);
            _doNotShowAgainCustomText = doNotShowAgainCustomText;
            InitializeDialogResources(button1, button2, button3);
        }

        public void InitializeDialogResources(string customButton1Text, string customButton2Text, string customButton3Text)
        {
            if (!string.IsNullOrEmpty(customButton1Text))
            {
                _customButton1.Text = "&" + customButton1Text;
                _customButton1.Visible = true;
            }
            if (!string.IsNullOrEmpty(customButton2Text))
            {
                _customButton2.Text = "&" + customButton2Text;
                _customButton2.Visible = true;
            }
            if (!string.IsNullOrEmpty(customButton3Text))
            {
                _customButton3.Text = "&" + customButton3Text;
                _customButton3.Visible = true;
            }

            dontShowThisAgain.Text = _doNotShowAgainCustomText ?? Texts.DontShowAgainCheckBoxText;
        }

        public CustomMessageDialog HideDontShowAgain()
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
    }
}