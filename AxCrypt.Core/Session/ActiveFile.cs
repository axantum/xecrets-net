#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.IO;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Session
{
    /// <summary>
    /// This class represent an active source files' current known state. Instances of this class are
    /// immutable.
    /// </summary>
    ///
    public sealed class ActiveFile : IJsonOnDeserialized
    {
        public ActiveFile()
        {
        }

        public ActiveFile(ActiveFile activeFile)
        {
            if (activeFile == null)
            {
                throw new ArgumentNullException(nameof(activeFile));
            }
            Initialize(activeFile);
            Properties = new ActiveFileProperties(activeFile.Properties.LastActivityTimeUtc, Properties.LastEncryptionWriteTimeUtc, activeFile.Properties.CryptoId);
            Identity = LogOnIdentity.Empty;
        }

        public ActiveFile(ActiveFile activeFile, LogOnIdentity decryptIdentity)
        {
            if (activeFile == null)
            {
                throw new ArgumentNullException(nameof(activeFile));
            }
            if (decryptIdentity == null)
            {
                throw new ArgumentNullException(nameof(decryptIdentity));
            }

            Initialize(activeFile, decryptIdentity);
            Properties = new ActiveFileProperties(activeFile.Properties.LastActivityTimeUtc, Properties.LastEncryptionWriteTimeUtc, activeFile.Properties.CryptoId);
        }

        public ActiveFile(ActiveFile activeFile, IDataStore encryptedFileInfo)
        {
            if (activeFile == null)
            {
                throw new ArgumentNullException(nameof(activeFile));
            }

            Initialize(activeFile);
            EncryptedFileInfo = encryptedFileInfo ?? throw new ArgumentNullException(nameof(encryptedFileInfo));
        }

        public ActiveFile(ActiveFile activeFile, ActiveFileStatus status)
        {
            if (activeFile == null)
            {
                throw new ArgumentNullException(nameof(activeFile));
            }

            Initialize(activeFile);
            Status = status;
        }

        public ActiveFile(ActiveFile activeFile, Guid cryptoId)
        {
            if (activeFile == null)
            {
                throw new ArgumentNullException(nameof(activeFile));
            }

            Initialize(activeFile);
            Properties = new ActiveFileProperties(activeFile.Properties.LastActivityTimeUtc, activeFile.Properties.LastEncryptionWriteTimeUtc, cryptoId);
        }

        public ActiveFile(ActiveFile activeFile, Guid cryptoId, LogOnIdentity identity)
        {
            if (activeFile == null)
            {
                throw new ArgumentNullException(nameof(activeFile));
            }

            Initialize(activeFile, identity);
            Properties = new ActiveFileProperties(activeFile.Properties.LastActivityTimeUtc, activeFile.Properties.LastEncryptionWriteTimeUtc, cryptoId);
        }

        public ActiveFile(ActiveFile activeFile, DateTime lastEncryptionWriteTimeUtc, ActiveFileStatus status)
        {
            if (activeFile == null)
            {
                throw new ArgumentNullException(nameof(activeFile));
            }
            Initialize(activeFile);
            Properties = new ActiveFileProperties(activeFile.Properties.LastActivityTimeUtc, lastEncryptionWriteTimeUtc, activeFile.Properties.CryptoId);
            Status = status;
        }

        public ActiveFile(IDataStore encryptedFileInfo, IDataStore decryptedFileInfo, LogOnIdentity key, ActiveFileStatus status, Guid cryptoId)
        {
            if (encryptedFileInfo == null)
            {
                throw new ArgumentNullException(nameof(encryptedFileInfo));
            }
            if (decryptedFileInfo == null)
            {
                throw new ArgumentNullException(nameof(decryptedFileInfo));
            }
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            Initialize(encryptedFileInfo, decryptedFileInfo, key, null, status, new ActiveFileProperties(New<INow>().Utc, encryptedFileInfo.LastWriteTimeUtc, cryptoId));
        }

        private void Initialize(ActiveFile other, LogOnIdentity identity)
        {
            Initialize(other.EncryptedFileInfo, other.DecryptedFileInfo, identity, other.Thumbprint, other.Status, other.Properties);
        }

        private void Initialize(ActiveFile other)
        {
            Initialize(other.EncryptedFileInfo, other.DecryptedFileInfo, other.Identity, other.Thumbprint, other.Status, other.Properties);
        }

        private void Initialize(IDataStore encryptedFileInfo, IDataStore decryptedFileInfo, LogOnIdentity identity, SymmetricKeyThumbprint? thumbprint, ActiveFileStatus status, ActiveFileProperties properties)
        {
            EncryptedFileInfo = New<IDataStore>(encryptedFileInfo.FullName);
            DecryptedFileInfo = New<IDataStore>(decryptedFileInfo.FullName);
            Identity = identity;
            Thumbprint = thumbprint;
            Status = status;
            Properties = new ActiveFileProperties(New<INow>().Utc, properties.LastEncryptionWriteTimeUtc, properties.CryptoId);

            IAxCryptDocument document = EncryptedFileInfo.GetAxCryptDocument(Identity);
            IsShared = document.IsKeyShared();
            IsMasterKeyShared = document.IsMasterKeyShared();
        }

        [AllowNull]
        [JsonIgnore]
        public IDataStore DecryptedFileInfo
        {
            get;
            private set;
        }

        [AllowNull]
        [JsonIgnore]
        public IDataStore EncryptedFileInfo
        {
            get;
            private set;
        }

        private SymmetricKeyThumbprint? _thumbprint;

        [JsonPropertyName("thumbprint")]
        public SymmetricKeyThumbprint? Thumbprint
        {
            get
            {
                if (_thumbprint == null && Identity != LogOnIdentity.Empty)
                {
                    _thumbprint = Identity.Passphrase.Thumbprint;
                }
                return _thumbprint;
            }
            set
            {
                _thumbprint = value;
            }
        }

        [AllowNull]
        private string _decryptedFolder;

        [JsonPropertyName("decryptedFolder")]
        public string DecryptedFolder
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

        [AllowNull]
        private string _decryptedName;

        [AllowNull]
        private byte[] _protectedName;

        [JsonPropertyName("protectedDecryptedName")]
        public byte[] ProtectedDecryptedName
        {
            get
            {
                _protectedName ??= New<IProtectedData>().Protect(Encoding.UTF8.GetBytes(Resolve.Portable.Path().GetFileName(DecryptedFileInfo.FullName)), null);
                return _protectedName;
            }
            set
            {
                byte[]? bytes = New<IProtectedData>().Unprotect(value, null);
                _decryptedName = Encoding.UTF8.GetString(bytes!, 0, bytes!.Length);
                _protectedName = (byte[])value.Clone();
            }
        }

        [JsonPropertyName("encryptedPath")]
        public string EncryptedPath
        {
            get
            {
                return EncryptedFileInfo.FullName;
            }
            set
            {
                EncryptedFileInfo = New<IDataStore>(value);
            }
        }

        [JsonPropertyName("status")]
        public ActiveFileStatus Status { get; set; }

        [JsonPropertyName("properties")]
        [AllowNull]
        public ActiveFileProperties Properties { get; set; }

        [JsonIgnore]
        public bool IsShared { get; private set; }

        [JsonIgnore]
        public bool IsMasterKeyShared { get; private set; }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            OnDeserialized();
        }

        public void OnDeserialized()
        {
            DecryptedFileInfo = New<IDataStore>(Resolve.Portable.Path().Combine(_decryptedFolder, _decryptedName));
            if (Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted))
            {
                Status |= ActiveFileStatus.NoProcessKnown;
            }
        }

        private LogOnIdentity _identity = LogOnIdentity.Empty;

        [JsonIgnore]
        public LogOnIdentity Identity
        {
            get
            {
                return _identity;
            }
            private set
            {
                _identity = value ?? throw new ArgumentNullException(nameof(value));
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
                throw new ArgumentNullException(nameof(key));
            }

            return key.Thumbprint == Thumbprint;
        }

        [JsonIgnore]
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

        [JsonIgnore]
        public ActiveFileVisualStates VisualState
        {
            get
            {
                ActiveFileVisualStates visualState = DetermineEncryptionState();
                if (Properties.CryptoId != Resolve.CryptoFactory.Preferred.CryptoId)
                {
                    visualState |= ActiveFileVisualStates.LowEncryption;
                }
                return visualState;
            }
        }

        [JsonIgnore]
        public bool IsDecrypted
        {
            get
            {
                if (Status.HasMask(ActiveFileStatus.DecryptedIsPendingDelete))
                {
                    return true;
                }
                if (Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted))
                {
                    return true;
                }

                return false;
            }
        }

        private ActiveFileVisualStates DetermineEncryptionState()
        {
            if (Status.HasMask(ActiveFileStatus.DecryptedIsPendingDelete))
            {
                return Identity != LogOnIdentity.Empty ? ActiveFileVisualStates.DecryptedWithKnownKey : ActiveFileVisualStates.DecryptedWithoutKnownKey;
            }
            if (Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted))
            {
                return Identity != LogOnIdentity.Empty ? ActiveFileVisualStates.DecryptedWithKnownKey : ActiveFileVisualStates.DecryptedWithoutKnownKey;
            }
            if (Status.HasMask(ActiveFileStatus.NotDecrypted))
            {
                return Identity != LogOnIdentity.Empty ? ActiveFileVisualStates.EncryptedWithKnownKey : ActiveFileVisualStates.EncryptedWithoutKnownKey;
            }
            throw new InvalidOperationException("ActiveFile in an unhandled visual state.".InvariantFormat());
        }
    }
}
