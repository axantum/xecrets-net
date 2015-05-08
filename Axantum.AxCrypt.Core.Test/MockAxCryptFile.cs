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
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    internal class MockAxCryptFile : AxCryptFile
    {
        public MockAxCryptFile()
        {
            EncryptMock = (sourceFile, destinationFile, passphrase, options, progress) => { throw new InvalidOperationException("Unexpected call to this method."); };
            EncryptFilesUniqueWithBackupAndWipeMock = (fileInfo, encryptionParameters, progress) => { throw new InvalidOperationException("Unexpected call to this method."); };
            EncryptFileUniqueWithBackupAndWipeMock = (fileInfo, encryptionParameters, progress) => { throw new InvalidOperationException("Unexpected call to this method."); };
            DecryptFilesUniqueWithWipeOfOriginalMock = (fileInfo, decryptionKey, statusChecker, progress) => { throw new InvalidOperationException("Unexpected call to this method."); };
        }

        public delegate void Action<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

        public Action<IDataStore, IDataStore, LogOnIdentity, AxCryptOptions, IProgressContext> EncryptMock { get; set; }

        public Action<IEnumerable<IDataContainer>, EncryptionParameters, IProgressContext> EncryptFilesUniqueWithBackupAndWipeMock { get; set; }

        public override void EncryptFoldersUniqueWithBackupAndWipe(IEnumerable<IDataContainer> folderInfos, EncryptionParameters encryptionParameters, IProgressContext progress)
        {
            EncryptFilesUniqueWithBackupAndWipeMock(folderInfos, encryptionParameters, progress);
        }

        public Action<IDataStore, EncryptionParameters, IProgressContext> EncryptFileUniqueWithBackupAndWipeMock { get; set; }

        public override void EncryptFileUniqueWithBackupAndWipe(IDataStore fileInfo, EncryptionParameters encryptionParameters, IProgressContext progress)
        {
            EncryptFileUniqueWithBackupAndWipeMock(fileInfo, encryptionParameters, progress);
        }

        public Action<IDataContainer, LogOnIdentity, IStatusChecker, IProgressContext> DecryptFilesUniqueWithWipeOfOriginalMock { get; set; }

        public override void DecryptFilesInsideFolderUniqueWithWipeOfOriginal(IDataContainer fileInfo, LogOnIdentity decryptionKey, IStatusChecker statusChecker, IProgressContext progress)
        {
            DecryptFilesUniqueWithWipeOfOriginalMock(fileInfo, decryptionKey, statusChecker, progress);
        }
    }
}