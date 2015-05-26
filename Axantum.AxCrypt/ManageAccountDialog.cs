using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

        public ManageAccountDialog(UserAsymmetricKeysStore keysStore, IUserSettings userSettings)
        {
            InitializeComponent();

            _userSettings = userSettings;
            _viewModel = new ManageAccountViewModel(keysStore);
            _viewModel.BindPropertyChanged<IEnumerable<AccountEmail>>("AccountEmails", ListAccountEmails);
        }

        private void ListAccountEmails(IEnumerable<AccountEmail> emails)
        {
            _accountEmailsListView.Items.Clear();
            foreach (AccountEmail email in emails)
            {
                ListViewItem item = new ListViewItem(email.EmailAddress);
                item.Name = "Email";

                ListViewItem.ListViewSubItem timestampColumn = item.SubItems.Add(String.Empty);
                timestampColumn.Name = "Timestamp";
                timestampColumn.Text = email.Timestamp.ToLocalTime().ToString(CultureInfo.CurrentCulture);

                _accountEmailsListView.Items.Add(item);
            }
        }

        private void _changePassphraseButton_Click(object sender, EventArgs e)
        {
            string passphrase;
            using (NewPassphraseDialog dialog = new NewPassphraseDialog(this, String.Empty, String.Empty))
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
    }
}