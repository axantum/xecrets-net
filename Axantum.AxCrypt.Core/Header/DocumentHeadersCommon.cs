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

using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Core.Header
{
    public class DocumentHeadersCommon
    {
        public IList<HeaderBlock> HeaderBlocks { get; private set; }

        public DocumentHeadersCommon(byte[] version)
        {
            HeaderBlocks = new List<HeaderBlock>();

            HeaderBlocks.Add(new PreambleHeaderBlock());
            HeaderBlocks.Add(new VersionHeaderBlock(version));
        }

        public void Load(AxCryptReaderBase axCryptReader)
        {
            HeaderBlocks.Clear();
            axCryptReader.Read();
            if (axCryptReader.CurrentItemType != AxCryptItemType.MagicGuid)
            {
                throw new FileFormatException("No magic Guid was found.", ErrorStatus.MagicGuidMissing);
            }

            while (axCryptReader.Read())
            {
                switch (axCryptReader.CurrentItemType)
                {
                    case AxCryptItemType.HeaderBlock:
                        HeaderBlocks.Add(axCryptReader.CurrentHeaderBlock);
                        break;

                    case AxCryptItemType.Data:
                        HeaderBlocks.Add(axCryptReader.CurrentHeaderBlock);
                        return;

                    default:
                        throw new InternalErrorException("The reader returned an AxCryptItemType it should not be possible for it to return.");
                }
            }
            throw new FileFormatException("Premature end of stream.", ErrorStatus.EndOfStream);
        }

        public void SetCurrentVersion(byte[] version)
        {
            VersionHeaderBlock versionHeaderBlock = FindHeaderBlock<VersionHeaderBlock>();
            versionHeaderBlock.SetCurrentVersion(version);
        }

        public VersionHeaderBlock VersionHeaderBlock
        {
            get
            {
                VersionHeaderBlock versionHeaderBlock = FindHeaderBlock<VersionHeaderBlock>();
                return versionHeaderBlock;
            }
        }

        public T FindHeaderBlock<T>() where T : HeaderBlock
        {
            foreach (HeaderBlock headerBlock in HeaderBlocks)
            {
                T typedHeaderHeaderBlock = headerBlock as T;
                if (typedHeaderHeaderBlock != null)
                {
                    return typedHeaderHeaderBlock;
                }
            }
            return null;
        }

        public void EnsureFileFormatVersion(int lowestMajorVersion, int highestMajorVersion)
        {
            VersionHeaderBlock versionHeaderBlock = FindHeaderBlock<VersionHeaderBlock>();
            if (versionHeaderBlock.FileVersionMajor > highestMajorVersion)
            {
                throw new FileFormatException("Too new file format.", ErrorStatus.TooNewFileFormatVersion);
            }
            if (versionHeaderBlock.FileVersionMajor < lowestMajorVersion)
            {
                throw new FileFormatException("Too old file format.", ErrorStatus.TooOldFileFormatVersion);
            }
        }
    }
}