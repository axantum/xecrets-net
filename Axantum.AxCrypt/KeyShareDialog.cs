using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    public partial class KeyShareDialog : StyledMessageBase
    {
        private SharingListViewModel _viewModel;

        public IEnumerable<UserPublicKey> SharedWith { get; private set; }

        public KeyShareDialog()
        {
            InitializeComponent();
        }

        public KeyShareDialog(Form parent, Func<KnownPublicKeys> knownPublicKeysFactory, IEnumerable<UserPublicKey> sharedWith, LogOnIdentity logOnIdentity)
            : this()
        {
            InitializeStyle(parent);

            _viewModel = new SharingListViewModel(knownPublicKeysFactory, sharedWith, logOnIdentity);
            _viewModel.BindPropertyChanged<IEnumerable<UserPublicKey>>(nameof(SharingListViewModel.SharedWith), (aks) => { _sharedWith.Items.Clear(); _sharedWith.Items.AddRange(aks.ToArray()); });
            _viewModel.BindPropertyChanged<IEnumerable<UserPublicKey>>(nameof(SharingListViewModel.NotSharedWith), (aks) => { _notSharedWith.Items.Clear(); _notSharedWith.Items.AddRange(aks.ToArray()); });

            _sharedWith.SelectedIndexChanged += (sender, e) => SetUnshareButtonState();
            _notSharedWith.SelectedIndexChanged += (sender, e) => SetShareButtonState();

            _shareButton.Click += (sender, e) => _viewModel.AddKeyShares.Execute(_notSharedWith.SelectedIndices.Cast<int>().Select(i => EmailAddress.Parse(_notSharedWith.Items[i].ToString())));
            _shareButton.Click += (sender, e) => SetShareButtonState();
            _unshareButton.Click += (sender, e) => _viewModel.RemoveKeyShares.Execute(_sharedWith.SelectedIndices.Cast<int>().Select(i => (UserPublicKey)_sharedWith.Items[i]));
            _unshareButton.Click += (sender, e) => SetUnshareButtonState();

            SetShareButtonState();
            SetUnshareButtonState();

            _notSharedWith.Focus();
        }

        private void SetShareButtonState()
        {
            _shareButton.Enabled = _notSharedWith.SelectedIndices.Count > 0;
        }

        private void SetUnshareButtonState()
        {
            _unshareButton.Enabled = _sharedWith.SelectedIndices.Count > 0;
        }

        private void _okButton_Click(object sender, EventArgs e)
        {
            SharedWith = _viewModel.SharedWith;
        }
    }
}