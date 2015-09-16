using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Forms.Style;
using Axantum.AxCrypt.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    public partial class CreateNewAccountDialog : Form
    {
        private CreateNewAccountViewModel _viewModel;

        private bool _isCreating = false;

        public CreateNewAccountDialog(Form parent, string passphrase)
        {
            InitializeComponent();
            new Styling(Resources.axcrypticon).Style(this);

            _viewModel = new CreateNewAccountViewModel(Resolve.AsymmetricKeysStore, passphrase);
            PassphraseTextBox.TextChanged += (sender, e) => { _viewModel.Passphrase = PassphraseTextBox.Text; };
            VerifyPassphraseTextbox.TextChanged += (sender, e) => { _viewModel.Verification = VerifyPassphraseTextbox.Text; };
            EmailTextBox.LostFocus += (sender, e) => { _viewModel.UserEmail = EmailTextBox.Text; AdHocValidateUserEmail(); };
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
            if (_isCreating || !AdHocValidationDueToMonoLimitations())
            {
                DialogResult = DialogResult.None;
                return;
            }

            CreateAccountAsync();
        }

        private async void CreateAccountAsync()
        {
            UseWaitCursor = true;
            _isCreating = true;

            try
            {
                await Task.Run(() => _viewModel.CreateAccount.Execute(null));
            }
            finally
            {
                _isCreating = false;
                UseWaitCursor = false;
            }

            DialogResult = DialogResult.OK;
        }

        private bool AdHocValidationDueToMonoLimitations()
        {
            bool validated = AdHocValidateAllFieldsIndependently();
            return validated;
        }

        private bool AdHocValidateAllFieldsIndependently()
        {
            return AdHocValidatePassphrase() & AdHocValidateVerfication() & AdHocValidateUserEmail();
        }

        private bool AdHocValidatePassphrase()
        {
            _errorProvider1.Clear();
            if (_viewModel["Passphrase"].Length > 0)
            {
                _errorProvider1.SetError(PassphraseTextBox, Resources.WrongPassphrase);
                return false;
            }
            return true;
        }

        private bool AdHocValidateVerfication()
        {
            _errorProvider2.Clear();
            if (_viewModel["Verification"].Length > 0)
            {
                _errorProvider2.SetError(VerifyPassphraseTextbox, Resources.PassphraseVerificationMismatch);
                return false;
            }
            return true;
        }

        private bool AdHocValidateUserEmail()
        {
            _errorProvider3.Clear();
            if (_viewModel["UserEmail"].Length > 0)
            {
                _errorProvider3.SetError(EmailTextBox, Resources.BadEmail);
                return false;
            }
            return true;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            if (_isCreating)
            {
                e.Cancel = true;
                MessageBox.Show(this, Resources.OfflineAccountBePatient, Resources.OfflineAccountTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            base.OnFormClosing(e);
        }
    }
}