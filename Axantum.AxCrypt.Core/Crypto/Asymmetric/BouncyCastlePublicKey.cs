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

using Axantum.AxCrypt.Abstractions;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
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

        private PublicKeyThumbprint _thumbprint = null;

        /// <summary>
        /// Gets the thumbprint of the public key.
        /// </summary>
        /// <value>
        /// The thumbprint.
        /// </value>
        /// <exception cref="System.InvalidOperationException">Attempt to get a public key thumbprint from a private key.</exception>
        public PublicKeyThumbprint Thumbprint
        {
            get
            {
                if (_thumbprint == null)
                {
                    RsaKeyParameters rsaKey = (RsaKeyParameters)Key;
                    if (rsaKey.IsPrivate)
                    {
                        throw new InvalidOperationException("Attempt to get a public key thumbprint from a private key.");
                    }
                    byte[] modulus = rsaKey.Modulus.ToByteArray();
                    byte[] exponent = rsaKey.Exponent.ToByteArray();
                    _thumbprint = new PublicKeyThumbprint(modulus, exponent);
                }
                return _thumbprint;
            }
        }

        public override string ToString()
        {
            return ToPem();
        }

        public bool Equals(IAsymmetricPublicKey other)
        {
            if (Object.ReferenceEquals(other, null) || !typeof(IAsymmetricPublicKey).IsAssignableFrom(other.GetType()))
            {
                return false;
            }
            return ToString() == other.ToString();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IAsymmetricPublicKey);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(BouncyCastlePublicKey left, BouncyCastlePublicKey right)
        {
            if (Object.ReferenceEquals(left, null))
            {
                return Object.ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(BouncyCastlePublicKey left, BouncyCastlePublicKey right)
        {
            return !(left == right);
        }
    }
}