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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Mono;
using Axantum.AxCrypt.Properties;

namespace Axantum.AxCrypt
{
    /// <summary>
    /// All code here is expected to execute on the GUI thread. If code may be called on another thread, this call
    /// must be made through ThreadSafeUi() .
    /// </summary>
    public partial class AxCryptMainForm : Form
    {
        private Uri _updateUrl = Settings.Default.UpdateUrl;

        private TabPage _logTabPage = null;

        private NotifyIcon _notifyIcon = null;

        public static MessageBoxOptions MessageBoxOptions { get; private set; }

        public AxCryptMainForm()
        {
            OS.Current = new RuntimeEnvironment();
            InitializeComponent();
            if (OS.Current.Platform == Platform.WindowsDesktop)
            {
                _notifyIcon = new NotifyIcon(components);
                _notifyIcon.MouseDoubleClick += trayNotifyIcon_MouseDoubleClick;
                _notifyIcon.Icon = Resources.axcrypticon;
                _notifyIcon.Visible = true;
            }
        }

        private void AxCryptMainForm_Load(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            Trace.Listeners.Add(new DelegateTraceListener("AxCryptMainFormListener", FormatTraceMessage)); //MLHIDE

            RestoreUserPreferences();

            Text = "{0} {1}{2}".InvariantFormat(Application.ProductName, Application.ProductVersion, String.IsNullOrEmpty(AboutBox.AssemblyDescription) ? String.Empty : " " + AboutBox.AssemblyDescription); //MLHIDE

            MessageBoxOptions = RightToLeft == RightToLeft.Yes ? MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading : 0;

            OS.Current.FileChanged += new EventHandler<EventArgs>(HandleFileChangedEvent);

            persistentState.Current.Changed += new EventHandler<ActiveFileChangedEventArgs>(HandleFileSystemStateChangedEvent);

            persistentState.Current.Load(FileSystemState.DefaultPathInfo);

            backgroundMonitor.UpdateCheck.VersionUpdate += new EventHandler<VersionEventArgs>(HandleVersionUpdateEvent);
            UpdateCheck(Settings.Default.LastUpdateCheckUtc);
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

        private void HandleFileChangedEvent(object sender, EventArgs e)
        {
            ThreadSafeUi(RestartTimer);
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

        private void UpdateActiveFilesViews(ActiveFile activeFile)
        {
            if (activeFile.Status.HasMask(ActiveFileStatus.NoLongerActive))
            {
                openFilesListView.Items.RemoveByKey(activeFile.EncryptedFileInfo.FullName);
                recentFilesListView.Items.RemoveByKey(activeFile.EncryptedFileInfo.FullName);
                return;
            }

            if (activeFile.Status.HasMask(ActiveFileStatus.NotDecrypted))
            {
                UpdateRecentFilesListView(activeFile);
                return;
            }

            if (activeFile.Status.HasMask(ActiveFileStatus.DecryptedIsPendingDelete) || activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted))
            {
                UpdateOpenFilesListView(activeFile);
                return;
            }
        }

        private void UpdateOpenFilesListView(ActiveFile activeFile)
        {
            ListViewItem item;
            item = new ListViewItem(Path.GetFileName(activeFile.DecryptedFileInfo.FullName), activeFile.Key != null ? "ActiveFile" : "Exclamation"); //MLHIDE
            ListViewItem.ListViewSubItem encryptedPathColumn = new ListViewItem.ListViewSubItem();
            encryptedPathColumn.Name = "EncryptedPath";           //MLHIDE
            encryptedPathColumn.Text = activeFile.EncryptedFileInfo.FullName;
            item.SubItems.Add(encryptedPathColumn);

            item.Name = activeFile.EncryptedFileInfo.FullName;
            openFilesListView.Items.RemoveByKey(item.Name);
            openFilesListView.Items.Add(item);
            recentFilesListView.Items.RemoveByKey(item.Name);
        }

        private void UpdateRecentFilesListView(ActiveFile activeFile)
        {
            ListViewItem item;
            if (String.IsNullOrEmpty(activeFile.DecryptedFileInfo.FullName))
            {
                item = new ListViewItem(String.Empty, "InactiveFile"); //MLHIDE
            }
            else
            {
                item = new ListViewItem(Path.GetFileName(activeFile.DecryptedFileInfo.FullName), "ActiveFile"); //MLHIDE
            }

            ListViewItem.ListViewSubItem dateColumn = new ListViewItem.ListViewSubItem();
            dateColumn.Text = activeFile.LastActivityTimeUtc.ToLocalTime().ToString(CultureInfo.CurrentCulture);
            dateColumn.Tag = activeFile.LastActivityTimeUtc;
            dateColumn.Name = "Date";                             //MLHIDE
            item.SubItems.Add(dateColumn);

            ListViewItem.ListViewSubItem encryptedPathColumn = new ListViewItem.ListViewSubItem();
            encryptedPathColumn.Name = "EncryptedPath";           //MLHIDE
            encryptedPathColumn.Text = activeFile.EncryptedFileInfo.FullName;
            item.SubItems.Add(encryptedPathColumn);

            item.Name = activeFile.EncryptedFileInfo.FullName;
            recentFilesListView.Items.RemoveByKey(item.Name);
            recentFilesListView.Items.Add(item);
            openFilesListView.Items.RemoveByKey(item.Name);
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
                foreach (string file in ofd.FileNames)
                {
                    EncryptFile(file);
                }
            }
        }

        private void EncryptFile(string file)
        {
            FileOperationsController operationsController = new FileOperationsController(persistentState.Current, file);

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
                using (EncryptPassphraseDialog passphraseDialog = new EncryptPassphraseDialog())
                {
                    passphraseDialog.ShowPassphraseCheckBox.Checked = Settings.Default.ShowEncryptPasshrase;
                    DialogResult dialogResult = passphraseDialog.ShowDialog();
                    if (dialogResult != DialogResult.OK)
                    {
                        e.Cancel = true;
                        return;
                    }
                    if (passphraseDialog.ShowPassphraseCheckBox.Checked != Settings.Default.ShowEncryptPasshrase)
                    {
                        Settings.Default.ShowEncryptPasshrase = passphraseDialog.ShowPassphraseCheckBox.Checked;
                        Settings.Default.Save();
                    }
                    e.Passphrase = passphraseDialog.PassphraseTextBox.Text;
                }
                persistentState.Current.KnownKeys.DefaultEncryptionKey = new Passphrase(e.Passphrase).DerivedPassphrase;
            };

            operationsController.ProcessFile += HandleProcessFileEvent;

            operationsController.EncryptFile(file);
        }

        private void HandleProcessFileEvent(object sender, FileOperationEventArgs e)
        {
            progressBackgroundWorker.BackgroundWorkWithProgress(e.DisplayContext,
                (ProgressContext progress) =>
                {
                    e.Progress = progress;
                    return ((FileOperationsController)sender).DoProcessFile(e);
                },
                (FileOperationStatus status) =>
                {
                    CheckStatusAndShowMessage(status, e.DisplayContext);
                });
        }

        private static void CheckStatusAndShowMessage(FileOperationStatus status, string displayContext)
        {
            switch (status)
            {
                case FileOperationStatus.Success:
                    break;

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
                    Resources.Canceled.InvariantFormat(displayContext).ShowWarning();
                    break;

                case FileOperationStatus.Exception:
                    Resources.Exception.InvariantFormat(displayContext).ShowWarning();
                    break;
                default:
                    Resources.UnrecognizedError.InvariantFormat(displayContext).ShowWarning();
                    break;
            }
        }

        private void decryptToolStripButton_Click(object sender, EventArgs e)
        {
            DecryptFilesViaDialog();
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
            foreach (string file in fileNames)
            {
                if (!DecryptFile(OS.Current.FileInfo(file)))
                {
                    return;
                }
            }
        }

        private bool DecryptFile(IRuntimeFileInfo source)
        {
            FileOperationsController operationsController = new FileOperationsController(persistentState.Current, source.Name);

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
                    sfd.InitialDirectory = Path.GetDirectoryName(source.FullName);
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
                AddKnownKey(e.Key);
            };

            operationsController.ProcessFile += HandleProcessFileEvent;

            return operationsController.DecryptFile(source.FullName);
        }

        private void AddKnownKey(AesKey key)
        {
            persistentState.Current.KnownKeys.Add(key);
            persistentState.Current.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
        }

        private void openEncryptedToolStripButton_Click(object sender, EventArgs e)
        {
            OpenDialog();
        }

        private void OpenDialog()
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

                foreach (string file in ofd.FileNames)
                {
                    if (!OpenEncrypted(file))
                    {
                        return;
                    }
                }
            }
        }

        private bool OpenEncrypted(string file)
        {
            FileOperationsController operationsController = new FileOperationsController(persistentState.Current, file);

            operationsController.QueryDecryptionPassphrase += HandleQueryDecryptionPassphraseEvent;

            operationsController.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
            {
                AddKnownKey(e.Key);
            };

            operationsController.ProcessFile += HandleProcessFileEvent;

            return operationsController.DecryptAndLaunch(file);
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

        public void RestartTimer()
        {
            activeFilePollingTimer.Enabled = false;
            activeFilePollingTimer.Interval = 1000;
            activeFilePollingTimer.Enabled = true;
        }

        private bool _pollingInProgress = false;

        private void activeFilePollingTimer_Tick(object sender, EventArgs e)
        {
            if (OS.Log.IsInfoEnabled)
            {
                OS.Log.LogInfo("Tick");                                 //MLHIDE
            }
            if (_pollingInProgress)
            {
                return;
            }
            activeFilePollingTimer.Enabled = false;
            try
            {
                _pollingInProgress = true;
                progressBackgroundWorker.BackgroundWorkWithProgress(Resources.UpdatingStatus,
                    (ProgressContext progress) =>
                    {
                        persistentState.Current.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, progress);
                        return FileOperationStatus.Success;
                    },
                    (FileOperationStatus status) =>
                    {
                        closeAndRemoveOpenFilesToolStripButton.Enabled = openFilesListView.Items.Count > 0;
                    });
            }
            finally
            {
                _pollingInProgress = false;
            }
        }

        private void openEncryptedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenDialog();
        }

        private void recentFilesListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string encryptedPath = recentFilesListView.SelectedItems[0].SubItems["EncryptedPath"].Text; //MLHIDE

            OpenEncrypted(encryptedPath);
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
            progressBackgroundWorker.BackgroundWorkWithProgress(Resources.PurgingActiveFiles,
                (ProgressContext progress) =>
                {
                    persistentState.Current.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, progress);
                    persistentState.Current.PurgeActiveFiles(progress);
                    return FileOperationStatus.Success;
                },
                (FileOperationStatus status) =>
                {
                    if (status != FileOperationStatus.Success)
                    {
                        CheckStatusAndShowMessage(status, Resources.PurgingActiveFiles);
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
                        sb.Append("{0}\n".InvariantFormat(Path.GetFileName(openFile.DecryptedFileInfo.FullName))); //MLHIDE
                    }
                    sb.ToString().ShowWarning();
                });
        }

        private void openFilesListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string encryptedPath = openFilesListView.SelectedItems[0].SubItems["EncryptedPath"].Text; //MLHIDE
            OpenEncrypted(encryptedPath);
        }

        private void removeRecentFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string encryptedPath = recentFilesListView.SelectedItems[0].SubItems["EncryptedPath"].Text; //MLHIDE
            persistentState.Current.RemoveRecentFile(encryptedPath);
        }

        private void recentFilesListView_MouseClick(object sender, MouseEventArgs e)
        {
            ShowContextMenu(recentFilesContextMenuStrip, sender, e);
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
            bool keyMatch = persistentState.Current.UpdateActiveFileWithKeyIfKeyMatchesThumbprint(passphrase.DerivedPassphrase);
            if (keyMatch)
            {
                persistentState.Current.KnownKeys.Add(passphrase.DerivedPassphrase);
            }
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
            BackgroundWorker worker = (BackgroundWorker)progressBar.Tag;
            worker.CancelAsync();
        }

        private void encryptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EncryptFilesViaDialog();
        }

        private void decryptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DecryptFilesViaDialog();
        }

        private void AxCryptMainForm_Resize(object sender, EventArgs e)
        {
            if (_notifyIcon == null)
            {
                return;
            }

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

        private void trayNotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
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
                _logTabPage = statusTabControl.TabPages["LogTabPage"]; //MLHIDE
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
    }
}