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
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.Core
{
    public class AxCryptFactory
    {
        public virtual IPassphrase CreatePassphrase(string passphrase, CryptoId cryptoId)
        {
            switch (cryptoId)
            {
                case CryptoId.Aes_256:
                    return new V2Passphrase(passphrase, 256);

                case CryptoId.Aes_128_V1:
                    return new V1Passphrase(passphrase);
            }
            throw new ArgumentException("Invalid CryptoId in parameter 'cryptoId'.");
        }

        public virtual IPassphrase CreatePassphrase(string passphrase, string encryptedFileFullName)
        {
            IPassphrase key = CreatePassphrase(passphrase, Factory.New<IRuntimeFileInfo>(encryptedFileFullName));
            return key;
        }

        public virtual IPassphrase CreatePassphrase(string passphrase, IRuntimeFileInfo encryptedFileInfo)
        {
            using (Stream encryptedStream = encryptedFileInfo.OpenRead())
            {
                Headers headers = new Headers();
                AxCryptReader reader = headers.Load(encryptedStream);

                IPassphrase key = reader.Crypto(headers, passphrase).Key;
                using (IAxCryptDocument document = reader.Document(key, headers))
                {
                    if (document.PassphraseIsValid)
                    {
                        return key;
                    }
                }
            }
            return null;
        }

        public virtual IAxCryptDocument CreateDocument(IPassphrase key)
        {
            IAxCryptDocument document;
            switch (key.CryptoId)
            {
                case CryptoId.Aes_128_V1:
                    document = new V1AxCryptDocument(Instance.CryptoFactory.Legacy.CreateCrypto(key, SymmetricIV.Zero128, 0), Instance.UserSettings.V1KeyWrapIterations);
                    break;

                case CryptoId.Aes_256:
                    document = new V2AxCryptDocument(Instance.CryptoFactory.Default.CreateCrypto(key, SymmetricIV.Zero128, 0), Instance.UserSettings.V2KeyWrapIterations);
                    break;

                default:
                    throw new ArgumentException("Invalid CryptoId in key.");
            }
            return document;
        }

        /// <summary>
        /// Instantiate an instance of IAxCryptDocument appropriate for the file provided, i.e. V1 or V2.
        /// </summary>
        /// <param name="passphrase">The passphrase.</param>
        /// <param name="fileInfo">The file to use.</param>
        /// <returns></returns>
        public virtual IAxCryptDocument CreateDocument(string passphrase, Stream inputStream)
        {
            Headers headers = new Headers();
            AxCryptReader reader = headers.Load(inputStream);

            IPassphrase key = reader.Crypto(headers, passphrase).Key;
            return reader.Document(key, headers);
        }

        /// <summary>
        /// Instantiate an instance of IAxCryptDocument appropriate for the file provided, i.e. V1 or V2.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public virtual IAxCryptDocument CreateDocument(IPassphrase key, Stream inputStream)
        {
            Headers headers = new Headers();
            AxCryptReader reader = headers.Load(inputStream);

            return reader.Document(key, headers);
        }
    }
}