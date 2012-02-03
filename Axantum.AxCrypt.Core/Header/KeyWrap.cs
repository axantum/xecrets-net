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
            if (wrapped.Length != 128 / 8 + 8)
            {
                throw new ArgumentException("The length of the wrapped data must be exactly the length of a key plus 8 bytes");
            }

            wrapped = (byte[])wrapped.Clone();
            byte[] saltedKey = (byte[])_key.Clone();
            saltedKey.Xor(_salt);

            AesManaged aes = new AesManaged();
            aes.Mode = CipherMode.ECB;
            aes.KeySize = 128;
            aes.Key = saltedKey;
            aes.Padding = PaddingMode.None;
            ICryptoTransform decryptor = aes.CreateDecryptor();

            byte[] block = new byte[decryptor.InputBlockSize];

            // wrapped[0..7] contains the A (IV) of the Key Wrap algorithm,
            // the rest is 'Wrapped Key Data'. We do the transform in-place.
            for (long j = _iterations - 1; j >= 0; --j)
            {
                for (int i = (aes.KeySize / 8) / 8; i >= 1; --i)
                {
                    long t = (((aes.KeySize / 8) / 8) * j) + i;
                    // MSB(B) = A XOR t
                    Array.Copy(wrapped, 0, block, 0, 8);
                    block.Xor(0, GetBytes(t), 0, 8);
                    // LSB(B) = Ri
                    Array.Copy(wrapped, i * 8, block, 8, 8);
                    // B = AESD(K, X xor t | Ri) where t = (n * j) + i
                    byte[] b = decryptor.TransformFinalBlock(block, 0, decryptor.InputBlockSize);
                    // A = MSB(B)
                    Array.Copy(b, 0, wrapped, 0, 8);
                    // Ri = LSB(B)
                    Array.Copy(b, 8, wrapped, i * 8, 8);
                }
            }
            return wrapped;
        }

        private static byte[] GetBytes(long value)
        {
            byte[] bytes = new byte[sizeof(long)];

            for (int i = bytes.Length - 1; i >= 0; --i)
            {
                bytes[i] = (byte)value;
                value >>= 8;
            }
            return bytes;
        }
    }
}