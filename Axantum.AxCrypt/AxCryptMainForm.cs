using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Properties;

namespace Axantum.AxCrypt
{
    public partial class AxCryptMainForm : Form
    {
        public AxCryptMainForm()
        {
            InitializeComponent();
            _messageBoxOptions = RightToLeft == RightToLeft.Yes ? MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading : 0;

            EncryptedFileManager.IgnoreApplication = !AxCryptEnvironment.Current.IsDesktopWindows;
            EncryptedFileManager.Changed += new EventHandler<EventArgs>(ActiveFileState_Changed);
            EncryptedFileManager.ForceActiveFilesStatus();
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

        private void ActiveFileState_Changed(object sender, EventArgs e)
        {
            OpenFilesListView.Items.Clear();
            RecentFilesListView.Items.Clear();
            ActiveFileMonitor.ForEach(false, (ActiveFile activeFile) => { return UpdateOpenFilesWith(activeFile); });
        }

        private ActiveFile UpdateOpenFilesWith(ActiveFile activeFile)
        {
            ListViewItem item;
            if (activeFile.Status.HasFlag(ActiveFileStatus.NotDecrypted))
            {
                if (String.IsNullOrEmpty(activeFile.DecryptedPath))
                {
                    item = new ListViewItem(Path.GetFileName(activeFile.DecryptedPath), "InactiveFile");
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
                item = new ListViewItem(Path.GetFileName(activeFile.DecryptedPath), "ActiveFile");
                item.SubItems.Add(activeFile.EncryptedPath);
                OpenFilesListView.Items.Add(item);
            }
            return activeFile;
        }

        private MessageBoxOptions _messageBoxOptions;

        private void toolStripButtonEncrypt_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = Resources.EncryptFileOpenDialogTitle;
                ofd.Multiselect = true;
                ofd.ShowDialog();
            }
        }

        private void toolStripButtonDecrypt_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = Resources.DecryptFileOpenDialogTitle;
                ofd.Multiselect = true;
                ofd.ShowDialog();
            }
        }

        private void FormAxCryptMain_Load(object sender, EventArgs e)
        {
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
                ofd.Filter = Resources.EncryptedFileDialogFilterPattern.InvariantFormat("*{0}".InvariantFormat(AxCryptEnvironment.Current.AxCryptExtension));
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
            if (EncryptedFileManager.Open(file, KnownKeys.Keys) == FileOperationStatus.InvalidKey)
            {
                Passphrase passphrase;
                FileOperationStatus status;
                do
                {
                    DecryptionPassphraseDialog passphraseDialog = new DecryptionPassphraseDialog();
                    DialogResult dialogResult = passphraseDialog.ShowDialog();
                    if (dialogResult != DialogResult.OK)
                    {
                        return;
                    }
                    passphrase = new Passphrase(passphraseDialog.Passphrase.Text);
                    status = EncryptedFileManager.Open(file, new AesKey[] { passphrase.DerivedPassphrase });
                } while (status == FileOperationStatus.InvalidKey);
                if (status != FileOperationStatus.Success)
                {
                    ShowMessageBox("Failed to decrypt and open {0}".InvariantFormat(file));
                }
            }
        }

        private void ShowMessageBox(string message)
        {
            MessageBox.Show(message, "AxCypt", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, _messageBoxOptions);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ActiveFilePolling_Tick(object sender, EventArgs e)
        {
            EncryptedFileManager.CheckActiveFilesStatus();
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
            EncryptedFileManager.IgnoreApplication = true;
            EncryptedFileManager.CheckActiveFilesStatus();
            EncryptedFileManager.PurgeActiveFiles();
        }
    }
}