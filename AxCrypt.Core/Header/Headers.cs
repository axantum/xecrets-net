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

using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.IO;
using AxCrypt.Core.Reader;
using AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Core.Header
{
    public class Headers
    {
        public IList<HeaderBlock> HeaderBlocks { get; private set; }

        public IList<HeaderBlock> TrailerBlocks { get; private set; }

        public Headers()
        {
            HeaderBlocks = new List<HeaderBlock>();
            TrailerBlocks = new List<HeaderBlock>();
        }

        public AxCryptReader CreateReader(LookAheadStream inputStream)
        {
            IList<HeaderBlock> headers = LoadUnversionedHeaders(inputStream);
            AxCryptReader reader = CreateVersionedReader(inputStream, headers);
            reader.Reinterpret(headers, HeaderBlocks);

            return reader;
        }

        public void Load(AxCryptReaderBase reader)
        {
            HeaderBlocks = LoadFromReader(reader);
        }

        private static IList<HeaderBlock> LoadUnversionedHeaders(LookAheadStream inputStream)
        {
            UnversionedAxCryptReader vxReader = new UnversionedAxCryptReader(inputStream);
            return LoadFromReader(vxReader);
        }

        private static IList<HeaderBlock> LoadFromReader(AxCryptReaderBase vxReader)
        {
            List<HeaderBlock> headers = new List<HeaderBlock>();
            _ = vxReader.Read();
            if (vxReader.CurrentItemType != AxCryptItemType.MagicGuid)
            {
                throw new FileFormatException("No magic Guid was found.", ErrorStatus.MagicGuidMissing);
            }

            ReadHeadersToLast(headers, vxReader, HeaderBlockType.Data);
            return headers;
        }

        private static AxCryptReader CreateVersionedReader(LookAheadStream inputStream, IList<HeaderBlock> headers)
        {
            VersionHeaderBlock? versionHeaderBlock = FindHeaderBlock<VersionHeaderBlock>(headers) ?? throw new FileFormatException("Missing VersionHeaderBlock.");
            AxCryptReader reader = versionHeaderBlock.FileVersionMajor switch
            {
                1 or 2 or 3 => new V1AxCryptReader(inputStream),
                4 => new V2AxCryptReader(inputStream),
                _ => throw new FileFormatException("Too new file format. You need a more recent version."),
            };
            return reader;
        }

        public void Trailers(AxCryptReaderBase reader)
        {
            ArgumentNullException.ThrowIfNull(reader);

            TrailerBlocks.Add(reader.CurrentHeaderBlock);
            ReadHeadersToLast(TrailerBlocks, reader, HeaderBlockType.V2Hmac);
        }

        private static void ReadHeadersToLast(IList<HeaderBlock> headerBlocks, AxCryptReaderBase axCryptReader, HeaderBlockType last)
        {
            while (axCryptReader.Read())
            {
                switch (axCryptReader.CurrentItemType)
                {
                    case AxCryptItemType.Data:
                    case AxCryptItemType.HeaderBlock:
                        break;

                    default:
                        throw new InternalErrorException("The reader returned an item type it should not be possible for it to return.");
                }

                headerBlocks.Add(axCryptReader.CurrentHeaderBlock);

                if (axCryptReader.CurrentHeaderBlock.HeaderBlockType == last)
                {
                    return;
                }
            }
            throw new FileFormatException("Premature end of stream.", ErrorStatus.EndOfStream);
        }

        public VersionHeaderBlock? VersionHeaderBlock
        {
            get
            {
                VersionHeaderBlock? versionHeaderBlock = FindHeaderBlock<VersionHeaderBlock>();
                return versionHeaderBlock;
            }
        }

        public T? FindHeaderBlock<T>() where T : HeaderBlock
        {
            return FindHeaderBlock<T>(HeaderBlocks);
        }

        public T? FindTrailerBlock<T>() where T : HeaderBlock
        {
            return FindHeaderBlock<T>(TrailerBlocks);
        }

        private static T? FindHeaderBlock<T>(IEnumerable<HeaderBlock> headerBlocks) where T : HeaderBlock
        {
            foreach (HeaderBlock headerBlock in headerBlocks)
            {
                if (headerBlock is T t)
                {
                    return t;
                }
            }
            return null;
        }

        public void EnsureFileFormatVersion(int lowestMajorVersion, int highestMajorVersion)
        {
            VersionHeaderBlock? versionHeaderBlock = FindHeaderBlock<VersionHeaderBlock>() ?? throw new FileFormatException("VersionHeaderBlock missing.");
            if (versionHeaderBlock.FileVersionMajor > highestMajorVersion)
            {
                throw new FileFormatException("Too new file format.", ErrorStatus.TooNewFileFormatVersion);
            }
            if (versionHeaderBlock.FileVersionMajor < lowestMajorVersion)
            {
                throw new FileFormatException("Too old file format.", ErrorStatus.TooOldFileFormatVersion);
            }
        }

        public Hmac Hmac
        {
            get
            {
                PreambleHeaderBlock? headerBlock = FindHeaderBlock<PreambleHeaderBlock>() ?? throw new FileFormatException("PreambleHeaderBlock missing.");

                return headerBlock.Hmac;
            }
            set
            {
                ArgumentNullException.ThrowIfNull(value);

                PreambleHeaderBlock? headerBlock = FindHeaderBlock<PreambleHeaderBlock>() ?? throw new FileFormatException("PreambleHeaderBlock missing.");
                headerBlock.Hmac = value;
            }
        }
    }
}
