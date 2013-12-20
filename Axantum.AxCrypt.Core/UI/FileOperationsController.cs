#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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
        /// Raised when a valid decryption passphrase was not found among the KnownKeys collection.
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
        /// Raised when the KnownKeys.DefaultEncryptionKey is not set.
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
        public FileOperationStatus EncryptFile(IRuntimeFileInfo fileInfo)
        {
            return DoFile(fileInfo, EncryptFilePreparation, EncryptFileOperation);
        }

        /// <summary>
        /// Decrypt a file, raising events as required by the situation.
        /// </summary>
        /// <param name="sourceFile">The full path to an encrypted file.</param>
        /// <returns>The resulting status of the operation.</returns>
        public FileOperationStatus DecryptFile(IRuntimeFileInfo fileInfo)
        {
            return DoFile(fileInfo, DecryptFilePreparation, DecryptFileOperation);
        }

        /// <summary>
        /// Decrypt a file, and launch the associated application raising events as required by
        /// the situation.
        /// </summary>
        /// <param name="fileInfo">The full path to an encrypted file.</param>
        /// <returns>A FileOperationStatus indicating the result of the operation.</returns>
        public FileOperationStatus DecryptAndLaunch(IRuntimeFileInfo fileInfo)
        {
            return DoFile(fileInfo, DecryptAndLaunchPreparation, DecryptAndLaunchFileOperation);
        }

        /// <summary>
        /// Verify that a file is encrypted with a known key.
        /// </summary>
        /// <param name="fileInfo">The file to verify.</param>
        /// <returns>FileOperationStatus.Success if  the file is encrypted with a known key.</returns>
        public FileOperationStatus VerifyEncrypted(IRuntimeFileInfo fileInfo)
        {
            return DoFile(fileInfo, DecryptAndLaunchPreparation, GetDocumentInfo);
        }

        /// <summary>
        /// Wipes a file securely synchronously.
        /// </summary>
        /// <param name="fileInfo">The full name of the file to wipe</param>
        /// <returns>A FileOperationStatus indicating the result of the operation.</returns>
        public FileOperationStatus WipeFile(IRuntimeFileInfo fileInfo)
        {
            return DoFile(fileInfo, WipeFilePreparation, WipeFileOperation);
        }

        #endregion Public Methods

        #region Private Methods

        private bool EncryptFilePreparation(IRuntimeFileInfo sourceFileInfo)
        {
            if (String.Compare(Path.GetExtension(sourceFileInfo.FullName), OS.Current.AxCryptExtension, StringComparison.OrdinalIgnoreCase) == 0)
            {
                _eventArgs.Status = FileOperationStatus.FileAlreadyEncrypted;
                return false;
            }
            IRuntimeFileInfo destinationFileInfo = OS.Current.FileInfo(AxCryptFile.MakeAxCryptFileName(sourceFileInfo));
            _eventArgs.SaveFileFullName = destinationFileInfo.FullName;
            _eventArgs.OpenFileFullName = sourceFileInfo.FullName;
            if (destinationFileInfo.Exists)
            {
                OnQuerySaveFileAs(_eventArgs);
                if (_eventArgs.Cancel)
                {
                    _eventArgs.Status = FileOperationStatus.Canceled;
                    return false;
                }
            }

            if (Instance.KnownKeys.DefaultEncryptionKey == null)
            {
                OnQueryEncryptionPassphrase(_eventArgs);
                if (_eventArgs.Cancel)
                {
                    _eventArgs.Status = FileOperationStatus.Canceled;
                    return false;
                }
                _eventArgs.Key = Passphrase.Derive(_eventArgs.Passphrase);
            }
            else
            {
                _eventArgs.Key = Instance.KnownKeys.DefaultEncryptionKey;
            }

            return true;
        }

        private bool EncryptFileOperation()
        {
            AxCryptFile.EncryptFileWithBackupAndWipe(_eventArgs.OpenFileFullName, _eventArgs.SaveFileFullName, _eventArgs.Key, _progress);

            _eventArgs.Status = FileOperationStatus.Success;
            return true;
        }

        private bool DecryptFilePreparation(IRuntimeFileInfo fileInfo)
        {
            if (!OpenAxCryptDocument(fileInfo, _eventArgs) || _eventArgs.Skip)
            {
                return false;
            }

            IRuntimeFileInfo destination = OS.Current.FileInfo(Path.Combine(Path.GetDirectoryName(fileInfo.FullName), _eventArgs.AxCryptDocument.DocumentHeaders.FileName));
            _eventArgs.SaveFileFullName = destination.FullName;
            if (destination.Exists)
            {
                OnQuerySaveFileAs(_eventArgs);
                if (_eventArgs.Cancel)
                {
                    _eventArgs.Status = FileOperationStatus.Canceled;
                    return false;
                }
            }

            return true;
        }

        private bool DecryptFileOperation()
        {
            _progress.NotifyLevelStart();
            try
            {
                AxCryptFile.DecryptFile(_eventArgs.AxCryptDocument, _eventArgs.SaveFileFullName, _progress);
            }
            finally
            {
                _eventArgs.AxCryptDocument.Dispose();
                _eventArgs.AxCryptDocument = null;
            }
            AxCryptFile.Wipe(_eventArgs.OpenFileFullName, _progress);

            _progress.NotifyLevelFinished();

            _eventArgs.Status = FileOperationStatus.Success;
            return true;
        }

        private bool DecryptAndLaunchPreparation(IRuntimeFileInfo fileInfo)
        {
            if (!OpenAxCryptDocument(fileInfo, _eventArgs))
            {
                return false;
            }

            return true;
        }

        private bool GetDocumentInfo()
        {
            try
            {
                if (!_eventArgs.Skip)
                {
                    _eventArgs.SaveFileFullName = _eventArgs.AxCryptDocument.DocumentHeaders.FileName;
                }
            }
            finally
            {
                if (_eventArgs.AxCryptDocument != null)
                {
                    _eventArgs.AxCryptDocument.Dispose();
                    _eventArgs.AxCryptDocument = null;
                }
            }

            _eventArgs.Status = FileOperationStatus.Success;
            return true;
        }

        private bool DecryptAndLaunchFileOperation()
        {
            try
            {
                _eventArgs.Status = Factory.New<FileOperation>().OpenAndLaunchApplication(_eventArgs.OpenFileFullName, _eventArgs.AxCryptDocument, _progress);
            }
            finally
            {
                _eventArgs.AxCryptDocument.Dispose();
                _eventArgs.AxCryptDocument = null;
            }

            _eventArgs.Status = FileOperationStatus.Success;
            return true;
        }

        private bool WipeFilePreparation(IRuntimeFileInfo fileInfo)
        {
            _eventArgs.OpenFileFullName = fileInfo.FullName;
            _eventArgs.SaveFileFullName = fileInfo.FullName;
            if (_progress.AllItemsConfirmed)
            {
                return true;
            }
            OnWipeQueryConfirmation(_eventArgs);
            if (_eventArgs.Cancel)
            {
                _eventArgs.Status = FileOperationStatus.Canceled;
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
                _eventArgs.Status = FileOperationStatus.Success;
                return true;
            }

            _progress.NotifyLevelStart();
            AxCryptFile.Wipe(OS.Current.FileInfo(_eventArgs.SaveFileFullName), _progress);
            _progress.NotifyLevelFinished();

            _eventArgs.Status = FileOperationStatus.Success;
            return true;
        }

        private bool OpenAxCryptDocument(IRuntimeFileInfo sourceFileInfo, FileOperationEventArgs e)
        {
            e.AxCryptDocument = null;
            try
            {
                e.OpenFileFullName = sourceFileInfo.FullName;
                AesKey key;
                if (sourceFileInfo.TryFindDecryptionKey(out key))
                {
                    e.AxCryptDocument = AxCryptFile.Document(sourceFileInfo, key, new ProgressContext());
                    e.Key = key;
                }

                Passphrase passphrase;
                while (e.AxCryptDocument == null)
                {
                    OnQueryDecryptionPassphrase(e);
                    if (e.Cancel)
                    {
                        e.Status = FileOperationStatus.Canceled;
                        return false;
                    }
                    if (e.Skip)
                    {
                        e.Status = FileOperationStatus.Success;
                        return true;
                    }
                    passphrase = new Passphrase(e.Passphrase);
                    e.AxCryptDocument = AxCryptFile.Document(sourceFileInfo, passphrase.DerivedPassphrase, new ProgressContext());
                    if (!e.AxCryptDocument.PassphraseIsValid)
                    {
                        e.AxCryptDocument.Dispose();
                        e.AxCryptDocument = null;
                        continue;
                    }
                    e.Key = passphrase.DerivedPassphrase;
                    OnKnownKeyAdded(e);
                }
            }
            catch (IOException ioex)
            {
                if (e.AxCryptDocument != null)
                {
                    e.AxCryptDocument.Dispose();
                    e.AxCryptDocument = null;
                }
                FileOperationStatus status = ioex is FileNotFoundException ? FileOperationStatus.FileDoesNotExist : FileOperationStatus.Exception;
                e.Status = status;
                return false;
            }
            return true;
        }

        private FileOperationStatus DoFile(IRuntimeFileInfo fileInfo, Func<IRuntimeFileInfo, bool> preparation, Func<bool> operation)
        {
            bool ok = false;
            Instance.UIThread.RunOnUIThread(() =>
            {
                ok = preparation(fileInfo);
            });
            if (ok)
            {
                operation();
            }
            OnCompleted(_eventArgs);

            return _eventArgs.Status;
        }

        #endregion Private Methods
    }
}