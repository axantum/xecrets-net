#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using System;
using System.IO;
using System.Linq;

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
        public event EventHandler<FileOperationEventArgs> QueryDecryptionPassphrase;

        protected virtual void OnQueryDecryptionPassphrase(FileOperationEventArgs e)
        {
            EventHandler<FileOperationEventArgs> handler = QueryDecryptionPassphrase;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raised when the no default encryption identity is known.
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
        public event EventHandler<FileOperationEventArgs> KnownKeyAdded;

        protected virtual void OnKnownKeyAdded(FileOperationEventArgs e)
        {
            EventHandler<FileOperationEventArgs> handler = KnownKeyAdded;
            if (handler != null)
            {
                handler(this, e);
            }
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
        public FileOperationContext EncryptFile(IDataStore fileInfo)
        {
            return DoFile(fileInfo, EncryptFilePreparation, EncryptFileOperation);
        }

        /// <summary>
        /// Decrypt a file, raising events as required by the situation.
        /// </summary>
        /// <param name="sourceFile">The full path to an encrypted file.</param>
        /// <returns>The resulting status of the operation.</returns>
        public FileOperationContext DecryptFile(IDataStore fileInfo)
        {
            return DoFile(fileInfo, DecryptFilePreparation, DecryptFileOperation);
        }

        /// <summary>
        /// Decrypt a file, and launch the associated application raising events as required by
        /// the situation.
        /// </summary>
        /// <param name="fileInfo">The full path to an encrypted file.</param>
        /// <returns>A FileOperationStatus indicating the result of the operation.</returns>
        public FileOperationContext DecryptAndLaunch(IDataStore fileInfo)
        {
            return DoFile(fileInfo, DecryptAndLaunchPreparation, DecryptAndLaunchFileOperation);
        }

        /// <summary>
        /// Verify that a file is encrypted with a known key.
        /// </summary>
        /// <param name="fileInfo">The file to verify.</param>
        /// <returns>FileOperationStatus.Success if  the file is encrypted with a known key.</returns>
        public FileOperationContext VerifyEncrypted(IDataStore fileInfo)
        {
            return DoFile(fileInfo, DecryptAndLaunchPreparation, GetAxCryptFileNameAsSaveFileName);
        }

        /// <summary>
        /// Wipes a file securely synchronously.
        /// </summary>
        /// <param name="fileInfo">The full name of the file to wipe</param>
        /// <returns>A FileOperationStatus indicating the result of the operation.</returns>
        public FileOperationContext WipeFile(IDataStore fileInfo)
        {
            return DoFile(fileInfo, WipeFilePreparation, WipeFileOperation);
        }

        #endregion Public Methods

        #region Private Methods

        private bool EncryptFilePreparation(IDataStore sourceFileInfo)
        {
            if (String.Compare(Resolve.Portable.Path().GetExtension(sourceFileInfo.FullName), OS.Current.AxCryptExtension, StringComparison.OrdinalIgnoreCase) == 0)
            {
                _eventArgs.Status = new FileOperationContext(sourceFileInfo.FullName, FileOperationStatus.FileAlreadyEncrypted);
                return false;
            }

            if (sourceFileInfo.IsLocked)
            {
                _eventArgs.Status = new FileOperationContext(sourceFileInfo.FullName, FileOperationStatus.FileLocked);
                return false;
            }

            IDataStore destinationFileInfo = TypeMap.Resolve.New<IDataStore>(AxCryptFile.MakeAxCryptFileName(sourceFileInfo));
            _eventArgs.SaveFileFullName = destinationFileInfo.FullName;
            _eventArgs.OpenFileFullName = sourceFileInfo.FullName;
            if (destinationFileInfo.IsAvailable)
            {
                OnQuerySaveFileAs(_eventArgs);
                if (_eventArgs.Cancel)
                {
                    _eventArgs.Status = new FileOperationContext(sourceFileInfo.FullName, FileOperationStatus.Canceled);
                    return false;
                }
            }

            if (Resolve.KnownIdentities.DefaultEncryptionIdentity == null)
            {
                OnQueryEncryptionPassphrase(_eventArgs);
                if (_eventArgs.Cancel)
                {
                    _eventArgs.Status = new FileOperationContext(sourceFileInfo.FullName, FileOperationStatus.Canceled);
                    return false;
                }
            }
            else
            {
                _eventArgs.LogOnIdentity = Resolve.KnownIdentities.DefaultEncryptionIdentity;
            }

            return true;
        }

        private bool EncryptFileOperation()
        {
            _eventArgs.CryptoId = Resolve.CryptoFactory.Default.Id;
            EncryptionParameters encryptionParameters = new EncryptionParameters(_eventArgs.CryptoId, _eventArgs.LogOnIdentity.Passphrase);
            TypeMap.Resolve.New<AxCryptFile>().EncryptFileWithBackupAndWipe(_eventArgs.OpenFileFullName, _eventArgs.SaveFileFullName, encryptionParameters, _progress);

            _eventArgs.Status = new FileOperationContext(String.Empty, FileOperationStatus.Success);
            return true;
        }

        private bool DecryptFilePreparation(IDataStore fileInfo)
        {
            if (fileInfo.IsLocked)
            {
                _eventArgs.Status = new FileOperationContext(fileInfo.FullName, FileOperationStatus.FileLocked);
                return false;
            }

            if (!OpenAxCryptDocument(fileInfo, _eventArgs) || _eventArgs.Skip)
            {
                return false;
            }

            IDataStore destination = TypeMap.Resolve.New<IDataStore>(_eventArgs.SaveFileFullName);
            if (destination.IsAvailable)
            {
                OnQuerySaveFileAs(_eventArgs);
                if (_eventArgs.Cancel)
                {
                    _eventArgs.Status = new FileOperationContext(fileInfo.FullName, FileOperationStatus.Canceled);
                    return false;
                }
            }

            return true;
        }

        private bool DecryptFileOperation()
        {
            _progress.NotifyLevelStart();
            using (IAxCryptDocument document = TypeMap.Resolve.New<AxCryptFile>().Document(_eventArgs.AxCryptFile, _eventArgs.LogOnIdentity, _progress))
            {
                TypeMap.Resolve.New<AxCryptFile>().DecryptFile(document, _eventArgs.SaveFileFullName, _progress);
            }
            TypeMap.Resolve.New<AxCryptFile>().Wipe(TypeMap.Resolve.New<IDataStore>(_eventArgs.OpenFileFullName), _progress);
            _progress.NotifyLevelFinished();

            _eventArgs.Status = new FileOperationContext(String.Empty, FileOperationStatus.Success);
            return true;
        }

        private bool DecryptAndLaunchPreparation(IDataStore fileInfo)
        {
            if (!OpenAxCryptDocument(fileInfo, _eventArgs))
            {
                return false;
            }

            return true;
        }

        private bool GetAxCryptFileNameAsSaveFileName()
        {
            if (!_eventArgs.Skip)
            {
                _eventArgs.SaveFileFullName = _eventArgs.AxCryptFile.FullName;
            }

            _eventArgs.Status = new FileOperationContext(String.Empty, FileOperationStatus.Success);
            return true;
        }

        private bool DecryptAndLaunchFileOperation()
        {
            _eventArgs.Status = TypeMap.Resolve.New<FileOperation>().OpenAndLaunchApplication(_eventArgs.OpenFileFullName, _eventArgs.LogOnIdentity, _eventArgs.AxCryptFile, _progress);

            _eventArgs.Status = new FileOperationContext(String.Empty, FileOperationStatus.Success);
            return true;
        }

        private bool WipeFilePreparation(IDataStore fileInfo)
        {
            if (fileInfo.IsLocked)
            {
                _eventArgs.Status = new FileOperationContext(fileInfo.FullName, FileOperationStatus.FileLocked);
                return false;
            }

            _eventArgs.OpenFileFullName = fileInfo.FullName;
            _eventArgs.SaveFileFullName = fileInfo.FullName;
            if (_progress.AllItemsConfirmed)
            {
                return true;
            }
            OnWipeQueryConfirmation(_eventArgs);
            if (_eventArgs.Cancel)
            {
                _eventArgs.Status = new FileOperationContext(fileInfo.FullName, FileOperationStatus.Canceled);
                return false;
            }

            if (_eventArgs.ConfirmAll)
            {
                _progress.AllItemsConfirmed = true;
            }
            return true;
        }

        private bool WipeFileOperation()
        {
            if (_eventArgs.Skip)
            {
                _eventArgs.Status = new FileOperationContext(String.Empty, FileOperationStatus.Success);
                return true;
            }

            _progress.NotifyLevelStart();
            TypeMap.Resolve.New<AxCryptFile>().Wipe(TypeMap.Resolve.New<IDataStore>(_eventArgs.SaveFileFullName), _progress);
            _progress.NotifyLevelFinished();

            _eventArgs.Status = new FileOperationContext(String.Empty, FileOperationStatus.Success);
            return true;
        }

        private bool OpenAxCryptDocument(IDataStore sourceFileInfo, FileOperationEventArgs e)
        {
            try
            {
                _progress.NotifyLevelStart();
                e.OpenFileFullName = sourceFileInfo.FullName;
                if (TryFindDecryptionKey(sourceFileInfo, e))
                {
                    return InfoFromDecryptedDocument(sourceFileInfo, e);
                }

                while (true)
                {
                    OnQueryDecryptionPassphrase(e);
                    if (e.Cancel)
                    {
                        e.Status = new FileOperationContext(sourceFileInfo.FullName, FileOperationStatus.Canceled);
                        return false;
                    }
                    if (e.Skip)
                    {
                        e.Status = new FileOperationContext(String.Empty, FileOperationStatus.Success);
                        return true;
                    }
                    if (InfoFromDecryptedDocument(sourceFileInfo, e))
                    {
                        OnKnownKeyAdded(e);
                        return true;
                    }
                }
            }
            catch (IOException ioex)
            {
                FileOperationContext status = new FileOperationContext(sourceFileInfo.FullName, ioex is FileNotFoundException ? FileOperationStatus.FileDoesNotExist : FileOperationStatus.Exception);
                e.Status = status;
                return false;
            }
            finally
            {
                _progress.NotifyLevelFinished();
            }
        }

        private bool InfoFromDecryptedDocument(IDataStore sourceFileInfo, FileOperationEventArgs e)
        {
            using (IAxCryptDocument document = TypeMap.Resolve.New<AxCryptFile>().Document(sourceFileInfo, e.LogOnIdentity, _progress))
            {
                if (!document.PassphraseIsValid)
                {
                    return false;
                }
                e.CryptoId = document.CryptoFactory.Id;
                IDataStore destination = TypeMap.Resolve.New<IDataStore>(Resolve.Portable.Path().Combine(Resolve.Portable.Path().GetDirectoryName(sourceFileInfo.FullName), document.FileName));
                e.SaveFileFullName = destination.FullName;
            }
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

        private FileOperationContext DoFile(IDataStore fileInfo, Func<IDataStore, bool> preparation, Func<bool> operation)
        {
            try
            {
                _progress.NotifyLevelStart();
                bool ok = RunOnUIThread(fileInfo, preparation);
                if (ok)
                {
                    operation();
                }
            }
            finally
            {
                OnCompleted(_eventArgs);
                _progress.NotifyLevelFinished();
            }

            return _eventArgs.Status;
        }

        private bool RunOnUIThread(IDataStore fileInfo, Func<IDataStore, bool> preparation)
        {
            bool ok = false;
            _progress.EnterSingleThread();
            try
            {
                if (_progress.Cancel)
                {
                    _eventArgs.Status = new FileOperationContext(fileInfo.FullName, FileOperationStatus.Canceled);
                    return ok;
                }
                Resolve.UIThread.RunOnUIThread(() => ok = preparation(fileInfo));
                if (_eventArgs.Status.Status == FileOperationStatus.Canceled)
                {
                    _progress.Cancel = true;
                }
            }
            catch (Exception)
            {
                _eventArgs.Status = new FileOperationContext(fileInfo.FullName, FileOperationStatus.Exception);
                throw;
            }
            finally
            {
                _progress.LeaveSingleThread();
            }
            return ok;
        }

        #endregion Private Methods
    }
}