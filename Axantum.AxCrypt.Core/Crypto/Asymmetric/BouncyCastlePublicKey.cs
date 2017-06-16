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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

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

using static Axantum.AxCrypt.Abstractions.TypeResolve;

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

        internal BouncyCastlePublicKey(AsymmetricKeyParameter publicKeyParameter)
        {
            Key = publicKeyParameter;
        }

        [JsonConstructor]
        public BouncyCastlePublicKey(string pem)
        {
            Key = FromPem(pem);
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

        public byte[] TransformRaw(byte[] buffer, int outputLength)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            IAsymmetricBlockCipher cipher = new RsaEngine();
            cipher.Init(true, Key);
            if (outputLength > cipher.GetOutputBlockSize())
            {
                throw new ArgumentOutOfRangeException(nameof(outputLength), "Too large output length");
            }

            byte[] transformed = TransformInternal(buffer, cipher);
            if (transformed.Length == outputLength)
            {
                return transformed;
            }

            byte[] truncated = new byte[outputLength];
            Array.Copy(transformed, transformed.Length - outputLength, truncated, 0, outputLength);
            return truncated;
        }

        public byte[] Transform(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            int rsaKeyBitSize = ((RsaKeyParameters)Key).Modulus.BitLength;
            ICryptoHash paddingHash = New<IAsymmetricFactory>().CreatePaddingHash(rsaKeyBitSize);
            int requiredBits = (paddingHash.HashSize * 2 + buffer.Length + 1) * 8;
            if (requiredBits > rsaKeyBitSize)
            {
                throw new InvalidOperationException("The RSA Key size is too small to fit the data + 1 + 2 times the padding hash size.");
            }

            IAsymmetricBlockCipher cipher = new OaepEncoding(new RsaBlindedEngine(), new BouncyCastleDigest(paddingHash));
            cipher.Init(true, new ParametersWithRandom(Key, BouncyCastleRandomGenerator.CreateSecureRandom()));

            return TransformInternal(buffer, cipher);
        }

        private byte[] TransformInternal(byte[] buffer, IAsymmetricBlockCipher cipher)
        {
            byte[] transformed = cipher.ProcessBlock(buffer, 0, buffer.Length);

            int rsaKeyByteLength = cipher.GetOutputBlockSize();
            if (transformed.Length < rsaKeyByteLength)
            {
                byte[] tmp = new byte[rsaKeyByteLength];
                transformed.CopyTo(tmp, tmp.Length - transformed.Length);
                transformed = tmp;
            }

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
            if (Object.ReferenceEquals(other, null))
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