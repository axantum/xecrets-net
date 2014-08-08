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

using Axantum.AxCrypt.Core.Runtime;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Crypto.Asymmetric
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AsymmetricPrivateKey : IAsymmetricKey
    {
        [JsonProperty("pem")]
        private string _serializedKey
        {
            get
            {
                return ToPem();
            }
            set
            {
                Key = FromPem(value);
            }
        }

        public AsymmetricKeyParameter Key { get; private set; }

        [JsonConstructor]
        private AsymmetricPrivateKey()
        {
        }

        internal AsymmetricPrivateKey(AsymmetricKeyParameter privateKey)
        {
            Key = privateKey;
        }

        public AsymmetricPrivateKey(string privateKeyPem)
        {
            Key = FromPem(privateKeyPem);
        }

        private AsymmetricKeyParameter FromPem(string pem)
        {
            using (TextReader reader = new StringReader(pem))
            {
                PemReader pemReader = new PemReader(reader);

                return ((AsymmetricCipherKeyPair)pemReader.ReadObject()).Private;
            }
        }

        private string ToPem()
        {
            using (TextWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                PemWriter pem = new PemWriter(writer);
                pem.WriteObject(Key);
                return writer.ToString();
            }
        }

        public byte[] Transform(byte[] buffer)
        {
            IAsymmetricBlockCipher cipher = new OaepEncoding(new RsaBlindedEngine(), new BouncyCastleDigest(Factory.New<IPaddingHash>()));

            cipher.Init(false, new ParametersWithRandom(Key, BouncyCastleRandomGenerator.CreateSecureRandom()));
            try
            {
                byte[] transformed = cipher.ProcessBlock(buffer, 0, buffer.Length);
                return transformed;
            }
            catch (CryptoException)
            {
                return null;
            }
        }

        public override string ToString()
        {
            return ToPem();
        }
    }
}