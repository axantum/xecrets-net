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
using Axantum.AxCrypt.Core.Runtime;

namespace Axantum.AxCrypt.Core.Crypto
{
    /// <summary>
    /// A salt for the AES Key Wrap. Instances of this class are immutable.
    /// </summary>
    public class KeyWrapSalt
    {
        private readonly byte[] _salt;

        /// <summary>
        /// An instance of KeyWrapSalt with all zeroes.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "The reference type 'AesIV' is, in fact, immutable.")]
        public static readonly KeyWrapSalt Zero = new KeyWrapSalt(new byte[0]);

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyWrapSalt"/> class.
        /// </summary>
        /// <param name="length">The length of the salt in bytes. It must be a valid AES key length.</param>
        public KeyWrapSalt(int length)
        {
            if (!AesKey.IsValidKeyLength(length))
            {
                throw new InternalErrorException("A key wrap salt length must at least be equal to a valid AES key length.");
            }
            _salt = OS.Current.GetRandomBytes(length);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyWrapSalt"/> class.
        /// </summary>
        /// <param name="length">The salt. It must be a valid AES key length.</param>
        public KeyWrapSalt(byte[] salt)
        {
            if (salt == null)
            {
                throw new ArgumentNullException("salt");
            }
            if (salt.Length != 0 && !AesKey.IsValidKeyLength(salt.Length))
            {
                throw new InternalErrorException("A key wrap salt length must at least be equal to a valid AES key length.");
            }
            _salt = (byte[])salt.Clone();
        }

        /// <summary>
        /// Gets the length of the salt.
        /// </summary>
        public int Length
        {
            get
            {
                return _salt.Length;
            }
        }

        /// <summary>
        /// Gets the bytes of the salt.
        /// </summary>
        /// <returns>Returns the bytes of the salt.</returns>
        public byte[] GetBytes()
        {
            return (byte[])_salt.Clone();
        }
    }
}