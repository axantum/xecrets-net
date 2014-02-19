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
    public class V1AxCryptReader : AxCryptReader
    {
        /// <summary>
        /// Instantiate an AxCryptReader from a stream.
        /// </summary>
        /// <param name="inputStream">The stream to read from, will be disposed when this instance is disposed.</param>
        /// <returns></returns>
        public V1AxCryptReader(Stream inputStream)
            : base(inputStream)
        {
        }

        protected override HeaderBlock HeaderBlockFactory(HeaderBlockType headerBlockType, byte[] dataBlock)
        {
            switch (headerBlockType)
            {
                case HeaderBlockType.Preamble:
                    return new PreambleHeaderBlock(dataBlock);

                case HeaderBlockType.Version:
                    return new VersionHeaderBlock(dataBlock);

                case HeaderBlockType.KeyWrap1:
                    return new V1KeyWrap1HeaderBlock(dataBlock);

                case HeaderBlockType.KeyWrap2:
                    return new V1KeyWrap2HeaderBlock(dataBlock);

                case HeaderBlockType.IdTag:
                    return new V1IdTagHeaderBlock(dataBlock);

                case HeaderBlockType.Data:
                    return new DataHeaderBlock(dataBlock);

                case HeaderBlockType.FileNameInfo:
                    return new V1FileNameInfoHeaderBlock(dataBlock);

                case HeaderBlockType.EncryptionInfo:
                    return new V1EncryptionInfoHeaderBlock(dataBlock);

                case HeaderBlockType.CompressionInfo:
                    return new V1CompressionInfoHeaderBlock(dataBlock);

                case HeaderBlockType.FileInfo:
                    return new FileInfoHeaderBlock(dataBlock);

                case HeaderBlockType.Compression:
                    return new V1CompressionHeaderBlock(dataBlock);

                case HeaderBlockType.UnicodeFileNameInfo:
                    return new V1UnicodeFileNameInfoHeaderBlock(dataBlock);
            }
            return new UnrecognizedHeaderBlock(headerBlockType, dataBlock);
        }
    }
}