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

using System.Security.Cryptography;

using AxCrypt.Core.Crypto.Asymmetric;

namespace Xecrets.Net.Core.Crypto.Asymmetric;

internal class NetKeyPair : IAsymmetricKeyPair
{
    public IAsymmetricPublicKey PublicKey { get; }

    public IAsymmetricPrivateKey? PrivateKey { get; }

    public NetKeyPair(int bits)
    {
        RSA rsa = RSA.Create(bits);

        PublicKey = new NetPublicKey(rsa.ExportRSAPublicKey());
        PrivateKey = new NetPrivateKey(rsa.ExportRSAPrivateKey());
    }

    public NetKeyPair(NetPublicKey publicKey, NetPrivateKey? privateKey)
    {
        PublicKey = publicKey;
        PrivateKey = privateKey;
    }

    public NetKeyPair(byte[] n, byte[] e, byte[] d, byte[] p, byte[] q, byte[] dp, byte[] dq, byte[] qinv)
    {
        RSA rsa = CreateRsaFromParameters(n, e, d, p, q, dp, dq, qinv);

        PublicKey = new NetPublicKey(rsa.ExportRSAPublicKey());
        PrivateKey = new NetPrivateKey(rsa.ExportRSAPrivateKey());
    }

    private static RSA CreateRsaFromParameters(byte[] n, byte[] e, byte[] d, byte[] p, byte[] q, byte[] dp, byte[] dq, byte[] qinv)
    {
        RSAParameters rsaParameters = new RSAParameters
        {
            Modulus = n,
            Exponent = e,
            D = d,
            P = p,
            Q = q,
            DP = dp,
            DQ = dq,
            InverseQ = qinv
        };

        RSA rsa = RSA.Create();
        rsa.ImportParameters(rsaParameters);
        return rsa;
    }

    public bool Equals(IAsymmetricKeyPair? other)
    {
        if (other == null)
        {
            return false;
        }
        if (ReferenceEquals(PrivateKey, null))
        {
            return ReferenceEquals(other.PrivateKey, null);
        }
        return PrivateKey.Equals(other.PrivateKey) && PublicKey.Equals(other.PublicKey);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not IAsymmetricKeyPair other)
        {
            return false;
        }

        return Equals(other);
    }

    public override int GetHashCode()
    {
        return (PrivateKey?.GetHashCode() ?? 0) ^ PublicKey.GetHashCode();
    }

    public static bool operator ==(NetKeyPair left, NetKeyPair right)
    {
        if (left is null)
        {
            return ReferenceEquals(right, null);
        }

        return left.Equals(right);
    }

    public static bool operator !=(NetKeyPair left, NetKeyPair right)
    {
        return !(left == right);
    }
}
