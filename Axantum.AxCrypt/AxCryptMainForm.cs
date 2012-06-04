using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
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

        private ProgressManager _progressManager;

        private EncryptedFileManager _encryptedFileManager;

        private IDictionary<BackgroundWorker, ProgressBar> _progressBars = new Dictionary<BackgroundWorker, ProgressBar>();

        public void FormatTraceMessage(string message)
        {
            int skipIndex = message.IndexOf(" Information", StringComparison.Ordinal);
            skipIndex = skipIndex < 0 ? message.IndexOf(" Warning", StringComparison.Ordinal) : skipIndex;
            skipIndex = skipIndex < 0 ? message.IndexOf(" Debug", StringComparison.Ordinal) : skipIndex;
            skipIndex = skipIndex < 0 ? message.IndexOf(" Error", StringComparison.Ordinal) : skipIndex;
            LogOutput.AppendText("{0} {1}".InvariantFormat(DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), message.Substring(skipIndex + 1)));
        }

        private MainFormThreadFacade _threadFacade;

        public AxCryptMainForm()
        {
            InitializeComponent();
            _threadFacade = new MainFormThreadFacade(this);
        }

        private void AxCryptMainForm_Load(object sender, EventArgs e)
        {
            DelegateTraceListener traceListener = new DelegateTraceListener(_threadFacade.FormatTraceMessage);
            traceListener.Name = "AxCryptMainFormListener";
            Trace.Listeners.Add(traceListener);

            _progressManager = new ProgressManager();
            _progressManager.Progress += new EventHandler<ProgressEventArgs>(ProgressManager_Progress);
            _encryptedFileManager = new EncryptedFileManager(_progressManager);

            _encryptedFileManager.IgnoreApplication = !AxCryptEnvironment.Current.IsDesktopWindows;
            _encryptedFileManager.Changed += new EventHandler<EventArgs>(_threadFacade.EncryptedFileManager_Changed);
            _encryptedFileManager.ForceActiveFilesStatus(new ProgressContext());

            RestoreUserPreferences();

            MessageBoxOptions = RightToLeft == RightToLeft.Yes ? MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading : 0;
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
            RecentFilesListView.Columns[0].Name = "DecryptedFile";
            RecentFilesListView.Columns[0].Width = userPreferences.RecentFilesDocumentWidth > 0 ? userPreferences.RecentFilesDocumentWidth : RecentFilesListView.Columns[0].Width;
        }

        private void ProgressManager_Progress(object sender, ProgressEventArgs e)
        {
            BackgroundWorker worker = e.Context as BackgroundWorker;
            if (worker != null)
            {
                worker.ReportProgress(e.Percent, worker);
            }
            ProgressBar progressBar = e.Context as ProgressBar;
            if (progressBar != null)
            {
                progressBar.Value = e.Percent;
                if (progressBar.Value == 100)
                {
                    progressBar.Parent = null;
                    progressBar.Dispose();
                }
            }
            return;
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
            _encryptedFileManager.ForEach(false,
                (ActiveFile activeFile) =>
                {
                    return UpdateOpenFilesWith(activeFile);
                });
        }

        private ActiveFile UpdateOpenFilesWith(ActiveFile activeFile)
        {
            ListViewItem item;
            if (activeFile.Status.HasFlag(ActiveFileStatus.NotDecrypted))
            {
                if (String.IsNullOrEmpty(activeFile.DecryptedPath))
                {
                    item = new ListViewItem(String.Empty, "InactiveFile");
                }
                else
                {
                    item = new ListViewItem(Path.GetFileName(activeFile.DecryptedPath), "ActiveFile");
                }

                ListViewItem.ListViewSubItem dateColumn = new ListViewItem.ListViewSubItem();
                dateColumn.Text = activeFile.LastAccessTimeUtc.ToLocalTime().ToString(CultureInfo.CurrentCulture);
                dateColumn.Tag = activeFile.LastAccessTimeUtc;
                dateColumn.Name = "Date";
                item.SubItems.Add(dateColumn);

                ListViewItem.ListViewSubItem encryptedPathColumn = new ListViewItem.ListViewSubItem();
                encryptedPathColumn.Name = "EncryptedPath";
                encryptedPathColumn.Text = activeFile.EncryptedPath;
                item.SubItems.Add(encryptedPathColumn);

                RecentFilesListView.Items.Add(item);
            }

            if (activeFile.Status.HasFlag(ActiveFileStatus.DecryptedIsPendingDelete) || activeFile.Status.HasFlag(ActiveFileStatus.AssumedOpenAndDecrypted))
            {
                item = new ListViewItem(Path.GetFileName(activeFile.DecryptedPath), activeFile.Key != null ? "ActiveFile" : "Exclamation");
                ListViewItem.ListViewSubItem encryptedPathColumn = new ListViewItem.ListViewSubItem();
                encryptedPathColumn.Name = "EncryptedPath";
                encryptedPathColumn.Text = activeFile.EncryptedPath;
                item.SubItems.Add(encryptedPathColumn);

                OpenFilesListView.Items.Add(item);
            }

            return activeFile;
        }

        private void toolStripButtonEncrypt_Click(object sender, EventArgs e)
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

            DoBackgroundWork(sourceFileInfo.FullName, null, (WorkerArguments arguments) =>
            {
                EncryptedFileManager.EncryptFile(sourceFileInfo, destinationFileInfo, key, arguments.Progress);
            });

            if (KnownKeys.DefaultEncryptionKey == null)
            {
                KnownKeys.DefaultEncryptionKey = key;
            }
        }

        private void DoBackgroundWork(string displayText, RunWorkerCompletedEventHandler completedHandler, Action<WorkerArguments> action)
        {
            BackgroundWorker worker = CreateWorker(completedHandler);
            worker.DoWork += (object sender, DoWorkEventArgs e) =>
            {
                WorkerArguments arguments = (WorkerArguments)e.Argument;
                action(arguments);
                e.Result = arguments.Result;
            };
            worker.RunWorkerAsync(new WorkerArguments(_progressManager.Create(displayText, worker)));
        }

        private BackgroundWorker CreateWorker(RunWorkerCompletedEventHandler completedHandler)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            ProgressBar progressBar = CreateProgressBar();
            _progressBars.Add(worker, progressBar);
            worker.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs e) =>
            {
                progressBar.Parent = null;
                _progressBars.Remove(worker);
                progressBar.Dispose();
                if (completedHandler != null)
                {
                    BeginInvoke((Action)(() => { completedHandler(sender, e); }));
                }
                worker.Dispose();
            };
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);

            return worker;
        }

        private ProgressBar CreateProgressBar()
        {
            ProgressBar progressBar = new ProgressBar();
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            ProgressPanel.Controls.Add(progressBar);
            progressBar.Dock = DockStyle.Fill;
            progressBar.Margin = new Padding(0);
            return progressBar;
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)e.UserState;
            ProgressBar progressBar = _progressBars[worker];
            progressBar.Value = e.ProgressPercentage;
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            ProgressBar progressBar = _progressBars[worker];
            progressBar.Parent = null;
            _progressBars.Remove(worker);
            progressBar.Dispose();
            worker.Dispose();
        }

        private void toolStripButtonDecrypt_Click(object sender, EventArgs e)
        {
            string[] fileNames;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = Resources.DecryptFileOpenDialogTitle;
                ofd.Multiselect = true;
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;
                ofd.DefaultExt = AxCryptEnvironment.Current.AxCryptExtension;
                ofd.Filter = Resources.EncryptedFileDialogFilterPattern.InvariantFormat("{0}".InvariantFormat(AxCryptEnvironment.Current.AxCryptExtension));
                ofd.Multiselect = true;
                ofd.Title = Resources.DecryptFileOpenDialogTitle;
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
            DoBackgroundWork(source.Name, null, (WorkerArguments arguments) =>
            {
                AxCryptFile.Decrypt(document, destination, AxCryptOptions.SetFileTimes, arguments.Progress);
                document.Dispose();
                AxCryptFile.Wipe(source);
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
                    document = AxCryptFile.Document(source, key, _progressManager.Create(Path.GetFileName(source.FullName)));
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
                    document = AxCryptFile.Document(source, passphrase.DerivedPassphrase, _progressManager.Create(Path.GetFileName(source.FullName)));
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
            _encryptedFileManager.CheckActiveFilesStatus(new ProgressContext());
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
                ofd.Filter = Resources.EncryptedFileDialogFilterPattern.InvariantFormat("{0}".InvariantFormat(AxCryptEnvironment.Current.AxCryptExtension));
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
            DoBackgroundWork(Path.GetFileName(file),
                (object sender, RunWorkerCompletedEventArgs e) =>
                {
                    FileOperationStatus status = (FileOperationStatus)e.Result;
                    if (status == FileOperationStatus.InvalidKey)
                    {
                        AskForPassphraseAndOpenEncrypted(file);
                    }
                    return;
                },
                (WorkerArguments arguments) =>
                {
                    arguments.Result = _encryptedFileManager.Open(file, KnownKeys.Keys, arguments.Progress);
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
            DoBackgroundWork(Path.GetFileName(file),
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
                    "Failed to decrypt and open {0}".InvariantFormat(file).ShowWarning();
                },
                (WorkerArguments arguments) =>
                {
                    arguments.Result = _encryptedFileManager.Open(file, new AesKey[] { passphrase.DerivedPassphrase }, arguments.Progress);
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
                Logging.Info("Tick");
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
                DoBackgroundWork("Updating Status",
                    (object sender1, RunWorkerCompletedEventArgs e1) =>
                    {
                        _pollingInProgress = false;
                    },
                    (WorkerArguments arguments) =>
                    {
                        _encryptedFileManager.CheckActiveFilesStatus(arguments.Progress);
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
            string encryptedPath = RecentFilesListView.SelectedItems[0].SubItems["EncryptedPath"].Text;

            OpenEncrypted(encryptedPath);
        }

        private void RecentFilesListView_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            ListView listView = (ListView)sender;
            string columnName = listView.Columns[e.ColumnIndex].Name;
            switch (columnName)
            {
                case "DecryptedFile":
                    Settings.Default.UserPreferences.RecentFilesDocumentWidth = listView.Columns[e.ColumnIndex].Width;
                    break;
            }
            Settings.Default.Save();
        }

        private void AxCryptMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            while (_progressBars.Count > 0)
            {
                Application.DoEvents();
            }
            PurgeActiveFiles();
            while (_progressBars.Count > 0)
            {
                Application.DoEvents();
            }
            Trace.Listeners.Remove("AxCryptMainFormListener");
        }

        private void PurgeActiveFiles()
        {
            _encryptedFileManager.IgnoreApplication = true;
            DoBackgroundWork("Purging Active Files",
                (object sender, RunWorkerCompletedEventArgs e) =>
                {
                    IList<ActiveFile> openFiles = _encryptedFileManager.FindOpenFiles();
                    if (openFiles.Count == 0)
                    {
                        return;
                    }
                    StringBuilder sb = new StringBuilder();
                    foreach (ActiveFile openFile in openFiles)
                    {
                        sb.Append("{0}\n".InvariantFormat(Path.GetFileName(openFile.DecryptedPath)));
                    }
                    sb.ToString().ShowWarning();
                },
                (WorkerArguments arguments) =>
                {
                    _encryptedFileManager.CheckActiveFilesStatus(arguments.Progress);
                    _encryptedFileManager.PurgeActiveFiles(arguments.Progress);
                });
        }

        private void OpenFilesListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string encryptedPath = OpenFilesListView.SelectedItems[0].SubItems["EncryptedPath"].Text;
            OpenEncrypted(encryptedPath);
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string encryptedPath = RecentFilesListView.SelectedItems[0].SubItems["EncryptedPath"].Text;
            _encryptedFileManager.RemoveRecentFile(encryptedPath);
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
            bool keyMatch = _encryptedFileManager.UpdateActiveFileIfKeyMatchesThumbprint(passphrase.DerivedPassphrase);
            if (keyMatch)
            {
                KnownKeys.Add(passphrase.DerivedPassphrase);
            }
        }
    }
}