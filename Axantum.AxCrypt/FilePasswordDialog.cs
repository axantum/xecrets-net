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
    public partial class FilePasswordDialog : StyledMessageBase
    {
        private LogOnViewModel _viewModel;

        public FilePasswordDialog()
        {
            InitializeComponent();
        }

        public FilePasswordDialog(Form parent, string encryptedFileFullName)
            : this()
        {
            InitializeStyle(parent);

            _viewModel = new LogOnViewModel(encryptedFileFullName);
            PassphraseTextBox.TextChanged += (sender, e) => { _viewModel.Passphrase = PassphraseTextBox.Text; };
            ShowPassphraseCheckBox.CheckedChanged += (sender, e) => { _viewModel.ShowPassphrase = ShowPassphraseCheckBox.Checked; };
            _newButton.Enabled = String.IsNullOrEmpty(encryptedFileFullName);
        }

        protected override void InitializeContentResources()
        {
            Text = Content.DialogFilePasswordTitle;

            PassphraseGroupBox.Text = Content.PassphrasePrompt;
            ShowPassphraseCheckBox.Text = Content.ShowPasswordOptionPrompt;
            _newButton.Text = Content.ButtonNewText;
            _buttonCancel.Text = Content.ButtonCancelText;
            _buttonOk.Text = Content.ButtonOkText;
            _fileGroupBox.Text = Content.DialogFilePasswordFilePromptText;
        }

        private void EncryptPassphraseDialog_Load(object s, EventArgs ea)
        {
            if (DesignMode)
            {
                return;
            }

            _viewModel.BindPropertyChanged(nameof(LogOnViewModel.ShowPassphrase), (bool show) => { PassphraseTextBox.UseSystemPasswordChar = !show; });
            _viewModel.BindPropertyChanged(nameof(LogOnViewModel.FileName), (string fileName) => { FileNameTextBox.Text = fileName; FileNamePanel.Visible = !String.IsNullOrEmpty(fileName); });
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            if (!AdHocValidationDueToMonoLimitations())
            {
                DialogResult = DialogResult.None;
                return;
            }
            DialogResult = DialogResult.OK;
        }

        private bool AdHocValidationDueToMonoLimitations()
        {
            if (_viewModel[nameof(LogOnViewModel.Passphrase)].Length == 0)
            {
                _errorProvider1.Clear();
                return true;
            }
            if (String.IsNullOrEmpty(_viewModel.FileName))
            {
                _errorProvider1.SetError(PassphraseTextBox, Content.UnkownLogOn);
            }
            else
            {
                _errorProvider1.SetError(PassphraseTextBox, Content.WrongPassphrase);
            }
            return false;
        }

        private void newButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Retry;
        }

        private void LogOnDialog_Activated(object sender, EventArgs e)
        {
            BringToFront();
            Focus();
        }

        private void PassphraseTextBox_Enter(object sender, EventArgs e)
        {
            _errorProvider1.Clear();
        }
    }
}