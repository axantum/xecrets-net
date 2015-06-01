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
    public partial class LogOnAccountDialog : Form
    {
        public LogOnAccountDialog()
        {
            InitializeComponent();
        }

        private LogOnAccountViewModel _viewModel;

        public LogOnAccountDialog(Form parent)
        {
            InitializeComponent();

            _viewModel = new LogOnAccountViewModel(String.Empty);

            PassphraseTextBox.TextChanged += (sender, e) => { _viewModel.Passphrase = PassphraseTextBox.Text; };
            ShowPassphraseCheckBox.CheckedChanged += (sender, e) => { _viewModel.ShowPassphrase = ShowPassphraseCheckBox.Checked; };
            EmailTextBox.TextChanged += (sender, e) => { _viewModel.UserEmail = EmailTextBox.Text; };

            Owner = parent;
            StartPosition = FormStartPosition.CenterParent;
        }

        private void LogOnAccountDialog_Load(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            _viewModel.BindPropertyChanged("ShowPassphrase", (bool show) => { PassphraseTextBox.UseSystemPasswordChar = !(ShowPassphraseCheckBox.Checked = show); });
            _viewModel.BindPropertyChanged("ShowEmail", (bool show) => { EmailPanel.Visible = show; });
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
            bool validated = true;
            _errorProvider1.Clear();
            _errorProvider2.Clear();

            if (_viewModel["Passphrase"].Length != 0)
            {
                _errorProvider1.SetError(PassphraseTextBox, EmailTextBox.Text.Length > 0 ? Resources.WrongPassphrase : Resources.UnkownLogOn);
                validated = false;
            }
            if (_viewModel["UserEmail"].Length != 0)
            {
                _errorProvider2.SetError(EmailTextBox, Resources.BadEmail);
                validated = false;
            }
            return validated;
        }

        private void NewButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Retry;
        }

        private void LogOnAccountDialog_Activated(object sender, EventArgs e)
        {
            TopMost = true;
            BringToFront();
            Focus();
        }

        private void PassphraseTextBox_Enter(object sender, EventArgs e)
        {
            _errorProvider1.Clear();
        }

        private void LogOnAccountDialog_ResizeAndMoveEnd(object sender, EventArgs e)
        {
            CenterToParent();
        }
    }
}