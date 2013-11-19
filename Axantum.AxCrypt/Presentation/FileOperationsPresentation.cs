using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Axantum.AxCrypt.Presentation
{
    public class FileOperationsPresentation
    {
        private PassphrasePresentation _passphrasePresentation;

        public FileOperationsPresentation(IMainView mainView)
        {
            _passphrasePresentation = new PassphrasePresentation(mainView);
        }

        public void EncryptFilesViaDialog()
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
                Instance.ParallelBackground.DoFiles(ofd.FileNames.Select(f => OS.Current.FileInfo(f)), EncryptFile, (status) => { });
            }
        }

        public void DecryptFilesViaDialog()
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
            Instance.ParallelBackground.DoFiles(fileNames.Select(f => OS.Current.FileInfo(f)), DecryptFile, (status) => { });
        }

        public void OpenFilesViaDialog(IRuntimeFileInfo fileInfo)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (fileInfo != null && fileInfo.IsFolder)
                {
                    ofd.InitialDirectory = fileInfo.FullName;
                }
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

                Instance.ParallelBackground.DoFiles(ofd.FileNames.Select(f => OS.Current.FileInfo(f)), OpenEncrypted, (status) => { });
            }
        }

        public void WipeFilesViaDialog()
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
            Instance.ParallelBackground.DoFiles(fileNames.Select(f => OS.Current.FileInfo(f)), WipeFile, (status) => { });
        }

        public void EncryptFileNonInteractive(IRuntimeFileInfo fullName, IThreadWorker worker, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            operationsController.QuerySaveFileAs += (object sender, FileOperationEventArgs e) =>
            {
                e.SaveFileFullName = OS.Current.FileInfo(e.SaveFileFullName).FullName.CreateUniqueFile();
            };

            operationsController.Completed += (object sender, FileOperationEventArgs e) =>
            {
                if (FactoryRegistry.Instance.Singleton<IStatusChecker>().CheckStatusAndShowMessage(e.Status, e.OpenFileFullName))
                {
                    IRuntimeFileInfo encryptedInfo = OS.Current.FileInfo(e.SaveFileFullName);
                    IRuntimeFileInfo decryptedInfo = OS.Current.FileInfo(FileOperation.GetTemporaryDestinationName(e.OpenFileFullName));
                    ActiveFile activeFile = new ActiveFile(encryptedInfo, decryptedInfo, e.Key, ActiveFileStatus.NotDecrypted);
                    Instance.FileSystemState.Add(activeFile);
                    Instance.FileSystemState.Save();
                }
            };

            operationsController.EncryptFile(fullName, worker);
        }

        public void VerifyAndAddActive(IRuntimeFileInfo fullName, IThreadWorker worker, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            operationsController.QueryDecryptionPassphrase += HandleQueryDecryptionPassphraseEvent;

            operationsController.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
            {
                Instance.KnownKeys.Add(e.Key);
            };

            operationsController.Completed += (object sender, FileOperationEventArgs e) =>
            {
                if (e.Skip)
                {
                    return;
                }
                if (FactoryRegistry.Instance.Singleton<IStatusChecker>().CheckStatusAndShowMessage(e.Status, e.OpenFileFullName))
                {
                    IRuntimeFileInfo encryptedInfo = OS.Current.FileInfo(e.OpenFileFullName);
                    IRuntimeFileInfo decryptedInfo = OS.Current.FileInfo(e.SaveFileFullName);
                    ActiveFile activeFile = new ActiveFile(encryptedInfo, decryptedInfo, e.Key, ActiveFileStatus.NotDecrypted);
                    Instance.FileSystemState.Add(activeFile);
                    Instance.FileSystemState.Save();
                }
            };

            operationsController.VerifyEncrypted(fullName, worker);
        }

        private void EncryptFile(IRuntimeFileInfo file, IThreadWorker worker, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

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
                string passphrase = _passphrasePresentation.AskForLogOnPassphrase(PassphraseIdentity.Empty);
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
                if (FactoryRegistry.Instance.Singleton<IStatusChecker>().CheckStatusAndShowMessage(e.Status, e.OpenFileFullName))
                {
                    IRuntimeFileInfo encryptedInfo = OS.Current.FileInfo(e.SaveFileFullName);
                    IRuntimeFileInfo decryptedInfo = OS.Current.FileInfo(FileOperation.GetTemporaryDestinationName(e.OpenFileFullName));
                    ActiveFile activeFile = new ActiveFile(encryptedInfo, decryptedInfo, e.Key, ActiveFileStatus.NotDecrypted);
                    Instance.FileSystemState.Add(activeFile);
                    Instance.FileSystemState.Save();
                }
            };

            operationsController.EncryptFile(file, worker);
        }

        public void DecryptFile(IRuntimeFileInfo file, IThreadWorker worker, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

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
                    sfd.InitialDirectory = Path.GetDirectoryName(file.FullName);
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
                if (FactoryRegistry.Instance.Singleton<IStatusChecker>().CheckStatusAndShowMessage(e.Status, e.OpenFileFullName))
                {
                    Instance.FileSystemState.Actions.RemoveRecentFiles(new IRuntimeFileInfo[] { OS.Current.FileInfo(e.OpenFileFullName) }, progress);
                }
            };

            operationsController.DecryptFile(file, worker);
        }

        private void WipeFile(IRuntimeFileInfo file, IThreadWorker worker, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            operationsController.WipeQueryConfirmation += (object sender, FileOperationEventArgs e) =>
            {
                using (ConfirmWipeDialog cwd = new ConfirmWipeDialog())
                {
                    cwd.FileNameLabel.Text = Path.GetFileName(file.FullName);
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
                if (e.Skip)
                {
                    return;
                }
                if (FactoryRegistry.Instance.Singleton<IStatusChecker>().CheckStatusAndShowMessage(e.Status, e.OpenFileFullName))
                {
                    Instance.FileSystemState.Actions.RemoveRecentFiles(new IRuntimeFileInfo[] { OS.Current.FileInfo(e.SaveFileFullName) }, progress);
                }
            };

            operationsController.WipeFile(file, worker);
        }

        public void OpenEncrypted(IRuntimeFileInfo file, IThreadWorker worker, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

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
                FactoryRegistry.Instance.Singleton<IStatusChecker>().CheckStatusAndShowMessage(e.Status, e.OpenFileFullName);
            };

            operationsController.DecryptAndLaunch(file, worker);
        }

        private void HandleQueryDecryptionPassphraseEvent(object sender, FileOperationEventArgs e)
        {
            string passphraseText = _passphrasePresentation.AskForLogOnOrDecryptPassphrase(e.OpenFileFullName);
            if (String.IsNullOrEmpty(passphraseText))
            {
                e.Cancel = true;
                return;
            }
            e.Passphrase = passphraseText;
        }
    }
}