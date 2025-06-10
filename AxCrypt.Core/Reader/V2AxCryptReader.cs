﻿#region Coypright and License

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
using AxCrypt.Core.Header;
using AxCrypt.Core.IO;
using System;

namespace AxCrypt.Core.Reader
{
    public class V2AxCryptReader : AxCryptReader
    {
        /// <summary>
        /// Instantiate an AxCryptReader from a stream.
        /// </summary>
        /// <param name="inputStream">The stream to read from, will be disposed when this instance is disposed.</param>
        /// <returns></returns>
        public V2AxCryptReader(LookAheadStream inputStream)
            : base(inputStream)
        {
        }

        protected override IAxCryptDocument Document()
        {
            base.Document();
            return new V2AxCryptDocument(this);
        }

        protected override HeaderBlock HeaderBlockFactory(HeaderBlockType headerBlockType, byte[] dataBlock)
        {
            switch (headerBlockType)
            {
                case HeaderBlockType.Preamble:
                    return new PreambleHeaderBlock(dataBlock);

                case HeaderBlockType.Version:
                    return new VersionHeaderBlock(dataBlock);

                case HeaderBlockType.V2KeyWrap:
                    return new V2KeyWrapHeaderBlock(dataBlock);

                case HeaderBlockType.V2AsymmetricKeyWrap:
                    return new V2AsymmetricKeyWrapHeaderBlock(dataBlock);

                case HeaderBlockType.AsymmetricRecipients:
                    return new V2AsymmetricRecipientsEncryptedHeaderBlock(dataBlock);

                case HeaderBlockType.Data:
                    return new DataHeaderBlock(dataBlock);

                case HeaderBlockType.FileInfo:
                    return new FileInfoEncryptedHeaderBlock(dataBlock);

                case HeaderBlockType.Compression:
                    return new V2CompressionEncryptedHeaderBlock(dataBlock);

                case HeaderBlockType.UnicodeFileNameInfo:
                    return new V2UnicodeFileNameInfoEncryptedHeaderBlock(dataBlock);

                case HeaderBlockType.PlaintextLengths:
                    return new V2PlaintextLengthsEncryptedHeaderBlock(dataBlock);

                case HeaderBlockType.V2Hmac:
                    return new V2HmacHeaderBlock(dataBlock);

                case HeaderBlockType.AlgorithmVerifier:
                    return new V2AlgorithmVerifierEncryptedHeaderBlock(dataBlock);

                case HeaderBlockType.AsymmetricMasterKey:
                    return new V2AsymmetricMasterKeyEncryptedHeaderBlock(dataBlock);

                case HeaderBlockType.AsymmetricMasterKeys:
                    return new V2AsymmetricMasterKeysEncryptedHeaderBlock(dataBlock);

                case HeaderBlockType.EncryptedDataPart:
                    return new EncryptedDataPartBlock(dataBlock);
            }
            return new UnrecognizedHeaderBlock(headerBlockType, dataBlock);
        }

        public override IAxCryptDocument Document(Passphrase passphrase, Guid cryptoId, Headers headers)
        {
            V2AxCryptDocument v2Document = new V2AxCryptDocument(this);
            v2Document.Load(passphrase, cryptoId, headers);
            return v2Document;
        }

        public override IAxCryptDocument Document(IAsymmetricPrivateKey privateKey, Guid cryptoId, Headers headers)
        {
            V2AxCryptDocument v2Document = new V2AxCryptDocument();
            if (cryptoId == new V1Aes128CryptoFactory().CryptoId)
            {
                return v2Document;
            }

            v2Document.Load(privateKey, cryptoId, headers);
            return v2Document;
        }
    }
}