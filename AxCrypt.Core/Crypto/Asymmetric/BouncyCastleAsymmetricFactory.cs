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

using AxCrypt.Api.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxCrypt.Core.Crypto.Asymmetric
{
    public class BouncyCastleAsymmetricFactory : IAsymmetricFactory
    {
        public CustomSerializer[] GetSerializers()
        {
            CustomSerializer[] jsonConverters = new CustomSerializer[]
            {
                new AbstractTypeSerializer<IAsymmetricPublicKey, BouncyCastlePublicKey>(),
                new AbstractTypeSerializer<IAsymmetricPrivateKey, BouncyCastlePrivateKey>(),
                new AbstractTypeSerializer<IAsymmetricKeyPair, BouncyCastleKeyPair>(),
            };
            return jsonConverters;
        }

        public IAsymmetricPrivateKey CreatePrivateKey(string privateKeyPem)
        {
            return new BouncyCastlePrivateKey(privateKeyPem);
        }

        public IAsymmetricPublicKey CreatePublicKey(string publicKeyPem)
        {
            return new BouncyCastlePublicKey(publicKeyPem);
        }

        public IAsymmetricKeyPair CreateKeyPair(int bits)
        {
            return new BouncyCastleKeyPair(bits);
        }

        public IAsymmetricKeyPair CreateKeyPair(string publicKeyPem, string privateKeyPem)
        {
            if (privateKeyPem == null)
            {
                throw new ArgumentNullException(nameof(privateKeyPem));
            }

            return new BouncyCastleKeyPair(new BouncyCastlePublicKey(publicKeyPem), privateKeyPem.Length == 0 ? null : new BouncyCastlePrivateKey(privateKeyPem));
        }

        public IAsymmetricKeyPair CreateKeyPair(byte[] n, byte[] e, byte[] d, byte[] p, byte[] q, byte[] dp, byte[] dq, byte[] qinv)
        {
            return new BouncyCastleKeyPair(n, e, d, p, q, dp, dq, qinv);
        }

        public ICryptoHash CreatePaddingHash(int keyBits)
        {
            return new BouncyCastlePaddingHash(keyBits);
        }
    }
}