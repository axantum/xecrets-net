using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Axantum.AxCrypt.Core.Header
{
    /// <summary>
    /// Implements AES Key Wrap Specification - http://csrc.nist.gov/groups/ST/toolkit/documents/kms/key-wrap.pdf .
    /// </summary>
    public class KeyWrap : IDisposable
    {
        private byte[] _key;
        private byte[] _salt;
        private long _iterations;

        public KeyWrap(byte[] key, byte[] salt, long iterations)
        {
            _key = (byte[])key.Clone();
            _salt = (byte[])salt.Clone();
            _iterations = iterations;

            byte[] saltedKey = (byte[])_key.Clone();
            saltedKey.Xor(_salt);
            Aes.Key = saltedKey;
        }

        private AesManaged _aes = null;

        private AesManaged Aes
        {
            get
            {
                if (_aes == null)
                {
                    _aes = new AesManaged();
                    _aes.Mode = CipherMode.ECB;
                    _aes.KeySize = 128;
                    _aes.Padding = PaddingMode.None;
                }
                return _aes;
            }
        }

        public byte[] Unwrap(byte[] wrapped)
        {
            if (wrapped.Length != 128 / 8 + 8)
            {
                throw new ArgumentException("The length of the wrapped data must be exactly the length of a key plus 8 bytes");
            }

            wrapped = (byte[])wrapped.Clone();
            ICryptoTransform decryptor = Aes.CreateDecryptor();

            byte[] block = new byte[decryptor.InputBlockSize];

            // wrapped[0..7] contains the A (IV) of the Key Wrap algorithm,
            // the rest is 'Wrapped Key Data', R[1], ..., R[n]. We do the transform in-place.
            for (long j = _iterations - 1; j >= 0; --j)
            {
                for (int i = (Aes.KeySize / 8) / 8; i >= 1; --i)
                {
                    long t = (((Aes.KeySize / 8) / 8) * j) + i;
                    // MSB(B) = A XOR t
                    Array.Copy(wrapped, 0, block, 0, 8);
                    block.Xor(0, GetBigEndianBytes(t), 0, 8);
                    // LSB(B) = R[i]
                    Array.Copy(wrapped, i * 8, block, 8, 8);
                    // B = AESD(K, X xor t | Ri) where t = (n * j) + i
                    byte[] b = decryptor.TransformFinalBlock(block, 0, decryptor.InputBlockSize);
                    // A = MSB(B)
                    Array.Copy(b, 0, wrapped, 0, 8);
                    // R[i] = LSB(B)
                    Array.Copy(b, 8, wrapped, i * 8, 8);
                }
            }
            return wrapped;
        }

        private static byte[] GetBigEndianBytes(long value)
        {
            byte[] bytes = new byte[sizeof(long)];

            for (int i = bytes.Length - 1; value != 0 && i >= 0; --i)
            {
                bytes[i] = (byte)value;
                value >>= 8;
            }
            return bytes;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Even if we're not using the parameter, it is part of the IDisposable pattern.")]
        private void Dispose(bool disposing)
        {
            if (_aes == null)
            {
                return;
            }
            _aes.Dispose();
            _aes = null;
        }

        ~KeyWrap()
        {
            Dispose(false);
        }

        #endregion IDisposable Members
    }
}