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

using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Crypto.Asymmetric
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class BouncyCastlePublicKey : IAsymmetricPublicKey
    {
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Json.NET serializer uses it.")]
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

        private AsymmetricKeyParameter Key { get; set; }

        [JsonConstructor]
        private BouncyCastlePublicKey()
        {
        }

        internal BouncyCastlePublicKey(AsymmetricKeyParameter publicKeyParameter)
        {
            Key = publicKeyParameter;
        }

        public BouncyCastlePublicKey(string publicKeyPem)
        {
            Key = FromPem(publicKeyPem);
        }

        private static AsymmetricKeyParameter FromPem(string pem)
        {
            using (TextReader reader = new StringReader(pem))
            {
                PemReader pemReader = new PemReader(reader);

                return (AsymmetricKeyParameter)pemReader.ReadObject();
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
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            IAsymmetricBlockCipher cipher = new OaepEncoding(new RsaBlindedEngine(), new BouncyCastleDigest(TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreatePaddingHash()));

            cipher.Init(true, new ParametersWithRandom(Key, BouncyCastleRandomGenerator.CreateSecureRandom()));
            byte[] transformed = cipher.ProcessBlock(buffer, 0, buffer.Length);

            return transformed;
        }

        public override string ToString()
        {
            return ToPem();
        }

        public bool Equals(IAsymmetricPublicKey other)
        {
            if ((object)other == null)
            {
                return false;
            }
            return ToString().Equals(other.ToString());
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !typeof(IAsymmetricPublicKey).IsAssignableFrom(obj.GetType()))
            {
                return false;
            }
            IAsymmetricPublicKey other = (IAsymmetricPublicKey)obj;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}