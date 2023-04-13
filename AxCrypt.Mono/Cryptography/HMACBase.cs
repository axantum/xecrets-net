using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Mono.Cryptography
{
    public abstract class HMACBase : System.Security.Cryptography.KeyedHashAlgorithm
    {
        protected int BlockSizeValue { get; set; } = 64;

        [AllowNull]
        private System.Security.Cryptography.HashAlgorithm _hash1;

        protected void SetHash1(System.Security.Cryptography.HashAlgorithm hash)
        {
            _hash1 = hash;
        }

        [AllowNull]
        private System.Security.Cryptography.HashAlgorithm _hash2;

        protected void SetHash2(System.Security.Cryptography.HashAlgorithm hash)
        {
            _hash2 = hash;
        }

        [AllowNull]
        private byte[] _inner;

        [AllowNull]
        private byte[] _outer;

        private bool _hashing = false;

        private void InitializeKey(byte[] key)
        {
            if (key.Length > BlockSizeValue)
            {
                KeyValue = _hash1.ComputeHash(key);
            }
            else
            {
                KeyValue = (byte[])key.Clone();
            }

            _inner = new byte[BlockSizeValue];
            _outer = new byte[BlockSizeValue];

            for (int i = 0; i < BlockSizeValue; i++)
            {
                _inner[i] = 0x36;
                _outer[i] = 0x5C;
            }

            for (int i = 0; i < KeyValue.Length; i++)
            {
                _inner[i] ^= KeyValue[i];
                _outer[i] ^= KeyValue[i];
            }
        }

        public override byte[] Key
        {
            get { return (byte[])KeyValue.Clone(); }
            set
            {
                if (_hashing)
                {
                    throw new System.Security.Cryptography.CryptographicException("Can't set Key when already hashing.");
                }
                InitializeKey(value);
            }
        }

        public override void Initialize()
        {
            _hash1.Initialize();
            _hash2.Initialize();
            _hashing = false;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            if (!_hashing)
            {
                _ = _hash1.TransformBlock(_inner, 0, _inner.Length, _inner, 0);
                _hashing = true;
            }
            _ = _hash1.TransformBlock(array, ibStart, cbSize, array, ibStart);
        }

        protected override byte[] HashFinal()
        {
            if (_hashing == false)
            {
                _ = _hash1.TransformBlock(_inner, 0, _inner.Length, _inner, 0);
                _hashing = true;
            }

            _ = _hash1.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            byte[] hashValue1 = _hash1.Hash!;

            _ = _hash2.TransformBlock(_outer, 0, _outer.Length, _outer, 0);
            _ = _hash2.TransformBlock(hashValue1, 0, hashValue1.Length, hashValue1, 0);
            _hashing = false;

            _ = _hash2.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            return _hash2.Hash!;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeInternal();
            }
            base.Dispose(disposing);
        }

        private void DisposeInternal()
        {
            if (_hash1 != null)
            {
                ((IDisposable)_hash1).Dispose();
                _hash1 = null;
            }
            if (_hash2 != null)
            {
                ((IDisposable)_hash2).Dispose();
                _hash2 = null;
            }
        }
    }
}
