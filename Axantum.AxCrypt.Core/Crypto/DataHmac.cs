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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Crypto
{
    /// <summary>
    /// The HMAC of the encrypted data. Instances of this class are immutable.
    /// </summary>
    public class DataHmac
    {
        private byte[] _hmac;

        public DataHmac(byte[] hmac)
        {
            if (hmac == null)
            {
                throw new ArgumentNullException("hmac");
            }
            if (hmac.Length != 16)
            {
                throw new InternalErrorException("HMAC must be exactly 16 bytes.");
            }
            _hmac = (byte[])hmac.Clone();
        }

        public int Length
        {
            get
            {
                return _hmac.Length;
            }
        }

        public byte[] GetBytes()
        {
            return (byte[])_hmac.Clone();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            DataHmac right = (DataHmac)obj;
            return this == right;
        }

        public override int GetHashCode()
        {
            return _hmac.GetHashCode();
        }

        public static bool operator ==(DataHmac left, DataHmac right)
        {
            if (Object.ReferenceEquals(left, null) || Object.ReferenceEquals(right, null))
            {
                return false;
            }
            return left._hmac.IsEquivalentTo(right._hmac);
        }

        public static bool operator !=(DataHmac left, DataHmac right)
        {
            return !(left == right);
        }
    }
}