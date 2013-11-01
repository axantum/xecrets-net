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
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Mono;
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
using System.Threading;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    /// <summary>
    /// All code here is expected to execute on the GUI thread. If code may be called on another thread, this call
    /// must be made through ThreadSafeUi() .
    /// </summary>
    public partial class AxCryptMainForm : Form, IMainView
    {
        private class RecentFilesByDateComparer : IComparer
        {
            #region IComparer Members

            public int Compare(object x, object y)
            {
                ListViewItem item1 = (ListViewItem)x;
                ListViewItem item2 = (ListViewItem)y;
                DateTime dateTime1 = (DateTime)item1.SubItems["Date"].Tag;
                DateTime dateTime2 = (DateTime)item2.SubItems["Date"].Tag;
                return dateTime2.CompareTo(dateTime1);
            }

            #endregion IComparer Members
        }

        private Uri _updateUrl = Settings.Default.UpdateUrl;

        private TabPage _logTabPage = null;

        private NotifyIcon _notifyIcon = null;

        private string _title;

        private WatchedFoldersCore _watchedFoldersCore;

        public static MessageBoxOptions MessageBoxOptions { get; private set; }

        public FileSystemState FileSystemState
        {
            get { return persistentState.Current; }
        }

        public ListView WatchedFolders
        {
            get { return watchedFoldersListView; }
        }

        public TabControl Tabs
        {
            get { return statusTabControl; }
        }

        public AxCryptMainForm()
        {
            InitializeComponent();
        }

        private IComparer _currentRecentFilesSorter = new RecentFilesByDateComparer();

        private void AxCryptMainForm_Load(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            OS.Current = new RuntimeEnvironment(this);
            if (OS.Current.Platform == Platform.WindowsDesktop)
            {
                _notifyIcon = new NotifyIcon(components);
                _notifyIcon.MouseClick += trayNotifyIcon_MouseClick;
                _notifyIcon.Icon = Resources.axcrypticon;
                _notifyIcon.Visible = true;
            }

            Trace.Listeners.Add(new DelegateTraceListener("AxCryptMainFormListener", FormatTraceMessage)); //MLHIDE

            RestoreUserPreferences();

            _title = "{0} {1}{2}".InvariantFormat(Application.ProductName, Application.ProductVersion, String.IsNullOrEmpty(AboutBox.AssemblyDescription) ? String.Empty : " " + AboutBox.AssemblyDescription); //MLHIDE

            MessageBoxOptions = RightToLeft == RightToLeft.Yes ? MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading : 0;

            _watchedFoldersCore = new WatchedFoldersCore(this);

            recentFilesListView.SmallImageList = CreateSmallImageListToAvoidLocalizationIssuesWithDesignerAndResources(components);
            recentFilesListView.LargeImageList = CreateLargeImageListToAvoidLocalizationIssuesWithDesignerAndResources(components);
            recentFilesListView.ListViewItemSorter = _currentRecentFilesSorter;

            OS.Current.SessionChanged += HandleWorkFolderStateChangedEvent;

            persistentState.Current.Changed += new EventHandler<ActiveFileChangedEventArgs>(HandleFileSystemStateChangedEvent);
            persistentState.Current.Load(FileSystemState.DefaultPathInfo);
            persistentState.Current.KnownKeys.Changed += new EventHandler<EventArgs>(HandleKnownKeysChangedEvent);

            OS.Current.KeyWrapIterations = persistentState.Current.KeyWrapIterations;
            OS.Current.ThumbprintSalt = persistentState.Current.ThumbprintSalt;

            SetToolButtonsState();

            backgroundMonitor.UpdateCheck.VersionUpdate += new EventHandler<VersionEventArgs>(HandleVersionUpdateEvent);
            UpdateCheck(Settings.Default.LastUpdateCheckUtc);

            OS.Current.NotifySessionChanged(new SessionEvent(SessionEventType.SessionChange));
        }

        private void HandleKnownKeysChangedEvent(object sender, EventArgs e)
        {
            OS.Current.NotifySessionChanged(new SessionEvent(SessionEventType.KnownKeyChange));
        }

        private void SetWindowTextWithLogonStatus()
        {
            string logonStatus;
            if (persistentState.Current.KnownKeys.DefaultEncryptionKey == null)
            {
                logonStatus = Resources.LoggedOffStatusText;
            }
            else
            {
                logonStatus = Resources.LoggedOnStatusText;
            }
            string text = "{0} - {1}".InvariantFormat(_title, logonStatus);
            if (String.Compare(Text, text, StringComparison.Ordinal) != 0)
            {
                Text = text;
            }
        }

        private static ImageList CreateSmallImageListToAvoidLocalizationIssuesWithDesignerAndResources(IContainer container)
        {
            ImageList smallImageList = new ImageList(container);

            smallImageList.Images.Add("ActiveFile", Resources.activefilegreen16);
            smallImageList.Images.Add("InactiveFile", Resources.inactivefilegreen16);
            smallImageList.Images.Add("Exclamation", Resources.exclamationgreen16);
            smallImageList.Images.Add("DecryptedFile", Resources.decryptedfilered16);
            smallImageList.Images.Add("DecryptedUnknownKeyFile", Resources.decryptedunknownkeyfilered16);
            smallImageList.Images.Add("ActiveFileKnownKey", Resources.fileknownkeygreen16);
            smallImageList.TransparentColor = System.Drawing.Color.Transparent;

            return smallImageList;
        }

        private static ImageList CreateLargeImageListToAvoidLocalizationIssuesWithDesignerAndResources(IContainer container)
        {
            ImageList largeImageList = new ImageList(container);

            largeImageList.Images.Add("ActiveFile", Resources.opendocument32);
            largeImageList.Images.Add("InactiveFile", Resources.helpquestiongreen32);
            largeImageList.Images.Add("Exclamation", Resources.exclamationgreen32);
            largeImageList.TransparentColor = System.Drawing.Color.Transparent;

            return largeImageList;
        }

        private void FormatTraceMessage(string message)
        {
            ThreadSafeUi(() =>
            {
                string formatted = "{0} {1}".InvariantFormat(OS.Current.UtcNow.ToString("o", CultureInfo.InvariantCulture), message.TrimLogMessage()); //MLHIDE
                logOutputTextBox.AppendText(formatted);
            });
        }

        private void ThreadSafeUi(Action action)
        {
            if (InvokeRequired)
            {
                BeginInvoke(action);
            }
            else
            {
                action();
            }
        }

        private void InteractionSafeUi(Action action)
        {
            if (InvokeRequired)
            {
                _interactionSemaphore.WaitOne();
                Action extendedAction = () =>
                    {
                        try
                        {
                            action();
                        }
                        finally
                        {
                            _interactionSemaphore.Release();
                        }
                    };
                BeginInvoke(extendedAction);
            }
            else
            {
                action();
            }
        }

        private Semaphore _interactionSemaphore = new Semaphore(1, 1);

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
            ThreadSafeUi(() =>
            {
                UpdateVersionStatus(e.VersionUpdateStatus, e.Version);
            });
        }

        private void HandleFileSystemStateChangedEvent(object sender, ActiveFileChangedEventArgs e)
        {
            ThreadSafeUi(() =>
            {
                UpdateActiveFilesViews(e.ActiveFile);
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

        private void RestoreUserPreferences()
        {
            recentFilesListView.Columns[0].Name = "DecryptedFile";    //MLHIDE
            recentFilesListView.Columns[0].Width = Settings.Default.RecentFilesDocumentWidth > 0 ? Settings.Default.RecentFilesDocumentWidth : recentFilesListView.Columns[0].Width;

            UpdateDebugMode();
        }

        private void SetToolButtonsState()
        {
            if (persistentState.Current.KnownKeys.DefaultEncryptionKey == null)
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

        private void UpdateActiveFilesViews(ActiveFile activeFile)
        {
            if (activeFile.Status.HasMask(ActiveFileStatus.NoLongerActive))
            {
                recentFilesListView.Items.RemoveByKey(activeFile.EncryptedFileInfo.FullName);
                return;
            }

            UpdateRecentFilesListView(activeFile);
        }

        private void UpdateRecentFilesListView(ActiveFile activeFile)
        {
            recentFilesListView.ListViewItemSorter = null;
            ListViewItem item = recentFilesListView.Items[activeFile.EncryptedFileInfo.FullName];
            if (item == null)
            {
                string text = Path.GetFileName(activeFile.DecryptedFileInfo.FullName);
                item = recentFilesListView.Items.Add(text);
                item.Name = activeFile.EncryptedFileInfo.FullName;

                ListViewItem.ListViewSubItem dateColumn = item.SubItems.Add(String.Empty);
                dateColumn.Name = "Date"; //MLHIDE

                ListViewItem.ListViewSubItem encryptedPathColumn = item.SubItems.Add(String.Empty);
                encryptedPathColumn.Name = "EncryptedPath"; //MLHIDE
            }

            UpdateListViewItem(item, activeFile);
            recentFilesListView.ListViewItemSorter = _currentRecentFilesSorter;
            while (recentFilesListView.Items.Count > Settings.Default.MaxNumberRecentFiles)
            {
                recentFilesListView.Items.RemoveAt(recentFilesListView.Items.Count - 1);
            }
        }

        private static void UpdateListViewItem(ListViewItem item, ActiveFile activeFile)
        {
            UpdateStatusDependentPropertiesOfListViewItem(item, activeFile);

            item.SubItems["EncryptedPath"].Text = activeFile.EncryptedFileInfo.FullName;
            item.SubItems["Date"].Text = activeFile.LastActivityTimeUtc.ToLocalTime().ToString(CultureInfo.CurrentCulture);
            item.SubItems["Date"].Tag = activeFile.LastActivityTimeUtc;
        }

        private static void UpdateStatusDependentPropertiesOfListViewItem(ListViewItem item, ActiveFile activeFile)
        {
            item.ImageKey = ImageKeyFromActiveFileStatus(activeFile);
            switch (item.ImageKey)
            {
                case "DecryptedFile":
                    item.ToolTipText = Resources.DecryptedFileToolTip;
                    break;

                case "DecryptedUnknownKeyFile":
                    item.ToolTipText = Resources.DecryptedUnknownKeyFileToolTip;
                    break;

                case "InactiveFile":
                    item.ToolTipText = Resources.InactiveFileToolTip;
                    break;

                case "ActiveFile":
                    item.ToolTipText = Resources.ActiveFileToolTip;
                    break;

                case "ActiveFileKnownKey":
                    item.ToolTipText = Resources.ActiveFileKnownKeyToolTip;
                    break;

                default:
                    item.ToolTipText = String.Empty;
                    break;
            }
        }

        private static string ImageKeyFromActiveFileStatus(ActiveFile activeFile)
        {
            if (activeFile.Status.HasMask(ActiveFileStatus.DecryptedIsPendingDelete))
            {
                return activeFile.Key != null ? "DecryptedFile" : "DecryptedUnknownKeyFile"; //MLHIDE
            }
            if (activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted))
            {
                return activeFile.Key != null ? "DecryptedFile" : "DecryptedUnknownKeyFile"; //MLHIDE
            }
            if (activeFile.Status.HasMask(ActiveFileStatus.NotDecrypted))
            {
                if (String.IsNullOrEmpty(activeFile.DecryptedFileInfo.FullName))
                {
                    return "InactiveFile"; //MLHIDE
                }
                return activeFile.Key != null ? "ActiveFileKnownKey" : "ActiveFile"; //MLHIDE
            }
            return String.Empty;
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
                ProcessFilesInBackground(ofd.FileNames, EncryptFile);
            }
        }

        private void EncryptFile(string file, IThreadWorker worker, ProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(persistentState.Current, progress);

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
                    string passphrase = AskForEncryptionPassphrase();
                    if (String.IsNullOrEmpty(passphrase))
                    {
                        e.Cancel = true;
                        return;
                    }
                    e.Passphrase = passphrase;
                    AesKey defaultEncryptionKey = new Passphrase(e.Passphrase).DerivedPassphrase;
                    persistentState.Current.KnownKeys.DefaultEncryptionKey = defaultEncryptionKey;
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
                        persistentState.Current.Add(activeFile);
                        persistentState.Current.Save();
                    }
                };

            operationsController.EncryptFile(file, worker);
        }

        private static string AskForEncryptionPassphrase()
        {
            using (EncryptPassphraseDialog passphraseDialog = new EncryptPassphraseDialog())
            {
                passphraseDialog.ShowPassphraseCheckBox.Checked = Settings.Default.ShowEncryptPasshrase;
                DialogResult dialogResult = passphraseDialog.ShowDialog();
                if (dialogResult != DialogResult.OK)
                {
                    return String.Empty;
                }
                if (passphraseDialog.ShowPassphraseCheckBox.Checked != Settings.Default.ShowEncryptPasshrase)
                {
                    Settings.Default.ShowEncryptPasshrase = passphraseDialog.ShowPassphraseCheckBox.Checked;
                    Settings.Default.Save();
                }
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
            ProcessFilesInBackground(fileNames, DecryptFile);
        }

        private void DecryptFile(string file, IThreadWorker worker, ProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(persistentState.Current, progress);

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
                    persistentState.Current.KnownKeys.Add(e.Key);
                };

            operationsController.Completed += (object sender, FileOperationEventArgs e) =>
                {
                    if (CheckStatusAndShowMessage(e.Status, e.OpenFileFullName))
                    {
                        persistentState.Current.RemoveRecentFiles(new string[] { e.OpenFileFullName }, progress);
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
            ProcessFilesInBackground(fileNames, WipeFile);
        }

        private void WipeFile(string file, IThreadWorker worker, ProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(persistentState.Current, progress);

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
                        persistentState.Current.RemoveRecentFiles(new string[] { e.SaveFileFullName }, progress);
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

                ProcessFilesInBackground(ofd.FileNames, OpenEncrypted);
            }
        }

        private void OpenEncrypted(string file, IThreadWorker worker, ProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(persistentState.Current, progress);

            operationsController.QueryDecryptionPassphrase += HandleQueryDecryptionPassphraseEvent;

            operationsController.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
                {
                    persistentState.Current.KnownKeys.Add(e.Key);
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

        private void ProcessFilesInBackground(IEnumerable<string> files, Action<string, IThreadWorker, ProgressContext> processFile)
        {
            WorkerGroup workerGroup = null;
            int maxConcurrency = Environment.ProcessorCount > 2 ? Environment.ProcessorCount - 1 : 2;
            progressBackgroundWorker.BackgroundWorkWithProgress(
                (ProgressContext progress) =>
                {
                    progress.AddItems(files.Count());
                    using (workerGroup = new WorkerGroup(maxConcurrency, progress))
                    {
                        foreach (string file in files)
                        {
                            IThreadWorker worker = workerGroup.CreateWorker();
                            string closureOverCopyOfLoopVariableFile = file;
                            InteractionSafeUi(() =>
                            {
                                processFile(closureOverCopyOfLoopVariableFile, worker, progress);
                            });
                            if (workerGroup.FirstError != FileOperationStatus.Success)
                            {
                                break;
                            }
                            progress.AddItems(-1);
                        }
                        workerGroup.WaitAllAndFinish();
                        return workerGroup.FirstError;
                    }
                },
                (FileOperationStatus status) =>
                {
                });
        }

        private static bool CheckStatusAndShowMessage(FileOperationStatus status, string displayContext)
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

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private bool _handleWorkFolderStateChangedInProgress = false;

        private void HandleWorkFolderStateChangedEvent(object sender, SessionEventArgs e)
        {
            if (OS.Log.IsInfoEnabled)
            {
                OS.Log.LogInfo("Tick");                                 //MLHIDE
            }

            if (_handleWorkFolderStateChangedInProgress)
            {
                OS.Current.NotifySessionChanged(new SessionEvent(SessionEventType.SessionChange));
                return;
            }
            _handleWorkFolderStateChangedInProgress = true;

            progressBackgroundWorker.BackgroundWorkWithProgress(
                (ProgressContext progress) =>
                {
                    progress.NotifyLevelStart();
                    try
                    {
                        persistentState.Current.HandleSessionStateEvents(e.SessionEvents, progress);
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
                    _watchedFoldersCore.UpdateListView();
                    _handleWorkFolderStateChangedInProgress = false;
                });
        }

        private bool FilesAreOpen
        {
            get
            {
                IList<ActiveFile> openFiles = persistentState.Current.DecryptedActiveFiles;
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
            string encryptedPath = recentFilesListView.SelectedItems[0].SubItems["EncryptedPath"].Text; //MLHIDE

            ProcessFilesInBackground(new string[] { encryptedPath }, OpenEncrypted);
        }

        private void recentFilesListView_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            ListView listView = (ListView)sender;
            string columnName = listView.Columns[e.ColumnIndex].Name;
            switch (columnName)
            {
                case "DecryptedFile":                                 //MLHIDE
                    Settings.Default.RecentFilesDocumentWidth = listView.Columns[e.ColumnIndex].Width;
                    break;
            }
            Settings.Default.Save();
        }

        private void PurgeActiveFiles()
        {
            progressBackgroundWorker.BackgroundWorkWithProgress(
                (ProgressContext progress) =>
                {
                    progress.NotifyLevelStart();
                    try
                    {
                        persistentState.Current.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, progress);
                        persistentState.Current.PurgeActiveFiles(progress);
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
                    IList<ActiveFile> openFiles = persistentState.Current.DecryptedActiveFiles;
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

            progressBackgroundWorker.BackgroundWorkWithProgress(
                (ProgressContext progress) =>
                {
                    persistentState.Current.RemoveRecentFiles(encryptedPaths, progress);
                    return FileOperationStatus.Success;
                },
                (FileOperationStatus status) =>
                {
                });
        }

        private void decryptAndRemoveFromListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IEnumerable<string> encryptedPaths = SelectedRecentFilesItems();

            ProcessFilesInBackground(encryptedPaths, DecryptFile);
        }

        private IEnumerable<string> SelectedRecentFilesItems()
        {
            IEnumerable<string> selected = recentFilesListView.SelectedItems.Cast<ListViewItem>().Select((ListViewItem item) => { return item.SubItems["EncryptedPath"].Text; }).ToArray();
            return selected;
        }

        private void recentFilesListView_MouseClick(object sender, MouseEventArgs e)
        {
            ShowContextMenu(recentFilesContextMenuStrip, sender, e);
        }

        private void watchedFoldersListView_MouseClick(object sender, MouseEventArgs e)
        {
            ShowContextMenu(watchedFoldersContextMenuStrip, sender, e);
        }

        private static void ShowContextMenu(ContextMenuStrip contextMenu, object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }
            ListView listView = (ListView)sender;
            contextMenu.Show(listView, e.Location);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (string watchedFolderPath in SelectedWatchedFoldersItems())
            {
                persistentState.Current.RemoveWatchedFolder(new WatchedFolder(watchedFolderPath));
                persistentState.Current.Save();
            }
        }

        private IEnumerable<string> SelectedWatchedFoldersItems()
        {
            IEnumerable<string> selected  = watchedFoldersListView.SelectedItems.Cast<ListViewItem>().Select((ListViewItem item) => { return item.Text; }).ToArray();
            return selected;
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
            Passphrase passphrase = new Passphrase(passphraseText);
            persistentState.Current.KnownKeys.Add(passphrase.DerivedPassphrase);
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

        private void encryptionKeyToolStripButton_Click(object sender, EventArgs e)
        {
            if (persistentState.Current.KnownKeys.DefaultEncryptionKey == null)
            {
                string passphrase = AskForEncryptionPassphrase();
                if (String.IsNullOrEmpty(passphrase))
                {
                    return;
                }
                AesKey defaultEncryptionKey = new Passphrase(passphrase).DerivedPassphrase;
                persistentState.Current.KnownKeys.DefaultEncryptionKey = defaultEncryptionKey;
            }
            else
            {
                persistentState.Current.KnownKeys.DefaultEncryptionKey = null;
                persistentState.Current.KnownKeys.Clear();
            }
            SetToolButtonsState();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
                if (_interactionSemaphore != null)
                {
                    _interactionSemaphore.Close();
                    _interactionSemaphore = null;
                }
                OS.Current = null;
            }
            base.Dispose(disposing);
        }

        private void watchedFoldersListView_DragDrop(object sender, DragEventArgs e)
        {
            IRuntimeFileInfo droppedFolder = GetDroppedFolderIfAny(e.Data);
            if (droppedFolder == null)
            {
                return;
            }
            persistentState.Current.AddWatchedFolder(new WatchedFolder(droppedFolder.FullName));
            persistentState.Current.Save();
        }

        private void watchedFoldersListView_DragEnter(object sender, DragEventArgs e)
        {
            IRuntimeFileInfo droppedFolder = GetDroppedFolderIfAny(e.Data);
            if (droppedFolder == null)
            {
                return;
            }
        }

        private void watchedFoldersListView_DragOver(object sender, DragEventArgs e)
        {
            IRuntimeFileInfo droppedFolder = GetDroppedFolderIfAny(e.Data);
            if (droppedFolder == null)
            {
                return;
            }
            e.Effect = DragDropEffects.Link;
        }

        private static IRuntimeFileInfo GetDroppedFolderIfAny(IDataObject dataObject)
        {
            IList<string> dropped = dataObject.GetData(DataFormats.FileDrop) as IList<string>;
            if (dropped == null)
            {
                return null;
            }
            if (dropped.Count != 1)
            {
                return null;
            }
            IRuntimeFileInfo fileInfo = OS.Current.FileInfo(dropped[0]);
            if (!fileInfo.IsFolder)
            {
                return null;
            }
            return fileInfo;
        }

        private void watchedFoldersListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            _watchedFoldersCore.OpenSelectedFolder();
        }
    }
}