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

using AxCrypt.Api.Model;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxCrypt.Core.Crypto.Asymmetric
{
    internal class BouncyCastleKeyPair : IAsymmetricKeyPair
    {
        public BouncyCastleKeyPair(int bits)
        {
            AsymmetricCipherKeyPair keyPair = GenerateKeyPair(bits);

            PublicKey = new BouncyCastlePublicKey(keyPair.Public);
            PrivateKey = new BouncyCastlePrivateKey(keyPair.Private);
        }

        public BouncyCastleKeyPair(BouncyCastlePublicKey publicKey, BouncyCastlePrivateKey? privateKey)
        {
            PublicKey = publicKey;
            PrivateKey = privateKey;
        }

        public BouncyCastleKeyPair(byte[] n, byte[] e, byte[] d, byte[] p, byte[] q, byte[] dp, byte[] dq, byte[] qinv)
        {
            AsymmetricKeyParameter publicKeyParameter = new RsaKeyParameters(false, new BigInteger(n), new BigInteger(e));
            AsymmetricKeyParameter privateKeyParameter = new RsaPrivateCrtKeyParameters(new BigInteger(n), new BigInteger(e), new BigInteger(d), new BigInteger(p), new BigInteger(q), new BigInteger(dp), new BigInteger(dq), new BigInteger(qinv));

            PublicKey = new BouncyCastlePublicKey(publicKeyParameter);
            PrivateKey = new BouncyCastlePrivateKey(privateKeyParameter);
        }

        public IAsymmetricPublicKey PublicKey { get; private set; }

        public IAsymmetricPrivateKey? PrivateKey { get; private set; }

        private static AsymmetricCipherKeyPair GenerateKeyPair(int bits)
        {
            KeyGenerationParameters keyParameters = new KeyGenerationParameters(BouncyCastleRandomGenerator.CreateSecureRandom(), bits);
            RsaKeyPairGenerator rsaGenerator = new RsaKeyPairGenerator();
            rsaGenerator.Init(keyParameters);
            AsymmetricCipherKeyPair keyPair = rsaGenerator.GenerateKeyPair();

            return keyPair;
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
            var other = obj as IAsymmetricKeyPair;
            if (other == null)
            {
                return false;
            }

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return (PrivateKey?.GetHashCode() ?? 0) ^ PublicKey.GetHashCode();
        }

        public static bool operator ==(BouncyCastleKeyPair left, BouncyCastleKeyPair right)
        {
            if (left is null)
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(BouncyCastleKeyPair left, BouncyCastleKeyPair right)
        {
            return !(left == right);
        }
    }
}
