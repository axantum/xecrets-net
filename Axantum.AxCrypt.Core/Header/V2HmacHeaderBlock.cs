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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core.Crypto;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Header
{
    public class V2HmacHeaderBlock : HeaderBlock
    {
        public V2HmacHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.V2Hmac, dataBlock)
        {
            if (dataBlock == null)
            {
                throw new ArgumentNullException("dataBlock");
            }
        }

        public V2HmacHeaderBlock()
            : this(new byte[V2Hmac.RequiredLength])
        {
        }

        public override object Clone()
        {
            V2HmacHeaderBlock block = new V2HmacHeaderBlock((byte[])GetDataBlockBytesReference().Clone());
            return block;
        }

        public Hmac Hmac
        {
            get
            {
                V2Hmac hmac = new V2Hmac(GetDataBlockBytesReference());
                return hmac;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                SetDataBlockBytesReference(value.GetBytes());
            }
        }
    }
}