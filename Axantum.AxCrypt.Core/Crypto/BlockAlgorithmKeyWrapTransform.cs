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
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Crypto
{
    public class BlockAlgorithmKeyWrapTransform : IKeyWrapTransform
    {
        private SymmetricAlgorithm _algorithm;

        private ICryptoTransform _transform;

        public BlockAlgorithmKeyWrapTransform(SymmetricAlgorithm symmetricAlgorithm, Salt salt, KeyWrapDirection keyWrapDirection)
        {
            if (symmetricAlgorithm == null)
            {
                throw new ArgumentNullException("symmetricAlgorithm");
            }
            if (salt == null)
            {
                throw new ArgumentNullException("salt");
            }

            if (salt.Length != 0 && salt.Length < symmetricAlgorithm.Key.Length)
            {
                throw new InternalErrorException("Salt is too short. It must be at least as long as the algorithm key, or empty for no salt.");
            }
            _algorithm = symmetricAlgorithm;

            byte[] saltedKey = _algorithm.Key;
            saltedKey.Xor(salt.GetBytes().Reduce(saltedKey.Length));
            _algorithm.Key = saltedKey;

            _algorithm.Mode = CipherMode.ECB;
            _algorithm.Padding = PaddingMode.None;

            BlockLength = _algorithm.BlockSize / 8;

            _transform = keyWrapDirection == KeyWrapDirection.Encrypt ? _algorithm.CreateEncryptor() : _algorithm.CreateDecryptor();
        }

        public byte[] TransformBlock(byte[] block)
        {
            if (block == null)
            {
                throw new ArgumentNullException("block");
            }
            if (block.Length != BlockLength)
            {
                throw new ArgumentException("Argument 'block' must be a single block for the algorithm.");
            }
            byte[] transformed = new byte[block.Length];
            _transform.TransformBlock(block, 0, block.Length, transformed, 0);
            return transformed;
        }

        public byte[] A()
        {
            byte[] a = new byte[BlockLength / 2];
            for (int i = 0; i < a.Length; ++i)
            {
                a[i] = 0xA6;
            }
            return a;
        }

        /// <summary>
        /// Gets the block length in bytes of the transforma.
        /// </summary>
        public int BlockLength
        {
            get;
            private set;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }
            if (_transform != null)
            {
                _transform.Dispose();
                _transform = null;
            }
            if (_algorithm != null)
            {
                // Clear() is implemented as a call to Dispose(), but Mono does not implement Dispose(), so this avoids a MoMA warning.
                _algorithm.Clear();
                _algorithm = null;
            }
        }
    }
}