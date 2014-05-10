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

using System;
using System.Diagnostics.CodeAnalysis;

namespace Axantum.AxCrypt.Core.Crypto
{
    /// <summary>
    /// An Initial Vector for block chaining with a symmetric algorithm. Instances of this class are immutable.
    /// </summary>
    public class SymmetricIV
    {
        private byte[] _iv;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "The reference type 'SymmetricIV' is, in fact, immutable.")]
        public static readonly SymmetricIV Zero128 = new SymmetricIV(new byte[128 / 8]);

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "The reference type 'SymmetricIV' is, in fact, immutable.")]
        public static readonly SymmetricIV Zero64 = new SymmetricIV(new byte[64 / 8]);

        /// <summary>
        /// Instantiate a new random IV
        /// </summary>
        public SymmetricIV(int blockBits)
        {
            _iv = Instance.RandomGenerator.Generate(blockBits / 8);
        }

        /// <summary>
        /// Instantiate a new IV
        /// </summary>
        /// <param name="iv">The Initial Vector to use</param>
        public SymmetricIV(byte[] iv)
        {
            if (iv == null)
            {
                throw new ArgumentNullException("iv");
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

        /// <summary>
        /// Gets the length of the IV.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int Length
        {
            get
            {
                return _iv.Length;
            }
        }
    }
}