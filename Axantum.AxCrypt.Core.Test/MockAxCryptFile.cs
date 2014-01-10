#region Coypright and License

/*
 * AxCrypt - Copyright 2013, Svante Seleborg, All Rights Reserved
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
            EncryptFilesUniqueWithBackupAndWipeMock = (fileInfo, encryptionKey, progress) => { throw new InvalidOperationException("Unexpected call to this method."); };
            EncryptFileUniqueWithBackupAndWipeMock = (fileInfo, encryptionKey, progress) => { throw new InvalidOperationException("Unexpected call to this method."); };
            DecryptFilesUniqueWithWipeOfOriginalMock = (fileInfo, decryptionKey, progress) => { throw new InvalidOperationException("Unexpected call to this method."); };
        }

        public delegate void Action<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

        public Action<IRuntimeFileInfo, IRuntimeFileInfo, Passphrase, AxCryptOptions, IProgressContext> EncryptMock { get; set; }

        public override void Encrypt(IRuntimeFileInfo sourceFile, IRuntimeFileInfo destinationFile, Passphrase passphrase, AxCryptOptions options, IProgressContext progress)
        {
            EncryptMock(sourceFile, destinationFile, passphrase, options, progress);
        }

        public Action<IEnumerable<IRuntimeFileInfo>, AesKey, IProgressContext> EncryptFilesUniqueWithBackupAndWipeMock { get; set; }

        public override void EncryptFilesUniqueWithBackupAndWipe(IEnumerable<IRuntimeFileInfo> folderInfos, AesKey encryptionKey, IProgressContext progress)
        {
            EncryptFilesUniqueWithBackupAndWipeMock(folderInfos, encryptionKey, progress);
        }

        public Action<IRuntimeFileInfo, AesKey, IProgressContext> EncryptFileUniqueWithBackupAndWipeMock { get; set; }

        public override void EncryptFileUniqueWithBackupAndWipe(IRuntimeFileInfo fileInfo, AesKey encryptionKey, IProgressContext progress)
        {
            EncryptFileUniqueWithBackupAndWipeMock(fileInfo, encryptionKey, progress);
        }

        public Action<IRuntimeFileInfo, AesKey, IProgressContext> DecryptFilesUniqueWithWipeOfOriginalMock { get; set; }

        public override void DecryptFilesInFolderUniqueWithWipeOfOriginal(IRuntimeFileInfo fileInfo, AesKey decryptionKey, IProgressContext progress)
        {
            DecryptFilesUniqueWithWipeOfOriginalMock(fileInfo, decryptionKey, progress);
        }
    }
}