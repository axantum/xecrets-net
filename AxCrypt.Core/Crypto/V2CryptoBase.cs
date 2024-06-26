﻿using AxCrypt.Abstractions.Algorithm;
using AxCrypt.Core.Algorithm;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Xecrets.Net.Cryptography;

namespace AxCrypt.Core.Crypto
{
    public abstract class V2CryptoBase : CryptoBase
    {
        private SymmetricIV? _iv;

        private long _blockCounter;

        private int _blockOffset;

        /// <summary>
        /// Initializes a new instance of the <see cref="V2CryptoBase" /> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="iv">The iv, or null for none.</param>
        /// <param name="keyStreamOffset">The key stream offset.</param>
        /// <exception cref="System.ArgumentNullException">factory
        /// or
        /// key
        /// or
        /// iv</exception>
        /// <exception cref="System.ArgumentException">Key length is invalid.
        /// or
        /// The IV length must be the same as the algorithm block length.</exception>
        protected void Initialize(SymmetricKey key, SymmetricIV? iv, long keyStreamOffset, SymmetricAlgorithm algorithm)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (algorithm == null)
            {
                throw new ArgumentNullException(nameof(algorithm));
            }

            if (!algorithm.ValidKeySize(key.Size))
            {
                throw new ArgumentException("Key length is invalid.");
            }
            iv ??= new SymmetricIV(new byte[algorithm.BlockSize / 8]);
            if (iv.Length != algorithm.BlockSize / 8)
            {
                throw new ArgumentException("The IV length must be the same as the algorithm block length.");
            }

            Key = key;
            _iv = iv;
            _blockCounter = keyStreamOffset / iv.Length;
            _blockOffset = (int)(keyStreamOffset % iv.Length);
        }

        public override int BlockLength
        {
            get
            {
                return _iv?.Length ?? throw new InvalidOperationException("Internal Program Error, _iv was null.");
            }
        }

        /// <summary>
        /// Create an instance of a transform suitable for NIST Key Wrap.
        /// </summary>
        /// <returns></returns>
        /// <value>
        /// An instance of the transform.
        /// </value>
        public override IKeyWrapTransform CreateKeyWrapTransform(Salt salt, KeyWrapDirection keyWrapDirection)
        {
            return new BlockAlgorithmKeyWrapTransform(CreateAlgorithmInternal(), salt, keyWrapDirection);
        }

        private SymmetricAlgorithm CreateAlgorithmInternal()
        {
            SymmetricAlgorithm algorithm = CreateAlgorithm();
            algorithm.SetKey(Key.GetBytes());
            algorithm.SetIV(_iv?.GetBytes() ?? throw new InvalidOperationException("Internal Program Error, _iv was null."));
            algorithm.Mode = CipherMode.ECB;
            algorithm.Padding = PaddingMode.None;

            return algorithm;
        }

        protected abstract SymmetricAlgorithm CreateAlgorithm();

        /// <summary>
        /// Decrypt in one operation.
        /// </summary>
        /// <param name="cipherText">The complete cipher text</param>
        /// <returns>
        /// The decrypted result minus any padding
        /// </returns>
        public override byte[] Decrypt(byte[] cipherText)
        {
            return Transform(cipherText);
        }

        /// <summary>
        /// Encrypt in one operation
        /// </summary>
        /// <param name="plaintext">The complete plaintext bytes</param>
        /// <returns>
        /// The cipher text, complete with any padding
        /// </returns>
        public override byte[] Encrypt(byte[] plaintext)
        {
            return Transform(plaintext);
        }

        private byte[] Transform(byte[] plaintext)
        {
            using SymmetricAlgorithm algorithm = CreateAlgorithmInternal();
            using ICryptoTransform transform = new CtrXecretsCryptoTransform(algorithm, _blockCounter, _blockOffset);
            return transform.TransformFinalBlock(plaintext, 0, plaintext.Length);
        }

        /// <summary>
        /// Using this instances parameters, create a decryptor
        /// </summary>
        /// <returns>
        /// A new decrypting transformation instance
        /// </returns>
        public override ICryptoTransform DecryptingTransform()
        {
            return new CtrXecretsCryptoTransform(CreateAlgorithmInternal(), _blockCounter, _blockOffset);
        }

        /// <summary>
        /// Using this instances parameters, create an encryptor
        /// </summary>
        /// <returns>
        /// A new encrypting transformation instance
        /// </returns>
        public override ICryptoTransform EncryptingTransform()
        {
            return new CtrXecretsCryptoTransform(CreateAlgorithmInternal(), _blockCounter, _blockOffset);
        }
    }
}
