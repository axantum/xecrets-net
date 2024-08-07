﻿#region Coypright and License

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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.Header;
using AxCrypt.Core.IO;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.Session;
using AxCrypt.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;
using static AxCrypt.Common.FrameworkTypeExtensions;

namespace AxCrypt.Core.UI.ViewModel
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "It's actually true, and this should be refactored later.")]
    public class FileOperationViewModel : ViewModelBase
    {
        private FileSystemState _fileSystemState;

        private SessionNotify _sessionNotify;

        private KnownIdentities _knownIdentities;

        private ParallelFileOperation _fileOperation;

        private IStatusChecker _statusChecker;

        public IdentityViewModel IdentityViewModel { get; private set; }

        public IEnumerable<UserPublicKey> Recipients { get; set; } = null;

        public FileOperationViewModel(FileSystemState fileSystemState, SessionNotify sessionNotify, KnownIdentities knownIdentities, ParallelFileOperation fileOperation, IStatusChecker statusChecker, IdentityViewModel identityViewModel)
        {
            _fileSystemState = fileSystemState;
            _sessionNotify = sessionNotify;
            _knownIdentities = knownIdentities;
            _fileOperation = fileOperation;
            _statusChecker = statusChecker;

            IdentityViewModel = identityViewModel;
            Recipients = null;

            InitializePropertyValues();
            SubscribeToModelEvents();
        }

        private void SubscribeToModelEvents()
        {
            _sessionNotify.AddCommand(HandleSessionChanged);
        }

        private void InitializePropertyValues()
        {
            DecryptFiles = new AsyncDelegateAction<IEnumerable<string>>((files) => DecryptFilesActionAsync(files));
            EncryptFiles = new AsyncDelegateAction<IEnumerable<string>>((files) => EncryptFilesActionAsync(files));
            OpenFiles = new AsyncDelegateAction<IEnumerable<string>>((files) => OpenFilesActionAsync(files));
            DecryptFolders = new AsyncDelegateAction<IEnumerable<string>>((folders) => DecryptFoldersActionAsync(folders), (folders) => Task.FromResult(_knownIdentities.IsLoggedOn));
            WipeFiles = new AsyncDelegateAction<IEnumerable<string>>((files) => WipeFilesActionAsync(files));
            RandomRenameFiles = new AsyncDelegateAction<IEnumerable<string>>((files) => RandomRenameFilesActionAsync(files));
            RestoreRandomRenameFiles = new AsyncDelegateAction<IEnumerable<string>>((files) => RestoreRandomRenameFilesActionAsync(files));
            OpenFilesFromFolder = new AsyncDelegateAction<string>((folder) => OpenFilesFromFolderActionAsync(folder), (folder) => Task.FromResult(true));
            AddRecentFiles = new AsyncDelegateAction<IEnumerable<string>>((files) => AddRecentFilesActionAsync(files));
            AsyncEncryptionUpgrade = new AsyncDelegateAction<IEnumerable<IDataContainer>>((containers) => EncryptionUpgradeActionAsync(containers), (containers) => Task.FromResult(_knownIdentities.IsLoggedOn));
            ShowInFolder = new AsyncDelegateAction<IEnumerable<string>>((files) => ShowInFolderActionAsync(files));
            TryBrokenFiles = new AsyncDelegateAction<IEnumerable<string>>((files) => TryBrokenFilesActionAsync(files));
            VerifyFiles = new AsyncDelegateAction<IEnumerable<string>>((files) => VerifyFilesActionAsync(files));
            IntegrityCheckFiles = new AsyncDelegateAction<IEnumerable<string>>((files) => IntegrityCheckFilesActionAsync(files));
        }

        public IAsyncAction DecryptFiles { get; private set; }

        public IAsyncAction EncryptFiles { get; private set; }

        public IAsyncAction OpenFiles { get; private set; }

        public IAsyncAction DecryptFolders { get; private set; }

        public IAsyncAction WipeFiles { get; private set; }

        public IAsyncAction RandomRenameFiles { get; private set; }

        public IAsyncAction RestoreRandomRenameFiles { get; private set; }

        public IAsyncAction OpenFilesFromFolder { get; private set; }

        public IAsyncAction AddRecentFiles { get; private set; }

        public IAsyncAction AsyncEncryptionUpgrade { get; private set; }

        public IAsyncAction ShowInFolder { get; private set; }

        public IAsyncAction TryBrokenFiles { get; private set; }

        public IAsyncAction VerifyFiles { get; private set; }

        public IAsyncAction IntegrityCheckFiles { get; private set; }

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

        public event EventHandler<FileOperationEventArgs> ToggleEncryptionUpgradeMode;

        protected virtual void OnToggleEncryptionUpgradeMode(FileOperationEventArgs e)
        {
            ToggleEncryptionUpgradeMode?.Invoke(this, e);
        }

        private async Task DecryptFoldersActionAsync(IEnumerable<string> folders)
        {
            await _fileOperation.DoFilesAsync(folders.Select(f => New<IDataContainer>(f)).ToList(), DecryptFolderWorkAsync, (status) => Task.FromResult(CheckStatusAndShowMessage(status, string.Empty)));
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
                await IdentityViewModel.AskForLogOnPassphrase.ExecuteAsync(LogOnIdentity.Empty);
            }
            if (!_knownIdentities.IsLoggedOn)
            {
                return;
            }
            IEnumerable<IDataStore> fileInfos = files.Select(f => New<IDataStore>(f)).ToList();
            await EncryptFewOrManyFilesAsync(fileInfos);
        }

        private async Task DecryptFilesActionAsync(IEnumerable<string> files)
        {
            files = files ?? SelectFiles(FileSelectionType.Decrypt);
            if (!files.Any())
            {
                return;
            }
            await _fileOperation.DoFilesAsync(files.Select(f => New<IDataStore>(f)).ToList(), DecryptFileWork, (status) => Task.FromResult(CheckStatusAndShowMessage(status, string.Empty)));
        }

        private async Task WipeFilesActionAsync(IEnumerable<string> files)
        {
            files = files ?? SelectFiles(FileSelectionType.Wipe);
            if (!files.Any())
            {
                return;
            }
            await _fileOperation.DoFilesAsync(files.Select(f => New<IDataStore>(f)).ToList(), WipeFileWorkAsync, (status) => Task.FromResult(CheckStatusAndShowMessage(status, string.Empty)));
        }

        private Task EncryptionUpgradeActionAsync(IEnumerable<IDataContainer> containers)
        {
            containers = containers ?? SelectFiles(FileSelectionType.Folder).Select((fn) => New<IDataContainer>(fn));

            if (!containers.Any())
            {
                return CompletedTask;
            }

            return _fileOperation.DoFilesAsync(new DataContainerCollection(containers).Where((ds) => ds.IsEncrypted()), EncryptionUpgradeWorkAsync, (status) => Task.FromResult(CheckStatusAndShowMessage(status, string.Empty)));
        }

        private async Task RandomRenameFilesActionAsync(IEnumerable<string> files)
        {
            files = files ?? SelectFiles(FileSelectionType.Rename);
            if (!files.Any())
            {
                return;
            }
            await _fileOperation.DoFilesAsync(files.Select(f => New<IDataStore>(f)).ToList(), RandomRenameFileWorkAsync, (status) => Task.FromResult(CheckStatusAndShowMessage(status, string.Empty)));
        }

        private async Task RestoreRandomRenameFilesActionAsync(IEnumerable<string> files)
        {
            files = files ?? SelectFiles(FileSelectionType.Rename);
            if (!files.Any())
            {
                return;
            }
            await _fileOperation.DoFilesAsync(files.Select(f => New<IDataStore>(f)).ToList(), RestoreRandomRenameFilesWorkAsync, (status) => Task.FromResult(CheckStatusAndShowMessage(status, string.Empty)));
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
            await _fileOperation.DoFilesAsync(files.Select(f => New<IDataStore>(f)).ToList(), OpenEncryptedWorkAsync, (status) => Task.FromResult(CheckStatusAndShowMessage(status, string.Empty)));
            await _fileSystemState.Save();
        }

        private Task<FileOperationContext> DecryptFileWork(IDataStore file, IProgressContext progress)
        {
            ActiveFile activeFile = New<FileSystemState>().FindActiveFileFromEncryptedPath(file.FullName);
            if (activeFile != null && activeFile.Status.HasFlag(ActiveFileStatus.AssumedOpenAndDecrypted))
            {
                return Task.FromResult(new FileOperationContext(file.FullName, ErrorStatus.FileLocked));
            }

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

            operationsController.KnownKeyAdded = new AsyncDelegateAction<FileOperationEventArgs>(async (FileOperationEventArgs e) =>
            {
                await _knownIdentities.AddAsync(e.LogOnIdentity);
            });

            operationsController.Completed += async (object sender, FileOperationEventArgs e) =>
            {
                if (e.Status.ErrorStatus == ErrorStatus.Success)
                {
                    await New<ActiveFileAction>().RemoveRecentFiles(new IDataStore[] { New<IDataStore>(e.OpenFileFullName) }, progress);
                }
            };

            return operationsController.DecryptFileAsync(file);
        }

        private Task<FileOperationContext> VerifyFileIntegrityWork(IDataStore file, IProgressContext progress)
        {
            return EncryptedFilePreconditions(file) ?? VerifyFileIntegrityAsync(file, IdentityViewModel.LogOnIdentity, progress);
        }

        private Task<FileOperationContext> VerifyFileIntegrityAsync(IDataStore dataStore, LogOnIdentity identity, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            operationsController.QueryDecryptionPassphrase = HandleQueryDecryptionPassphraseEventAsync;

            return operationsController.VerifyFileIntegrityAsync(dataStore);
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

            operationsController.Completed += async (object sender, FileOperationEventArgs e) =>
            {
                if (e.Skip)
                {
                    return;
                }
                if (e.Status.ErrorStatus == ErrorStatus.Success)
                {
                    await New<ActiveFileAction>().RemoveRecentFiles(new IDataStore[] { New<IDataStore>(e.SaveFileFullName) }, progress);
                }
            };

            return operationsController.WipeFileAsync(file);
        }

        private Task<FileOperationContext> EncryptionUpgradeWorkAsync(IDataStore store, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            operationsController.QueryDecryptionPassphrase = HandleQueryDecryptionPassphraseEventAsync;

            operationsController.KnownKeyAdded = new AsyncDelegateAction<FileOperationEventArgs>(async (FileOperationEventArgs e) =>
            {
                await _knownIdentities.AddAsync(e.LogOnIdentity);
            });

            return operationsController.EncryptionUpgradeAsync(store);
        }

        private static bool CheckStatusAndShowMessage(FileOperationContext context, string fallbackName)
        {
            return Resolve.StatusChecker.CheckStatusAndShowMessage(context.ErrorStatus, string.IsNullOrEmpty(context.FullName) ? fallbackName : context.FullName, context.InternalMessage);
        }

        private Task<FileOperationContext> RandomRenameFileWorkAsync(IDataStore file, IProgressContext progress)
        {
            return EncryptedFilePreconditions(file) ?? RandomRenameInternal(file);
        }

        private static Task<FileOperationContext> RandomRenameInternal(IDataStore file)
        {
            file.RandomRename();

            return Task.FromResult(new FileOperationContext(file.FullName, ErrorStatus.Success));
        }

        private Task<FileOperationContext> RestoreRandomRenameFilesWorkAsync(IDataStore file, IProgressContext progress)
        {
            return EncryptedFilePreconditions(file) ?? RestoreRandomRenameInternal(file, progress);
        }

        private Task<FileOperationContext> RestoreRandomRenameInternal(IDataStore file, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);
            operationsController.QueryDecryptionPassphrase = HandleQueryOpenPassphraseEventAsync;

            operationsController.Completed += (object sender, FileOperationEventArgs e) =>
            {
                if (e.Status.ErrorStatus == ErrorStatus.Success)
                {
                    file.RestoreRandomRename(e.LogOnIdentity);
                }
            };

            return operationsController.VerifyEncryptedAsync(file);
        }

        private static Task<FileOperationContext> EncryptedFilePreconditions(IDataStore file)
        {
            if (!file.IsAvailable)
            {
                return Task.FromResult(new FileOperationContext(file.FullName, ErrorStatus.FileDoesNotExist));
            }
            if (!file.IsEncrypted())
            {
                return Task.FromResult(new FileOperationContext(file.FullName, ErrorStatus.Success));
            }
            if (file.IsInUse())
            {
                return Task.FromResult(new FileOperationContext(file.FullName, ErrorStatus.FileLocked));
            }

            return null;
        }

        private Task<FileOperationContext> OpenEncryptedWorkAsync(IDataStore file, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            operationsController.QueryDecryptionPassphrase = HandleQueryOpenPassphraseEventAsync;

            operationsController.KnownKeyAdded = new AsyncDelegateAction<FileOperationEventArgs>(async (FileOperationEventArgs e) =>
            {
                if (!_fileSystemState.KnownPassphrases.Any(i => i.Thumbprint == e.LogOnIdentity.Passphrase.Thumbprint))
                {
                    _fileSystemState.KnownPassphrases.Add(e.LogOnIdentity.Passphrase);
                }
                await _knownIdentities.AddAsync(e.LogOnIdentity);
            });

            operationsController.SetConvertLegacyOptionCommandAsync = async () =>
            {
                if (Resolve.UserSettings.EncryptionUpgradeMode != EncryptionUpgradeMode.NotDecided)
                {
                    return;
                }
                if (!New<LicensePolicy>().Capabilities.Has(LicenseCapability.EditExistingFiles))
                {
                    return;
                }

                bool autoConvert = await New<IPopup>().ShowAsync(PopupButtons.OkCancel, Texts.OptionsConvertMenuItemText, Texts.LegacyOpenMessage) == PopupButtons.Ok;
                autoConvert = autoConvert && New<IVerifySignInPassword>().Verify(Texts.LegacyConversionVerificationPrompt);
                New<UserSettings>().EncryptionUpgradeMode = autoConvert ? EncryptionUpgradeMode.AutoUpgrade : EncryptionUpgradeMode.RetainWithoutUpgrade;
            };
            return operationsController.DecryptAndLaunchAsync(file);
        }

        private async Task HandleQueryOpenPassphraseEventAsync(FileOperationEventArgs e)
        {
            await QueryDecryptPassphraseAsync(e);

            if (e.Cancel)
            {
                return;
            }
        }

        private Task HandleQueryDecryptionPassphraseEventAsync(FileOperationEventArgs e)
        {
            return QueryDecryptPassphraseAsync(e);
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
            FileOperationsController operationsController = new FileOperationsController(progress);

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
                if (e.Status.ErrorStatus == ErrorStatus.FileAlreadyEncrypted)
                {
                    e.Status = new FileOperationContext(string.Empty, ErrorStatus.Success);
                }
            };
            return controller.EncryptFileAsync(file, Recipients);
        }

        private Task<FileOperationContext> EncryptFileWorkManyAsync(IDataStore file, IProgressContext progress)
        {
            FileOperationsController controller = EncryptFileWorkController(progress);
            return controller.EncryptFileAsync(file, Recipients);
        }

        private Task<FileOperationContext> VerifyAndAddActiveWorkAsync(IDataStore fullName, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            operationsController.QueryDecryptionPassphrase += HandleQueryDecryptionPassphraseEventAsync;

            operationsController.KnownKeyAdded = new AsyncDelegateAction<FileOperationEventArgs>(async (FileOperationEventArgs e) =>
            {
                if (!_fileSystemState.KnownPassphrases.Any(i => i.Thumbprint == e.LogOnIdentity.Passphrase.Thumbprint))
                {
                    _fileSystemState.KnownPassphrases.Add(e.LogOnIdentity.Passphrase);
                }
                await _knownIdentities.AddAsync(e.LogOnIdentity);
            });

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

            return operationsController.VerifyEncryptedAsync(fullName);
        }

        private Task OpenFilesFromFolderActionAsync(string folder)
        {
            FileSelectionEventArgs fileSelectionArgs = new FileSelectionEventArgs(new string[] { folder })
            {
                FileSelectionType = FileSelectionType.Open,
            };
            OnSelectingFiles(fileSelectionArgs);
            if (fileSelectionArgs.Cancel)
            {
                return Constant.CompletedTask;
            }
            return OpenFilesActionAsync(fileSelectionArgs.SelectedFiles);
        }

        private async Task<FileOperationContext> DecryptFolderWorkAsync(IDataContainer folder, IProgressContext progress)
        {
            await New<AxCryptFile>().DecryptFilesInsideFolderUniqueWithWipeOfOriginalAsync(folder, _knownIdentities.DefaultEncryptionIdentity, _statusChecker, progress);
            return new FileOperationContext(String.Empty, ErrorStatus.Success);
        }

        private async Task AddRecentFilesActionAsync(IEnumerable<string> files)
        {
            IEnumerable<IDataStore> fileInfos = files.Select(f => New<IDataStore>(f)).ToList();
            if (Resolve.KnownIdentities.IsLoggedOn)
            {
                await EncryptFewOrManyFilesAsync(fileInfos);
            }
            await ProcessEncryptedFilesDroppedInRecentListAsync(fileInfos.Where(fileInfo => fileInfo.Type() == FileInfoTypes.EncryptedFile));
        }

        private async Task ProcessEncryptedFilesDroppedInRecentListAsync(IEnumerable<IDataStore> encryptedFiles)
        {
            await _fileOperation.DoFilesAsync(encryptedFiles, VerifyAndAddActiveWorkAsync, (status) => Task.FromResult(CheckStatusAndShowMessage(status, string.Empty)));
            await _fileSystemState.Save();
        }

        private async Task EncryptFewOrManyFilesAsync(IEnumerable<IDataStore> encryptableFiles)
        {
            if (!encryptableFiles.Any())
            {
                return;
            }
            if (encryptableFiles.Count() > New<UserSettings>().FewFilesThreshold)
            {
                await _fileOperation.DoFilesAsync(encryptableFiles, EncryptFileWorkManyAsync, (status) => CheckEncryptionStatus(status));
            }
            else
            {
                await _fileOperation.DoFilesAsync(encryptableFiles, EncryptFileWorkOneAsync, (status) => CheckEncryptionStatus(status));
            }
            await New<FileSystemState>().Save();
        }

        private static Task<bool> CheckEncryptionStatus(FileOperationContext foc)
        {
            if (foc.ErrorStatus == ErrorStatus.FileAlreadyEncrypted)
            {
                foc = new FileOperationContext(foc.FullName, ErrorStatus.Success);
            }

            return Task.FromResult(CheckStatusAndShowMessage(foc, string.Empty));
        }

        private Task HandleSessionChanged(SessionNotification notification)
        {
            switch (notification.NotificationType)
            {
                case SessionNotificationType.SignOut:
                case SessionNotificationType.SignIn:
                case SessionNotificationType.SessionStart:
                    ((AsyncDelegateAction<string>)OpenFilesFromFolder).RaiseCanExecuteChanged();
                    break;
            }
            return Constant.CompletedTask;
        }

        private Task ShowInFolderActionAsync(IEnumerable<string> files)
        {
            return _fileOperation.DoFilesAsync(files.Select(f => New<IDataStore>(f)).ToList(), OpenFileLocationAsync, (status) => Task.FromResult(CheckStatusAndShowMessage(status, string.Empty)));
        }

        private Task<FileOperationContext> OpenFileLocationAsync(IDataStore file, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            return operationsController.OpenFileLocationAsync(file);
        }

        private async Task TryBrokenFilesActionAsync(IEnumerable<string> files)
        {
            files = files ?? SelectFiles(FileSelectionType.Decrypt);
            if (!files.Any())
            {
                return;
            }
            if (!_knownIdentities.IsLoggedOn)
            {
                await IdentityViewModel.AskForDecryptPassphrase.ExecuteAsync(files.First());
            }
            if (!_knownIdentities.IsLoggedOn)
            {
                return;
            }
            await _fileOperation.DoFilesAsync(files.Select(f => New<IDataStore>(f)).ToList(), TryBrokenFileWork, (status) => Task.FromResult(CheckStatusAndShowMessage(status, string.Empty)));
        }

        private Task<FileOperationContext> TryBrokenFileWork(IDataStore file, IProgressContext progress)
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

            operationsController.KnownKeyAdded = new AsyncDelegateAction<FileOperationEventArgs>(async (FileOperationEventArgs e) =>
            {
                await _knownIdentities.AddAsync(e.LogOnIdentity);
            });

            operationsController.Completed += async (object sender, FileOperationEventArgs e) =>
            {
                if (e.Status.ErrorStatus == ErrorStatus.Success)
                {
                    await New<ActiveFileAction>().RemoveRecentFiles(new IDataStore[] { New<IDataStore>(e.OpenFileFullName) }, progress);
                }
            };

            return operationsController.TryDecryptBrokenFileAsync(file);
        }

        private async Task VerifyFilesActionAsync(IEnumerable<string> files)
        {
            files = files ?? SelectFiles(FileSelectionType.Decrypt);
            if (!files.Any())
            {
                return;
            }
            if (!_knownIdentities.IsLoggedOn)
            {
                await IdentityViewModel.AskForDecryptPassphrase.ExecuteAsync(files.First());
            }
            if (!_knownIdentities.IsLoggedOn)
            {
                return;
            }

            await _fileOperation.DoFilesAsync(files.Select(f => New<IDataStore>(f)).ToList(), VerifyFileIntegrityWork, (status) => Task.FromResult(CheckStatusAndShowMessage(status, string.Empty)));
        }

        private async Task IntegrityCheckFilesActionAsync(IEnumerable<string> files)
        {
            files = files ?? SelectFiles(FileSelectionType.Decrypt);
            if (!files.Any())
            {
                return;
            }

            await _fileOperation.DoFilesAsync(files.Select(f => New<IDataStore>(f)).ToList(), IntegrityCheckWork, (status) => Task.FromResult(CheckStatusAndShowMessage(status, string.Empty)));
        }

        private Task<FileOperationContext> IntegrityCheckWork(IDataStore file, IProgressContext progress)
        {
            return IntegrityCheckAsync(file, progress);
        }

        private Task<FileOperationContext> IntegrityCheckAsync(IDataStore dataStore, IProgressContext progress)
        {
            FileOperationsController operationsController = new FileOperationsController(progress);

            return operationsController.IntegrityCheckAsync(dataStore);
        }
    }
}