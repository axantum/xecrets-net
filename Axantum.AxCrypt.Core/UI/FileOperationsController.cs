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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;

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
        private FileSystemState _fileSystemState;

        private FileOperationEventArgs _eventArgs;

        private ProgressContext _progress;

        /// <summary>
        /// Create a new instance, without any progress reporting.
        /// </summary>
        /// <param name="fileSystemState">The current FileSystemStatem instance</param>
        public FileOperationsController(FileSystemState fileSystemState)
            : this(fileSystemState, new ProgressContext())
        {
        }

        /// <summary>
        /// Create a new instance, reporting progress
        /// </summary>
        /// <param name="fileSystemState">The current FileSystemStatem instance</param>
        /// <param name="progress">The instance of ProgressContext to report progress via</param>
        public FileOperationsController(FileSystemState fileSystemState, ProgressContext progress)
        {
            _eventArgs = new FileOperationEventArgs();
            _fileSystemState = fileSystemState;
            _progress = progress;
        }

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
        public FileOperationStatus EncryptFile(string fullName)
        {
            return DoFile(fullName, EncryptFilePreparation, EncryptFileOperation);
        }

        /// <summary>
        /// Process a file with the main task performed in a background thread.
        /// </summary>
        /// <param name="fullName"></param>
        public void EncryptFile(string fullName, IThreadWorker worker)
        {
            DoFile(fullName, worker, EncryptFilePreparation, EncryptFileOperation);
        }

        private bool EncryptFilePreparation(string fullName)
        {
            if (String.Compare(Path.GetExtension(fullName), OS.Current.AxCryptExtension, StringComparison.OrdinalIgnoreCase) == 0)
            {
                _eventArgs.Status = FileOperationStatus.InvalidPath;
                return false;
            }
            IRuntimeFileInfo sourceFileInfo = OS.Current.FileInfo(fullName);
            IRuntimeFileInfo destinationFileInfo = OS.Current.FileInfo(AxCryptFile.MakeAxCryptFileName(sourceFileInfo));
            _eventArgs.SaveFileFullName = destinationFileInfo.FullName;
            _eventArgs.OpenFileFullName = fullName;
            if (destinationFileInfo.Exists)
            {
                OnQuerySaveFileAs(_eventArgs);
                if (_eventArgs.Cancel)
                {
                    _eventArgs.Status = FileOperationStatus.Canceled;
                    return false;
                }
            }

            if (_fileSystemState.KnownKeys.DefaultEncryptionKey == null)
            {
                OnQueryEncryptionPassphrase(_eventArgs);
                if (_eventArgs.Cancel)
                {
                    _eventArgs.Status = FileOperationStatus.Canceled;
                    return false;
                }
                Passphrase passphrase = new Passphrase(_eventArgs.Passphrase);
                _eventArgs.Key = passphrase.DerivedPassphrase;
            }
            else
            {
                _eventArgs.Key = _fileSystemState.KnownKeys.DefaultEncryptionKey;
            }

            return true;
        }

        private bool EncryptFileOperation()
        {
            AxCryptFile.EncryptFileWithBackupAndWipe(_eventArgs.OpenFileFullName, _eventArgs.SaveFileFullName, _eventArgs.Key, _progress);

            _eventArgs.Status = FileOperationStatus.Success;
            return true;
        }

        /// <summary>
        /// Decrypt a file, raising events as required by the situation.
        /// </summary>
        /// <param name="sourceFile">The full path to an encrypted file.</param>
        /// <returns>'True' if the operation did not fail so far, 'False' if it definitely has failed.</returns>
        /// <remarks>
        /// Since especially the actual operation typically is executed asynchronously, the
        /// return value and status do not conclusive indicate success. Only a failure return
        /// is conclusive.
        /// </remarks>
        public FileOperationStatus DecryptFile(string fullName)
        {
            return DoFile(fullName, DecryptFilePreparation, DecryptFileOperation);
        }

        /// <summary>
        /// Process a file with the main task performed in a background thread.
        /// </summary>
        /// <param name="fullName"></param>
        public void DecryptFile(string fullName, IThreadWorker worker)
        {
            DoFile(fullName, worker, DecryptFilePreparation, DecryptFileOperation);
        }

        private bool DecryptFilePreparation(string fullName)
        {
            if (!OpenAxCryptDocument(fullName, _eventArgs))
            {
                return false;
            }

            IRuntimeFileInfo destination = OS.Current.FileInfo(Path.Combine(Path.GetDirectoryName(fullName), _eventArgs.AxCryptDocument.DocumentHeaders.FileName));
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
                AxCryptFile.Decrypt(_eventArgs.AxCryptDocument, OS.Current.FileInfo(_eventArgs.SaveFileFullName), AxCryptOptions.SetFileTimes, _progress);
            }
            finally
            {
                _eventArgs.AxCryptDocument.Dispose();
                _eventArgs.AxCryptDocument = null;
            }
            AxCryptFile.Wipe(OS.Current.FileInfo(_eventArgs.OpenFileFullName), _progress);
            _progress.NotifyLevelFinished();

            _eventArgs.Status = FileOperationStatus.Success;
            return true;
        }

        /// <summary>
        /// Decrypt a file, and launch the associated application raising events as required by
        /// the situation.
        /// </summary>
        /// <param name="fullName">The full path to an encrypted file.</param>
        /// <returns>'True' if the operation did not fail so far, 'False' if it definitely has failed.</returns>
        public FileOperationStatus DecryptAndLaunch(string fullName)
        {
            return DoFile(fullName, DecryptAndLaunchPreparation, DecryptAndLaunchFileOperation);
        }

        /// <summary>
        /// Decrypt a file, and launch the associated application raising events as required by
        /// the situation. The decryption is performed asynchronously on a background thread.
        /// </summary>
        /// <param name="sourceFile">The full path to an encrypted file.</param>
        /// <param name="worker">The worker thread on which to execute the decryption and launch.</param>
        /// <returns>'True' if the operation did not fail so far, 'False' if it definitely has failed.</returns>
        public void DecryptAndLaunch(string fullName, IThreadWorker worker)
        {
            DoFile(fullName, worker, DecryptAndLaunchPreparation, DecryptAndLaunchFileOperation);
        }

        private bool DecryptAndLaunchPreparation(string fullName)
        {
            if (!OpenAxCryptDocument(fullName, _eventArgs))
            {
                return false;
            }

            return true;
        }

        private bool DecryptAndLaunchFileOperation()
        {
            try
            {
                _eventArgs.Status = _fileSystemState.OpenAndLaunchApplication(_eventArgs.OpenFileFullName, _eventArgs.AxCryptDocument, _progress);
            }
            finally
            {
                _eventArgs.AxCryptDocument.Dispose();
                _eventArgs.AxCryptDocument = null;
            }

            _eventArgs.Status = FileOperationStatus.Success;
            return true;
        }

        private bool OpenAxCryptDocument(string fullName, FileOperationEventArgs e)
        {
            e.AxCryptDocument = null;
            try
            {
                IRuntimeFileInfo source = OS.Current.FileInfo(fullName);
                e.OpenFileFullName = source.FullName;
                foreach (AesKey key in _fileSystemState.KnownKeys.Keys)
                {
                    e.AxCryptDocument = AxCryptFile.Document(source, key, new ProgressContext());
                    if (e.AxCryptDocument.PassphraseIsValid)
                    {
                        break;
                    }
                    e.AxCryptDocument.Dispose();
                    e.AxCryptDocument = null;
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
                    passphrase = new Passphrase(e.Passphrase);
                    e.AxCryptDocument = AxCryptFile.Document(source, passphrase.DerivedPassphrase, new ProgressContext());
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

        private void DoFile(string fullName, IThreadWorker worker, Func<string, bool> preparation, Func<bool> operation)
        {
            if (!preparation(fullName))
            {
                worker.Abort();
                OnCompleted(_eventArgs);
                return;
            }
            worker.Work += (object workerSender, ThreadWorkerEventArgs threadWorkerEventArgs) =>
            {
                operation();
            };
            worker.Completing += (object workerSender, ThreadWorkerEventArgs threadWorkerEventArgs) =>
            {
                _eventArgs.Status = threadWorkerEventArgs.Result;
                OnCompleted(_eventArgs);
            };
            worker.Run();
        }

        private FileOperationStatus DoFile(string fullName, Func<string, bool> preparation, Func<bool> operation)
        {
            if (preparation(fullName))
            {
                operation();
            }
            OnCompleted(_eventArgs);

            return _eventArgs.Status;
        }
    }
}