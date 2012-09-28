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

        public FileOperationsController(FileSystemState fileSystemState)
        {
            _fileSystemState = fileSystemState;
        }

        public event EventHandler<FileOperationEventArgs> SaveFileRequest;

        protected virtual void OnSaveFileRequest(FileOperationEventArgs e)
        {
            EventHandler<FileOperationEventArgs> handler = SaveFileRequest;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<FileOperationEventArgs> DecryptionPassphraseRequest;

        protected virtual void OnDecryptionPassphraseRequest(FileOperationEventArgs e)
        {
            EventHandler<FileOperationEventArgs> handler = DecryptionPassphraseRequest;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<FileOperationEventArgs> EncryptionPassphraseRequest;

        protected virtual void OnEncryptionPassphraseRequest(FileOperationEventArgs e)
        {
            EventHandler<FileOperationEventArgs> handler = EncryptionPassphraseRequest;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<FileOperationEventArgs> FileOperationRequest;

        protected virtual void OnFileOperationRequest(FileOperationEventArgs e)
        {
            EventHandler<FileOperationEventArgs> handler = FileOperationRequest;
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

        public event EventHandler<FileOperationEventArgs> DefaultEncryptionKeySet;

        protected virtual void OnDefaultEncryptionKeySet(FileOperationEventArgs e)
        {
            EventHandler<FileOperationEventArgs> handler = DefaultEncryptionKeySet;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public Func<FileOperationEventArgs, FileOperationStatus> DoOperation { get; private set; }

        public bool EncryptFile(string file)
        {
            if (String.Compare(Path.GetExtension(file), OS.Current.AxCryptExtension, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return false;
            }
            IRuntimeFileInfo sourceFileInfo = OS.Current.FileInfo(file);
            IRuntimeFileInfo destinationFileInfo = OS.Current.FileInfo(AxCryptFile.MakeAxCryptFileName(sourceFileInfo));
            FileOperationEventArgs e = new FileOperationEventArgs();
            e.SaveFileName = destinationFileInfo.FullName;
            e.OpenFileName = file;
            if (destinationFileInfo.Exists)
            {
                OnSaveFileRequest(e);
                if (e.Cancel)
                {
                    return false;
                }
            }

            if (_fileSystemState.KnownKeys.DefaultEncryptionKey == null)
            {
                OnEncryptionPassphraseRequest(e);
                if (e.Cancel)
                {
                    return false;
                }
                Passphrase passphrase = new Passphrase(e.Passphrase);
                e.Key = passphrase.DerivedPassphrase;
            }
            else
            {
                e.Key = _fileSystemState.KnownKeys.DefaultEncryptionKey;
            }
            DoOperation = EncryptFileOperation;
            OnFileOperationRequest(e);
            return true;
        }

        public bool DecryptFile(string file)
        {
            FileOperationEventArgs e = new FileOperationEventArgs();

            e.AxCryptDocument = null;
            try
            {
                IRuntimeFileInfo source = OS.Current.FileInfo(file);
                e.OpenFileName = source.FullName;
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
                    OnDecryptionPassphraseRequest(e);
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

            IRuntimeFileInfo destination = OS.Current.FileInfo(Path.Combine(Path.GetDirectoryName(file), e.AxCryptDocument.DocumentHeaders.FileName));
            if (destination.Exists)
            {
                OnSaveFileRequest(e);
                if (e.Cancel)
                {
                    return false;
                }
            }
            e.SaveFileName = destination.FullName;

            DoOperation = DecryptFileOperation;
            OnFileOperationRequest(e);
            return true;
        }

        private static FileOperationStatus DecryptFileOperation(FileOperationEventArgs e)
        {
            try
            {
                AxCryptFile.Decrypt(e.AxCryptDocument, OS.Current.FileInfo(e.SaveFileName), AxCryptOptions.SetFileTimes, e.Progress);
            }
            finally
            {
                e.AxCryptDocument.Dispose();
                e.AxCryptDocument = null;
            }
            AxCryptFile.Wipe(OS.Current.FileInfo(e.OpenFileName));
            return FileOperationStatus.Success;
        }

        private static FileOperationStatus EncryptFileOperation(FileOperationEventArgs e)
        {
            AxCryptFile.EncryptFileWithBackupAndWipe(e.OpenFileName, e.SaveFileName, e.Key, e.Progress);
            return FileOperationStatus.Success;
        }
    }
}