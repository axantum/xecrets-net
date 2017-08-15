using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static Axantum.AxCrypt.Abstractions.TypeResolve;
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

        public ImportPrivatePasswordDialog(Form parent, UserSettings userSettings, KnownIdentities knownIdentities)
            : this()
        {
            InitializeStyle(parent);

            _viewModel = new ImportPrivateKeysViewModel(userSettings, knownIdentities);

            _privateKeyFileTextBox.TextChanged += (sender, e) => { _viewModel.PrivateKeyFileName = _privateKeyFileTextBox.Text; ClearErrorProviders(); };
            _passphraseTextBox.TextChanged += (sender, e) => { _viewModel.PasswordText = _passphraseTextBox.Text; _privateKeyFileTextBox.ScrollToCaret(); ClearErrorProviders(); };
            _showPassphraseCheckBox.CheckedChanged += (sender, e) => { _viewModel.ShowPassword = _showPassphraseCheckBox.Checked; };

            _viewModel.BindPropertyChanged<bool>(nameof(_viewModel.ImportSuccessful), (ok) => { if (!ok) { _errorProvider1.SetError(_browsePrivateKeyFileButton, Texts.FailedPrivateImport); } });
            _viewModel.BindPropertyChanged<bool>(nameof(_viewModel.ShowPassword), (show) => { _showPassphraseCheckBox.Checked = show; _passphraseTextBox.UseSystemPasswordChar = !show; });
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
            TaskRunner.WaitFor(() => _viewModel.ImportFile.ExecuteAsync(null));
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

            if (_viewModel[nameof(ImportPrivateKeysViewModel.PasswordText)].Length > 0)
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
                ofd.DefaultExt = New<IRuntimeEnvironment>().AxCryptExtension;
                ofd.Filter = Texts.FileFilterDialogFilterPatternWin.InvariantFormat(ofd.DefaultExt, Texts.FileFilterFileTypeAxCryptIdFiles, Texts.FileFilterFileTypeAllFiles);
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

        private void ClearErrorProviders()
        {
            _errorProvider1.Clear();
            _errorProvider2.Clear();
        }
    }
}