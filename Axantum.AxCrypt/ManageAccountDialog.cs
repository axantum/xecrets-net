using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Abstractions.Rest;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Forms.Style;
using Axantum.AxCrypt.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    public partial class ManageAccountDialog : Form
    {
        private ManageAccountViewModel _viewModel;

        private IUserSettings _userSettings;

        public ManageAccountDialog(KnownIdentities knownIdentities, IUserSettings userSettings)
        {
            if (knownIdentities == null)
            {
                throw new ArgumentNullException(nameof(knownIdentities));
            }

            InitializeComponent();
            new Styling(Resources.axcrypticon).Style(this);

            _userSettings = userSettings;
            UserAsymmetricKeysStore userKeyPairs = new UserAsymmetricKeysStore(TypeMap.Resolve.New<RestIdentity, IAccountService>(new RestIdentity(Resolve.KnownIdentities.DefaultEncryptionIdentity.UserEmail.Address, Resolve.KnownIdentities.DefaultEncryptionIdentity.Passphrase.Text)));
            _viewModel = new ManageAccountViewModel(userKeyPairs, knownIdentities);
            _viewModel.BindPropertyChanged<IEnumerable<AccountProperties>>("AccountEmails", ListAccountEmails);
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
            using (NewPassphraseDialog dialog = new NewPassphraseDialog(this, Resources.ChangePassphraseDialogTitle, String.Empty, String.Empty))
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