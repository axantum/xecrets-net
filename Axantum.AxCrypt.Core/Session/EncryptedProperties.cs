#region Coypright and License

/*
 * AxCrypt - Copyright 2015, Svante Seleborg, All Rights Reserved
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
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Session
{
    public class EncryptedProperties
    {
        private EncryptedProperties()
        {
            SharedKeyHolders = new UserPublicKey[0];

            DateTime utcNow = DateTime.UtcNow;
            CreationTimeUtc = utcNow;
            LastAccessTimeUtc = utcNow;
            LastWriteTimeUtc = utcNow;
        }

        public EncryptedProperties(string fileName)
            : this()
        {
            FileName = fileName;
        }

        public bool IsValid { get; set; }

        public IEnumerable<UserPublicKey> SharedKeyHolders { get; private set; }

        public string FileName { get; private set; }

        public DecryptionParameter DecryptionParameter { get; set; }

        public DateTime CreationTimeUtc { get; set; }

        public DateTime LastAccessTimeUtc { get; set; }

        public DateTime LastWriteTimeUtc { get; set; }

        /// <summary>
        /// Factory method to instantiate an EncryptedProperties instance. It is required and assumed that the
        /// currently logged on user has the required keys to decrypt the file.
        /// </summary>
        /// <param name="dataStore">The data store to instantiate from.</param>
        /// <returns>The properties or an empty set if the data store does not exist, or no-one is logged on.</returns>
        public static EncryptedProperties Create(IDataStore dataStore)
        {
            return Create(dataStore, Resolve.KnownIdentities.DefaultEncryptionIdentity);
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
            catch (FileNotFoundException)
            {
                return new EncryptedProperties();
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
                return new EncryptedProperties();
            }

            EncryptedProperties properties = new EncryptedProperties();
            IEnumerable<DecryptionParameter> decryptionParameters = DecryptionParameter.CreateAll(new Passphrase[] { identity.Passphrase }, identity.PrivateKeys, Resolve.CryptoFactory.OrderedIds);

            using (IAxCryptDocument document = TypeMap.Resolve.New<AxCryptFactory>().CreateDocument(decryptionParameters, stream))
            {
                if (!document.PassphraseIsValid)
                {
                    return properties;
                }

                properties.SharedKeyHolders = document.AsymmetricRecipients;
                properties.FileName = document.FileName;
                properties.DecryptionParameter = document.DecryptionParameter;
                properties.CreationTimeUtc = document.CreationTimeUtc;
                properties.LastWriteTimeUtc = document.LastWriteTimeUtc;
                properties.LastAccessTimeUtc = document.LastAccessTimeUtc;
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

            EncryptedProperties properties = new EncryptedProperties();

            if (!document.PassphraseIsValid)
            {
                return properties;
            }

            properties.DecryptionParameter = document.DecryptionParameter;
            properties.FileName = document.FileName;
            properties.IsValid = document.PassphraseIsValid;
            properties.CreationTimeUtc = document.CreationTimeUtc;
            properties.LastAccessTimeUtc = document.LastAccessTimeUtc;
            properties.LastWriteTimeUtc = document.LastWriteTimeUtc;

            return properties;
        }
    }
}