﻿#region Coypright and License

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

using Texts = AxCrypt.Content.Texts;

namespace Axantum.AxCrypt
{
    public partial class FilePasswordDialog : StyledMessageBase
    {
        private FilePasswordViewModel _viewModel;

        public FilePasswordDialog()
        {
            InitializeComponent();
        }

        public FilePasswordDialog(Form parent, string encryptedFileFullName)
            : this()
        {
            InitializeStyle(parent);

            _viewModel = new FilePasswordViewModel(encryptedFileFullName);
            PassphraseTextBox.TextChanged += (sender, e) => { _viewModel.Passphrase = PassphraseTextBox.Text; };
            ShowPassphraseCheckBox.CheckedChanged += (sender, e) => { _viewModel.ShowPassphrase = ShowPassphraseCheckBox.Checked; };
        }

        protected override void InitializeContentResources()
        {
            Text = Texts.DialogFilePasswordTitle;

            _passphraseGroupBox.Text = Texts.PassphrasePrompt;
            ShowPassphraseCheckBox.Text = Texts.ShowPasswordOptionPrompt;
            _cancelButton.Text = Texts.ButtonCancelText;
            _okButton.Text = Texts.ButtonOkText;
            _fileNameGroupBox.Text = Texts.PromptFileText;
            _keyFileGroupBox.Text = Texts.KeyFilePrompt;
        }

        private void EncryptPassphraseDialog_Load(object s, EventArgs ea)
        {
            if (DesignMode)
            {
                return;
            }

            _viewModel.BindPropertyChanged(nameof(FilePasswordViewModel.ShowPassphrase), (bool show) => { PassphraseTextBox.UseSystemPasswordChar = !show; });
            _viewModel.BindPropertyChanged(nameof(FilePasswordViewModel.FileName), (string fileName) => { FileNameTextBox.Text = fileName; FileNamePanel.Visible = !String.IsNullOrEmpty(fileName); });
            _viewModel.BindPropertyChanged(nameof(FilePasswordViewModel.AskForKeyFile), (bool askForKeyFile) => { KeyFilePanel.Visible = askForKeyFile; });
        }

        private void OkButton_Click(object sender, EventArgs e)
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
            if (_viewModel[nameof(FilePasswordViewModel.Passphrase)].Length == 0)
            {
                _errorProvider1.Clear();
                return true;
            }
            if (String.IsNullOrEmpty(_viewModel.FileName))
            {
                _errorProvider1.SetError(PassphraseTextBox, Texts.UnkownLogOn);
            }
            else
            {
                _errorProvider1.SetError(PassphraseTextBox, Texts.WrongPassphrase);
            }
            return false;
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

        private void KeyFileBrowseForButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = Texts.KeyFileBrowseTitle;
                ofd.Multiselect = false;
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;
                ofd.Filter = Texts.KeyFileBrowseFilter;
                DialogResult result = ofd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    KeyFileTextBox.Text = ofd.FileName;
                    KeyFileTextBox.SelectionStart = ofd.FileName.Length;
                    KeyFileTextBox.SelectionLength = 1;
                    KeyFileTextBox.Focus();
                }
            }
        }
    }
}