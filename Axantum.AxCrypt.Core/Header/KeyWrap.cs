using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Axantum.AxCrypt.Core.Header
{
    public class KeyWrap
    {
        private byte[] _key;
        private byte[] _salt;
        private long _iterations;

        public KeyWrap(byte[] key, byte[] salt, long iterations)
        {
            _key = (byte[])key.Clone();
            _salt = (byte[])salt.Clone();
            _iterations = iterations;
        }

        public byte[] Unwrap(byte[] wrapped)
        {
            wrapped = (byte[])wrapped.Clone();
            byte[] saltedKey = (byte[])_key.Clone();
            saltedKey.Xor(_salt);

            AesManaged aes = new AesManaged();
            aes.Mode = CipherMode.ECB;
            aes.KeySize = 128;
            aes.Key = saltedKey;
            ICryptoTransform decryptor = aes.CreateDecryptor();

            byte[] bBlock = new byte[decryptor.InputBlockSize];

            // wrapped[0..7] contains the A (IV) of the Key Wrap algorithm,
            // the rest is 'Wrapped Key Data'. We do the transform in-place.
            for (long j = _iterations - 1; j >= 0; --j)
            {
                for (int i = aes.KeySize / 8; i >= 1; --i)
                {
                    // B = AESD(K, A XOR t | R[i]) where t = (n * j) + i
                    long t = ((aes.KeySize / 8)* j) + i;
                    wrapped.CopyTo(bBlock, 4);
                    wrapped.Xor(BitConverter.GetBytes(t));
                    Array.Copy(wrapped, i * 8, bBlock, 0)
                }
            }
            return new byte[0];
        }
    }
}