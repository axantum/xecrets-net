using System;
using System.Linq;
using System.Security.Cryptography;

namespace Axantum.AxCrypt.Core.Crypto
{
    public abstract class V2CryptoBase : CryptoBase
    {
        private SymmetricIV _iv;

        private long _blockCounter;

        private int _blockOffset;

        public V2CryptoBase(ICryptoFactory factory, IDerivedKey key, SymmetricIV iv, long keyStreamOffset)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (iv == null)
            {
                throw new ArgumentNullException("iv");
            }
            using (SymmetricAlgorithm algorithm = CreateRawAlgorithm())
            {
                if (!algorithm.ValidKeySize(key.DerivedKey.Size))
                {
                    throw new ArgumentException("Key length is invalid.");
                }
                if (iv.Length != algorithm.BlockSize / 8)
                {
                    throw new ArgumentException("The IV length must be the same as the algorithm block length.");
                }
            }

            Factory = factory;
            Key = key;
            _iv = iv;
            _blockCounter = keyStreamOffset / iv.Length;
            _blockOffset = (int)(keyStreamOffset % iv.Length);
        }

        public override int BlockLength
        {
            get
            {
                return _iv.Length;
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
            SymmetricAlgorithm algorithm = CreateRawAlgorithm();
            algorithm.Key = Key.DerivedKey.GetBytes();
            algorithm.IV = _iv.GetBytes();
            algorithm.Mode = CipherMode.ECB;
            algorithm.Padding = PaddingMode.None;

            return algorithm;
        }

        protected abstract SymmetricAlgorithm CreateRawAlgorithm();

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
            using (SymmetricAlgorithm algorithm = CreateAlgorithmInternal())
            {
                using (ICryptoTransform transform = new CounterModeCryptoTransform(algorithm, _blockCounter, _blockOffset))
                {
                    return transform.TransformFinalBlock(plaintext, 0, plaintext.Length);
                }
            }
        }

        /// <summary>
        /// Using this instances parameters, create a decryptor
        /// </summary>
        /// <returns>
        /// A new decrypting transformation instance
        /// </returns>
        public override ICryptoTransform CreateDecryptingTransform()
        {
            return new CounterModeCryptoTransform(CreateAlgorithmInternal(), _blockCounter, _blockOffset);
        }

        /// <summary>
        /// Using this instances parameters, create an encryptor
        /// </summary>
        /// <returns>
        /// A new encrypting transformation instance
        /// </returns>
        public override ICryptoTransform CreateEncryptingTransform()
        {
            return new CounterModeCryptoTransform(CreateAlgorithmInternal(), _blockCounter, _blockOffset);
        }
    }
}