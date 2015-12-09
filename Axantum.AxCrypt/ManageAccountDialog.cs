using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Axantum.AxCrypt.Abstractions.TypeResolve;
using Content = AxCrypt.Content.Content;

namespace Axantum.AxCrypt
{
    public partial class ManageAccountDialog : StyledMessageBase
    {
        private ManageAccountViewModel _viewModel;

        private IUserSettings _userSettings;

        public ManageAccountDialog()
        {
            InitializeComponent();
        }

        public ManageAccountDialog(Form parent, KnownIdentities knownIdentities, IUserSettings userSettings)
            : this()
        {
            if (knownIdentities == null)
            {
                throw new ArgumentNullException(nameof(knownIdentities));
            }

            InitializeStyle(parent);

            _userSettings = userSettings;
            AccountStorage userKeyPairs = new AccountStorage(New<LogOnIdentity, IAccountService>(Resolve.KnownIdentities.DefaultEncryptionIdentity));
            _viewModel = new ManageAccountViewModel(userKeyPairs, knownIdentities);
            _viewModel.BindPropertyChanged<IEnumerable<AccountProperties>>(nameof(ManageAccountViewModel.AccountProperties), ListAccountEmails);
        }

        protected override void InitializeContentResources()
        {
            Text = Content.DialogManageAxcryptIdTitle;

            _changePassphraseButton.Text = Content.ButtonChangePasswordText;
            _dateHeader.Text = Content.ColumnTimestampHeader;
        }

        private void ListAccountEmails(IEnumerable<AccountProperties> emails)
        {
            _accountEmailsListView.Items.Clear();
            foreach (AccountProperties email in emails)
            {
                ListViewItem item = new ListViewItem(email.Timestamp.ToLocalTime().ToString(CultureInfo.CurrentCulture));
                item.Name = "Timestamp";

                _accountEmailsListView.Items.Add(item);
            }

            if (_accountEmailsListView.Items.Count == 0)
            {
                _emailLabel.Text = String.Empty;
                return;
            }

            _accountEmailsListView.Columns[0].Width = _accountEmailsListView.ClientSize.Width;
            _emailLabel.Text = emails.First().EmailAddress;
        }

        private void _changePassphraseButton_Click(object sender, EventArgs e)
        {
            string passphrase;
            using (NewPassphraseDialog dialog = new NewPassphraseDialog(this, Content.ChangePassphraseDialogTitle, String.Empty, String.Empty))
            {
                dialog.ShowPassphraseCheckBox.Checked = _userSettings.DisplayEncryptPassphrase;
                DialogResult dialogResult = dialog.ShowDialog(this);
                if (dialogResult != DialogResult.OK || dialog.PassphraseTextBox.Text.Length == 0)
                {
                    return;
                }
                _userSettings.DisplayEncryptPassphrase = dialog.ShowPassphraseCheckBox.Checked;
                passphrase = dialog.PassphraseTextBox.Text;
            }
            _viewModel.ChangePassphrase.Execute(passphrase);
        }

        private void ManageAccountDialog_Load(object sender, EventArgs e)
        {
        }

        private void _accountEmailsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
    }
}