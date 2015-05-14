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
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
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
        public virtual DecryptionParameter FindDecryptionParameter(IEnumerable<DecryptionParameter> decryptionParameters, IDataStore encryptedFileInfo)
        {
            DecryptionParameter foundParameter;
            using (CreateDocument(decryptionParameters, encryptedFileInfo.OpenRead(), out foundParameter))
            {
            }
            return foundParameter;
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
        public virtual IAxCryptDocument CreateDocument(IEnumerable<DecryptionParameter> decryptionParameters, Stream inputStream)
        {
            DecryptionParameter foundParameter;
            return CreateDocument(decryptionParameters, inputStream, out foundParameter);
        }

        private static IAxCryptDocument CreateDocument(IEnumerable<DecryptionParameter> decryptionParameters, Stream inputStream, out DecryptionParameter foundParameter)
        {
            Headers headers = new Headers();
            AxCryptReaderBase reader = headers.CreateReader(new LookAheadStream(inputStream));

            IAxCryptDocument document = AxCryptReaderBase.Document(reader);
            foreach (DecryptionParameter decryptionParameter in decryptionParameters)
            {
                if (decryptionParameter.Passphrase != null)
                {
                    document.Load(decryptionParameter.Passphrase, decryptionParameter.CryptoId, headers);
                    if (document.PassphraseIsValid)
                    {
                        foundParameter = decryptionParameter;
                        return document;
                    }
                }
                if (decryptionParameter.PrivateKey != null)
                {
                    document.Load(decryptionParameter.PrivateKey, decryptionParameter.CryptoId, headers);
                    if (document.PassphraseIsValid)
                    {
                        foundParameter = decryptionParameter;
                        return document;
                    }
                }
            }
            foundParameter = null;
            return document;
        }
    }
}