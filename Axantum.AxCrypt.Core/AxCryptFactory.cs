﻿#region Coypright and License

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
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Reader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.Core
{
    public class AxCryptFactory
    {
        public virtual IDerivedKey CreatePassphrase(Passphrase passphrase, IRuntimeFileInfo encryptedFileInfo, IEnumerable<Guid> cryptoIds)
        {
            foreach (Guid cryptoId in cryptoIds)
            {
                IDerivedKey key = CreatePassphraseInternal(passphrase, encryptedFileInfo, cryptoId);
                if (key != null)
                {
                    return key;
                }
            }
            return null;
        }

        private static IDerivedKey CreatePassphraseInternal(Passphrase passphrase, IRuntimeFileInfo encryptedFileInfo, Guid cryptoId)
        {
            using (Stream encryptedStream = encryptedFileInfo.OpenRead())
            {
                Headers headers = new Headers();
                AxCryptReader reader = headers.Load(encryptedStream);

                using (IAxCryptDocument document = reader.Document(passphrase, cryptoId, headers))
                {
                    if (document.PassphraseIsValid)
                    {
                        return document.KeyEncryptingCrypto.Key;
                    }
                }
            }
            return null;
        }

        public virtual IAxCryptDocument CreateDocument(Passphrase key, Guid cryptoId)
        {
            ICryptoFactory factory = Instance.CryptoFactory.Create(cryptoId);
            if (factory.Id == Instance.CryptoFactory.Legacy.Id)
            {
                return new V1AxCryptDocument(factory.CreateCrypto(key), Instance.UserSettings.GetKeyWrapIterations(CryptoFactory.Aes128V1Id));
            }
            return new V2AxCryptDocument(factory.CreateCrypto(key), Instance.UserSettings.GetKeyWrapIterations(cryptoId));
        }

        /// <summary>
        /// Instantiate an instance of IAxCryptDocument appropriate for the file provided, i.e. V1 or V2.
        /// </summary>
        /// <param name="passphrase">The passphrase.</param>
        /// <param name="fileInfo">The file to use.</param>
        /// <returns></returns>
        public virtual IAxCryptDocument CreateDocument(Passphrase passphrase, Stream inputStream)
        {
            Headers headers = new Headers();
            AxCryptReader reader = headers.Load(inputStream);

            IEnumerable<Guid> cryptoIds = Instance.CryptoFactory.OrderedIds;
            IAxCryptDocument document = null;
            foreach (Guid cryptoId in cryptoIds)
            {
                document = reader.Document(passphrase, cryptoId, headers);
                if (document.PassphraseIsValid)
                {
                    return document;
                }
            }
            return document;
        }

        /// <summary>
        /// Instantiate an instance of IAxCryptDocument appropriate for the file provided, i.e. V1 or V2.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public virtual IAxCryptDocument CreateDocument(Passphrase key, Guid cryptoId, Stream inputStream)
        {
            Headers headers = new Headers();
            AxCryptReader reader = headers.Load(inputStream);

            return reader.Document(key, cryptoId, headers);
        }
    }
}