using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public Action<IRuntimeFileInfo, IRuntimeFileInfo, Passphrase, AxCryptOptions, ProgressContext> EncryptMock { get; set; }

        public override void Encrypt(IRuntimeFileInfo sourceFile, IRuntimeFileInfo destinationFile, Passphrase passphrase, AxCryptOptions options, ProgressContext progress)
        {
            EncryptMock(sourceFile, destinationFile, passphrase, options, progress);
        }

        public Action<IRuntimeFileInfo, AesKey, ProgressContext> EncryptFilesUniqueWithBackupAndWipeMock { get; set; }

        public override void EncryptFilesUniqueWithBackupAndWipe(IRuntimeFileInfo fileInfo, AesKey encryptionKey, ProgressContext progress)
        {
            EncryptFilesUniqueWithBackupAndWipeMock(fileInfo, encryptionKey, progress);
        }

        public Action<IRuntimeFileInfo, AesKey, ProgressContext> EncryptFileUniqueWithBackupAndWipeMock { get; set; }

        public override void EncryptFileUniqueWithBackupAndWipe(IRuntimeFileInfo fileInfo, AesKey encryptionKey, ProgressContext progress)
        {
            EncryptFileUniqueWithBackupAndWipeMock(fileInfo, encryptionKey, progress);
        }

        public Action<IRuntimeFileInfo, AesKey, ProgressContext> DecryptFilesUniqueWithWipeOfOriginalMock { get; set; }

        public override void DecryptFilesUniqueWithWipeOfOriginal(IRuntimeFileInfo fileInfo, AesKey decryptionKey, ProgressContext progress)
        {
            DecryptFilesUniqueWithWipeOfOriginalMock(fileInfo, decryptionKey, progress);
        }
    }
}