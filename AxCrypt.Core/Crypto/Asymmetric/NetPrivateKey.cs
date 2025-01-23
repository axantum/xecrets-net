#region Copyright and GPL License

/*
 * Xecrets Cli - Copyright © 2022-2025, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets Cli, parts of which in turn are derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * However, this code is not derived from AxCrypt and is separately copyrighted and only licensed as follows unless
 * explicitly licensed otherwise. If you use any part of this code in your software, please see https://www.gnu.org/licenses/
 * for details of what this means for you.
 *
 * Xecrets Cli is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets Cli is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets Cli.  If not, see <https://www.gnu.org/licenses/>.
 *
 * The source repository can be found at https://github.com/ please go there for more information, suggestions and
 * contributions. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Copyright and GPL License

using System.Numerics;
using System.Security.Cryptography;

using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto.Asymmetric;

using static AxCrypt.Abstractions.TypeResolve;

namespace Xecrets.Net.Core.Crypto.Asymmetric;

internal class NetPrivateKey : IAsymmetricPrivateKey
{
    private readonly RSA _rsa;

    public NetPrivateKey(byte[] pkcs1bytes)
    {
        _rsa = RSA.Create();
        _rsa.ImportRSAPrivateKey(pkcs1bytes, out _);
    }

    public NetPrivateKey(string pem)
    {
        _rsa = RSA.Create();
        _rsa.ImportFromPem(pem);
    }

    public byte[]? TransformRaw(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        int maxByteLength = (_rsa.KeySize + 7) / 8;
        if (buffer.Length > maxByteLength)
        {
            throw new ArgumentException("Data is too long for the key size.", nameof(buffer));
        }
        if (buffer.Length < maxByteLength)
        {
            byte[] block = new byte[maxByteLength];
            Array.Copy(buffer, 0, block, maxByteLength - buffer.Length, buffer.Length);
            buffer = block;
        }

        RSAParameters parameters = _rsa?.ExportParameters(includePrivateParameters: true) ?? throw new InvalidOperationException("RSA key not initialized.");

        return Decrypt(buffer, parameters);
    }

    private static byte[] Decrypt(byte[] data, RSAParameters rsaParameters)
    {

        BigInteger modulus = new BigInteger(rsaParameters.Modulus, isUnsigned: true, isBigEndian: true);
        BigInteger d = new BigInteger(rsaParameters.D, isUnsigned: true, isBigEndian: true);
        BigInteger encryptedMessage = new BigInteger(data, isUnsigned: true, isBigEndian: true);

        BigInteger decryptedMessage = BigInteger.ModPow(encryptedMessage, d, modulus);
        return decryptedMessage.ToByteArray(isUnsigned: true, isBigEndian: true);
    }

    public byte[]? Transform(byte[] buffer)
    {
        if (buffer == null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        int rsaKeyBitLength = _rsa.KeySize;
        RSAEncryptionPadding padding;
        if (rsaKeyBitLength < 1024)
        {
            padding = RSAEncryptionPadding.CreateOaep(HashAlgorithmName.MD5);
        }
        else if (rsaKeyBitLength < 2048)
        {
            padding = RSAEncryptionPadding.OaepSHA256;
        }
        else
        {
            padding = RSAEncryptionPadding.OaepSHA512;
        }

        try
        {
            byte[] transformed = _rsa.Decrypt(buffer.AsSpan().Slice(start: 0, length: (rsaKeyBitLength + 7) / 8), padding);
            return transformed;
        }
        catch (CryptographicException cex)
        {
            New<IReport>().Exception(cex);
            return null;
        }
    }

    public override string ToString()
    {
        return _rsa.ExportRSAPrivateKeyPem().Replace("\n", Environment.NewLine) + Environment.NewLine;
    }

    public bool Equals(IAsymmetricPrivateKey? other)
    {
        if (other == null)
        {
            return false;
        }

        NetPrivateKey otherKey = (NetPrivateKey)other;
        RSAParameters otherParameters = otherKey._rsa.ExportParameters(includePrivateParameters: true);
        RSAParameters parameters = _rsa.ExportParameters(includePrivateParameters: true);

        return AreRsaParametersEqual(parameters, otherParameters);
    }

    private static bool AreRsaParametersEqual(RSAParameters params1, RSAParameters params2)
    {
        static bool AreEqual(byte[]? array1, byte[]? array2)
        {
            if (array1 == null && array2 == null)
            {
                return true;
            }

            if (array1 == null || array2 == null)
            {
                return false;
            }

            return array1.SequenceEqual(array2);
        }

        return AreEqual(params1.Modulus, params2.Modulus) &&
               AreEqual(params1.Exponent, params2.Exponent) &&
               AreEqual(params1.D, params2.D) &&
               AreEqual(params1.P, params2.P) &&
               AreEqual(params1.Q, params2.Q) &&
               AreEqual(params1.DP, params2.DP) &&
               AreEqual(params1.DQ, params2.DQ) &&
               AreEqual(params1.InverseQ, params2.InverseQ);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not IAsymmetricPrivateKey other)
        {
            return false;
        }

        return Equals(other);
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public static bool operator ==(NetPrivateKey left, NetPrivateKey right)
    {
        if (left is null)
        {
            return ReferenceEquals(right, null);
        }

        return left.Equals(right);
    }

    public static bool operator !=(NetPrivateKey left, NetPrivateKey right)
    {
        return !(left == right);
    }
}
