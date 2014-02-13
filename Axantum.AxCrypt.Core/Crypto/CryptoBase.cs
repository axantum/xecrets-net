using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Axantum.AxCrypt.Core.Crypto
{
    public abstract class CryptoBase : ICrypto
    {
        private ICollection<int> _validKeyLengths;

        public abstract string Name { get; }

        private AesKey _key;

        public AesKey Key
        {
            get
            {
                return _key;
            }

            protected set
            {
                if (!IsValidKeyLength(value.Length))
                {
                    throw new ArgumentException("Key length is invalid.");
                }
                _key = value;
            }
        }

        public int BlockLength
        {
            get;
            protected set;
        }

        public abstract SymmetricAlgorithm CreateAlgorithm();

        public abstract byte[] Decrypt(byte[] cipherText);

        public abstract byte[] Encrypt(byte[] plaintext);

        public abstract ICryptoTransform CreateDecryptingTransform();

        public abstract ICryptoTransform CreateEncryptingTransform();

        /// <summary>
        /// Check if a key length is valid for AES
        /// </summary>
        /// <param name="length">The length in bytes</param>
        /// <returns>true if the length in bytes is a valid key length for AES</returns>
        public virtual bool IsValidKeyLength(int length)
        {
            if (_validKeyLengths == null)
            {
                _validKeyLengths = ValidKeyLengths();
            }
            return _validKeyLengths.Contains(length);
        }

        private ICollection<int> ValidKeyLengths()
        {
            List<int> validKeyLengths = new List<int>();
            using (SymmetricAlgorithm algorithm = CreateAlgorithm())
            {
                foreach (KeySizes keySizes in algorithm.LegalKeySizes)
                {
                    for (int validKeySizeInBits = keySizes.MinSize; validKeySizeInBits <= keySizes.MaxSize; validKeySizeInBits += keySizes.SkipSize)
                    {
                        validKeyLengths.Add(validKeySizeInBits / 8);
                    }
                }
            }
            return validKeyLengths;
        }
    }
}