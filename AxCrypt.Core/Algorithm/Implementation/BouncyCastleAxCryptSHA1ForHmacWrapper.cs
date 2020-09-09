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

using AxCrypt.Core.Crypto;
using AxCrypt.Core.Portable;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Diagnostics.CodeAnalysis;

namespace AxCrypt.Core.Algorithm.Implementation
{
    /// <summary>
    /// Used to calculate HMACSHA1 the AxCrypt way.
    /// </summary>
    /// <remarks>
    /// The standard implementation uses a block size of 64, which is really only relevant
    /// as to how to treat the key. AxCrypt uses a block size of 20. This class is required because
    /// the standard implementation does not have the GetByteLength function virtual.
    /// </remarks>
    internal sealed class BouncyCastleAxCryptSha1ForHmacWrapper : IDigest
    {
        private IDigest _digest;

        public BouncyCastleAxCryptSha1ForHmacWrapper(IDigest digest)
        {
            _digest = digest;
        }

        public string AlgorithmName
        {
            get { return _digest.AlgorithmName; }
        }

        public int GetDigestSize()
        {
            return _digest.GetDigestSize();
        }

        public int GetByteLength()
        {
            return _digest.GetDigestSize();
        }

        public void Update(byte input)
        {
            _digest.Update(input);
        }

        public void BlockUpdate(byte[] input, int inOff, int length)
        {
            _digest.BlockUpdate(input, inOff, length);
        }

        public int DoFinal(byte[] output, int outOff)
        {
            return _digest.DoFinal(output, outOff);
        }

        public void Reset()
        {
            _digest.Reset();
        }
    }
}