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

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Crypto.Asymmetric
{
    public class AsymmetricPublicKey : IAsymmetricKey
    {
        private AsymmetricKeyParameter Key { get; set; }

        internal AsymmetricPublicKey(AsymmetricKeyParameter publicKey)
        {
            Key = publicKey;
        }

        public AsymmetricPublicKey(string publicKeyPem)
        {
            using (TextReader reader = new StringReader(publicKeyPem))
            {
                PemReader pemReader = new PemReader(reader);

                Key = (AsymmetricKeyParameter)pemReader.ReadObject();
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
            IAsymmetricBlockCipher cipher = new OaepEncoding(new RsaBlindedEngine(), new Sha512Digest());

            cipher.Init(true, new ParametersWithRandom(Key, BouncyCastleRandomGenerator.CreateSecureRandom()));
            byte[] transformed = cipher.ProcessBlock(buffer, 0, buffer.Length);

            return transformed;
        }

        public string Pem
        {
            get { return ToPem(); }
        }
    }
}
