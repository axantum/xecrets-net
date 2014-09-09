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

using System.Text;

namespace Axantum.AxCrypt.Core.Header
{
    /// <summary>
    /// An arbitrary string encoded as Ansi Code Page 1252, with a C-style terminating 'nul' character.
    /// </summary>
    public class V1IdTagHeaderBlock : HeaderBlock
    {
        public V1IdTagHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.IdTag, dataBlock)
        {
        }

        public V1IdTagHeaderBlock(string idTag)
            : base(HeaderBlockType.IdTag)
        {
            IdTag = idTag;
        }

        public override object Clone()
        {
            V1IdTagHeaderBlock block = new V1IdTagHeaderBlock((byte[])GetDataBlockBytesReference().Clone());
            return block;
        }

        private static Encoding GetEncoding()
        {
            return Encoding.GetEncoding("Windows-1252");
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