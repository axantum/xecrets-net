#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
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
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Forms;
using System;
using System.Linq;
using System.Windows.Forms;
using Texts = AxCrypt.Content.Texts;

namespace Axantum.AxCrypt
{
    public partial class FilePasswordDialog : StyledMessageBase
    {
        public FilePasswordDialog()
        {
            InitializeComponent();
        }

        public FilePasswordDialog(Form parent, string encryptedFileFullName)
            : this()
        {
            ViewModel = new FilePasswordViewModel(encryptedFileFullName);

            InitializeStyle(parent);
            InitializePropertyValues();
            BindPropertyChangedEvents();
        }

        public FilePasswordViewModel ViewModel { get; private set; }

        protected override void InitializeContentResources()
        {
            Text = Texts.DialogFilePasswordTitle;

            _passphraseGroupBox.Text = Texts.PassphrasePrompt;
            _showPassphraseCheckBox.Text = Texts.ShowPasswordOptionPrompt;
            _cancelButton.Text = "&" + Texts.ButtonCancelText;
            _okButton.Text = "&" + Texts.ButtonOkText;
            _moreButton.Text = Texts.MoreButtonText;
            _moreButtonToolTip.SetToolTip(_moreButton, Texts.MoreKeyFileToolTop);
            _fileNameGroupBox.Text = Texts.PromptFileText;
            _keyFileGroupBox.Text = Texts.KeyFilePrompt;
            KeyFilePanel.Visible = false;
        }

        private void InitializePropertyValues()
        {
            _passphraseTextBox.TextChanged += (sender, e) => { ViewModel.PasswordText = _passphraseTextBox.Text; ClearErrorProviders(); };
            _keyFileTextBox.TextChanged += (sender, e) => { ViewModel.KeyFileName = _keyFileTextBox.Text; };
            _showPassphraseCheckBox.CheckedChanged += (sender, e) => { ViewModel.ShowPassword = _showPassphraseCheckBox.Checked; };
        }

        private void BindPropertyChangedEvents()
        {
            ViewModel.BindPropertyChanged(nameof(FilePasswordViewModel.ShowPassword), (bool show) => { _passphraseTextBox.UseSystemPasswordChar = !show; _showPassphraseCheckBox.Checked = show; });
            ViewModel.BindPropertyChanged(nameof(FilePasswordViewModel.FileName), (string fileName) => { _fileNameTextBox.Text = fileName; FileNamePanel.Visible = !String.IsNullOrEmpty(fileName); });
            ViewModel.BindPropertyChanged(nameof(FilePasswordViewModel.IsLegacyFile), (bool isLegacy) => { _moreButton.Visible = isLegacy; });
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
            _errorProvider1.Clear();

            if (ViewModel[nameof(FilePasswordViewModel.KeyFileName)].Length > 0)
            {
                _errorProvider1.SetError(_keyFileBrowseForButton, Texts.FileNotFound);
                return false;
            }

            if (ViewModel[nameof(FilePasswordViewModel.PasswordText)].Length == 0)
            {
                return true;
            }

            if (String.IsNullOrEmpty(ViewModel.FileName))
            {
                _errorProvider1.SetError(_passphraseTextBox, Texts.UnknownLogOn);
            }
            else
            {
                _errorProvider1.SetError(_passphraseTextBox, ViewModel.ValidationError.ToValidationMessage());
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

        private void KeyFileTextBox_Enter(object sender, EventArgs e)
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
                    _keyFileTextBox.Text = ofd.FileName;
                    _keyFileTextBox.SelectionStart = ofd.FileName.Length;
                    _keyFileTextBox.SelectionLength = 1;
                    _keyFileTextBox.Focus();
                }
            }
        }

        private void MoreButton_Click(object sender, EventArgs e)
        {
            KeyFilePanel.Visible = true;
            _moreButton.Visible = false;
        }

        private void ClearErrorProviders()
        {
            _errorProvider1.Clear();
        }
    }
}