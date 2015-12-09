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

using Axantum.AxCrypt.Core.UI.ViewModel;
using System;
using System.Linq;
using System.Windows.Forms;

using Content = AxCrypt.Content.Content;

namespace Axantum.AxCrypt
{
    public partial class NewPassphraseDialog : StyledMessageBase
    {
        private NewPassphraseViewModel _viewModel;

        public NewPassphraseDialog()
        {
            InitializeComponent();
        }

        public NewPassphraseDialog(Form parent, string title, string passphrase, string encryptedFileFullName)
            : this()
        {
            InitializeStyle(parent);

            Text = title;
            _viewModel = new NewPassphraseViewModel(passphrase, encryptedFileFullName);
            PassphraseTextBox.TextChanged += (sender, e) => { _viewModel.Passphrase = PassphraseTextBox.Text; };
            VerifyPassphraseTextbox.TextChanged += (sender, e) => { _viewModel.Verification = VerifyPassphraseTextbox.Text; };
            ShowPassphraseCheckBox.CheckedChanged += (sender, e) => { _viewModel.ShowPassphrase = ShowPassphraseCheckBox.Checked; };
        }

        protected override void InitializeContentResources()
        {
            Text = Content.DialogNewPasswordTitle;

            PassphraseGroupBox.Text = Content.PassphrasePrompt;
            ShowPassphraseCheckBox.Text = Content.ShowPasswordOptionPrompt;

            _fileGroupBox.Text = Content.PromptFileText;
            _buttonCancel.Text = Content.ButtonCancelText;
            _buttonOk.Text = Content.ButtonOkText;
            _verifyPasswordLabel.Text = Content.VerifyPasswordPrompt;
        }

        private void EncryptPassphraseDialog_Load(object s, EventArgs ee)
        {
            if (DesignMode)
            {
                return;
            }

            _viewModel.BindPropertyChanged(nameof(NewPassphraseViewModel.ShowPassphrase), (bool show) => { PassphraseTextBox.UseSystemPasswordChar = VerifyPassphraseTextbox.UseSystemPasswordChar = !show; });
            _viewModel.BindPropertyChanged(nameof(NewPassphraseViewModel.FileName), (string fileName) => { FileNameTextBox.Text = fileName; FileNamePanel.Visible = !String.IsNullOrEmpty(fileName); });
            _viewModel.BindPropertyChanged(nameof(NewPassphraseViewModel.Passphrase), (string p) => { PassphraseTextBox.Text = p; });
            _viewModel.BindPropertyChanged(nameof(NewPassphraseViewModel.Verification), (string p) => { VerifyPassphraseTextbox.Text = p; });

            PassphraseTextBox.Focus();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            if (!AdHocValidationDueToMonoLimitations())
            {
                DialogResult = DialogResult.None;
            }
        }

        private bool AdHocValidationDueToMonoLimitations()
        {
            bool validated = true;
            if (_viewModel[nameof(NewPassphraseViewModel.Verification)].Length > 0)
            {
                _errorProvider1.SetError(VerifyPassphraseTextbox, Content.PassphraseVerificationMismatch);
                validated = false;
            }
            if (_viewModel[nameof(NewPassphraseViewModel.Passphrase)].Length > 0)
            {
                _errorProvider1.SetError(PassphraseTextBox, Content.WrongPassphrase);
                validated = false;
            }
            if (validated)
            {
                _errorProvider1.Clear();
            }
            _errorProvider2.Clear();
            return validated;
        }
    }
}