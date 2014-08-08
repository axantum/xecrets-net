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
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Crypto.Asymmetric
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AsymmetricKeyPair
    {
        [JsonConstructor]
        private AsymmetricKeyPair()
        {
        }

        public AsymmetricKeyPair(AsymmetricPublicKey publicKey, AsymmetricPrivateKey privateKey)
        {
            PublicKey = publicKey;
            PrivateKey = privateKey;
        }

        public static AsymmetricKeyPair Create(int bits)
        {
            AsymmetricCipherKeyPair keyPair = GenerateKeyPair(bits);

            AsymmetricPublicKey publicKey = new AsymmetricPublicKey(keyPair.Public);
            AsymmetricPrivateKey privateKey = new AsymmetricPrivateKey(keyPair.Private);

            return new AsymmetricKeyPair(publicKey, privateKey);
        }

        [JsonProperty("publickey")]
        [JsonConverter(typeof(InterfaceJsonConverter<AsymmetricPublicKey, IAsymmetricKey>))]
        public IAsymmetricKey PublicKey { get; set; }

        [JsonProperty("privatekey")]
        [JsonConverter(typeof(InterfaceJsonConverter<AsymmetricPrivateKey, IAsymmetricKey>))]
        public IAsymmetricKey PrivateKey { get; set; }

        private static AsymmetricCipherKeyPair GenerateKeyPair(int bits)
        {
            KeyGenerationParameters keyParameters = new KeyGenerationParameters(BouncyCastleRandomGenerator.CreateSecureRandom(), bits);
            RsaKeyPairGenerator rsaGenerator = new RsaKeyPairGenerator();
            rsaGenerator.Init(keyParameters);
            AsymmetricCipherKeyPair keyPair = rsaGenerator.GenerateKeyPair();

            return keyPair;
        }
    }
}