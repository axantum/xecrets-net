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

            _customOkButton.Visible = false;
            _customCancelButton.Visible = false;
            _customAbortButton.Visible = false;
        }

        public CustomMessageDialog(Form parent)
            : this(parent, null, null)
        {
        }

        public CustomMessageDialog(Form parent, string doNotShowAgainCustomText, string button1Text)
            : this(parent, doNotShowAgainCustomText, button1Text, null)
        {
        }

        public CustomMessageDialog(Form parent, string doNotShowAgainCustomText, string button1Text, string button2Text)
           : this(parent, doNotShowAgainCustomText, button1Text, button2Text, null)
        {
        }

        public CustomMessageDialog(Form parent, string doNotShowAgainCustomText, string button1Text, string button2Text, string button3Text)
           : this()
        {
            InitializeStyle(parent);
            _doNotShowAgainCustomText = doNotShowAgainCustomText;
            InitializeDialogResources(button1Text, button2Text, button3Text);
        }

        public void InitializeDialogResources(string customButton1Text, string customButton2Text, string customButton3Text)
        {
            if (!string.IsNullOrEmpty(customButton1Text))
            {
                _customOkButton.Text = "&" + customButton1Text;
                _customOkButton.Visible = true;
            }
            if (!string.IsNullOrEmpty(customButton2Text))
            {
                _customCancelButton.Text = "&" + customButton2Text;
                _customCancelButton.Visible = true;
            }
            if (!string.IsNullOrEmpty(customButton3Text))
            {
                _customAbortButton.Text = "&" + customButton3Text;
                _customAbortButton.Visible = true;
            }

            dontShowThisAgain.Text = _doNotShowAgainCustomText ?? Texts.DontShowAgainCheckBoxText;
        }

        public CustomMessageDialog HideDontShowAgain()
        {
            dontShowThisAgain.Visible = false;
            tableLayoutPanel.RowCount = 2;
            return this;
        }

        private void ReSizeButtonsPanel()
        {
            flowLayoutPanel.PerformLayout();
            flowLayoutPanel.Left = (flowLayoutPanel.Parent.ClientRectangle.Width - flowLayoutPanel.Width) / 2;
        }
    }
}