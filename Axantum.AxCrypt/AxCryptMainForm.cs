#region Coypright and License

/*
 * AxCrypt - Copyright 2015, Svante Seleborg, All Rights Reserved
 *
 * This file is part of AxCrypt.
 *
 * AxCrypt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AxCrypt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AxCrypt.  If not, see <http://www.gnu.org/licenses/>.
 *
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Ipc;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Forms.Style;
using Axantum.AxCrypt.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt
{
    /// <summary>
    /// All code here is expected to execute on the GUI thread. If code may be called on another thread, this call
    /// must be made through ThreadSafeUi() .
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ax")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public partial class AxCryptMainForm : Form, IStatusChecker
    {
        private NotifyIcon _notifyIcon = null;

        private MainViewModel _mainViewModel;

        private FileOperationViewModel _fileOperationViewModel;

        private KnownFoldersViewModel _knownFoldersViewModel;

        public static MessageBoxOptions MessageBoxOptions { get; private set; }

        private DebugLogOutputDialog _debugOutput;

        private TabPage _hiddenWatchedFoldersTabPage;

        private bool _shownFirstTime = false;

        public AxCryptMainForm()
        {
            InitializeComponent();
            new Styling(Resources.axcrypticon).Style(this, _recentFilesContextMenuStrip, _watchedFoldersContextMenuStrip);
        }

        private void AxCryptMainForm_Load(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            try
            {
                InitializeProgram();
            }
            catch
            {
                ClearAllSettingsAndReinitialize();
                throw;
            }
        }

        private void InitializeProgram()
        {
            RegisterTypeFactories();
            if (!ValidateSettings())
            {
                return;
            }

            StartKeyPairService();
            SetupViewModels();
            AttachLogListener();
            ConfigureUiOptions();
            SetupPathFilters();
            IntializeControls();
            RestoreUserPreferences();
            BindToViewModels();
            BindToFileOperationViewModel();
            SetupCommandService();
            SendStartSessionNotification();
        }

        private void StartKeyPairService()
        {
            if (!String.IsNullOrEmpty(Resolve.UserSettings.UserEmail))
            {
                return;
            }
            New<KeyPairService>().Start();
        }

        private bool ValidateSettings()
        {
            if (Resolve.UserSettings.SettingsVersion >= Resolve.UserSettings.CurrentSettingsVersion)
            {
                return true;
            }

            Resources.UserSettingsFormatChangeNeedsReset.ShowWarning();
            ClearAllSettingsAndReinitialize();
            CloseAndExit();
            return false;
        }

        private async void AxCryptMainForm_Shown(object sender, EventArgs e)
        {
            SetTopControlsEnabled(false);
            await DoInitialSignInAsync();
            SetTopControlsEnabled(true);
        }

        private void SetTopControlsEnabled(bool enabled)
        {
            foreach (Control control in Controls)
            {
                control.Enabled = enabled;
            }
        }

        private async Task DoInitialSignInAsync()
        {
            AccountStatus status = await New<LogOnIdentity, IAccountService>(LogOnIdentity.Empty).StatusAsync();
            DialogResult dialogResult = DialogResult.OK;
            do
            {
                if (!EnsureEmailAccount(status))
                {
                    Application.Exit();
                    return;
                }

                switch (status)
                {
                    case AccountStatus.NotFound:
                        await New<LogOnIdentity, IAccountService>(LogOnIdentity.Empty).SignupAsync(Resolve.UserSettings.UserEmail);
                        dialogResult = MessageDialog.ShowOkCancelExit(this, "Signing Up", "You have now registered. Please check your email for an activation link. When you have activated your account, click OK.");
                        break;

                    case AccountStatus.InvalidName:
                        Resolve.UserSettings.UserEmail = String.Empty;
                        dialogResult = MessageDialog.ShowOkCancelExit(this, "Invalid Email Address", "There is no such email address. Please enter your real email address, and try again.");
                        break;

                    case AccountStatus.Unverified:
                        dialogResult = MessageDialog.ShowOkCancelExit(this, "Check Email Address", "You must verify your email address. Please check your email, and try again.");
                        break;

                    case AccountStatus.Verified:
                        break;

                    case AccountStatus.Offline:
                        dialogResult = MessageDialog.ShowOkCancelExit(this, "Internet Acccess Required", "You must have Internet access this first time. Please check your connection, and try again.");
                        break;

                    case AccountStatus.Unknown:
                    case AccountStatus.Unauthenticated:
                    case AccountStatus.DefinedByServer:
                        Resolve.UserSettings.UserEmail = String.Empty;
                        dialogResult = MessageDialog.ShowOkCancelExit(this, "Unexpected Error", "Something unexpected went wrong. Please try again. If the problem persists, please report this.");
                        break;

                    default:
                        break;
                }
                if (dialogResult == DialogResult.Abort)
                {
                    Application.Exit();
                    return;
                }
                if (dialogResult == DialogResult.Cancel)
                {
                    return;
                }
                status = await New<LogOnIdentity, IAccountService>(LogOnIdentity.Empty).StatusAsync();
            } while (status != AccountStatus.Verified);

            if (!await EnsureCreateAccountAsync())
            {
                Application.Exit();
                return;
            }
            if (_shownFirstTime)
            {
                return;
            }
            _shownFirstTime = true;
            _pendingRequest = new CommandCompleteEventArgs(CommandVerb.Startup, new string[0]);
            LogOnAndDoPendingRequest();
        }

        private bool EnsureEmailAccount(AccountStatus status)
        {
            if (String.IsNullOrEmpty(Resolve.UserSettings.UserEmail))
            {
                return AskForEmailAddressToUse();
            }

            switch (status)
            {
                case AccountStatus.Unknown:
                case AccountStatus.NotFound:
                case AccountStatus.InvalidName:
                    return AskForEmailAddressToUse();

                default:
                    break;
            }
            return true;
        }

        private bool AskForEmailAddressToUse()
        {
            using (EmailDialog dialog = new EmailDialog(this))
            {
                dialog.EmailTextBox.Text = Resolve.UserSettings.UserEmail;
                DialogResult result = dialog.ShowDialog(this);
                if (result != DialogResult.OK)
                {
                    return false;
                }
                Resolve.UserSettings.UserEmail = dialog.EmailTextBox.Text;
            }
            return true;
        }

        private async Task<bool> EnsureCreateAccountAsync()
        {
            if (await OfflineAccountExistsAsync())
            {
                return true;
            }

            New<KeyPairService>().Start();
            using (CreateNewAccountDialog dialog = new CreateNewAccountDialog(this, String.Empty, EmailAddress.Parse(Resolve.UserSettings.UserEmail)))
            {
                DialogResult dialogResult = dialog.ShowDialog(this);
                if (dialogResult != DialogResult.OK)
                {
                    return false;
                }

                Passphrase passphrase = new Passphrase(dialog.PassphraseTextBox.Text);
                EmailAddress emailAddress = EmailAddress.Parse(dialog.EmailTextBox.Text);
                AccountStorage store = new AccountStorage(New<LogOnIdentity, IAccountService>(new LogOnIdentity(emailAddress, passphrase)));
                if (!store.HasKeyPairAsync().Result)
                {
                    return false;
                }
                Resolve.KnownIdentities.DefaultEncryptionIdentity = new LogOnIdentity(store.ActiveKeyPairAsync().Result, passphrase);
            }
            return true;
        }

        private async static Task<bool> OfflineAccountExistsAsync()
        {
            AccountStorage store = new AccountStorage(New<LogOnIdentity, IAccountService>(new LogOnIdentity(EmailAddress.Parse(Resolve.UserSettings.UserEmail), Passphrase.Empty)));

            return await store.StatusAsync() == Api.Model.AccountStatus.Verified;
        }

        private void LogOnAndDoPendingRequest()
        {
            if (!_mainViewModel.LoggedOn)
            {
                _fileOperationViewModel.IdentityViewModel.LogOnLogOff.Execute(Resolve.CryptoFactory.Default.Id);
            }
            if (_mainViewModel.LoggedOn)
            {
                DoRequest(_pendingRequest);
            }
            _pendingRequest = new CommandCompleteEventArgs(CommandVerb.Unknown, new string[0]);
        }

        private static void SendStartSessionNotification()
        {
            Resolve.SessionNotify.Notify(new SessionNotification(SessionNotificationType.SessionStart));
        }

        private void SetupCommandService()
        {
            Resolve.CommandService.Received += New<CommandHandler>().RequestReceived;
            Resolve.CommandService.StartListening();
            New<CommandHandler>().CommandComplete += AxCryptMainForm_CommandComplete;
        }

        private void ConfigureUiOptions()
        {
            MessageBoxOptions = RightToLeft == RightToLeft.Yes ? MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading : 0;
        }

        private void AttachLogListener()
        {
            Resolve.Log.Logged += (logger, loggingEventArgs) =>
            {
                Resolve.UIThread.PostOnUIThread(() =>
                {
                    if (_debugOutput == null || !_debugOutput.Visible)
                    {
                        return;
                    }
                    string formatted = "{0} {1}".InvariantFormat(OS.Current.UtcNow.ToString("o", CultureInfo.InvariantCulture), loggingEventArgs.Message.TrimLogMessage());
                    _debugOutput.AppendText(formatted);
                });
            };
        }

        private void SetupViewModels()
        {
            _mainViewModel = New<MainViewModel>();
            _fileOperationViewModel = New<FileOperationViewModel>();
            _knownFoldersViewModel = New<KnownFoldersViewModel>();
        }

        private void RegisterTypeFactories()
        {
            TypeMap.Register.Singleton<IUIThread>(() => new UIThread(this));
            TypeMap.Register.Singleton<IProgressBackground>(() => _progressBackgroundWorker);
            TypeMap.Register.Singleton<IStatusChecker>(() => this);
            TypeMap.Register.New<SessionNotificationHandler>(() => new SessionNotificationHandler(Resolve.FileSystemState, Resolve.KnownIdentities, New<ActiveFileAction>(), New<AxCryptFile>(), this));

            TypeMap.Register.New<IdentityViewModel>(() => new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify));
            TypeMap.Register.New<FileOperationViewModel>(() => new FileOperationViewModel(Resolve.FileSystemState, Resolve.SessionNotify, Resolve.KnownIdentities, Resolve.ParallelFileOperation, New<IStatusChecker>(), New<IdentityViewModel>()));
            TypeMap.Register.New<MainViewModel>(() => new MainViewModel(Resolve.FileSystemState, Resolve.UserSettings));
            TypeMap.Register.New<KnownFoldersViewModel>(() => new KnownFoldersViewModel(Resolve.FileSystemState, Resolve.SessionNotify, Resolve.KnownIdentities));
            TypeMap.Register.New<WatchedFoldersViewModel>(() => new WatchedFoldersViewModel(Resolve.FileSystemState));
        }

        private static void SetupPathFilters()
        {
            if (OS.Current.Platform != Platform.WindowsDesktop)
            {
                return;
            }

            OS.PathFilters.Add(new Regex(@"\\\.dropbox$"));
            OS.PathFilters.Add(new Regex(@"\\desktop\.ini$"));
            OS.PathFilters.Add(new Regex(@".*\.tmp$"));
            AddEnvironmentVariableBasedPathFilter(@"^{0}(?!Temp$)", "SystemRoot");
            AddEnvironmentVariableBasedPathFilter(@"^{0}(?!Temp$)", "windir");
            AddEnvironmentVariableBasedPathFilter(@"^{0}", "ProgramFiles");
            AddEnvironmentVariableBasedPathFilter(@"^{0}", "ProgramFiles(x86)");
            AddEnvironmentVariableBasedPathFilter(@"^{0}$", "SystemDrive");
        }

        private static void AddEnvironmentVariableBasedPathFilter(string formatRegularExpression, string name)
        {
            IDataContainer folder = name.FolderFromEnvironment();
            if (folder == null)
            {
                return;
            }
            string escapedPath = folder.FullName.Replace(@"\", @"\\");
            OS.PathFilters.Add(new Regex(formatRegularExpression.InvariantFormat(escapedPath)));
        }

        private void IntializeControls()
        {
            if (OS.Current.Platform == Platform.WindowsDesktop)
            {
                InitializeNotifyIcon();
            }

            ResizeEnd += (sender, e) =>
            {
                if (WindowState == FormWindowState.Normal)
                {
                    Preferences.MainWindowHeight = Height;
                    Preferences.MainWindowWidth = Width;
                }
            };
            Move += (sender, e) =>
            {
                if (WindowState == FormWindowState.Normal)
                {
                    Preferences.MainWindowLocation = Location;
                }
            };
            FormClosing += (sender, e) =>
            {
                EncryptPendingFiles();
                while (_mainViewModel.Working)
                {
                    Application.DoEvents();
                }
                WarnIfAnyDecryptedFiles();
            };

            _encryptToolStripButton.Tag = FileInfoTypes.EncryptableFile;
            _decryptToolStripButton.Tag = FileInfoTypes.EncryptedFile;

            _hiddenWatchedFoldersTabPage = _statusTabControl.TabPages["_watchedFoldersTabPage"];

            _recentFilesListView.SmallImageList = CreateSmallImageListToAvoidLocalizationIssuesWithDesignerAndResources();
            _recentFilesListView.LargeImageList = CreateLargeImageListToAvoidLocalizationIssuesWithDesignerAndResources();
            _recentFilesListView.ColumnWidthChanged += RecentFilesListView_ColumnWidthChanged;

            _updateStatusButton.Click += _updateToolStripButton_Click;
            _closeAndRemoveOpenFilesToolStripButton.Click += CloseAndRemoveOpenFilesToolStripButton_Click;
            _cleanDecryptedToolStripMenuItem.Click += CloseAndRemoveOpenFilesToolStripButton_Click;
            _optionsChangePassphraseToolStripMenuItem.Click += ChangePassphraseToolStripMenuItem_Click;

            InitializePolicyMenu();
        }

        private void InitializePolicyMenu()
        {
            string currentPolicyName = New<ICryptoPolicy>().Name;
            foreach (string policyName in New<CryptoPolicy>().PolicyNames)
            {
                ToolStripMenuItem item = new ToolStripMenuItem();
                item.Text = policyName;
                item.Checked = policyName == currentPolicyName;
                item.Click += PolicyMenuItem_Click;
                _debugCryptoPolicyToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        private void InitializeNotifyIcon()
        {
            _notifyIcon = new NotifyIcon(components);
            _notifyIcon.Icon = Resources.axcrypticon;
            _notifyIcon.Text = Resources.AxCryptFileEncryption;
            _notifyIcon.BalloonTipTitle = Resources.AxCryptFileEncryption;
            _notifyIcon.BalloonTipText = Resources.TrayBalloonTooltip;
            _notifyIcon.Visible = true;

            _notifyIcon.MouseClick += (sender, e) =>
            {
                Show();
                WindowState = FormWindowState.Normal;
            };

            Resize += (sender, e) =>
            {
                switch (WindowState)
                {
                    case FormWindowState.Minimized:
                        _notifyIcon.Visible = true;
                        _notifyIcon.ShowBalloonTip(500);
                        Hide();
                        break;

                    case FormWindowState.Normal:
                        _notifyIcon.Visible = false;
                        break;
                }
            };
        }

        private static ImageList CreateSmallImageListToAvoidLocalizationIssuesWithDesignerAndResources()
        {
            ImageList smallImageList = new ImageList();

            smallImageList.Images.Add("ActiveFile", Resources.activefilegreen16);
            smallImageList.Images.Add("Exclamation", Resources.exclamationgreen16);
            smallImageList.Images.Add("DecryptedFile", Resources.decryptedfilered16);
            smallImageList.Images.Add("DecryptedUnknownKeyFile", Resources.decryptedunknownkeyfilered16);
            smallImageList.Images.Add("ActiveFileKnownKey", Resources.fileknownkeygreen16);
            smallImageList.Images.Add("CleanUpNeeded", Resources.clean_broom);
            smallImageList.TransparentColor = System.Drawing.Color.Transparent;

            return smallImageList;
        }

        private static ImageList CreateLargeImageListToAvoidLocalizationIssuesWithDesignerAndResources()
        {
            ImageList largeImageList = new ImageList();

            largeImageList.Images.Add("ActiveFile", Resources.opendocument32);
            largeImageList.Images.Add("Exclamation", Resources.exclamationgreen32);
            largeImageList.TransparentColor = System.Drawing.Color.Transparent;

            return largeImageList;
        }

        private void RecentFilesListView_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            switch (e.ColumnIndex)
            {
                case 0:
                    Preferences.RecentFilesDocumentWidth = _recentFilesListView.Columns[e.ColumnIndex].Width;
                    break;

                case 2:
                    Preferences.RecentFilesDateTimeWidth = _recentFilesListView.Columns[e.ColumnIndex].Width;
                    break;

                case 3:
                    Preferences.RecentFilesEncryptedPathWidth = _recentFilesListView.Columns[e.ColumnIndex].Width;
                    break;

                case 4:
                    Preferences.RecentFilesCryptoNameWidth = _recentFilesListView.Columns[e.ColumnIndex].Width;
                    break;
            }
        }

        private void RestoreUserPreferences()
        {
            Preferences.RecentFilesMaxNumber = 100;

            if (WindowState == FormWindowState.Normal)
            {
                Height = Preferences.MainWindowHeight.Fallback(Height);
                Width = Preferences.MainWindowWidth.Fallback(Width);
                Location = Preferences.MainWindowLocation.Fallback(Location);
            }

            _recentFilesListView.Columns[0].Width = Preferences.RecentFilesDocumentWidth.Fallback(_recentFilesListView.Columns[0].Width);
            _recentFilesListView.Columns[2].Width = Preferences.RecentFilesDateTimeWidth.Fallback(_recentFilesListView.Columns[2].Width);
            _recentFilesListView.Columns[3].Width = Preferences.RecentFilesEncryptedPathWidth.Fallback(_recentFilesListView.Columns[3].Width);
            _recentFilesListView.Columns[4].Width = Preferences.RecentFilesCryptoNameWidth.Fallback(_recentFilesListView.Columns[4].Width);

            _mainViewModel.RecentFilesComparer = GetComparer(Preferences.RecentFilesSortColumn, !Preferences.RecentFilesAscending);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void BindToViewModels()
        {
            _mainViewModel.Title = "{0} {1}{2}".InvariantFormat(Application.ProductName, Application.ProductVersion, String.IsNullOrEmpty(AboutBox.AssemblyDescription) ? String.Empty : " " + AboutBox.AssemblyDescription);
            _mainViewModel.CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version;

            _mainViewModel.BindPropertyChanged("LoggedOn", (bool loggedOn) => { SetWindowTextWithLogonStatus(loggedOn); });
            _mainViewModel.BindPropertyChanged("LoggedOn", (bool loggedOn) => { _debugManageAccountToolStripMenuItem.Enabled = loggedOn && Resolve.KnownIdentities.DefaultEncryptionIdentity.UserKeys != null; });
            _mainViewModel.BindPropertyChanged("LoggedOn", (bool loggedOn) => { _optionsChangePassphraseToolStripMenuItem.Enabled = loggedOn && Resolve.KnownIdentities.DefaultEncryptionIdentity.UserKeys != null; });
            _mainViewModel.BindPropertyChanged("LoggedOn", (bool loggedOn) => { _exportSharingKeyToolStripMenuItem.Enabled = loggedOn && Resolve.KnownIdentities.DefaultEncryptionIdentity.UserKeys != null; });
            _mainViewModel.BindPropertyChanged("LoggedOn", (bool loggedOn) => { _exportMyPrivateKeyToolStripMenuItem.Enabled = loggedOn && Resolve.KnownIdentities.DefaultEncryptionIdentity.UserKeys != null; });
            _mainViewModel.BindPropertyChanged("LoggedOn", (bool loggedOn) => { _importOthersSharingKeyToolStripMenuItem.Enabled = loggedOn && Resolve.KnownIdentities.DefaultEncryptionIdentity.UserKeys != null; });
            _mainViewModel.BindPropertyChanged("LoggedOn", (bool loggedOn) => { _importMyPrivateKeyToolStripMenuItem.Enabled = !loggedOn; });
            _mainViewModel.BindPropertyChanged("LoggedOn", (bool loggedOn) => { _createAccountToolStripMenuItem.Enabled = !loggedOn; });
            _mainViewModel.BindPropertyChanged("LoggedOn", (bool loggedOn) => { _logOnLogOffLabel.Text = loggedOn ? Resources.LogOffText : Resources.LogOnText; });

            _mainViewModel.BindPropertyChanged("EncryptFileEnabled", (bool enabled) => { _encryptToolStripButton.Enabled = enabled; });
            _mainViewModel.BindPropertyChanged("EncryptFileEnabled", (bool enabled) => { _encryptToolStripMenuItem.Enabled = enabled; });
            _mainViewModel.BindPropertyChanged("DecryptFileEnabled", (bool enabled) => { _decryptToolStripButton.Enabled = enabled; });
            _mainViewModel.BindPropertyChanged("DecryptFileEnabled", (bool enabled) => { _decryptToolStripMenuItem.Enabled = enabled; });
            _mainViewModel.BindPropertyChanged("OpenEncryptedEnabled", (bool enabled) => { _openEncryptedToolStripMenuItem.Enabled = enabled; });
            _mainViewModel.BindPropertyChanged("FilesArePending", (bool filesArePending) => { _closeAndRemoveOpenFilesToolStripButton.Enabled = filesArePending; });
            _mainViewModel.BindPropertyChanged("WatchedFolders", (IEnumerable<string> folders) => { UpdateWatchedFolders(folders); });
            _mainViewModel.BindPropertyChanged("WatchedFoldersEnabled", (bool enabled) => { if (enabled) _statusTabControl.TabPages.Add(_hiddenWatchedFoldersTabPage); else _statusTabControl.TabPages.Remove(_hiddenWatchedFoldersTabPage); });
            _mainViewModel.BindPropertyChanged("WatchedFoldersEnabled", (bool enabled) => { _encryptedFoldersToolStripMenuItem.Enabled = enabled; });
            _mainViewModel.BindPropertyChanged("RecentFiles", (IEnumerable<ActiveFile> files) => { UpdateRecentFiles(files); });
            _mainViewModel.BindPropertyChanged("VersionUpdateStatus", (VersionUpdateStatus vus) => { UpdateVersionStatus(vus); });
            _mainViewModel.BindPropertyChanged("DebugMode", (bool enabled) => { UpdateDebugMode(enabled); });
            _mainViewModel.BindPropertyChanged("TryBrokenFile", (bool enabled) => { tryBrokenFileToolStripMenuItem.Checked = enabled; });

            _debugCheckVersionNowToolStripMenuItem.Click += (sender, e) => { _mainViewModel.UpdateCheck.Execute(DateTime.MinValue); };
            _optionsClearAllSettingsAndExitToolStripMenuItem.Click += (sender, e) => { _mainViewModel.ClearPassphraseMemory.Execute(null); };
            _optionsDebugToolStripMenuItem.Click += (sender, e) => { _mainViewModel.DebugMode = !_mainViewModel.DebugMode; };
            _removeRecentFileToolStripMenuItem.Click += (sender, e) => { _mainViewModel.RemoveRecentFiles.Execute(_mainViewModel.SelectedRecentFiles); };

            _watchedFoldersListView.SelectedIndexChanged += (sender, e) => { _mainViewModel.SelectedWatchedFolders = _watchedFoldersListView.SelectedItems.Cast<ListViewItem>().Select(lvi => lvi.Text); };
            _watchedFoldersListView.MouseClick += (sender, e) => { if (e.Button == MouseButtons.Right) _watchedFoldersContextMenuStrip.Show((Control)sender, e.Location); };
            _watchedFoldersListView.DragOver += (sender, e) => { _mainViewModel.DragAndDropFiles = e.GetDragged(); e.Effect = GetEffectsForWatchedFolders(e); };
            _watchedFoldersListView.DragDrop += (sender, e) => { _mainViewModel.AddWatchedFolders.Execute(_mainViewModel.DragAndDropFiles); };
            _watchedFoldersOpenExplorerHereMenuItem.Click += (sender, e) => { _mainViewModel.OpenSelectedFolder.Execute(_mainViewModel.SelectedWatchedFolders.First()); };
            _watchedFoldersRemoveMenuItem.Click += (sender, e) => { _mainViewModel.RemoveWatchedFolders.Execute(_mainViewModel.SelectedWatchedFolders); };

            _recentFilesListView.ColumnClick += (sender, e) => { SetSortOrder(e.Column); };
            _recentFilesListView.SelectedIndexChanged += (sender, e) => { _mainViewModel.SelectedRecentFiles = _recentFilesListView.SelectedItems.Cast<ListViewItem>().Select(lvi => lvi.SubItems["EncryptedPath"].Text); };
            _recentFilesListView.MouseClick += (sender, e) => { if (e.Button == MouseButtons.Right) _recentFilesContextMenuStrip.Show((Control)sender, e.Location); };
            _recentFilesListView.MouseClick += (sender, e) => { if (e.Button == MouseButtons.Right) _shareKeysToolStripMenuItem.Enabled = _recentFilesListView.SelectedItems.Count == 1 && Resolve.KnownIdentities.IsLoggedOn; };
            _recentFilesListView.DragOver += (sender, e) => { _mainViewModel.DragAndDropFiles = e.GetDragged(); e.Effect = GetEffectsForRecentFiles(e); };

            _shareKeysToolStripMenuItem.Click += (sender, e) => { ShareKeys(_mainViewModel.SelectedRecentFiles); };
            _mainToolStrip.DragOver += (sender, e) => { _mainViewModel.DragAndDropFiles = e.GetDragged(); e.Effect = GetEffectsForMainToolStrip(e); };

            _knownFoldersViewModel.BindPropertyChanged("KnownFolders", (IEnumerable<KnownFolder> folders) => UpdateKnownFolders(folders));
            _knownFoldersViewModel.KnownFolders = KnownFoldersDiscovery.Discover();
        }

        private static void BindToWatchedFoldersViewModel()
        {
        }

        private void BindToFileOperationViewModel()
        {
            _decryptAndRemoveFromListToolStripMenuItem.Click += (sender, e) => { _fileOperationViewModel.DecryptFiles.Execute(_mainViewModel.SelectedRecentFiles); };
            _decryptToolStripButton.Click += (sender, e) => { _fileOperationViewModel.DecryptFiles.Execute(null); };
            _decryptToolStripMenuItem.Click += (sender, e) => { _fileOperationViewModel.DecryptFiles.Execute(null); };
            _logOnLogOffLabel.LinkClicked += (sender, e) => { LogOnOrLogOffAndLogOnAgain(); };
            _encryptToolStripButton.Click += (sender, e) => { _fileOperationViewModel.EncryptFiles.Execute(null); };
            _encryptToolStripMenuItem.Click += (sender, e) => { _fileOperationViewModel.EncryptFiles.Execute(null); };
            _openEncryptedToolStripMenuItem.Click += (sender, e) => { _fileOperationViewModel.OpenFilesFromFolder.Execute(String.Empty); };
            _secureDeleteToolStripMenuItem.Click += (sender, e) => { _fileOperationViewModel.WipeFiles.Execute(null); };

            _watchedFoldersListView.MouseDoubleClick += (sender, e) => { _fileOperationViewModel.OpenFilesFromFolder.Execute(_mainViewModel.SelectedWatchedFolders.FirstOrDefault()); };
            _watchedFoldersdecryptTemporarilyMenuItem.Click += (sender, e) => { _fileOperationViewModel.DecryptFolders.Execute(_mainViewModel.SelectedWatchedFolders); };

            _recentFilesOpenToolStripMenuItem.Click += (sender, e) => { _fileOperationViewModel.OpenFiles.Execute(_mainViewModel.SelectedRecentFiles); };
            _recentFilesListView.MouseDoubleClick += (sender, e) => { _fileOperationViewModel.OpenFiles.Execute(_mainViewModel.SelectedRecentFiles); };
            _recentFilesListView.DragDrop += (sender, e) => { DropFilesOrFoldersInRecentFilesListView(); };

            _fileOperationViewModel.IdentityViewModel.LoggingOn += (sender, e) => { HandleLogOn(e); };
            _fileOperationViewModel.SelectingFiles += (sender, e) => { HandleFileSelection(e); };

            _decryptToolStripButton.Tag = _fileOperationViewModel.DecryptFiles;
            _encryptToolStripButton.Tag = _fileOperationViewModel.EncryptFiles;
        }

        private void LogOnOrLogOffAndLogOnAgain()
        {
            bool wasLoggedOn = Resolve.KnownIdentities.IsLoggedOn;
            _fileOperationViewModel.IdentityViewModel.LogOnLogOff.Execute(Resolve.CryptoFactory.Default.Id);
            bool didLogOff = wasLoggedOn && !Resolve.KnownIdentities.IsLoggedOn;
            if (didLogOff)
            {
                _fileOperationViewModel.IdentityViewModel.LogOnLogOff.Execute(Resolve.CryptoFactory.Default.Id);
            }
        }

        private void DropFilesOrFoldersInRecentFilesListView()
        {
            if (_mainViewModel.DroppableAsRecent)
            {
                _fileOperationViewModel.AddRecentFiles.Execute(_mainViewModel.DragAndDropFiles);
            }
            if (_mainViewModel.DroppableAsWatchedFolder)
            {
                ShowWatchedFolders(_mainViewModel.DragAndDropFiles);
            }
        }

        private void HandleLogOn(LogOnEventArgs e)
        {
            if (e.IsAskingForPreviouslyUnknownPassphrase)
            {
                HandleCreateNewLogOn(e);
            }
            else
            {
                HandleExistingLogOn(e);
            }
        }

        private void HandleCreateNewLogOn(LogOnEventArgs e)
        {
            RestoreWindowWithFocus();
            if (!String.IsNullOrEmpty(e.EncryptedFileFullName))
            {
                HandleCreateNewLogOnForEncryptedFile(e);
            }
            else
            {
                HandleCreateNewAccount(e);
            }
        }

        private void HandleCreateNewLogOnForEncryptedFile(LogOnEventArgs e)
        {
            using (NewPassphraseDialog passphraseDialog = new NewPassphraseDialog(this, Resources.NewPassphraseDialogTitle, e.Passphrase, e.EncryptedFileFullName))
            {
                passphraseDialog.ShowPassphraseCheckBox.Checked = e.DisplayPassphrase;
                DialogResult dialogResult = passphraseDialog.ShowDialog(this);
                if (dialogResult != DialogResult.OK || passphraseDialog.PassphraseTextBox.Text.Length == 0)
                {
                    e.Cancel = true;
                    return;
                }
                e.DisplayPassphrase = passphraseDialog.ShowPassphraseCheckBox.Checked;
                e.Passphrase = passphraseDialog.PassphraseTextBox.Text;
                e.Name = String.Empty;
            }
            return;
        }

        private void HandleCreateNewAccount(LogOnEventArgs e)
        {
            using (CreateNewAccountDialog dialog = new CreateNewAccountDialog(this, e.Passphrase, EmailAddress.Empty))
            {
                DialogResult dialogResult = dialog.ShowDialog(this);
                if (dialogResult != DialogResult.OK)
                {
                    e.Cancel = true;
                    return;
                }
                e.DisplayPassphrase = dialog.ShowPassphraseCheckBox.Checked;
                e.Passphrase = dialog.PassphraseTextBox.Text;
                e.UserEmail = dialog.EmailTextBox.Text;
            }
        }

        private void HandleExistingLogOn(LogOnEventArgs e)
        {
            RestoreWindowWithFocus();
            if (!String.IsNullOrEmpty(e.EncryptedFileFullName) && (String.IsNullOrEmpty(Resolve.UserSettings.UserEmail) || Resolve.KnownIdentities.IsLoggedOn))
            {
                HandleExistingLogOnForEncryptedFile(e);
            }
            else
            {
                HandleExistingAccountLogOn(e);
            }
        }

        private void HandleExistingLogOnForEncryptedFile(LogOnEventArgs e)
        {
            using (LogOnDialog logOnDialog = new LogOnDialog(this, e.EncryptedFileFullName))
            {
                logOnDialog.ShowPassphraseCheckBox.Checked = e.DisplayPassphrase;
                DialogResult dialogResult = logOnDialog.ShowDialog(this);
                if (dialogResult == DialogResult.Retry)
                {
                    e.Passphrase = logOnDialog.PassphraseTextBox.Text;
                    e.IsAskingForPreviouslyUnknownPassphrase = true;
                    return;
                }

                if (dialogResult != DialogResult.OK || logOnDialog.PassphraseTextBox.Text.Length == 0)
                {
                    e.Cancel = true;
                    return;
                }
                e.DisplayPassphrase = logOnDialog.ShowPassphraseCheckBox.Checked;
                e.Passphrase = logOnDialog.PassphraseTextBox.Text;
            }
            return;
        }

        private void HandleExistingAccountLogOn(LogOnEventArgs e)
        {
            using (LogOnAccountDialog logOnDialog = new LogOnAccountDialog(this, Resolve.UserSettings))
            {
                logOnDialog.ShowPassphraseCheckBox.Checked = e.DisplayPassphrase;
                DialogResult dialogResult = logOnDialog.ShowDialog(this);
                if (dialogResult == DialogResult.Retry)
                {
                    e.Passphrase = logOnDialog.PassphraseTextBox.Text;
                    e.UserEmail = logOnDialog.EmailTextBox.Text;
                    e.IsAskingForPreviouslyUnknownPassphrase = true;
                    return;
                }

                if (dialogResult == DialogResult.Cancel)
                {
                    CloseAndExit();
                }

                if (dialogResult != DialogResult.OK || logOnDialog.PassphraseTextBox.Text.Length == 0)
                {
                    e.Cancel = true;
                    return;
                }
                e.DisplayPassphrase = logOnDialog.ShowPassphraseCheckBox.Checked;
                e.Passphrase = logOnDialog.PassphraseTextBox.Text;
                e.UserEmail = logOnDialog.EmailTextBox.Text;
            }
            return;
        }

        private static void HandleFileSelection(FileSelectionEventArgs e)
        {
            switch (e.FileSelectionType)
            {
                case FileSelectionType.SaveAsEncrypted:
                case FileSelectionType.SaveAsDecrypted:
                    HandleSaveAsFileSelection(e);
                    break;

                case FileSelectionType.WipeConfirm:
                    HandleWipeConfirm(e);
                    break;

                default:
                    HandleOpenFileSelection(e);
                    break;
            }
        }

        private static void HandleWipeConfirm(FileSelectionEventArgs e)
        {
            using (ConfirmWipeDialog cwd = new ConfirmWipeDialog())
            {
                cwd.FileNameLabel.Text = Path.GetFileName(e.SelectedFiles[0]);
                e.Skip = false;
                DialogResult confirmResult = cwd.ShowDialog();
                e.ConfirmAll = cwd.ConfirmAllCheckBox.Checked;
                e.Skip = confirmResult == DialogResult.No;
                e.Cancel = confirmResult == DialogResult.Cancel;
            }
        }

        private static void HandleOpenFileSelection(FileSelectionEventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (e.SelectedFiles != null && e.SelectedFiles.Count > 0 && !String.IsNullOrEmpty(e.SelectedFiles[0]))
                {
                    IDataContainer initialFolder = New<IDataContainer>(e.SelectedFiles[0]);
                    if (initialFolder.IsAvailable)
                    {
                        ofd.InitialDirectory = initialFolder.FullName;
                    }
                }
                switch (e.FileSelectionType)
                {
                    case FileSelectionType.Decrypt:
                        ofd.Title = Resources.DecryptFileOpenDialogTitle;
                        ofd.Multiselect = true;
                        ofd.CheckFileExists = true;
                        ofd.CheckPathExists = true;
                        ofd.DefaultExt = OS.Current.AxCryptExtension;
                        ofd.Filter = Resources.EncryptedFileDialogFilterPattern.InvariantFormat("{0}".InvariantFormat(OS.Current.AxCryptExtension));
                        ofd.Multiselect = true;
                        break;

                    case FileSelectionType.Encrypt:
                        ofd.Title = Resources.EncryptFileOpenDialogTitle;
                        ofd.Multiselect = true;
                        ofd.CheckFileExists = true;
                        ofd.CheckPathExists = true;
                        break;

                    case FileSelectionType.Open:
                        ofd.Title = Resources.OpenEncryptedFileOpenDialogTitle;
                        ofd.Multiselect = false;
                        ofd.CheckFileExists = true;
                        ofd.CheckPathExists = true;
                        ofd.DefaultExt = OS.Current.AxCryptExtension;
                        ofd.Filter = Resources.EncryptedFileDialogFilterPattern.InvariantFormat("{0}".InvariantFormat(OS.Current.AxCryptExtension));
                        break;

                    case FileSelectionType.Wipe:
                        ofd.Title = Resources.WipeFileSelectFileDialogTitle;
                        ofd.Multiselect = true;
                        ofd.CheckFileExists = true;
                        ofd.CheckPathExists = true;
                        break;

                    case FileSelectionType.ImportPublicKeys:
                        ofd.Title = Resources.ImportPublicKeysFileSelectionTitle;
                        ofd.Multiselect = true;
                        ofd.CheckFileExists = true;
                        ofd.CheckPathExists = true;
                        ofd.Filter = Resources.ImportPublicKeysFileFilter;
                        break;

                    case FileSelectionType.ImportPrivateKeys:
                        ofd.Title = Resources.ImportPrivateKeysFileSelectionTitle;
                        ofd.Multiselect = false;
                        ofd.CheckFileExists = true;
                        ofd.CheckPathExists = true;
                        ofd.Filter = Resources.ImportPrivateKeysFileFilter;
                        break;

                    default:
                        break;
                }
                DialogResult result = ofd.ShowDialog();
                e.Cancel = result != DialogResult.OK;
                e.SelectedFiles.Clear();
                foreach (string fileName in ofd.FileNames)
                {
                    e.SelectedFiles.Add(fileName);
                }
            }
        }

        private static void HandleSaveAsFileSelection(FileSelectionEventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                switch (e.FileSelectionType)
                {
                    case FileSelectionType.SaveAsEncrypted:
                        sfd.Title = Resources.EncryptFileSaveAsDialogTitle;
                        sfd.DefaultExt = OS.Current.AxCryptExtension;
                        sfd.AddExtension = true;
                        sfd.Filter = Resources.EncryptedFileDialogFilterPattern.InvariantFormat(OS.Current.AxCryptExtension);
                        break;

                    case FileSelectionType.SaveAsDecrypted:
                        string extension = Path.GetExtension(e.SelectedFiles[0]);
                        sfd.Title = Resources.DecryptedSaveAsFileDialogTitle;
                        sfd.DefaultExt = extension;
                        sfd.AddExtension = !String.IsNullOrEmpty(extension);
                        sfd.Filter = Resources.DecryptedSaveAsFileDialogFilterPattern.InvariantFormat(extension);
                        break;
                }
                sfd.CheckPathExists = true;
                sfd.FileName = Path.GetFileName(e.SelectedFiles[0]);
                sfd.InitialDirectory = Path.GetDirectoryName(e.SelectedFiles[0]);
                sfd.ValidateNames = true;
                sfd.OverwritePrompt = true;
                sfd.RestoreDirectory = true;
                DialogResult saveAsResult = sfd.ShowDialog();
                e.Cancel = saveAsResult != DialogResult.OK;
                e.SelectedFiles[0] = sfd.FileName;
            }
        }

        private void AxCryptMainForm_CommandComplete(object sender, CommandCompleteEventArgs e)
        {
            Resolve.UIThread.RunOnUIThread(() => DoRequest(e));
        }

        private CommandCompleteEventArgs _pendingRequest = new CommandCompleteEventArgs(CommandVerb.Unknown, new string[0]);

        private void DoRequest(CommandCompleteEventArgs e)
        {
            if (!Resolve.KnownIdentities.IsLoggedOn)
            {
                switch (e.Verb)
                {
                    case CommandVerb.Encrypt:
                    case CommandVerb.Decrypt:
                    case CommandVerb.Open:
                    case CommandVerb.ShowLogOn:
                        if (_pendingRequest.Verb == CommandVerb.Startup)
                        {
                            _pendingRequest = e;
                            return;
                        }
                        _pendingRequest = e;
                        LogOnAndDoPendingRequest();
                        return;

                    default:
                        break;
                }
            }
            switch (e.Verb)
            {
                case CommandVerb.Encrypt:
                    _fileOperationViewModel.EncryptFiles.Execute(e.Arguments);
                    break;

                case CommandVerb.Decrypt:
                    _fileOperationViewModel.DecryptFiles.Execute(e.Arguments);
                    break;

                case CommandVerb.Open:
                    _fileOperationViewModel.OpenFiles.Execute(e.Arguments);
                    break;

                case CommandVerb.Wipe:
                    _fileOperationViewModel.WipeFiles.Execute(e.Arguments);
                    break;

                case CommandVerb.RandomRename:
                    _fileOperationViewModel.RandomRenameFiles.Execute(e.Arguments);
                    break;

                case CommandVerb.Exit:
                    CloseAndExit();
                    break;

                case CommandVerb.Show:
                    RestoreWindowWithFocus();
                    break;

                case CommandVerb.Register:
                    Process.Start("https://www.axantum.com/Xecrets/LoggedOff/Register.aspx");
                    break;

                case CommandVerb.About:
                    RestoreWindowWithFocus();
                    using (AboutBox aboutBox = new AboutBox())
                    {
                        aboutBox.ShowDialog();
                    }
                    break;

                default:
                    break;
            }
        }

        private static bool IsImmediate(CommandVerb verb)
        {
            switch (verb)
            {
                case CommandVerb.Wipe:
                case CommandVerb.Show:
                case CommandVerb.Exit:
                case CommandVerb.About:
                case CommandVerb.Register:
                    return true;
            }
            return false;
        }

        private void RestoreWindowWithFocus()
        {
            if (ContainsFocus)
            {
                return;
            }

            Show();
            WindowState = FormWindowState.Normal;
            Activate();

            if (OwnedForms.Any())
            {
                OwnedForms.First().Focus();
            }
        }

        private DragDropEffects GetEffectsForMainToolStrip(DragEventArgs e)
        {
            if (_mainViewModel.DragAndDropFiles.Count() != 1)
            {
                return DragDropEffects.None;
            }
            Point point = _mainToolStrip.PointToClient(new Point(e.X, e.Y));
            ToolStripButton button = _mainToolStrip.GetItemAt(point) as ToolStripButton;
            if (button == null)
            {
                return DragDropEffects.None;
            }
            if (!button.Enabled)
            {
                return DragDropEffects.None;
            }
            if (button == _encryptToolStripButton)
            {
                if ((_mainViewModel.DragAndDropFilesTypes & FileInfoTypes.EncryptableFile) == FileInfoTypes.EncryptableFile)
                {
                    return (DragDropEffects.Link | DragDropEffects.Copy) & e.AllowedEffect;
                }
            }
            if (button == _decryptToolStripButton)
            {
                if ((_mainViewModel.DragAndDropFilesTypes & FileInfoTypes.EncryptedFile) == FileInfoTypes.EncryptedFile)
                {
                    return (DragDropEffects.Link | DragDropEffects.Copy) & e.AllowedEffect;
                }
            }
            return DragDropEffects.None;
        }

        private DragDropEffects GetEffectsForRecentFiles(DragEventArgs e)
        {
            if (!_mainViewModel.DroppableAsRecent && !_mainViewModel.DroppableAsWatchedFolder)
            {
                return DragDropEffects.None;
            }
            return (DragDropEffects.Link | DragDropEffects.Copy) & e.AllowedEffect;
        }

        public DragDropEffects GetEffectsForWatchedFolders(DragEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            if (!_mainViewModel.DroppableAsWatchedFolder)
            {
                return DragDropEffects.None;
            }
            return (DragDropEffects.Link | DragDropEffects.Copy) & e.AllowedEffect;
        }

        private void SetWindowTextWithLogonStatus(bool isLoggedOn)
        {
            string logonStatus;
            if (isLoggedOn)
            {
                UserKeyPair userKeys = Resolve.KnownIdentities.DefaultEncryptionIdentity.UserKeys;
                logonStatus = userKeys != UserKeyPair.Empty ? Resources.AccountLoggedOnStatusText.InvariantFormat(userKeys.UserEmail) : Resources.LoggedOnStatusText.InvariantFormat(String.Empty);
            }
            else
            {
                logonStatus = Resources.LoggedOffStatusText;
            }
            Text = "{0} - {1}".InvariantFormat(_mainViewModel.Title, logonStatus);
        }

        private void UpdateVersionStatus(VersionUpdateStatus status)
        {
            switch (status)
            {
                case VersionUpdateStatus.IsUpToDateOrRecentlyChecked:
                    _updateStatusButton.ToolTipText = Axantum.AxCrypt.Properties.Resources.NoNeedToCheckForUpdatesTooltip;
                    _updateStatusButton.Image = Resources.bulb_green_40px;
                    _updateStatusButton.Enabled = false;
                    break;

                case VersionUpdateStatus.LongTimeSinceLastSuccessfulCheck:
                    _updateStatusButton.ToolTipText = Axantum.AxCrypt.Properties.Resources.OldVersionTooltip;
                    _updateStatusButton.Image = Resources.bulb_red_40px;
                    _updateStatusButton.Enabled = true;
                    break;

                case VersionUpdateStatus.NewerVersionIsAvailable:
                    _updateStatusButton.ToolTipText = Axantum.AxCrypt.Properties.Resources.NewVersionIsAvailableTooltip.InvariantFormat(_mainViewModel.UpdatedVersion);
                    _updateStatusButton.Image = Resources.bulb_red_40px;
                    _updateStatusButton.Enabled = true;
                    break;

                case VersionUpdateStatus.Unknown:
                case VersionUpdateStatus.ShortTimeSinceLastSuccessfulCheck:
                    _updateStatusButton.ToolTipText = Axantum.AxCrypt.Properties.Resources.ClickToCheckForNewerVersionTooltip;
                    _updateStatusButton.Image = Resources.bulb_red_40px;
                    _updateStatusButton.Enabled = true;
                    break;
            }
        }

        private void UpdateDebugMode(bool enabled)
        {
            _optionsDebugToolStripMenuItem.Checked = enabled;
            _debugToolStripMenuItem.Visible = enabled;
        }

        private void UpdateWatchedFolders(IEnumerable<string> watchedFolders)
        {
            _watchedFoldersListView.BeginUpdate();
            try
            {
                _watchedFoldersListView.Items.Clear();
                foreach (string folder in watchedFolders)
                {
                    ListViewItem item = _watchedFoldersListView.Items.Add(folder);
                    item.Name = folder;
                }
            }
            finally
            {
                _watchedFoldersListView.EndUpdate();
            }
        }

        private void UpdateRecentFiles(IEnumerable<ActiveFile> files)
        {
            _recentFilesListView.BeginUpdate();
            try
            {
                HashSet<string> newFiles = new HashSet<string>(files.Select(f => f.DecryptedFileInfo.FullName));
                Dictionary<string, int> currentFiles = new Dictionary<string, int>();
                for (int i = 0; i < _recentFilesListView.Items.Count;)
                {
                    if (!newFiles.Contains(_recentFilesListView.Items[i].Name))
                    {
                        _recentFilesListView.Items.RemoveAt(i);
                        continue;
                    }
                    ++i;
                    currentFiles.Add(_recentFilesListView.Items[i].Name, i);
                }
                List<ListViewItem> newItems = new List<ListViewItem>();
                foreach (ActiveFile file in files)
                {
                    string text = Path.GetFileName(file.DecryptedFileInfo.FullName);
                    ListViewItem item = new ListViewItem(text);
                    item.Name = file.EncryptedFileInfo.FullName;

                    ListViewItem.ListViewSubItem sharingIndicatorColumn = item.SubItems.Add(String.Empty);
                    sharingIndicatorColumn.Name = "SharingIndicator";

                    ListViewItem.ListViewSubItem dateColumn = item.SubItems.Add(String.Empty);
                    dateColumn.Name = "Date";

                    ListViewItem.ListViewSubItem encryptedPathColumn = item.SubItems.Add(String.Empty);
                    encryptedPathColumn.Name = "EncryptedPath";

                    ListViewItem.ListViewSubItem cryptoNameColumn = item.SubItems.Add(String.Empty);
                    cryptoNameColumn.Name = "CryptoName";

                    UpdateListViewItemAsync(item, file);
                    int i;
                    if (currentFiles.TryGetValue(item.Name, out i))
                    {
                        _recentFilesListView.Items[i] = item;
                    }
                    else
                    {
                        newItems.Add(item);
                    }
                }
                _recentFilesListView.Items.AddRange(newItems.ToArray());
                while (_recentFilesListView.Items.Count > Preferences.RecentFilesMaxNumber)
                {
                    _recentFilesListView.Items.RemoveAt(_recentFilesListView.Items.Count - 1);
                }
            }
            finally
            {
                _recentFilesListView.EndUpdate();
            }
        }

        private static async void UpdateListViewItemAsync(ListViewItem item, ActiveFile activeFile)
        {
            UpdateStatusDependentPropertiesOfListViewItem(item, activeFile);

            EncryptedProperties encryptedProperties = await EncryptedPropertiesAsync(activeFile.EncryptedFileInfo);

            item.SubItems["EncryptedPath"].Text = activeFile.EncryptedFileInfo.FullName;
            item.SubItems["SharingIndicator"].Text = SharingIndicator(encryptedProperties.SharedKeyHolders.Count());
            item.SubItems["Date"].Text = activeFile.Properties.LastActivityTimeUtc.ToLocalTime().ToString(CultureInfo.CurrentCulture);
            item.SubItems["Date"].Tag = activeFile.Properties.LastActivityTimeUtc;

            try
            {
                if (activeFile.Properties.CryptoId != Guid.Empty)
                {
                    item.SubItems["CryptoName"].Text = Resolve.CryptoFactory.Create(activeFile.Properties.CryptoId).Name;
                }
            }
            catch (ArgumentException)
            {
                item.SubItems["CryptoName"].Text = Resources.UnknownCrypto;
            }
        }

        private static string SharingIndicator(int count)
        {
            if (count == 0)
            {
                return String.Empty;
            }
            return count.ToString(CultureInfo.CurrentCulture);
        }

        private static async Task<EncryptedProperties> EncryptedPropertiesAsync(IDataStore dataStore)
        {
            return await Task.Run(() => EncryptedProperties.Create(dataStore));
        }

        private static void UpdateStatusDependentPropertiesOfListViewItem(ListViewItem item, ActiveFile activeFile)
        {
            switch (activeFile.VisualState)
            {
                case ActiveFileVisualState.DecryptedWithKnownKey:
                case ActiveFileVisualState.DecryptedWithoutKnownKey:
                    item.ImageKey = "CleanUpNeeded";
                    item.ToolTipText = Resources.CleanUpNeededToolTip;
                    break;

                case ActiveFileVisualState.EncryptedWithoutKnownKey:
                case ActiveFileVisualState.EncryptedWithKnownKey:
                    item.ImageKey = String.Empty;
                    item.ToolTipText = Resources.DoubleClickToOpenToolTip;
                    break;

                default:
                    throw new InvalidOperationException("Unexpected ActiveFileVisualState value.");
            }
        }

        private void UpdateKnownFolders(IEnumerable<KnownFolder> folders)
        {
            GetKnownFoldersToolItems().Skip(1).ToList().ForEach(f => _mainToolStrip.Items.Remove(f));

            bool anyFolders = folders.Any();
            GetKnownFoldersToolItems().First().Visible = anyFolders;

            if (!anyFolders)
            {
                return;
            }

            int i = _mainToolStrip.Items.IndexOf(GetKnownFoldersToolItems().First()) + 1;
            foreach (KnownFolder knownFolder in folders)
            {
                ToolStripButton button = new ToolStripButton((Image)knownFolder.Image);
                button.ImageScaling = ToolStripItemImageScaling.None;
                button.Tag = knownFolder;
                button.Click += (sender, e) =>
                {
                    ToolStripItem item = sender as ToolStripItem;
                    _fileOperationViewModel.OpenFilesFromFolder.Execute(((KnownFolder)item.Tag).My.FullName);
                };
                button.Image = (Image)knownFolder.Image;
                button.Enabled = knownFolder.Enabled;
                _mainToolStrip.Items.Insert(i, button);
                ++i;
            }
        }

        private List<ToolStripItem> GetKnownFoldersToolItems()
        {
            List<ToolStripItem> buttons = new List<ToolStripItem>();
            int i = _mainToolStrip.Items.IndexOf(_knownFoldersSeparator);
            buttons.Add(_mainToolStrip.Items[i++]);
            while (i < _mainToolStrip.Items.Count && _mainToolStrip.Items[i] is ToolStripButton)
            {
                buttons.Add(_mainToolStrip.Items[i++]);
            }
            return buttons;
        }

        public bool CheckStatusAndShowMessage(ErrorStatus status, string displayContext)
        {
            switch (status)
            {
                case ErrorStatus.Success:
                    return true;

                case ErrorStatus.UnspecifiedError:
                    Resources.FileOperationFailed.InvariantFormat(displayContext).ShowWarning();
                    break;

                case ErrorStatus.FileAlreadyExists:
                    Resources.FileAlreadyExists.InvariantFormat(displayContext).ShowWarning();
                    break;

                case ErrorStatus.FileDoesNotExist:
                    Resources.FileDoesNotExist.InvariantFormat(displayContext).ShowWarning();
                    break;

                case ErrorStatus.CannotWriteDestination:
                    Resources.CannotWrite.InvariantFormat(displayContext).ShowWarning();
                    break;

                case ErrorStatus.CannotStartApplication:
                    Resources.CannotStartApplication.InvariantFormat(displayContext).ShowWarning();
                    break;

                case ErrorStatus.InconsistentState:
                    Resources.InconsistentState.InvariantFormat(displayContext).ShowWarning();
                    break;

                case ErrorStatus.InvalidKey:
                    Resources.InvalidKey.InvariantFormat(displayContext).ShowWarning();
                    break;

                case ErrorStatus.Canceled:
                    break;

                case ErrorStatus.Exception:
                    Resources.Exception.InvariantFormat(displayContext).ShowWarning();
                    break;

                case ErrorStatus.InvalidPath:
                    Resources.InvalidPath.InvariantFormat(displayContext).ShowWarning();
                    break;

                case ErrorStatus.FolderAlreadyWatched:
                    Resources.FolderAlreadyWatched.InvariantFormat(displayContext).ShowWarning();
                    break;

                case ErrorStatus.FileLocked:
                    Resources.FileIsLockedWarning.InvariantFormat(displayContext).ShowWarning();
                    break;

                case ErrorStatus.Unknown:
                    Resources.UnknownFileStatus.InvariantFormat(displayContext).ShowWarning();
                    break;

                case ErrorStatus.Working:
                    Resources.WorkingFileStatus.InvariantFormat(displayContext).ShowWarning();
                    break;

                case ErrorStatus.Aborted:
                    Resources.AbortedFileStatus.InvariantFormat(displayContext).ShowWarning();
                    break;

                case ErrorStatus.FileAlreadyEncrypted:
                    Resources.FileAlreadyEncryptedStatus.InvariantFormat(displayContext).ShowWarning();
                    break;

                default:
                    Resources.UnrecognizedError.InvariantFormat(displayContext, status).ShowWarning();
                    break;
            }
            return false;
        }

        private void _exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseAndExit();
        }

        private void CloseAndExit()
        {
            if (_debugOutput != null)
            {
                _debugOutput.AllowClose = true;
            }
            Application.Exit();
        }

        #region ToolStrip

        private void MainToolStrip_DragDrop(object sender, DragEventArgs e)
        {
            Point point = _mainToolStrip.PointToClient(new Point(e.X, e.Y));
            ToolStripButton button = _mainToolStrip.GetItemAt(point) as ToolStripButton;
            if (button == null)
            {
                return;
            }
            if (!button.Enabled)
            {
                return;
            }

            ((IAction)button.Tag).Execute(_mainViewModel.DragAndDropFiles);
        }

        #endregion ToolStrip

        private void EncryptPendingFiles()
        {
            _mainViewModel.EncryptPendingFiles.Execute(null);
        }

        private void WarnIfAnyDecryptedFiles()
        {
            IEnumerable<ActiveFile> openFiles = _mainViewModel.DecryptedFiles;
            if (!openFiles.Any())
            {
                return;
            }
            StringBuilder sb = new StringBuilder();
            foreach (ActiveFile openFile in openFiles)
            {
                sb.Append("{0}{1}".InvariantFormat(Path.GetFileName(openFile.DecryptedFileInfo.FullName), Environment.NewLine));
            }
            sb.ToString().ShowWarning();
        }

        private void SetSortOrder(int column)
        {
            ActiveFileComparer comparer = GetComparer(column, Preferences.RecentFilesSortColumn == column ? Preferences.RecentFilesAscending : false);
            if (comparer == null)
            {
                return;
            }
            Preferences.RecentFilesAscending = !comparer.ReverseSort;
            Preferences.RecentFilesSortColumn = column;
            _mainViewModel.RecentFilesComparer = comparer;
        }

        private static ActiveFileComparer GetComparer(int column, bool reverseSort)
        {
            ActiveFileComparer comparer;
            switch (column)
            {
                case 0:
                    comparer = ActiveFileComparer.DecryptedNameComparer;
                    break;

                case 1:
                    return null;

                case 2:
                    comparer = ActiveFileComparer.DateComparer;
                    break;

                case 3:
                    comparer = ActiveFileComparer.EncryptedNameComparer;
                    break;

                case 4:
                    comparer = ActiveFileComparer.CryptoNameComparer;
                    break;

                default:
                    throw new ArgumentException("column is wrong.");
            }
            comparer.ReverseSort = reverseSort;
            return comparer;
        }

        private static ActiveFileComparer ChooseComparer(ActiveFileComparer current, ActiveFileComparer comparer)
        {
            if (current != null && current.GetType() == comparer.GetType())
            {
                comparer.ReverseSort = !current.ReverseSort;
            }
            return comparer;
        }

        private void CloseOpenFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EncryptPendingFiles();
        }

        private void CloseAndRemoveOpenFilesToolStripButton_Click(object sender, EventArgs e)
        {
            EncryptPendingFiles();
        }

        private void ProgressBackgroundWorker_ProgressBarClicked(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }
            _progressContextMenuStrip.Tag = sender;
            _progressContextMenuStrip.Show((Control)sender, e.Location);
        }

        private void ProgressBackgroundWorker_ProgressBarCreated(object sender, ControlEventArgs e)
        {
            _progressTableLayoutPanel.Controls.Add(e.Control);
        }

        private void ProgressContextCancelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
            ContextMenuStrip menuStrip = (ContextMenuStrip)menuItem.GetCurrentParent();
            ProgressBar progressBar = (ProgressBar)menuStrip.Tag;
            IProgressContext progress = (IProgressContext)progressBar.Tag;
            progress.Cancel = true;
        }

        private void EnglishLanguageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetLanguage("en-US");
        }

        private void SwedishLanguageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetLanguage("sv-SE");
        }

        private void SetLanguage(string cultureName)
        {
            Resolve.UserSettings.CultureName = cultureName;
            if (Resolve.Log.IsInfoEnabled)
            {
                Resolve.Log.LogInfo("Set new UI language culture to '{0}'.".InvariantFormat(Resolve.UserSettings.CultureName));
            }
            Resources.LanguageChangeRestartPrompt.ShowWarning();
            CloseAndExit();
        }

        private void LanguageToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripMenuItem languageMenu = (ToolStripMenuItem)sender;
            CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;
            if (!currentUICulture.IsNeutralCulture)
            {
                currentUICulture = currentUICulture.Parent;
            }
            string currentLanguage = currentUICulture.Name;
            foreach (ToolStripItem item in languageMenu.DropDownItems)
            {
                string languageName = item.Tag as string;
                if (String.IsNullOrEmpty(languageName))
                {
                    continue;
                }
                if (languageName == currentLanguage)
                {
                    ((ToolStripMenuItem)item).Checked = true;
                    break;
                }
            }
        }

        private void _updateToolStripButton_Click(object sender, EventArgs e)
        {
            Resolve.UserSettings.LastUpdateCheckUtc = OS.Current.UtcNow;
            Process.Start(Resolve.UserSettings.UpdateUrl.ToString());
        }

        private void SetOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (DebugOptionsDialog dialog = new DebugOptionsDialog())
            {
                dialog._legacyRestApiBaseUrl.Text = Resolve.UserSettings.LegacyRestApiBaseUrl.ToString();
                dialog._restApiBaseUrl.Text = Resolve.UserSettings.RestApiBaseUrl.ToString();
                dialog._timeoutTimeSpan.Text = Resolve.UserSettings.ApiTimeOut.ToString();
                DialogResult result = dialog.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return;
                }
                Resolve.UserSettings.LegacyRestApiBaseUrl = new Uri(dialog._legacyRestApiBaseUrl.Text);
                Resolve.UserSettings.RestApiBaseUrl = new Uri(dialog._restApiBaseUrl.Text);
                Resolve.UserSettings.ApiTimeOut = TimeSpan.Parse(dialog._timeoutTimeSpan.Text);
            }
        }

        private void _aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (AboutBox aboutBox = new AboutBox())
            {
                aboutBox.ShowDialog();
            }
        }

        private void HelpToolStripButton_Click(object sender, EventArgs e)
        {
            Process.Start(Resolve.UserSettings.AxCrypt2HelpUrl.ToString());
        }

        private void _viewHelpMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Resolve.UserSettings.AxCrypt2HelpUrl.ToString());
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeInternal();
            }
            base.Dispose(disposing);
        }

        private void DisposeInternal()
        {
            if (components != null)
            {
                components.Dispose();
            }
            if (_mainViewModel != null)
            {
                _mainViewModel.Dispose();
                _mainViewModel = null;
            }
        }

        private void ClearPassphraseMemoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearAllSettingsAndReinitialize();
            CloseAndExit();
        }

        private static void ClearAllSettingsAndReinitialize()
        {
            Resolve.UserSettings.Delete();
            Resolve.FileSystemState.Delete();
            Resolve.UserSettings.SettingsVersion = Resolve.UserSettings.CurrentSettingsVersion;
        }

        private void PolicyMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            SetCheckedToolStripMenuItem(item);
            TypeMap.Register.Singleton<ICryptoPolicy>(() => New<CryptoPolicy>().Create(item.Text));
        }

        private static void SetCheckedToolStripMenuItem(ToolStripMenuItem item)
        {
            foreach (ToolStripItem tsi in item.GetCurrentParent().Items)
            {
                ((ToolStripMenuItem)tsi).Checked = false;
            }
            item.Checked = true;
        }

        private void CryptoPolicyToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
        }

        private void loggingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            item.Checked = !item.Checked;
            if (_debugOutput == null)
            {
                _debugOutput = new DebugLogOutputDialog();
            }
            _debugOutput.Visible = item.Checked;
        }

        private void encryptedFoldersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowWatchedFolders(new string[0]);
        }

        private void ShowWatchedFolders(IEnumerable<string> additional)
        {
            using (WatchedFoldersDialog dialog = new WatchedFoldersDialog(this, additional))
            {
                dialog.ShowDialog();
            }
        }

        private void CreateAccountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (CreateNewAccountDialog dialog = new CreateNewAccountDialog(this, String.Empty, EmailAddress.Empty))
            {
                dialog.ShowDialog();
            }
        }

        private void ManageAccountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (ManageAccountDialog dialog = new ManageAccountDialog(Resolve.KnownIdentities, Resolve.UserSettings))
            {
                dialog.ShowDialog();
            }
        }

        private void ChangePassphraseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AccountStorage userKeyPairs = new AccountStorage(New<LogOnIdentity, IAccountService>(Resolve.KnownIdentities.DefaultEncryptionIdentity));
            ManageAccountViewModel viewModel = new ManageAccountViewModel(userKeyPairs, Resolve.KnownIdentities);

            string passphrase;
            using (NewPassphraseDialog dialog = new NewPassphraseDialog(this, Resources.ChangePassphraseDialogTitle, String.Empty, String.Empty))
            {
                dialog.ShowPassphraseCheckBox.Checked = Resolve.UserSettings.DisplayEncryptPassphrase;
                DialogResult dialogResult = dialog.ShowDialog(this);
                if (dialogResult != DialogResult.OK || dialog.PassphraseTextBox.Text.Length == 0)
                {
                    return;
                }
                Resolve.UserSettings.DisplayEncryptPassphrase = dialog.ShowPassphraseCheckBox.Checked;
                passphrase = dialog.PassphraseTextBox.Text;
            }
            viewModel.ChangePassphrase.Execute(passphrase);
        }

        private void ExportMySharingKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UserKeyPair keyPair = Resolve.KnownIdentities.DefaultEncryptionIdentity.UserKeys;
            EmailAddress userEmail = keyPair.UserEmail;
            IAsymmetricPublicKey publicKey = keyPair.KeyPair.PublicKey;
            string fileName;
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Export Public Sharing Key";
                sfd.DefaultExt = ".txt";
                sfd.AddExtension = true;
                sfd.Filter = "AxCrypt Public Sharing Key Files (*.txt)|*.txt|All Files (*.*)|*.*";
                sfd.CheckPathExists = true;
                sfd.FileName = "AxCrypt Sharing Key (#{1}) - {0}.txt".InvariantFormat(userEmail.Address, publicKey.Tag);
                sfd.ValidateNames = true;
                sfd.OverwritePrompt = true;
                sfd.RestoreDirectory = false;
                DialogResult saveAsResult = sfd.ShowDialog();
                if (saveAsResult != DialogResult.OK)
                {
                    return;
                }
                fileName = sfd.FileName;
            }

            UserPublicKey userPublicKey = new UserPublicKey(userEmail, publicKey);
            string serialized = Resolve.Serializer.Serialize(userPublicKey);
            File.WriteAllText(fileName, serialized);
        }

        private void ImportOthersSharingKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileSelectionViewModel fileSelection = new FileSelectionViewModel();
            fileSelection.SelectingFiles += (sfsender, sfe) => { HandleOpenFileSelection(sfe); };
            fileSelection.SelectFiles.Execute(FileSelectionType.ImportPublicKeys);

            ImportPublicKeysViewModel importPublicKeys = new ImportPublicKeysViewModel(New<KnownPublicKeys>);
            importPublicKeys.ImportFiles.Execute(fileSelection.SelectedFiles);
        }

        private void ImportMyPrivateKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (ImportPrivatePasswordDialog dialog = new ImportPrivatePasswordDialog(this, Resolve.UserSettings, Resolve.KnownIdentities))
            {
                dialog.ShowDialog();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "re")]
        private static void HandleRequestPrivateKeyPassword(LogOnEventArgs re)
        {
            throw new NotImplementedException();
        }

        private async void ShareKeys(IEnumerable<string> fileNames)
        {
            foreach (string file in fileNames)
            {
                EncryptedProperties encryptedProperties = await EncryptedPropertiesAsync(New<IDataStore>(file));
                IEnumerable<UserPublicKey> sharedWith = encryptedProperties.SharedKeyHolders;
                using (KeyShareDialog dialog = new KeyShareDialog(New<KnownPublicKeys>, sharedWith, Resolve.KnownIdentities.DefaultEncryptionIdentity))
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        continue;
                    }
                    sharedWith = dialog.SharedWith;
                }

                EncryptionParameters encryptionParameters = new EncryptionParameters(encryptedProperties.DecryptionParameter.CryptoId);
                encryptionParameters.Passphrase = Resolve.KnownIdentities.DefaultEncryptionIdentity.Passphrase;
                encryptionParameters.Add(New<KnownPublicKeys>().PublicKeys.Where(pk => sharedWith.Any(s => s.Email == pk.Email)));
                encryptionParameters.Add(Resolve.KnownIdentities.DefaultEncryptionIdentity.PublicKeys);

                Resolve.ProgressBackground.Work((Func<IProgressContext, FileOperationContext>)((IProgressContext progress) =>
                {
                    New<AxCryptFile>().ChangeEncryption(New<IDataStore>(file), Resolve.KnownIdentities.DefaultEncryptionIdentity, encryptionParameters, progress);
                    return new FileOperationContext(file, ErrorStatus.Success);
                }),
                (FileOperationContext foc) =>
                {
                    if (foc.ErrorStatus == ErrorStatus.Success)
                    {
                        Resolve.SessionNotify.Notify(new SessionNotification(SessionNotificationType.ActiveFileChange, foc.FullName));
                    }
                });
            }
        }

        private void ExportMyPrivateKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UserKeyPair userKeyPair = Resolve.KnownIdentities.DefaultEncryptionIdentity.UserKeys;
            IAsymmetricPublicKey publicKey = userKeyPair.KeyPair.PublicKey;

            string fileName;
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Export Account Secret and Sharing Key Pair";
                sfd.DefaultExt = ".axx";
                sfd.AddExtension = true;
                sfd.Filter = "AxCrypt Account Secret and Sharing Key Pair Files (*.axx)|*.axx|All Files (*.*)|*.*";
                sfd.CheckPathExists = true;
                sfd.FileName = "AxCrypt Account Key Pair (#{1}) - {0}.axx".InvariantFormat(userKeyPair.UserEmail, publicKey.Tag);
                sfd.ValidateNames = true;
                sfd.OverwritePrompt = true;
                sfd.RestoreDirectory = false;
                DialogResult saveAsResult = sfd.ShowDialog();
                if (saveAsResult != DialogResult.OK)
                {
                    return;
                }
                fileName = sfd.FileName;
            }

            byte[] export = userKeyPair.ToArray(Resolve.KnownIdentities.DefaultEncryptionIdentity.Passphrase);
            File.WriteAllBytes(fileName, export);
        }

        private void tryBrokenFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _mainViewModel.TryBrokenFile = !_mainViewModel.TryBrokenFile;
        }
    }
}