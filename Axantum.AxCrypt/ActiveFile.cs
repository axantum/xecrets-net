#define CODE_ANALYSIS

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;

namespace Axantum.AxCrypt
{
    /// <summary>
    /// This class represent an active source files' current known state. Instances of this class are
    /// immutable. Instances of this class are considered equal on basis of equivalence of the
    /// path of the encrypted source file.
    /// </summary>
    ///
    [DataContract(Namespace = "http://www.axantum.com/Serialization/")]
    public class ActiveFile : IDisposable
    {
        public ActiveFile(string encryptedPath, string decryptedPath, DateTime lastWriteTimeUtc, AesKey key, ActiveFileStatus status, Process process)
        {
            _encryptedFileInfo = new FileInfo(encryptedPath);
            EncryptedPath = _encryptedFileInfo.FullName;
            _decryptedFileInfo = new FileInfo(decryptedPath);
            decryptedPath = _decryptedFileInfo.FullName;
            _decryptedFolder = Path.GetDirectoryName(decryptedPath);
            _protectedDecryptedName = Path.GetFileName(decryptedPath);
            Key = key;
            Status = status;
            LastAccessTimeUtc = DateTime.UtcNow;
            Process = process;
            if (lastWriteTimeUtc == DateTime.MinValue)
            {
                lastWriteTimeUtc = _decryptedFileInfo.LastWriteTimeUtc;
            }
            LastWriteTimeUtc = lastWriteTimeUtc;
        }

        public ActiveFile(string encryptedPath, string decryptedPath, AesKey key, ActiveFileStatus status, Process process)
            : this(encryptedPath, decryptedPath, DateTime.MinValue, key, status, process)
        {
        }

        public ActiveFile(ActiveFile activeFile, ActiveFileStatus status, Process process)
            : this(activeFile.EncryptedPath, activeFile.DecryptedPath, activeFile.LastWriteTimeUtc, activeFile.Key, status, process)
        {
            if (process != null && Object.ReferenceEquals(process, activeFile.Process))
            {
                activeFile.Process = null;
            }
        }

        public ActiveFile(ActiveFile destinationActiveFile, AesKey key)
            : this(destinationActiveFile, destinationActiveFile.Status, destinationActiveFile.Process)
        {
            Key = key;
        }

        private FileInfo _decryptedFileInfo;

        public FileInfo DecryptedFileInfo
        {
            get
            {
                if (_decryptedFileInfo == null)
                {
                    _decryptedFileInfo = new FileInfo(DecryptedPath);
                }
                return _decryptedFileInfo;
            }
        }

        private FileInfo _encryptedFileInfo;

        public FileInfo EncryptedFileInfo
        {
            get
            {
                if (_encryptedFileInfo == null)
                {
                    _encryptedFileInfo = new FileInfo(EncryptedPath);
                }
                return _encryptedFileInfo;
            }
        }

        public string DecryptedPath
        {
            get
            {
                return Path.Combine(_decryptedFolder, _protectedDecryptedName);
            }
        }

        string _protectedDecryptedName;

        [DataMember]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is a protected method, only used for serialization where an array in this case is appropriate.")]
        protected byte[] ProtectedDecryptedName
        {
            get
            {
                return ProtectedData.Protect(Encoding.UTF8.GetBytes(Path.GetFileName(DecryptedPath)), null, DataProtectionScope.CurrentUser);
            }
            set
            {
                byte[] bytes = ProtectedData.Unprotect(value, null, DataProtectionScope.CurrentUser);
                _protectedDecryptedName = Encoding.UTF8.GetString(bytes);
            }
        }

        string _decryptedFolder;

        [DataMember]
        protected string DecryptedFolder
        {
            get
            {
                return Path.GetDirectoryName(DecryptedPath);
            }
            set
            {
                _decryptedFolder = value;
            }
        }

        [DataMember]
        public string EncryptedPath { get; private set; }

        [DataMember]
        public ActiveFileStatus Status { get; private set; }

        [DataMember]
        public DateTime LastAccessTimeUtc { get; private set; }

        [DataMember]
        public DateTime LastWriteTimeUtc { get; private set; }

        public Process Process { get; private set; }

        public AesKey Key { get; private set; }

        public bool IsModified
        {
            get
            {
                if (!DecryptedFileInfo.Exists)
                {
                    return false;
                }
                return DecryptedFileInfo.LastWriteTimeUtc > LastWriteTimeUtc;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Even if we're not using the parameter, it is part of the IDisposable pattern.")]
        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }
            if (Process == null)
            {
                return;
            }
            Process.Dispose();
            Process = null;
        }

        #endregion IDisposable Members
    }
}