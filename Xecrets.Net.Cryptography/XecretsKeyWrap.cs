#region Coypright and GPL License

/*
 * Xecrets.Net - Copyright © 2022-2026, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets.Net, parts of which in turn are derived from AxCrypt as licensed under GPL v3 or later.
 *
 * However, this code is not derived from AxCrypt and is separately copyrighted and only licensed as follows unless
 * explicitly licensed otherwise. If you use any part of this code in your software, please see https://www.gnu.org/licenses/
 * for details of what this means for you.
 *
 * Xecrets.Net is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets.Net is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets.Net.  If not, see <https://www.gnu.org/licenses/>.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Coypright and GPL License

using System.Buffers.Binary;
using System.Security.Cryptography;

namespace Xecrets.Net.Cryptography;

// This replaces AxCrypt.Core.Crypto.KeyWrap. It implements the AES Key Wrap of RFC 3394 / NIST SP 800-38F (KW),
// generalized to an arbitrary number of iterations (the specification uses 6), and optionally with the counter 't'
// XOR-ed in little endian byte order as in AxCrypt V1. Any salting of the key encrypting key is done by the caller.
public class XecretsKeyWrap
{
    private const int HalfBlockLength = sizeof(ulong);

    private const int BlockLength = HalfBlockLength * 2;

    // The default initial value of RFC 3394 section 2.2.3.1.
    private const ulong DefaultIv = 0xA6A6A6A6A6A6A6A6;

    private readonly long _keyWrapIterations;

    private readonly bool _littleEndianCounter;

    public XecretsKeyWrap(long keyWrapIterations, bool littleEndianCounter)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(keyWrapIterations, 6);

        _keyWrapIterations = keyWrapIterations;
        _littleEndianCounter = littleEndianCounter;
    }

    public byte[] Wrap(ReadOnlySpan<byte> keyEncryptingKey, ReadOnlySpan<byte> keyMaterial)
    {
        ValidateKeyEncryptingKey(keyEncryptingKey);
        if (keyMaterial.Length < 2 * HalfBlockLength || keyMaterial.Length % HalfBlockLength != 0)
        {
            throw new ArgumentException("The key material must be a multiple of 64 bits, and at least 128 bits.", nameof(keyMaterial));
        }

        int n = keyMaterial.Length / HalfBlockLength;
        byte[] wrapped = new byte[keyMaterial.Length + HalfBlockLength];
        keyMaterial.CopyTo(wrapped.AsSpan(HalfBlockLength));

        using ICryptoTransform encryptor = CreateTransform(keyEncryptingKey, encrypt: true);

        byte[] block = new byte[BlockLength];
        byte[] b = new byte[BlockLength];
        ulong a = DefaultIv;
        try
        {
            for (long j = 0; j < _keyWrapIterations; ++j)
            {
                ulong tBase = (ulong)(n * j);
                for (int i = 1; i <= n; ++i)
                {
                    // B = AES(K, A | R[i])
                    Span<byte> r = wrapped.AsSpan(i * HalfBlockLength, HalfBlockLength);
                    BinaryPrimitives.WriteUInt64BigEndian(block, a);
                    r.CopyTo(block.AsSpan(HalfBlockLength));
                    _ = encryptor.TransformBlock(block, 0, BlockLength, b, 0);

                    // A = MSB64(B) XOR t where t = (n * j) + i
                    a = BinaryPrimitives.ReadUInt64BigEndian(b) ^ Counter(tBase + (ulong)i);

                    // R[i] = LSB64(B)
                    b.AsSpan(HalfBlockLength).CopyTo(r);
                }
            }
            BinaryPrimitives.WriteUInt64BigEndian(wrapped, a);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(block);
            CryptographicOperations.ZeroMemory(b);
        }

        return wrapped;
    }

    public byte[] Unwrap(ReadOnlySpan<byte> keyEncryptingKey, ReadOnlySpan<byte> wrapped)
    {
        ValidateKeyEncryptingKey(keyEncryptingKey);
        if (wrapped.Length < 3 * HalfBlockLength || wrapped.Length % HalfBlockLength != 0)
        {
            throw new ArgumentException("The wrapped data must be a multiple of 64 bits, and at least 192 bits.", nameof(wrapped));
        }

        int n = wrapped.Length / HalfBlockLength - 1;
        byte[] unwrapped = wrapped[HalfBlockLength..].ToArray();

        using ICryptoTransform decryptor = CreateTransform(keyEncryptingKey, encrypt: false);

        byte[] block = new byte[BlockLength];
        byte[] b = new byte[BlockLength];
        ulong a = BinaryPrimitives.ReadUInt64BigEndian(wrapped);
        try
        {
            for (long j = _keyWrapIterations - 1; j >= 0; --j)
            {
                ulong tBase = (ulong)(n * j);
                for (int i = n; i >= 1; --i)
                {
                    // B = AES-1(K, (A XOR t) | R[i]) where t = (n * j) + i
                    Span<byte> r = unwrapped.AsSpan((i - 1) * HalfBlockLength, HalfBlockLength);
                    BinaryPrimitives.WriteUInt64BigEndian(block, a ^ Counter(tBase + (ulong)i));
                    r.CopyTo(block.AsSpan(HalfBlockLength));
                    _ = decryptor.TransformBlock(block, 0, BlockLength, b, 0);

                    // A = MSB64(B)
                    a = BinaryPrimitives.ReadUInt64BigEndian(b);

                    // R[i] = LSB64(B)
                    b.AsSpan(HalfBlockLength).CopyTo(r);
                }
            }
        }
        finally
        {
            CryptographicOperations.ZeroMemory(block);
            CryptographicOperations.ZeroMemory(b);
        }

        Span<byte> expectedIv = stackalloc byte[HalfBlockLength];
        Span<byte> actualIv = stackalloc byte[HalfBlockLength];
        BinaryPrimitives.WriteUInt64BigEndian(expectedIv, DefaultIv);
        BinaryPrimitives.WriteUInt64BigEndian(actualIv, a);
        if (!CryptographicOperations.FixedTimeEquals(actualIv, expectedIv))
        {
            CryptographicOperations.ZeroMemory(unwrapped);
            return [];
        }

        return unwrapped;
    }

    // The counter is XOR-ed into A, which is handled as a big endian value. The specification XORs it as a big
    // endian value, while AxCrypt V1 XORs it in little endian byte order.
    private ulong Counter(ulong t) => _littleEndianCounter ? BinaryPrimitives.ReverseEndianness(t) : t;

    // A single long-lived transform is used for all blocks. The one-shot Aes.EncryptEcb()/DecryptEcb() set up the
    // native cipher anew for every call, which makes them several times slower for single block operations.
    private static ICryptoTransform CreateTransform(ReadOnlySpan<byte> keyEncryptingKey, bool encrypt)
    {
        byte[] key = keyEncryptingKey.ToArray();
        try
        {
            using Aes aes = Aes.Create();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;
            return encrypt ? aes.CreateEncryptor(key, null) : aes.CreateDecryptor(key, null);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(key);
        }
    }

    private static void ValidateKeyEncryptingKey(ReadOnlySpan<byte> keyEncryptingKey)
    {
        if (keyEncryptingKey.Length != 16 && keyEncryptingKey.Length != 24 && keyEncryptingKey.Length != 32)
        {
            throw new ArgumentException("The key encrypting key must be 128, 192 or 256 bits.", nameof(keyEncryptingKey));
        }
    }
}
