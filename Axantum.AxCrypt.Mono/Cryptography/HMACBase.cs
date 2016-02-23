using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Mono.Cryptography
{
    public abstract class HMACBase : System.Security.Cryptography.KeyedHashAlgorithm
    {
        protected int BlockSizeValue { get; set; } = 64;

        protected System.Security.Cryptography.HashAlgorithm Hash1;
        protected System.Security.Cryptography.HashAlgorithm Hash2;

        private byte[] _inner;
        private byte[] _outer;

        private bool _hashing = false;

        private void InitializeKey(byte[] key)
        {
            if (key.Length > BlockSizeValue)
            {
                KeyValue = Hash1.ComputeHash(key);
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
            Hash1.Initialize();
            Hash2.Initialize();
            _hashing = false;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            if (!_hashing)
            {
                Hash1.TransformBlock(_inner, 0, _inner.Length, _inner, 0);
                _hashing = true;
            }
            Hash1.TransformBlock(array, ibStart, cbSize, array, ibStart);
        }

        protected override byte[] HashFinal()
        {
            if (_hashing == false)
            {
                Hash1.TransformBlock(_inner, 0, _inner.Length, _inner, 0);
                _hashing = true;
            }

            Hash1.TransformFinalBlock(new Byte[0], 0, 0);
            byte[] hashValue1 = Hash1.Hash;

            Hash2.TransformBlock(_outer, 0, _outer.Length, _outer, 0);
            Hash2.TransformBlock(hashValue1, 0, hashValue1.Length, hashValue1, 0);
            _hashing = false;

            Hash2.TransformFinalBlock(new Byte[0], 0, 0);
            return Hash2.Hash;
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
            if (Hash1 != null)
            {
                ((IDisposable)Hash1).Dispose();
                Hash1 = null;
            }
            if (Hash2 != null)
            {
                ((IDisposable)Hash2).Dispose();
                Hash2 = null;
            }
        }
    }
}