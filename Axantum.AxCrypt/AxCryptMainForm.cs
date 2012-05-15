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
using Axantum.AxCrypt.Properties;

namespace Axantum.AxCrypt
{
    public partial class AxCryptMainForm : Form
    {
        public AxCryptMainForm()
        {
            InitializeComponent();
            _messageBoxOptions = RightToLeft == RightToLeft.Yes ? MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading : 0;
            EncryptedFileManager.Changed += new EventHandler<EventArgs>(ActiveFileState_Changed);
        }

        private void ActiveFileState_Changed(object sender, EventArgs e)
        {
            OpenFilesListView.Items.Clear();
            RecentFilesListView.Items.Clear();
            ActiveFileMonitor.ForEach((ActiveFile activeFile) => { UpdateOpenFilesWith(activeFile); });
        }

        private void UpdateOpenFilesWith(ActiveFile activeFile)
        {
            ListViewItem item;
            if (activeFile.Status == ActiveFileStatus.Deleted)
            {
                item = new ListViewItem(Path.GetFileName(activeFile.DecryptedPath), "InactiveFile");
                item.SubItems.Add(activeFile.EncryptedPath);
                ListViewItem.ListViewSubItem dateColumn = new ListViewItem.ListViewSubItem();
                dateColumn.Text = activeFile.LastAccessTimeUtc.ToString(CultureInfo.CurrentCulture);
                dateColumn.Tag = activeFile.LastAccessTimeUtc;
                item.SubItems.Add(dateColumn);
                RecentFilesListView.Items.Add(item);
            }
            if (activeFile.Status == ActiveFileStatus.Locked)
            {
                item = new ListViewItem(Path.GetFileName(activeFile.DecryptedPath), "ActiveFile");
                item.SubItems.Add(activeFile.EncryptedPath);
                OpenFilesListView.Items.Add(item);
            }
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
                DialogResult result = ofd.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return;
                }

                foreach (string file in ofd.FileNames)
                {
                    if (EncryptedFileManager.Open(file) != FileOperationStatus.Success)
                    {
                        ShowMessageBox("Failed to decrypt and open {0}".InvariantFormat(file));
                    }
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
    }
}