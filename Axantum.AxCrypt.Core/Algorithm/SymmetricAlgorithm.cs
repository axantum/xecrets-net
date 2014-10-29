using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Algorithm
{
    public abstract class SymmetricAlgorithm : IDisposable
    {
        protected KeySizes[] blockSizes;

        protected KeySizes[] keySizes;

        public virtual int FeedbackSize { get; set; }

        private byte[] _iv;

        public virtual byte[] IV
        {
            get
            {
                if (_iv == null)
                {
                    GenerateIV();
                }
                return (byte[])_iv.Clone();
            }
            set
            {
                _iv = (byte[])value.Clone();
            }
        }

        private byte[] _key;

        public virtual byte[] Key
        {
            get
            {
                if (_key == null)
                {
                    GenerateKey();
                }
                return (byte[])_key.Clone();
            }
            set
            {
                _key = (byte[])value.Clone();
                _keySize = _key.Length * 8;
            }
        }

        private int _keySize;

        public virtual int KeySize
        {
            get
            {
                return _keySize;
            }
            set
            {
                _keySize = value;
                _key = null;
            }
        }

        public virtual KeySizes[] LegalBlockSizes
        {
            get
            {
                return blockSizes;
            }
        }

        public virtual KeySizes[] LegalKeySizes
        {
            get
            {
                return keySizes;
            }
        }

        public virtual CipherMode Mode { get; set; }

        public virtual PaddingMode Padding { get; set; }

        public virtual bool ValidKeySize(int bitLength)
        {
            foreach (KeySizes sizes in keySizes)
            {
                for (int length = sizes.MinSize; length <= sizes.MaxSize; length += sizes.SkipSize)
                {
                    if (length == bitLength)
                    {
                        return true;
                    }
                    if (sizes.SkipSize == 0)
                    {
                        break;
                    }
                }
            }
            return false;
        }

        public virtual void Clear()
        {
            Dispose();
        }

        private int _blockSize;

        public virtual int BlockSize
        {
            get
            {
                return _blockSize;
            }
            set
            {
                _blockSize = value;
                _iv = null;
            }
        }

        public virtual ICryptoTransform CreateDecryptor()
        {
            return CreateDecryptor(_key, _iv);
        }

        public abstract ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV);

        public virtual ICryptoTransform CreateEncryptor()
        {
            return CreateEncryptor(_key, _iv);
        }

        public abstract ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV);

        public abstract void GenerateIV();

        public abstract void GenerateKey();

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
            if (_key != null)
            {
                Array.Clear(_key, 0, _key.Length);
                _key = null;
            }
            if (_iv != null)
            {
                Array.Clear(_iv, 0, _iv.Length);
                _iv = null;
            }
        }
    }
}