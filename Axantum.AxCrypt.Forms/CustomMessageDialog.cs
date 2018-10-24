using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Axantum.AxCrypt.Forms
{
    internal class CustomMessageDialog : MessageDialog
    {
        private string _customButton1Text = "";
        private string _customButton2Text = "";
        private string _customButton3Text = "";

        public CustomMessageDialog()
            : base()
        {
        }

        public CustomMessageDialog(Form parent)
            : this(parent, null)
        {
        }

        public CustomMessageDialog(Form parent, string doNotShowAgainCustomText)
            : base(parent, doNotShowAgainCustomText)
        {
        }

        public void InitializeCustomButtons(string button1Text)
        {
            InitializeCustomButtons(button1Text, null);
        }

        public void InitializeCustomButtons(string button1Text, string button2Text)
        {
            InitializeCustomButtons(button1Text, button2Text, null);
        }

        public void InitializeCustomButtons(string customButton1Text, string customButton2Text, string customButton3Text)
        {
            _customButton1Text = customButton1Text;
            _customButton2Text = customButton2Text;
            _customButton3Text = customButton3Text;
        }

        protected override void InitializeContentResources()
        {
            InitializeCustomButtonTexts(_customButton1Text, _customButton2Text, _customButton3Text);
        }
    }
}