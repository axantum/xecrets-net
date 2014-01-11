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

using Axantum.AxCrypt.Core.Extensions;

namespace Axantum.AxCrypt.Core.Reader
{
    public class DataHeaderBlock : HeaderBlock
    {
        public DataHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.Data, dataBlock)
        {
        }

        public DataHeaderBlock()
            : this(new byte[8])
        {
        }

        public override object Clone()
        {
            DataHeaderBlock block = new DataHeaderBlock((byte[])GetDataBlockBytesReference().Clone());
            return block;
        }

        public long CipherTextLength
        {
            get
            {
                return GetDataBlockBytesReference().GetLittleEndianValue(0, sizeof(long));
            }

            set
            {
                value.GetLittleEndianBytes().CopyTo(GetDataBlockBytesReference(), 0);
            }
        }
    }
}