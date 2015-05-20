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

using Axantum.AxCrypt.Core.Algorithm;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Portable;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Axantum.AxCrypt.Core.Crypto
{
    /// <summary>Implements password-based key derivation functionality, PBKDF2, by using a pseudo-random number generator based on <see cref="T:System.Security.Cryptography.HMACSHA512" />.</summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pbkdf")]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sha")]
    public class Pbkdf2HmacSha512
    {
        private byte[] _bytes;

        /// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Rfc2898DeriveBytes" /> class using a password, a salt, and number of iterations to derive the key.</summary>
        /// <param name="password">The password used to derive the key. </param>
        /// <param name="salt">The key salt used to derive the key.</param>
        /// <param name="derivationIterations">The number of iterations for the operation. </param>
        /// <exception cref="T:System.ArgumentNullException">The password or salt is null. </exception>
        public Pbkdf2HmacSha512(string password, Salt salt, int derivationIterations)
        {
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            if (salt == null)
            {
                throw new ArgumentNullException("salt");
            }
            if (derivationIterations <= 0)
            {
                throw new ArgumentOutOfRangeException("derivationIterations", "Must be greater than 0.");
            }

            _bytes = F(password, salt, derivationIterations);
        }

        /// <summary>Returns the pseudo-random key for this object.</summary>
        /// <returns>A byte array filled with 64 pseudo-random key bytes.</returns>
        public byte[] GetBytes()
        {
            if (_bytes == null)
            {
                throw new InternalErrorException("The key bytes can only be read once.");
            }

            byte[] bytes = _bytes;
            _bytes = null;
            return bytes;
        }

        private static readonly byte[] _empty = new byte[0];

        private static byte[] F(string password, Salt salt, int derivationIterations)
        {
            HMAC hmacsha512 = TypeMap.Resolve.New<HMACSHA512>().Initialize(new SymmetricKey(new UTF8Encoding(false).GetBytes(password)));

            hmacsha512.TransformBlock(salt.GetBytes(), 0, salt.Length, null, 0);
            byte[] iBytes = 1.GetBigEndianBytes();

            hmacsha512.TransformBlock(iBytes, 0, iBytes.Length, null, 0);
            hmacsha512.TransformFinalBlock(_empty, 0, 0);

            byte[] u = hmacsha512.Hash();
            byte[] un = u;

            for (int c = 2; c <= derivationIterations; ++c)
            {
                hmacsha512.Initialize();
                hmacsha512.TransformBlock(u, 0, u.Length, null, 0);
                hmacsha512.TransformFinalBlock(_empty, 0, 0);
                u = hmacsha512.Hash();
                for (int i = 0; i < u.Length; i++)
                {
                    un[i] ^= u[i];
                }
            }

            return un;
        }
    }
}