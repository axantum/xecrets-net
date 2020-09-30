using AxCrypt.Core.UI.ViewModel;
using AxCrypt.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

using Texts = AxCrypt.Content.Texts;

namespace AxCrypt.Desktop.Window
{
    public partial class EmailDialog : StyledMessageBase
    {
        private AccountEmailViewModel _viewModel;

        public EmailDialog()
        {
            InitializeComponent();
        }

        public EmailDialog(Form parent)
            : this()
        {
            InitializeStyle(parent);
            _viewModel = new AccountEmailViewModel();
        }

        protected override void InitializeContentResources()
        {
            Text = Texts.DialogAxCryptIdEmailTitle;

            _firstTimePromptlabel.Text = Texts.DialogAxCryptIdFirstTimeText;
            _buttonOk.Text = "&" + Texts.ButtonOkText;
            _buttonHelp.Text = "&" + Texts.ButtonHelpText;
            _emailGroupBox.Text = Texts.PromptEmailText;
        }

        private void EmailDialog_Load(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            EmailTextBox.LostFocus += (s, ea) => { _viewModel.UserEmail = EmailTextBox.Text; ClearErrorProviders(); };
            EmailTextBox.TextChanged += (s, ea) => { ClearErrorProviders(); };
            EmailTextBox.Focus();
        }

        private void EmailDialog_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            LaunchHelpPage();
        }

        private static void LaunchHelpPage()
        {
            Process.Start(@"https://www.axcrypt.net/documentation/get-started/");
        }

        private bool AdHocValidationDueToMonoLimitations()
        {
            bool validated = AdHocValidateAllFieldsIndependently();
            return validated;
        }

        private bool AdHocValidateAllFieldsIndependently()
        {
            return AdHocValidateUserEmail();
        }

        private bool AdHocValidateUserEmail()
        {
            _errorProvider1.Clear();
            if (_viewModel[nameof(AccountEmailViewModel.UserEmail)].Length > 0)
            {
                _errorProvider1.SetError(EmailTextBox, Texts.BadEmail);
                return false;
            }
            return true;
        }

        private void _buttonOk_Click(object sender, EventArgs e)
        {
            _viewModel.UserEmail = EmailTextBox.Text;
            if (!AdHocValidationDueToMonoLimitations())
            {
                DialogResult = DialogResult.None;
                return;
            }
        }

        private void ClearErrorProviders()
        {
            _errorProvider1.Clear();
        }

        private void _buttonHelp_Click(object sender, EventArgs e)
        {
            LaunchHelpPage();
        }
    }
}