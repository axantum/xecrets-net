using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    public partial class KeyShareDialog : Form
    {
        private SharingListViewModel _viewModel;

        public KeyShareDialog(Func<KnownPublicKeys> knownPublicKeysFactory, LogOnIdentity logOnIdentity)
        {
            InitializeComponent();
            new Styling().Style(this);

            _viewModel = new SharingListViewModel(knownPublicKeysFactory, logOnIdentity);
            _viewModel.BindPropertyChanged<IEnumerable<EmailAddress>>("AddedKeyShares", (aks) => { _sharedKeysCheckListBox.Items.Clear(); _sharedKeysCheckListBox.Items.AddRange(aks.ToArray()); });
            _viewModel.BindPropertyChanged<IEnumerable<EmailAddress>>("KnownKeyShares", (aks) => { _contactsComboBox.Items.Clear(); _contactsComboBox.Items.AddRange(aks.ToArray()); _contactsComboBox.Text = String.Empty; });

            _shareButton.Enabled = false;
            _contactsComboBox.SelectedIndexChanged += (sender, e) => _shareButton.Enabled = _contactsComboBox.SelectedIndex >= 0;
            _shareButton.Click += (sender, e) => _viewModel.AddKeyShares.Execute(new EmailAddress[] { new EmailAddress(_contactsComboBox.Text) });
            _selectAllButton.Click += (sender, e) => { for (int i = 0; i < _sharedKeysCheckListBox.Items.Count; ++i) _sharedKeysCheckListBox.SetItemChecked(i, !_sharedKeysCheckListBox.GetItemChecked(i)); };
            _unshareButton.Click += (sender, e) => { _viewModel.RemoveKeyShares.Execute(_sharedKeysCheckListBox.CheckedItems.Cast<EmailAddress>()); };

            _contactsComboBox.Focus();
        }
    }
}