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
using System.Diagnostics.CodeAnalysis;
using Axantum.AxCrypt.Core.System;

namespace Axantum.AxCrypt.Core.Crypto
{
    /// <summary>
    /// An Initial Vector for CBC chaining with AES. Instances of this class are immutable.
    /// </summary>
    public class AesIV
    {
        private byte[] _iv;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "The reference type 'AesIV' is, in fact, immutable.")]
        public static readonly AesIV Zero = new AesIV(new byte[16]);

        /// <summary>
        /// Instantiate a new random IV
        /// </summary>
        public AesIV()
        {
            _iv = AxCryptEnvironment.Current.GetRandomBytes(16);
        }

        /// <summary>
        /// Instantiate a new IV
        /// </summary>
        /// <param name="iv">The Initial Vector to use</param>
        public AesIV(byte[] iv)
        {
            if (iv == null)
            {
                throw new ArgumentNullException("iv");
            }
            if (iv.Length != 16)
            {
                throw new InternalErrorException("AesIv must be exactly 16 bytes long.");
            }
            _iv = (byte[])iv.Clone();
        }

        /// <summary>
        /// Get the actual IV bytes
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            return (byte[])_iv.Clone();
        }
    }
}