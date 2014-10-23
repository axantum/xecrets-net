#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace Axantum.AxCrypt.Core.Session
{
    /// <summary>
    /// This class represent an active source files' current known state. Instances of this class are
    /// immutable.
    /// </summary>
    ///
    [DataContract(Namespace = "http://www.axantum.com/Serialization/")]
    public sealed class ActiveFile
    {
        public ActiveFile(ActiveFile activeFile)
        {
            if (activeFile == null)
            {
                throw new ArgumentNullException("activeFile");
            }
            Initialize(activeFile);
            Properties = new ActiveFileProperties(activeFile.Properties.LastActivityTimeUtc, Properties.LastEncryptionWriteTimeUtc, activeFile.Properties.CryptoId);
            Key = null;
        }

        public ActiveFile(ActiveFile activeFile, Passphrase key)
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
            Properties = new ActiveFileProperties(activeFile.Properties.LastActivityTimeUtc, Properties.LastEncryptionWriteTimeUtc, activeFile.Properties.CryptoId);
            Key = key;
        }

        public ActiveFile(ActiveFile activeFile, IRuntimeFileInfo encryptedFileInfo)
        {
            if (activeFile == null)
            {
                throw new ArgumentNullException("activeFile");
            }
            if (encryptedFileInfo == null)
            {
                throw new ArgumentNullException("encryptedFileInfo");
            }

            Initialize(activeFile);
            EncryptedFileInfo = encryptedFileInfo;
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
            Properties = new ActiveFileProperties(activeFile.Properties.LastActivityTimeUtc, lastEncryptionWriteTimeUtc, activeFile.Properties.CryptoId);
            Status = status;
        }

        public ActiveFile(IRuntimeFileInfo encryptedFileInfo, IRuntimeFileInfo decryptedFileInfo, Passphrase key, ActiveFileStatus status, Guid cryptoId)
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
            Initialize(encryptedFileInfo, decryptedFileInfo, key, null, status, new ActiveFileProperties(OS.Current.UtcNow, encryptedFileInfo.LastWriteTimeUtc, cryptoId));
        }

        private void Initialize(ActiveFile other)
        {
            Initialize(other.EncryptedFileInfo, other.DecryptedFileInfo, other.Key, other.Thumbprint, other.Status, other.Properties);
        }

        private void Initialize(IRuntimeFileInfo encryptedFileInfo, IRuntimeFileInfo decryptedFileInfo, Passphrase key, SymmetricKeyThumbprint thumbprint, ActiveFileStatus status, ActiveFileProperties properties)
        {
            EncryptedFileInfo = TypeMap.Resolve.New<IRuntimeFileInfo>(encryptedFileInfo.FullName);
            DecryptedFileInfo = TypeMap.Resolve.New<IRuntimeFileInfo>(decryptedFileInfo.FullName);
            Key = key;
            Thumbprint = thumbprint;
            Status = status;
            Properties = new ActiveFileProperties(OS.Current.UtcNow, properties.LastEncryptionWriteTimeUtc, properties.CryptoId);
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

        private SymmetricKeyThumbprint _thumbprint;

        [DataMember(Name = "Thumbprint")]
        public SymmetricKeyThumbprint Thumbprint
        {
            get
            {
                if (_thumbprint == null && Key != null)
                {
                    _thumbprint = Key.Thumbprint;
                }
                return _thumbprint;
            }
            private set
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
                return Resolve.Portable.Path().GetDirectoryName(DecryptedFileInfo.FullName);
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
                return TypeMap.Resolve.New<IDataProtection>().Protect(Encoding.UTF8.GetBytes(Resolve.Portable.Path().GetFileName(DecryptedFileInfo.FullName)));
            }
            set
            {
                byte[] bytes = TypeMap.Resolve.New<IDataProtection>().Unprotect(value);
                _decryptedName = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
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
                EncryptedFileInfo = TypeMap.Resolve.New<IRuntimeFileInfo>(value);
            }
        }

        [DataMember]
        public ActiveFileStatus Status { get; private set; }

        [DataMember]
        public ActiveFileProperties Properties { get; private set; }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            DecryptedFileInfo = TypeMap.Resolve.New<IRuntimeFileInfo>(Resolve.Portable.Path().Combine(_decryptedFolder, _decryptedName));
            if (Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted))
            {
                Status |= ActiveFileStatus.NoProcessKnown;
            }
        }

        private Passphrase _key;

        public Passphrase Key
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
        public bool ThumbprintMatch(Passphrase key)
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
                if (!DecryptedFileInfo.IsAvailable)
                {
                    return false;
                }
                bool isModified = DecryptedFileInfo.LastWriteTimeUtc > Properties.LastEncryptionWriteTimeUtc;
                if (Resolve.Log.IsInfoEnabled)
                {
                    Resolve.Log.LogInfo("IsModified == '{0}' for file '{3}' info last write time '{1}' and active file last write time '{2}'".InvariantFormat(isModified.ToString(), DecryptedFileInfo.LastWriteTimeUtc.ToString(), Properties.LastEncryptionWriteTimeUtc.ToString(), DecryptedFileInfo.Name));
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
                    return Key != null ? ActiveFileVisualState.EncryptedWithKnownKey : ActiveFileVisualState.EncryptedWithoutKnownKey;
                }
                throw new InvalidOperationException("ActiveFile in an unhandled visual state.".InvariantFormat());
            }
        }
    }
}