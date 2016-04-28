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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Crypto
{
    public interface ICryptoHash
    {
        /// <summary>
        /// The algorithm name
        /// </summary>
        string AlgorithmName { get; }

        /// <summary>
        /// Gets the size, in bytes, of the digest produced by this message digest
        /// </summary>
        /// <returns>The size, in bytes</returns>
        int HashSize { get; }

        /// <summary>
        /// Gets the size, in bytes, of the internal buffer used by this digest.
        /// </summary>
        /// <returns>The size, in bytes</returns>
        int BufferLength { get; }

        /// <summary>
        /// Update the message digest with a single byte.
        /// </summary>
        /// <param name="input">The byte</param>
        void Update(byte input);

        /// <summary>
        /// Update the message digest with a block of bytes
        /// </summary>
        /// <param name="input">The byte array containing the data.</param>
        /// <param name="offset">The offset into the byte array where the data starts.</param>
        /// <param name="length">The length of the data.</param>
        void BlockUpdate(byte[] input, int offset, int length);

        /// <summary>
        /// Close the digest, producing the final digest value. The DoFinal
        /// call leaves the digest reset.
        /// </summary>
        /// <param name="output">The array the digest is to be copied into.</param>
        /// <param name="offset">The offset into the out array the digest is to start at.</param>
        /// <returns></returns>
        int DoFinal(byte[] output, int offset);

        /// <summary>
        /// Reset the digest back to it's initial state.
        /// </summary>
        void Reset();
    }
}