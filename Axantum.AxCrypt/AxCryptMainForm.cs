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
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.System;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Properties;

namespace Axantum.AxCrypt
{
    /// <summary>
    /// All code here is guaranteed to execute on the GUI thread. If code may be called on another thread, this call
    /// should be made through MainFormThreadFacade .
    /// </summary>
    public partial class AxCryptMainForm : Form
    {
        public static MessageBoxOptions MessageBoxOptions { get; private set; }

        public void FormatTraceMessage(string message)
        {
            int skipIndex = message.IndexOf(" Information", StringComparison.Ordinal); //MLHIDE
            skipIndex = skipIndex < 0 ? message.IndexOf(" Warning", StringComparison.Ordinal) : skipIndex; //MLHIDE
            skipIndex = skipIndex < 0 ? message.IndexOf(" Debug", StringComparison.Ordinal) : skipIndex; //MLHIDE
            skipIndex = skipIndex < 0 ? message.IndexOf(" Error", StringComparison.Ordinal) : skipIndex; //MLHIDE
            LogOutput.AppendText("{0} {1}".InvariantFormat(DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), message.Substring(skipIndex + 1))); //MLHIDE
        }

        private MainFormThreadFacade ThreadFacade { get; set; }

        public AxCryptMainForm()
        {
            InitializeComponent();
        }

        private void AxCryptMainForm_Load(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            ThreadFacade = new MainFormThreadFacade(this);

            DelegateTraceListener traceListener = new DelegateTraceListener(ThreadFacade.FormatTraceMessage);
            traceListener.Name = "AxCryptMainFormListener";           //MLHIDE
            Trace.Listeners.Add(traceListener);

            EncryptedFileManager.SetProcessTracking(AxCryptEnvironment.Current.IsDesktopWindows);
            EncryptedFileManager.Changed += new EventHandler<EventArgs>(ThreadFacade.EncryptedFileManager_Changed);
            EncryptedFileManager.ForceActiveFilesStatus(new ProgressContext());

            RestoreUserPreferences();

            MessageBoxOptions = RightToLeft == RightToLeft.Yes ? MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading : 0;
        }

        private void AxCryptMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            ThreadFacade.WaitForBackgroundIdle();
            PurgeActiveFiles();
            ThreadFacade.WaitForBackgroundIdle();
            Trace.Listeners.Remove("AxCryptMainFormListener");        //MLHIDE
        }

        private void RestoreUserPreferences()
        {
            UserPreferences userPreferences = Settings.Default.UserPreferences;
            if (userPreferences == null)
            {
                userPreferences = new UserPreferences();
                Settings.Default.UserPreferences = userPreferences;
                Settings.Default.Save();
                return;
            }
            RecentFilesListView.Columns[0].Name = "DecryptedFile";    //MLHIDE
            RecentFilesListView.Columns[0].Width = userPreferences.RecentFilesDocumentWidth > 0 ? userPreferences.RecentFilesDocumentWidth : RecentFilesListView.Columns[0].Width;
        }

        public void RestartTimer()
        {
            ActiveFilePolling.Enabled = false;
            ActiveFilePolling.Interval = 1000;
            ActiveFilePolling.Enabled = true;
        }

        private void UpdateActiveFileState()
        {
            OpenFilesListView.Items.Clear();
            RecentFilesListView.Items.Clear();
            EncryptedFileManager.ForEach(false,
                (ActiveFile activeFile) =>
                {
                    return UpdateOpenFilesWith(activeFile);
                });
            CloseAndRemoveOpenFilesButton.Enabled = OpenFilesListView.Items.Count > 0;
        }

        private ActiveFile UpdateOpenFilesWith(ActiveFile activeFile)
        {
            ListViewItem item;
            if (activeFile.Status.HasFlag(ActiveFileStatus.NotDecrypted))
            {
                if (String.IsNullOrEmpty(activeFile.DecryptedPath))
                {
                    item = new ListViewItem(String.Empty, "InactiveFile"); //MLHIDE
                }
                else
                {
                    item = new ListViewItem(Path.GetFileName(activeFile.DecryptedPath), "ActiveFile"); //MLHIDE
                }

                ListViewItem.ListViewSubItem dateColumn = new ListViewItem.ListViewSubItem();
                dateColumn.Text = activeFile.LastAccessTimeUtc.ToLocalTime().ToString(CultureInfo.CurrentCulture);
                dateColumn.Tag = activeFile.LastAccessTimeUtc;
                dateColumn.Name = "Date";                             //MLHIDE
                item.SubItems.Add(dateColumn);

                ListViewItem.ListViewSubItem encryptedPathColumn = new ListViewItem.ListViewSubItem();
                encryptedPathColumn.Name = "EncryptedPath";           //MLHIDE
                encryptedPathColumn.Text = activeFile.EncryptedPath;
                item.SubItems.Add(encryptedPathColumn);

                RecentFilesListView.Items.Add(item);
            }

            if (activeFile.Status.HasFlag(ActiveFileStatus.DecryptedIsPendingDelete) || activeFile.Status.HasFlag(ActiveFileStatus.AssumedOpenAndDecrypted))
            {
                item = new ListViewItem(Path.GetFileName(activeFile.DecryptedPath), activeFile.Key != null ? "ActiveFile" : "Exclamation"); //MLHIDE
                ListViewItem.ListViewSubItem encryptedPathColumn = new ListViewItem.ListViewSubItem();
                encryptedPathColumn.Name = "EncryptedPath";           //MLHIDE
                encryptedPathColumn.Text = activeFile.EncryptedPath;
                item.SubItems.Add(encryptedPathColumn);

                OpenFilesListView.Items.Add(item);
            }

            return activeFile;
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
            if (String.Compare(Path.GetExtension(file), AxCryptEnvironment.Current.AxCryptExtension, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return;
            }

            IRuntimeFileInfo sourceFileInfo = AxCryptEnvironment.Current.FileInfo(file);
            IRuntimeFileInfo destinationFileInfo = AxCryptEnvironment.Current.FileInfo(AxCryptFile.MakeAxCryptFileName(sourceFileInfo));
            if (destinationFileInfo.Exists)
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Title = Resources.EncryptFileSaveAsDialogTitle;
                    sfd.AddExtension = true;
                    sfd.ValidateNames = true;
                    sfd.CheckPathExists = true;
                    sfd.DefaultExt = AxCryptEnvironment.Current.AxCryptExtension;
                    sfd.FileName = destinationFileInfo.FullName;
                    sfd.Filter = Resources.EncryptedFileDialogFilterPattern.InvariantFormat(AxCryptEnvironment.Current.AxCryptExtension);
                    sfd.InitialDirectory = Path.GetDirectoryName(destinationFileInfo.FullName);
                    sfd.ValidateNames = true;
                    DialogResult saveAsResult = sfd.ShowDialog();
                    if (saveAsResult != DialogResult.OK)
                    {
                        return;
                    }
                    destinationFileInfo = AxCryptEnvironment.Current.FileInfo(sfd.FileName);
                }
            }

            AesKey key = null;
            if (KnownKeys.DefaultEncryptionKey == null)
            {
                EncryptPassphraseDialog passphraseDialog = new EncryptPassphraseDialog();
                DialogResult dialogResult = passphraseDialog.ShowDialog();
                if (dialogResult != DialogResult.OK)
                {
                    return;
                }
                Passphrase passphrase = new Passphrase(passphraseDialog.PassphraseTextBox.Text);
                key = passphrase.DerivedPassphrase;
            }
            else
            {
                key = KnownKeys.DefaultEncryptionKey;
            }

            ThreadFacade.DoBackgroundWork(sourceFileInfo.FullName,
                (WorkerArguments arguments) =>
                {
                    EncryptedFileManager.EncryptFile(sourceFileInfo, destinationFileInfo, key, arguments.Progress);
                    arguments.Result = FileOperationStatus.Success;
                },
                (object sender, RunWorkerCompletedEventArgs e) =>
                {
                    FileOperationStatus status = (FileOperationStatus)e.Result;
                    CheckStatusAndShowMessage(status, sourceFileInfo.Name);
                });

            if (KnownKeys.DefaultEncryptionKey == null)
            {
                KnownKeys.DefaultEncryptionKey = key;
            }
        }

        private static void CheckStatusAndShowMessage(FileOperationStatus status, string displayText)
        {
            switch (status)
            {
                case FileOperationStatus.Success:
                    break;
                case FileOperationStatus.UnspecifiedError:
                    Resources.FileOperationFailed.InvariantFormat(displayText).ShowWarning();
                    break;
                case FileOperationStatus.FileAlreadyExists:
                    Resources.FileAlreadyExists.InvariantFormat(displayText).ShowWarning();
                    break;
                case FileOperationStatus.FileDoesNotExist:
                    Resources.FileDoesNotExist.InvariantFormat(displayText).ShowWarning();
                    break;
                case FileOperationStatus.CannotWriteDestination:
                    Resources.CannotWrite.InvariantFormat(displayText).ShowWarning();
                    break;
                case FileOperationStatus.CannotStartApplication:
                    Resources.CannotStartApplication.InvariantFormat(displayText).ShowWarning();
                    break;
                case FileOperationStatus.InconsistentState:
                    Resources.InconsistentState.InvariantFormat(displayText).ShowWarning();
                    break;
                case FileOperationStatus.InvalidKey:
                    Resources.InvalidKey.InvariantFormat(displayText).ShowWarning();
                    break;
                case FileOperationStatus.Canceled:
                    Resources.Canceled.InvariantFormat(displayText).ShowWarning();
                    break;
                default:
                    Resources.UnrecognizedError.InvariantFormat(displayText).ShowWarning();
                    break;
            }
        }

        private void toolStripButtonDecrypt_Click(object sender, EventArgs e)
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
                ofd.DefaultExt = AxCryptEnvironment.Current.AxCryptExtension;
                ofd.Filter = Resources.EncryptedFileDialogFilterPattern.InvariantFormat("{0}".InvariantFormat(AxCryptEnvironment.Current.AxCryptExtension)); //MLHIDE
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
                if (!DecryptFile(AxCryptEnvironment.Current.FileInfo(file)))
                {
                    return;
                }
            }
        }

        private bool DecryptFile(IRuntimeFileInfo source)
        {
            AxCryptDocument document = GetDocumentToDecrypt(source);
            if (document == null)
            {
                return false;
            }
            IRuntimeFileInfo destination = GetDestination(source, document.DocumentHeaders.FileName);
            if (destination == null)
            {
                return false;
            }
            ThreadFacade.DoBackgroundWork(source.Name,
                (WorkerArguments arguments) =>
                {
                    try
                    {
                        AxCryptFile.Decrypt(document, destination, AxCryptOptions.SetFileTimes, arguments.Progress);
                    }
                    finally
                    {
                        document.Dispose();
                    }
                    AxCryptFile.Wipe(source);
                    arguments.Result = FileOperationStatus.Success;
                },
                (object sender, RunWorkerCompletedEventArgs e) =>
                {
                    FileOperationStatus status = (FileOperationStatus)e.Result;
                    CheckStatusAndShowMessage(status, source.Name);
                });
            return true;
        }

        private AxCryptDocument GetDocumentToDecrypt(IRuntimeFileInfo source)
        {
            AxCryptDocument document = null;
            try
            {
                foreach (AesKey key in KnownKeys.Keys)
                {
                    document = AxCryptFile.Document(source, key, new ProgressContext());
                    if (document.PassphraseIsValid)
                    {
                        return document;
                    }
                    document.Dispose();
                }

                Passphrase passphrase;
                while (true)
                {
                    DecryptPassphraseDialog passphraseDialog = new DecryptPassphraseDialog();
                    DialogResult dialogResult = passphraseDialog.ShowDialog();
                    if (dialogResult != DialogResult.OK)
                    {
                        return null;
                    }
                    passphrase = new Passphrase(passphraseDialog.Passphrase.Text);
                    document = AxCryptFile.Document(source, passphrase.DerivedPassphrase, new ProgressContext());
                    if (document.PassphraseIsValid)
                    {
                        AddKnownKey(document.DocumentHeaders.KeyEncryptingKey);
                        return document;
                    }
                    document.Dispose();
                }
            }
            catch (Exception)
            {
                if (document != null)
                {
                    document.Dispose();
                }
                throw;
            }
        }

        private static IRuntimeFileInfo GetDestination(IRuntimeFileInfo source, string fileName)
        {
            IRuntimeFileInfo destination = AxCryptEnvironment.Current.FileInfo(Path.Combine(Path.GetDirectoryName(source.FullName), fileName));
            if (!destination.Exists)
            {
                return destination;
            }
            string extension = Path.GetExtension(destination.FullName);
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.AddExtension = !String.IsNullOrEmpty(extension);
            sfd.CheckPathExists = true;
            sfd.DefaultExt = extension;
            sfd.Filter = Resources.DecryptedSaveAsFileDialogFilterPattern.InvariantFormat(extension);
            sfd.InitialDirectory = Path.GetDirectoryName(source.FullName);
            sfd.OverwritePrompt = true;
            sfd.RestoreDirectory = true;
            sfd.Title = Resources.DecryptedSaveAsFileDialogTitle;
            DialogResult result = sfd.ShowDialog();
            if (result != DialogResult.OK)
            {
                return null;
            }
            destination = AxCryptEnvironment.Current.FileInfo(sfd.FileName);
            return destination;
        }

        private void AddKnownKey(AesKey key)
        {
            KnownKeys.Add(key);
            EncryptedFileManager.CheckActiveFilesStatus(new ProgressContext());
        }

        private void toolStripButtonOpenEncrypted_Click(object sender, EventArgs e)
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
                ofd.DefaultExt = AxCryptEnvironment.Current.AxCryptExtension;
                ofd.Filter = Resources.EncryptedFileDialogFilterPattern.InvariantFormat("{0}".InvariantFormat(AxCryptEnvironment.Current.AxCryptExtension)); //MLHIDE
                DialogResult result = ofd.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return;
                }

                foreach (string file in ofd.FileNames)
                {
                    OpenEncrypted(file);
                }
            }
        }

        private void OpenEncrypted(string file)
        {
            ThreadFacade.DoBackgroundWork(Path.GetFileName(file),
                (WorkerArguments arguments) =>
                {
                    arguments.Result = EncryptedFileManager.Open(file, KnownKeys.Keys, arguments.Progress);
                },
                (object sender, RunWorkerCompletedEventArgs e) =>
                {
                    FileOperationStatus status = (FileOperationStatus)e.Result;
                    if (status == FileOperationStatus.InvalidKey)
                    {
                        AskForPassphraseAndOpenEncrypted(file);
                        return;
                    }
                    CheckStatusAndShowMessage(status, file);
                });
        }

        private void AskForPassphraseAndOpenEncrypted(string file)
        {
            Passphrase passphrase;
            passphrase = AskForDecryptPassphrase();
            if (passphrase == null)
            {
                return;
            }
            ThreadFacade.DoBackgroundWork(Path.GetFileName(file),
                (WorkerArguments arguments) =>
                {
                    arguments.Result = EncryptedFileManager.Open(file, new AesKey[] { passphrase.DerivedPassphrase }, arguments.Progress);
                },
                (object sender, RunWorkerCompletedEventArgs e) =>
                {
                    FileOperationStatus status = (FileOperationStatus)e.Result;
                    if (status == FileOperationStatus.Success)
                    {
                        AddKnownKey(passphrase.DerivedPassphrase);
                        return;
                    }
                    if (status == FileOperationStatus.InvalidKey)
                    {
                        AskForPassphraseAndOpenEncrypted(file);
                        return;
                    }
                    CheckStatusAndShowMessage(status, file);
                });
        }

        private Passphrase AskForDecryptPassphrase()
        {
            DecryptPassphraseDialog passphraseDialog = new DecryptPassphraseDialog();
            DialogResult dialogResult = passphraseDialog.ShowDialog(this);
            if (dialogResult != DialogResult.OK)
            {
                return null;
            }
            Passphrase passphrase = new Passphrase(passphraseDialog.Passphrase.Text);
            return passphrase;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private bool _fileOperationInProgress = false;

        private bool _pollingInProgress = false;

        private void ActiveFilePolling_Tick(object sender, EventArgs e)
        {
            if (Logging.IsInfoEnabled)
            {
                Logging.Info("Tick");                                 //MLHIDE
            }
            if (_pollingInProgress)
            {
                return;
            }
            if (_fileOperationInProgress)
            {
                return;
            }
            ActiveFilePolling.Enabled = false;
            try
            {
                _fileOperationInProgress = false/*true*/;
                _pollingInProgress = true;
                ThreadFacade.DoBackgroundWork(Resources.UpdatingStatus,
                    (WorkerArguments arguments) =>
                    {
                        EncryptedFileManager.CheckActiveFilesStatus(arguments.Progress);
                    },
                    (object sender1, RunWorkerCompletedEventArgs e1) =>
                    {
                        _pollingInProgress = false;
                    });
            }
            finally
            {
                _fileOperationInProgress = false;
            }
            UpdateActiveFileState();
        }

        private void openEncryptedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenDialog();
        }

        private void RecentFilesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void RecentFilesListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string encryptedPath = RecentFilesListView.SelectedItems[0].SubItems["EncryptedPath"].Text; //MLHIDE

            OpenEncrypted(encryptedPath);
        }

        private void RecentFilesListView_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            ListView listView = (ListView)sender;
            string columnName = listView.Columns[e.ColumnIndex].Name;
            switch (columnName)
            {
                case "DecryptedFile":                                 //MLHIDE
                    Settings.Default.UserPreferences.RecentFilesDocumentWidth = listView.Columns[e.ColumnIndex].Width;
                    break;
            }
            Settings.Default.Save();
        }

        private void PurgeActiveFiles()
        {
            EncryptedFileManager.SetProcessTracking(false);
            ThreadFacade.DoBackgroundWork(Resources.PurgingActiveFiles,
                (WorkerArguments arguments) =>
                {
                    EncryptedFileManager.CheckActiveFilesStatus(arguments.Progress);
                    EncryptedFileManager.PurgeActiveFiles(arguments.Progress);
                },
                (object sender, RunWorkerCompletedEventArgs e) =>
                {
                    IList<ActiveFile> openFiles = EncryptedFileManager.FindOpenFiles();
                    if (openFiles.Count == 0)
                    {
                        return;
                    }
                    StringBuilder sb = new StringBuilder();
                    foreach (ActiveFile openFile in openFiles)
                    {
                        sb.Append("{0}\n".InvariantFormat(Path.GetFileName(openFile.DecryptedPath))); //MLHIDE
                    }
                    sb.ToString().ShowWarning();
                });
        }

        private void OpenFilesListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string encryptedPath = OpenFilesListView.SelectedItems[0].SubItems["EncryptedPath"].Text; //MLHIDE
            OpenEncrypted(encryptedPath);
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string encryptedPath = RecentFilesListView.SelectedItems[0].SubItems["EncryptedPath"].Text; //MLHIDE
            EncryptedFileManager.RemoveRecentFile(encryptedPath);
        }

        private void RecentFilesListView_MouseClick(object sender, MouseEventArgs e)
        {
            ShowContextMenu(RecentFilesContextMenu, sender, e);
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

        private void CloseStripMenuItem_Click(object sender, EventArgs e)
        {
            PurgeActiveFiles();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            PurgeActiveFiles();
        }

        private void OpenFilesListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }
            ListView recentFiles = (ListView)sender;
            OpenFilesContextMenu.Show(recentFiles, e.Location);
        }

        private void EnterPassphraseMenuItem_Click(object sender, EventArgs e)
        {
            Passphrase passphrase = AskForDecryptPassphrase();
            if (passphrase == null)
            {
                return;
            }
            bool keyMatch = EncryptedFileManager.UpdateActiveFileIfKeyMatchesThumbprint(passphrase.DerivedPassphrase);
            if (keyMatch)
            {
                KnownKeys.Add(passphrase.DerivedPassphrase);
            }
        }

        public void ShowProgressContextMenu(Control progressControl, Point location)
        {
            ProgressContextMenu.Tag = progressControl;
            ProgressContextMenu.Show(progressControl, location);
        }

        private void ProgressContextCancelMenu_Click(object sender, EventArgs e)
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
            if (!AxCryptEnvironment.Current.IsDesktopWindows)
            {
                return;
            }

            TrayNotifyIcon.BalloonTipTitle = "AxCrypt File Encryption";
            TrayNotifyIcon.BalloonTipText = "Double-click to restore the AxCrypt application window.";

            if (FormWindowState.Minimized == this.WindowState)
            {
                TrayNotifyIcon.Visible = true;
                TrayNotifyIcon.ShowBalloonTip(500);
                this.Hide();
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                TrayNotifyIcon.Visible = false;
            }
        }

        private void TrayNotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void EnglishMenuItem_Click(object sender, EventArgs e)
        {
            SetLanguage("en-US"); //MLHIDE
        }

        private void SwedishMenuItem_Click(object sender, EventArgs e)
        {
            SetLanguage("sv-SE"); //MLHIDE
        }

        private static void SetLanguage(string cultureName)
        {
            Settings.Default.UserPreferences.CultureName = cultureName;
            Settings.Default.Save();
            "You must restart AxCrypt in order for the language change to take effect.".ShowWarning();
        }
    }
}