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

using Axantum.AxCrypt.Core.Header;
using System;
using System.IO;

namespace Axantum.AxCrypt.Core.Reader
{
    public class V2AxCryptReader : AxCryptReader
    {
        /// <summary>
        /// Instantiate an AxCryptReader from a stream.
        /// </summary>
        /// <param name="inputStream">The stream to read from, will be disposed when this instance is disposed.</param>
        /// <returns></returns>
        public V2AxCryptReader(Stream inputStream)
            : base(inputStream)
        {
        }

        protected override HeaderBlock HeaderBlockFactory(HeaderBlockType headerBlockType, byte[] dataBlock)
        {
            switch (headerBlockType)
            {
                case HeaderBlockType.Version:
                    return new VersionHeaderBlock(dataBlock);

                case HeaderBlockType.V2KeyWrap:
                    return new V2KeyWrapHeaderBlock(dataBlock);

                case HeaderBlockType.Data:
                    return new DataHeaderBlock(dataBlock);

                case HeaderBlockType.FileInfo:
                    return new FileInfoHeaderBlock(dataBlock);

                case HeaderBlockType.Compression:
                    return new V2CompressionHeaderBlock(dataBlock);

                case HeaderBlockType.UnicodeFileNameInfo:
                    return new V2UnicodeFileNameInfoHeaderBlock(dataBlock);

                case HeaderBlockType.PlaintextLengths:
                    return new V2PlaintextLengthsHeaderBlock(dataBlock);

                case HeaderBlockType.V2Hmac:
                    return new V2HmacHeaderBlock(dataBlock);

                case HeaderBlockType.EncryptedDataPart:
                    return new EncryptedDataPartBlock(dataBlock);
            }
            return new UnrecognizedHeaderBlock(headerBlockType, dataBlock);
        }
    }
}