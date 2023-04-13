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

using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.Header;
using AxCrypt.Core.IO;
using AxCrypt.Core.Reader;
using AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core
{
    public class AxCryptFactory
    {
        public virtual bool IsPassphraseValid(Passphrase passphrase, string encryptedFileFullName)
        {
            IDataStore encryptedStore = New<IDataStore>(encryptedFileFullName);
            IEnumerable<DecryptionParameter> parameters = encryptedStore.DecryptionParameters(passphrase, Array.Empty<IAsymmetricPrivateKey>());
            return New<AxCryptFactory>().FindDecryptionParameter(parameters, encryptedStore) != null;
        }

        public virtual DecryptionParameter? FindDecryptionParameter(IEnumerable<DecryptionParameter> decryptionParameters, IDataStore encryptedFileInfo)
        {
            if (encryptedFileInfo == null)
            {
                throw new ArgumentNullException(nameof(encryptedFileInfo));
            }

            using Stream fileStream = encryptedFileInfo.OpenRead();
            using IAxCryptDocument document = CreateDocument(decryptionParameters, fileStream);

            return document.DecryptionParameter;
        }

        public virtual IAxCryptDocument CreateDocument(EncryptionParameters encryptionParameters)
        {
            if (encryptionParameters == null)
            {
                throw new ArgumentNullException(nameof(encryptionParameters));
            }
            if (encryptionParameters.Passphrase == Passphrase.Empty)
            {
                throw new InternalErrorException("Cannot allow encryption with an empty password.", Abstractions.ErrorStatus.InternalError);
            }

            long keyWrapIterations = Resolve.UserSettings.GetKeyWrapIterations(encryptionParameters.CryptoId);
            if (encryptionParameters.CryptoId == new V1Aes128CryptoFactory().CryptoId)
            {
                return new V1AxCryptDocument(encryptionParameters.Passphrase, keyWrapIterations);
            }
            return new V2AxCryptDocument(encryptionParameters, keyWrapIterations);
        }

        public virtual Headers Headers(Stream inputStream)
        {
            Headers headers = new Headers();
            _ = headers.CreateReader(new LookAheadStream(inputStream));
            return headers;
        }

        /// <summary>
        /// Instantiate an instance of IAxCryptDocument appropriate for the file provided, i.e. V1 or V2.
        /// </summary>
        /// <param name="decryptionParameters">The possible decryption parameters to try.</param>
        /// <param name="inputStream">The input stream.</param>
        /// <returns></returns>
        public virtual IAxCryptDocument CreateDocument(IEnumerable<DecryptionParameter> decryptionParameters, Stream inputStream)
        {
            if (decryptionParameters == null)
            {
                throw new ArgumentNullException(nameof(decryptionParameters));
            }

            Headers headers = new Headers();
            AxCryptReaderBase reader = headers.CreateReader(new LookAheadStream(inputStream));

            IAxCryptDocument document = AxCryptReaderBase.Document(reader);
            foreach (DecryptionParameter decryptionParameter in decryptionParameters)
            {
                if (decryptionParameter.Passphrase != null)
                {
                    _ = document.Load(decryptionParameter.Passphrase, decryptionParameter.CryptoId, headers);
                    if (document.PassphraseIsValid)
                    {
                        document.DecryptionParameter = decryptionParameter;
                        return document;
                    }
                }
                if (decryptionParameter.PrivateKey != null)
                {
                    _ = document.Load(decryptionParameter.PrivateKey, decryptionParameter.CryptoId, headers);
                    if (document.PassphraseIsValid)
                    {
                        document.DecryptionParameter = decryptionParameter;
                        return document;
                    }
                }
            }
            return document;
        }
    }
}
