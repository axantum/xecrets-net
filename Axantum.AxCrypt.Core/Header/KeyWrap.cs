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
 * The source is maintained at http://AxCrypt.codeplex.com/ please visit for
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
        private static readonly byte[] _a = new byte[] { 0x0a6, 0x0a6, 0x0a6, 0x0a6, 0x0a6, 0x0a6, 0x0a6, 0x0a6 };
        private const int KEY_BITS = 128;

        private byte[] _key;
        private byte[] _salt;
        private long _iterations;
        private KeyWrapMode _mode;
        private bool _isLittleEndian;

        public KeyWrap(byte[] key, byte[] salt, long iterations, KeyWrapMode mode)
            : this(key, salt, iterations, mode, Environment.Current)
        {
        }

        protected KeyWrap(byte[] key, byte[] salt, long iterations, KeyWrapMode mode, IEnvironment environment)
        {
            _key = (byte[])key.Clone();
            _salt = (byte[])salt.Clone();
            _iterations = iterations;

            byte[] saltedKey = (byte[])_key.Clone();
            saltedKey.Xor(_salt);
            Aes.Key = saltedKey;
            _mode = mode;

            _isLittleEndian = environment.IsLittleEndian;
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
                    _aes.KeySize = KEY_BITS;
                    _aes.Padding = PaddingMode.None;
                }
                return _aes;
            }
        }

        public byte[] Unwrap(byte[] wrapped)
        {
            if (wrapped.Length != KEY_BITS / 8 + _a.Length)
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
                    switch (_mode)
                    {
                        case KeyWrapMode.Specification:
                            block.Xor(0, GetBigEndianBytes(t), 0, 8);
                            break;
                        case KeyWrapMode.AxCrypt:
                            block.Xor(0, GetLittleEndianBytes(t), 0, 8);
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
            return wrapped;
        }

        public static bool IsKeyUnwrapValid(byte[] unwrapped)
        {
            if (unwrapped == null)
            {
                throw new ArgumentNullException("unwrapped");
            }
            if (unwrapped.Length != KEY_BITS / 8 + 8)
            {
                return false;
            }
            return unwrapped.IsEquivalentTo(0, _a, 0, _a.Length);
        }

        internal static byte[] GetKeyBytes(byte[] unwrapped)
        {
            if (unwrapped == null)
            {
                throw new ArgumentNullException("unwrapped");
            }
            if (unwrapped.Length != KEY_BITS / 8 + 8)
            {
                throw new ArgumentException("unwrapped has incorrect length");
            }
            byte[] keyBytes = new byte[KEY_BITS / 8];
            Array.Copy(unwrapped, 8, keyBytes, 0, keyBytes.Length);
            return keyBytes;
        }

        protected byte[] GetBigEndianBytes(long value)
        {
            if (!_isLittleEndian)
            {
                return BitConverter.GetBytes(value);
            }

            byte[] bytes = new byte[sizeof(ulong)];

            for (int i = bytes.Length - 1; value != 0 && i >= 0; --i)
            {
                bytes[i] = (byte)value;
                value >>= 8;
            }
            return bytes;
        }

        protected byte[] GetLittleEndianBytes(long value)
        {
            if (_isLittleEndian)
            {
                return BitConverter.GetBytes(value);
            }

            byte[] bytes = new byte[sizeof(ulong)];

            for (int i = 0; value != 0 && i < bytes.Length; ++i)
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