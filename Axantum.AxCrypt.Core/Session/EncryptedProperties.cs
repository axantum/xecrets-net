using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;
using System;

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
            SharedKeyHolders = new EmailAddress[0];

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

        public bool IsValid { get; private set; }

        public IEnumerable<EmailAddress> SharedKeyHolders { get; private set; }

        public string FileName { get; private set; }

        public DecryptionParameter DecryptionParameter { get; private set; }

        public DateTime CreationTimeUtc { get; set; }

        public DateTime LastAccessTimeUtc { get; set; }

        public DateTime LastWriteTimeUtc { get; set; }

        public static EncryptedProperties Create(IDataStore dataStore)
        {
            using (Stream stream = LockedStream.OpenRead(dataStore))
            {
                return Create(stream);
            }
        }

        public static EncryptedProperties Create(Stream stream)
        {
            if (!Resolve.KnownIdentities.IsLoggedOn)
            {
                return new EncryptedProperties();
            }

            return Create(stream, Resolve.KnownIdentities.DefaultEncryptionIdentity);
        }

        public static EncryptedProperties Create(IDataStore encrypted, LogOnIdentity identity)
        {
            using (Stream stream = encrypted.OpenRead())
            {
                return Create(stream, identity);
            }
        }

        public static EncryptedProperties Create(Stream stream, LogOnIdentity identity)
        {
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