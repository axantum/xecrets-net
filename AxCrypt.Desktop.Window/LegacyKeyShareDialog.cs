﻿using AxCrypt.Abstractions;
using AxCrypt.Api;
using AxCrypt.Api.Model;
using AxCrypt.Common;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.UI;
using AxCrypt.Core.UI.ViewModel;
using AxCrypt.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AxCrypt.Abstractions.TypeResolve;
using Texts = AxCrypt.Content.Texts;

namespace AxCrypt.Desktop.Window
{
    public partial class LegacyKeyShareDialog : StyledMessageBase
    {
        private SharingListViewModel _viewModel;

        public LegacyKeyShareDialog()
        {
            InitializeComponent();
        }

        public LegacyKeyShareDialog(Form parent, SharingListViewModel viewModel)
            : this()
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            InitializeStyle(parent);

            _viewModel = viewModel;
            _viewModel.BindPropertyChanged<IEnumerable<UserPublicKey>>(nameof(SharingListViewModel.SharedWith), (aks) => { _sharedWith.Items.Clear(); _sharedWith.Items.AddRange(aks.Distinct(UserPublicKey.EmailComparer).ToArray()); SetNotSharedWithActionButtonsState(); });
            _viewModel.BindPropertyChanged<IEnumerable<UserPublicKey>>(nameof(SharingListViewModel.NotSharedWith), (aks) => { _notSharedWith.Items.Clear(); aks = FilterNotSharedContactsByCapability(aks); _notSharedWith.Items.AddRange(aks.ToArray()); SetNotSharedWithActionButtonsState(); });
            _viewModel.BindPropertyChanged<string>(nameof(SharingListViewModel.NewKeyShare), (email) => SetNotSharedWithActionButtonsState());
            _viewModel.BindPropertyChanged<bool>(nameof(SharingListViewModel.IsOnline), (isOnline) => { SetNewContactState(isOnline); });

            _sharedWith.SelectedIndexChanged += (sender, e) => SetUnshareButtonState();
            _notSharedWith.SelectedIndexChanged += (sender, e) => SetNotSharedWithActionButtonsState();

            _sharedWith.MouseDoubleClick += async (sender, e) => await Unshare(_sharedWith.IndexFromPoint(e.Location));
            _notSharedWith.MouseDoubleClick += async (sender, e) =>
            {
                await ShareSelectedIndices(new int[] { _notSharedWith.IndexFromPoint(e.Location) });
            };

            _newContact.TextChanged += (sender, e) => { _viewModel.NewKeyShare = _newContact.Text.Trim(); ClearErrorProviders(); };
            _newContact.Enter += (sender, e) => { _sharedWith.ClearSelected(); _notSharedWith.ClearSelected(); };

            _shareButton.Click += async (sender, e) =>
            {
                await ShareSelectedIndices(_notSharedWith.SelectedIndices.Cast<int>());

                AccountStatus accountStatus = await ShareNewContactAsync();
                if (accountStatus == AccountStatus.Unverified)
                {
                    await DisplayInviteMessageAsync(_viewModel.NewKeyShare);
                }
                if (accountStatus != AccountStatus.Unknown)
                {
                    _newContact.Text = string.Empty;
                }
            };
            _unshareButton.Click += async (sender, e) =>
            {
                await Unshare();
                SetUnshareButtonState();
            };

            _removeKnownContactButton.Click += async (sender, e) =>
            {
                await RemoveKnownContact();
            };

            SetOkButtonState();
            _notSharedWith.Focus();
        }

        private IEnumerable<UserPublicKey> FilterNotSharedContactsByCapability(IEnumerable<UserPublicKey> notSharedWithContacts)
        {
            if (!New<LicensePolicy>().Capabilities.Has(LicenseCapability.KeySharing))
            {
                return notSharedWithContacts.Where(nswe => nswe.IsUserImported);
            }

            return notSharedWithContacts;
        }

        private void SetNewContactState(bool isOnline)
        {
            if (!New<LicensePolicy>().Capabilities.Has(LicenseCapability.KeySharing))
            {
                _newContact.Enabled = false;
                _newContact.Text = $"[{Texts.PremiumFeatureToolTipText}]";
                return;
            }

            _newContact.Enabled = isOnline;
            _newContact.Text = isOnline ? _viewModel.NewKeyShare : $"[{Texts.OfflineIndicatorText}]";
        }

        protected override void InitializeContentResources()
        {
            Text = Texts.DialogKeyShareTitle;

            _knownContactsGroupBox.Text = Texts.PromptKnownContacts;
            _addContactGroupBox.Text = Texts.PromptAddContact;
            _unshareButton.Text = Texts.ButtonUnshareLeftText;
            _shareButton.Text = Texts.ButtonShareRightText;
            _removeKnownContactButton.Text = Texts.ButtonRemoveKnownContactText;
            _sharedWithGroupBox.Text = Texts.PromptSharedWith;
            _okButton.Text = "&" + Texts.ButtonOkText;
            _cancelButton.Text = "&" + Texts.ButtonCancelText;
        }

        private async Task DisplayInviteMessageAsync(string email)
        {
            await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.SharedWithUnverfiedMessageTitle, Texts.SharedWithUnverifiedMessagePattern.InvariantFormat(email));
        }

        private void SetNotSharedWithActionButtonsState()
        {
            bool isNewKeyShare = !String.IsNullOrEmpty(_viewModel.NewKeyShare);
            if (isNewKeyShare)
            {
                _notSharedWith.ClearSelected();
                _sharedWith.ClearSelected();
            }

            bool notSharedWithHasSelectedIndices = _notSharedWith.SelectedIndices.Count > 0;
            _shareButton.Visible = notSharedWithHasSelectedIndices || isNewKeyShare;
            _removeKnownContactButton.Visible = notSharedWithHasSelectedIndices;
            if (notSharedWithHasSelectedIndices)
            {
                _sharedWith.ClearSelected();
            }

            AcceptButton = _shareButton;
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
            if (_unshareButton.Visible || _shareButton.Visible || _removeKnownContactButton.Visible)
            {
                _okButton.Enabled = false;
                return;
            }

            _okButton.Enabled = true;
            AcceptButton = _okButton;
        }

        private async Task Unshare()
        {
            await _viewModel.RemoveKeyShares.ExecuteAsync(_sharedWith.SelectedIndices.Cast<int>().Select(i => (UserPublicKey)_sharedWith.Items[i]));
        }

        private async Task Unshare(int index)
        {
            if (index == ListBox.NoMatches)
            {
                return;
            }

            await _viewModel.RemoveKeyShares.ExecuteAsync(new UserPublicKey[] { (UserPublicKey)_sharedWith.Items[index] });
            SetUnshareButtonState();
        }

        private async Task RemoveKnownContact()
        {
            await _viewModel.RemoveKnownContact.ExecuteAsync(_notSharedWith.SelectedIndices.Cast<int>().Select(i => (UserPublicKey)_notSharedWith.Items[i]).ToList());
        }

        private async Task<AccountStatus> ShareNewContactAsync()
        {
            if (string.IsNullOrEmpty(_viewModel.NewKeyShare))
            {
                return AccountStatus.Unknown;
            }
            if (!AdHocValidationDueToMonoLimitations())
            {
                return AccountStatus.Unknown;
            }

            AccountStatus accountStatus = await VerifyNewKeyShareStatus();
            switch (accountStatus)
            {
                case AccountStatus.Unverified:
                case AccountStatus.Verified:
                case AccountStatus.NotFound:
                    break;

                default:
                    return accountStatus;
            }

            try
            {
                await _viewModel.AddNewKeyShare.ExecuteAsync(_viewModel.NewKeyShare);
                if (_viewModel.SharedWith.Where(sw => sw.Email.ToString() == _viewModel.NewKeyShare).Any())
                {
                    return accountStatus;
                }

                if (New<AxCryptOnlineState>().IsOffline)
                {
                    SetOfflineError();
                }
            }
            catch (BadRequestApiException braex)
            {
                New<IReport>().Exception(braex);
                _errorProvider1.SetError(_newContact, Texts.InvalidEmail);
                _errorProvider1.SetIconPadding(_newContact, 3);
            }

            return AccountStatus.Unknown;
        }

        private void SetOfflineError()
        {
            _newContact.Enabled = !New<AxCryptOnlineState>().IsOffline;
            _newContact.Text = $"[{Texts.OfflineIndicatorText}]";
            _errorProvider1.SetError(_newContact, Texts.KeySharingOffline);
        }

        private async Task<AccountStatus> VerifyNewKeyShareStatus()
        {
            await _viewModel.UpdateNewKeyShareStatus.ExecuteAsync(null);
            AccountStatus sharedUserAccountStatus = _viewModel.NewKeyShareStatus;

            if (sharedUserAccountStatus == AccountStatus.Offline)
            {
                SetOfflineError();
                return sharedUserAccountStatus;
            }

            if (sharedUserAccountStatus != AccountStatus.NotFound)
            {
                return sharedUserAccountStatus;
            }

            using (KeySharingInviteUserDialog inviteDialog = new KeySharingInviteUserDialog(this))
            {
                if (inviteDialog.ShowDialog(this) != DialogResult.OK)
                {
                    return AccountStatus.Unknown;
                }
            }

            return sharedUserAccountStatus;
        }

        private Task ShareSelectedIndices(IEnumerable<int> indices)
        {
            return _viewModel.AddKeyShares.ExecuteAsync(indices.Where(i => i != ListBox.NoMatches).Select(i => EmailAddress.Parse(_notSharedWith.Items[i].ToString())));
        }

        private void _okButton_Click(object sender, EventArgs e)
        {
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
                _errorProvider1.SetError(_newContact, Texts.InvalidEmail);
                _errorProvider1.SetIconPadding(_newContact, 3);
                return false;
            }
            return true;
        }

        private void ClearErrorProviders()
        {
            _errorProvider1.Clear();
        }
    }
}