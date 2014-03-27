using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Axantum.AxCrypt.Core.Crypto
{
    public abstract class CryptoBase : ICrypto
    {
        private static ICollection<int> _validKeyLengths;

        private static int _blockLength;

        public abstract string Name { get; }

        private IPassphrase _key;

        /// <summary>
        /// Gets the key associated with this instance.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public IPassphrase Key
        {
            get
            {
                return _key;
            }

            protected set
            {
                _key = value;
            }
        }

        /// <summary>
        /// Gets the underlying algorithm block length in bytes
        /// </summary>
        public int BlockLength
        {
            get { return _blockLength; }
        }

        /// <summary>
        /// Create an instance of a transform suitable for NIST Key Wrap.
        /// </summary>
        /// <param name="salt"></param>
        /// <param name="keyWrapDirection"></param>
        /// <returns></returns>
        /// <value>
        /// An instance of the transform.
        ///   </value>
        public abstract IKeyWrapTransform CreateKeyWrapTransform(Salt salt, KeyWrapDirection keyWrapDirection);

        /// <summary>
        /// Decrypt in one operation.
        /// </summary>
        /// <param name="cipherText"></param>
        /// <returns>
        /// The decrypted result minus any padding
        /// </returns>
        public abstract byte[] Decrypt(byte[] cipherText);

        /// <summary>
        /// Encrypt in one operation
        /// </summary>
        /// <param name="plaintext">The complete plaintext bytes</param>
        /// <returns>
        /// The cipher text, complete with any padding
        /// </returns>
        public abstract byte[] Encrypt(byte[] plaintext);

        /// <summary>
        /// Using this instances parameters, create a decryptor
        /// </summary>
        /// <returns>
        /// A new decrypting transformation instance
        /// </returns>
        public abstract ICryptoTransform CreateDecryptingTransform();

        /// <summary>
        /// Using this instances parameters, create an encryptor
        /// </summary>
        /// <returns>
        /// A new encrypting transformation instance
        /// </returns>
        public abstract ICryptoTransform CreateEncryptingTransform();

        /// <summary>
        /// Check if the provided length in bytes is appropriate for the underlying algorithm.
        /// </summary>
        /// <param name="length">Proposed key length in bytes.</param>
        /// <returns>
        /// true if the length is valid for the algorithm.
        /// </returns>
        public bool IsValidKeyLength(int length)
        {
            return _validKeyLengths.Contains(length);
        }

        protected static void SetValidKeyLengths(KeySizes[] legalKeySizes)
        {
            List<int> validKeyLengths = new List<int>();
            foreach (KeySizes keySizes in legalKeySizes)
            {
                for (int validKeySizeInBits = keySizes.MinSize; validKeySizeInBits <= keySizes.MaxSize; validKeySizeInBits += keySizes.SkipSize)
                {
                    validKeyLengths.Add(validKeySizeInBits / 8);
                }
            }
            _validKeyLengths = validKeyLengths;
        }

        protected static void SetBlockLength(int blockLength)
        {
            _blockLength = blockLength;
        }
    }
}