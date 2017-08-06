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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Test
{
    internal class MockAxCryptFile : AxCryptFile
    {
        public MockAxCryptFile()
        {
            EncryptMock = (sourceFile, destinationFile, passphrase, options, progress) => { throw new InvalidOperationException("Unexpected call to this method."); };
            EncryptFilesUniqueWithBackupAndWipeMockAsync = (fileInfo, encryptionParameters, progress) => { throw new InvalidOperationException("Unexpected call to this method."); };
            EncryptFileUniqueWithBackupAndWipeMockAsync = (fileInfo, encryptionParameters, progress) => { throw new InvalidOperationException("Unexpected call to this method."); };
            DecryptFilesUniqueWithWipeOfOriginalMockAsync = (fileInfo, decryptionKey, statusChecker, progress) => { throw new InvalidOperationException("Unexpected call to this method."); };
        }

        public delegate void Action<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

        public Action<IDataStore, IDataStore, LogOnIdentity, AxCryptOptions, IProgressContext> EncryptMock { get; set; }

        public Func<IEnumerable<IDataContainer>, EncryptionParameters, IProgressContext, Task> EncryptFilesUniqueWithBackupAndWipeMockAsync { get; set; }

        public override Task EncryptFoldersUniqueWithBackupAndWipeAsync(IEnumerable<IDataContainer> folderInfos, EncryptionParameters encryptionParameters, IProgressContext progress)
        {
            return EncryptFilesUniqueWithBackupAndWipeMockAsync(folderInfos, encryptionParameters, progress);
        }

        public Func<IDataStore, EncryptionParameters, IProgressContext, Task> EncryptFileUniqueWithBackupAndWipeMockAsync { get; set; }

        public override Task EncryptFileUniqueWithBackupAndWipeAsync(IDataStore fileInfo, EncryptionParameters encryptionParameters, IProgressContext progress)
        {
            return EncryptFileUniqueWithBackupAndWipeMockAsync(fileInfo, encryptionParameters, progress);
        }

        public Action<IDataContainer, LogOnIdentity, IStatusChecker, IProgressContext> DecryptFilesUniqueWithWipeOfOriginalMockAsync { get; set; }

        public override Task DecryptFilesInsideFolderUniqueWithWipeOfOriginalAsync(IDataContainer fileInfo, LogOnIdentity decryptionKey, IStatusChecker statusChecker, IProgressContext progress)
        {
            DecryptFilesUniqueWithWipeOfOriginalMockAsync(fileInfo, decryptionKey, statusChecker, progress);
            return Task.FromResult<object>(null);
        }
    }
}