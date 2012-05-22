using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Properties;

namespace Axantum.AxCrypt
{
    public partial class EncryptPassphraseDialog : Form
    {
        public EncryptPassphraseDialog()
        {
            InitializeComponent();
            SetAutoValidateViaReflectionToAvoidMoMaWarning();
        }

        private void SetAutoValidateViaReflectionToAvoidMoMaWarning()
        {
            if (AxCryptEnvironment.Current.IsDesktopWindows)
            {
                PropertyInfo propertyInfo = typeof(EncryptPassphraseDialog).GetProperty("AutoValidate");
                propertyInfo.SetValue(this, AutoValidate.EnableAllowFocusChange, null);
            }
        }

        private void VerifyPassphraseTextbox_Validating(object sender, CancelEventArgs e)
        {
            if (String.Compare(PassphraseTextBox.Text, VerifyPassphraseTextbox.Text, StringComparison.Ordinal) != 0)
            {
                e.Cancel = true;
                errorProvider1.SetError(VerifyPassphraseTextbox, Resources.PassphraseVerificationMismatch);
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            if (!ValidateChildren(ValidationConstraints.Visible))
            {
                DialogResult = DialogResult.None;
            }
        }

        private void VerifyPassphraseTextbox_Validated(object sender, EventArgs e)
        {
            errorProvider1.Clear();
        }
    }
}