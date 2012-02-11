#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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
 * The source is maintained at http://AxCrypt.codeplex.com/ please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.Reader;

namespace Axantum.AxCrypt.Core
{
    /// <summary>
    /// Enables an single point of interaction for a an AxCrypt encrypted stream with all but the data available
    /// in-memory.
    /// </summary>
    public class AxCryptDocument
    {
        private enum State
        {
        }

        private AxCryptReader _axCryptReader;

        /// <summary>
        /// Loads an AxCrypt file from the specified reader.
        /// </summary>
        /// <param name="axCryptReader">The reader.</param>
        public void Load(AxCryptReader axCryptReader)
        {
            _axCryptReader = axCryptReader;
            LoadHeaders();
        }

        private void LoadHeaders()
        {
            _axCryptReader.Read();
            if (_axCryptReader.ItemType != AxCryptItemType.MagicGuid)
            {
                throw new FileFormatException("No magic Guid was found.");
            }
            List<HeaderBlock> headerBlocks = new List<HeaderBlock>();
            while (_axCryptReader.Read())
            {
                switch (_axCryptReader.ItemType)
                {
                    case AxCryptItemType.None:
                        throw new FileFormatException("Header type is not allowed to be 'none'.");
                    case AxCryptItemType.MagicGuid:
                        throw new FileFormatException("Duplicate magic Guid found.");
                    case AxCryptItemType.HeaderBlock:
                        headerBlocks.Add(_axCryptReader.HeaderBlock);
                        break;
                    case AxCryptItemType.Data:
                        return;
                    case AxCryptItemType.EndOfStream:
                        throw new FileFormatException("End of stream found too early.");
                    default:
                        throw new FileFormatException("Unknown header type found.");
                }
            }
            ParseHeaders(headerBlocks);
        }

        private void ParseHeaders(IList<HeaderBlock> headerBlocks)
        {
        }

        /// <summary>
        /// Decrypts the encrypted data to the given stream
        /// </summary>
        /// <param name="plainTextStream">The plain text stream.</param>
        public void DecryptTo(Stream plaintextStream)
        {
        }
    }
}