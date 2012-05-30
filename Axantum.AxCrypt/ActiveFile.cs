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
        private static readonly DateTime UnknownTimeMarker = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public ActiveFile(string encryptedPath, string decryptedPath, DateTime lastWriteTimeUtc, AesKey key, ActiveFileStatus status, Process process)
        {
            _encryptedFileInfo = new FileInfo(encryptedPath);
            EncryptedPath = _encryptedFileInfo.FullName;
            _decryptedFileInfo = new FileInfo(decryptedPath);
            decryptedPath = _decryptedFileInfo.FullName;
            _decryptedFolder = Path.GetDirectoryName(decryptedPath);
            _protectedDecryptedName = Path.GetFileName(decryptedPath);
            Key = key;
            if (key != null)
            {
                AesKeyThumbprint thumbprint = new AesKeyThumbprint(key, KeyThumbprintSalt);
                KeyThumbprintBytes = thumbprint.GetThumbprintBytes();
            }
            Status = status;
            LastAccessTimeUtc = DateTime.UtcNow;
            Process = process;
            if (lastWriteTimeUtc < UnknownTimeMarker)
            {
                lastWriteTimeUtc = _decryptedFileInfo.LastWriteTimeUtc;
            }
            LastWriteTimeUtc = lastWriteTimeUtc;
        }

        public ActiveFile(string encryptedPath, string decryptedPath, AesKey key, ActiveFileStatus status, Process process)
            : this(encryptedPath, decryptedPath, DateTime.MinValue, key, status, process)
        {
        }

        public ActiveFile(ActiveFile activeFile, ActiveFileStatus status, AesKey key, Process process)
            : this(activeFile.EncryptedPath, activeFile.DecryptedPath, activeFile.LastWriteTimeUtc, key, status, process)
        {
            if (process != null && Object.ReferenceEquals(process, activeFile.Process))
            {
                activeFile.Process = null;
            }
            _keyThumbprintBytes = activeFile._keyThumbprintBytes;
            _keyThumbprintSalt = activeFile._keyThumbprintSalt;
        }

        public ActiveFile(ActiveFile activeFile, ActiveFileStatus status, Process process)
            : this(activeFile, status, activeFile.Key, process)
        {
        }

        public ActiveFile(ActiveFile activeFile, ActiveFileStatus status)
            : this(activeFile, status, activeFile.Process)
        {
        }

        public ActiveFile(ActiveFile activeFile, AesKey key)
            : this(activeFile, activeFile.Status, key, activeFile.Process)
        {
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

        private byte[] _keyThumbprintSalt = AxCryptEnvironment.Current.GetRandomBytes(32);

        [DataMember]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is a protected method, only used for serialization where an array in this case is appropriate.")]
        protected byte[] KeyThumbprintSalt
        {
            get { return _keyThumbprintSalt; }
            set { _keyThumbprintSalt = value; }
        }

        private byte[] _keyThumbprintBytes;

        [DataMember]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is a protected method, only used for serialization where an array in this case is appropriate.")]
        protected byte[] KeyThumbprintBytes
        {
            get { return _keyThumbprintBytes; }
            set { _keyThumbprintBytes = value; }
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

        public AesKey Key
        {
            get;
            set;
        }

        public bool ThumbprintMatch(AesKey key)
        {
            AesKeyThumbprint thumbprint = new AesKeyThumbprint(key, KeyThumbprintSalt);

            bool match = thumbprint.GetThumbprintBytes().IsEquivalentTo(KeyThumbprintBytes);
            return match;
        }

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