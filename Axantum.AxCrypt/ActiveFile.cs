using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt
{
    /// <summary>
    /// This class represent an active source files current state. Instances of this class are
    /// immutable. Instances of this class are considered equal on basis of equivalence of the
    /// path of the encrypted source file.
    /// </summary>
    public class ActiveFile
    {
        public ActiveFile(string encryptedPath, string decryptedPath, DateTime lastWriteTimeUtc)
        {
            EncryptedPath = Path.GetFullPath(encryptedPath);
            DecryptedPath = Path.GetFullPath(decryptedPath);
            LastWriteTimeUtc = lastWriteTimeUtc;
        }

        public ActiveFile(string encryptedPath)
        {
            EncryptedPath = Path.GetFullPath(encryptedPath);
            DecryptedPath = String.Empty;
            LastWriteTimeUtc = DateTime.MinValue;
        }

        public string DecryptedPath { get; private set; }

        public string EncryptedPath { get; private set; }

        public DateTime LastWriteTimeUtc { get; private set; }

        public override bool Equals(object obj)
        {
            ActiveFile other = obj as ActiveFile;
            if (other == null)
            {
                return false;
            }
            return EncryptedPath.Equals(other.EncryptedPath);
        }

        public override int GetHashCode()
        {
            return EncryptedPath.GetHashCode();
        }
    }
}