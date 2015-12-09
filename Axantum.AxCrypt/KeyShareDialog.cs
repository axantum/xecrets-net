using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Axantum.AxCrypt.Abstractions.TypeResolve;
using Content = AxCrypt.Content.Content;

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
            _viewModel.BindPropertyChanged<string>(nameof(SharingListViewModel.NewKeyShare), (email) => SetShareButtonState());

            _sharedWith.SelectedIndexChanged += (sender, e) => SetUnshareButtonState();
            _notSharedWith.SelectedIndexChanged += (sender, e) => SetShareButtonState();

            _newContact.TextChanged += (sender, e) =>
            {
                _viewModel.NewKeyShare = _newContact.Text;
            };
            _newContact.Enter += (sender, e) => { _sharedWith.ClearSelected(); _notSharedWith.ClearSelected(); };

            _shareButton.Click += async (sender, e) =>
            {
                await ShareSelectedKnownContactsAsync();
                string newContact = _viewModel.NewKeyShare;
                if (await ShareNewContactAsync())
                {
                    await DisplayInviteMessageAsync(_viewModel.NewKeyShare);
                };
                _newContact.Text = String.Empty;
                SetShareButtonState();
            };
            _unshareButton.Click += (sender, e) =>
            {
                Unshare();
                SetUnshareButtonState();
            };

            SetOkButtonState();
            _notSharedWith.Focus();
        }

        protected override void InitializeContentResources()
        {
            Text = Content.DialogKeyShareTitle;

            _knownContactsGroupBox.Text = Content.PromptKnownContacts;
            _addContactGroupBox.Text = Content.PromptAddContact;
            _unshareButton.Text = Content.ButtonUnshareLeftText;
            _shareButton.Text = Content.ButtonShareRightText;
            _sharedWithGroupBox.Text = Content.PromptSharedWith;
            _okButton.Text = Content.ButtonOkText;
            _cancelButton.Text = Content.ButtonCancelText;
        }

        private async Task DisplayInviteMessageAsync(string email)
        {
            AccountStatus status = await New<LogOnIdentity, IAccountService>(LogOnIdentity.Empty).StatusAsync(EmailAddress.Parse(email));
            if (status != AccountStatus.Unverified)
            {
                return;
            }

            MessageDialog.ShowOk(this, Content.SharedWithUnverfiedMessageTitle, Content.SharedWithUnverifiedMessagePattern.InvariantFormat(email));
        }

        private void SetShareButtonState()
        {
            bool isNewKeyShare = !String.IsNullOrEmpty(_viewModel.NewKeyShare);
            if (isNewKeyShare)
            {
                _notSharedWith.ClearSelected();
                _sharedWith.ClearSelected();
            }
            _shareButton.Visible = _notSharedWith.SelectedIndices.Count > 0 || isNewKeyShare;
            if (_shareButton.Visible)
            {
                _sharedWith.ClearSelected();
                AcceptButton = _shareButton;
            }
            SetOkButtonState();
        }

        private void SetUnshareButtonState()
        {
            _unshareButton.Visible = _sharedWith.SelectedIndices.Count > 0;
            if (_unshareButton.Visible)
            {
                _notSharedWith.ClearSelected();
                AcceptButton = _unshareButton;
            }
            SetOkButtonState();
        }

        private void SetOkButtonState()
        {
            if (_unshareButton.Visible || _shareButton.Visible)
            {
                _okButton.Enabled = false;
                return;
            }

            _okButton.Enabled = true;
            AcceptButton = _okButton;
        }

        private void Unshare()
        {
            _viewModel.RemoveKeyShares.Execute(_sharedWith.SelectedIndices.Cast<int>().Select(i => (UserPublicKey)_sharedWith.Items[i]));
        }

        private async Task<bool> ShareNewContactAsync()
        {
            if (String.IsNullOrEmpty(_viewModel.NewKeyShare))
            {
                return false;
            }
            if (!AdHocValidationDueToMonoLimitations())
            {
                return false;
            }
            try
            {
                await _viewModel.AsyncAddNewKeyShare.ExecuteAsync(_viewModel.NewKeyShare);
                return true;
            }
            catch (UserInputException)
            {
                _errorProvider1.SetError(_newContact, Content.InvalidEmail);
                _errorProvider1.SetIconPadding(_newContact, 3);
                return false;
            }
            catch (OfflineApiException)
            {
                _errorProvider1.SetError(_newContact, Content.KeySharingOffline);
                _errorProvider1.SetIconPadding(_newContact, 3);
                return false;
            }
        }

        private async Task ShareSelectedKnownContactsAsync()
        {
            await _viewModel.AsyncAddKeyShares.ExecuteAsync(_notSharedWith.SelectedIndices.Cast<int>().Select(i => EmailAddress.Parse(_notSharedWith.Items[i].ToString())));
        }

        private void _okButton_Click(object sender, EventArgs e)
        {
            SharedWith = _viewModel.SharedWith;
        }

        private bool AdHocValidationDueToMonoLimitations()
        {
            bool validated = AdHocValidateAllFieldsIndependently();
            return validated;
        }

        private bool AdHocValidateAllFieldsIndependently()
        {
            return AdHocValidateNewKeyShare();
        }

        private bool AdHocValidateNewKeyShare()
        {
            _errorProvider1.Clear();
            if (_viewModel[nameof(SharingListViewModel.NewKeyShare)].Length > 0)
            {
                _errorProvider1.SetError(_newContact, Content.InvalidEmail);
                _errorProvider1.SetIconPadding(_newContact, 3);
                return false;
            }
            return true;
        }
    }
}