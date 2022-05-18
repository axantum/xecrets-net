using AxCrypt.Abstractions;
using AxCrypt.Api;
using AxCrypt.Api.Model;
using AxCrypt.Common;
using AxCrypt.Content;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.IO;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.UI;
using AxCrypt.Core.UI.ViewModel;
using AxCrypt.Desktop.Window.Properties;
using AxCrypt.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Desktop.Window
{
    public partial class KeyShareDialog : StyledMessageBase
    {
        private static class FormStyles
        {
            public static System.Drawing.Color GreenColor = System.Drawing.Color.FromArgb(237, 241, 234);

            public static System.Drawing.Color GreyColor = System.Drawing.Color.FromArgb(1, 73, 73, 73);

            public static System.Drawing.Color LightGreyColor = System.Drawing.Color.FromArgb(202, 202, 202);

            public static System.Drawing.Font OpenSans12Bold = new System.Drawing.Font("Open Sans", 12F, System.Drawing.FontStyle.Bold);

            public static System.Drawing.Font OpenSans10Regular = new System.Drawing.Font("Open Sans", 10F, System.Drawing.FontStyle.Regular);

            public static System.Drawing.Font OpenSans9Dot5Regular = new System.Drawing.Font("Open Sans", 9.5F, System.Drawing.FontStyle.Regular);
        }

        private SharingListViewModel _viewModel;

        private IList<ShareKeyUser> _shareKeyUserList { get; set; }

        private IEnumerable<string> _selectedFileList { get; set; }

        private EmailAddress _selectedUserEmailForContextMenuOperation;

        private Control _selectedControlForContextMenuOperation;

        public KeyShareDialog()
        {
            InitializeComponent();
        }

        public KeyShareDialog(Form parent, SharingListViewModel viewModel, IEnumerable<string> fileorFolderNames)
            : this()
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            InitializeStyle(parent);

            _viewModel = viewModel;
            _viewModel.BindPropertyChanged<IEnumerable<UserPublicKey>>(nameof(SharingListViewModel.SharedWith), (aks) => { _shareKeyUserList = aks.Distinct(UserPublicKey.EmailComparer).ToArray().Select(user => new ShareKeyUser(user.Email, AccountStatus.Verified)).ToList(); });
            _viewModel.BindPropertyChanged<bool>(nameof(SharingListViewModel.IsOnline), (bool isOnline) => { SetNewContactState(); });
            _addNewUserTextBox.TextChanged += (sender, e) => { _viewModel.NewKeyShare = _addNewUserTextBox.Text.Trim(); ClearErrorProviders(); };
            _addNewUserTextBox.Enter += (sender, e) => { Textbox_GotFocus(sender, e); };
            _addNewUserTextBox.Leave += Textbox_LostFocus;

            _addButton.Click += async (sender, e) =>
            {
                await AddShareKeyUser();
            };

            this.Click += (sender, args) =>
            {
                this._selectedSharedKeyUserPopupLayout.Visible = false;
            };

            this._sharedWithUsersListLayoutPanel.Click += (sender, args) =>
            {
                this._selectedSharedKeyUserPopupLayout.Visible = false;
            };

            InitializeControls(viewModel, fileorFolderNames);
        }

        protected override void InitializeContentResources()
        {
            Text = Texts.ShareAccessTitle;
            _shareAccessTitle.Text = "&" + Texts.ShareAccessTitle;
            _addUserLabel.Text = "&" + Texts.AddUserPromptText;
            _addButton.Text = "&" + Texts.AddPromptText;
            _applyButton.Text = "&" + Texts.ApplyLabel;
            _cancelButton.Text = "&" + Texts.ButtonCancelText;
        }

        private void InitializeControls(SharingListViewModel viewModel, IEnumerable<string> fileorFolderNames)
        {
            _selectedFileList = fileorFolderNames;

            foreach (string fileName in _selectedFileList)
            {
                AddSelectedFileToList(New<IDataContainer>(fileName).Name);
            }
            int selectedFileCount = _selectedFileList.Count();
            this._moreFilesInfo.Visible = selectedFileCount > 3;
            this._moreFilesInfo.Text = selectedFileCount > 3 ? string.Format(Texts.MoreFilesLabel, (selectedFileCount - 3)) : "";

            this._sharedWithUsersListLayout.Width = 462;

            InitializeSharedKeyUsersListLayout();

            Button unShareSubMenuItemButton = ContextMenuItem(Texts.UnSharePromptText, Resources.unShareIcon, () => UnSharePopupMenuItemAction());
            this._selectedSharedKeyUserPopupLayout.Controls.Add(unShareSubMenuItemButton);

            //FlowLayoutPanel refreshPopupMenuItem = SelectedSharedKeyUserPopup(Texts.RefreshPromptText, Resources.refreshIcon);
            //this._selectedSharedKeyUserPopupLayout.Controls.Add(refreshPopupMenuItem);
        }

        private void InitializeSharedKeyUsersListLayout()
        {
            this._sharedWithUsersListLayout.Controls.Clear();

            foreach (ShareKeyUser user in _shareKeyUserList)
            {
                AddSharedKeyUserToList(user);
            }

            UpdateShareKeyUserListLayout();
        }

        private async Task UnSharePopupMenuItemAction()
        {
            if (_selectedUserEmailForContextMenuOperation == EmailAddress.Empty)
            {
                return;
            }

            await _viewModel.RemoveKeyShares.ExecuteAsync(new UserPublicKey[] { (UserPublicKey)_viewModel.SharedWith.First(su => su.Email == _selectedUserEmailForContextMenuOperation) });

            if (_shareKeyUserList.Count == 5)
            {
                InitializeSharedKeyUsersListLayout();
                this._selectedSharedKeyUserPopupLayout.Visible = false;
            }
            else
            {
                if (_selectedControlForContextMenuOperation != null)
                {
                    int index = this._sharedWithUsersListLayout.Controls.IndexOf(_selectedControlForContextMenuOperation);
                    Control underline = this._sharedWithUsersListLayout.Controls[index + 1];

                    this._sharedWithUsersListLayout.Controls.Remove(_selectedControlForContextMenuOperation);
                    this._sharedWithUsersListLayout.Controls.Remove(underline);

                    this._selectedSharedKeyUserPopupLayout.Visible = false;
                }

                UpdateShareKeyUserListLayout();
            }

            SetOkButtonState();
        }

        private async Task AddShareKeyUser()
        {
            if (string.IsNullOrWhiteSpace(_addNewUserTextBox.Text) || _addNewUserTextBox.Text == Texts.AddEmailPromptText)
            {
                return;
            }

            EmailAddress addedUserEmailAddress;
            if (!EmailAddress.TryParse(_addNewUserTextBox.Text.Trim(), out addedUserEmailAddress))
            {
                _errorProvider1.SetError(_addNewUserTextBox, Texts.InvalidEmail);
                return;
            }

            if (_shareKeyUserList.Any(user => user.UserEmail == addedUserEmailAddress))
            {
                return;
            }

            if (!New<AxCryptOnlineState>().IsOnline && !CanShareKeyInOfflineFor(addedUserEmailAddress))
            {
                await DisplayOfflineWarningMessageAsync();
                _addNewUserTextBox.Text = string.Empty;
                _addNewUserTextBox.Focus();
                return;
            }

            _addButton.Enabled = false;

            AccountStatus accountStatus = AccountStatus.Verified;
            if (!_viewModel.NotSharedWith.Any(nsw => nsw.Email == addedUserEmailAddress))
            {
                accountStatus = await ShareNewContactAsync();
            }

            await ShareSelectedIndices(new List<string>() { addedUserEmailAddress.Address });

            ShareKeyUser shareKeyUser = _shareKeyUserList.Single(sku => sku.UserEmail == EmailAddress.Parse(_viewModel.NewKeyShare));
            shareKeyUser = new ShareKeyUser(EmailAddress.Parse(_viewModel.NewKeyShare), accountStatus);

            if (_shareKeyUserList.Count == 6)
            {
                InitializeSharedKeyUsersListLayout();
            }
            else
            {
                AddSharedKeyUserToList(shareKeyUser);
                UpdateShareKeyUserListLayout();
            }

            _addButton.Enabled = true;
            SetOkButtonState();
            _addNewUserTextBox.Text = string.Empty;
            _addNewUserTextBox.Focus();
        }

        private bool CanShareKeyInOfflineFor(EmailAddress addedUserEmailAddress)
        {
            if (!_viewModel.NotSharedWith.Any())
            {
                return false;
            }

            if (!_viewModel.NotSharedWith.Any(nsw => nsw.Email == addedUserEmailAddress))
            {
                return false;
            }

            return true;
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
                    await DisplayOfflineWarningMessageAsync();
                }
            }
            catch (BadRequestApiException braex)
            {
                New<IReport>().Exception(braex);
                _errorProvider1.SetError(_addNewUserTextBox, Texts.InvalidEmail);
                _errorProvider1.SetIconPadding(_addNewUserTextBox, 3);
            }

            return AccountStatus.Unknown;
        }

        private async Task<AccountStatus> VerifyNewKeyShareStatus()
        {
            await _viewModel.UpdateNewKeyShareStatus.ExecuteAsync(null);
            AccountStatus sharedUserAccountStatus = _viewModel.NewKeyShareStatus;

            if (sharedUserAccountStatus == AccountStatus.Offline)
            {
                return sharedUserAccountStatus;
            }

            if (sharedUserAccountStatus != AccountStatus.NotFound)
            {
                return sharedUserAccountStatus;
            }

            return sharedUserAccountStatus;
        }

        private Task ShareSelectedIndices(IEnumerable<string> userList)
        {
            return _viewModel.AddKeyShares.ExecuteAsync(userList.Select(user => EmailAddress.Parse(user)));
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
                _errorProvider1.SetError(_addNewUserTextBox, Texts.InvalidEmail);
                _errorProvider1.SetIconPadding(_addNewUserTextBox, 3);
                return false;
            }

            return true;
        }

        private async Task DisplayOfflineWarningMessageAsync()
        {
            await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.WarningTitle, Texts.KeySharingOffline);
        }

        private void SetNewContactState()
        {
            if (!New<LicensePolicy>().Capabilities.Has(LicenseCapability.KeySharing))
            {
                _addNewUserTextBox.Enabled = false;
                _addNewUserTextBox.Text = $"[{Texts.PremiumFeatureToolTipText}]";
                return;
            }

            _addNewUserTextBox.Text = Texts.AddEmailPromptText;
        }

        private void Textbox_GotFocus(object sender, EventArgs e)
        {
            System.Windows.Forms.TextBox textBox = ((System.Windows.Forms.TextBox)sender);
            if (!string.IsNullOrEmpty(_viewModel.NewKeyShare) && _addNewUserTextBox.Text != Texts.AddEmailPromptText)
            {
                return;
            }

            textBox.Text = "";
            textBox.ForeColor = Color.Black;
        }

        private void Textbox_LostFocus(object sender, EventArgs e)
        {
            System.Windows.Forms.TextBox textBox = ((System.Windows.Forms.TextBox)sender);
            if (textBox == null || textBox.Text.Any())
            {
                return;
            }

            textBox.Text = Texts.AddEmailPromptText;
            textBox.ForeColor = FormStyles.LightGreyColor;
        }

        private void SetOkButtonState()
        {
            _applyButton.Enabled = true;
            AcceptButton = _addButton;
        }

        private void ClearErrorProviders()
        {
            _errorProvider1.Clear();
        }

        #region SelectedFileListItemAndSharedKeyUsersListItemTemplate

        private void AddSelectedFileToList(string selectedFileName)
        {
            if (_selectedFileListLayout.Controls.Count >= 3)
            {
                return;
            }

            PictureBox fileIcon = new PictureBox
            {
                Image = Resources.TextDocumentIcon,
                Name = "fileIcon",
                SizeMode = PictureBoxSizeMode.AutoSize,
            };

            Label fileName = new Label
            {
                AutoSize = true,
                Name = "fileName",
                Text = Truncate(selectedFileName, 14),
                Padding = new Padding(0, 2, 0, 0),
                Font = FormStyles.OpenSans9Dot5Regular,
                TextAlign = ContentAlignment.MiddleCenter
            };

            FlowLayoutPanel selectedFileItem = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = FormStyles.GreenColor,
                Padding = new System.Windows.Forms.Padding(3, 3, 0, 3),
                Name = "selectedFileItem",
                Size = new System.Drawing.Size(150, 28),
                Controls =
                {
                    fileIcon,
                    fileName
                }
            };

            _selectedFileListLayout.Controls.Add(selectedFileItem);
        }

        private void AddSharedKeyUserToList(ShareKeyUser shareKeyUser)
        {
            int verticalScrollSize = 18;
            int width = _sharedWithUsersListLayout.Width;
            if (_shareKeyUserList.Count > 5)
            {
                width = width - verticalScrollSize; // Substract a vertical scroll width, To show a scroll without horizontal scroll
            }

            int imageAndMenuWidth = (int)Math.Round(0.1 * width);
            int textWidth = (int)Math.Round(0.79 * width);

            PictureBox userTypeIcon = new PictureBox
            {
                Image = shareKeyUser.Image,
                ImeMode = System.Windows.Forms.ImeMode.NoControl,
                Name = "UserTypeIcon",
                SizeMode = PictureBoxSizeMode.AutoSize,
                Size = new System.Drawing.Size(imageAndMenuWidth, 32),
                Padding = new System.Windows.Forms.Padding(20, 10, 0, 10),
            };

            Label userEmail = new Label
            {
                Name = "UserEmail",
                Text = Truncate(shareKeyUser.UserEmail.Address, 45),
                Size = new System.Drawing.Size(textWidth, 32),
                Padding = new System.Windows.Forms.Padding(12, 3, 0, 0),
                Font = FormStyles.OpenSans10Regular,
                ForeColor = FormStyles.GreyColor,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            Button contextMenuButton = new Button
            {
                BackgroundImage = shareKeyUser.DotImage,
                BackgroundImageLayout = ImageLayout.Center,
                FlatStyle = FlatStyle.Flat,
                Size = new System.Drawing.Size(imageAndMenuWidth, 32),
                Name = "contextMenuButton",
            };
            contextMenuButton.FlatAppearance.BorderSize = 0;
            contextMenuButton.FlatAppearance.BorderColor = FormStyles.GreenColor;
            contextMenuButton.FlatAppearance.MouseOverBackColor = Color.Transparent;

            contextMenuButton.Click += (sender, args) => OnContextMenuClick(sender, args, shareKeyUser.UserEmail.Address);

            FlowLayoutPanel ShareKeyUserEntry = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoScroll = false,
                AutoSize = true,
                Height = 33,
                BackColor = FormStyles.GreenColor,
                Margin = new System.Windows.Forms.Padding(0, 0, 0, 0),
                Controls =
                {
                    userTypeIcon,
                    userEmail,
                    contextMenuButton
                }
            };

            Panel userListPanelLine = new Panel
            {
                Size = new System.Drawing.Size(imageAndMenuWidth + textWidth + imageAndMenuWidth + 5, 1),
                BackColor = FormStyles.LightGreyColor,
                Name = "userListPanelLine",
                Margin = new System.Windows.Forms.Padding(0, 0, 0, 0),
            };

            this._sharedWithUsersListLayout.Controls.Add(ShareKeyUserEntry);
            this._sharedWithUsersListLayout.Controls.Add(userListPanelLine);
        }

        private string Truncate(string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
        }

        private void OnContextMenuClick(object sender, EventArgs e, string selectedUserEmail)
        {
            Button selectedItem = (Button)sender;
            _selectedControlForContextMenuOperation = selectedItem.Parent;
            int contextMenuLocationX = this._sharedWithUsersListLayoutPanel.Location.X + _selectedControlForContextMenuOperation.Location.X + (_selectedControlForContextMenuOperation.Width - 70);
            int contextMenuLocationY = this._sharedWithUsersListLayoutPanel.Location.Y + _selectedControlForContextMenuOperation.Location.Y + 20;

            this._selectedSharedKeyUserPopupLayout.Location = new Point(contextMenuLocationX, contextMenuLocationY);
            this._selectedSharedKeyUserPopupLayout.Visible = true;

            if (!EmailAddress.TryParse(selectedUserEmail, out _selectedUserEmailForContextMenuOperation))
            {
                //show an error message
            }
        }

        private void UpdateShareKeyUserListLayout()
        {
            this._sharedKeyUsersListTitle.Text = string.Format(Texts.SharedKeyUserListTitle, _shareKeyUserList.Count());

            if (!_shareKeyUserList.Any())
            {
                this._sharedWithUsersListLayout.BackgroundImage = Resources.ShareKeyBackground;
                this._sharedWithUsersListLayout.BackgroundImageLayout = ImageLayout.Center;
                return;
            }

            this._sharedWithUsersListLayout.BackgroundImage = null;
        }

        private Button ContextMenuItem(string title, Image icon, Func<Task> buttonOnClickAction)
        {
            Button subMenuItemButton = new Button
            {
                Image = icon,
                Text = title,
                FlatStyle = FlatStyle.Flat,
                Size = new System.Drawing.Size(108, 30),
                Name = "subMenuItemButton",
                TextImageRelation = TextImageRelation.ImageBeforeText,
            };

            subMenuItemButton.FlatAppearance.BorderSize = 0;
            subMenuItemButton.FlatAppearance.BorderColor = FormStyles.GreenColor;
            subMenuItemButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(242, 244, 240);
            subMenuItemButton.Click += async (object o, EventArgs e) => { await buttonOnClickAction(); };

            return subMenuItemButton;
        }

        private void DrawBorder(object sender, PaintEventArgs e)
        {
            if (sender.GetType() != typeof(Panel))
            {
                return;
            }

            ControlPaint.DrawBorder(e.Graphics, e.ClipRectangle, FormStyles.LightGreyColor, ButtonBorderStyle.Solid);
        }

        #endregion SelectedFileListItemAndSharedKeyUsersListItemTemplate
    }
}