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
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Properties;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    public partial class LogOnDialog : Form
    {
        private FileSystemState _fileSystemState;

        public LogOnDialog(FileSystemState fileSystemState, PassphraseIdentity identity)
        {
            InitializeComponent();
            SetAutoValidateViaReflectionToAvoidMoMaWarning();
            _fileSystemState = fileSystemState;

            if (String.IsNullOrEmpty(identity.Name))
            {
                return;
            }
            PassphraseGroupBox.Text = Resources.EnterPassphraseForIdentityPrompt.InvariantFormat(identity.Name);
        }

        private void EncryptPassphraseDialog_Load(object sender, EventArgs e)
        {
            ShowHidePasshrase();
        }

        private void ShowHidePasshrase()
        {
            PassphraseTextBox.UseSystemPasswordChar = !ShowPassphraseCheckBox.Checked;
        }

        private void SetAutoValidateViaReflectionToAvoidMoMaWarning()
        {
            if (OS.Current.Platform == Platform.WindowsDesktop)
            {
                PropertyInfo propertyInfo = typeof(EncryptPassphraseDialog).GetProperty("AutoValidate");
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

        private void PassphraseTextBox_Validating(object sender, CancelEventArgs e)
        {
            AesKeyThumbprint thumbprint = Passphrase.Derive(PassphraseTextBox.Text).Thumbprint;
            if (!_fileSystemState.Identities.Any(identity => identity.Thumbprint == thumbprint))
            {
                e.Cancel = true;
                _errorProvider1.SetError(PassphraseTextBox, Resources.UnkownLogOn);
            }
        }

        private void PassphraseTextBox_Validated(object sender, EventArgs e)
        {
            _errorProvider1.Clear();
        }

        private void newButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Retry;
        }

        private void LogOnDialog_Activated(object sender, EventArgs e)
        {
            TopMost = true;
            BringToFront();
            Focus();
        }
    }
}