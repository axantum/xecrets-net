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
using Axantum.AxCrypt.Core.Extensions;
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
    public partial class LogOnDialog : Form
    {
        private LogOnViewModel _viewModel;

        public LogOnDialog(string identityName, string encryptedFileFullName)
        {
            InitializeComponent();
            SetAutoValidateViaReflectionToAvoidMoMaWarning();

            _viewModel = new LogOnViewModel(identityName, encryptedFileFullName);

            PassphraseTextBox.TextChanged += (sender, e) => { _viewModel.Passphrase = PassphraseTextBox.Text; };
            ShowPassphraseCheckBox.CheckedChanged += (sender, e) => { _viewModel.ShowPassphrase = ShowPassphraseCheckBox.Checked; };

            _viewModel.BindPropertyChanged("IdentityName", (string id) => { PassphraseGroupBox.Text = !String.IsNullOrEmpty(id) ? Resources.EnterPassphraseForIdentityPrompt.InvariantFormat(id) : Resources.PassphrasePrompt; });
            _viewModel.BindPropertyChanged("ShowPassphrase", (bool show) => { PassphraseTextBox.UseSystemPasswordChar = !show; });
            _viewModel.BindPropertyChanged("FileName", (string fileName) => { FileNameTextBox.Text = fileName; FileNamePanel.Visible = !String.IsNullOrEmpty(fileName); });
        }

        private void EncryptPassphraseDialog_Load(object sender, EventArgs e)
        {
        }

        private void SetAutoValidateViaReflectionToAvoidMoMaWarning()
        {
            if (OS.Current.Platform == Platform.WindowsDesktop)
            {
                PropertyInfo propertyInfo = typeof(LogOnDialog).GetProperty("AutoValidate");
                propertyInfo.SetValue(this, AutoValidate.EnableAllowFocusChange, null);
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            if (!ValidateChildren(ValidationConstraints.Visible))
            {
                DialogResult = DialogResult.None;
                return;
            }
            if (!String.IsNullOrEmpty(_viewModel.FileName) && String.IsNullOrEmpty(_viewModel.IdentityName))
            {
                DialogResult = DialogResult.Retry;
            }
        }

        private void PassphraseTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (_viewModel["Passphrase"].Length > 0)
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