#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
 */

#endregion Coypright and License

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

using AxCrypt.Core.Runtime;

namespace AxCrypt.Core.Crypto;

/// <summary>
/// Implements AES (Generalized to any symmetric cipher) Key Wrap Specification - http://csrc.nist.gov/groups/ST/toolkit/documents/kms/key-wrap.pdf .
/// </summary>
public class KeyWrap
{
    private const int HalfBlockLength = sizeof(ulong);

    private const int ExpectedBlockLength = HalfBlockLength * 2;

    private readonly Salt _salt;

    private readonly long _keyWrapIterations;

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
        if (keyWrapIterations < 6)
        {
            throw new InternalErrorException("Key wrap iterations must be at least 6.");
        }

        if (mode != KeyWrapMode.Specification && mode != KeyWrapMode.AxCrypt)
        {
            throw new InternalErrorException("mode");
        }

        _salt = salt ?? throw new ArgumentNullException(nameof(salt));
        _mode = mode;
        _keyWrapIterations = keyWrapIterations;
    }

    public byte[] Wrap(ICrypto crypto, byte[] keyMaterial)
    {
        ArgumentNullException.ThrowIfNull(crypto);
        ArgumentNullException.ThrowIfNull(keyMaterial);

        if (crypto.BlockLength != 16)
        {
            throw new InternalErrorException("The key wrap algorithm block size must be 128 bits.");
        }


        using IKeyWrapTransform encryptor = crypto.CreateKeyWrapTransform(_salt, KeyWrapDirection.Encrypt);
        return WrapInternal(keyMaterial, encryptor);
    }

    private byte[] WrapInternal(byte[] keyMaterial, IKeyWrapTransform encryptor)
    {
        byte[] a = encryptor.A();
        int keyBlockCount = keyMaterial.Length / HalfBlockLength;

        byte[] wrapped = new byte[keyMaterial.Length + a.Length];
        Unsafe.WriteUnaligned(ref wrapped[0], Unsafe.ReadUnaligned<ulong>(ref a[0]));
        Buffer.BlockCopy(keyMaterial, 0, wrapped, HalfBlockLength, keyMaterial.Length);

        byte[] block = new byte[ExpectedBlockLength];
        // wrapped[0..HalfBlockLength-1] contains the A (IV) of the Key Wrap algorithm,
        // the rest is 'Key Data'. We do the transform in-place.
        switch (_mode)
        {
            case KeyWrapMode.Specification:
                WrapBlocksSpecification(encryptor, wrapped, block, keyBlockCount);
                return wrapped;

            case KeyWrapMode.AxCrypt:
                WrapBlocksAxCrypt(encryptor, wrapped, block, keyBlockCount);
                return wrapped;

            default:
                throw new InternalErrorException("mode");
        }
    }

    private void WrapBlocksSpecification(IKeyWrapTransform encryptor, byte[] wrapped, byte[] block, int keyBlockCount)
    {
        for (int j = 0; j < _keyWrapIterations; j++)
        {
            ulong tBase = (ulong)(keyBlockCount * j);
            for (int i = 1; i <= keyBlockCount; i++)
            {
                // B = AESE(K, A | R[i])
                Unsafe.WriteUnaligned(ref block[0], Unsafe.ReadUnaligned<ulong>(ref wrapped[0]));
                Unsafe.WriteUnaligned(ref block[HalfBlockLength], Unsafe.ReadUnaligned<ulong>(ref wrapped[i * HalfBlockLength]));
                byte[] b = encryptor.TransformBlock(block);
                // A = MSB64(B) XOR t where t = (n * j) + i
                XorBigEndian(b, 0, tBase + (ulong)i);
                Unsafe.WriteUnaligned(ref wrapped[0], Unsafe.ReadUnaligned<ulong>(ref b[0]));
                // R[i] = LSB64(B)
                Unsafe.WriteUnaligned(ref wrapped[i * HalfBlockLength], Unsafe.ReadUnaligned<ulong>(ref b[HalfBlockLength]));
            }
        }
    }

    private void WrapBlocksAxCrypt(IKeyWrapTransform encryptor, byte[] wrapped, byte[] block, int keyBlockCount)
    {
        for (int j = 0; j < _keyWrapIterations; j++)
        {
            ulong tBase = (ulong)(keyBlockCount * j);
            for (int i = 1; i <= keyBlockCount; i++)
            {
                // B = AESE(K, A | R[i])
                Unsafe.WriteUnaligned(ref block[0], Unsafe.ReadUnaligned<ulong>(ref wrapped[0]));
                Unsafe.WriteUnaligned(ref block[HalfBlockLength], Unsafe.ReadUnaligned<ulong>(ref wrapped[i * HalfBlockLength]));
                byte[] b = encryptor.TransformBlock(block);
                // A = MSB64(B) XOR t where t = (n * j) + i
                XorLittleEndian(b, 0, tBase + (ulong)i);
                Unsafe.WriteUnaligned(ref wrapped[0], Unsafe.ReadUnaligned<ulong>(ref b[0]));
                // R[i] = LSB64(B)
                Unsafe.WriteUnaligned(ref wrapped[i * HalfBlockLength], Unsafe.ReadUnaligned<ulong>(ref b[HalfBlockLength]));
            }
        }
    }

    /// <summary>
    /// Wrap key data using the AES Key Wrap specification
    /// </summary>
    /// <param name="crypto"></param>
    /// <param name="keyToWrap">The key to wrap</param>
    /// <returns>The wrapped key data, 8 bytes longer than the key</returns>
    public byte[] Wrap(ICrypto crypto, SymmetricKey keyToWrap)
    {
        ArgumentNullException.ThrowIfNull(crypto);
        ArgumentNullException.ThrowIfNull(keyToWrap);

        return Wrap(crypto, keyToWrap.GetBytes());
    }

    /// <summary>
    /// Unwrap an AES Key Wrapped-key
    /// </summary>
    /// <param name="crypto"></param>
    /// <param name="wrapped">The full wrapped data, the length of a key + 8 bytes</param>
    /// <returns>The unwrapped key data, or a zero-length array if the unwrap was unsuccessful due to wrong key</returns>
    public byte[] Unwrap(ICrypto crypto, byte[] wrapped)
    {
        ArgumentNullException.ThrowIfNull(wrapped);
        ArgumentNullException.ThrowIfNull(crypto);

        if (wrapped.Length % HalfBlockLength != 0)
        {
            throw new InternalErrorException(
                "The length of the wrapped data must a multiple of half the algorithm block size.");
        }

        if (wrapped.Length < 24)
        {
            throw new InternalErrorException(
                "The length of the wrapped data must be large enough to accommodate at least a 128-bit key.");
        }

        using IKeyWrapTransform decryptor = crypto.CreateKeyWrapTransform(_salt, KeyWrapDirection.Decrypt);
        return UnwrapInternal(wrapped, decryptor);
    }

    private byte[] UnwrapInternal(byte[] wrapped, IKeyWrapTransform decryptor)
    {
        byte[] a = decryptor.A();
        int wrappedKeyLength = wrapped.Length - a.Length;
        int keyBlockCount = wrappedKeyLength / HalfBlockLength;

        wrapped = (byte[])wrapped.Clone();

        byte[] block = new byte[ExpectedBlockLength];

        // wrapped[0..7] contains the A (IV) of the Key Wrap algorithm,
        // the rest is 'Wrapped Key Data', R[1], ..., R[n]. We do the transform in-place.
        switch (_mode)
        {
            case KeyWrapMode.Specification:
                UnwrapBlocksSpecification(decryptor, wrapped, block, keyBlockCount);
                break;

            case KeyWrapMode.AxCrypt:
                UnwrapBlocksAxCrypt(decryptor, wrapped, block, keyBlockCount);
                break;

            default:
                throw new InternalErrorException("mode");
        }

        if (Unsafe.ReadUnaligned<ulong>(ref wrapped[0]) != Unsafe.ReadUnaligned<ulong>(ref a[0]))
        {
            return [];
        }

        byte[] unwrapped = new byte[wrapped.Length - a.Length];
        Buffer.BlockCopy(wrapped, HalfBlockLength, unwrapped, 0, unwrapped.Length);
        return unwrapped;
    }

    private void UnwrapBlocksSpecification(IKeyWrapTransform decryptor, byte[] wrapped, byte[] block, int keyBlockCount)
    {
        for (int j = (int)_keyWrapIterations - 1; j >= 0; --j)
        {
            ulong tBase = (ulong)(keyBlockCount * j);
            for (int i = keyBlockCount; i >= 1; --i)
            {
                // MSB(B) = A XOR t
                Unsafe.WriteUnaligned(ref block[0], Unsafe.ReadUnaligned<ulong>(ref wrapped[0]));
                XorBigEndian(block, 0, tBase + (ulong)i);
                // LSB(B) = R[i]
                Unsafe.WriteUnaligned(ref block[HalfBlockLength], Unsafe.ReadUnaligned<ulong>(ref wrapped[i * HalfBlockLength]));
                // B = AESD(K, X xor t | R[i]) where t = (n * j) + i
                byte[] b = decryptor.TransformBlock(block);
                // A = MSB(B)
                Unsafe.WriteUnaligned(ref wrapped[0], Unsafe.ReadUnaligned<ulong>(ref b[0]));
                // R[i] = LSB(B)
                Unsafe.WriteUnaligned(ref wrapped[i * HalfBlockLength], Unsafe.ReadUnaligned<ulong>(ref b[HalfBlockLength]));
            }
        }
    }

    private void UnwrapBlocksAxCrypt(IKeyWrapTransform decryptor, byte[] wrapped, byte[] block, int keyBlockCount)
    {
        for (int j = (int)_keyWrapIterations - 1; j >= 0; --j)
        {
            ulong tBase = (ulong)(keyBlockCount * j);
            for (int i = keyBlockCount; i >= 1; --i)
            {
                // MSB(B) = A XOR t
                Unsafe.WriteUnaligned(ref block[0], Unsafe.ReadUnaligned<ulong>(ref wrapped[0]));
                XorLittleEndian(block, 0, tBase + (ulong)i);
                // LSB(B) = R[i]
                Unsafe.WriteUnaligned(ref block[HalfBlockLength], Unsafe.ReadUnaligned<ulong>(ref wrapped[i * HalfBlockLength]));
                // B = AESD(K, X xor t | R[i]) where t = (n * j) + i
                byte[] b = decryptor.TransformBlock(block);
                // A = MSB(B)
                Unsafe.WriteUnaligned(ref wrapped[0], Unsafe.ReadUnaligned<ulong>(ref b[0]));
                // R[i] = LSB(B)
                Unsafe.WriteUnaligned(ref wrapped[i * HalfBlockLength], Unsafe.ReadUnaligned<ulong>(ref b[HalfBlockLength]));
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void XorBigEndian(byte[] buffer, int offset, ulong value) =>
        Unsafe.WriteUnaligned(ref buffer[offset],
            Unsafe.ReadUnaligned<ulong>(ref buffer[offset]) ^ BinaryPrimitives.ReverseEndianness(value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void XorLittleEndian(byte[] buffer, int offset, ulong value) =>
        Unsafe.WriteUnaligned(ref buffer[offset], Unsafe.ReadUnaligned<ulong>(ref buffer[offset]) ^ value);
}
