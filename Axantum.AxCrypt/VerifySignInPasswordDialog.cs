using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Forms;
using AxCrypt.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    public partial class VerifySignInPasswordDialog : StyledMessageBase
    {
        private readonly VerifySignInPasswordViewModel _viewModel;

        private readonly string _verifyInstructionText;

        public VerifySignInPasswordDialog()
        {
            InitializeComponent();
        }

        public VerifySignInPasswordDialog(Form owner, VerifySignInPasswordViewModel viewModel, string verifyInstructionText)
            : this()
        {
            InitializeStyle(owner);

            _viewModel = viewModel;

            _verifyInstructionText = verifyInstructionText;
        }

        private void VerifySignInPasswordDialog_Load(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            _passphraseTextBox.LostFocus += (s, ea) => { _viewModel.PasswordText = _passphraseTextBox.Text; };
            _passphraseTextBox.TextChanged += (s, ea) => { ClearErrorProviders(); };
            _passphraseTextBox.Validating += (s, ea) => { _viewModel.PasswordText = _passphraseTextBox.Text; };
            _showPassphraseCheckBox.CheckedChanged += (s, ea) => { _viewModel.ShowPassword = _showPassphraseCheckBox.Checked; };

            _viewModel.BindPropertyChanged(nameof(_viewModel.ShowPassword), (bool show) => { _passphraseTextBox.UseSystemPasswordChar = !(_showPassphraseCheckBox.Checked = show); });
        }

        protected override void InitializeContentResources()
        {
            Text = Texts.SignInVerificationTitle;

            _verifyInstructionLabel.Text = _verifyInstructionText;
            _passphraseGroupBox.Text = Texts.VerifyPasswordPrompt;
            _showPassphraseCheckBox.Text = Texts.ShowPasswordOptionPrompt;
            _buttonCancel.Text = "&" + Texts.ButtonCancelText;
            _buttonOk.Text = "&" + Texts.ButtonOkText;
        }

        private void ButtonOk_Click(object sender, EventArgs e)
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
            bool validated = AdHocValidatePassphrase();

            return validated;
        }

        private bool AdHocValidatePassphrase()
        {
            _errorProvider1.Clear();
            if (_viewModel[nameof(_viewModel.PasswordText)].Length != 0)
            {
                _errorProvider1.SetError(_passphraseTextBox, Texts.WrongPassphrase);
                return false;
            }
            return true;
        }

        private void VerifySignInPasswordDialog_Activated(object sender, EventArgs e)
        {
            BringToFront();
            _passphraseTextBox.Focus();
        }

        private void PassphraseTextBox_Enter(object sender, EventArgs e)
        {
            _errorProvider1.Clear();
        }

        private void VerifySignInPasswordDialog_ResizeAndMoveEnd(object sender, EventArgs e)
        {
            CenterToParent();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
        }

        private void ClearErrorProviders()
        {
            _errorProvider1.Clear();
        }
    }
}