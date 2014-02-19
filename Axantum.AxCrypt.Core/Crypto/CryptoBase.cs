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

        public int BlockLength
        {
            get { return _blockLength; }
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