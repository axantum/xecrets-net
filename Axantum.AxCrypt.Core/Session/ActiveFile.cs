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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace Axantum.AxCrypt.Core.Session
{
    /// <summary>
    /// This class represent an active source files' current known state. Instances of this class are
    /// essentially immutable. Essentially comes from the fact that the Process property can only
    /// be owned by one instance at a time, when a new instance is made from an existing instance,
    /// the existing Process property is set to null.
    /// </summary>
    ///
    [DataContract(Namespace = "http://www.axantum.com/Serialization/")]
    public sealed class ActiveFile : IDisposable
    {
        public ActiveFile(ActiveFile activeFile)
        {
            if (activeFile == null)
            {
                throw new ArgumentNullException("activeFile");
            }
            Initialize(activeFile);
            LastActivityTimeUtc = activeFile.LastActivityTimeUtc;
            Key = null;
        }

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
            LastActivityTimeUtc = activeFile.LastActivityTimeUtc;
            Key = key;
        }

        public ActiveFile(ActiveFile activeFile, ActiveFileStatus status, ILauncher process)
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

        public ActiveFile(ActiveFile activeFile, DateTime lastEncryptionWriteTimeUtc, ActiveFileStatus status)
        {
            if (activeFile == null)
            {
                throw new ArgumentNullException("activeFile");
            }
            Initialize(activeFile);
            LastEncryptionWriteTimeUtc = lastEncryptionWriteTimeUtc;
            Status = status;
        }

        public ActiveFile(IRuntimeFileInfo encryptedFileInfo, IRuntimeFileInfo decryptedFileInfo, AesKey key, ActiveFileStatus status, ILauncher process)
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
            Initialize(encryptedFileInfo, decryptedFileInfo, decryptedFileInfo.LastWriteTimeUtc, key, null, status, process);
        }

        private void Initialize(ActiveFile other)
        {
            Initialize(other.EncryptedFileInfo, other.DecryptedFileInfo, other.LastEncryptionWriteTimeUtc, other.Key, other.Thumbprint, other.Status, other.Process);
            if (other.Process != null)
            {
                other.Process = null;
            }
        }

        private void Initialize(IRuntimeFileInfo encryptedFileInfo, IRuntimeFileInfo decryptedFileInfo, DateTime lastWriteTimeUtc, AesKey key, AesKeyThumbprint thumbprint, ActiveFileStatus status, ILauncher process)
        {
            EncryptedFileInfo = OS.Current.FileInfo(encryptedFileInfo.FullName);
            DecryptedFileInfo = OS.Current.FileInfo(decryptedFileInfo.FullName);
            Key = key;
            Thumbprint = thumbprint;
            Status = status;
            LastActivityTimeUtc = OS.Current.UtcNow;
            Process = process;
            LastEncryptionWriteTimeUtc = lastWriteTimeUtc;
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

        private AesKeyThumbprint _thumbprint;

        [DataMember(Name = "Thumbprint")]
        private AesKeyThumbprint Thumbprint
        {
            get
            {
                if (_thumbprint == null && Key != null)
                {
                    _thumbprint = Key.Thumbprint;
                }
                return _thumbprint;
            }
            set
            {
                _thumbprint = value;
            }
        }

        private string _decryptedFolder;

        [DataMember]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a private property used for serialization.")]
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

        private string _decryptedName;

        [DataMember]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a private property used for serialization.")]
        private byte[] ProtectedDecryptedName
        {
            get
            {
                return OS.Current.DataProtection.Protect(Encoding.UTF8.GetBytes(Path.GetFileName(DecryptedFileInfo.FullName)));
            }
            set
            {
                byte[] bytes = OS.Current.DataProtection.Unprotect(value);
                _decryptedName = Encoding.UTF8.GetString(bytes);
            }
        }

        [DataMember]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a private property used for serialization.")]
        private string EncryptedPath
        {
            get
            {
                return EncryptedFileInfo.FullName;
            }
            set
            {
                EncryptedFileInfo = OS.Current.FileInfo(value);
            }
        }

        [DataMember]
        public ActiveFileStatus Status { get; private set; }

        [DataMember]
        public DateTime LastActivityTimeUtc { get; private set; }

        /// <summary>
        /// Records the Last Write Time that was valid at the most recent encryption update of the encrypted file.
        /// </summary>
        [DataMember]
        private DateTime LastEncryptionWriteTimeUtc { get; set; }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            DecryptedFileInfo = OS.Current.FileInfo(Path.Combine(_decryptedFolder, _decryptedName));
        }

        public ILauncher Process { get; private set; }

        private AesKey _key;

        public AesKey Key
        {
            get
            {
                return _key;
            }
            private set
            {
                _key = value;
            }
        }

        /// <summary>
        /// Check if a provided key matches the thumbprint of this instance.
        /// </summary>
        /// <param name="key">A key to check against this instances thumbprint.</param>
        /// <returns>true if the thumbprint matches the provided key.</returns>
        public bool ThumbprintMatch(AesKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            return key.Thumbprint == Thumbprint;
        }

        public bool IsModified
        {
            get
            {
                if (!DecryptedFileInfo.Exists)
                {
                    return false;
                }
                bool isModified = DecryptedFileInfo.LastWriteTimeUtc > LastEncryptionWriteTimeUtc;
                if (OS.Log.IsInfoEnabled)
                {
                    OS.Log.LogInfo("IsModified == '{0}' for file '{3}' info last write time '{1}' and active file last write time '{2}'".InvariantFormat(isModified.ToString(), DecryptedFileInfo.LastWriteTimeUtc.ToString(), LastEncryptionWriteTimeUtc.ToString(), DecryptedFileInfo.Name));
                }
                return isModified;
            }
        }

        public ActiveFileVisualState VisualState
        {
            get
            {
                if (Status.HasMask(ActiveFileStatus.DecryptedIsPendingDelete))
                {
                    return Key != null ? ActiveFileVisualState.DecryptedWithKnownKey : ActiveFileVisualState.DecryptedWithoutKnownKey;
                }
                if (Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted))
                {
                    return Key != null ? ActiveFileVisualState.DecryptedWithKnownKey : ActiveFileVisualState.DecryptedWithoutKnownKey;
                }
                if (Status.HasMask(ActiveFileStatus.NotDecrypted))
                {
                    if (String.IsNullOrEmpty(DecryptedFileInfo.FullName))
                    {
                        return ActiveFileVisualState.EncryptedNeverBeenDecrypted;
                    }
                    return Key != null ? ActiveFileVisualState.EncryptedWithKnownKey : ActiveFileVisualState.EncryptedWithoutKnownKey;
                }
                throw new InvalidOperationException("ActieFile in an unhandled visual state.");
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Process == null)
                {
                    return;
                }
                Process.Dispose();
                Process = null;
            }
        }

        #endregion IDisposable Members
    }
}