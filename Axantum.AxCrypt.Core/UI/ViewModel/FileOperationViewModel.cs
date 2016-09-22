#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
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
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;
using static Axantum.AxCrypt.Common.FrameworkTypeExtensions;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class FileOperationViewModel : ViewModelBase
    {
        private FileSystemState _fileSystemState;

        private SessionNotify _sessionNotify;

        private KnownIdentities _knownIdentities;

        private ParallelFileOperation _fileOperation;

        private IStatusChecker _statusChecker;

        public IdentityViewModel IdentityViewModel { get; private set; }

        public FileOperationViewModel(FileSystemState fileSystemState, SessionNotify sessionNotify, KnownIdentities knownIdentities, ParallelFileOperation fileOperation, IStatusChecker statusChecker, IdentityViewModel identityViewModel)
        {
            _fileSystemState = fileSystemState;
            _sessionNotify = sessionNotify;
            _knownIdentities = knownIdentities;
            _fileOperation = fileOperation;
            _statusChecker = statusChecker;

            IdentityViewModel = identityViewModel;

            InitializePropertyValues();
            SubscribeToModelEvents();
        }

        private void SubscribeToModelEvents()
        {
            _sessionNotify.Notification += HandleSessionChanged;
        }

        private void InitializePropertyValues()
        {
            DecryptFiles = new AsyncDelegateAction<IEnumerable<string>>(async (files) => await DecryptFilesActionAsync(files));
            EncryptFiles = new AsyncDelegateAction<IEnumerable<string>>(async (files) => await EncryptFilesActionAsync(files));
            OpenFiles = new AsyncDelegateAction<IEnumerable<string>>(async (files) => await OpenFilesActionAsync(files));
            DecryptFolders = new AsyncDelegateAction<IEnumerable<string>>(async (folders) => await DecryptFoldersActionAsync(folders), (folders) => _knownIdentities.IsLoggedOn);
            WipeFiles = new AsyncDelegateAction<IEnumerable<string>>(async (files) => await WipeFilesActionAsync(files));
            RandomRenameFiles = new AsyncDelegateAction<IEnumerable<string>>(async (files) => await RandomRenameFilesActionAsync(files));
            OpenFilesFromFolder = new AsyncDelegateAction<string>(async (folder) => await OpenFilesFromFolderActionAsync(folder), (folder) => true);
            AddRecentFiles = new AsyncDelegateAction<IEnumerable<string>>(async (files) => await AddRecentFilesActionAsync(files));
            AsyncUpgradeFiles = new AsyncDelegateAction<IEnumerable<IDataContainer>>((containers) => UpgradeFilesActionAsync(containers), (containers) => _knownIdentities.IsLoggedOn);
        }

        public IAsyncAction DecryptFiles { get; private set; }

        public IAsyncAction EncryptFiles { get; private set; }

        public IAsyncAction OpenFiles { get; private set; }

        public IAsyncAction DecryptFolders { get; private set; }

        public IAsyncAction WipeFiles { get; private set; }

        public IAsyncAction RandomRenameFiles { get; private set; }

        public IAsyncAction OpenFilesFromFolder { get; private set; }

        public IAsyncAction AddRecentFiles { get; private set; }

        public IAsyncAction AsyncUpgradeFiles { get; private set; }

        public event EventHandler<FileSelectionEventArgs> SelectingFiles;

        protected virtual void OnSelectingFiles(FileSelectionEventArgs e)
        {
            SelectingFiles?.Invoke(this, e);
        }

        public event EventHandler<FileOperationEventArgs> FirstLegacyOpen;

        protected virtual void OnFirstLegacyOpen(FileOperationEventArgs e)
        {
            FirstLegacyOpen?.Invoke(this, e);
        }

        public event EventHandler<FileOperationEventArgs> ToggleLegacyConversion;

        protected virtual void OnToggleLegacyConversion(FileOperationEventArgs e)
        {
            ToggleLegacyConversion?.Invoke(this, e);
        }

        private async Task DecryptFoldersActionAsync(IEnumerable<string> folders)
        {
            await _fileOperation.DoFilesAsync(folders.Select(f => New<IDataContainer>(f)).ToList(), DecryptFolderWorkAsync, (status) => CheckStatusAndShowMessage(status, string.Empty));
        }

        private async Task EncryptFilesActionAsync(IEnumerable<string> files)
        {
            files = files ?? SelectFiles(FileSelectionType.Encrypt);
            if (!files.Any())
            {
                return;
            }
            if (!_knownIdentities.IsLoggedOn)
            {
                IdentityViewModel.AskForLogOnPassphrase.Execute(LogOnIdentity.Empty);
            }
            if (!_knownIdentities.IsLoggedOn)
            {
                return;
            }
            await EncryptOneOrManyFilesAsync(files.Select(f => New<IDataStore>(f)).ToList());
        }

        private async Task DecryptFilesActionAsync(IEnumerable<string> files)
        {
            files = files ?? SelectFiles(FileSelectionType.Decrypt);
            if (!files.Any())
            {
                return;
            }
            if (!_knownIdentities.IsLoggedOn)
            {
                IdentityViewModel.AskForDecryptPassphrase.Execute(files.First());
            }
            if (!_knownIdentities.IsLoggedOn)
            {
                return;
            }
            await _fileOperation.DoFilesAsync(files.Select(f => New<IDataStore>(f)).ToList(), DecryptFileWork, (status) => CheckStatusAndShowMessage(status, string.Empty));
        }

        private async Task WipeFilesActionAsync(IEnumerable<string> files)
        {
            files = files ?? SelectFiles(FileSelectionType.Wipe);
            if (!files.Any())
            {
                return;
            }
            await _fileOperation.DoFilesAsync(files.Select(f => New<IDataStore>(f)).ToList(), WipeFileWorkAsync, (status) => CheckStatusAndShowMessage(status, string.Empty));
        }

        private Task UpgradeFilesActionAsync(IEnumerable<IDataContainer> containers)
        {
            containers = containers ?? SelectFiles(FileSelectionType.Folder).Select((fn) => New<IDataContainer>(fn));

            if (!containers.Any())
            {
                return CompletedTask;
            }

            return _fileOperation.DoFilesAsync(new DataContainerCollection(containers).Where((ds) => ds.IsLegacyV1()), UpgradeFilesWorkAsync, (status) => CheckStatusAndShowMessage(status, string.Empty));
        }

        private async Task RandomRenameFilesActionAsync(IEnumerable<string> files)
        {
            files = files ?? SelectFiles(FileSelectionType.Rename);
            if (!files.Any())
            {
                return;
            }
            await _fileOperation.DoFilesAsync(files.Select(f => New<IDataStore>(f)).ToList(), RandomRenameFileWorkAsync, (status) => CheckStatusAndShowMessage(status, string.Empty));
        }

        private IEnumerable<string> SelectFiles(FileSelectionType fileSelectionType)
        {
            FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs(new string[0])
            {
                FileSelectionType = fileSelectionType,
            };
            OnSelectingFiles(fileSelectionArgs);
            if (fileSelectionArgs.Cancel)
            {
                return new string[0];
            }
            return fileSelectionArgs.SelectedFiles;
        }

        private async Task OpenFilesActionAsync(IEnumerable<string> files)
        {
            await _fileOperation.DoFilesAsync(files.Select(f => New<IDataStore>(f)).ToList(), OpenEncryptedWorkAsync, (status) => CheckStatusAndShowMessage(status, string.Empty));
            _fileSystemState.Save();
        }

        private Task<FileOperationContext> DecryptFileWork(IDataStore file, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            operationsController.QueryDecryptionPassphrase = HandleQueryDecryptionPassphraseEventAsync;

            operationsController.QuerySaveFileAs += (object sender, FileOperationEventArgs e) =>
            {
                FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs(new string[] { e.SaveFileFullName })
                {
                    FileSelectionType = FileSelectionType.SaveAsDecrypted,
                };
                OnSelectingFiles(fileSelectionArgs);
                if (fileSelectionArgs.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                e.SaveFileFullName = fileSelectionArgs.SelectedFiles[0];
            };

            operationsController.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
            {
                _knownIdentities.Add(e.LogOnIdentity);
            };

            operationsController.Completed += (object sender, FileOperationEventArgs e) =>
            {
                if (e.Status.ErrorStatus == ErrorStatus.Success)
                {
                    New<ActiveFileAction>().RemoveRecentFiles(new IDataStore[] { New<IDataStore>(e.OpenFileFullName) }, progress);
                }
            };

            return operationsController.DecryptFile(file);
        }

        private Task<FileOperationContext> WipeFileWorkAsync(IDataStore file, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            operationsController.WipeQueryConfirmation += (object sender, FileOperationEventArgs e) =>
            {
                FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs(new string[] { file.FullName, })
                {
                    FileSelectionType = FileSelectionType.WipeConfirm,
                };
                OnSelectingFiles(fileSelectionArgs);
                e.Cancel = fileSelectionArgs.Cancel;
                e.Skip = fileSelectionArgs.Skip;
                e.ConfirmAll = fileSelectionArgs.ConfirmAll;
                e.SaveFileFullName = fileSelectionArgs.SelectedFiles.FirstOrDefault() ?? String.Empty;
            };

            operationsController.Completed += (object sender, FileOperationEventArgs e) =>
            {
                if (e.Skip)
                {
                    return;
                }
                if (e.Status.ErrorStatus == ErrorStatus.Success)
                {
                    New<ActiveFileAction>().RemoveRecentFiles(new IDataStore[] { New<IDataStore>(e.SaveFileFullName) }, progress);
                }
            };

            return operationsController.WipeFile(file);
        }

        private Task<FileOperationContext> UpgradeFilesWorkAsync(IDataStore store, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            operationsController.QueryDecryptionPassphrase = HandleQueryDecryptionPassphraseEventAsync;

            operationsController.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
            {
                _knownIdentities.Add(e.LogOnIdentity);
            };

            return operationsController.UpgradeFileAsync(store);
        }

        private static bool CheckStatusAndShowMessage(FileOperationContext context, string fallbackName)
        {
            return Resolve.StatusChecker.CheckStatusAndShowMessage(context.ErrorStatus, string.IsNullOrEmpty(context.FullName) ? fallbackName : context.FullName, context.InternalMessage);
        }

        private Task<FileOperationContext> RandomRenameFileWorkAsync(IDataStore file, IProgressContext progress)
        {
            file.MoveTo(file.CreateRandomUniqueName().FullName);

            return Task.FromResult(new FileOperationContext(file.FullName, ErrorStatus.Success));
        }

        private async Task<FileOperationContext> OpenEncryptedWorkAsync(IDataStore file, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            operationsController.QueryDecryptionPassphrase = HandleQueryOpenPassphraseEventAsync;

            operationsController.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
            {
                if (!_fileSystemState.KnownPassphrases.Any(i => i.Thumbprint == e.LogOnIdentity.Passphrase.Thumbprint))
                {
                    _fileSystemState.KnownPassphrases.Add(e.LogOnIdentity.Passphrase);
                }
                _knownIdentities.Add(e.LogOnIdentity);
            };

            return await operationsController.DecryptAndLaunchAsync(file);
        }

        private async Task HandleQueryOpenPassphraseEventAsync(FileOperationEventArgs e)
        {
            await QueryDecryptPassphraseAsync(e);

            if (e.Cancel)
            {
                return;
            }

            if (Resolve.UserSettings.LegacyConversionMode == LegacyConversionMode.NotDecided)
            {
                Resolve.UserSettings.LegacyConversionMode = LegacyConversionMode.AutoConvertLegacyFiles;
            }
        }

        private async Task HandleQueryDecryptionPassphraseEventAsync(FileOperationEventArgs e)
        {
            await QueryDecryptPassphraseAsync(e);
        }

        private async Task QueryDecryptPassphraseAsync(FileOperationEventArgs e)
        {
            await IdentityViewModel.AskForDecryptPassphrase.ExecuteAsync(e.OpenFileFullName);
            if (IdentityViewModel.LogOnIdentity == LogOnIdentity.Empty)
            {
                e.Cancel = true;
                return;
            }
            e.LogOnIdentity = IdentityViewModel.LogOnIdentity;
        }

        private static FileOperationsController EncryptFileWorkController(IProgressContext progress)
        {
            FileOperationsController operationsController = New<IProgressContext, FileOperationsController>(progress);

            operationsController.QuerySaveFileAs += (object sender, FileOperationEventArgs e) =>
            {
                using (FileLock lockedSave = e.SaveFileFullName.CreateUniqueFile())
                {
                    e.SaveFileFullName = lockedSave.DataStore.FullName;
                    lockedSave.DataStore.Delete();
                }
            };

            return operationsController;
        }

        private Task<FileOperationContext> EncryptFileWorkOneAsync(IDataStore file, IProgressContext progress)
        {
            FileOperationsController controller = EncryptFileWorkController(progress);
            controller.Completed += (object sender, FileOperationEventArgs e) =>
            {
                if (e.Status.ErrorStatus == ErrorStatus.Success)
                {
                    IDataStore encryptedInfo = New<IDataStore>(e.SaveFileFullName);
                    IDataStore decryptedInfo = New<IDataStore>(FileOperation.GetTemporaryDestinationName(e.OpenFileFullName));
                    ActiveFile activeFile = new ActiveFile(encryptedInfo, decryptedInfo, e.LogOnIdentity, ActiveFileStatus.NotDecrypted, e.CryptoId);
                    _fileSystemState.Add(activeFile);
                }
            };
            return controller.EncryptFile(file);
        }

        private Task<FileOperationContext> EncryptFileWorkManyAsync(IDataStore file, IProgressContext progress)
        {
            FileOperationsController controller = EncryptFileWorkController(progress);
            return controller.EncryptFile(file);
        }

        private Task<FileOperationContext> VerifyAndAddActiveWorkAsync(IDataStore fullName, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            operationsController.QueryDecryptionPassphrase += HandleQueryDecryptionPassphraseEventAsync;

            operationsController.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
            {
                if (!_fileSystemState.KnownPassphrases.Any(i => i.Thumbprint == e.LogOnIdentity.Passphrase.Thumbprint))
                {
                    _fileSystemState.KnownPassphrases.Add(e.LogOnIdentity.Passphrase);
                }
                _knownIdentities.Add(e.LogOnIdentity);
            };

            operationsController.Completed += (object sender, FileOperationEventArgs e) =>
            {
                if (e.Status.ErrorStatus == ErrorStatus.Success)
                {
                    IDataStore encryptedInfo = New<IDataStore>(e.OpenFileFullName);
                    IDataStore decryptedInfo = New<IDataStore>(FileOperation.GetTemporaryDestinationName(e.SaveFileFullName));
                    ActiveFile activeFile = new ActiveFile(encryptedInfo, decryptedInfo, e.LogOnIdentity, ActiveFileStatus.NotDecrypted, e.CryptoId);
                    _fileSystemState.Add(activeFile);
                }
            };

            return operationsController.VerifyEncrypted(fullName);
        }

        private async Task OpenFilesFromFolderActionAsync(string folder)
        {
            FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs(new string[] { folder })
            {
                FileSelectionType = FileSelectionType.Open,
            };
            OnSelectingFiles(fileSelectionArgs);
            if (fileSelectionArgs.Cancel)
            {
                return;
            }
            await OpenFilesActionAsync(fileSelectionArgs.SelectedFiles);
        }

        private async Task<FileOperationContext> DecryptFolderWorkAsync(IDataContainer folder, IProgressContext progress)
        {
            await New<AxCryptFile>().DecryptFilesInsideFolderUniqueWithWipeOfOriginalAsync(folder, _knownIdentities.DefaultEncryptionIdentity, _statusChecker, progress);
            return new FileOperationContext(String.Empty, ErrorStatus.Success);
        }

        private async Task AddRecentFilesActionAsync(IEnumerable<string> files)
        {
            IEnumerable<IDataStore> fileInfos = files.Select(f => New<IDataStore>(f)).ToList();
            await EncryptOneOrManyFilesAsync(fileInfos.Where(fileInfo => Resolve.KnownIdentities.IsLoggedOn && fileInfo.Type() == FileInfoTypes.EncryptableFile));
            await ProcessEncryptedFilesDroppedInRecentListAsync(fileInfos.Where(fileInfo => fileInfo.Type() == FileInfoTypes.EncryptedFile));
        }

        private async Task ProcessEncryptedFilesDroppedInRecentListAsync(IEnumerable<IDataStore> encryptedFiles)
        {
            await _fileOperation.DoFilesAsync(encryptedFiles, VerifyAndAddActiveWorkAsync, (status) => CheckStatusAndShowMessage(status, string.Empty));
            _fileSystemState.Save();
        }

        private async Task EncryptOneOrManyFilesAsync(IEnumerable<IDataStore> encryptableFiles)
        {
            if (!encryptableFiles.Any())
            {
                return;
            }
            if (encryptableFiles.Count() > 1)
            {
                await _fileOperation.DoFilesAsync(encryptableFiles, EncryptFileWorkManyAsync, (status) => CheckEncryptionStatus(status));
            }
            else
            {
                await _fileOperation.DoFilesAsync(encryptableFiles, EncryptFileWorkOneAsync, (status) => CheckEncryptionStatus(status));
            }
            New<FileSystemState>().Save();
        }

        private static void CheckEncryptionStatus(FileOperationContext foc)
        {
            if (foc.ErrorStatus == ErrorStatus.FileAlreadyEncrypted)
            {
                foc = new FileOperationContext(foc.FullName, ErrorStatus.Success);
            }

            CheckStatusAndShowMessage(foc, string.Empty);
        }

        private void HandleSessionChanged(object sender, SessionNotificationEventArgs e)
        {
            switch (e.Notification.NotificationType)
            {
                case SessionNotificationType.LogOff:
                case SessionNotificationType.LogOn:
                case SessionNotificationType.SessionStart:
                    ((AsyncDelegateAction<string>)OpenFilesFromFolder).RaiseCanExecuteChanged();
                    break;
            }
        }
    }
}