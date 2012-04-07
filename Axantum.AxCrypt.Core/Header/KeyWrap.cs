#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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
using System.Security.Cryptography;
using System.Text;

namespace Axantum.AxCrypt.Core.Header
{
    /// <summary>
    /// Implements AES Key Wrap Specification - http://csrc.nist.gov/groups/ST/toolkit/documents/kms/key-wrap.pdf .
    /// </summary>
    public class KeyWrap : IDisposable
    {
        private static readonly byte[] A = new byte[] { 0x0a6, 0x0a6, 0x0a6, 0x0a6, 0x0a6, 0x0a6, 0x0a6, 0x0a6 };

        private byte[] _key;
        private byte[] _salt;
        private long _iterations;
        private KeyWrapMode _mode;

        /// <summary>
        /// Create a KeyWrap instance for wrapping or unwrapping
        /// </summary>
        /// <param name="key">The key wrapping key</param>
        /// <param name="iterations">The number of wrapping iterations, at least 6</param>
        /// <param name="mode">Use original specification mode or AxCrypt mode (only difference is that 't' is little endian in AxCrypt mode)</param>
        public KeyWrap(byte[] key, long iterations, KeyWrapMode mode)
            : this(key, null, iterations, mode)
        {
        }

        /// <summary>
        /// Create a KeyWrap instance for wrapping or unwrapping
        /// </summary>
        /// <param name="key">The key wrapping key</param>
        /// <param name="salt">An optional salt, or null if none. AxCrypt uses a salt.</param>
        /// <param name="iterations">The number of wrapping iterations, at least 6</param>
        /// <param name="mode">Use original specification mode or AxCrypt mode (only difference is that 't' is little endian in AxCrypt mode)</param>
        public KeyWrap(byte[] key, byte[] salt, long iterations, KeyWrapMode mode)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (!IsKeySizeValid(key.Length * 8))
            {
                throw new ArgumentException("key length is incorrect");
            }
            if (salt != null && salt.Length != key.Length)
            {
                throw new ArgumentException("salt length is incorrect");
            }
            if (iterations < 6)
            {
                throw new ArgumentOutOfRangeException("iterations");
            }
            if (mode != KeyWrapMode.Specification && mode != KeyWrapMode.AxCrypt)
            {
                throw new ArgumentOutOfRangeException("mode");
            }
            Initialize(key, salt, iterations, mode);
        }

        private void Initialize(byte[] key, byte[] salt, long iterations, KeyWrapMode mode)
        {
            _key = (byte[])key.Clone();
            if (salt != null)
            {
                _salt = (byte[])salt.Clone();
            }
            else
            {
                _salt = new byte[0];
            }
            _iterations = iterations;

            byte[] saltedKey = (byte[])_key.Clone();
            saltedKey.Xor(_salt);

            _aes.Mode = CipherMode.ECB;
            _aes.KeySize = _key.Length * 8;
            _aes.Key = saltedKey;
            _aes.Padding = PaddingMode.None;

            _mode = mode;
        }

        private bool IsKeySizeValid(int keySizeInBits)
        {
            foreach (KeySizes keySizes in _aes.LegalKeySizes)
            {
                for (int validKeySizeInBits = keySizes.MinSize; validKeySizeInBits <= keySizes.MaxSize; validKeySizeInBits += keySizes.SkipSize)
                {
                    if (validKeySizeInBits == keySizeInBits)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private AesManaged _aes = new AesManaged();

        /// <summary>
        /// Wrap key data using the AES Key Wrap specification
        /// </summary>
        /// <param name="keyToWrap">The key to wrap</param>
        /// <returns>The wrapped key data, 8 bytes longer than the key</returns>
        public byte[] Wrap(byte[] keyToWrap)
        {
            if (keyToWrap == null)
            {
                throw new ArgumentNullException("keyToWrap");
            }
            if (keyToWrap.Length != _key.Length)
            {
                throw new ArgumentException("keyToWrap has incorrect length.");
            }

            byte[] wrapped = new byte[_key.Length + A.Length];
            A.CopyTo(wrapped, 0);

            Array.Copy(keyToWrap, 0, wrapped, A.Length, keyToWrap.Length);

            ICryptoTransform encryptor = _aes.CreateEncryptor();

            byte[] block = new byte[encryptor.InputBlockSize];

            // wrapped[0..7] contains the A (IV) of the Key Wrap algorithm,
            // the rest is 'Key Data'. We do the transform in-place.
            for (int j = 0; j < _iterations; j++)
            {
                for (int i = 1; i <= (_aes.KeySize / 8) / 8; i++)
                {
                    // B = AESE(K, A | R[i])
                    Array.Copy(wrapped, 0, block, 0, 8);
                    Array.Copy(wrapped, i * 8, block, 8, 8);
                    byte[] b = encryptor.TransformFinalBlock(block, 0, encryptor.InputBlockSize);
                    // A = MSB64(B) XOR t where t = (n * j) + i
                    long t = (((_aes.KeySize / 8) / 8) * j) + i;
                    switch (_mode)
                    {
                        case KeyWrapMode.Specification:
                            b.Xor(0, t.GetBigEndianBytes(), 0, 8);
                            break;
                        case KeyWrapMode.AxCrypt:
                            b.Xor(0, t.GetLittleEndianBytes(), 0, 8);
                            break;
                        default:
                            throw new InvalidOperationException("Unrecognized Mode");
                    }
                    Array.Copy(b, 0, wrapped, 0, 8);
                    // R[i] = LSB64(B)
                    Array.Copy(b, 8, wrapped, i * 8, 8);
                }
            }

            return wrapped;
        }

        /// <summary>
        /// Unwrap an AES Key Wrapped-key
        /// </summary>
        /// <param name="wrapped">The full wrapped data, the length of a key + 8 bytes</param>
        /// <returns>The unwrapped key data, or a zero-length array if the unwrap was unsuccessful due to wrong key</returns>
        public byte[] Unwrap(byte[] wrapped)
        {
            if (wrapped.Length != _key.Length + A.Length)
            {
                throw new ArgumentException("The length of the wrapped data must be exactly the length of a key plus 8 bytes");
            }

            wrapped = (byte[])wrapped.Clone();
            ICryptoTransform decryptor = _aes.CreateDecryptor();

            byte[] block = new byte[decryptor.InputBlockSize];

            // wrapped[0..7] contains the A (IV) of the Key Wrap algorithm,
            // the rest is 'Wrapped Key Data', R[1], ..., R[n]. We do the transform in-place.
            for (long j = _iterations - 1; j >= 0; --j)
            {
                for (int i = (_aes.KeySize / 8) / 8; i >= 1; --i)
                {
                    long t = (((_aes.KeySize / 8) / 8) * j) + i;
                    // MSB(B) = A XOR t
                    Array.Copy(wrapped, 0, block, 0, 8);
                    switch (_mode)
                    {
                        case KeyWrapMode.Specification:
                            block.Xor(0, t.GetBigEndianBytes(), 0, 8);
                            break;
                        case KeyWrapMode.AxCrypt:
                            block.Xor(0, t.GetLittleEndianBytes(), 0, 8);
                            break;
                        default:
                            throw new InvalidOperationException("Unrecognized Mode");
                    }
                    // LSB(B) = R[i]
                    Array.Copy(wrapped, i * 8, block, 8, 8);
                    // B = AESD(K, X xor t | R[i]) where t = (n * j) + i
                    byte[] b = decryptor.TransformFinalBlock(block, 0, decryptor.InputBlockSize);
                    // A = MSB(B)
                    Array.Copy(b, 0, wrapped, 0, 8);
                    // R[i] = LSB(B)
                    Array.Copy(b, 8, wrapped, i * 8, 8);
                }
            }

            if (!IsKeyUnwrapValid(wrapped))
            {
                return new byte[0];
            }

            byte[] unwrapped = new byte[_key.Length];
            Array.Copy(wrapped, A.Length, unwrapped, 0, unwrapped.Length);
            return unwrapped;
        }

        private bool IsKeyUnwrapValid(byte[] unwrapped)
        {
            if (unwrapped == null)
            {
                throw new ArgumentNullException("unwrapped");
            }
            if (unwrapped.Length != _key.Length + A.Length)
            {
                return false;
            }
            return unwrapped.IsEquivalentTo(0, A, 0, A.Length);
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
            if (!disposing)
            {
                return;
            }
            if (_aes == null)
            {
                return;
            }
            // Clear() is implemented as a call to Dispose(), but Mono does not implement Dispose(), so this avoids a MoMA warning.
            _aes.Clear();
            _aes = null;
        }

        ~KeyWrap()
        {
            Dispose(false);
        }

        #endregion IDisposable Members
    }
}