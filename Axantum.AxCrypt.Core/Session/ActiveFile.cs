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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.System;

namespace Axantum.AxCrypt.Core.Session
{
    /// <summary>
    /// This class represent an active source files' current known state. Instances of this class are
    /// essentially immutable. Instances of this class are considered equal on basis of equivalence of the
    /// path of the encrypted source file.
    /// </summary>
    ///
    [DataContract(Namespace = "http://www.axantum.com/Serialization/")]
    public sealed class ActiveFile : IDisposable
    {
        public ActiveFile(ActiveFile activeFile, AesKey key)
        {
            if (activeFile == null)
            {
                throw new ArgumentNullException("activeFile");
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            Initialize(activeFile);
            Key = key;
        }

        public ActiveFile(ActiveFile activeFile, ActiveFileStatus status, Process process)
        {
            if (activeFile == null)
            {
                throw new ArgumentNullException("activeFile");
            }
            Initialize(activeFile);
            Status = status;
            Process = process;
        }

        public ActiveFile(ActiveFile activeFile, ActiveFileStatus status)
        {
            if (activeFile == null)
            {
                throw new ArgumentNullException("activeFile");
            }
            Initialize(activeFile);
            Status = status;
        }

        public ActiveFile(ActiveFile activeFile, DateTime lastWriteTimeUtc, ActiveFileStatus status)
        {
            if (activeFile == null)
            {
                throw new ArgumentNullException("activeFile");
            }
            Initialize(activeFile);
            LastWriteTimeUtc = lastWriteTimeUtc;
            Status = status;
        }

        public ActiveFile(IRuntimeFileInfo encryptedFileInfo, IRuntimeFileInfo decryptedFileInfo, AesKey key, ActiveFileStatus status, Process process)
        {
            if (encryptedFileInfo == null)
            {
                throw new ArgumentNullException("encryptedFileInfo");
            }
            if (decryptedFileInfo == null)
            {
                throw new ArgumentNullException("decryptedFileInfo");
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            Initialize(encryptedFileInfo, decryptedFileInfo, decryptedFileInfo.LastWriteTimeUtc, key, status, process);
        }

        private void Initialize(ActiveFile other)
        {
            Initialize(other.EncryptedFileInfo, other.DecryptedFileInfo, other.LastWriteTimeUtc, other.Key, other.Status, other.Process);
            if (other.Process != null)
            {
                other.Process = null;
            }
        }

        private void Initialize(IRuntimeFileInfo encryptedFileInfo, IRuntimeFileInfo decryptedFileInfo, DateTime lastWriteTimeUtc, AesKey key, ActiveFileStatus status, Process process)
        {
            EncryptedFileInfo = encryptedFileInfo;
            DecryptedFileInfo = decryptedFileInfo;
            Key = key;
            Status = status;
            LastAccessTimeUtc = DateTime.UtcNow;
            Process = process;
            LastWriteTimeUtc = lastWriteTimeUtc;
        }

        public IRuntimeFileInfo DecryptedFileInfo
        {
            get;
            private set;
        }

        public IRuntimeFileInfo EncryptedFileInfo
        {
            get;
            private set;
        }

        private byte[] _keyThumbprintSalt = AxCryptEnvironment.Current.GetRandomBytes(32);

        [DataMember]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a private method used for serialization.")]
        private byte[] KeyThumbprintSalt
        {
            get { return _keyThumbprintSalt; }
            set { _keyThumbprintSalt = value; }
        }

        private byte[] _keyThumbprintBytes;

        [DataMember]
        private byte[] KeyThumbprintBytes
        {
            get
            {
                if (_keyThumbprintBytes == null)
                {
                    AesKeyThumbprint thumbprint = new AesKeyThumbprint(Key, KeyThumbprintSalt);
                    _keyThumbprintBytes = thumbprint.GetThumbprintBytes();
                }

                return _keyThumbprintBytes;
            }
            set { _keyThumbprintBytes = value; }
        }

        string _decryptedFolder;

        [DataMember]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a private method used for serialization.")]
        private string DecryptedFolder
        {
            get
            {
                return Path.GetDirectoryName(DecryptedFileInfo.FullName);
            }
            set
            {
                _decryptedFolder = value;
            }
        }

        string _decryptedName;

        [DataMember]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a private method used for serialization.")]
        private byte[] ProtectedDecryptedName
        {
            get
            {
                return ProtectedData.Protect(Encoding.UTF8.GetBytes(Path.GetFileName(DecryptedFileInfo.FullName)), null, DataProtectionScope.CurrentUser);
            }
            set
            {
                byte[] bytes = ProtectedData.Unprotect(value, null, DataProtectionScope.CurrentUser);
                _decryptedName = Encoding.UTF8.GetString(bytes);
            }
        }

        [DataMember]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a private method used for serialization.")]
        private string EncryptedPath
        {
            get
            {
                return EncryptedFileInfo.FullName;
            }
            set
            {
                EncryptedFileInfo = AxCryptEnvironment.Current.FileInfo(value);
            }
        }

        [DataMember]
        public ActiveFileStatus Status { get; private set; }

        [DataMember]
        public DateTime LastAccessTimeUtc { get; private set; }

        [DataMember]
        public DateTime LastWriteTimeUtc { get; private set; }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            DecryptedFileInfo = AxCryptEnvironment.Current.FileInfo(Path.Combine(_decryptedFolder, _decryptedName));
        }

        public Process Process { get; private set; }

        private AesKey _key;

        public AesKey Key
        {
            get
            {
                return _key;
            }
            set
            {
                if (_key != null && !_key.Equals(value))
                {
                    KeyThumbprintBytes = null;
                }
                _key = value;
            }
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
                bool isModified = DecryptedFileInfo.LastWriteTimeUtc > LastWriteTimeUtc;
                if (Logging.IsInfoEnabled)
                {
                    Logging.Info("IsModified == '{0}' for file '{3}' info last write time '{1}' and active file last write time '{2}'".InvariantFormat(isModified.ToString(), DecryptedFileInfo.LastWriteTimeUtc.ToString(), LastWriteTimeUtc.ToString(), DecryptedFileInfo.Name));
                }
                return isModified;
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