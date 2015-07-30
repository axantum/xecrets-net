using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
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

        public IEnumerable<UserPublicKey> SharedWith { get; private set; }

        public KeyShareDialog(Func<KnownPublicKeys> knownPublicKeysFactory, IEnumerable<UserPublicKey> sharedWith, LogOnIdentity logOnIdentity)
        {
            InitializeComponent();
            new Styling().Style(this);

            _viewModel = new SharingListViewModel(knownPublicKeysFactory, sharedWith, logOnIdentity);
            _viewModel.BindPropertyChanged<IEnumerable<UserPublicKey>>("SharedWith", (aks) => { _sharedWithListBox.Items.Clear(); _sharedWithListBox.Items.AddRange(aks.ToArray()); });
            _viewModel.BindPropertyChanged<IEnumerable<UserPublicKey>>("NotSharedWith", (aks) => { _notSharedWithListBox.Items.Clear(); _notSharedWithListBox.Items.AddRange(aks.ToArray()); });

            _sharedWithListBox.SelectedIndexChanged += (sender, e) => SetUnshareButtonState();
            _notSharedWithListBox.SelectedIndexChanged += (sender, e) => SetShareButtonState();

            _shareButton.Click += (sender, e) => _viewModel.AddKeyShares.Execute(_notSharedWithListBox.SelectedIndices.Cast<int>().Select(i => EmailAddress.Parse(_notSharedWithListBox.Items[i].ToString())));
            _shareButton.Click += (sender, e) => SetShareButtonState();
            _unshareButton.Click += (sender, e) => _viewModel.RemoveKeyShares.Execute(_sharedWithListBox.SelectedIndices.Cast<int>().Select(i => (UserPublicKey)_sharedWithListBox.Items[i]));
            _unshareButton.Click += (sender, e) => SetUnshareButtonState();

            SetShareButtonState();
            SetUnshareButtonState();

            _notSharedWithListBox.Focus();
        }

        private void SetShareButtonState()
        {
            _shareButton.Enabled = _notSharedWithListBox.SelectedIndices.Count > 0;
        }

        private void SetUnshareButtonState()
        {
            _unshareButton.Enabled = _sharedWithListBox.SelectedIndices.Count > 0;
        }

        private void _okButton_Click(object sender, EventArgs e)
        {
            SharedWith = _viewModel.SharedWith;
        }
    }
}
