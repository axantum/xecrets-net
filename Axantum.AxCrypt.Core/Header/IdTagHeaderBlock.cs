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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using System.Text;

namespace Axantum.AxCrypt.Core.Header
{
    /// <summary>
    /// An arbitrary string encoded as Ansi Code Page 1252, with a C-style terminating 'nul' character.
    /// </summary>
    public class IdTagHeaderBlock : HeaderBlock
    {
        public IdTagHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.IdTag, dataBlock)
        {
        }

        public IdTagHeaderBlock(string idTag)
            : base(HeaderBlockType.IdTag)
        {
            IdTag = idTag;
        }

        private static Encoding GetEncoding()
        {
            // The default fall back is "?", and we use the parameterless constructor to avoid Mono Migration Assistant (MoMA) warnings.
            return Encoding.GetEncoding(1252, new EncoderReplacementFallback(), new DecoderReplacementFallback());
        }

        public string IdTag
        {
            get
            {
                string idTag = GetEncoding().GetString(GetDataBlockBytesReference(), 0, GetDataBlockBytesReference().Length - 1);
                return idTag;
            }
            set
            {
                byte[] idTagBytes = GetEncoding().GetBytes(value);
                byte[] dataBlock = new byte[idTagBytes.Length + 1];
                idTagBytes.CopyTo(dataBlock, 0);
                dataBlock[dataBlock.Length - 1] = 0;
                SetDataBlockBytesReference(dataBlock);
            }
        }
    }
}