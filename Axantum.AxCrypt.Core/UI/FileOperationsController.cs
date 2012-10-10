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
using Axantum.AxCrypt.Core.Session;

namespace Axantum.AxCrypt.Core.UI
{
    public class FileOperationsController
    {
        private FileSystemState _fileSystemState;

        private FileOperationEventArgs _eventArgs;

        public FileOperationsController(FileSystemState fileSystemState, string displayContext)
        {
            _eventArgs = new FileOperationEventArgs(displayContext, new ProgressContext());
            _fileSystemState = fileSystemState;
        }

        public event EventHandler<FileOperationEventArgs> QuerySaveFileAs;

        protected virtual void OnQuerySaveFileAs(FileOperationEventArgs e)
        {
            EventHandler<FileOperationEventArgs> handler = QuerySaveFileAs;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<FileOperationEventArgs> QueryDecryptionPassphrase;

        protected virtual void OnQueryDecryptionPassphrase(FileOperationEventArgs e)
        {
            EventHandler<FileOperationEventArgs> handler = QueryDecryptionPassphrase;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<FileOperationEventArgs> QueryEncryptionPassphrase;

        protected virtual void OnQueryEncryptionPassphrase(FileOperationEventArgs e)
        {
            EventHandler<FileOperationEventArgs> handler = QueryEncryptionPassphrase;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<FileOperationEventArgs> ProcessFile;

        protected virtual void OnProcessFile(FileOperationEventArgs e)
        {
            EventHandler<FileOperationEventArgs> handler = ProcessFile;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<FileOperationEventArgs> KnownKeyAdded;

        protected virtual void OnKnownKeyAdded(FileOperationEventArgs e)
        {
            EventHandler<FileOperationEventArgs> handler = KnownKeyAdded;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public Func<FileOperationEventArgs, FileOperationStatus> DoProcessFile { get; private set; }

        public bool EncryptFile(string sourceFile)
        {
            if (String.Compare(Path.GetExtension(sourceFile), OS.Current.AxCryptExtension, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return false;
            }
            IRuntimeFileInfo sourceFileInfo = OS.Current.FileInfo(sourceFile);
            IRuntimeFileInfo destinationFileInfo = OS.Current.FileInfo(AxCryptFile.MakeAxCryptFileName(sourceFileInfo));
            _eventArgs.SaveFileFullName = destinationFileInfo.FullName;
            _eventArgs.OpenFileFullName = sourceFile;
            if (destinationFileInfo.Exists)
            {
                OnQuerySaveFileAs(_eventArgs);
                if (_eventArgs.Cancel)
                {
                    return false;
                }
            }

            if (_fileSystemState.KnownKeys.DefaultEncryptionKey == null)
            {
                OnQueryEncryptionPassphrase(_eventArgs);
                if (_eventArgs.Cancel)
                {
                    return false;
                }
                Passphrase passphrase = new Passphrase(_eventArgs.Passphrase);
                _eventArgs.Key = passphrase.DerivedPassphrase;
            }
            else
            {
                _eventArgs.Key = _fileSystemState.KnownKeys.DefaultEncryptionKey;
            }
            DoProcessFile = EncryptFileOperation;
            OnProcessFile(_eventArgs);
            return true;
        }

        public bool DecryptFile(string sourceFile)
        {
            if (!OpenAxCryptDocument(sourceFile, _eventArgs))
            {
                return false;
            }

            IRuntimeFileInfo destination = OS.Current.FileInfo(Path.Combine(Path.GetDirectoryName(sourceFile), _eventArgs.AxCryptDocument.DocumentHeaders.FileName));
            _eventArgs.SaveFileFullName = destination.FullName;
            if (destination.Exists)
            {
                OnQuerySaveFileAs(_eventArgs);
                if (_eventArgs.Cancel)
                {
                    return false;
                }
            }

            DoProcessFile = DecryptFileOperation;
            OnProcessFile(_eventArgs);
            return true;
        }

        public bool DecryptAndLaunch(string sourceFile)
        {
            if (!OpenAxCryptDocument(sourceFile, _eventArgs))
            {
                return false;
            }
            DoProcessFile = DecryptAndLaunchFileOperation;
            OnProcessFile(_eventArgs);
            return false;
        }

        private bool OpenAxCryptDocument(string sourceFile, FileOperationEventArgs e)
        {
            e.AxCryptDocument = null;
            try
            {
                IRuntimeFileInfo source = OS.Current.FileInfo(sourceFile);
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
            catch (Exception)
            {
                if (e.AxCryptDocument != null)
                {
                    e.AxCryptDocument.Dispose();
                }
                throw;
            }
            return true;
        }

        private FileOperationStatus DecryptAndLaunchFileOperation(FileOperationEventArgs e)
        {
            try
            {
                return _fileSystemState.OpenAndLaunchApplication(e.OpenFileFullName, e.AxCryptDocument, e.Progress);
            }
            finally
            {
                e.AxCryptDocument.Dispose();
                e.AxCryptDocument = null;
            }
        }

        private static FileOperationStatus DecryptFileOperation(FileOperationEventArgs e)
        {
            try
            {
                AxCryptFile.Decrypt(e.AxCryptDocument, OS.Current.FileInfo(e.SaveFileFullName), AxCryptOptions.SetFileTimes, e.Progress);
            }
            finally
            {
                e.AxCryptDocument.Dispose();
                e.AxCryptDocument = null;
            }
            AxCryptFile.Wipe(OS.Current.FileInfo(e.OpenFileFullName));
            return FileOperationStatus.Success;
        }

        private static FileOperationStatus EncryptFileOperation(FileOperationEventArgs e)
        {
            AxCryptFile.EncryptFileWithBackupAndWipe(e.OpenFileFullName, e.SaveFileFullName, e.Key, e.Progress);
            return FileOperationStatus.Success;
        }
    }
}