using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
            ActiveFileState.Changed += new EventHandler<EventArgs>(ActiveFileState_Changed);
        }

        private void ActiveFileState_Changed(object sender, EventArgs e)
        {
            OpenFilesListView.Clear();
            foreach (ActiveFile activeFile in ActiveFileState.ActiveFiles)
            {
                OpenFilesListView.Items.Add(new ListViewItem(Path.GetFileName(activeFile.DecryptedPath)));
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
            ActiveFileState.CheckActiveFilesStatus();
        }
    }
}