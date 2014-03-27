using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace Axantum.AxCrypt.Core.Crypto
{
    public class BlockAlgorithmKeyWrapTransform : IKeyWrapTransform
    {
        private SymmetricAlgorithm _algorithm;

        private ICryptoTransform _transform;

        public BlockAlgorithmKeyWrapTransform(SymmetricAlgorithm symmetricAlgorithm, Salt salt, KeyWrapDirection keyWrapDirection)
        {
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
            return _transform.TransformFinalBlock(block, 0, BlockLength);
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