using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Texts = AxCrypt.Content.Texts;

namespace Axantum.AxCrypt
{
    public partial class ImportPrivatePasswordDialog : StyledMessageBase
    {
        private ImportPrivateKeysViewModel _viewModel;

        public ImportPrivatePasswordDialog()
        {
            InitializeComponent();
        }

        public ImportPrivatePasswordDialog(Form parent, IUserSettings userSettings, KnownIdentities knownIdentities)
            : this()
        {
            InitializeStyle(parent);

            _viewModel = new ImportPrivateKeysViewModel(userSettings, knownIdentities);

            _privateKeyFileTextBox.TextChanged += (sender, e) => { _viewModel.PrivateKeyFileName = _privateKeyFileTextBox.Text; };
            _passphraseTextBox.TextChanged += (sender, e) => { _viewModel.Passphrase = _passphraseTextBox.Text; _privateKeyFileTextBox.ScrollToCaret(); };
            _showPassphraseCheckBox.CheckedChanged += (sender, e) => { _viewModel.ShowPassphrase = _showPassphraseCheckBox.Checked; };

            _viewModel.BindPropertyChanged<bool>(nameof(ImportPrivateKeysViewModel.ImportSuccessful), (ok) => { if (!ok) { _errorProvider1.SetError(_browsePrivateKeyFileButton, Texts.FailedPrivateImport); } });
            _viewModel.BindPropertyChanged<bool>(nameof(ImportPrivateKeysViewModel.ShowPassphrase), (show) => { _showPassphraseCheckBox.Checked = show; _passphraseTextBox.UseSystemPasswordChar = !show; });
        }

        protected override void InitializeContentResources()
        {
            Text = Texts.DialogImportPrivateAxCryptIdTitle;

            PassphraseGroupBox.Text = Texts.PassphrasePrompt;
            _showPassphraseCheckBox.Text = Texts.ShowPasswordOptionPrompt;
            _buttonCancel.Text = "&" + Texts.ButtonCancelText;
            _buttonOk.Text = "&" + Texts.ButtonOkText;
            _accessIdGroupBox.Text = Texts.DialogImportPrivateAxCryptIdAccessIdPrompt;
            _browsePrivateKeyFileButton.Text = Texts.ButtonEllipsisText;
        }

        private void _buttonOk_Click(object sender, EventArgs e)
        {
            if (!AdHocValidationDueToMonoLimitations())
            {
                DialogResult = DialogResult.None;
                return;
            }
            _viewModel.ImportFile.Execute(null);
            if (!_viewModel.ImportSuccessful)
            {
                DialogResult = DialogResult.None;
                return;
            }
            DialogResult = DialogResult.OK;
        }

        private bool AdHocValidationDueToMonoLimitations()
        {
            bool validated = true;

            if (_viewModel[nameof(ImportPrivateKeysViewModel.Passphrase)].Length > 0)
            {
                _errorProvider1.SetError(_passphraseTextBox, Texts.WrongPassphrase);
                validated = false;
            }
            else
            {
                _errorProvider1.Clear();
            }

            if (_viewModel[nameof(ImportPrivateKeysViewModel.PrivateKeyFileName)].Length > 0)
            {
                _errorProvider2.SetError(_browsePrivateKeyFileButton, Texts.FileNotFound);
                validated = false;
            }
            else
            {
                _errorProvider2.Clear();
            }

            return validated;
        }

        private void _browsePrivateKeyFileButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = Texts.ImportPrivateKeysFileSelectionTitle;
                ofd.Multiselect = false;
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;
                ofd.Filter = Texts.ImportPrivateKeysFileFilter;
                DialogResult result = ofd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    _privateKeyFileTextBox.Text = ofd.FileName;
                    _privateKeyFileTextBox.SelectionStart = ofd.FileName.Length;
                    _privateKeyFileTextBox.SelectionLength = 1;
                    _passphraseTextBox.Focus();
                }
            }
        }
    }
}