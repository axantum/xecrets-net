using AxCrypt.Core;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.Service;
using AxCrypt.Core.Session;
using AxCrypt.Core.UI;
using AxCrypt.Core.UI.ViewModel;
using AxCrypt.Forms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AxCrypt.Abstractions.TypeResolve;
using Texts = AxCrypt.Content.Texts;

namespace AxCrypt.Desktop.Window
{
    public partial class ManageAccountDialog : StyledMessageBase
    {
        private ManageAccountViewModel _viewModel;

        public ManageAccountDialog()
        {
            InitializeComponent();
        }

        public static async Task<ManageAccountDialog> CreateAsync(Form parent)
        {
            ManageAccountDialog mad = new ManageAccountDialog();

            mad.InitializeStyle(parent);

            AccountStorage userKeyPairs = new AccountStorage(New<LogOnIdentity, IAccountService>(Resolve.KnownIdentities.DefaultEncryptionIdentity));
            mad._viewModel = await ManageAccountViewModel.CreateAsync(userKeyPairs);
            mad._viewModel.BindPropertyChanged<IEnumerable<AccountProperties>>(nameof(ManageAccountViewModel.AccountProperties), mad.ListAccountEmails);

            return mad;
        }

        protected override void InitializeContentResources()
        {
            Text = Texts.DialogManageAxcryptIdTitle;

            _changePassphraseButton.Text = Texts.ButtonChangePasswordText;
            _dateHeader.Text = Texts.ColumnTimestampHeader;
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

        private async void _changePassphraseButton_Click(object sender, EventArgs e)
        {
            New<UserSettings>().UserEmail.ProcessChangePassword();
        }

        private void ManageAccountDialog_Load(object sender, EventArgs e)
        {
        }

        private void _accountEmailsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
    }
}