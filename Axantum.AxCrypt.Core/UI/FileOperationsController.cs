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
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI
{
    /// <summary>
    /// This class implements the controlling logic for various file-oriented operations which typically
    /// require user interaction. Opportunity to insert user interaction is provided via events which are
    /// raised on a need-to-know basis. Instances of this class should typically be instantiated on a GUI
    /// thread and methods should be called from the GUI thread. Support is provided for doing the heavy
    /// lifting on background threads.
    /// </summary>
    public class FileOperationsController
    {
        private FileOperationEventArgs _eventArgs;

        private IProgressContext _progress;

        #region Constructors

        /// <summary>
        /// Create a new instance, without any progress reporting.
        /// </summary>
        public FileOperationsController()
            : this(new ProgressContext())
        {
        }

        /// <summary>
        /// Create a new instance, reporting progress
        /// </summary>
        /// <param name="progress">The instance of ProgressContext to report progress via</param>
        public FileOperationsController(IProgressContext progress)
        {
            _eventArgs = new FileOperationEventArgs();
            _progress = progress;
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised whenever there is a need to specify a file to save to because the expected target
        /// name already exists.
        /// </summary>
        public event EventHandler<FileOperationEventArgs> QuerySaveFileAs;

        protected virtual void OnQuerySaveFileAs(FileOperationEventArgs e)
        {
            EventHandler<FileOperationEventArgs> handler = QuerySaveFileAs;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raised when a valid decryption passphrase was not found.
        /// </summary>
        public Func<FileOperationEventArgs, Task> QueryDecryptionPassphrase { get; set; }

        protected virtual Task OnQueryDecryptionPassphrase(FileOperationEventArgs e)
        {
            return QueryDecryptionPassphrase?.Invoke(e);
        }

        /// <summary>
        /// Occurs when no default encryption identity is known.
        /// </summary>
        public event EventHandler<FileOperationEventArgs> QueryEncryptionPassphrase;

        protected virtual void OnQueryEncryptionPassphrase(FileOperationEventArgs e)
        {
            EventHandler<FileOperationEventArgs> handler = QueryEncryptionPassphrase;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Occurs when sharing keys may be added for an encryption.
        /// </summary>
        public event EventHandler<FileOperationEventArgs> QuerySharedPublicKeys;

        protected virtual void OnQuerySharedPublicKeys(FileOperationEventArgs e)
        {
            EventHandler<FileOperationEventArgs> handler = QuerySharedPublicKeys;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raised to confirm that a file really should be wiped.
        /// </summary>
        public event EventHandler<FileOperationEventArgs> WipeQueryConfirmation;

        protected virtual void OnWipeQueryConfirmation(FileOperationEventArgs e)
        {
            EventHandler<FileOperationEventArgs> handler = WipeQueryConfirmation;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raised when a new KnowKey is added.
        /// </summary>
        public AsyncDelegateAction<FileOperationEventArgs> KnownKeyAdded { get; set; }

        protected virtual async Task OnKnownKeyAdded(FileOperationEventArgs e)
        {
            await (KnownKeyAdded?.ExecuteAsync(e) ?? Constant.CompletedTask);
        }

        /// <summary>
        /// Always raised at the end of an operation, regardless of errors or cancellation.
        /// </summary>
        /// <param name="e"></param>
        public event EventHandler<FileOperationEventArgs> Completed;

        protected virtual void OnCompleted(FileOperationEventArgs e)
        {
            EventHandler<FileOperationEventArgs> handler = Completed;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public Func<Task> SetConvertLegacyOptionCommandAsync { get; set; } = () => Constant.CompletedTask;

        #endregion Events

        #region Public Methods

        /// <summary>
        /// Encrypt file, raising events as required by the situation.
        /// </summary>
        /// <param name="sourceFile">The full path to a plain-text file to encrypt.</param>
        /// <returns>'True' if the operation did not fail so far, 'False' if it definitely has failed.</returns>
        /// <remarks>
        /// Since especially the actual operation typically is executed asynchronously, the
        /// return value and status do not conclusive indicate success. Only a failure return
        /// is conclusive.
        /// </remarks>
        public Task<FileOperationContext> EncryptFileAsync(IDataStore fileInfo)
        {
            return DoFileAsync(fileInfo, EncryptFilePreparationAsync, EncryptFileOperationAsync);
        }

        /// <summary>
        /// Decrypt a file, raising events as required by the situation.
        /// </summary>
        /// <param name="sourceFile">The full path to an encrypted file.</param>
        /// <returns>The resulting status of the operation.</returns>
        public Task<FileOperationContext> DecryptFileAsync(IDataStore fileInfo)
        {
            return DoFileAsync(fileInfo, DecryptFilePreparationAsync, DecryptFileOperationAsync);
        }

        /// <summary>
        /// Decrypt a file, and launch the associated application raising events as required by
        /// the situation.
        /// </summary>
        /// <param name="fileInfo">The full path to an encrypted file.</param>
        /// <returns>A FileOperationStatus indicating the result of the operation.</returns>
        public Task<FileOperationContext> DecryptAndLaunchAsync(IDataStore fileInfo)
        {
            return DoFileAsync(fileInfo, DecryptAndLaunchPreparationAsync, DecryptAndLaunchFileOperationAsync);
        }

        /// <summary>
        /// Launch the containing folder.
        /// </summary>
        /// <param name="fileInfo">The full path to an encrypted file.</param>
        /// <returns>A FileOperationStatus indicating the result of the operation.</returns>
        public Task<FileOperationContext> OpenFileLocationAsync(IDataStore fileInfo)
        {
            return DoFileAsync(fileInfo, OpenFileLocationPreparationAsync, OpenFileLocationOperationAsync);
        }

        /// <summary>
        /// Try Decrypt a broken file, raising events as required by the situation.
        /// </summary>
        /// <param name="fileInfo">The full path to an encrypted file.</param>
        /// <returns>The resulting status of the operation.</returns>
        public Task<FileOperationContext> TryDecryptBrokenFileAsync(IDataStore fileInfo)
        {
            return DoFileAsync(fileInfo, TryDecryptBrokenFilePreparationAsync, TryDecryptBrokenFileOperationAsync);
        }

        /// <summary>
        /// Verify that a file is encrypted with a known key.
        /// </summary>
        /// <param name="fileInfo">The file to verify.</param>
        /// <returns>FileOperationStatus.Success if  the file is encrypted with a known key.</returns>
        public Task<FileOperationContext> VerifyEncryptedAsync(IDataStore fileInfo)
        {
            return DoFileAsync(fileInfo, VerifyCanDecryptPreparationAsync, GetAxCryptFileNameAsSaveFileNameAsync);
        }

        /// <summary>
        /// Wipes a file securely synchronously.
        /// </summary>
        /// <param name="fileInfo">The full name of the file to wipe</param>
        /// <returns>A FileOperationStatus indicating the result of the operation.</returns>
        public Task<FileOperationContext> WipeFileAsync(IDataStore fileInfo)
        {
            return DoFileAsync(fileInfo, WipeFilePreparationAsync, WipeFileOperationAsync);
        }

        public Task<FileOperationContext> UpgradeFileAsync(IDataStore store)
        {
            return DoFileAsync(store, UpgradeFilePreparationAsync, UpgradeFileOperationAsync);
        }

        #endregion Public Methods

        #region Private Methods

        private Task<bool> EncryptFilePreparationAsync(IDataStore sourceFileInfo)
        {
            _eventArgs.OpenFileFullName = sourceFileInfo.FullName;

            if (String.Compare(Resolve.Portable.Path().GetExtension(sourceFileInfo.FullName), OS.Current.AxCryptExtension, StringComparison.OrdinalIgnoreCase) == 0)
            {
                _eventArgs.Status = new FileOperationContext(sourceFileInfo.FullName, ErrorStatus.FileAlreadyEncrypted);
                return Task.FromResult(false);
            }

            using (FileLock fileLock = New<FileLocker>().Acquire(sourceFileInfo))
            {
                if (IsLocked(fileLock))
                {
                    return Task.FromResult(false);
                }

                IDataStore destinationFileInfo = New<IDataStore>(AxCryptFile.MakeAxCryptFileName(sourceFileInfo));
                _eventArgs.SaveFileFullName = destinationFileInfo.FullName;
                _eventArgs.OpenFileFullName = sourceFileInfo.FullName;
                if (destinationFileInfo.IsAvailable)
                {
                    OnQuerySaveFileAs(_eventArgs);
                    if (_eventArgs.Cancel)
                    {
                        _eventArgs.Status = new FileOperationContext(sourceFileInfo.FullName, ErrorStatus.Canceled);
                        return Task.FromResult(false);
                    }
                }

                if (Resolve.KnownIdentities.DefaultEncryptionIdentity == LogOnIdentity.Empty)
                {
                    OnQueryEncryptionPassphrase(_eventArgs);
                    if (_eventArgs.Cancel)
                    {
                        _eventArgs.Status = new FileOperationContext(sourceFileInfo.FullName, ErrorStatus.Canceled);
                        return Task.FromResult(false);
                    }
                }
                else
                {
                    _eventArgs.LogOnIdentity = Resolve.KnownIdentities.DefaultEncryptionIdentity;
                }

                OnQuerySharedPublicKeys(_eventArgs);
            }

            return Task.FromResult(true);
        }

        private bool IsLocked(FileLock fileLock)
        {
            IDataStore dataStore = fileLock.DataStore;
            if (dataStore.IsLocked())
            {
                _eventArgs.Status = new FileOperationContext(dataStore.FullName, ErrorStatus.FileLocked);
                return true;
            }

            return false;
        }

        private bool IsWriteProtected(FileLock fileLock)
        {
            IDataStore dataStore = fileLock.DataStore;
            if (dataStore.IsWriteProtected)
            {
                _eventArgs.Status = new FileOperationContext(dataStore.FullName, ErrorStatus.FileWriteProtected);
                return true;
            }
            return false;
        }

        private async Task<bool> EncryptFileOperationAsync()
        {
            _eventArgs.CryptoId = Resolve.CryptoFactory.Default(New<ICryptoPolicy>()).CryptoId;
            EncryptionParameters encryptionParameters = new EncryptionParameters(_eventArgs.CryptoId, _eventArgs.LogOnIdentity);
            await encryptionParameters.AddAsync(await (await GetWatchedFolderKeyShares(_eventArgs.OpenFileFullName)).ToKnownPublicKeysAsync(_eventArgs.LogOnIdentity));

            await New<AxCryptFile>().EncryptFileWithBackupAndWipeAsync(_eventArgs.OpenFileFullName, _eventArgs.SaveFileFullName, encryptionParameters, _progress);

            _eventArgs.Status = new FileOperationContext(String.Empty, ErrorStatus.Success);
            return true;
        }

        private static async Task<IEnumerable<EmailAddress>> GetWatchedFolderKeyShares(string sourceFileName)
        {
            if (!New<LicensePolicy>().Capabilities.Has(LicenseCapability.SecureFolders))
            {
                return new EmailAddress[0];
            }

            IDataStore sourceFileInfo = New<IDataStore>(sourceFileName);
            WatchedFolder sourceFileWatchedFolder = Resolve.FileSystemState.WatchedFolders.FindOrDefault(sourceFileInfo);

            return sourceFileWatchedFolder?.KeyShares ?? new EmailAddress[0];
        }

        private async Task<bool> DecryptFilePreparationAsync(IDataStore fileInfo)
        {
            if (!await CheckDecryptionIdentityAndLockingAsync(fileInfo))
            {
                return false;
            }

            IDataStore destination = New<IDataStore>(_eventArgs.SaveFileFullName);
            if (destination.IsAvailable)
            {
                OnQuerySaveFileAs(_eventArgs);
                if (_eventArgs.Cancel)
                {
                    _eventArgs.Status = new FileOperationContext(fileInfo.FullName, ErrorStatus.Canceled);
                    return false;
                }
            }

            return true;
        }

        private Task<bool> UpgradeFilePreparationAsync(IDataStore store)
        {
            return CheckDecryptionIdentityAndLockingAsync(store);
        }

        private async Task<bool> CheckDecryptionIdentityAndLockingAsync(IDataStore fileInfo)
        {
            _eventArgs.OpenFileFullName = fileInfo.FullName;
            if (!fileInfo.IsEncrypted())
            {
                _eventArgs.Status = new FileOperationContext(fileInfo.FullName, "Wrong extension", ErrorStatus.WrongFileExtensionError);
                return false;
            }

            using (FileLock fileLock = New<FileLocker>().Acquire(fileInfo))
            {
                if (IsLocked(fileLock))
                {
                    return false;
                }

                if (!await OpenAxCryptDocumentAsync(fileInfo, _eventArgs) || _eventArgs.Skip)
                {
                    return false;
                }
            }
            return true;
        }

        private Task<bool> DecryptFileOperationAsync()
        {
            _progress.NotifyLevelStart();
            try
            {
                using (FileLock encryptedFileLock = New<FileLocker>().Acquire(_eventArgs.AxCryptFile))
                {
                    using (IAxCryptDocument document = New<AxCryptFile>().Document(_eventArgs.AxCryptFile, _eventArgs.LogOnIdentity, _progress))
                    {
                        New<AxCryptFile>().DecryptFile(document, _eventArgs.SaveFileFullName, _progress);
                        if (_eventArgs.AxCryptFile.IsWriteProtected)
                        {
                            New<IDataStore>(_eventArgs.SaveFileFullName).IsWriteProtected = true;
                        }
                    }
                    if (_eventArgs.AxCryptFile.IsWriteProtected)
                    {
                        _eventArgs.AxCryptFile.IsWriteProtected = false;
                    }
                    New<AxCryptFile>().Wipe(encryptedFileLock, _progress);
                }
            }
            catch (AxCryptException ace)
            {
                New<IReport>().Exception(ace);
                _eventArgs.Status = new FileOperationContext(_eventArgs.OpenFileFullName, ace.ErrorStatus);
                return Task.FromResult(false);
            }
            finally
            {
                _progress.NotifyLevelFinished();
            }
            _eventArgs.Status = new FileOperationContext(String.Empty, ErrorStatus.Success);
            return Task.FromResult(true);
        }

        private async Task<bool> DecryptAndLaunchPreparationAsync(IDataStore fileInfo)
        {
            if (!await VerifyCanDecryptPreparationAsync(fileInfo))
            {
                return false;
            }

            if (_eventArgs.CryptoId != new V1Aes128CryptoFactory().CryptoId)
            {
                return true;
            }

            await SetConvertLegacyOptionCommandAsync();
            return true;
        }

        private async Task<bool> VerifyCanDecryptPreparationAsync(IDataStore fileInfo)
        {
            _eventArgs.OpenFileFullName = fileInfo.FullName;

            if (!fileInfo.IsEncrypted())
            {
                _eventArgs.Status = new FileOperationContext(fileInfo.FullName, "Wrong extension", ErrorStatus.WrongFileExtensionError);
                return false;
            }

            if (!await OpenAxCryptDocumentAsync(fileInfo, _eventArgs))
            {
                return false;
            }

            return true;
        }

        private Task<bool> GetAxCryptFileNameAsSaveFileNameAsync()
        {
            if (!_eventArgs.Skip)
            {
                _eventArgs.SaveFileFullName = _eventArgs.AxCryptFile.FullName;
            }

            _eventArgs.Status = new FileOperationContext(String.Empty, ErrorStatus.Success);
            return Constant.TrueTask;
        }

        private async Task<bool> DecryptAndLaunchFileOperationAsync()
        {
            _eventArgs.Status = await New<FileOperation>().OpenAndLaunchApplication(_eventArgs.LogOnIdentity, _eventArgs.AxCryptFile, _progress);
            return true;
        }

        private Task<bool> WipeFilePreparationAsync(IDataStore fileInfo)
        {
            using (FileLock fileLock = New<FileLocker>().Acquire(fileInfo))
            {
                if (IsWriteProtected(fileLock) || IsLocked(fileLock))
                {
                    return Task.FromResult(false);
                }

                _eventArgs.OpenFileFullName = fileInfo.FullName;
                _eventArgs.SaveFileFullName = fileInfo.FullName;
                if (_progress.AllItemsConfirmed)
                {
                    return Task.FromResult(true);
                }
                OnWipeQueryConfirmation(_eventArgs);
                if (_eventArgs.Cancel)
                {
                    _eventArgs.Status = new FileOperationContext(fileInfo.FullName, ErrorStatus.Canceled);
                    return Task.FromResult(false);
                }

                if (_eventArgs.ConfirmAll)
                {
                    _progress.AllItemsConfirmed = true;
                }
            }
            return Task.FromResult(true);
        }

        private Task<bool> WipeFileOperationAsync()
        {
            if (_eventArgs.Skip)
            {
                _eventArgs.Status = new FileOperationContext(String.Empty, ErrorStatus.Success);
                return Constant.TrueTask;
            }

            ActiveFile activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_eventArgs.SaveFileFullName);
            if (activeFile != null && activeFile.Status.HasFlag(ActiveFileStatus.AssumedOpenAndDecrypted))
            {
                _eventArgs.Status = new FileOperationContext(_eventArgs.OpenFileFullName, ErrorStatus.FileLocked);
                return Constant.FalseTask;
            }

            _progress.NotifyLevelStart();
            try
            {
                using (FileLock wipeFileLock = New<FileLocker>().Acquire(New<IDataStore>(_eventArgs.SaveFileFullName)))
                {
                    New<AxCryptFile>().Wipe(wipeFileLock, _progress);
                }
            }
            finally
            {
                _progress.NotifyLevelFinished();
            }

            _eventArgs.Status = new FileOperationContext(String.Empty, ErrorStatus.Success);
            return Constant.TrueTask;
        }

        private async Task<bool> UpgradeFileOperationAsync()
        {
            if (_eventArgs.Skip)
            {
                _eventArgs.Status = new FileOperationContext(String.Empty, ErrorStatus.Success);
                return true;
            }

            _progress.NotifyLevelStart();
            try
            {
                EncryptionParameters encryptionParameters = new EncryptionParameters(New<CryptoFactory>().Default(New<ICryptoPolicy>()).CryptoId, New<KnownIdentities>().DefaultEncryptionIdentity);
                await New<AxCryptFile>().ChangeEncryptionAsync(_eventArgs.AxCryptFile, _eventArgs.LogOnIdentity, encryptionParameters, _progress);
            }
            finally
            {
                _progress.NotifyLevelFinished();
            }

            _eventArgs.Status = new FileOperationContext(String.Empty, ErrorStatus.Success);
            return true;
        }

        private async Task<bool> OpenAxCryptDocumentAsync(IDataStore sourceFileInfo, FileOperationEventArgs e)
        {
            _progress.NotifyLevelStart();
            try
            {
                e.OpenFileFullName = sourceFileInfo.FullName;
                if (TryFindDecryptionKey(sourceFileInfo, e))
                {
                    return InfoFromDecryptedDocument(sourceFileInfo, e);
                }

                while (true)
                {
                    await OnQueryDecryptionPassphrase(e);
                    if (e.Cancel)
                    {
                        e.Status = new FileOperationContext(sourceFileInfo.FullName, ErrorStatus.Canceled);
                        return false;
                    }
                    if (e.Skip)
                    {
                        e.Status = new FileOperationContext(String.Empty, ErrorStatus.Success);
                        return true;
                    }
                    if (InfoFromDecryptedDocument(sourceFileInfo, e))
                    {
                        await OnKnownKeyAdded(e);
                        return true;
                    }
                }
            }
            catch (IOException ioex)
            {
                New<IReport>().Exception(ioex);
                FileOperationContext status = new FileOperationContext(sourceFileInfo.FullName, ioex.Message, ioex.IsFileOrDirectoryNotFound() ? ErrorStatus.FileDoesNotExist : ErrorStatus.Exception);
                e.Status = status;
                return false;
            }
            finally
            {
                _progress.NotifyLevelFinished();
            }
        }

        private static bool InfoFromDecryptedDocument(IDataStore sourceFileInfo, FileOperationEventArgs e)
        {
            EncryptedProperties properties = New<AxCryptFile>().CreateEncryptedProperties(sourceFileInfo, e.LogOnIdentity);
            if (!properties.IsValid)
            {
                return false;
            }

            e.CryptoId = properties.DecryptionParameter.CryptoId;
            IDataStore destination = New<IDataStore>(Resolve.Portable.Path().Combine(Resolve.Portable.Path().GetDirectoryName(sourceFileInfo.FullName), properties.FileMetaData.FileName));
            e.SaveFileFullName = destination.FullName;
            e.AxCryptFile = sourceFileInfo;
            return true;
        }

        private static bool TryFindDecryptionKey(IDataStore fileInfo, FileOperationEventArgs e)
        {
            Guid cryptoId;
            LogOnIdentity logOnIdentity = fileInfo.TryFindPassphrase(out cryptoId);
            if (logOnIdentity == null)
            {
                return false;
            }

            e.CryptoId = cryptoId;
            e.LogOnIdentity = logOnIdentity;
            return true;
        }

        private async Task<FileOperationContext> DoFileAsync(IDataStore fileInfo, Func<IDataStore, Task<bool>> preparation, Func<Task<bool>> operation)
        {
            _progress.NotifyLevelStart();
            try
            {
                bool ok = await RunSequentiallyAsync(fileInfo, preparation);
                if (ok)
                {
                    await operation();
                }
            }
            catch (OperationCanceledException ocex)
            {
                _eventArgs.Status = new FileOperationContext(fileInfo.FullName, ocex.Messages(), ErrorStatus.Canceled);
            }
            catch (AxCryptException aex)
            {
                New<IReport>().Exception(aex);
                _eventArgs.Status = new FileOperationContext(string.IsNullOrEmpty(aex.DisplayContext) ? fileInfo.FullName : aex.DisplayContext, aex.Messages(), aex.ErrorStatus);
            }
            catch (Exception ex)
            {
                New<IReport>().Exception(ex);
                _eventArgs.Status = new FileOperationContext(fileInfo.FullName, ex.Messages(), ErrorStatus.Exception);
            }
            finally
            {
                _progress.NotifyLevelFinished();
                OnCompleted(_eventArgs);
            }

            return _eventArgs.Status;
        }

        private async Task<bool> RunSequentiallyAsync(IDataStore fileInfo, Func<IDataStore, Task<bool>> preparation)
        {
            bool ok = false;
            await _progress.EnterSingleThread();
            try
            {
                if (_progress.Cancel)
                {
                    _eventArgs.Status = new FileOperationContext(fileInfo.FullName, ErrorStatus.Canceled);
                    return ok;
                }
                ok = await preparation(fileInfo);
                if (_eventArgs.Status.ErrorStatus == ErrorStatus.Canceled)
                {
                    _progress.Cancel = true;
                }
            }
            finally
            {
                _progress.LeaveSingleThread();
            }
            return ok;
        }

        private Task<bool> OpenFileLocationPreparationAsync(IDataStore fileInfo)
        {
            _eventArgs.OpenFileFullName = fileInfo.FullName;

            return Constant.TrueTask;
        }

        private Task<bool> OpenFileLocationOperationAsync()
        {
            _eventArgs.Status = New<FileOperation>().OpenFileLocation(_eventArgs.OpenFileFullName);

            return Constant.TrueTask;
        }

        private async Task<bool> TryDecryptBrokenFilePreparationAsync(IDataStore fileInfo)
        {
            if (!await CheckDecryptionIdentityAndLockingAsync(fileInfo))
            {
                return false;
            }

            IDataStore destination = New<IDataStore>(_eventArgs.SaveFileFullName);
            if (destination.IsAvailable)
            {
                OnQuerySaveFileAs(_eventArgs);
                if (_eventArgs.Cancel)
                {
                    _eventArgs.Status = new FileOperationContext(fileInfo.FullName, ErrorStatus.Canceled);
                    return false;
                }
            }

            return true;
        }

        private Task<bool> TryDecryptBrokenFileOperationAsync()
        {
            _progress.NotifyLevelStart();
            try
            {
                using (IAxCryptDocument document = New<AxCryptFile>().Document(_eventArgs.AxCryptFile, _eventArgs.LogOnIdentity, _progress))
                {
                    New<AxCryptFile>().TryDecryptBrokenFile(document, _eventArgs.SaveFileFullName, _progress);
                    if (_eventArgs.AxCryptFile.IsWriteProtected)
                    {
                        New<IDataStore>(_eventArgs.SaveFileFullName).IsWriteProtected = true;
                    }
                }
                if (_eventArgs.AxCryptFile.IsWriteProtected)
                {
                    _eventArgs.AxCryptFile.IsWriteProtected = false;
                }
            }
            catch (AxCryptException ace)
            {
                New<IReport>().Exception(ace);
                _eventArgs.Status = new FileOperationContext(_eventArgs.OpenFileFullName, ace.ErrorStatus);
                return Constant.FalseTask;
            }
            finally
            {
                _progress.NotifyLevelFinished();
            }
            _eventArgs.Status = new FileOperationContext(String.Empty, ErrorStatus.Success);
            return Constant.TrueTask;
        }

        #endregion Private Methods
    }
}