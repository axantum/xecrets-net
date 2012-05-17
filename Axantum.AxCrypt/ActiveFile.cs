using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Axantum.AxCrypt
{
    /// <summary>
    /// This class represent an active source files' current known state. Instances of this class are
    /// immutable. Instances of this class are considered equal on basis of equivalence of the
    /// path of the encrypted source file.
    /// </summary>
    ///
    [DataContract(Namespace = "http://www.axantum.com/Serialization/")]
    public class ActiveFile
    {
        public ActiveFile(string encryptedPath, string decryptedPath, ActiveFileStatus status)
        {
            EncryptedPath = Path.GetFullPath(encryptedPath);
            DecryptedPath = Path.GetFullPath(decryptedPath);
            FileInfo decryptedFileInfo = new FileInfo(decryptedPath);
            LastWriteTimeUtc = decryptedFileInfo.LastWriteTimeUtc;
            Status = status;
            LastAccessTimeUtc = DateTime.UtcNow;
        }

        [IgnoreDataMember]
        public string DecryptedPath { get; private set; }

        [DataMember]
        public string EncryptedPath { get; private set; }

        [DataMember]
        public DateTime LastWriteTimeUtc { get; private set; }

        [DataMember]
        public ActiveFileStatus Status { get; private set; }

        [DataMember]
        public DateTime LastAccessTimeUtc { get; private set; }
    }
}