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

namespace Axantum.AxCrypt.Core.Crypto
{
    /// <summary>
    /// Implements AES (Generalized to any symmetric cipher) Key Wrap Specification - http://csrc.nist.gov/groups/ST/toolkit/documents/kms/key-wrap.pdf .
    /// </summary>
    public class KeyWrap
    {
        private Salt _salt;

        private long _keyWrapIterations;

        private readonly KeyWrapMode _mode;

        /// <summary>
        /// Create a KeyWrap instance for wrapping or unwrapping
        /// </summary>
        /// <param name="keyWrapIterations">The number of wrapping iterations, at least 6</param>
        /// <param name="mode">Use original specification mode or AxCrypt mode (only difference is that 't' is little endian in AxCrypt mode)</param>
        public KeyWrap(long keyWrapIterations, KeyWrapMode mode)
            : this(Salt.Zero, keyWrapIterations, mode)
        {
        }

        /// <summary>
        /// Create a KeyWrap instance for wrapping or unwrapping
        /// </summary>
        /// <param name="salt">A salt. This is required by AxCrypt, although the algorithm supports not using a salt.</param>
        /// <param name="keyWrapIterations">The number of wrapping iterations, at least 6</param>
        /// <param name="mode">Use original specification mode or AxCrypt mode (only difference is that 't' is little endian in AxCrypt mode)</param>
        public KeyWrap(Salt salt, long keyWrapIterations, KeyWrapMode mode)
        {
            if (salt == null)
            {
                throw new ArgumentNullException("salt");
            }
            if (keyWrapIterations < 6)
            {
                throw new InternalErrorException("Key wrap iterations must be at least 6.");
            }
            if (mode != KeyWrapMode.Specification && mode != KeyWrapMode.AxCrypt)
            {
                throw new InternalErrorException("mode");
            }
            _salt = salt;
            _mode = mode;
            _keyWrapIterations = keyWrapIterations;
        }

        public byte[] Wrap(ICrypto crypto, byte[] keyMaterial)
        {
            if (crypto == null)
            {
                throw new ArgumentNullException("crypto");
            }
            if (keyMaterial == null)
            {
                throw new ArgumentNullException("keyMaterial");
            }

            using (IKeyWrapTransform encryptor = crypto.CreateKeyWrapTransform(_salt, KeyWrapDirection.Encrypt))
            {
                return WrapInternal(keyMaterial, encryptor);
            }
        }

        private byte[] WrapInternal(byte[] keyMaterial, IKeyWrapTransform encryptor)
        {
            byte[] a = encryptor.A();

            byte[] wrapped = new byte[keyMaterial.Length + a.Length];
            a.CopyTo(wrapped, 0);

            Array.Copy(keyMaterial, 0, wrapped, a.Length, keyMaterial.Length);

            byte[] block = new byte[encryptor.BlockLength];
            int halfBlockLength = encryptor.BlockLength / 2;
            // wrapped[0..halfBlockLength-1] contains the A (IV) of the Key Wrap algorithm,
            // the rest is 'Key Data'. We do the transform in-place.
            for (int j = 0; j < _keyWrapIterations; j++)
            {
                for (int i = 1; i <= keyMaterial.Length / halfBlockLength; i++)
                {
                    // B = AESE(K, A | R[i])
                    Array.Copy(wrapped, 0, block, 0, halfBlockLength);
                    Array.Copy(wrapped, i * halfBlockLength, block, halfBlockLength, halfBlockLength);
                    byte[] b = encryptor.TransformBlock(block);
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
        public byte[] Wrap(ICrypto crypto, SymmetricKey keyToWrap)
        {
            if (crypto == null)
            {
                throw new ArgumentNullException("crypto");
            }
            if (keyToWrap == null)
            {
                throw new ArgumentNullException("keyToWrap");
            }
            return Wrap(crypto, keyToWrap.GetBytes());
        }

        /// <summary>
        /// Unwrap an AES Key Wrapped-key
        /// </summary>
        /// <param name="wrapped">The full wrapped data, the length of a key + 8 bytes</param>
        /// <returns>The unwrapped key data, or a zero-length array if the unwrap was unsuccessful due to wrong key</returns>
        public byte[] Unwrap(ICrypto crypto, byte[] wrapped)
        {
            if (wrapped == null)
            {
                throw new ArgumentNullException("wrapped");
            }

            if (crypto == null)
            {
                throw new ArgumentNullException("crypto");
            }
            if (wrapped.Length % (crypto.BlockLength / 2) != 0)
            {
                throw new InternalErrorException("The length of the wrapped data must a multiple of half the algorithm block size.");
            }
            if (wrapped.Length < 24)
            {
                throw new InternalErrorException("The length of the wrapped data must be large enough to accommodate at least a 128-bit key.");
            }

            using (IKeyWrapTransform decryptor = crypto.CreateKeyWrapTransform(_salt, KeyWrapDirection.Decrypt))
            {
                return UnwrapInternal(wrapped, decryptor);
            }
        }

        private byte[] UnwrapInternal(byte[] wrapped, IKeyWrapTransform decryptor)
        {
            byte[] a = decryptor.A();
            int halfBlockLength = decryptor.BlockLength / 2;
            int wrappedKeyLength = wrapped.Length - a.Length;

            wrapped = (byte[])wrapped.Clone();

            byte[] block = new byte[decryptor.BlockLength];

            // wrapped[0..7] contains the A (IV) of the Key Wrap algorithm,
            // the rest is 'Wrapped Key Data', R[1], ..., R[n]. We do the transform in-place.
            for (long j = _keyWrapIterations - 1; j >= 0; --j)
            {
                for (int i = wrappedKeyLength / halfBlockLength; i >= 1; --i)
                {
                    long t = ((wrappedKeyLength / halfBlockLength) * j) + i;
                    // MSB(B) = A XOR t
                    Array.Copy(wrapped, 0, block, 0, halfBlockLength);
                    switch (_mode)
                    {
                        case KeyWrapMode.Specification:
                            block.Xor(0, t.GetBigEndianBytes(), 0, halfBlockLength);
                            break;

                        case KeyWrapMode.AxCrypt:
                            block.Xor(0, t.GetLittleEndianBytes(), 0, halfBlockLength);
                            break;
                    }
                    // LSB(B) = R[i]
                    Array.Copy(wrapped, i * halfBlockLength, block, halfBlockLength, halfBlockLength);
                    // B = AESD(K, X xor t | R[i]) where t = (n * j) + i
                    byte[] b = decryptor.TransformBlock(block);
                    // A = MSB(B)
                    Array.Copy(b, 0, wrapped, 0, halfBlockLength);
                    // R[i] = LSB(B)
                    Array.Copy(b, halfBlockLength, wrapped, i * halfBlockLength, halfBlockLength);
                }
            }

            if (!wrapped.IsEquivalentTo(0, a, 0, a.Length))
            {
                return new byte[0];
            }

            byte[] unwrapped = new byte[wrapped.Length - a.Length];
            Array.Copy(wrapped, a.Length, unwrapped, 0, wrapped.Length - a.Length);
            return unwrapped;
        }
    }
}