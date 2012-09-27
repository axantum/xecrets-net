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

        public event EventHandler<FileOperationEventArgs> EncryptionPassphraseRequest;

        protected virtual void OnEncryptionPassphraseRequest(FileOperationEventArgs e)
        {
            EventHandler<FileOperationEventArgs> handler = EncryptionPassphraseRequest;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<FileOperationEventArgs> DoOperation;

        protected virtual void OnDoOperation(FileOperationEventArgs e)
        {
            EventHandler<FileOperationEventArgs> handler = DoOperation;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public bool EncryptFile(string file)
        {
            if (String.Compare(Path.GetExtension(file), Os.Current.AxCryptExtension, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return false;
            }
            IRuntimeFileInfo sourceFileInfo = Os.Current.FileInfo(file);
            IRuntimeFileInfo destinationFileInfo = Os.Current.FileInfo(AxCryptFile.MakeAxCryptFileName(sourceFileInfo));
            FileOperationEventArgs e = new FileOperationEventArgs();
            e.FileName = destinationFileInfo.FullName;
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
                _fileSystemState.KnownKeys.DefaultEncryptionKey = passphrase.DerivedPassphrase;
            }

            e.Key = _fileSystemState.KnownKeys.DefaultEncryptionKey;
            OnDoOperation(e);
            return true;
        }
    }
}