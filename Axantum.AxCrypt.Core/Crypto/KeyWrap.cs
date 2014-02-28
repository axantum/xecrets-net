#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace Axantum.AxCrypt.Core.Crypto
{
    /// <summary>
    /// Implements AES (Generalized to any symmetric cipher) Key Wrap Specification - http://csrc.nist.gov/groups/ST/toolkit/documents/kms/key-wrap.pdf .
    /// </summary>
    public class KeyWrap : IDisposable
    {
        private readonly byte[] _A;

        private long _iterations;
        private readonly KeyWrapMode _mode;
        private SymmetricAlgorithm _algorithm;

        /// <summary>
        /// Create a KeyWrap instance for wrapping or unwrapping
        /// </summary>
        /// <param name="key">The key wrapping key</param>
        /// <param name="iterations">The number of wrapping iterations, at least 6</param>
        /// <param name="mode">Use original specification mode or AxCrypt mode (only difference is that 't' is little endian in AxCrypt mode)</param>
        public KeyWrap(ICrypto crypto, long iterations, KeyWrapMode mode)
            : this(crypto, KeyWrapSalt.Zero, iterations, mode)
        {
        }

        /// <summary>
        /// Create a KeyWrap instance for wrapping or unwrapping
        /// </summary>
        /// <param name="key">The key wrapping key</param>
        /// <param name="salt">A salt. This is required by AxCrypt, although the algorithm supports not using a salt.</param>
        /// <param name="iterations">The number of wrapping iterations, at least 6</param>
        /// <param name="mode">Use original specification mode or AxCrypt mode (only difference is that 't' is little endian in AxCrypt mode)</param>
        public KeyWrap(ICrypto crypto, KeyWrapSalt salt, long iterations, KeyWrapMode mode)
        {
            if (crypto == null)
            {
                throw new ArgumentNullException("crypto");
            }
            if (salt == null)
            {
                throw new ArgumentNullException("salt");
            }
            _algorithm = crypto.CreateAlgorithm();
            if (salt.Length != 0 && salt.Length < _algorithm.Key.Length)
            {
                throw new InternalErrorException("salt length is incorrect");
            }
            if (iterations < 6)
            {
                throw new InternalErrorException("iterations");
            }
            if (mode != KeyWrapMode.Specification && mode != KeyWrapMode.AxCrypt)
            {
                throw new InternalErrorException("mode");
            }
            _mode = mode;

            _iterations = iterations;

            byte[] saltedKey = _algorithm.Key;
            saltedKey.Xor(salt.GetBytes().Reduce(_algorithm.Key.Length));

            _algorithm.Mode = CipherMode.ECB;
            _algorithm.KeySize = saltedKey.Length * 8;
            _algorithm.Key = saltedKey;
            _algorithm.Padding = PaddingMode.None;

            _A = new byte[_algorithm.BlockSize / 8 / 2];
            for (int i = 0; i < _A.Length; ++i)
            {
                _A[i] = 0xA6;
            }
        }

        public int BlockSize
        {
            get { return _algorithm.BlockSize / 8; }
        }

        public byte[] Wrap(byte[] keyMaterial)
        {
            if (keyMaterial == null)
            {
                throw new ArgumentNullException("keyMaterial");
            }
            if (_algorithm == null)
            {
                throw new ObjectDisposedException("_algorithm");
            }

            byte[] wrapped = new byte[keyMaterial.Length + _A.Length];
            _A.CopyTo(wrapped, 0);

            Array.Copy(keyMaterial, 0, wrapped, _A.Length, keyMaterial.Length);

            ICryptoTransform encryptor = _algorithm.CreateEncryptor();

            byte[] block = new byte[encryptor.InputBlockSize];
            int halfBlockLength = block.Length / 2;
            // wrapped[0..halfBlockLength-1] contains the A (IV) of the Key Wrap algorithm,
            // the rest is 'Key Data'. We do the transform in-place.
            for (int j = 0; j < _iterations; j++)
            {
                for (int i = 1; i <= keyMaterial.Length / halfBlockLength; i++)
                {
                    // B = AESE(K, A | R[i])
                    Array.Copy(wrapped, 0, block, 0, halfBlockLength);
                    Array.Copy(wrapped, i * halfBlockLength, block, halfBlockLength, halfBlockLength);
                    byte[] b = encryptor.TransformFinalBlock(block, 0, encryptor.InputBlockSize);
                    // A = MSB64(B) XOR t where t = (n * j) + i
                    long t = ((keyMaterial.Length / halfBlockLength) * j) + i;
                    switch (_mode)
                    {
                        case KeyWrapMode.Specification:
                            b.Xor(0, t.GetBigEndianBytes(), 0, halfBlockLength);
                            break;

                        case KeyWrapMode.AxCrypt:
                            b.Xor(0, t.GetLittleEndianBytes(), 0, halfBlockLength);
                            break;
                    }
                    Array.Copy(b, 0, wrapped, 0, halfBlockLength);
                    // R[i] = LSB64(B)
                    Array.Copy(b, halfBlockLength, wrapped, i * halfBlockLength, halfBlockLength);
                }
            }

            return wrapped;
        }

        /// <summary>
        /// Wrap key data using the AES Key Wrap specification
        /// </summary>
        /// <param name="keyToWrap">The key to wrap</param>
        /// <returns>The wrapped key data, 8 bytes longer than the key</returns>
        public byte[] Wrap(SymmetricKey keyToWrap)
        {
            if (keyToWrap == null)
            {
                throw new ArgumentNullException("keyToWrap");
            }
            if (_algorithm == null)
            {
                throw new ObjectDisposedException("_algorithm");
            }
            return Wrap(keyToWrap.GetBytes());
        }

        /// <summary>
        /// Unwrap an AES Key Wrapped-key
        /// </summary>
        /// <param name="wrapped">The full wrapped data, the length of a key + 8 bytes</param>
        /// <returns>The unwrapped key data, or a zero-length array if the unwrap was unsuccessful due to wrong key</returns>
        public byte[] Unwrap(byte[] wrapped)
        {
            if (_algorithm == null)
            {
                throw new ObjectDisposedException("_algorithm");
            }
            if (wrapped.Length % 8 != 0)
            {
                throw new InternalErrorException("The length of the wrapped data must a multiple of 8 bytes.");
            }
            if (wrapped.Length < 24)
            {
                throw new InternalErrorException("The length of the wrapped data must be large enough to accomdate at least a 128-bit key.");
            }

            int wrappedKeyLength = wrapped.Length - _A.Length;

            wrapped = (byte[])wrapped.Clone();
            ICryptoTransform decryptor = _algorithm.CreateDecryptor();

            byte[] block = new byte[decryptor.InputBlockSize];

            // wrapped[0..7] contains the A (IV) of the Key Wrap algorithm,
            // the rest is 'Wrapped Key Data', R[1], ..., R[n]. We do the transform in-place.
            for (long j = _iterations - 1; j >= 0; --j)
            {
                for (int i = wrappedKeyLength / 8; i >= 1; --i)
                {
                    long t = ((wrappedKeyLength / 8) * j) + i;
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

            if (!wrapped.IsEquivalentTo(0, _A, 0, _A.Length))
            {
                return new byte[0];
            }

            byte[] unwrapped = new byte[wrapped.Length - _A.Length];
            Array.Copy(wrapped, _A.Length, unwrapped, 0, wrapped.Length - _A.Length);
            return unwrapped;
        }

        #region IDisposable Members

        /// <summary>
        /// Performs required tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
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
            if (_algorithm == null)
            {
                return;
            }
            // Clear() is implemented as a call to Dispose(), but Mono does not implement Dispose(), so this avoids a MoMA warning.
            _algorithm.Clear();
            _algorithm = null;
        }

        ~KeyWrap()
        {
            Dispose(false);
        }

        #endregion IDisposable Members
    }
}