#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
 *
 * This file is part of AxCrypt.
 *
 * AxCrypt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AxCrypt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AxCrypt.  If not, see <http://www.gnu.org/licenses/>.
 *
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Properties;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    public partial class EncryptPassphraseDialog : Form
    {
        private FileSystemState _fileSystemState;

        public EncryptPassphraseDialog(FileSystemState fileSystemState)
        {
            InitializeComponent();
            SetAutoValidateViaReflectionToAvoidMoMaWarning();
            _fileSystemState = fileSystemState;
        }

        private void EncryptPassphraseDialog_Load(object sender, EventArgs e)
        {
            ShowHidePasshrase();
            if (!_fileSystemState.Identities.Any(identity => String.Compare(identity.Name, Environment.UserName, StringComparison.OrdinalIgnoreCase) == 0))
            {
                NameTextBox.Text = Environment.UserName;
                PassphraseTextBox.Focus();
            }
            else
            {
                NameTextBox.Focus();
            }
        }

        private void ShowHidePasshrase()
        {
            PassphraseTextBox.UseSystemPasswordChar = !ShowPassphraseCheckBox.Checked;
            VerifyPassphraseTextbox.UseSystemPasswordChar = !ShowPassphraseCheckBox.Checked;
        }

        private void SetAutoValidateViaReflectionToAvoidMoMaWarning()
        {
            if (OS.Current.Platform == Platform.WindowsDesktop)
            {
                PropertyInfo propertyInfo = typeof(EncryptPassphraseDialog).GetProperty("AutoValidate"); //MLHIDE
                propertyInfo.SetValue(this, AutoValidate.EnableAllowFocusChange, null);
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            if (!ValidateChildren(ValidationConstraints.Visible))
            {
                DialogResult = DialogResult.None;
            }
        }

        private void ShowPassphraseCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ShowHidePasshrase();
        }

        private void VerifyPassphraseTextbox_Validating(object sender, CancelEventArgs e)
        {
            if (String.Compare(PassphraseTextBox.Text, VerifyPassphraseTextbox.Text, StringComparison.Ordinal) != 0)
            {
                e.Cancel = true;
                errorProvider1.SetError(VerifyPassphraseTextbox, Resources.PassphraseVerificationMismatch);
            }
        }

        private void VerifyPassphraseTextbox_Validated(object sender, EventArgs e)
        {
            errorProvider1.Clear();
        }

        private void NameTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (_fileSystemState.Identities.Any(i => i.Name == NameTextBox.Text))
            {
                e.Cancel = true;
                errorProvider2.SetError(NameTextBox, Resources.LogOnExists);
            }
        }

        private void NameTextBox_Validated(object sender, EventArgs e)
        {
            errorProvider2.Clear();
        }
    }
}