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

using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;

using AxCrypt.Core.Crypto.Asymmetric;

namespace Xecrets.Net.Core.Crypto.Asymmetric;

internal class NetPublicKey : IAsymmetricPublicKey
{
    private readonly RSA? _rsa;

    private readonly Exception? _ex;

    public NetPublicKey(byte[] pkcs1bytes)
    {
        _rsa = RSA.Create();
        _rsa.ImportRSAPublicKey(pkcs1bytes, out _);
    }

    public NetPublicKey(string pem)
    {
        try
        {
            _rsa = RSA.Create();
            _rsa.ImportFromPem(pem);
        }
        catch (Exception ex)
        {
            _rsa = null;
            _ex = ex;
        }
    }

    public string Tag
    {
        get
        {
            int notSoImportantToBeUniqueTag = GetHashCode();
            if (notSoImportantToBeUniqueTag < 0)
            {
                notSoImportantToBeUniqueTag = -(notSoImportantToBeUniqueTag + 1);
            }

            return notSoImportantToBeUniqueTag.ToString(CultureInfo.InvariantCulture);
        }
    }

    private PublicKeyThumbprint? _thumbprint = null;
    private readonly byte[] byteZero = [0];

    /// <summary>
    /// Gets the thumbprint of the public key.
    /// </summary>
    /// <value>
    /// The thumbprint.
    /// </value>
    public PublicKeyThumbprint Thumbprint
    {
        get
        {
            if (_thumbprint == null)
            {
                RSAParameters rsaParameters = _rsa?.ExportParameters(false) ?? throw _ex!;
                byte[] modulus = rsaParameters.Modulus!;
                // Legacy BouncyCastle implementation inserts a zero byte first,
                // because the BigInteger conversion to byte array for unsigned
                // values adds one bit required, thus the first byte is zero if
                // the modules is large enough. This is probably a mistake in
                // the original implementation, but we strive to be as
                // compatible as we can be. The legacy code only does this for
                // the Thumbprint, and it should not be critical if the
                // implementations differ in some edge cases.
                if (modulus.Length == _rsa.KeySize / 8 && modulus[0] >= 0x80)
                {
                    // Add the extra byte (always zero) to the modulus (as in the legacy BouncyCastle implementation)
                    modulus = [.. byteZero, .. modulus];
                }
                byte[] exponent = rsaParameters.Exponent!;
                _thumbprint = new PublicKeyThumbprint(modulus, exponent);
            }
            return _thumbprint;
        }
    }

    public byte[] Transform(byte[] buffer)
    {
        if (buffer == null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        int rsaKeyBitLength = _rsa?.KeySize ?? throw _ex!;
        RSAEncryptionPadding padding;
        int hashSize;
        if (rsaKeyBitLength < 1024)
        {
            padding = RSAEncryptionPadding.CreateOaep(HashAlgorithmName.MD5);
            hashSize = 128 / 8;
        }
        else if (rsaKeyBitLength < 2048)
        {
            padding = RSAEncryptionPadding.OaepSHA256;
            hashSize = 256 / 8;
        }
        else
        {
            padding = RSAEncryptionPadding.OaepSHA512;
            hashSize = 512 / 8;
        }

        int requiredBits = (hashSize * 2 + buffer.Length + 1) * 8;
        if (requiredBits > rsaKeyBitLength)
        {
            throw new InvalidOperationException("The RSA Key size is too small to fit the data + 1 + 2 times the padding hash size.");
        }

        byte[] transformed = _rsa.Encrypt(buffer, padding);
        return transformed;
    }

    public byte[] TransformRaw(byte[] buffer, int outputLength)
    {
        RSAParameters parameters = _rsa?.ExportParameters(includePrivateParameters: false) ?? throw new InvalidOperationException("RSA key not initialized.");
        return Encrypt(buffer, parameters);
    }

    private static byte[] Encrypt(byte[] data, RSAParameters rsaParameters)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        BigInteger modulus = new BigInteger(rsaParameters.Modulus, isUnsigned: true, isBigEndian: true);
        BigInteger exponent = new BigInteger(rsaParameters.Exponent, isUnsigned: true, isBigEndian: true);
        BigInteger message = new BigInteger(data, isUnsigned: true, isBigEndian: true);

        BigInteger encryptedMessage = BigInteger.ModPow(message, exponent, modulus);
        return encryptedMessage.ToByteArray(isUnsigned: true, isBigEndian: true);
    }

    public override string ToString()
    {
        return _rsa?.ExportSubjectPublicKeyInfoPem().Replace("\n", Environment.NewLine) + Environment.NewLine ?? throw _ex!;
    }

    public bool Equals(IAsymmetricPublicKey? other)
    {
        if (other is null)
        {
            return false;
        }
        NetPublicKey otherKey = (NetPublicKey)other;
        RSAParameters otherParameters = otherKey._rsa!.ExportParameters(includePrivateParameters: false);
        RSAParameters parameters = _rsa!.ExportParameters(includePrivateParameters: false);

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
        return Equals(obj as IAsymmetricPublicKey);
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public static bool operator ==(NetPublicKey left, NetPublicKey right)
    {
        if (left is null)
        {
            return ReferenceEquals(right, null);
        }

        return left.Equals(right);
    }

    public static bool operator !=(NetPublicKey left, NetPublicKey right)
    {
        return !(left == right);
    }
}
