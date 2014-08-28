using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    public partial class CreateNewAccountDialog : Form
    {
        private CreateNewAccountViewModel _viewModel;

        public CreateNewAccountDialog(Form parent, string passphrase)
        {
            InitializeComponent();
            _viewModel = new CreateNewAccountViewModel(Instance.AsymmetricKeysStore, passphrase);
            PassphraseTextBox.TextChanged += (sender, e) => { _viewModel.Passphrase = PassphraseTextBox.Text; };
            VerifyPassphraseTextbox.TextChanged += (sender, e) => { _viewModel.Verification = VerifyPassphraseTextbox.Text; };
            EmailTextBox.TextChanged += (sender, e) => { _viewModel.UserEmail = EmailTextBox.Text; };
            ShowPassphraseCheckBox.CheckedChanged += (sender, e) => { _viewModel.ShowPassphrase = ShowPassphraseCheckBox.Checked; };

            Owner = parent;
            Owner.Activated += (sender, e) => Activate();
            StartPosition = FormStartPosition.CenterParent;
        }

        private void CreateNewAccountDialog_Load(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            _viewModel.BindPropertyChanged("ShowPassphrase", (bool show) => { PassphraseTextBox.UseSystemPasswordChar = VerifyPassphraseTextbox.UseSystemPasswordChar = !(ShowPassphraseCheckBox.Checked = show); });
            _viewModel.BindPropertyChanged("Passphrase", (string p) => { PassphraseTextBox.Text = p; });
            _viewModel.BindPropertyChanged("Verification", (string v) => { VerifyPassphraseTextbox.Text = v; });

            EmailTextBox.Focus();
        }

        private void _buttonOk_Click(object sender, EventArgs e)
        {
            if (!AdHocValidationDueToMonoLimitations())
            {
                DialogResult = DialogResult.None;
                return;
            }
            _viewModel.CreateAccount.Execute(null);
        }

        private bool AdHocValidationDueToMonoLimitations()
        {
            bool validated = true;
            if (_viewModel["Passphrase"].Length > 0)
            {
                _errorProvider1.SetError(PassphraseTextBox, Resources.WrongPassphrase);
                validated = false;
            }
            else
            {
                _errorProvider1.Clear();
            }
            if (_viewModel["Verification"].Length > 0)
            {
                _errorProvider2.SetError(VerifyPassphraseTextbox, Resources.PassphraseVerificationMismatch);
                validated = false;
            }
            else
            {
                _errorProvider2.Clear();
            }
            if (_viewModel["UserEmail"].Length > 0)
            {
                _errorProvider3.SetError(EmailTextBox, Resources.BadEmail);
                validated = false;
            }
            else
            {
                _errorProvider3.Clear();
            }
            return validated;
        }
    }
}