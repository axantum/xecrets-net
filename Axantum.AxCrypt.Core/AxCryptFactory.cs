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
        public virtual Guid TryFindCryptoId(Passphrase passphrase, IDataStore encryptedFileInfo, IEnumerable<Guid> cryptoIds)
        {
            foreach (Guid cryptoId in cryptoIds)
            {
                if (TryOneCryptoId(passphrase, encryptedFileInfo, cryptoId))
                {
                    return cryptoId;
                }
            }
            return Guid.Empty;
        }

        private static bool TryOneCryptoId(Passphrase passphrase, IDataStore encryptedFileInfo, Guid cryptoId)
        {
            using (Stream encryptedStream = encryptedFileInfo.OpenRead())
            {
                Headers headers = new Headers();
                AxCryptReader reader = headers.Load(encryptedStream);

                using (IAxCryptDocument document = reader.Document(passphrase, cryptoId, headers))
                {
                    if (document.PassphraseIsValid)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual IAxCryptDocument CreateDocument(EncryptionParameters encryptionParameters)
        {
            long keyWrapIterations = Resolve.UserSettings.GetKeyWrapIterations(encryptionParameters.CryptoId);
            if (encryptionParameters.CryptoId == V1Aes128CryptoFactory.CryptoId)
            {
                return new V1AxCryptDocument(encryptionParameters.Passphrase, keyWrapIterations);
            }
            return new V2AxCryptDocument(encryptionParameters, keyWrapIterations);
        }

        /// <summary>
        /// Instantiate an instance of IAxCryptDocument appropriate for the file provided, i.e. V1 or V2.
        /// </summary>
        /// <param name="decryptionParameters">The possible decryption parameters to try.</param>
        /// <param name="inputStream">The input stream.</param>
        /// <returns></returns>
        public virtual IAxCryptDocument CreateDocument(DecryptionParameters decryptionParameters, Stream inputStream)
        {
            Headers headers = new Headers();
            AxCryptReader reader = headers.Load(inputStream);

            IAxCryptDocument document = null;
            foreach (Guid cryptoId in decryptionParameters.CryptoIds)
            {
                document = reader.Document(decryptionParameters.Passphrase, cryptoId, headers);
                if (document.PassphraseIsValid)
                {
                    return document;
                }
            }
            return document;
        }
    }
}