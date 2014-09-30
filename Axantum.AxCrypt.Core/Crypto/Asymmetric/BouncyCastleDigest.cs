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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Crypto.Asymmetric
{
    internal class BouncyCastleDigest : Org.BouncyCastle.Crypto.IDigest
    {
        private ICryptoHash _cryptoHash;

        public BouncyCastleDigest(ICryptoHash cryptoHash)
        {
            _cryptoHash = cryptoHash;
        }

        public string AlgorithmName
        {
            get { return _cryptoHash.AlgorithmName; }
        }

        public int GetDigestSize()
        {
            return _cryptoHash.HashSize;
        }

        public int GetByteLength()
        {
            return _cryptoHash.BufferLength;
        }

        public void Update(byte input)
        {
            _cryptoHash.Update(input);
        }

        public void BlockUpdate(byte[] input, int inOff, int length)
        {
            _cryptoHash.BlockUpdate(input, inOff, length);
        }

        public int DoFinal(byte[] output, int outOff)
        {
            return _cryptoHash.DoFinal(output, outOff);
        }

        public void Reset()
        {
            _cryptoHash.Reset();
        }
    }
}