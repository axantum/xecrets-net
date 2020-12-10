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
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Session
{
    public class EncryptedProperties
    {
        public EncryptedProperties(string fileName)
        {
            FileMetaData = new FileMetaData(fileName);
        }

        private EncryptedProperties()
        {
        }

        private static readonly EncryptedProperties Invalid = new EncryptedProperties();

        public bool IsValid { get; set; }

        public IEnumerable<UserPublicKey> SharedKeyHolders { get; private set; } = new UserPublicKey[0];

        public FileMetaData FileMetaData { get; private set; }

        public DecryptionParameter DecryptionParameter { get; set; }

        public IAsymmetricPublicKey MasterPublicKey { get; set; }

        /// <summary>
        /// Factory method to instantiate an EncryptedProperties instance. It is required and assumed that the
        /// currently logged on user has the required keys to decrypt the file.
        /// </summary>
        /// <param name="encrypted">The data store to instantiate from.</param>
        /// <returns>The properties or an empty set if the data store does not exist, or no-one is logged on.</returns>
        public static EncryptedProperties Create(IDataStore encrypted)
        {
            return Create(encrypted, Resolve.KnownIdentities.DefaultEncryptionIdentity);
        }

        public static EncryptedProperties Create(IDataStore encrypted, LogOnIdentity identity)
        {
            if (encrypted == null)
            {
                throw new ArgumentNullException("encrypted");
            }

            try
            {
                using (Stream stream = encrypted.OpenRead())
                {
                    return Create(stream, identity);
                }
            }
            catch (FileNotFoundException fnfex)
            {
                New<IReport>().Exception(fnfex);
                return Invalid;
            }
        }

        public static EncryptedProperties Create(Stream stream, LogOnIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            if (identity == LogOnIdentity.Empty)
            {
                return Invalid;
            }

            EncryptedProperties properties = new EncryptedProperties(null);
            using (IAxCryptDocument document = New<AxCryptFactory>().CreateDocument(identity.DecryptionParameters(), stream))
            {
                if (!document.PassphraseIsValid)
                {
                    return Invalid;
                }

                properties = new EncryptedProperties(document.FileName);
                properties.SharedKeyHolders = document.AsymmetricRecipients;
                properties.MasterPublicKey = document.AsymmetricMasterKey;
                properties.DecryptionParameter = document.DecryptionParameter;
                properties.FileMetaData.CreationTimeUtc = document.CreationTimeUtc;
                properties.FileMetaData.LastWriteTimeUtc = document.LastWriteTimeUtc;
                properties.FileMetaData.LastAccessTimeUtc = document.LastAccessTimeUtc;
                properties.IsValid = true;
            }

            return properties;
        }

        public static EncryptedProperties Create(IAxCryptDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            if (!document.PassphraseIsValid)
            {
                return Invalid;
            }

            EncryptedProperties properties = new EncryptedProperties(document.FileName);
            properties.DecryptionParameter = document.DecryptionParameter;
            properties.IsValid = document.PassphraseIsValid;
            properties.FileMetaData.CreationTimeUtc = document.CreationTimeUtc;
            properties.FileMetaData.LastAccessTimeUtc = document.LastAccessTimeUtc;
            properties.FileMetaData.LastWriteTimeUtc = document.LastWriteTimeUtc;

            return properties;
        }
    }
}