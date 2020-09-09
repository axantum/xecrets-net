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

using AxCrypt.Core.Extensions;
using AxCrypt.Core.Runtime;
using System;

namespace AxCrypt.Core.Crypto
{
    /// <summary>
    /// The HMAC of AxCrypt encrypted data. Instances of this class are immutable.
    /// </summary>
    public abstract class Hmac
    {
        private byte[] _hmac;

        protected void Initialize(byte[] hmac, int requiredLength)
        {
            if (hmac == null)
            {
                throw new ArgumentNullException("hmac");
            }
            if (hmac.Length != requiredLength)
            {
                throw new InternalErrorException("HMAC must be exactly {0} bytes.".InvariantFormat(requiredLength));
            }
            _hmac = (byte[])hmac.Clone();
        }

        /// <summary>
        /// Gets the length of the hash.
        /// </summary>
        public int Length
        {
            get
            {
                return _hmac.Length;
            }
        }

        /// <summary>
        /// Gets the hash bytes.
        /// </summary>
        /// <returns>The hash bytes</returns>
        public byte[] GetBytes()
        {
            return (byte[])_hmac.Clone();
        }

        /// <summary>
        /// Determines whether the specified <see cref="Runtime.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="Runtime.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="Runtime.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            Hmac right = (Hmac)obj;
            return this == right;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            int hashCode = 0;
            foreach (byte b in _hmac)
            {
                hashCode += (hashCode << 8) + b;
            }
            return hashCode;
        }

        /// <summary>
        /// Implements the operator == for DataHmac
        /// </summary>
        /// <param name="left">The left instance to compare</param>
        /// <param name="right">The right instance to compare</param>
        /// <returns>
        /// True if the two instances compare as equivalent, false otherwise.
        /// </returns>
        public static bool operator ==(Hmac left, Hmac right)
        {
            if (Object.ReferenceEquals(left, right))
            {
                return true;
            }
            if (Object.ReferenceEquals(left, null) || Object.ReferenceEquals(right, null))
            {
                return false;
            }
            return left._hmac.IsEquivalentTo(right._hmac);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left instance to compare</param>
        /// <param name="right">The right instance to compare</param>
        /// <returns>
        /// True if the two instances do not compare as equivalent, false otherwise.
        /// </returns>
        public static bool operator !=(Hmac left, Hmac right)
        {
            return !(left == right);
        }
    }
}