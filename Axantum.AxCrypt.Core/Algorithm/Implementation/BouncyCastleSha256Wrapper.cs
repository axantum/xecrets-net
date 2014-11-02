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

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Algorithm.Implementation
{
    internal class BouncyCastleSha256Wrapper : Sha256
    {
        private IDigest _hashAlgorithm;

        public BouncyCastleSha256Wrapper()
        {
            _hashAlgorithm = new Sha1Digest();
        }

        public override byte[] ComputeHash(byte[] buffer)
        {
            return ComputeHash(buffer, 0, buffer.Length);
        }

        public override byte[] ComputeHash(byte[] buffer, int offset, int count)
        {
            _hashAlgorithm.Reset();
            _hashAlgorithm.BlockUpdate(buffer, offset, count);
            return Hash;
        }

        public override byte[] ComputeHash(Stream inputStream)
        {
            _hashAlgorithm.Reset();
            byte[] block = new byte[_hashAlgorithm.GetByteLength()];
            int count;
            while ((count = inputStream.Read(block, 0, block.Length)) > 0)
            {
                _hashAlgorithm.BlockUpdate(block, 0, count);
            }
            return Hash;
        }

        private byte[] _hash;

        public override byte[] Hash
        {
            get
            {
                if (_hash == null)
                {
                    _hash = new byte[_hashAlgorithm.GetDigestSize()];
                    _hashAlgorithm.DoFinal(_hash, 0);
                }
                return _hash;
            }
        }

        public override int HashSize
        {
            get { return _hashAlgorithm.GetDigestSize(); }
        }

        public override void Initialize()
        {
            _hashAlgorithm.Reset();
        }

        public override bool CanReuseTransform
        {
            get { return true; }
        }

        public override bool CanTransformMultipleBlocks
        {
            get { return true; }
        }

        public override int InputBlockSize
        {
            get { return _hashAlgorithm.GetByteLength(); }
        }

        public override int OutputBlockSize
        {
            get { return _hashAlgorithm.GetDigestSize(); }
        }

        public override int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            _hashAlgorithm.BlockUpdate(inputBuffer, inputOffset, inputCount);
            return inputCount;
        }

        public override byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            _hashAlgorithm.BlockUpdate(inputBuffer, inputOffset, inputCount);
            return Hash;
        }

        public override void Dispose()
        {
        }
    }
}