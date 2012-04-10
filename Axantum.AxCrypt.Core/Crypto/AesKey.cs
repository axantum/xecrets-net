using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Axantum.AxCrypt.Core.Crypto
{
    /// <summary>
    /// Hold a key for AES. This class is immutable.
    /// </summary>
    public class AesKey
    {
        private static ICollection<int> _validAesKeySizes = ValidAesKeySizes();

        private byte[] _aesKey;

        public AesKey()
        {
            _aesKey = Environment.Current.GetRandomBytes(16);
        }

        public AesKey(AesKey aesKey)
        {
            _aesKey = (byte[])aesKey._aesKey.Clone();
        }

        public AesKey(byte[] key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (!_validAesKeySizes.Contains(key.Length))
            {
                throw new InternalErrorException("Invalid AES key size");
            }
            _aesKey = (byte[])key.Clone();
        }

        public byte[] Get()
        {
            return (byte[])_aesKey.Clone();
        }

        public int Length
        {
            get
            {
                return _aesKey.Length;
            }
        }

        private static ICollection<int> ValidAesKeySizes()
        {
            List<int> validAesKeySizes = new List<int>();
            using (AesManaged aes = new AesManaged())
            {
                foreach (KeySizes keySizes in aes.LegalKeySizes)
                {
                    for (int validKeySizeInBits = keySizes.MinSize; validKeySizeInBits <= keySizes.MaxSize; validKeySizeInBits += keySizes.SkipSize)
                    {
                        validAesKeySizes.Add(validKeySizeInBits / 8);
                    }
                }
            }
            return validAesKeySizes;
        }
    }
}