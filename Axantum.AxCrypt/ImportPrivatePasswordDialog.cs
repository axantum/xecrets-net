using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Forms.Style;
using Axantum.AxCrypt.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    public partial class ImportPrivatePasswordDialog : Form
    {
        private ImportPrivateKeysViewModel _viewModel;

        public ImportPrivatePasswordDialog()
        {
            InitializeComponent();
            new Styling(Resources.axcrypticon).Style(this);
        }

        public ImportPrivatePasswordDialog(Form parent, UserAsymmetricKeysStore keysStore, IUserSettings userSettings, KnownIdentities knownIdentities)
            : this()
        {
            _viewModel = new ImportPrivateKeysViewModel(keysStore, userSettings, knownIdentities);

            _privateKeyFileTextBox.TextChanged += (sender, e) => { _viewModel.PrivateKeyFileName = _privateKeyFileTextBox.Text; };
            _passphraseTextBox.TextChanged += (sender, e) => { _viewModel.Passphrase = _passphraseTextBox.Text; _privateKeyFileTextBox.ScrollToCaret(); };
            _showPassphraseCheckBox.CheckedChanged += (sender, e) => { _viewModel.ShowPassphrase = _showPassphraseCheckBox.Checked; };

            _viewModel.BindPropertyChanged<bool>("ImportSuccessful", (ok) => { if (!ok) { _errorProvider1.SetError(_browsePrivateKeyFileButton, Resources.FailedPrivateImport); } });
            _viewModel.BindPropertyChanged<bool>("ShowPassphrase", (show) => { _showPassphraseCheckBox.Checked = show; _passphraseTextBox.UseSystemPasswordChar = !show; });

            Owner = parent;
            Owner.Activated += (sender, e) => Activate();
            StartPosition = FormStartPosition.CenterParent;
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

            if (_viewModel["Passphrase"].Length > 0)
            {
                _errorProvider1.SetError(_passphraseTextBox, Resources.WrongPassphrase);
                validated = false;
            }
            else
            {
                _errorProvider1.Clear();
            }

            if (_viewModel["PrivateKeyFileName"].Length > 0)
            {
                _errorProvider2.SetError(_browsePrivateKeyFileButton, Resources.FileNotFound);
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
                ofd.Title = Resources.ImportPrivateKeysFileSelectionTitle;
                ofd.Multiselect = false;
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;
                ofd.Filter = Resources.ImportPrivateKeysFileFilter;
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