#region Coypright and License

/*
 * AxCrypt - Copyright 2013, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Ipc;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Mono;
using Axantum.AxCrypt.Presentation;
using Axantum.AxCrypt.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    /// <summary>
    /// All code here is expected to execute on the GUI thread. If code may be called on another thread, this call
    /// must be made through ThreadSafeUi() .
    /// </summary>
    public partial class AxCryptMainForm : Form, IMainView, IStatusChecker
    {
        private Uri _updateUrl;

        private TabPage _hiddenLogTabPage = null;

        private NotifyIcon _notifyIcon = null;

        private string _title;

        private WatchedFolderPresentation _watchedFoldersPresentation;

        private RecentFilesPresentation _recentFilesPresentation;

        private FileOperationsPresentation _fileOperationsPresentation;

        private PassphrasePresentation _passphrasePresentation;

        private MainViewModel _mainViewModel = new MainViewModel();

        public static MessageBoxOptions MessageBoxOptions { get; private set; }

        public ListView WatchedFolders
        {
            get { return _watchedFoldersListView; }
        }

        public ListView RecentFiles
        {
            get { return _recentFilesListView; }
        }

        public TabControl Tabs
        {
            get { return _statusTabControl; }
        }

        public IContainer Components
        {
            get { return components; }
        }

        public Control Control
        {
            get { return this; }
        }

        public DragDropEffects GetDragDropEffects(object sender, DragEventArgs e)
        {
            if (Object.ReferenceEquals(sender, _mainToolStrip))
            {
                return GetEffectsForMainToolStrip(e);
            }
            return DragDropEffects.None;
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
            if (button == _decryptToolStripButton || button == _openEncryptedToolStripButton)
            {
                if ((_mainViewModel.DragAndDropFilesTypes & FileInfoTypes.EncryptedFile) == FileInfoTypes.EncryptedFile)
                {
                    return (DragDropEffects.Link | DragDropEffects.Copy) & e.AllowedEffect;
                }
            }
            return DragDropEffects.None;
        }

        public AxCryptMainForm()
        {
            InitializeComponent();
        }

        private bool _loaded = false;

        private void AxCryptMainForm_Load(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            if (OS.Current.Platform == Platform.WindowsDesktop)
            {
                _notifyIcon = new NotifyIcon(components);
                _notifyIcon.MouseClick += TrayNotifyIcon_MouseClick;
                _notifyIcon.Icon = Resources.axcrypticon;
                _notifyIcon.Visible = true;
            }

            Trace.Listeners.Add(new DelegateTraceListener("AxCryptMainFormListener", FormatTraceMessage)); //MLHIDE

            FactoryRegistry.Instance.Singleton<KnownKeys>(new KnownKeys());
            FactoryRegistry.Instance.Singleton<IUIThread>(new UIThread(this));
            FactoryRegistry.Instance.Singleton<IProgressBackground>(_progressBackgroundWorker);
            FactoryRegistry.Instance.Singleton<IStatusChecker>(this);
            FactoryRegistry.Instance.Singleton<ParallelBackground>(new ParallelBackground());
            FactoryRegistry.Instance.Singleton<ProcessState>(new ProcessState());

            _watchedFoldersPresentation = new WatchedFolderPresentation(this);
            _recentFilesPresentation = new RecentFilesPresentation(this);
            _fileOperationsPresentation = new FileOperationsPresentation(this);
            _passphrasePresentation = new PassphrasePresentation(this);

            UpdateDebugMode();

            _title = "{0} {1}{2}".InvariantFormat(Application.ProductName, Application.ProductVersion, String.IsNullOrEmpty(AboutBox.AssemblyDescription) ? String.Empty : " " + AboutBox.AssemblyDescription); //MLHIDE
            _updateUrl = Instance.FileSystemState.Settings.UpdateUrl;

            MessageBoxOptions = RightToLeft == RightToLeft.Yes ? MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading : 0;

            _encryptToolStripButton.Tag = FileInfoTypes.EncryptableFile;
            _openEncryptedToolStripButton.Tag = FileInfoTypes.EncryptedFile;
            _decryptToolStripButton.Tag = FileInfoTypes.EncryptedFile;

            SetupPathFilters();

            OS.Current.SessionChanged += HandleSessionChangedEvent;

            Instance.FileSystemState.ActiveFileChanged += HandleActiveFileChangedEvent;

            OS.Current.KeyWrapIterations = Instance.FileSystemState.KeyWrapIterations;
            OS.Current.ThumbprintSalt = Instance.FileSystemState.ThumbprintSalt;

            _backgroundMonitor.UpdateCheck.VersionUpdate += new EventHandler<VersionEventArgs>(HandleVersionUpdateEvent);
            UpdateCheck(Instance.FileSystemState.Settings.LastUpdateCheckUtc);

            RestoreUserPreferences();

            Instance.CommandService.Received += AxCryptMainForm_Request;
            Instance.CommandService.StartListening();

            BindToMainViewModel();

            _loaded = true;
            ReStartSession();
        }

        private void BindToMainViewModel()
        {
            _mainViewModel.BindPropertyChanged("LogonEnabled", (bool logonEnabled) => { _encryptionKeyToolStripButton.Image = logonEnabled ? Resources.encryptionkeygreen32 : Resources.encryptionkeyred32; });
            _mainViewModel.BindPropertyChanged("LogonEnabled", (bool logonEnabled) => { _encryptionKeyToolStripButton.ToolTipText = logonEnabled ? Resources.NoDefaultEncryptionKeySetToolTip : Resources.DefaultEncryptionKeyIsIsetToolTip; });
            _mainViewModel.BindPropertyChanged("EncryptFileEnabled", (bool enabled) => { _encryptToolStripButton.Enabled = enabled; });
            _mainViewModel.BindPropertyChanged("EncryptFileEnabled", (bool enabled) => { _encryptToolStripMenuItem.Enabled = enabled; });
            _mainViewModel.BindPropertyChanged("DecryptFileEnabled", (bool enabled) => { _decryptToolStripButton.Enabled = enabled; });
            _mainViewModel.BindPropertyChanged("DecryptFileEnabled", (bool enabled) => { _decryptToolStripMenuItem.Enabled = enabled; });
            _mainViewModel.BindPropertyChanged("OpenEncryptedEnabled", (bool enabled) => { _openEncryptedToolStripButton.Enabled = enabled; });
            _mainViewModel.BindPropertyChanged("OpenEncryptedEnabled", (bool enabled) => { _openEncryptedToolStripMenuItem.Enabled = enabled; });

            _encryptionKeyToolStripButton.Click += (sender, e) => { _mainViewModel.LogOnLogOff(); };
            _encryptToolStripButton.Click += (sender, e) => { _mainViewModel.EncryptFiles(); };
            _encryptToolStripMenuItem.Click += (sender, e) => { _mainViewModel.EncryptFiles(); };
            _decryptToolStripButton.Click += (sender, e) => { _mainViewModel.DecryptFiles(); };
            _decryptToolStripMenuItem.Click += (sender, e) => { _mainViewModel.DecryptFiles(); };
            _openEncryptedToolStripButton.Click += (sender, e) => { _mainViewModel.OpenFileFromFolder(String.Empty); };
            _openEncryptedToolStripMenuItem.Click += (sender, e) => { _mainViewModel.OpenFileFromFolder(String.Empty); };
            
            _watchedFoldersListView.SelectedIndexChanged += (sender, e) => { _mainViewModel.SelectedWatchedFolders = _watchedFoldersListView.SelectedItems.Cast<ListViewItem>().Select(lvi => lvi.Text); };
            _watchedFoldersListView.MouseDoubleClick += (sender, e) => { _mainViewModel.OpenFileFromFolder(_mainViewModel.SelectedWatchedFolders.FirstOrDefault()); };

            _recentFilesListView.SelectedIndexChanged += (sender, e) => { _mainViewModel.SelectedRecentFiles = _recentFilesListView.SelectedItems.Cast<ListViewItem>().Select(lvi => lvi.Text); };
            _watchedFoldersListView.MouseDoubleClick += (sender, e) => { _mainViewModel.OpenFiles(_mainViewModel.SelectedRecentFiles); };

            _mainToolStrip.DragOver += (sender, e) => { _mainViewModel.DragAndDropFiles = e.GetDragged(); e.Effect = GetEffectsForMainToolStrip(e); };

            _mainViewModel.LoggingOn += (object me, LogOnEventArgs args) => { HandleLogOn(args); };
            _mainViewModel.SelectingFiles += (object me, FileSelectionEventArgs args) => { HandleFileSelection(args); };
        }

        private void HandleLogOn(LogOnEventArgs args)
        {
            if (args.CreateNew)
            {
                HandleCreateNewLogOn(args);
            }
            else
            {
                HandleExistingLogOn(args);
            }
        }

        private void HandleCreateNewLogOn(LogOnEventArgs args)
        {
            using (EncryptPassphraseDialog passphraseDialog = new EncryptPassphraseDialog(Instance.FileSystemState, args.Passphrase))
            {
                passphraseDialog.ShowPassphraseCheckBox.Checked = args.DisplayPassphrase;
                DialogResult dialogResult = passphraseDialog.ShowDialog(this);
                if (dialogResult != DialogResult.OK || passphraseDialog.PassphraseTextBox.Text.Length == 0)
                {
                    args.Cancel = true;
                    return;
                }
                args.DisplayPassphrase = passphraseDialog.ShowPassphraseCheckBox.Checked;
                args.Passphrase = passphraseDialog.PassphraseTextBox.Text;
            }
            return;
        }

        private void HandleExistingLogOn(LogOnEventArgs args)
        {
            using (LogOnDialog logOnDialog = new LogOnDialog(Instance.FileSystemState, args.Identity))
            {
                logOnDialog.ShowPassphraseCheckBox.Checked = args.DisplayPassphrase;
                DialogResult dialogResult = logOnDialog.ShowDialog(this);
                if (dialogResult == DialogResult.Retry)
                {
                    args.CreateNew = true;
                    return;
                }

                if (dialogResult != DialogResult.OK || logOnDialog.PassphraseTextBox.Text.Length == 0)
                {
                    args.Cancel = true;
                    return;
                }
                args.DisplayPassphrase = logOnDialog.ShowPassphraseCheckBox.Checked;
                args.Passphrase = logOnDialog.PassphraseTextBox.Text;
            }
            return;
        }

        private void HandleFileSelection(FileSelectionEventArgs args)
        {
            switch (args.FileSelectionType)
            {
                case FileSelectionType.SaveAsEncrypted:
                case FileSelectionType.SaveAsDecrypted:
                    HandleSaveAsFileSelection(args);
                    break;
                default:
                    HandleOpenFileSelection(args);
                    break;
            }
        }

        private void HandleOpenFileSelection(FileSelectionEventArgs args)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (args.SelectedFiles != null && args.SelectedFiles.Count > 0 && !String.IsNullOrEmpty(args.SelectedFiles[0]))
                {
                    IRuntimeFileInfo initialFolder = OS.Current.FileInfo(args.SelectedFiles[0]);
                    if (initialFolder.IsFolder)
                    {
                        ofd.InitialDirectory = initialFolder.FullName;
                    }
                }
                switch (args.FileSelectionType)
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
                    default:
                        break;
                }
                DialogResult result = ofd.ShowDialog();
                if (result != DialogResult.OK)
                {
                    args.Cancel = true;
                    return;
                }
                args.SelectedFiles = new List<string>(ofd.FileNames);
            }
        }

        private void HandleSaveAsFileSelection(FileSelectionEventArgs args)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                switch (args.FileSelectionType)
                {
                    case FileSelectionType.SaveAsEncrypted:
                        sfd.Title = Resources.EncryptFileSaveAsDialogTitle;
                        sfd.DefaultExt = OS.Current.AxCryptExtension;
                        sfd.AddExtension = true;
                        sfd.Filter = Resources.EncryptedFileDialogFilterPattern.InvariantFormat(OS.Current.AxCryptExtension);
                        break;
                    case FileSelectionType.SaveAsDecrypted:
                        string extension = Path.GetExtension(args.SelectedFiles[0]);
                        sfd.Title = Resources.DecryptedSaveAsFileDialogTitle;
                        sfd.DefaultExt = extension;
                        sfd.AddExtension = !String.IsNullOrEmpty(extension);
                        sfd.Filter = Resources.DecryptedSaveAsFileDialogFilterPattern.InvariantFormat(extension);
                        break;
                }
                sfd.CheckPathExists = true;
                sfd.FileName = Path.GetFileName(args.SelectedFiles[0]);
                sfd.InitialDirectory = Path.GetDirectoryName(args.SelectedFiles[0]);
                sfd.ValidateNames = true;
                sfd.OverwritePrompt = true;
                sfd.RestoreDirectory = true;
                DialogResult saveAsResult = sfd.ShowDialog();
                if (saveAsResult != DialogResult.OK)
                {
                    args.Cancel = true;
                    return;
                }
                args.SelectedFiles[0] = sfd.FileName;
            }
        }

        private void AxCryptMainForm_Request(object sender, CommandServiceArgs e)
        {
            Instance.UIThread.RunOnUIThread(() => DoRequest(e));
        }

        private void DoRequest(CommandServiceArgs e)
        {
            switch (e.RequestCommand)
            {
                case CommandVerb.Open:
                    RestoreWindowWithFocus();
                    _mainViewModel.OpenFiles(e.Paths);
                    break;

                case CommandVerb.Exit:
                    Application.Exit();
                    break;

                case CommandVerb.Show:
                    break;
            }
        }

        private void RestoreWindowWithFocus()
        {
            if (WindowState == FormWindowState.Normal)
            {
                WindowState = FormWindowState.Minimized;
            }
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void RestoreUserPreferences()
        {
            if (WindowState == FormWindowState.Normal)
            {
                Height = Instance.FileSystemState.Settings.MainWindowHeight > 0 ? Instance.FileSystemState.Settings.MainWindowHeight : Height;
                Width = Instance.FileSystemState.Settings.MainWindowWidth > 0 ? Instance.FileSystemState.Settings.MainWindowWidth : Width;
                Point settingsLocation = new Point(Instance.FileSystemState.Settings.MainWindowLocationX, Instance.FileSystemState.Settings.MainWindowLocationY);
                Location = settingsLocation != Point.Empty ? settingsLocation : Location;
            }
        }

        private static void SetupPathFilters()
        {
            OS.PathFilters.Add(new Regex(@"\\\.dropbox$"));
            OS.PathFilters.Add(new Regex(@"\\desktop\.ini$"));
            AddEnvironmentVariableBasedPathFilter(@"^{0}(?!Temp$)", "SystemRoot");
            AddEnvironmentVariableBasedPathFilter(@"^{0}(?!Temp$)", "windir");
            AddEnvironmentVariableBasedPathFilter(@"^{0}", "ProgramFiles");
            AddEnvironmentVariableBasedPathFilter(@"^{0}", "ProgramFiles(x86)");
            AddEnvironmentVariableBasedPathFilter(@"^{0}$", "SystemDrive");
        }

        private static void AddEnvironmentVariableBasedPathFilter(string formatRegularExpression, string name)
        {
            string folder = name.FolderFromEnvironment();
            if (String.IsNullOrEmpty(folder))
            {
                return;
            }
            folder = folder.Replace(@"\", @"\\");
            OS.PathFilters.Add(new Regex(formatRegularExpression.InvariantFormat(folder)));
        }

        private void SetWindowTextWithLogonStatus()
        {
            string logonStatus;
            if (Instance.KnownKeys.IsLoggedOn)
            {
                PassphraseIdentity identity = Instance.FileSystemState.Identities.First(i => i.Thumbprint == Instance.KnownKeys.DefaultEncryptionKey.Thumbprint);
                logonStatus = Resources.LoggedOnStatusText.InvariantFormat(identity.Name);
            }
            else
            {
                logonStatus = Resources.LoggedOffStatusText;
            }
            string text = "{0} - {1}".InvariantFormat(_title, logonStatus);
            if (String.Compare(Text, text, StringComparison.Ordinal) != 0)
            {
                Text = text;
            }
        }

        private void FormatTraceMessage(string message)
        {
            Instance.UIThread.RunOnUIThread(() =>
            {
                string formatted = "{0} {1}".InvariantFormat(OS.Current.UtcNow.ToString("o", CultureInfo.InvariantCulture), message.TrimLogMessage()); //MLHIDE
                _logOutputTextBox.AppendText(formatted);
            });
        }

        private void UpdateCheck(DateTime lastCheckUtc)
        {
            _backgroundMonitor.UpdateCheck.CheckInBackground(lastCheckUtc, Instance.FileSystemState.Settings.NewestKnownVersion, Instance.FileSystemState.Settings.AxCrypt2VersionCheckUrl, Instance.FileSystemState.Settings.UpdateUrl);
        }

        private void UpdateVersionStatus(VersionUpdateStatus status, Version version)
        {
            switch (status)
            {
                case VersionUpdateStatus.IsUpToDateOrRecentlyChecked:
                    _updateToolStripButton.ToolTipText = Axantum.AxCrypt.Properties.Resources.NoNeedToCheckForUpdatesTooltip;
                    _updateToolStripButton.Enabled = false;
                    break;

                case VersionUpdateStatus.LongTimeSinceLastSuccessfulCheck:
                    _updateToolStripButton.ToolTipText = Axantum.AxCrypt.Properties.Resources.OldVersionTooltip;
                    _updateToolStripButton.Image = Resources.refreshred;
                    _updateToolStripButton.Enabled = true;
                    break;

                case VersionUpdateStatus.NewerVersionIsAvailable:
                    _updateToolStripButton.ToolTipText = Axantum.AxCrypt.Properties.Resources.NewVersionIsAvailableTooltip.InvariantFormat(version);
                    _updateToolStripButton.Image = Resources.refreshred;
                    _updateToolStripButton.Enabled = true;
                    break;

                case VersionUpdateStatus.ShortTimeSinceLastSuccessfulCheck:
                    _updateToolStripButton.ToolTipText = Axantum.AxCrypt.Properties.Resources.ClickToCheckForNewerVersionTooltip;
                    _updateToolStripButton.Image = Resources.refreshgreen;
                    _updateToolStripButton.Enabled = true;
                    break;
            }
        }

        private void HandleVersionUpdateEvent(object sender, VersionEventArgs e)
        {
            Instance.FileSystemState.Settings.LastUpdateCheckUtc = OS.Current.UtcNow;
            Instance.FileSystemState.Settings.NewestKnownVersion = e.Version.ToString();
            Instance.FileSystemState.Save();
            _updateUrl = e.UpdateWebpageUrl;
            Instance.UIThread.RunOnUIThread(() =>
            {
                UpdateVersionStatus(e.VersionUpdateStatus, e.Version);
            });
        }

        private void HandleActiveFileChangedEvent(object sender, ActiveFileChangedEventArgs e)
        {
            Instance.UIThread.RunOnUIThread(() =>
            {
                _recentFilesPresentation.UpdateActiveFilesViews(e.ActiveFile);
            });
        }

        private void AxCryptMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            Instance.BackgroundWork.WaitForIdle();
            PurgeActiveFiles();
            Instance.BackgroundWork.WaitForIdle();
            Trace.Listeners.Remove("AxCryptMainFormListener");        //MLHIDE
        }

        private void ReStartSession()
        {
            _recentFilesListView.Items.Clear();
            _watchedFoldersListView.Items.Clear();
            SetWindowTextWithLogonStatus();
            OS.Current.NotifySessionChanged(new SessionEvent(SessionEventType.SessionStart));
        }

        public bool CheckStatusAndShowMessage(FileOperationStatus status, string displayContext)
        {
            switch (status)
            {
                case FileOperationStatus.Success:
                    return true;

                case FileOperationStatus.UnspecifiedError:
                    Resources.FileOperationFailed.InvariantFormat(displayContext).ShowWarning();
                    break;

                case FileOperationStatus.FileAlreadyExists:
                    Resources.FileAlreadyExists.InvariantFormat(displayContext).ShowWarning();
                    break;

                case FileOperationStatus.FileDoesNotExist:
                    Resources.FileDoesNotExist.InvariantFormat(displayContext).ShowWarning();
                    break;

                case FileOperationStatus.CannotWriteDestination:
                    Resources.CannotWrite.InvariantFormat(displayContext).ShowWarning();
                    break;

                case FileOperationStatus.CannotStartApplication:
                    Resources.CannotStartApplication.InvariantFormat(displayContext).ShowWarning();
                    break;

                case FileOperationStatus.InconsistentState:
                    Resources.InconsistentState.InvariantFormat(displayContext).ShowWarning();
                    break;

                case FileOperationStatus.InvalidKey:
                    Resources.InvalidKey.InvariantFormat(displayContext).ShowWarning();
                    break;

                case FileOperationStatus.Canceled:
                    break;

                case FileOperationStatus.Exception:
                    Resources.Exception.InvariantFormat(displayContext).ShowWarning();
                    break;

                case FileOperationStatus.InvalidPath:
                    Resources.InvalidPath.InvariantFormat(displayContext).ShowWarning();
                    break;

                default:
                    Resources.UnrecognizedError.InvariantFormat(displayContext).ShowWarning();
                    break;
            }
            return false;
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ClearPassphraseMemoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AxCryptFile.Wipe(FileSystemState.DefaultPathInfo, new ProgressContext());
            FactoryRegistry.Instance.Singleton<FileSystemState>(FileSystemState.Create(FileSystemState.DefaultPathInfo));
            FactoryRegistry.Instance.Singleton<KnownKeys>(new KnownKeys());
            ReStartSession();
        }

        private bool _handleSessionChangedInProgress = false;

        private void HandleSessionChangedEvent(object sender, SessionEventArgs e)
        {
            if (!_loaded)
            {
                return;
            }
            Instance.UIThread.RunOnUIThread(() => SessionChangedInternal(e));
        }

        private void SessionChangedInternal(SessionEventArgs e)
        {
            if (OS.Log.IsInfoEnabled)
            {
                OS.Log.LogInfo("Tick");                                 //MLHIDE
            }

            if (_handleSessionChangedInProgress)
            {
                OS.Current.NotifySessionChanged(new SessionEvent(SessionEventType.SessionChange));
                return;
            }
            _handleSessionChangedInProgress = true;

            Instance.BackgroundWork.Work(
                (IProgressContext progress) =>
                {
                    progress.NotifyLevelStart();
                    try
                    {
                        Instance.FileSystemState.Actions.HandleSessionEvents(e.SessionEvents, progress);
                    }
                    finally
                    {
                        progress.NotifyLevelFinished();
                    }
                    return FileOperationStatus.Success;
                },
                (FileOperationStatus status) =>
                {
                    _closeAndRemoveOpenFilesToolStripButton.Enabled = FilesAreOpen;
                    SetWindowTextWithLogonStatus();
                    _watchedFoldersPresentation.UpdateListView();
                    _handleSessionChangedInProgress = false;
                });
        }

        private static bool FilesAreOpen
        {
            get
            {
                IList<ActiveFile> openFiles = Instance.FileSystemState.DecryptedActiveFiles;
                return openFiles.Count > 0;
            }
        }

        #region ToolStrip

        private void MainToolStrip_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = GetDragDropEffects(sender, e);
        }

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

            if (Object.ReferenceEquals(button, _decryptToolStripButton))
            {
                _mainViewModel.DecryptFiles(e.GetDragged());
                return;
            }
            if (Object.ReferenceEquals(button, _openEncryptedToolStripButton))
            {
                _mainViewModel.OpenFiles(e.GetDragged());
                return;
            }
            if (Object.ReferenceEquals(button, _encryptToolStripButton))
            {
                _mainViewModel.EncryptFiles(e.GetDragged());
                return;
            }
        }

        private void MainToolStrip_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
        }

        #endregion ToolStrip

        private void RecentFilesListView_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if (!_loaded)
            {
                return;
            }
            _recentFilesPresentation.ChangeColumnWidth(e.ColumnIndex);
        }

        private void PurgeActiveFiles()
        {
            Instance.BackgroundWork.Work(
                (IProgressContext progress) =>
                {
                    progress.NotifyLevelStart();
                    try
                    {
                        Instance.FileSystemState.Actions.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, progress);
                        Instance.FileSystemState.Actions.PurgeActiveFiles(progress);
                    }
                    finally
                    {
                        progress.NotifyLevelFinished();
                    }

                    return FileOperationStatus.Success;
                },
                (FileOperationStatus status) =>
                {
                    if (!CheckStatusAndShowMessage(status, Resources.PurgingActiveFiles))
                    {
                        return;
                    }
                    IList<ActiveFile> openFiles = Instance.FileSystemState.DecryptedActiveFiles;
                    if (openFiles.Count == 0)
                    {
                        return;
                    }
                    StringBuilder sb = new StringBuilder();
                    foreach (ActiveFile openFile in openFiles)
                    {
                        sb.Append("{0}{1}".InvariantFormat(Path.GetFileName(openFile.DecryptedFileInfo.FullName), Environment.NewLine)); //MLHIDE
                    }
                    sb.ToString().ShowWarning();
                });
        }

        private void OpenFilesListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
        }

        private void RemoveRecentFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IEnumerable<string> encryptedPaths = SelectedRecentFilesItems();

            Instance.BackgroundWork.Work(
                (IProgressContext progress) =>
                {
                    Instance.FileSystemState.Actions.RemoveRecentFiles(encryptedPaths.Select(f => OS.Current.FileInfo(f)), progress);
                    return FileOperationStatus.Success;
                },
                (FileOperationStatus status) =>
                {
                });
        }

        private void DecryptAndRemoveFromListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IEnumerable<string> encryptedPaths = SelectedRecentFilesItems();
            _mainViewModel.DecryptFiles(SelectedRecentFilesItems());
        }

        private IEnumerable<string> SelectedRecentFilesItems()
        {
            IEnumerable<string> selected = _recentFilesListView.SelectedItems.Cast<ListViewItem>().Select((ListViewItem item) => { return item.SubItems["EncryptedPath"].Text; }).ToArray();
            return selected;
        }

        private void RecentFilesListView_MouseClick(object sender, MouseEventArgs e)
        {
            _recentFilesPresentation.ShowContextMenu(_recentFilesContextMenuStrip, e);
        }

        private void RecentFilesListView_DragOver(object sender, DragEventArgs e)
        {
            _recentFilesPresentation.StartDragAndDrop(e);
        }

        private void RecentFilesListView_DragDrop(object sender, DragEventArgs e)
        {
            _recentFilesPresentation.DropDragAndDrop(e);
        }

        private void RecentFilesListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            _recentFilesPresentation.ColumnClick(e.Column);
        }

        private void CloseOpenFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PurgeActiveFiles();
        }

        private void CloseAndRemoveOpenFilesToolStripButton_Click(object sender, EventArgs e)
        {
            PurgeActiveFiles();
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

        private void WipeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _fileOperationsPresentation.WipeFilesViaDialog();
        }

        private void AxCryptMainForm_Resize(object sender, EventArgs e)
        {
            if (_notifyIcon == null)
            {
                return;
            }

            _notifyIcon.Text = Resources.AxCryptFileEncryption;
            _notifyIcon.BalloonTipTitle = Resources.AxCryptFileEncryption;
            _notifyIcon.BalloonTipText = Resources.TrayBalloonTooltip;

            if (FormWindowState.Minimized == WindowState)
            {
                _notifyIcon.Visible = true;
                _notifyIcon.ShowBalloonTip(500);
                this.Hide();
            }
            else if (FormWindowState.Normal == WindowState)
            {
                _notifyIcon.Visible = false;
            }
        }

        private void AxCryptMainForm_ResizeEnd(object sender, EventArgs e)
        {
            if (FormWindowState.Normal == WindowState)
            {
                Instance.FileSystemState.Settings.MainWindowHeight = Height;
                Instance.FileSystemState.Settings.MainWindowWidth = Width;
                Instance.FileSystemState.Save();
            }
        }

        private void AxCryptMainForm_Move(object sender, EventArgs e)
        {
            if (!_loaded)
            {
                return;
            }
            if (FormWindowState.Normal == WindowState)
            {
                Instance.FileSystemState.Settings.MainWindowLocationX = Location.X;
                Instance.FileSystemState.Settings.MainWindowLocationY = Location.Y;
                Instance.FileSystemState.Save();
            }
        }

        private void TrayNotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void EnglishLanguageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetLanguage("en-US"); //MLHIDE
        }

        private void SwedishLanguageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetLanguage("sv-SE"); //MLHIDE
        }

        private static void SetLanguage(string cultureName)
        {
            Instance.FileSystemState.Settings.CultureName = cultureName;
            Instance.FileSystemState.Save();
            if (OS.Log.IsInfoEnabled)
            {
                OS.Log.LogInfo("Set new UI language culture to '{0}'.".InvariantFormat(Instance.FileSystemState.Settings.CultureName)); //MLHIDE
            }
            Resources.LanguageChangeRestartPrompt.ShowWarning();
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

        private void UpdateToolStripButton_Click(object sender, EventArgs e)
        {
            Instance.FileSystemState.Settings.LastUpdateCheckUtc = OS.Current.UtcNow;
            Instance.FileSystemState.Save();
            Process.Start(_updateUrl.ToString());
        }

        private void debugOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Instance.FileSystemState.Settings.DebugMode = !Instance.FileSystemState.Settings.DebugMode;
            Instance.FileSystemState.Save();
            UpdateDebugMode();
        }

        private void UpdateDebugMode()
        {
            _debugOptionsToolStripMenuItem.Checked = Instance.FileSystemState.Settings.DebugMode;
            _debugToolStripMenuItem.Visible = Instance.FileSystemState.Settings.DebugMode;
            if (Instance.FileSystemState.Settings.DebugMode)
            {
                OS.Log.SetLevel(LogLevel.Debug);
                if (_hiddenLogTabPage != null)
                {
                    _statusTabControl.TabPages.Add(_hiddenLogTabPage);
                }
                ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
                {
                    return true;
                };
            }
            else
            {
                ServicePointManager.ServerCertificateValidationCallback = null;
                OS.Log.SetLevel(LogLevel.Error);
                _hiddenLogTabPage = _logTabPage; //MLHIDE
                _statusTabControl.TabPages.Remove(_logTabPage);
            }
        }

        private void SetUpdateCheckUrlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (DebugOptionsDialog dialog = new DebugOptionsDialog())
            {
                dialog.UpdateCheckServiceUrl.Text = Instance.FileSystemState.Settings.AxCrypt2VersionCheckUrl.ToString();
                DialogResult result = dialog.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return;
                }
                Instance.FileSystemState.Settings.AxCrypt2VersionCheckUrl = new Uri(dialog.UpdateCheckServiceUrl.Text);
            }
        }

        private void CheckVersionNowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateCheck(DateTime.MinValue);
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (AboutBox aboutBox = new AboutBox())
            {
                aboutBox.ShowDialog();
            }
        }

        private void HelpToolStripButton_Click(object sender, EventArgs e)
        {
            Process.Start(Instance.FileSystemState.Settings.AxCrypt2HelpUrl.ToString());
        }

        private void ViewHelpMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Instance.FileSystemState.Settings.AxCrypt2HelpUrl.ToString());
        }

        #region Watched Folders

        private void WatchedFoldersListView_DragDrop(object sender, DragEventArgs e)
        {
            _watchedFoldersPresentation.DropDragAndDrop(e);
        }

        private void watchedFoldersListView_DragOver(object sender, DragEventArgs e)
        {
            _watchedFoldersPresentation.StartDragAndDrop(e);
        }

        private void WatchedFoldersListView_MouseClick(object sender, MouseEventArgs e)
        {
            _watchedFoldersPresentation.ShowContextMenu(_watchedFoldersContextMenuStrip, e);
        }

        private void WatchedFoldersListView_DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _watchedFoldersPresentation.RemoveSelectedWatchedFolders();
        }

        private void WatchedFoldersListView_OpenExplorerHereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _watchedFoldersPresentation.OpenSelectedFolder();
        }

        private void watchedFoldersListView_DecryptTemporarilyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string folder = _watchedFoldersListView.SelectedItems[0].Text;
            Instance.BackgroundWork.Work(
                (IProgressContext progress) =>
                {
                    progress.NotifyLevelStart();
                    try
                    {
                        _watchedFoldersPresentation.DecryptSelectedFolder(folder, progress);
                    }
                    finally
                    {
                        progress.NotifyLevelFinished();
                    }
                    return FileOperationStatus.Success;
                },
                (FileOperationStatus status) =>
                {
                });
        }

        #endregion Watched Folders

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
            FactoryRegistry.Instance.Clear();
        }
    }
}