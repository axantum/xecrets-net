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
        }

        public IEnumerable<EmailAddress> SharedKeyHolders { get; private set; }

        public static EncryptedProperties Create(IDataStore dataStore)
        {
            using (Stream stream = LockedStream.OpenRead(dataStore))
            {
                return Create(stream);
            }
        }

        public static EncryptedProperties Create(Stream stream)
        {
            EncryptedProperties properties = new EncryptedProperties();
            if (!Resolve.KnownIdentities.IsLoggedOn)
            {
                return properties;
            }

            LogOnIdentity logOnIdentity = Resolve.KnownIdentities.DefaultEncryptionIdentity;
            IEnumerable<DecryptionParameter> decryptionParameters = DecryptionParameter.CreateAll(new Passphrase[] { logOnIdentity.Passphrase }, logOnIdentity.PrivateKeys, Resolve.CryptoFactory.OrderedIds);

            using (IAxCryptDocument document = TypeMap.Resolve.New<AxCryptFactory>().CreateDocument(decryptionParameters, stream))
            {
                if (!document.PassphraseIsValid)
                {
                    return properties;
                }

                properties.SharedKeyHolders = document.AsymmetricRecipients;
            }

            return properties;
        }
    }
}