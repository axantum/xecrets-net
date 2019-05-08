#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
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
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Api;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
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
using Axantum.AxCrypt.Desktop;
using Axantum.AxCrypt.Forms;
using Axantum.AxCrypt.Forms.Implementation;
using Axantum.AxCrypt.Forms.Style;
using Axantum.AxCrypt.Mono;
using Axantum.AxCrypt.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Texts = AxCrypt.Content.Texts;

namespace Axantum.AxCrypt
{
    /// <summary>
    /// All code here is expected to execute on the GUI thread. If code may be called on another thread, this call
    /// must be made through ThreadSafeUi() .
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ax")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public partial class AxCryptMainForm : Form, ISignIn
    {
        private MainViewModel _mainViewModel;

        private FileOperationViewModel _fileOperationViewModel;

        private KnownFoldersViewModel _knownFoldersViewModel;

        public static MessageBoxOptions MessageBoxOptions { get; private set; }

        private DebugLogOutputDialog _debugOutput;

        private TabPage _hiddenWatchedFoldersTabPage;

        private CommandLine _commandLine;

        private bool _startMinimized;

        public AxCryptMainForm()
        {
            InitializeComponent();
            new Styling(Resources.axcrypticon).Style(this, _recentFilesContextMenuStrip, _watchedFoldersContextMenuStrip);
        }

        public AxCryptMainForm(CommandLine commandLine)
            : this()
        {
            if (commandLine == null)
            {
                throw new ArgumentNullException(nameof(commandLine));
            }

            _commandLine = commandLine;
            _startMinimized = commandLine.HasCommands;
        }

        private bool _isInitializing = true;

        private async void AxCryptMainForm_Load(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            try
            {
                await InitializeProgram();
            }
            catch (Exception ex)
            {
                await new ApplicationManager().ClearAllSettings();
                MessageBox.Show(ex.Message, "AxCrypt failed to start. All Settings cleared.", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Application.Exit();
            }
            finally
            {
                _isInitializing = false;
            }
        }

        private ApiVersion _apiVersion;

        private async Task InitializeProgram()
        {
            InitializeContentResources();
            RegisterTypeFactories();
            CheckLavasoftWebCompanionExistence();
            EnsureUiContextInitialized();
            EnsureFileAssociation();

            if (!await new ApplicationManager().ValidateSettings())
            {
                return;
            }

            SetupViewModelsAndNotificationsBeforeAnyNotificationsAreSent();
            CheckOfflineModeFirst();
            await GetApiVersionAsync();
            SetThisVersion();
            StartKeyPairService();
            AttachLogListener();
            ConfigureUiOptions();
            SetupPathFilters();
            IntializeControls();
            InitializeMouseDownFilter();
            RestoreUserPreferences();
            BindToViewModels();
            BindToFileOperationViewModel();
            WireUpEvents();
            SetupCommandService();
            await Resolve.SessionNotify.NotifyAsync(new SessionNotification(SessionNotificationType.SessionStart));
            StartupProcessMonitor();
            ExecuteCommandLine();
        }

        private static void EnsureFileAssociation()
        {
            if (New<InstallationVerifier>().IsApplicationInstalled && !New<InstallationVerifier>().IsFileAssociationOk)
            {
                Texts.FileAssociationBrokenWarning.ShowWarning(Texts.WarningTitle, DoNotShowAgainOptions.FileAssociationBrokenWarning);
            }
        }

        private static void CheckLavasoftWebCompanionExistence()
        {
            if (New<InstallationVerifier>().IsLavasoftApplicationInstalled)
            {
                Texts.LavasoftWebCompanionExistenceWarning.ShowWarning(Texts.WarningTitle, DoNotShowAgainOptions.LavasoftWebCompanionExistenceWarning);
            }
        }

        private void CheckOfflineModeFirst()
        {
            if (_commandLine.IsOfflineCommand)
            {
                New<UserSettings>().OfflineMode = true;
            }
        }

        private async Task GetApiVersionAsync()
        {
            try
            {
                _apiVersion = await New<ICache>().GetItemAsync(CacheKey.RootKey.Subkey("WrapMessageDialogsAsync_ApiVersion"), () => New<GlobalApiClient>().ApiVersionAsync(Environment.OSVersion.VersionString, New<AboutAssembly>().AssemblyVersion));
            }
            catch (ApiException aex)
            {
                await aex.HandleApiExceptionAsync();
                _apiVersion = ApiVersion.Zero;
            }
        }

        private static void SetThisVersion()
        {
            New<UserSettings>().ThisVersion = New<IVersion>().Current.ToString();
        }

        private static void EnsureUiContextInitialized()
        {
            New<IUIThread>().Yield();
        }

        private void InitializeContentResources()
        {
            SetCulture();

            _addSecureFolderToolStripMenuItem.Text = "&" + Texts.AddSecureFolderMenuItemText;
            _alwaysOfflineToolStripMenuItem.Text = "&" + Texts.AlwaysOffline;
            _alwaysOfflineToolStripMenuItem.ToolTipText = Texts.AlwaysOfflineToolTip;
            _checkForUpdateToolStripMenuItem.Text = "&" + Texts.CheckForUpdateMenuText;
            _checkForUpdateToolStripMenuItem.ToolTipText = Texts.CheckForUpdateMenuToolTip;
            _cleanDecryptedToolStripMenuItem.Text = "&" + Texts.CleanDecryptedToolStripMenuItemText;
            _closeAndRemoveOpenFilesToolStripButton.ToolTipText = Texts.CloseAndRemoveOpenFilesToolStripButtonToolTipText;
            _createAccountToolStripMenuItem.Text = "&" + Texts.CreateAccountToolStripMenuItemText;
            _createAccountToolStripMenuItem.ToolTipText = Texts.CreateAccountToolStripMenuItemToolTipText;
            _cryptoName.Text = Texts.CryptoNameText;
            _debugCheckVersionNowToolStripMenuItem.Text = "&" + Texts.DebugCheckVersionNowToolStripMenuItemText;
            _debugCryptoPolicyToolStripMenuItem.Text = "&" + Texts.DebugCryptoPolicyToolStripMenuItemText;
            _debugLoggingToolStripMenuItem.Text = "&" + Texts.DebugLoggingToolStripMenuItemText;
            _debugManageAccountToolStripMenuItem.Text = "&" + Texts.DebugManageAccountToolStripMenuItemText;
            _debugOpenReportToolStripMenuItem.Text = "&" + Texts.ReportSnapshotOpenMenuItem;
            _debugOptionsToolStripMenuItem.Text = "&" + Texts.DebugOptionsToolStripMenuItemText;
            _debugToolStripMenuItem.Text = "&" + Texts.DebugToolStripMenuItemText;
            _VerifyFileToolStripMenuItem.Text = "&" + Texts.VerifyFileToolStripMenuItem;
            _axcryptFileFormatCheckToolStripMenuItem.Text = "&" + Texts.DebugFileFormatCheckToolStripMenuItemText;
            _decryptAndRemoveFromListToolStripMenuItem.Text = "&" + Texts.DecryptAndRemoveFromListToolStripMenuItemText;
            _decryptedFileColumnHeader.Text = Texts.DecryptedFileColumnHeaderText;
            _decryptToolStripMenuItem.Text = "&" + Texts.DecryptToolStripMenuItemText;
            _dutchToolStripMenuItem.Text = "&" + Texts.DutchLanguageSelection;
            _encryptedFoldersToolStripMenuItem.Text = "&" + Texts.EncryptedFoldersToolStripMenuItemText;
            _encryptedPathColumnHeader.Text = Texts.EncryptedPathColumnHeaderText;
            _encryptToolStripButton.ToolTipText = Texts.EncryptToolStripButtonToolTipText;
            _encryptToolStripMenuItem.Text = "&" + Texts.EncryptToolStripMenuItemText;
            _englishLanguageToolStripMenuItem.Text = "&" + Texts.EnglishLanguageToolStripMenuItemText;
            _exitToolStripMenuItem.Text = "&" + Texts.ExitToolStripMenuItemText;
            _exportMyPrivateKeyToolStripMenuItem.Text = "&" + Texts.ExportMyPrivateKeyToolStripMenuItemText;
            _exportMyPrivateKeyToolStripMenuItem.ToolTipText = Texts.ExportMyPrivateKeyToolStripMenuItemToolTipText;
            _exportSharingKeyToolStripMenuItem.Text = "&" + Texts.ExportSharingKeyToolStripMenuItemText;
            _exportSharingKeyToolStripMenuItem.ToolTipText = Texts.ExportSharingKeyToolStripMenuItemToolTipText;
            _feedbackButton.ToolTipText = Texts.FeedbackButtonText;
            _fileToolStripMenuItem.Text = "&" + Texts.FileToolStripMenuItemText;
            _francaisLanguageToolStripMenuItem.Text = "&" + Texts.FrancaisLanguageToolStripMenuItemText;
            _germanLanguageToolStripMenuItem.Text = "&" + Texts.GermanLanguageSelectionText;
            _helpAboutToolStripMenuItem.Text = "&" + Texts.HelpAboutToolStripMenuItemText;
            _helpToolStripMenuItem.Text = "&" + Texts.HelpToolStripMenuItemText;
            _helpViewHelpMenuItem.Text = "&" + Texts.HelpViewHelpMenuItemText;
            _importMyPrivateKeyToolStripMenuItem.Text = "&" + Texts.ImportMyPrivateKeyToolStripMenuItemText;
            _importMyPrivateKeyToolStripMenuItem.ToolTipText = Texts.ImportMyPrivateKeyToolStripMenuItemToolTipText;
            _importOthersSharingKeyToolStripMenuItem.Text = "&" + Texts.ImportOthersSharingKeyToolStripMenuItemText;
            _importOthersSharingKeyToolStripMenuItem.ToolTipText = Texts.ImportOthersSharingKeyToolStripMenuItemToolTipText;
            _inviteUserToolStripMenuItem.Text = Texts.InviteUserToolStripMenuItemText;
            _italianLanguageToolStripMenuItem.Text = "&" + Texts.ItalianLanguageSelection;
            _keyManagementToolStripMenuItem.Text = "&" + Texts.KeyManagementToolStripMenuItemText;
            _keyShareToolStripButton.ToolTipText = Texts.KeySharingToolTip;
            _koreanLanguageToolStripMenuItem.Text = "&" + Texts.KoreanLanguageSelection;
            _lastAccessTimeColumnHeader.Text = Texts.LastAccessTimeColumnHeaderText;
            _notifyIcon.Text = Texts.AxCryptFileEncryption;
            _notifySignInToolStripMenuItem.Text = "&" + Texts.LogOnText;
            _notifySignOutToolStripMenuItem.Text = "&" + Texts.LogOffText;
            _notifyExitToolStripMenuItem.Text = "&" + Texts.ExitToolStripMenuItemText;
            _notifyAdvancedToolStripMenuItem.Text = "&" + Texts.McInfMenuShow;
            _openEncryptedToolStripButton.ToolTipText = Texts.OpenToolStripButtonToolTipText;
            _openEncryptedToolStripMenuItem.Text = "&" + Texts.OpenEncryptedToolStripMenuItemText;
            _optionsEncryptionUpgradeModeToolStripMenuItem.Text = "&" + Texts.OptionsConvertMenuItemText;
            _optionsEncryptionUpgradeModeToolStripMenuItem.ToolTipText = Texts.OptionsConvertMenuToolTip;
            _optionsChangePassphraseToolStripMenuItem.Text = "&" + Texts.OptionsChangePassphraseToolStripMenuItemText;
            _optionsClearAllSettingsAndRestartToolStripMenuItem.Text = "&" + Texts.OptionsClearAllSettingsAndExitToolStripMenuItemText;
            _optionsDebugToolStripMenuItem.Text = "&" + Texts.OptionsDebugToolStripMenuItemText;
            _optionsLanguageToolStripMenuItem.Text = "&" + Texts.OptionsLanguageToolStripMenuItemText;
            _optionsIncludeSubfoldersToolStripMenuItem.Text = "&" + Texts.OptionsIncludeSubfoldersToolStripMenuItemText;
            _optionsToolStripMenuItem.Text = "&" + Texts.OptionsToolStripMenuItemText;
            _passwordResetToolStripMenuItem.Text = "&" + Texts.ButtonPasswordResetText;
            _passwordResetToolStripMenuItem.ToolTipText = Texts.ButtonPasswordResetToolTip;
            _polishLanguageToolStripMenuItem.Text = "&" + Texts.PolishLanguageToolStripMenuItemText;
            _portugueseBrazilToolStripMenuItem.Text = "&" + Texts.PortugueseBrazilLanguageSelection;
            _progressContextCancelToolStripMenuItem.Text = "&" + Texts.ButtonCancelText;
            _recentFilesOpenToolStripMenuItem.Text = "&" + Texts.RecentFilesOpenToolStripMenuItemText;
            _recentFilesRestoreAnonymousNamesMenuItem.Text = "&" + Texts.RestoreAnonymousNamesMenuText;
            _clearRecentFilesToolStripMenuItem.Text = "&" + Texts.ClearRecentFilesToolStripMenuItemText;
            _recentFilesTabPage.Text = Texts.RecentFilesTabPageText;
            _recentFilesShowInFolderToolStripMenuItem.Text = "&" + Texts.ShowInFolderText;
            _removeRecentFileToolStripMenuItem.Text = "&" + Texts.RemoveRecentFileToolStripMenuItemText;
            _renameToolStripMenuItem.Text = "&" + Texts.AnonymousRenameMenuText;
            _renameToolStripMenuItem.ToolTipText = Texts.AnonymousRenameToolTip;
            _restoreAnonymousNamesToolStripMenuItem.Text = "&" + Texts.RestoreAnonymousNamesMenuText;
            _restoreAnonymousNamesToolStripMenuItem.ToolTipText = Texts.RestoreAnonymousNamesToolTip;
            _russianLanguageToolStripMenuItem.Text = "&" + Texts.RussianLanguageSelection;
            _secretsToolStripButton.ToolTipText = Texts.SecretsButtonToolTipText;
            _secureDeleteToolStripMenuItem.Text = "&" + Texts.SecureDeleteToolStripMenuItemText;
            _shareKeysToolStripMenuItem.Text = "&" + Texts.ShareKeysToolStripMenuItemText;
            _signInToolStripMenuItem.Text = "&" + Texts.LogOnText;
            _signOutToolStripMenuItem.Text = "&" + Texts.LogOffText;
            _spanishLanguageToolStripMenuItem.Text = "&" + Texts.SpanishLanguageToolStripMenuItemText;
            _swedishLanguageToolStripMenuItem.Text = "&" + Texts.SwedishLanguageToolStripMenuItemText;
            _turkishLanguageToolStripMenuItem.Text = "&" + Texts.TurkishLanguageToolStripMenuItemText;
            _tryBrokenFileToolStripMenuItem.Text = "&" + Texts.TryBrokenFileToolStripMenuItemText;
            _encryptionUpgradeMenuItem.Text = "&" + Texts.UpgradeLegacyFilesMenuItemText;
            _encryptionUpgradeMenuItem.ToolTipText = Texts.UpgradeLegacyFilesMenuToolTip;
            _watchedFolderColumnHeader.Text = Texts.WatchedFolderColumnHeaderText;
            _watchedFoldersAddSecureFolderMenuItem.Text = "&" + Texts.AddSecureFolderMenuItemText;
            _watchedFoldersdecryptTemporarilyMenuItem.Text = "&" + Texts.MenuDecryptTemporarilyText;
            _watchedFoldersKeySharingMenuItem.Text = "&" + Texts.ShareKeysToolStripMenuItemText;
            _watchedFoldersOpenExplorerHereMenuItem.Text = "&" + Texts.WatchedFoldersOpenExplorerHereMenuItemText;
            _watchedFoldersRemoveMenuItem.Text = "&" + Texts.WatchedFoldersRemoveMenuItemText;
            _watchedFoldersTabPage.Text = Texts.WatchedFoldersTabPageText;
            _inactivitySignOutToolStripMenuItem.Text = Texts.IdleSignOutToolStripMenuItemText;
            _disableInactivitySignOutToolStripMenuItem.Text = Texts.DisableIdleSignOutToolStripMenuItemText;
            _fiveMinuteInactivitySignOutToolStripMenuItem.Text = Texts.IdleMinutesSignOutToolStripMenuItemText.InvariantFormat(5);
            _fifteenMinuteInactivitySignOutToolStripMenuItem.Text = Texts.IdleMinutesSignOutToolStripMenuItemText.InvariantFormat(15);
            _thirtyMinuteInactivitySignOutToolStripMenuItem.Text = Texts.IdleMinutesSignOutToolStripMenuItemText.InvariantFormat(30);
            _sixtyMinuteInactivitySignOutToolStripMenuItem.Text = Texts.IdleMinutesSignOutToolStripMenuItemText.InvariantFormat(60);
            _getPremiumToolStripMenuItem.Text = Texts.UpgradePromptText;
        }

        private static void SetCulture()
        {
            if (String.IsNullOrEmpty(Resolve.UserSettings.CultureName))
            {
                return;
            }
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(Resolve.UserSettings.CultureName);
        }

        private static void StartKeyPairService()
        {
            if (!String.IsNullOrEmpty(Resolve.UserSettings.UserEmail))
            {
                return;
            }
            New<KeyPairService>().Start();
        }

        private async void AxCryptMainForm_ShownAsync(object sender, EventArgs e)
        {
            New<IRuntimeEnvironment>().FirstInstanceIsReady();
            while (_isInitializing)
            {
                Application.DoEvents();
            }

            if (_startMinimized || _commandLine.IsStartCommand)
            {
                ShowNotifyIcon();
                return;
            }

            if (New<UserSettings>().RestoreFullWindow || !_commandLine.HasCommands)
            {
                Styling.RestoreWindowWithFocus(this);
            }
            await SignInAsync();
        }

        public bool IsSigningIn { get; set; }

        public async Task SignIn()
        {
            await _fileOperationViewModel.IdentityViewModel.LogOnAsync.ExecuteAsync(null);
        }

        private async Task SignInAsync()
        {
            SignUpSignIn signUpSignIn = new SignUpSignIn()
            {
                Version = _apiVersion,
                UserEmail = New<UserSettings>().UserEmail,
            };

            await signUpSignIn.DialogsAsync(this, this);

            New<UserSettings>().UserEmail = signUpSignIn.UserEmail;

            if (signUpSignIn.StopAndExit)
            {
                await new ApplicationManager().StopAndExit();
                return;
            }

            await SetSignInSignOutStatusAsync(_mainViewModel.LoggedOn);
        }

        private static void StartupProcessMonitor()
        {
            TypeMap.Register.Singleton(() => new ProcessMonitor());
            New<ProcessMonitor>();
        }

        private void ExecuteCommandLine()
        {
            if (!_commandLine.CommandItems.Any() || _commandLine.IsOfflineCommand || _commandLine.IsStartCommand)
            {
                return;
            }

            Task.Run(() =>
            {
                _commandLine.Execute();
                new ExplorerRefresh().Notify();
            });
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
                Resolve.UIThread.PostTo(() =>
                {
                    if (_debugOutput == null || !_debugOutput.Visible)
                    {
                        return;
                    }
                    string formatted = "{0} {1}".InvariantFormat(New<INow>().Utc.ToString("o", CultureInfo.InvariantCulture), loggingEventArgs.Message.TrimLogMessage());
                    _debugOutput.AppendText(formatted);
                });
            };
        }

        private void SetupViewModelsAndNotificationsBeforeAnyNotificationsAreSent()
        {
            New<LicensePolicy>();
            _mainViewModel = New<MainViewModel>();
            _fileOperationViewModel = New<FileOperationViewModel>();
            _knownFoldersViewModel = New<KnownFoldersViewModel>();
            New<SessionNotify>().AddCommand(async (notification) => await New<SessionNotificationHandler>().HandleNotificationAsync(notification));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "It's not actually complex since it's just a registry.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "It's not actually complex since it's just a registry.")]
        private void RegisterTypeFactories()
        {
            TypeMap.Register.Singleton<IUIThread>(() => new UIThread(this));
            TypeMap.Register.Singleton<IProgressBackground>(() => _progressBackgroundWorker);
            TypeMap.Register.Singleton<IStatusChecker>(() => new StatusChecker());
            TypeMap.Register.Singleton<IDataItemSelection>(() => new FileFolderSelection(this));
            TypeMap.Register.Singleton<IDeviceLocked>(() => new DeviceLocked());
            TypeMap.Register.Singleton<IInternetState>(() => new InternetState());
            TypeMap.Register.Singleton<InstallationVerifier>(() => new InstallationVerifier());
            TypeMap.Register.Singleton<IKnownFolderImageProvider>(() => new KnownFolderImageProvider());
            TypeMap.Register.Singleton<InactivitySignOut>(() => new InactivitySignOut(New<UserSettings>().InactivitySignOutTime));
            TypeMap.Register.Singleton<MouseDownFilter>(() => new MouseDownFilter(this));
            TypeMap.Register.Singleton<IGlobalNotification>(() => new NotifyIconGlobalNotification(_notifyIcon));

            TypeMap.Register.New<SessionNotificationHandler>(() => new SessionNotificationHandler(Resolve.FileSystemState, Resolve.KnownIdentities, New<ActiveFileAction>(), New<AxCryptFile>(), New<IStatusChecker>()));
            TypeMap.Register.New<IdentityViewModel>(() => new IdentityViewModel(Resolve.FileSystemState, Resolve.KnownIdentities, Resolve.UserSettings, Resolve.SessionNotify));
            TypeMap.Register.New<FileOperationViewModel>(() => new FileOperationViewModel(Resolve.FileSystemState, Resolve.SessionNotify, Resolve.KnownIdentities, Resolve.ParallelFileOperation, New<IStatusChecker>(), New<IdentityViewModel>()));
            TypeMap.Register.New<MainViewModel>(() => new MainViewModel(Resolve.FileSystemState, Resolve.UserSettings));
            TypeMap.Register.New<KnownFoldersViewModel>(() => new KnownFoldersViewModel(Resolve.FileSystemState, Resolve.SessionNotify, Resolve.KnownIdentities));
            TypeMap.Register.New<WatchedFoldersViewModel>(() => new WatchedFoldersViewModel(Resolve.FileSystemState));

            TypeMap.Register.Singleton<AboutBox>(() => new AboutBox());

            FormsTypes.Register(this);
        }

        private static void SetupPathFilters()
        {
            if (OS.Current.Platform != Platform.WindowsDesktop)
            {
                return;
            }

            New<FileFilter>().AddUnencryptable(new Regex(@"\\\.dropbox$"));
            New<FileFilter>().AddUnencryptable(new Regex(@"\\desktop\.ini$"));
            New<FileFilter>().AddUnencryptable(new Regex(@".*\.tmp$"));
            New<FileFilter>().AddUnencryptable(new Regex(@"^.*\\~\$[^\\]*$"));

            AddEnvironmentVariableBasedFilePathFilter(@"^{0}(?!Temp$)", "SystemRoot");
            AddEnvironmentVariableBasedFilePathFilter(@"^{0}(?!Temp$)", "windir");
            AddEnvironmentVariableBasedFilePathFilter(@"^{0}", "ProgramFiles");
            AddEnvironmentVariableBasedFilePathFilter(@"^{0}", "ProgramFiles(x86)");
            AddEnvironmentVariableBasedFilePathFilter(@"^{0}$", "SystemDrive");

            New<FileFilter>().AddPlatformIndependent();

            AddEnvironmentVariableBasedFolderPathFilter("ProgramData");
            AddEnvironmentVariableBasedFolderPathFilter("ProgramFiles(x86)");
            AddEnvironmentVariableBasedFolderPathFilter("ProgramFiles");
            AddEnvironmentVariableBasedFolderPathFilter("SystemRoot");
            AddEnvironmentVariableBasedFolderPathFilter("APPDATA");
            AddEnvironmentVariableBasedFolderPathFilter("LOCALAPPDATA");
            AddEnvironmentVariableBasedFolderPathFilter("windir");
            AddEnvironmentVariableBasedFolderPathFilter("ProgramW6432");
        }

        private static void AddEnvironmentVariableBasedFilePathFilter(string formatRegularExpression, string name)
        {
            IDataContainer folder = name.FolderFromEnvironment();
            if (folder == null)
            {
                return;
            }
            string escapedPath = folder.FullName.Replace(@"\", @"\\");
            New<FileFilter>().AddUnencryptable(new Regex(formatRegularExpression.InvariantFormat(escapedPath)));
        }

        private static void AddEnvironmentVariableBasedFolderPathFilter(string name)
        {
            IDataContainer folder = name.FolderFromEnvironment();
            if (folder == null)
            {
                return;
            }
            New<FileFilter>().AddForbiddenFolderFilters(folder.FullName);
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

            _encryptToolStripButton.Tag = FileInfoTypes.EncryptableFile;

            _hiddenWatchedFoldersTabPage = _statusTabControl.TabPages["_watchedFoldersTabPage"];

            _cleanDecryptedToolStripMenuItem.Click += CloseAndRemoveOpenFilesToolStripButton_Click;
            _closeAndRemoveOpenFilesToolStripButton.Click += CloseAndRemoveOpenFilesToolStripButton_Click;
            _feedbackButton.Click += (sender, e) => Process.Start(Texts.LinkToFeedbackWebPage);
            _optionsChangePassphraseToolStripMenuItem.Click += ChangePassphraseToolStripMenuItem_Click;
            _signInToolStripMenuItem.Click += async (sender, e) => await LogOnOrLogOffAndLogOnAgainAsync();
            _notifySignOutToolStripMenuItem.Click += async (sender, e) => await _fileOperationViewModel.IdentityViewModel.LogOnLogOff.ExecuteAsync(null);
            _notifySignInToolStripMenuItem.Click += async (sender, e) => await LogOnOrLogOffAndLogOnAgainAsync();
            _signOutToolStripMenuItem.Click += async (sender, e) => await LogOnOrLogOffAndLogOnAgainAsync();
            _alwaysOfflineToolStripMenuItem.Click += (sender, e) =>
            {
                bool offlineMode = !New<UserSettings>().OfflineMode;
                _alwaysOfflineToolStripMenuItem.Checked = offlineMode;
                New<UserSettings>().OfflineMode = offlineMode;
                New<AxCryptOnlineState>().IsOffline = offlineMode;
            };
            _softwareStatusButton.Click += _softwareStatusButton_Click;
#if DEBUG
            _debugCryptoPolicyToolStripMenuItem.Visible = true;
#endif
        }

        private async Task ConfigureMenusAccordingToPolicyAsync(LicenseCapabilities license)
        {
            await ConfigurePolicyMenuAsync(license);
            await ConfigureSecureWipeAsync(license);
            await ConfigureKeyShareMenusAsync(license);
            await ConfigureSecretsMenusAsync(license);
            await ConfigureAnonymousRenameAsync(license);
            await ConfigureIncludeSubfoldersMenuAsync(license);
            await ConfigureInactivityTimeOutMenuAsync(license);
        }

        private async Task ConfigureKeyShareMenusAsync(LicenseCapabilities license)
        {
            if (license.Has(LicenseCapability.KeySharing))
            {
                _keyShareToolStripButton.Image = Resources.share_border_80px;
                _keyShareToolStripButton.ToolTipText = Texts.KeySharingToolTip;
            }
            else
            {
                _keyShareToolStripButton.Image = Resources.share_border_grey_premium_80px;
                _keyShareToolStripButton.ToolTipText = Texts.PremiumNeededForKeyShare;
            }
        }

        private async Task ConfigureSecretsMenusAsync(LicenseCapabilities license)
        {
            if (license.Has(LicenseCapability.PasswordManagement))
            {
                _secretsToolStripButton.Image = Resources.passwords_80px;
                _secretsToolStripButton.ToolTipText = Texts.SecretsButtonToolTipText;
            }
            else
            {
                _secretsToolStripButton.Image = Resources.passwords_grey_premium_80px;
                _secretsToolStripButton.ToolTipText = Texts.ToolTipPremiumNeededForSecrets;
            }
        }

        private async Task ConfigureSecureWipeAsync(LicenseCapabilities license)
        {
            if (license.Has(LicenseCapability.SecureWipe))
            {
                _secureDeleteToolStripMenuItem.Image = Resources.delete;
                _secureDeleteToolStripMenuItem.ToolTipText = String.Empty;
            }
            else
            {
                _secureDeleteToolStripMenuItem.Image = Resources.premium_32px;
                _secureDeleteToolStripMenuItem.ToolTipText = Texts.PremiumFeatureToolTipText;
            }
        }

        private async Task ConfigureAnonymousRenameAsync(LicenseCapabilities license)
        {
            if (license.Has(LicenseCapability.RandomRename))
            {
                _renameToolStripMenuItem.Image = null;
                _renameToolStripMenuItem.ToolTipText = Texts.AnonymousRenameToolTip;
                _restoreAnonymousNamesToolStripMenuItem.Image = null;
                _restoreAnonymousNamesToolStripMenuItem.ToolTipText = Texts.RestoreAnonymousNamesToolTip;
            }
            else
            {
                _renameToolStripMenuItem.Image = Resources.premium_32px;
                _renameToolStripMenuItem.ToolTipText = Texts.PremiumFeatureToolTipText;
                _restoreAnonymousNamesToolStripMenuItem.Image = Resources.premium_32px;
                _restoreAnonymousNamesToolStripMenuItem.ToolTipText = Texts.PremiumFeatureToolTipText;
            }
        }

        private async Task ConfigurePolicyMenuAsync(LicenseCapabilities license)
        {
            ToolStripMenuItem item;
            _debugCryptoPolicyToolStripMenuItem.DropDownItems.Clear();

            item = new ToolStripMenuItem();
            item.Text = Texts.LicensePremiumNameText;
            item.Checked = license.Has(LicenseCapability.Premium);
            item.Click += PolicyMenuItem_Click;
            _debugCryptoPolicyToolStripMenuItem.DropDownItems.Add(item);

            item = new ToolStripMenuItem();
            item.Text = Texts.LicenseFreeNameText;
            item.Checked = !license.Has(LicenseCapability.Premium);
            item.Click += PolicyMenuItem_Click;
            _debugCryptoPolicyToolStripMenuItem.DropDownItems.Add(item);
        }

        private async Task ConfigureIncludeSubfoldersMenuAsync(LicenseCapabilities license)
        {
            if (license.Has(LicenseCapability.IncludeSubfolders))
            {
                _optionsIncludeSubfoldersToolStripMenuItem.Image = null;
                _optionsIncludeSubfoldersToolStripMenuItem.ToolTipText = String.Empty;
            }
            else
            {
                _optionsIncludeSubfoldersToolStripMenuItem.Image = Resources.premium_32px;
                _optionsIncludeSubfoldersToolStripMenuItem.ToolTipText = Texts.PremiumFeatureToolTipText;
            }
        }

        private async Task ConfigureInactivityTimeOutMenuAsync(LicenseCapabilities license)
        {
            if (license.Has(LicenseCapability.InactivitySignOut))
            {
                _inactivitySignOutToolStripMenuItem.Image = null;
                _inactivitySignOutToolStripMenuItem.ToolTipText = String.Empty;
            }
            else
            {
                _inactivitySignOutToolStripMenuItem.Image = Resources.premium_32px;
                _inactivitySignOutToolStripMenuItem.ToolTipText = Texts.PremiumFeatureToolTipText;
            }
        }

        private void InactivitySignOutToolStripMenuItem_Opening(object sender, CancelEventArgs e)
        {
            if (!New<LicensePolicy>().Capabilities.Has(LicenseCapability.InactivitySignOut))
            {
                e.Cancel = true;
            }
        }

        private bool _balloonTipShown = false;

        private void InitializeNotifyIcon()
        {
            _notifyIcon.Icon = Resources.axcrypticon;
            _notifyIcon.Visible = false;

            _notifyIcon.DoubleClick += (object sender, EventArgs e) =>
            {
                Styling.RestoreWindowWithFocus(this);
                New<UserSettings>().RestoreFullWindow = true;
            };

            _notifyAdvancedToolStripMenuItem.Click += (sender, e) =>
            {
                Styling.RestoreWindowWithFocus(this);
                New<UserSettings>().RestoreFullWindow = true;
            };

            _notifyIcon.MouseClick += (sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                    mi.Invoke(sender, null);
                }
            };

            Resize += (sender, e) =>
            {
                switch (WindowState)
                {
                    case FormWindowState.Minimized:
                        ShowNotifyIcon();
                        New<UserSettings>().RestoreFullWindow = false;
                        break;

                    case FormWindowState.Normal:
                        _notifyIcon.Visible = false;
                        break;
                }
            };
        }

        private void ShowNotifyIcon()
        {
            _notifyIcon.Visible = true;

            if (!_balloonTipShown)
            {
                _notifyIcon.BalloonTipTitle = Texts.AxCryptFileEncryption;
                _notifyIcon.BalloonTipText = Texts.TrayBalloonTooltip;
                _notifyIcon.ShowBalloonTip(500);

                _balloonTipShown = true;
            }

            Hide();
        }

        private void RestoreUserPreferences()
        {
            Height = Preferences.MainWindowHeight.Fallback(Height);
            Width = Preferences.MainWindowWidth.Fallback(Width);
            Location = Preferences.MainWindowLocation.Fallback(Location).Safe();

            _mainViewModel.RecentFilesComparer = GetComparer(Preferences.RecentFilesSortColumn, !Preferences.RecentFilesAscending);
            _alwaysOfflineToolStripMenuItem.Checked = New<UserSettings>().OfflineMode;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void BindToViewModels()
        {
            _mainViewModel.BindPropertyChanged(nameof(_mainViewModel.DebugMode), (bool enabled) => { UpdateDebugMode(enabled); });
            _mainViewModel.BindPropertyChanged(nameof(_mainViewModel.DecryptFileEnabled), (bool enabled) => { _decryptToolStripMenuItem.Enabled = enabled; });
            _mainViewModel.BindPropertyChanged(nameof(_mainViewModel.DownloadVersion), async (DownloadVersion dv) => { await SetSoftwareStatus(); await DisplayUpdateCheckPopups(); });
            _mainViewModel.BindPropertyChanged(nameof(_mainViewModel.EncryptFileEnabled), (bool enabled) => { _encryptToolStripButton.Enabled = enabled; });
            _mainViewModel.BindPropertyChanged(nameof(_mainViewModel.EncryptFileEnabled), (bool enabled) => { _encryptToolStripMenuItem.Enabled = enabled; });
            _mainViewModel.BindPropertyChanged(nameof(_mainViewModel.FilesArePending), (bool filesArePending) => { _cleanDecryptedToolStripMenuItem.Enabled = filesArePending; });
            _mainViewModel.BindPropertyChanged(nameof(_mainViewModel.FilesArePending), (bool filesArePending) => { _closeAndRemoveOpenFilesToolStripButton.Enabled = filesArePending; _closeAndRemoveOpenFilesToolStripButton.ToolTipText = filesArePending ? Texts.CloseAndRemoveOpenFilesToolStripButtonToolTipText : string.Empty; });
            _mainViewModel.BindPropertyChanged(nameof(_mainViewModel.EncryptionUpgradeMode), (EncryptionUpgradeMode mode) => _optionsEncryptionUpgradeModeToolStripMenuItem.Checked = mode == EncryptionUpgradeMode.AutoUpgrade);
            _mainViewModel.BindPropertyChanged(nameof(_mainViewModel.License), async (LicenseCapabilities license) => await _knownFoldersViewModel.UpdateState.ExecuteAsync(null));
            _mainViewModel.BindPropertyAsyncChanged(nameof(_mainViewModel.License), async (LicenseCapabilities license) => { await ConfigureMenusAccordingToPolicyAsync(license); });
            _mainViewModel.BindPropertyAsyncChanged(nameof(_mainViewModel.License), async (LicenseCapabilities license) => { await _daysLeftPremiumLabel.ConfigureAsync(New<KnownIdentities>().DefaultEncryptionIdentity); });
            _mainViewModel.BindPropertyAsyncChanged(nameof(_mainViewModel.License), async (LicenseCapabilities license) => { await SetWindowTitleTextAsync(_mainViewModel.LoggedOn); });
            _mainViewModel.BindPropertyChanged(nameof(_mainViewModel.License), (LicenseCapabilities license) => { _recentFilesListView.UpdateRecentFiles(_mainViewModel.RecentFiles); });
            _mainViewModel.BindPropertyAsyncChanged(nameof(_mainViewModel.LoggedOn), async (bool loggedOn) => { await _daysLeftPremiumLabel.ConfigureAsync(New<KnownIdentities>().DefaultEncryptionIdentity); });
            _mainViewModel.BindPropertyAsyncChanged(nameof(_mainViewModel.LoggedOn), async (bool loggedOn) => { if (loggedOn) New<InactivitySignOut>().RestartInactivityTimer(); });
            _mainViewModel.BindPropertyChanged(nameof(_mainViewModel.LoggedOn), async (bool loggedOn) => { await SetSignInSignOutStatusAsync(loggedOn); });
            _mainViewModel.BindPropertyChanged(nameof(_mainViewModel.LoggedOn), async (bool loggedOn) => { await new Display().LocalSignInWarningPopUpAsync(loggedOn); });
            _mainViewModel.BindPropertyChanged(nameof(_mainViewModel.OpenEncryptedEnabled), (bool enabled) => { _openEncryptedToolStripMenuItem.Enabled = enabled; });
            _mainViewModel.BindPropertyChanged(nameof(_mainViewModel.RandomRenameEnabled), (bool enabled) => { _renameToolStripMenuItem.Enabled = enabled; });
            _mainViewModel.BindPropertyChanged(nameof(_mainViewModel.RecentFiles), (IEnumerable<ActiveFile> files) => { _recentFilesListView.UpdateRecentFiles(files); });
            _mainViewModel.BindPropertyChanged(nameof(_mainViewModel.WatchedFolders), (IEnumerable<string> folders) => { UpdateWatchedFolders(folders); });
            _mainViewModel.BindPropertyChanged(nameof(_mainViewModel.WatchedFoldersEnabled), (bool enabled) => { ConfigureWatchedFoldersMenus(enabled); });
            _mainViewModel.BindPropertyChanged(nameof(_mainViewModel.FolderOperationMode), (FolderOperationMode SecureFolderLevel) => { _optionsIncludeSubfoldersToolStripMenuItem.Checked = SecureFolderLevel == FolderOperationMode.IncludeSubfolders ? true : false; });
            _checkForUpdateToolStripMenuItem.Click += async (sender, e) => { _userInitiatedUpdateCheckPending = true; await _mainViewModel.AxCryptUpdateCheck.ExecuteAsync(DateTime.MinValue); };
            _debugCheckVersionNowToolStripMenuItem.Click += async (sender, e) => { _userInitiatedUpdateCheckPending = true; await _mainViewModel.AxCryptUpdateCheck.ExecuteAsync(DateTime.MinValue); };
            _debugOpenReportToolStripMenuItem.Click += (sender, e) => { New<IReport>().Open(); };
            _knownFoldersViewModel.BindPropertyChanged(nameof(_knownFoldersViewModel.KnownFolders), (IEnumerable<KnownFolder> folders) => UpdateKnownFolders(folders));
            _knownFoldersViewModel.KnownFolders = New<IKnownFoldersDiscovery>().Discover();
            _mainToolStrip.DragOver += async (sender, e) => { _mainViewModel.DragAndDropFiles = e.GetDragged(); e.Effect = await GetEffectsForMainToolStripAsync(e); };
            _optionsEncryptionUpgradeModeToolStripMenuItem.Click += (sender, e) => ToggleEncryptionUpgradeMode();
            _optionsClearAllSettingsAndRestartToolStripMenuItem.Click += async (sender, e) => { if (!await WarnIfAnyDecryptedFiles()) { await new ApplicationManager().ClearAllSettings(); await ShutDownAnd(New<IUIThread>().RestartApplication); } };
            _optionsDebugToolStripMenuItem.Click += (sender, e) => { _mainViewModel.DebugMode = !_mainViewModel.DebugMode; };
            _optionsIncludeSubfoldersToolStripMenuItem.Click += async (sender, e) => { await PremiumFeature_ClickAsync(LicenseCapability.IncludeSubfolders, (ss, ee) => { return ToggleIncludeSubfoldersOption(); }, sender, e); };
            _inactivitySignOutToolStripMenuItem.Click += async (sender, e) => { await PremiumFeature_ClickAsync(LicenseCapability.InactivitySignOut, async (ss, ee) => { }, sender, e); };
            _recentFilesListView.ColumnClick += (sender, e) => { SetSortOrder(e.Column); };
            _recentFilesListView.DragOver += (sender, e) => { _mainViewModel.DragAndDropFiles = e.GetDragged(); e.Effect = GetEffectsForRecentFiles(e); };
            _recentFilesListView.MouseClick += (sender, e) => { if (e.Button == MouseButtons.Right) _recentFilesContextMenuStrip.Show((Control)sender, e.Location); };
            _recentFilesListView.SelectedIndexChanged += (sender, e) => { _mainViewModel.SelectedRecentFiles = _recentFilesListView.SelectedItems.Cast<ListViewItem>().Select(lvi => RecentFilesListView.EncryptedPath(lvi)); };
            _removeRecentFileToolStripMenuItem.Click += async (sender, e) => { await _mainViewModel.RemoveRecentFiles.ExecuteAsync(_mainViewModel.SelectedRecentFiles); };
            _clearRecentFilesToolStripMenuItem.Click += async (sender, e) => { await _mainViewModel.RemoveRecentFiles.ExecuteAsync(_mainViewModel.RecentFiles.Select(files => files.EncryptedFileInfo.FullName)); };
            _shareKeysToolStripMenuItem.Click += async (sender, e) => { await ShareKeysAsync(_mainViewModel.SelectedRecentFiles); };
            _watchedFoldersAddSecureFolderMenuItem.Click += async (sender, e) => { await PremiumFeature_ClickAsync(LicenseCapability.SecureFolders, (ss, ee) => { WatchedFoldersAddSecureFolderMenuItem_Click(ss, ee); return Constant.CompletedTask; }, sender, e); };
            _watchedFoldersKeySharingMenuItem.Click += async (sender, e) => { await PremiumFeature_ClickAsync(LicenseCapability.KeySharing, (ss, ee) => { return WatchedFoldersKeySharingAsync(_mainViewModel.SelectedWatchedFolders); }, sender, e); };
            _watchedFoldersListView.DragDrop += async (sender, e) => { await PremiumFeature_ClickAsync(LicenseCapability.SecureFolders, (ss, ee) => { return _mainViewModel.AddWatchedFolders.ExecuteAsync(_mainViewModel.DragAndDropFiles); }, sender, e); };
            _watchedFoldersListView.DragOver += (sender, e) => { _mainViewModel.DragAndDropFiles = e.GetDragged(); e.Effect = GetEffectsForWatchedFolders(e); };
            _watchedFoldersListView.MouseDown += (sender, e) => { if (e.Button == MouseButtons.Right) { ShowHideWatchedFoldersContextMenuItems(e.Location); _watchedFoldersContextMenuStrip.Show((Control)sender, e.Location); } };
            _watchedFoldersListView.SelectedIndexChanged += (sender, e) => { _mainViewModel.SelectedWatchedFolders = _watchedFoldersListView.SelectedItems.Cast<ListViewItem>().Select(lvi => lvi.Text); };
            _watchedFoldersOpenExplorerHereMenuItem.Click += (sender, e) => { _mainViewModel.OpenSelectedFolder.Execute(_mainViewModel.SelectedWatchedFolders.First()); };
            _watchedFoldersRemoveMenuItem.Click += async (sender, e) => { await _mainViewModel.RemoveWatchedFolders.ExecuteAsync(_mainViewModel.SelectedWatchedFolders); };
            _getPremiumToolStripMenuItem.Click += async (sender, e) => { await DisplayPremiumPurchasePage(New<LogOnIdentity, IAccountService>(New<KnownIdentities>().DefaultEncryptionIdentity)); };
            _recentFilesRestoreAnonymousNamesMenuItem.Click += async (sender, e) => await PremiumFeature_ClickAsync(LicenseCapability.RandomRename, async (ss, ee) => { await _fileOperationViewModel.RestoreRandomRenameFiles.ExecuteAsync(_mainViewModel.SelectedRecentFiles); }, sender, e);
        }

        private void _recentFilesContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            bool noPendingDecrypted = !_mainViewModel.SelectedActiveFiles().Any(af => af.IsDecrypted);
            bool filesSelected = _mainViewModel.SelectedRecentFiles.Any();
            bool loggedOn = _mainViewModel.LoggedOn;
            bool canRandomRename = _mainViewModel.RandomRenameEnabled;

            _removeRecentFileToolStripMenuItem.Enabled = noPendingDecrypted;
            _decryptAndRemoveFromListToolStripMenuItem.Enabled = noPendingDecrypted;
            _shareKeysToolStripMenuItem.Enabled = filesSelected & loggedOn & noPendingDecrypted;
            _recentFilesRestoreAnonymousNamesMenuItem.Enabled = canRandomRename & filesSelected & loggedOn & noPendingDecrypted;
            _clearRecentFilesToolStripMenuItem.Enabled = noPendingDecrypted;
        }

        private void ConfigureWatchedFoldersMenus(bool enabled)
        {
            foreach (Control control in _hiddenWatchedFoldersTabPage.Controls)
            {
                control.Enabled = enabled;
            }
            _hiddenWatchedFoldersTabPage.ToolTipText = enabled ? string.Empty : Texts.PremiumFeatureToolTipText;

            if (enabled)
            {
                _encryptedFoldersToolStripMenuItem.ToolTipText = string.Empty;
                _encryptedFoldersToolStripMenuItem.Image = Resources.folder;
            }
            else
            {
                _encryptedFoldersToolStripMenuItem.ToolTipText = Texts.PremiumFeatureToolTipText;
                _encryptedFoldersToolStripMenuItem.Image = Resources.premium_32px;
            }

            if (enabled)
            {
                _addSecureFolderToolStripMenuItem.ToolTipText = string.Empty;
                _addSecureFolderToolStripMenuItem.Image = null;
            }
            else
            {
                _addSecureFolderToolStripMenuItem.ToolTipText = Texts.PremiumFeatureToolTipText;
                _addSecureFolderToolStripMenuItem.Image = Resources.premium_32px;
            }
        }

        private void ShowHideWatchedFoldersContextMenuItems(Point location)
        {
            bool itemSelected = _watchedFoldersListView.HitTest(location).Location == ListViewHitTestLocations.Label;
            _watchedFoldersdecryptTemporarilyMenuItem.Visible = itemSelected;
            _watchedFoldersOpenExplorerHereMenuItem.Visible = itemSelected;
            _watchedFoldersRemoveMenuItem.Visible = itemSelected;
            _watchedFoldersKeySharingMenuItem.Visible = itemSelected;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void BindToFileOperationViewModel()
        {
            _addSecureFolderToolStripMenuItem.Click += async (sender, e) => await PremiumFeature_ClickAsync(LicenseCapability.SecureFolders, (ss, ee) => { WatchedFoldersAddSecureFolderMenuItem_Click(ss, ee); return Task.FromResult<object>(null); }, sender, e);
            _decryptAndRemoveFromListToolStripMenuItem.Click += async (sender, e) => { await _fileOperationViewModel.DecryptFiles.ExecuteAsync(_mainViewModel.SelectedRecentFiles); };
            _decryptToolStripMenuItem.Click += async (sender, e) => { await _fileOperationViewModel.DecryptFiles.ExecuteAsync(null); };
            _encryptedFoldersToolStripMenuItem.Click += async (sender, e) => await PremiumFeature_ClickAsync(LicenseCapability.SecureFolders, (ss, ee) => { encryptedFoldersToolStripMenuItem_Click(ss, ee); return Task.FromResult<object>(null); }, sender, e);
            _encryptToolStripButton.Click += async (sender, e) => await _fileOperationViewModel.EncryptFiles.ExecuteAsync(null);
            _encryptToolStripButton.Tag = _fileOperationViewModel.EncryptFiles;
            _encryptToolStripMenuItem.Click += async (sender, e) => { await _fileOperationViewModel.EncryptFiles.ExecuteAsync(null); };
            _fileOperationViewModel.FirstLegacyOpen += (sender, e) => New<IUIThread>().SendTo(async () => await SetLegacyOpenMode(e));
            _fileOperationViewModel.IdentityViewModel.LoggingOnAsync = async (e) => await New<IUIThread>().SendToAsync(async () => await HandleLogOn(e));
            _fileOperationViewModel.SelectingFiles += (sender, e) => New<IUIThread>().SendTo(() => New<IDataItemSelection>().HandleSelection(e));
            _fileOperationViewModel.ToggleEncryptionUpgradeMode += (sender, e) => New<IUIThread>().SendTo(() => ToggleEncryptionUpgradeMode());
            _inviteUserToolStripMenuItem.Click += async (sender, e) => { await PremiumFeature_ClickAsync(LicenseCapability.KeySharing, async (ss, ee) => { await InviteUserAsync(); }, sender, e); };
            _keyShareToolStripButton.Click += async (sender, e) => { await PremiumFeature_ClickAsync(LicenseCapability.KeySharing, async (ss, ee) => { await ShareKeysWithFileSelectionAsync(); }, sender, e); };
            _openEncryptedToolStripButton.Click += async (sender, e) => { await _fileOperationViewModel.OpenFilesFromFolder.ExecuteAsync(string.Empty); };
            _openEncryptedToolStripMenuItem.Click += async (sender, e) => { await _fileOperationViewModel.OpenFilesFromFolder.ExecuteAsync(string.Empty); };
            _recentFilesListView.DragDrop += async (sender, e) => { await DropFilesOrFoldersInRecentFilesListViewAsync(); };
            _recentFilesListView.MouseDoubleClick += async (sender, e) => { await _fileOperationViewModel.OpenFiles.ExecuteAsync(_mainViewModel.SelectedRecentFiles); };
            _recentFilesOpenToolStripMenuItem.Click += async (sender, e) => { await _fileOperationViewModel.OpenFiles.ExecuteAsync(_mainViewModel.SelectedRecentFiles); };
            _renameToolStripMenuItem.Click += async (sender, e) => await PremiumFeature_ClickAsync(LicenseCapability.RandomRename, async (ss, ee) => { await _fileOperationViewModel.RandomRenameFiles.ExecuteAsync(null); }, sender, e);
            _restoreAnonymousNamesToolStripMenuItem.Click += async (sender, e) => await PremiumFeature_ClickAsync(LicenseCapability.RandomRename, async (ss, ee) => { await _fileOperationViewModel.RestoreRandomRenameFiles.ExecuteAsync(null); }, sender, e);
            _secretsToolStripButton.Click += async (sender, e) => { await PremiumFeature_ClickAsync(LicenseCapability.PasswordManagement, (ss, ee) => { Process.Start(Texts.LinkToSecretsPageWithUserNameFormat.QueryFormat(Resolve.UserSettings.AccountWebUrl, Resolve.KnownIdentities.DefaultEncryptionIdentity.UserEmail)); return Task.FromResult<object>(null); }, sender, e); };
            _secureDeleteToolStripMenuItem.Click += async (sender, e) => await PremiumFeature_ClickAsync(LicenseCapability.SecureWipe, async (ss, ee) => { await _fileOperationViewModel.WipeFiles.ExecuteAsync(null); }, sender, e);
            _tryBrokenFileToolStripMenuItem.Click += async (sender, e) => { await _fileOperationViewModel.TryBrokenFiles.ExecuteAsync(null); };
            _encryptionUpgradeMenuItem.Click += async (sender, e) => await _fileOperationViewModel.AsyncEncryptionUpgrade.ExecuteAsync(null);
            _VerifyFileToolStripMenuItem.Click += async (sender, e) => { await _fileOperationViewModel.VerifyFiles.ExecuteAsync(null); };
            _axcryptFileFormatCheckToolStripMenuItem.Click += async (sender, e) => { await _fileOperationViewModel.IntegrityCheckFiles.ExecuteAsync(null); };
            _watchedFoldersdecryptTemporarilyMenuItem.Click += async (sender, e) => { await _fileOperationViewModel.DecryptFolders.ExecuteAsync(_mainViewModel.SelectedWatchedFolders); };
            _watchedFoldersListView.MouseDoubleClick += async (sender, e) => { await _fileOperationViewModel.OpenFilesFromFolder.ExecuteAsync(_mainViewModel.SelectedWatchedFolders.FirstOrDefault()); };
            _recentFilesShowInFolderToolStripMenuItem.Click += async (sender, e) => { await _fileOperationViewModel.ShowInFolder.ExecuteAsync(_mainViewModel.SelectedRecentFiles); };
        }

        private DeviceLocking _deviceLocking;

        private void WireUpEvents()
        {
            _deviceLocking = new DeviceLocking(
                async () =>
                {
                    await EncryptPendingFiles();

                    if (await _fileOperationViewModel.IdentityViewModel.LogOff.CanExecuteAsync(null))
                    {
                        await _fileOperationViewModel.IdentityViewModel.LogOff.ExecuteAsync(null);
                    }
                },
                async () =>
                {
                    await ShutDownAnd(New<IUIThread>().ExitApplication);
                }
            );

            New<AxCryptOnlineState>().OnlineStateChanged += async (sender, e) =>
            {
                AxCryptOnlineState onLineState = (AxCryptOnlineState)sender;
                if (onLineState.IsOnline)
                {
                    New<ICache>().RemoveItem(CacheKey.RootKey);
                    New<IInternetState>().Clear();

                    await New<SessionNotify>().NotifyAsync(new SessionNotification(SessionNotificationType.RefreshLicensePolicy, New<KnownIdentities>().DefaultEncryptionIdentity));
                    await _mainViewModel.AxCryptUpdateCheck.ExecuteAsync(DateTime.MinValue);
                }
                New<IUIThread>().PostTo(async () =>
                {
                    await SetWindowTitleTextAsync(_mainViewModel.LoggedOn);
                    await _daysLeftPremiumLabel.ConfigureAsync(New<KnownIdentities>().DefaultEncryptionIdentity);
                });
            };
            New<AxCryptOnlineState>().RaiseOnlineStateChanged();
        }

        private static void WireDownEvents()
        {
        }

        private async Task SetSignInSignOutStatusAsync(bool isSignedIn)
        {
            await SetWindowTitleTextAsync(isSignedIn);

            bool isSignedInWithAxCryptId = New<KnownIdentities>().IsLoggedOnWithAxCryptId;

            _createAccountToolStripMenuItem.Enabled = !isSignedIn;
            _debugManageAccountToolStripMenuItem.Enabled = isSignedInWithAxCryptId;
            _exportMyPrivateKeyToolStripMenuItem.Enabled = isSignedInWithAxCryptId;
            _exportSharingKeyToolStripMenuItem.Enabled = isSignedInWithAxCryptId;
            _importMyPrivateKeyToolStripMenuItem.Enabled = !isSignedIn;
            _importOthersSharingKeyToolStripMenuItem.Enabled = isSignedInWithAxCryptId;
            _inviteUserToolStripMenuItem.Enabled = New<AxCryptOnlineState>().IsOnline && isSignedIn;
            _optionsEncryptionUpgradeModeToolStripMenuItem.Enabled = isSignedInWithAxCryptId;
            _optionsChangePassphraseToolStripMenuItem.Enabled = New<AxCryptOnlineState>().IsOnline;
            _passwordResetToolStripMenuItem.Enabled = !isSignedIn && !string.IsNullOrEmpty(New<UserSettings>().UserEmail);
            _signInToolStripMenuItem.Visible = !isSignedIn;
            _notifySignInToolStripMenuItem.Visible = !isSignedIn;
            _signOutToolStripMenuItem.Visible = isSignedIn;
            _notifySignOutToolStripMenuItem.Visible = isSignedIn;
            _encryptionUpgradeMenuItem.Enabled = isSignedInWithAxCryptId;
        }

        private static async Task DisplayPremiumPurchasePage(IAccountService accountService)
        {
            string tag = New<KnownIdentities>().IsLoggedOn ? (await accountService.AccountAsync()).Tag ?? string.Empty : string.Empty;
            string link = Texts.LinkToAxCryptPremiumPurchasePage.QueryFormat(Resolve.UserSettings.AccountWebUrl, New<KnownIdentities>().DefaultEncryptionIdentity.UserEmail, tag);
            Process.Start(link);
        }

        private static async Task SetLegacyOpenMode(FileOperationEventArgs e)
        {
            if (!Resolve.KnownIdentities.IsLoggedOn)
            {
                return;
            }

            PopupButtons click = await New<IPopup>().ShowAsync(PopupButtons.OkCancel, Texts.WarningTitle, Texts.LegacyOpenMessage);
            if (click == PopupButtons.Cancel)
            {
                e.Cancel = true;
                return;
            }
        }

        private void ToggleEncryptionUpgradeMode()
        {
            if (_mainViewModel.EncryptionUpgradeMode == EncryptionUpgradeMode.AutoUpgrade)
            {
                _mainViewModel.EncryptionUpgradeMode = EncryptionUpgradeMode.RetainWithoutUpgrade;
                return;
            }

            if (!New<IVerifySignInPassword>().Verify(Texts.LegacyConversionVerificationPrompt))
            {
                return;
            }

            _mainViewModel.EncryptionUpgradeMode = EncryptionUpgradeMode.AutoUpgrade;
        }

        private async Task LogOffAndLogOnAgainAsync()
        {
            if (!Resolve.KnownIdentities.IsLoggedOn)
            {
                return;
            }

            await LogOnOrLogOffAndLogOnAgainAsync();
        }

        private async Task LogOnOrLogOffAndLogOnAgainAsync()
        {
            bool wasLoggedOn = Resolve.KnownIdentities.IsLoggedOn;
            if (wasLoggedOn)
            {
                await _fileOperationViewModel.IdentityViewModel.LogOnLogOff.ExecuteAsync(null);
            }
            else
            {
                await SignInAsync();
            }
            bool didLogOff = wasLoggedOn && !Resolve.KnownIdentities.IsLoggedOn;
            if (didLogOff)
            {
                await SignInAsync();
            }
        }

        private async Task DropFilesOrFoldersInRecentFilesListViewAsync()
        {
            await this.WithWaitCursorAsync(async () =>
            {
                if (_mainViewModel.DroppableAsRecent)
                {
                    await _fileOperationViewModel.AddRecentFiles.ExecuteAsync(_mainViewModel.DragAndDropFiles);
                }
                if (_mainViewModel.DroppableAsWatchedFolder)
                {
                    await PremiumFeatureActionAsync(LicenseCapability.SecureFolders, async () =>
                    {
                        ShowWatchedFoldersTab();
                        await _mainViewModel.AddWatchedFolders.ExecuteAsync(_mainViewModel.DragAndDropFiles);
                    });
                }
            }, () => { });
        }

        private async Task HandleLogOn(LogOnEventArgs e)
        {
            if (e.IsAskingForPreviouslyUnknownPassphrase)
            {
                HandleCreateNewLogOn(e);
            }
            else
            {
                await HandleExistingLogOn(e);
            }
            if (New<UserSettings>().RestoreFullWindow)
            {
                Styling.RestoreWindowWithFocus(this);
            }
        }

        private void HandleCreateNewLogOn(LogOnEventArgs e)
        {
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
            NewPasswordViewModel viewModel = new NewPasswordViewModel(e.Passphrase.Text, e.EncryptedFileFullName);
            using (NewPassphraseDialog passphraseDialog = new NewPassphraseDialog(this, Texts.NewPassphraseDialogTitle, viewModel))
            {
                viewModel.ShowPassword = e.DisplayPassphrase;
                DialogResult dialogResult = passphraseDialog.ShowDialog(this);
                e.DisplayPassphrase = viewModel.ShowPassword;
                if (dialogResult != DialogResult.OK || viewModel.PasswordText.Length == 0)
                {
                    e.Cancel = true;
                    return;
                }
                e.Passphrase = new Passphrase(viewModel.PasswordText);
                e.Name = String.Empty;
            }
            return;
        }

        private void HandleCreateNewAccount(LogOnEventArgs e)
        {
            using (CreateNewAccountDialog dialog = new CreateNewAccountDialog(this, e.Passphrase.Text, EmailAddress.Empty))
            {
                DialogResult dialogResult = dialog.ShowDialog(this);
                if (dialogResult != DialogResult.OK)
                {
                    e.Cancel = true;
                    return;
                }
                e.DisplayPassphrase = dialog.ShowPassphraseCheckBox.Checked;
                e.Passphrase = new Passphrase(dialog.PassphraseTextBox.Text);
                e.UserEmail = dialog.EmailTextBox.Text;
            }
        }

        private async Task HandleExistingLogOn(LogOnEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.EncryptedFileFullName) && (string.IsNullOrEmpty(Resolve.UserSettings.UserEmail) || Resolve.KnownIdentities.IsLoggedOn))
            {
                HandleExistingLogOnForEncryptedFile(e);
            }
            else
            {
                await HandleExistingAccountLogOn(e);
            }
        }

        private void HandleExistingLogOnForEncryptedFile(LogOnEventArgs e)
        {
            using (FilePasswordDialog logOnDialog = new FilePasswordDialog(this, e.EncryptedFileFullName))
            {
                DialogResult dialogResult = logOnDialog.ShowDialog(this);
                if (dialogResult == DialogResult.Retry)
                {
                    e.Passphrase = logOnDialog.ViewModel.Passphrase;
                    e.IsAskingForPreviouslyUnknownPassphrase = true;
                    return;
                }

                if (dialogResult != DialogResult.OK || logOnDialog.ViewModel.Passphrase == Passphrase.Empty)
                {
                    e.Cancel = true;
                    return;
                }
                e.Passphrase = logOnDialog.ViewModel.Passphrase;
            }
            return;
        }

        private async Task HandleExistingAccountLogOn(LogOnEventArgs e)
        {
            await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.WarningTitle, Texts.WillNotForgetPasswordWarningText, DoNotShowAgainOptions.WillNotForgetPassword, Texts.WillNotForgetPasswordCheckBoxText);

            LogOnAccountViewModel viewModel = new LogOnAccountViewModel(Resolve.UserSettings, e.EncryptedFileFullName);
            using (LogOnAccountDialog logOnDialog = new LogOnAccountDialog(this, viewModel))
            {
                DialogResult dialogResult = logOnDialog.ShowDialog(this);

                if (dialogResult == DialogResult.Retry)
                {
                    await ResetAllSettingsAndRestart();
                }

                if (dialogResult == DialogResult.Cancel)
                {
                    await new ApplicationManager().StopAndExit();
                }

                if (dialogResult != DialogResult.OK || viewModel.PasswordText.Length == 0)
                {
                    e.Cancel = true;
                    return;
                }

                e.Passphrase = new Passphrase(viewModel.PasswordText);
                e.UserEmail = viewModel.UserEmail;
            }
            return;
        }

        private async Task ResetAllSettingsAndRestart()
        {
            if (await WarnIfAnyDecryptedFiles())
            {
                return;
            }

            PopupButtons result = await New<IPopup>().ShowAsync(PopupButtons.OkCancel, Texts.WarningTitle, Texts.ResetAllSettingsWarningText);
            if (result == PopupButtons.Ok)
            {
                new ApplicationManager().WaitForBackgroundToComplete();
                await new ApplicationManager().ClearAllSettings();
                await new ApplicationManager().ShutdownBackgroundSafe();

                New<IUIThread>().RestartApplication();
            }
        }

        private void AxCryptMainForm_CommandComplete(object sender, CommandCompleteEventArgs e)
        {
            Resolve.UIThread.SendToAsync(async () => await DoRequestAsync(e));
        }

        private async Task DoRequestAsync(CommandCompleteEventArgs e)
        {
            switch (e.Verb)
            {
                case CommandVerb.About:
                    New<AboutBox>().ShowNow();
                    return;

                case CommandVerb.Exit:
                    await new ApplicationManager().StopAndExit();
                    return;
            }

            bool wasSignedIn = New<KnownIdentities>().IsLoggedOn;
            if (!wasSignedIn)
            {
                switch (e.Verb)
                {
                    case CommandVerb.Show:
                        New<UserSettings>().RestoreFullWindow = true;
                        Styling.RestoreWindowWithFocus(this);
                        break;

                    case CommandVerb.ShowLogOn:
                        RestoreFormConditionally();
                        break;
                }

                switch (e.Verb)
                {
                    case CommandVerb.Open:
                        await _fileOperationViewModel.OpenFiles.ExecuteAsync(e.Arguments);
                        return;

                    case CommandVerb.Decrypt:
                        await _fileOperationViewModel.DecryptFiles.ExecuteAsync(e.Arguments);
                        return;

                    case CommandVerb.Encrypt:
                    case CommandVerb.Show:
                    case CommandVerb.RandomRename:
                    case CommandVerb.Wipe:
                    case CommandVerb.ShowLogOn:
                        await SignInAsync();
                        break;

                    default:
                        break;
                }

                switch (e.Verb)
                {
                    case CommandVerb.Show:
                    case CommandVerb.ShowLogOn:
                        return;
                }
            }

            if (!New<KnownIdentities>().IsLoggedOn)
            {
                return;
            }

            if (wasSignedIn)
            {
                await ShowSignedInInformationAsync(e.Verb, e.Arguments);
            }

            switch (e.Verb)
            {
                case CommandVerb.Encrypt:
                    await _fileOperationViewModel.EncryptFiles.ExecuteAsync(e.Arguments);
                    break;

                case CommandVerb.Decrypt:
                    await _fileOperationViewModel.DecryptFiles.ExecuteAsync(e.Arguments);
                    break;

                case CommandVerb.Open:
                    await _fileOperationViewModel.OpenFiles.ExecuteAsync(e.Arguments);
                    break;

                case CommandVerb.Wipe:
                    await _fileOperationViewModel.WipeFiles.ExecuteAsync(e.Arguments);
                    break;

                case CommandVerb.RandomRename:
                    await PremiumFeatureActionAsync(LicenseCapability.RandomRename, () => _fileOperationViewModel.RandomRenameFiles.ExecuteAsync(e.Arguments));
                    break;

                case CommandVerb.Show:
                    Styling.RestoreWindowWithFocus(this);
                    break;

                case CommandVerb.SetOfflineMode:
                    New<UserSettings>().OfflineMode = true;
                    break;

                case CommandVerb.SignOut:
                    if (New<KnownIdentities>().IsLoggedOn)
                    {
                        await New<KnownIdentities>().SetDefaultEncryptionIdentity(LogOnIdentity.Empty);
                    }
                    break;

                case CommandVerb.Register:
                    Process.Start(Texts.LinkToSignUpWebPage.QueryFormat(Resolve.UserSettings.AccountWebUrl));
                    break;

                default:
                    break;
            }
        }

        private static Task ShowSignedInInformationAsync(CommandVerb verb, IEnumerable<string> files)
        {
            if (New<UserSettings>().DoNotShowAgain.HasFlag(DoNotShowAgainOptions.SignedInSoNoPasswordRequired))
            {
                return Constant.CompletedTask;
            }

            switch (verb)
            {
                case CommandVerb.Encrypt:
                    return ShowSignedInInformationAlert();

                case CommandVerb.Decrypt:
                case CommandVerb.Open:
                    bool isAnyFileKeyKnown = files.Select(f => New<IDataStore>(f)).IsAnyFileKeyKnown();
                    if (isAnyFileKeyKnown)
                    {
                        return ShowSignedInInformationAlert();
                    }
                    break;

                default:
                    break;
            }
            return Constant.CompletedTask;
        }

        private static Task ShowSignedInInformationAlert()
        {
            return New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.InformationTitle, Texts.NoPasswordRequiredInformationText, DoNotShowAgainOptions.SignedInSoNoPasswordRequired);
        }

        private async Task<DragDropEffects> GetEffectsForMainToolStripAsync(DragEventArgs e)
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
            if (button == _keyShareToolStripButton && _mainViewModel.License.Has(LicenseCapability.KeySharing))
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

        private async Task SetWindowTitleTextAsync(bool isLoggedOn)
        {
            Text = await new Display().WindowTitleTextAsync(isLoggedOn);
        }

        private async Task SetSoftwareStatus()
        {
            VersionUpdateStatus status = _mainViewModel.VersionUpdateStatus;
            switch (status)
            {
                case VersionUpdateStatus.ShortTimeSinceLastSuccessfulCheck:
                case VersionUpdateStatus.IsUpToDate:
                    _softwareStatusButton.ToolTipText = Texts.NoNeedToCheckForUpdatesTooltip;
                    _softwareStatusButton.Image = Resources.bulb_green_40px;
                    break;

                case VersionUpdateStatus.LongTimeSinceLastSuccessfulCheck:
                    _softwareStatusButton.ToolTipText = Texts.OldVersionTooltip;
                    _softwareStatusButton.Image = Resources.bulb_red_40px;
                    break;

                case VersionUpdateStatus.NewerVersionIsAvailable:
                    _softwareStatusButton.ToolTipText = Texts.NewVersionIsAvailableText.InvariantFormat(_mainViewModel.DownloadVersion.Version) + ' ' + Texts.ClickToDownloadText;
                    _softwareStatusButton.Image = Resources.bulb_red_40px;
                    break;

                case VersionUpdateStatus.Unknown:
                    _softwareStatusButton.ToolTipText = Texts.ClickToCheckForNewerVersionTooltip;
                    _softwareStatusButton.Image = Resources.bulb_red_40px;
                    break;
            }
        }

        private bool _userInitiatedUpdateCheckPending = false;

        private async Task DisplayUpdateCheckPopups()
        {
            await new Display().UpdateCheckPopups(_userInitiatedUpdateCheckPending, _mainViewModel.DownloadVersion);
            _userInitiatedUpdateCheckPending = false;
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
                button.ImageScaling = ToolStripItemImageScaling.SizeToFit;
                button.Size = new Size(40, 40);
                button.Margin = new Padding(0);
                button.Padding = new Padding(0);
                button.AutoSize = false;
                button.ImageAlign = ContentAlignment.MiddleCenter;
                button.Tag = knownFolder;
                button.ToolTipText = Texts.DefaultSecureFolderToolTip;
                button.Click += async (sender, e) =>
                {
                    ToolStripItem item = sender as ToolStripItem;
                    await _fileOperationViewModel.OpenFilesFromFolder.ExecuteAsync(((KnownFolder)item.Tag).My.FullName);
                };
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

        private async void _exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (await WarnIfAnyDecryptedFiles())
            {
                return;
            }
            await ShutDownAnd(New<IUIThread>().ExitApplication);
        }

        private void RestoreFormConditionally()
        {
            if (!New<UserSettings>().RestoreFullWindow)
            {
                return;
            }
            Styling.RestoreWindowWithFocus(this);
        }

        private async Task ShutDownAnd(Action finalAction)
        {
            await new ApplicationManager().ShutdownBackgroundSafe();
            await EncryptPendingFiles();

            finalAction();
        }

        #region ToolStrip

        private async void MainToolStrip_DragDrop(object sender, DragEventArgs e)
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

            await ((IAsyncAction)button.Tag).ExecuteAsync(_mainViewModel.DragAndDropFiles);
        }

        #endregion ToolStrip

        private async Task EncryptPendingFiles()
        {
            if (_mainViewModel != null)
            {
                new ApplicationManager().WaitForBackgroundToComplete();
                await _mainViewModel.EncryptPendingFiles.ExecuteAsync(null);
                new ApplicationManager().WaitForBackgroundToComplete();
            }
        }

        private async Task<bool> WarnIfAnyDecryptedFiles()
        {
            IEnumerable<ActiveFile> openFiles = _mainViewModel.DecryptedFiles ?? new ActiveFile[0];
            if (!openFiles.Any())
            {
                return false;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Texts.DecryptedFilesWarning).AppendLine();
            foreach (ActiveFile openFile in openFiles)
            {
                sb.Append("{0}{1}".InvariantFormat(Path.GetFileName(openFile.DecryptedFileInfo.FullName), Environment.NewLine));
            }
            sb.Append("{0}{1}".InvariantFormat(Environment.NewLine, Texts.DecryptedFilesWarningWhenExitOrReset));
            await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.WarningTitle, sb.ToString());
            return true;
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
                    comparer = ActiveFileComparer.DateComparer;
                    break;

                case 2:
                    comparer = ActiveFileComparer.EncryptedNameComparer;
                    break;

                case 3:
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

        private async Task PremiumFeature_ClickAsync(LicenseCapability requiredCapability, Func<object, EventArgs, Task> realHandler, object sender, EventArgs e)
        {
            if (_mainViewModel.License.Has(requiredCapability))
            {
                if (realHandler != null)
                {
                    await realHandler(sender, e);
                }
                return;
            }

            await DisplayPremiumPurchasePage(New<LogOnIdentity, IAccountService>(New<KnownIdentities>().DefaultEncryptionIdentity));
        }

        private async Task PremiumFeatureActionAsync(LicenseCapability requiredCapability, Func<Task> realHandler)
        {
            if (_mainViewModel.License.Has(requiredCapability))
            {
                await realHandler();
                return;
            }

            await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.WarningTitle, Texts.PremiumFeatureToolTipText);
        }

        private async void CloseAndRemoveOpenFilesToolStripButton_Click(object sender, EventArgs e)
        {
            await EncryptPendingFiles();
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

        private async void LanguageToolStripMenuItem_ClickAsync(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
            await SetLanguageAsync((string)menuItem.Tag);
        }

        private async Task SetLanguageAsync(string cultureName)
        {
            Resolve.UserSettings.CultureName = cultureName;
            if (Resolve.Log.IsInfoEnabled)
            {
                Resolve.Log.LogInfo("Set new UI language culture to '{0}'.".InvariantFormat(Resolve.UserSettings.CultureName));
            }

            InitializeContentResources();
            await SetWindowTitleTextAsync(_mainViewModel.LoggedOn);
            _daysLeftPremiumLabel.UpdateText();
            await SetSoftwareStatus();
        }

        private void OptionsLanguageToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripMenuItem languageMenu = (ToolStripMenuItem)sender;
            string currentLanguage = Thread.CurrentThread.CurrentUICulture.Name;
            foreach (ToolStripItem item in languageMenu.DropDownItems)
            {
                string languageName = item.Tag as string;
                ((ToolStripMenuItem)item).Checked = languageName == currentLanguage;
            }
        }

        private async void _softwareStatusButton_Click(object sender, EventArgs e)
        {
            switch (_mainViewModel.VersionUpdateStatus)
            {
                case VersionUpdateStatus.LongTimeSinceLastSuccessfulCheck:
                case VersionUpdateStatus.ShortTimeSinceLastSuccessfulCheck:
                case VersionUpdateStatus.IsUpToDate:
                case VersionUpdateStatus.Unknown:
                    _userInitiatedUpdateCheckPending = true;
                    await _mainViewModel.AxCryptUpdateCheck.ExecuteAsync(DateTime.MinValue);
                    break;

                case VersionUpdateStatus.NewerVersionIsAvailable:
                    Process.Start(Resolve.UserSettings.UpdateUrl.ToString());
                    break;

                default:
                    break;
            }
        }

        private void SetOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (DebugOptionsDialog dialog = new DebugOptionsDialog(this))
            {
                dialog._restApiBaseUrl.Text = Resolve.UserSettings.RestApiBaseUrl.ToString();
                dialog._timeoutTimeSpan.Text = Resolve.UserSettings.ApiTimeout.ToString();
                DialogResult result = dialog.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return;
                }
                Resolve.UserSettings.RestApiBaseUrl = new Uri(dialog._restApiBaseUrl.Text);
                Resolve.UserSettings.ApiTimeout = TimeSpan.Parse(dialog._timeoutTimeSpan.Text);
            }
        }

        private void _aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            New<AboutBox>().ShowNow();
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
            if (_deviceLocking != null)
            {
                _deviceLocking.Dispose();
                _deviceLocking = null;
            }

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

        private void PasswordReset_Click(object sender, EventArgs e)
        {
            string userEmail = New<UserSettings>().UserEmail.ToString();
            Process.Start(userEmail.GetPasswordResetUrl().ToString());
        }

        private void PolicyMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            SetCheckedToolStripMenuItem(item);
            ReRegisterPolicy(item);
            _mainViewModel.LicenseUpdate.Execute(null);
        }

        private static void ReRegisterPolicy(ToolStripMenuItem item)
        {
            if (item.Text == Texts.LicenseFreeNameText)
            {
                TypeMap.Register.Singleton<LicensePolicy>(() => new FreeForcedLicensePolicy());
                return;
            }
            if (item.Text == Texts.LicensePremiumNameText)
            {
                TypeMap.Register.Singleton<LicensePolicy>(() => new PremiumForcedLicensePolicy());
                return;
            }
            throw new InvalidOperationException("Unexpected license policy name.");
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
            ShowWatchedFoldersTab();
        }

        private void ShowWatchedFoldersTab()
        {
            _statusTabControl.SelectedIndex = 1;
        }

        private void CreateAccountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (CreateNewAccountDialog dialog = new CreateNewAccountDialog(this, String.Empty, EmailAddress.Empty))
            {
                dialog.ShowDialog();
            }
        }

        private async void ManageAccountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (ManageAccountDialog dialog = await ManageAccountDialog.CreateAsync(this))
            {
                dialog.ShowDialog();
            }
        }

        private async void ChangePassphraseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Resolve.KnownIdentities.IsLoggedOnWithAxCryptId)
            {
                await SignIn();
            }
            if (!Resolve.KnownIdentities.IsLoggedOnWithAxCryptId)
            {
                return;
            }

            AccountStorage accountStorage = new AccountStorage(New<LogOnIdentity, IAccountService>(Resolve.KnownIdentities.DefaultEncryptionIdentity));
            ManageAccountViewModel viewModel = await ManageAccountViewModel.CreateAsync(accountStorage);

            if (await this.ChangePasswordDialogAsync(viewModel))
            {
                await LogOnOrLogOffAndLogOnAgainAsync();
            }
        }

        private void ExportMySharingKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UserKeyPair activeKeyPair = Resolve.KnownIdentities.DefaultEncryptionIdentity.ActiveEncryptionKeyPair;
            EmailAddress userEmail = activeKeyPair.UserEmail;
            IAsymmetricPublicKey publicKey = activeKeyPair.KeyPair.PublicKey;
            string fileName;
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = Texts.DialogExportSharingKeyTitle;
                sfd.DefaultExt = ".txt";
                sfd.AddExtension = true;
                sfd.Filter = Texts.FileFilterDialogFilterPatternWin.InvariantFormat("." + sfd.DefaultExt, Texts.FileFilterFileTypePublicSharingKeyFiles, Texts.FileFilterFileTypeAllFiles);
                sfd.CheckPathExists = true;
                sfd.FileName = Texts.DialogExportSharingKeyFileName.InvariantFormat(userEmail.Address, publicKey.Tag);
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
            fileSelection.SelectingFiles += (sfsender, sfe) => { New<IDataItemSelection>().HandleSelection(sfe); };
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

        private async Task ShareKeysWithFileSelectionAsync()
        {
            FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs(new string[0])
            {
                FileSelectionType = FileSelectionType.KeySharing,
            };
            await New<IDataItemSelection>().HandleSelection(fileSelectionArgs);

            if (fileSelectionArgs.Cancel)
            {
                return;
            }

            await ShareKeysAsync(fileSelectionArgs.SelectedFiles);
        }

        private async Task ShareKeysAsync(IEnumerable<string> fileNames)
        {
            SharingListViewModel viewModel = await SharingListViewModel.CreateForFilesAsync(fileNames, Resolve.KnownIdentities.DefaultEncryptionIdentity);
            using (KeyShareDialog dialog = new KeyShareDialog(this, viewModel))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
            }
            await viewModel.ShareFiles.ExecuteAsync(null);
        }

        private async Task WatchedFoldersKeySharingAsync(IEnumerable<string> folderPaths)
        {
            if (!folderPaths.Any())
            {
                return;
            }

            SharingListViewModel viewModel = await SharingListViewModel.CreateForFoldersAsync(folderPaths, Resolve.KnownIdentities.DefaultEncryptionIdentity);
            using (KeyShareDialog dialog = new KeyShareDialog(this, viewModel))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
            }

            await viewModel.ShareFolders.ExecuteAsync(null);
        }

        private void ExportMyPrivateKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UserKeyPair activeKeyPair = Resolve.KnownIdentities.DefaultEncryptionIdentity.ActiveEncryptionKeyPair;
            IAsymmetricPublicKey publicKey = activeKeyPair.KeyPair.PublicKey;

            string fileName;
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = Texts.DialogExportAxCryptIdTitle;
                sfd.DefaultExt = New<IRuntimeEnvironment>().AxCryptExtension;
                sfd.AddExtension = true;
                sfd.Filter = Texts.FileFilterDialogFilterPatternWin.InvariantFormat("." + sfd.DefaultExt, Texts.FileFilterFileTypeAxCryptIdFiles, Texts.FileFilterFileTypeAllFiles);
                sfd.CheckPathExists = true;
                sfd.FileName = Texts.DialogExportAxCryptIdFileName.InvariantFormat(activeKeyPair.UserEmail, publicKey.Tag);
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

            byte[] export = activeKeyPair.ToArray(Resolve.KnownIdentities.DefaultEncryptionIdentity.Passphrase);
            File.WriteAllBytes(fileName, export);
        }

        private void AxCryptMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                WindowState = FormWindowState.Minimized;
                return;
            }

            if (_debugOutput != null)
            {
                _debugOutput.AllowClose = true;
            }

            WireDownEvents();
        }

        private async void WatchedFoldersAddSecureFolderMenuItem_Click(object sender, EventArgs e)
        {
            string folder = SelectSecureFolder();
            if (string.IsNullOrEmpty(folder))
            {
                return;
            }

            await _mainViewModel.AddWatchedFolders.ExecuteAsync(new string[] { folder });
        }

        private string SelectSecureFolder()
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = Texts.AddSecureFolderTitle;
                fbd.ShowNewFolderButton = true;
                DialogResult result = fbd.ShowDialog(this);

                if (result == DialogResult.OK)
                {
                    return fbd.SelectedPath;
                }
            }
            return String.Empty;
        }

        private async Task ToggleIncludeSubfoldersOption()
        {
            if (_mainViewModel.FolderOperationMode == FolderOperationMode.IncludeSubfolders)
            {
                _mainViewModel.FolderOperationMode = FolderOperationMode.SingleFolder;
                return;
            }

            if (!New<IVerifySignInPassword>().Verify(Texts.ChangeOptionGenericWarning))
            {
                return;
            }

            PopupButtons result = await New<IPopup>().ShowAsync(PopupButtons.OkCancel, Texts.IncludeSubfoldersConfirmationTitle, Texts.IncludeSubfoldersConfirmationBody);
            if (result == PopupButtons.Ok)
            {
                _mainViewModel.FolderOperationMode = FolderOperationMode.IncludeSubfolders;
            }
        }

        private void IdleSignOutToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripMenuItem timeOutMenu = (ToolStripMenuItem)sender;
            TimeSpan selectedTimeOutDuration = New<UserSettings>().InactivitySignOutTime;
            bool hasFeature = New<LicensePolicy>().Capabilities.Has(LicenseCapability.InactivitySignOut);
            foreach (ToolStripItem item in timeOutMenu.DropDownItems)
            {
                ToolStripMenuItem menuItem = (ToolStripMenuItem)item;
                int timeOutDuration = int.Parse(menuItem.Tag.ToString());
                menuItem.Checked = TimeSpan.FromMinutes(timeOutDuration) == selectedTimeOutDuration;
                menuItem.Enabled = hasFeature && !menuItem.Checked;
            }
        }

        private async void IdleSignOutToolStripMenuItem_ClickAsync(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
            New<UserSettings>().InactivitySignOutTime = TimeSpan.FromMinutes(int.Parse(menuItem.Tag.ToString()));
            TypeMap.Register.Singleton<InactivitySignOut>(() => new InactivitySignOut(New<UserSettings>().InactivitySignOutTime));
            New<InactivitySignOut>().RestartInactivityTimer();
        }

        private void InitializeMouseDownFilter()
        {
            New<MouseDownFilter>().FormClicked += AxCryptMainForm_ClickAsync;
        }

        private async void AxCryptMainForm_ClickAsync(object sender, EventArgs e)
        {
            New<InactivitySignOut>().RestartInactivityTimer();
        }

        private async Task InviteUserAsync()
        {
            using (InviteUserDialog dialog = new InviteUserDialog(this))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
            }
        }
    }
}