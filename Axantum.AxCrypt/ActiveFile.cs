using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt
{
    public class ActiveFile
    {
        public ActiveFile(string encryptedPath, string decryptedPath, DateTime lastWriteTimeUtc)
        {
            EncryptedPath = encryptedPath;
            DecryptedPath = decryptedPath;
            LastWriteTimeUtc = lastWriteTimeUtc;
        }

        public string DecryptedPath { get; private set; }

        public string EncryptedPath { get; private set; }

        public DateTime LastWriteTimeUtc { get; private set; }
    }
}