using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using AxCrypt.Content;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Forms.Implementation
{
    public class FileFolderSelection : IDataItemSelection
    {
        private IWin32Window _owner;

        public FileFolderSelection(IWin32Window owner)
        {
            _owner = owner;
        }

        public Task HandleSelection(FileSelectionEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            try
            {
                New<IMainUI>().Enabled = false;
                HandleSelectionInternal(e);
            }
            finally
            {
                New<IMainUI>().Enabled = true;
            }
            return Constant.CompletedTask;
        }

        private void HandleSelectionInternal(FileSelectionEventArgs e)
        {
            switch (e.FileSelectionType)
            {
                case FileSelectionType.SaveAsEncrypted:
                case FileSelectionType.SaveAsDecrypted:
                    HandleSaveAsFileSelection(e);
                    break;

                case FileSelectionType.WipeConfirm:
                    HandleWipeConfirm(e);
                    break;

                case FileSelectionType.Folder:
                    HandleFolderSelection(e);
                    break;

                default:
                    HandleOpenFileSelection(e);
                    break;
            }
        }

        private void HandleFolderSelection(FileSelectionEventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.ShowNewFolderButton = false;
                fbd.Description = Texts.UpgradeLegacyFilesMenuToolTip;
                fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                DialogResult result = fbd.ShowDialog(_owner);
                e.SelectedFiles.Clear();
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
                else
                {
                    e.SelectedFiles.Add(fbd.SelectedPath);
                }
            }
        }

        private static void HandleWipeConfirm(FileSelectionEventArgs e)
        {
            using (ConfirmWipeDialog cwd = new ConfirmWipeDialog())
            {
                cwd.FileNameLabel.Text = Path.GetFileName(e.SelectedFiles[0]);
                e.Skip = false;
                DialogResult confirmResult = cwd.ShowDialog();
                e.ConfirmAll = cwd._confirmAllCheckBox.Checked;
                e.Skip = confirmResult == DialogResult.No;
                e.Cancel = confirmResult == DialogResult.Cancel;
            }
        }

        private static void HandleOpenFileSelection(FileSelectionEventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (e.SelectedFiles != null && e.SelectedFiles.Count > 0 && !String.IsNullOrEmpty(e.SelectedFiles[0]))
                {
                    IDataContainer initialFolder = New<IDataContainer>(e.SelectedFiles[0]);
                    if (initialFolder.IsAvailable)
                    {
                        ofd.InitialDirectory = initialFolder.FullName;
                    }
                }
                switch (e.FileSelectionType)
                {
                    case FileSelectionType.Decrypt:
                        ofd.Title = Texts.DecryptFileOpenDialogTitle;
                        ofd.Multiselect = true;
                        ofd.CheckFileExists = true;
                        ofd.CheckPathExists = true;
                        ofd.DefaultExt = New<IRuntimeEnvironment>().AxCryptExtension;
                        ofd.Filter = Texts.FileFilterDialogFilterPatternWin.InvariantFormat(ofd.DefaultExt, Texts.FileFilterFileTypeAxCryptFiles, Texts.FileFilterFileTypeAllFiles);
                        ofd.Multiselect = true;
                        break;

                    case FileSelectionType.Rename:
                        ofd.Title = Texts.AnonymousRenameSelectFilesDialogTitle;
                        ofd.Multiselect = true;
                        ofd.CheckFileExists = true;
                        ofd.CheckPathExists = true;
                        ofd.DefaultExt = New<IRuntimeEnvironment>().AxCryptExtension;
                        ofd.Filter = Texts.FileFilterDialogFilterPatternWin.InvariantFormat(ofd.DefaultExt, Texts.FileFilterFileTypeAxCryptFiles, Texts.FileFilterFileTypeAllFiles);
                        ofd.Multiselect = true;
                        break;

                    case FileSelectionType.Encrypt:
                        ofd.Title = Texts.EncryptFileOpenDialogTitle;
                        ofd.Multiselect = true;
                        ofd.CheckFileExists = true;
                        ofd.CheckPathExists = true;
                        break;

                    case FileSelectionType.Open:
                        ofd.Title = Texts.OpenEncryptedFileOpenDialogTitle;
                        ofd.Multiselect = false;
                        ofd.CheckFileExists = true;
                        ofd.CheckPathExists = true;
                        ofd.DefaultExt = New<IRuntimeEnvironment>().AxCryptExtension;
                        ofd.Filter = Texts.FileFilterDialogFilterPatternWin.InvariantFormat(ofd.DefaultExt, Texts.FileFilterFileTypeAxCryptFiles, Texts.FileFilterFileTypeAllFiles);
                        break;

                    case FileSelectionType.Wipe:
                        ofd.Title = Texts.WipeFileSelectFileDialogTitle;
                        ofd.Multiselect = true;
                        ofd.CheckFileExists = true;
                        ofd.CheckPathExists = true;
                        break;

                    case FileSelectionType.ImportPublicKeys:
                        ofd.Title = Texts.ImportPublicKeysFileSelectionTitle;
                        ofd.Multiselect = true;
                        ofd.CheckFileExists = true;
                        ofd.CheckPathExists = true;
                        ofd.Filter = Texts.ImportPublicKeysFileFilter;
                        break;

                    case FileSelectionType.ImportPrivateKeys:
                        ofd.Title = Texts.ImportPrivateKeysFileSelectionTitle;
                        ofd.Multiselect = false;
                        ofd.CheckFileExists = true;
                        ofd.CheckPathExists = true;
                        ofd.Filter = Texts.ImportPrivateKeysFileFilter;
                        break;

                    case FileSelectionType.KeySharing:
                        ofd.Title = Texts.ShareKeysFileOpenDialogTitle;
                        ofd.Multiselect = true;
                        ofd.CheckFileExists = true;
                        ofd.CheckPathExists = true;
                        ofd.DefaultExt = New<IRuntimeEnvironment>().AxCryptExtension;
                        ofd.Filter = Texts.FileFilterDialogFilterPatternWin.InvariantFormat(ofd.DefaultExt, Texts.FileFilterFileTypeAxCryptFiles, Texts.FileFilterFileTypeAllFiles);
                        break;

                    default:
                        break;
                }
                DialogResult result = ofd.ShowDialog();
                e.Cancel = result != DialogResult.OK;
                e.SelectedFiles.Clear();
                foreach (string fileName in ofd.FileNames)
                {
                    e.SelectedFiles.Add(fileName);
                }
            }
        }

        private static void HandleSaveAsFileSelection(FileSelectionEventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                switch (e.FileSelectionType)
                {
                    case FileSelectionType.SaveAsEncrypted:
                        sfd.Title = Texts.EncryptFileSaveAsDialogTitle;
                        sfd.DefaultExt = OS.Current.AxCryptExtension;
                        sfd.AddExtension = true;
                        sfd.Filter = Texts.FileFilterDialogFilterPatternWin.InvariantFormat(sfd.DefaultExt, Texts.FileFilterFileTypeAxCryptFiles, Texts.FileFilterFileTypeAllFiles);
                        break;

                    case FileSelectionType.SaveAsDecrypted:
                        string extension = Path.GetExtension(e.SelectedFiles[0]);
                        sfd.Title = Texts.DecryptedSaveAsFileDialogTitle;
                        sfd.DefaultExt = extension;
                        sfd.AddExtension = !String.IsNullOrEmpty(extension);
                        sfd.Filter = Texts.FileFilterDialogFilterPatternWin.InvariantFormat(sfd.DefaultExt, Texts.FileFilterFileTypeFiles, Texts.FileFilterFileTypeAllFiles);
                        break;
                }
                sfd.CheckPathExists = true;
                sfd.FileName = Path.GetFileName(e.SelectedFiles[0]);
                sfd.InitialDirectory = Path.GetDirectoryName(e.SelectedFiles[0]);
                sfd.ValidateNames = true;
                sfd.OverwritePrompt = true;
                sfd.RestoreDirectory = true;
                DialogResult saveAsResult = sfd.ShowDialog();
                e.Cancel = saveAsResult != DialogResult.OK;
                e.SelectedFiles[0] = sfd.FileName;
            }
        }
    }
}