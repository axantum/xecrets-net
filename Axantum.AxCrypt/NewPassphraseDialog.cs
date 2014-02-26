#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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
using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Properties;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    public partial class NewPassphraseDialog : Form
    {
        private NewPassphraseViewModel _viewModel;

        public NewPassphraseDialog(string passphrase, string encryptedFileFullName)
        {
            InitializeComponent();

            SetAutoValidateViaReflectionToAvoidMoMaWarning();

            _viewModel = new NewPassphraseViewModel(passphrase, Environment.UserName, encryptedFileFullName);

            NameTextBox.TextChanged += (sender, e) => { _viewModel.IdentityName = NameTextBox.Text; };
            PassphraseTextBox.TextChanged += (sender, e) => { _viewModel.Passphrase = PassphraseTextBox.Text; };
            VerifyPassphraseTextbox.TextChanged += (sender, e) => { _viewModel.Verification = VerifyPassphraseTextbox.Text; };
            ShowPassphraseCheckBox.CheckedChanged += (sender, e) => { _viewModel.ShowPassphrase = ShowPassphraseCheckBox.Checked; };

            _viewModel.BindPropertyChanged("ShowPassphrase", (bool show) => { PassphraseTextBox.UseSystemPasswordChar = VerifyPassphraseTextbox.UseSystemPasswordChar = !show; });
            _viewModel.BindPropertyChanged("FileName", (string fileName) => { FileNameTextBox.Text = fileName; FileNamePanel.Visible = !String.IsNullOrEmpty(fileName); });
            _viewModel.BindPropertyChanged("Passphrase", (string p) => { PassphraseTextBox.Text = p; });
            _viewModel.BindPropertyChanged("Verification", (string p) => { VerifyPassphraseTextbox.Text = p; });
        }

        private void EncryptPassphraseDialog_Load(object sender, EventArgs e)
        {
            if (_viewModel.IdentityName.Length != 0)
            {
                NameTextBox.Text = _viewModel.IdentityName;
                PassphraseTextBox.Focus();
            }
            else
            {
                NameTextBox.Focus();
            }
        }

        private void SetAutoValidateViaReflectionToAvoidMoMaWarning()
        {
            if (OS.Current.Platform == Platform.WindowsDesktop)
            {
                PropertyInfo propertyInfo = typeof(NewPassphraseDialog).GetProperty("AutoValidate");
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

        private void VerifyPassphraseTextbox_Validating(object sender, CancelEventArgs e)
        {
            if (_viewModel["Verification"].Length > 0)
            {
                e.Cancel = true;
                _errorProvider1.SetError(VerifyPassphraseTextbox, Resources.PassphraseVerificationMismatch);
            }
            if (_viewModel["Passphrase"].Length > 0)
            {
                e.Cancel = true;
                _errorProvider1.SetError(PassphraseTextBox, Resources.WrongPassphrase);
            }
        }

        private void VerifyPassphraseTextbox_Validated(object sender, EventArgs e)
        {
            _errorProvider1.Clear();
        }

        private void NameTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (_viewModel["IdentityName"].Length > 0)
            {
                e.Cancel = true;
                _errorProvider2.SetError(NameTextBox, Resources.LogOnExists);
            }
        }

        private void NameTextBox_Validated(object sender, EventArgs e)
        {
            _errorProvider2.Clear();
        }
    }
}