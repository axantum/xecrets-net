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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Algorithm
{
    public abstract class SymmetricAlgorithm : IDisposable
    {
        protected KeySizes[] BlockSizes { get; set; }

        protected KeySizes[] KeySizes { get; set; }

        private int _feedbackSize;

        public virtual int FeedbackSize
        {
            get
            {
                return _feedbackSize;
            }
            set
            {
                InitializeFeedbackSize(value);
            }
        }

        protected void InitializeFeedbackSize(int feedbackSize)
        {
            _feedbackSize = feedbackSize;
        }

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
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

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
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

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
                InitializeKeySize(value);
            }
        }

        protected void InitializeKeySize(int keySize)
        {
            _keySize = keySize;
            _key = null;
        }

        public virtual KeySizes[] LegalBlockSizes
        {
            get
            {
                return BlockSizes;
            }
        }

        public virtual KeySizes[] LegalKeySizes
        {
            get
            {
                return KeySizes;
            }
        }

        public virtual CipherMode Mode { get; set; }

        public virtual PaddingMode Padding { get; set; }

        public virtual bool ValidKeySize(int bitLength)
        {
            foreach (KeySizes sizes in KeySizes)
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
                InitializeBlockSize(value);
            }
        }

        protected void InitializeBlockSize(int blockSize)
        {
            _blockSize = blockSize;
            _iv = null;
        }

        public virtual ICryptoTransform CreateDecryptingTransform()
        {
            return CreateDecryptingTransform(Key, IV);
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rgb")]
        public abstract ICryptoTransform CreateDecryptingTransform(byte[] rgbKey, byte[] rgbIV);

        public virtual ICryptoTransform CreateEncryptingTransform()
        {
            return CreateEncryptingTransform(Key, IV);
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rgb")]
        public abstract ICryptoTransform CreateEncryptingTransform(byte[] rgbKey, byte[] rgbIV);

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