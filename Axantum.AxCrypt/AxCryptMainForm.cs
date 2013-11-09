#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
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
        private Uri _updateUrl = Settings.Default.UpdateUrl;

        private TabPage _logTabPage = null;

        private NotifyIcon _notifyIcon = null;

        private string _title;

        private WatchedFolderPresentation _watchedFoldersPresentation;

        private RecentFilesPresentation _recentFilesPresentation;

        public static MessageBoxOptions MessageBoxOptions { get; private set; }

        public ListView WatchedFolders
        {
            get { return watchedFoldersListView; }
        }

        public ListView RecentFiles
        {
            get { return recentFilesListView; }
        }

        public TabControl Tabs
        {
            get { return statusTabControl; }
        }

        public IContainer Components
        {
            get { return components; }
        }

        public bool IsOnUIThread
        {
            get { return !InvokeRequired; }
        }

        public void BackgroundWorkWithProgress(Func<ProgressContext, FileOperationStatus> work, Action<FileOperationStatus> complete)
        {
            progressBackgroundWorker.BackgroundWorkWithProgress(work, complete);
        }

        public void RunOnUIThread(Action action)
        {
            BeginInvoke(action);
        }

        public AxCryptMainForm()
        {
            InitializeComponent();

            FactoryRegistry.Instance.Singleton<KnownKeys>(new KnownKeys());
            FactoryRegistry.Instance.Singleton<IUIThread>(this);
            FactoryRegistry.Instance.Singleton<IBackgroundWork>(this);
            FactoryRegistry.Instance.Singleton<IStatusChecker>(this);
            FactoryRegistry.Instance.Singleton<Background>(new Background());
            FactoryRegistry.Instance.Singleton<IRuntimeEnvironment>(new RuntimeEnvironment(this));
            FactoryRegistry.Instance.Singleton<FileSystemState>(new FileSystemState());

            _watchedFoldersPresentation = new WatchedFolderPresentation(this);
            _recentFilesPresentation = new RecentFilesPresentation(this);
        }

        private void AxCryptMainForm_Load(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            if (OS.Current.Platform == Platform.WindowsDesktop)
            {
                _notifyIcon = new NotifyIcon(components);
                _notifyIcon.MouseClick += trayNotifyIcon_MouseClick;
                _notifyIcon.Icon = Resources.axcrypticon;
                _notifyIcon.Visible = true;
            }

            Trace.Listeners.Add(new DelegateTraceListener("AxCryptMainFormListener", FormatTraceMessage)); //MLHIDE

            UpdateDebugMode();

            _title = "{0} {1}{2}".InvariantFormat(Application.ProductName, Application.ProductVersion, String.IsNullOrEmpty(AboutBox.AssemblyDescription) ? String.Empty : " " + AboutBox.AssemblyDescription); //MLHIDE

            MessageBoxOptions = RightToLeft == RightToLeft.Yes ? MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading : 0;

            SetupPathFilters();

            OS.Current.SessionChanged += HandleSessionChangedEvent;

            Instance.FileSystemState.Changed += new EventHandler<ActiveFileChangedEventArgs>(HandleFileSystemStateChangedEvent);
            Instance.FileSystemState.Load(FileSystemState.DefaultPathInfo);

            OS.Current.KeyWrapIterations = Instance.FileSystemState.KeyWrapIterations;
            OS.Current.ThumbprintSalt = Instance.FileSystemState.ThumbprintSalt;

            SetToolButtonsState();

            backgroundMonitor.UpdateCheck.VersionUpdate += new EventHandler<VersionEventArgs>(HandleVersionUpdateEvent);
            UpdateCheck(Settings.Default.LastUpdateCheckUtc);

            OS.Current.NotifySessionChanged(new SessionEvent(SessionEventType.SessionChange));
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
            if (Instance.KnownKeys.DefaultEncryptionKey == null)
            {
                logonStatus = Resources.LoggedOffStatusText;
            }
            else
            {
                PassphraseIdentity identity = Instance.FileSystemState.Identities.First(i => i.Thumbprint == Instance.KnownKeys.DefaultEncryptionKey.Thumbprint);
                logonStatus = Resources.LoggedOnStatusText.InvariantFormat(identity.Name);
            }
            string text = "{0} - {1}".InvariantFormat(_title, logonStatus);
            if (String.Compare(Text, text, StringComparison.Ordinal) != 0)
            {
                Text = text;
            }
        }

        private void FormatTraceMessage(string message)
        {
            Instance.Background.RunOnUIThread(() =>
            {
                string formatted = "{0} {1}".InvariantFormat(OS.Current.UtcNow.ToString("o", CultureInfo.InvariantCulture), message.TrimLogMessage()); //MLHIDE
                logOutputTextBox.AppendText(formatted);
            });
        }

        private void UpdateCheck(DateTime lastCheckUtc)
        {
            backgroundMonitor.UpdateCheck.CheckInBackground(lastCheckUtc, Settings.Default.NewestKnownVersion, Settings.Default.AxCrypt2VersionCheckUrl, Settings.Default.UpdateUrl);
        }

        private void UpdateVersionStatus(VersionUpdateStatus status, Version version)
        {
            switch (status)
            {
                case VersionUpdateStatus.IsUpToDateOrRecentlyChecked:
                    updateToolStripButton.ToolTipText = Axantum.AxCrypt.Properties.Resources.NoNeedToCheckForUpdatesTooltip;
                    updateToolStripButton.Enabled = false;
                    break;

                case VersionUpdateStatus.LongTimeSinceLastSuccessfulCheck:
                    updateToolStripButton.ToolTipText = Axantum.AxCrypt.Properties.Resources.OldVersionTooltip;
                    updateToolStripButton.Image = Resources.refreshred;
                    updateToolStripButton.Enabled = true;
                    break;

                case VersionUpdateStatus.NewerVersionIsAvailable:
                    updateToolStripButton.ToolTipText = Axantum.AxCrypt.Properties.Resources.NewVersionIsAvailableTooltip.InvariantFormat(version);
                    updateToolStripButton.Image = Resources.refreshred;
                    updateToolStripButton.Enabled = true;
                    break;

                case VersionUpdateStatus.ShortTimeSinceLastSuccessfulCheck:
                    updateToolStripButton.ToolTipText = Axantum.AxCrypt.Properties.Resources.ClickToCheckForNewerVersionTooltip;
                    updateToolStripButton.Image = Resources.refreshgreen;
                    updateToolStripButton.Enabled = true;
                    break;
            }
        }

        private void HandleVersionUpdateEvent(object sender, VersionEventArgs e)
        {
            Settings.Default.LastUpdateCheckUtc = OS.Current.UtcNow;
            Settings.Default.NewestKnownVersion = e.Version.ToString();
            Settings.Default.Save();
            _updateUrl = e.UpdateWebpageUrl;
            Instance.Background.RunOnUIThread(() =>
            {
                UpdateVersionStatus(e.VersionUpdateStatus, e.Version);
            });
        }

        private void HandleFileSystemStateChangedEvent(object sender, ActiveFileChangedEventArgs e)
        {
            Instance.Background.RunOnUIThread(() =>
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

            progressBackgroundWorker.WaitForBackgroundIdle();
            PurgeActiveFiles();
            progressBackgroundWorker.WaitForBackgroundIdle();
            Trace.Listeners.Remove("AxCryptMainFormListener");        //MLHIDE
        }

        private void SetToolButtonsState()
        {
            if (Instance.KnownKeys.DefaultEncryptionKey == null)
            {
                encryptionKeyToolStripButton.Image = Resources.encryptionkeygreen32;
                encryptionKeyToolStripButton.ToolTipText = Resources.NoDefaultEncryptionKeySetToolTip;
            }
            else
            {
                encryptionKeyToolStripButton.Image = Resources.encryptionkeyred32;
                encryptionKeyToolStripButton.ToolTipText = Resources.DefaultEncryptionKeyIsIsetToolTip;
            }
            SetWindowTextWithLogonStatus();
        }

        private void toolStripButtonEncrypt_Click(object sender, EventArgs e)
        {
            EncryptFilesViaDialog();
        }

        private void EncryptFilesViaDialog()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = Resources.EncryptFileOpenDialogTitle;
                ofd.Multiselect = true;
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;
                DialogResult result = ofd.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return;
                }
                Instance.Background.ProcessFiles(ofd.FileNames, EncryptFile);
            }
        }

        private void EncryptFile(string file, IThreadWorker worker, ProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(Instance.FileSystemState, progress);

            operationsController.QuerySaveFileAs += (object sender, FileOperationEventArgs e) =>
                {
                    using (SaveFileDialog sfd = new SaveFileDialog())
                    {
                        sfd.Title = Resources.EncryptFileSaveAsDialogTitle;
                        sfd.AddExtension = true;
                        sfd.ValidateNames = true;
                        sfd.CheckPathExists = true;
                        sfd.DefaultExt = OS.Current.AxCryptExtension;
                        sfd.FileName = e.SaveFileFullName;
                        sfd.Filter = Resources.EncryptedFileDialogFilterPattern.InvariantFormat(OS.Current.AxCryptExtension);
                        sfd.InitialDirectory = Path.GetDirectoryName(e.SaveFileFullName);
                        sfd.ValidateNames = true;
                        DialogResult saveAsResult = sfd.ShowDialog();
                        if (saveAsResult != DialogResult.OK)
                        {
                            e.Cancel = true;
                            return;
                        }
                        e.SaveFileFullName = sfd.FileName;
                    }
                };

            operationsController.QueryEncryptionPassphrase += (object sender, FileOperationEventArgs e) =>
                {
                    string passphrase = AskForLogOnPassphrase();
                    if (String.IsNullOrEmpty(passphrase))
                    {
                        e.Cancel = true;
                        return;
                    }
                    e.Passphrase = passphrase;
                };

            operationsController.Completed += (object sender, FileOperationEventArgs e) =>
                {
                    if (e.Status == FileOperationStatus.FileAlreadyEncrypted)
                    {
                        e.Status = FileOperationStatus.Success;
                        return;
                    }
                    if (CheckStatusAndShowMessage(e.Status, e.OpenFileFullName))
                    {
                        IRuntimeFileInfo encryptedInfo = OS.Current.FileInfo(e.SaveFileFullName);
                        IRuntimeFileInfo decryptedInfo = OS.Current.FileInfo(FileOperation.GetTemporaryDestinationName(e.OpenFileFullName));
                        ActiveFile activeFile = new ActiveFile(encryptedInfo, decryptedInfo, e.Key, ActiveFileStatus.NotDecrypted, null);
                        Instance.FileSystemState.Add(activeFile);
                        Instance.FileSystemState.Save();
                    }
                };

            operationsController.EncryptFile(file, worker);
        }

        private string AskForDecryptPassphrase()
        {
            using (DecryptPassphraseDialog passphraseDialog = new DecryptPassphraseDialog())
            {
                passphraseDialog.ShowPassphraseCheckBox.Checked = Settings.Default.ShowDecryptPassphrase;
                DialogResult dialogResult = passphraseDialog.ShowDialog(this);
                if (dialogResult != DialogResult.OK)
                {
                    return null;
                }
                if (passphraseDialog.ShowPassphraseCheckBox.Checked != Settings.Default.ShowDecryptPassphrase)
                {
                    Settings.Default.ShowDecryptPassphrase = passphraseDialog.ShowPassphraseCheckBox.Checked;
                    Settings.Default.Save();
                }
                return passphraseDialog.Passphrase.Text;
            }
        }

        private string AskForLogOnPassphrase()
        {
            string passphrase = AskForLogOnOrEncryptionPassphrase();
            if (passphrase.Length == 0)
            {
                return String.Empty;
            }

            Instance.KnownKeys.DefaultEncryptionKey = Passphrase.Derive(passphrase);
            return passphrase;
        }

        private string AskForLogOnOrEncryptionPassphrase()
        {
            using (LogOnDialog logOnDialog = new LogOnDialog(Instance.FileSystemState))
            {
                logOnDialog.ShowPassphraseCheckBox.Checked = Settings.Default.ShowEncryptPasshrase;
                DialogResult dialogResult = logOnDialog.ShowDialog();
                if (dialogResult == DialogResult.Retry)
                {
                    return AskForNewEncryptionPassphrase();
                }

                if (dialogResult != DialogResult.OK || logOnDialog.PassphraseTextBox.Text.Length == 0)
                {
                    return String.Empty;
                }

                if (logOnDialog.ShowPassphraseCheckBox.Checked != Settings.Default.ShowEncryptPasshrase)
                {
                    Settings.Default.ShowEncryptPasshrase = logOnDialog.ShowPassphraseCheckBox.Checked;
                    Settings.Default.Save();
                }
                return logOnDialog.PassphraseTextBox.Text;
            }
        }

        private string AskForNewEncryptionPassphrase()
        {
            using (EncryptPassphraseDialog passphraseDialog = new EncryptPassphraseDialog(Instance.FileSystemState))
            {
                passphraseDialog.ShowPassphraseCheckBox.Checked = Settings.Default.ShowEncryptPasshrase;
                DialogResult dialogResult = passphraseDialog.ShowDialog();
                if (dialogResult != DialogResult.OK || passphraseDialog.PassphraseTextBox.Text.Length == 0)
                {
                    return String.Empty;
                }

                if (passphraseDialog.ShowPassphraseCheckBox.Checked != Settings.Default.ShowEncryptPasshrase)
                {
                    Settings.Default.ShowEncryptPasshrase = passphraseDialog.ShowPassphraseCheckBox.Checked;
                    Settings.Default.Save();
                }

                Passphrase passphrase = new Passphrase(passphraseDialog.PassphraseTextBox.Text);
                PassphraseIdentity identity = Instance.FileSystemState.Identities.FirstOrDefault(i => i.Thumbprint == passphrase.DerivedPassphrase.Thumbprint);
                if (identity != null)
                {
                    return passphraseDialog.PassphraseTextBox.Text;
                }

                identity = new PassphraseIdentity(passphraseDialog.NameTextBox.Text, passphrase.DerivedPassphrase);
                Instance.FileSystemState.Identities.Add(identity);
                Instance.FileSystemState.Save();

                return passphraseDialog.PassphraseTextBox.Text;
            }
        }

        private void DecryptFilesViaDialog()
        {
            string[] fileNames;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = Resources.DecryptFileOpenDialogTitle;
                ofd.Multiselect = true;
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;
                ofd.DefaultExt = OS.Current.AxCryptExtension;
                ofd.Filter = Resources.EncryptedFileDialogFilterPattern.InvariantFormat("{0}".InvariantFormat(OS.Current.AxCryptExtension)); //MLHIDE
                ofd.Multiselect = true;
                DialogResult result = ofd.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return;
                }
                fileNames = ofd.FileNames;
            }
            Instance.Background.ProcessFiles(fileNames, DecryptFile);
        }

        private void DecryptFile(string file, IThreadWorker worker, ProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(Instance.FileSystemState, progress);

            operationsController.QueryDecryptionPassphrase += HandleQueryDecryptionPassphraseEvent;

            operationsController.QuerySaveFileAs += (object sender, FileOperationEventArgs e) =>
                {
                    string extension = Path.GetExtension(e.SaveFileFullName);
                    using (SaveFileDialog sfd = new SaveFileDialog())
                    {
                        sfd.AddExtension = !String.IsNullOrEmpty(extension);
                        sfd.CheckPathExists = true;
                        sfd.DefaultExt = extension;
                        sfd.Filter = Resources.DecryptedSaveAsFileDialogFilterPattern.InvariantFormat(extension);
                        sfd.InitialDirectory = Path.GetDirectoryName(file);
                        sfd.FileName = Path.GetFileName(e.SaveFileFullName);
                        sfd.OverwritePrompt = true;
                        sfd.RestoreDirectory = true;
                        sfd.Title = Resources.DecryptedSaveAsFileDialogTitle;
                        DialogResult result = sfd.ShowDialog();
                        if (result != DialogResult.OK)
                        {
                            e.Cancel = true;
                            return;
                        }
                        e.SaveFileFullName = sfd.FileName;
                    }
                    return;
                };

            operationsController.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
                {
                    Instance.KnownKeys.Add(e.Key);
                };

            operationsController.Completed += (object sender, FileOperationEventArgs e) =>
                {
                    if (CheckStatusAndShowMessage(e.Status, e.OpenFileFullName))
                    {
                        Instance.FileSystemState.Actions.RemoveRecentFiles(new string[] { e.OpenFileFullName }, progress);
                    }
                };

            operationsController.DecryptFile(file, worker);
        }

        private void WipeFilesViaDialog()
        {
            string[] fileNames;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = Resources.WipeFileSelectFileDialogTitle;
                ofd.Multiselect = true;
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;
                DialogResult result = ofd.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return;
                }
                fileNames = ofd.FileNames;
            }
            Instance.Background.ProcessFiles(fileNames, WipeFile);
        }

        private void WipeFile(string file, IThreadWorker worker, ProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(Instance.FileSystemState, progress);

            operationsController.WipeQueryConfirmation += (object sender, FileOperationEventArgs e) =>
            {
                using (ConfirmWipeDialog cwd = new ConfirmWipeDialog())
                {
                    cwd.FileNameLabel.Text = Path.GetFileName(file);
                    DialogResult confirmResult = cwd.ShowDialog();
                    e.ConfirmAll = cwd.ConfirmAllCheckBox.Checked;
                    if (confirmResult == DialogResult.Yes)
                    {
                        e.Skip = false;
                    }
                    if (confirmResult == DialogResult.No)
                    {
                        e.Skip = true;
                    }
                    if (confirmResult == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                    }
                }
            };

            operationsController.Completed += (object sender, FileOperationEventArgs e) =>
            {
                if (CheckStatusAndShowMessage(e.Status, e.OpenFileFullName))
                {
                    if (!e.Skip)
                    {
                        Instance.FileSystemState.Actions.RemoveRecentFiles(new string[] { e.SaveFileFullName }, progress);
                    }
                }
            };

            operationsController.WipeFile(file, worker);
        }

        private void OpenFilesViaDialog()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = Resources.OpenEncryptedFileOpenDialogTitle;
                ofd.Multiselect = false;
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;
                ofd.DefaultExt = OS.Current.AxCryptExtension;
                ofd.Filter = Resources.EncryptedFileDialogFilterPattern.InvariantFormat("{0}".InvariantFormat(OS.Current.AxCryptExtension)); //MLHIDE
                DialogResult result = ofd.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return;
                }

                Instance.Background.ProcessFiles(ofd.FileNames, OpenEncrypted);
            }
        }

        private void OpenEncrypted(string file, IThreadWorker worker, ProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(Instance.FileSystemState, progress);

            operationsController.QueryDecryptionPassphrase += HandleQueryDecryptionPassphraseEvent;

            operationsController.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
                {
                    Instance.KnownKeys.Add(e.Key);
                };

            operationsController.Completed += (object sender, FileOperationEventArgs e) =>
                {
                    if (e.Status == FileOperationStatus.Canceled)
                    {
                        return;
                    }
                    CheckStatusAndShowMessage(e.Status, e.OpenFileFullName);
                };

            operationsController.DecryptAndLaunch(file, worker);
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

        private void openEncryptedToolStripButton_Click(object sender, EventArgs e)
        {
            OpenFilesViaDialog();
        }

        private void HandleQueryDecryptionPassphraseEvent(object sender, FileOperationEventArgs e)
        {
            string passphraseText = AskForDecryptPassphrase();
            if (passphraseText == null)
            {
                e.Cancel = true;
                return;
            }
            e.Passphrase = passphraseText;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private bool _handleSessionChangedInProgress = false;

        private void HandleSessionChangedEvent(object sender, SessionEventArgs e)
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

            BackgroundWorkWithProgress(
                (ProgressContext progress) =>
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
                    closeAndRemoveOpenFilesToolStripButton.Enabled = FilesAreOpen;
                    SetToolButtonsState();
                    _watchedFoldersPresentation.UpdateListView();
                    _handleSessionChangedInProgress = false;
                });
        }

        private bool FilesAreOpen
        {
            get
            {
                IList<ActiveFile> openFiles = Instance.FileSystemState.DecryptedActiveFiles;
                return openFiles.Count > 0;
            }
        }

        private void decryptToolStripButton_Click(object sender, EventArgs e)
        {
            DecryptFilesViaDialog();
        }

        private void openEncryptedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFilesViaDialog();
        }

        private void recentFilesListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Instance.Background.ProcessFiles(new string[] { _recentFilesPresentation.SelectedEncryptedPath }, OpenEncrypted);
        }

        private void recentFilesListView_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            _recentFilesPresentation.ChangeColumnWidth(e.ColumnIndex);
        }

        private void PurgeActiveFiles()
        {
            BackgroundWorkWithProgress(
                (ProgressContext progress) =>
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

        private void openFilesListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
        }

        private void removeRecentFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IEnumerable<string> encryptedPaths = SelectedRecentFilesItems();

            BackgroundWorkWithProgress(
                (ProgressContext progress) =>
                {
                    Instance.FileSystemState.Actions.RemoveRecentFiles(encryptedPaths, progress);
                    return FileOperationStatus.Success;
                },
                (FileOperationStatus status) =>
                {
                });
        }

        private void decryptAndRemoveFromListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IEnumerable<string> encryptedPaths = SelectedRecentFilesItems();

            Instance.Background.ProcessFiles(encryptedPaths, DecryptFile);
        }

        private IEnumerable<string> SelectedRecentFilesItems()
        {
            IEnumerable<string> selected = recentFilesListView.SelectedItems.Cast<ListViewItem>().Select((ListViewItem item) => { return item.SubItems["EncryptedPath"].Text; }).ToArray();
            return selected;
        }

        private void recentFilesListView_MouseClick(object sender, MouseEventArgs e)
        {
            _recentFilesPresentation.ShowContextMenu(recentFilesContextMenuStrip, e);
        }

        private void recentFilesListView_DragOver(object sender, DragEventArgs e)
        {
            _recentFilesPresentation.StartDragAndDrop(e);
        }

        private void recentFilesListView_DragDrop(object sender, DragEventArgs e)
        {
            _recentFilesPresentation.DropDragAndDrop(e);
        }

        private void closeOpenFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PurgeActiveFiles();
        }

        private void closeAndRemoveOpenFilesToolStripButton_Click(object sender, EventArgs e)
        {
            PurgeActiveFiles();
        }

        private void openFilesListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }
            ListView recentFiles = (ListView)sender;
            openFilesContextMenuStrip.Show(recentFiles, e.Location);
        }

        private void enterPassphraseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string passphraseText = AskForDecryptPassphrase();
            if (passphraseText == null)
            {
                return;
            }
            Instance.KnownKeys.Add(Passphrase.Derive(passphraseText));
        }

        private void progressBackgroundWorker_ProgressBarClicked(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }
            progressContextMenuStrip.Tag = sender;
            progressContextMenuStrip.Show((Control)sender, e.Location);
        }

        private void progressBackgroundWorker_ProgressBarCreated(object sender, ControlEventArgs e)
        {
            progressTableLayoutPanel.Controls.Add(e.Control);
        }

        private void progressContextCancelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
            ContextMenuStrip menuStrip = (ContextMenuStrip)menuItem.GetCurrentParent();
            ProgressBar progressBar = (ProgressBar)menuStrip.Tag;
            ProgressContext progress = (ProgressContext)progressBar.Tag;
            progress.Cancel = true;
        }

        private void encryptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EncryptFilesViaDialog();
        }

        private void decryptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DecryptFilesViaDialog();
        }

        private void wipeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WipeFilesViaDialog();
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

            if (FormWindowState.Minimized == this.WindowState)
            {
                _notifyIcon.Visible = true;
                _notifyIcon.ShowBalloonTip(500);
                this.Hide();
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                _notifyIcon.Visible = false;
            }
        }

        private void trayNotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void englishLanguageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetLanguage("en-US"); //MLHIDE
        }

        private void swedishLanguageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetLanguage("sv-SE"); //MLHIDE
        }

        private static void SetLanguage(string cultureName)
        {
            Settings.Default.Language = cultureName;
            Settings.Default.Save();
            if (OS.Log.IsInfoEnabled)
            {
                OS.Log.LogInfo("Set new UI language culture to '{0}'.".InvariantFormat(Settings.Default.Language)); //MLHIDE
            }
            Resources.LanguageChangeRestartPrompt.ShowWarning();
        }

        private void languageToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
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

        private void updateToolStripButton_Click(object sender, EventArgs e)
        {
            Settings.Default.LastUpdateCheckUtc = OS.Current.UtcNow;
            Settings.Default.Save();
            Process.Start(_updateUrl.ToString());
        }

        private void debugOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.Debug = !Settings.Default.Debug;
            Settings.Default.Save();
            UpdateDebugMode();
        }

        private void UpdateDebugMode()
        {
            debugOptionsToolStripMenuItem.Checked = Settings.Default.Debug;
            debugToolStripMenuItem.Visible = Settings.Default.Debug;
            if (Settings.Default.Debug)
            {
                OS.Log.SetLevel(LogLevel.Debug);
                if (_logTabPage != null)
                {
                    statusTabControl.TabPages.Add(_logTabPage);
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
                _logTabPage = statusTabControl.TabPages["logTabPage"]; //MLHIDE
                statusTabControl.TabPages.Remove(_logTabPage);
            }
        }

        private void setUpdateCheckUrlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (DebugOptionsDialog dialog = new DebugOptionsDialog())
            {
                dialog.UpdateCheckServiceUrl.Text = Settings.Default.AxCrypt2VersionCheckUrl.ToString();
                DialogResult result = dialog.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return;
                }
                Settings.Default.AxCrypt2VersionCheckUrl = new Uri(dialog.UpdateCheckServiceUrl.Text);
            }
        }

        private void checkVersionNowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateCheck(DateTime.MinValue);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (AboutBox aboutBox = new AboutBox())
            {
                aboutBox.ShowDialog();
            }
        }

        private void helpToolStripButton_Click(object sender, EventArgs e)
        {
            Process.Start(Settings.Default.AxCrypt2HelpUrl.ToString());
        }

        private void viewHelpMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Settings.Default.AxCrypt2HelpUrl.ToString());
        }

        private void LogOnToolStripButton_Click(object sender, EventArgs e)
        {
            if (Instance.KnownKeys.IsLoggedOn)
            {
                Instance.KnownKeys.Clear();
                return;
            }

            if (Instance.FileSystemState.Identities.Any(identity => true))
            {
                TryLogOnToExistingIdentity();
            }
            else
            {
                string passphrase = AskForNewEncryptionPassphrase();
                if (String.IsNullOrEmpty(passphrase))
                {
                    return;
                }

                Instance.KnownKeys.DefaultEncryptionKey = Passphrase.Derive(passphrase);
            }
            SetToolButtonsState();
        }

        private void TryLogOnToExistingIdentity()
        {
            string passphrase = AskForLogOnPassphrase();
            if (String.IsNullOrEmpty(passphrase))
            {
                return;
            }
        }

        #region Watched Folders

        private void watchedFoldersListView_DragDrop(object sender, DragEventArgs e)
        {
            _watchedFoldersPresentation.DropDragAndDrop(e);
        }

        private void watchedFoldersListView_DragOver(object sender, DragEventArgs e)
        {
            _watchedFoldersPresentation.StartDragAndDrop(e);
        }

        private void watchedFoldersListView_MouseClick(object sender, MouseEventArgs e)
        {
            _watchedFoldersPresentation.ShowContextMenu(watchedFoldersContextMenuStrip, e);
        }

        private void watchedFoldersListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            _watchedFoldersPresentation.OpenSelectedFolder();
        }

        private void watchedFoldersListView_deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _watchedFoldersPresentation.RemoveSelectedWatchedFolders();
        }

        private void watchedFoldersListView_decryptTemporarilyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string folder = watchedFoldersListView.SelectedItems[0].Text;
            BackgroundWorkWithProgress(
                (ProgressContext progress) =>
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