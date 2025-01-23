#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions Copyright © 2022-2025, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets Cli, but is derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * The changes and additions are separately copyrighted and only licensed under GPL v3 or later as detailed below,
 * unless explicitly licensed otherwise. If you use any part of these changes and additions in your software,
 * please see https://www.gnu.org/licenses/ for details of what this means for you.
 * 
 * Warning: If you are using the original AxCrypt code under a non-GPL v3 or later license, these changes and additions
 * are not included in that license. If you use these changes under those circumstances, all your code becomes subject to
 * the GPL v3 or later license, according to the principle of strong copyleft as applied to GPL v3 or later.
 *
 * Xecrets Cli is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets Cli is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets Cli. If not, see
 * https://www.gnu.org/licenses/.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions, as well for commit history detailing changes and additions that fall under the strong
 * copyleft provisions mentioned above. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Xecrets Cli Copyright and GPL License notice
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

using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto.Asymmetric;

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

using static AxCrypt.Abstractions.TypeResolve;

namespace Xecrets.Net.Core.Test.LegacyImplementation
{
    internal class BouncyCastlePrivateKey : IAsymmetricPrivateKey
    {
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

        internal BouncyCastlePrivateKey(AsymmetricKeyParameter privateKey)
        {
            Key = privateKey;
        }

        public BouncyCastlePrivateKey(string pem)
        {
            Key = FromPem(pem);
        }

        private static AsymmetricKeyParameter FromPem(string pem)
        {
            using TextReader reader = new StringReader(pem);
            PemReader pemReader = new PemReader(reader);

            return ((AsymmetricCipherKeyPair)pemReader.ReadObject()).Private;
        }

        private string ToPem()
        {
            using TextWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            PemWriter pem = new PemWriter(writer);
            pem.WriteObject(Key);
            return writer.ToString() ?? throw new InvalidOperationException("TextWriter.ToString() returned null!");
        }

        public byte[]? TransformRaw(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            int rsaKeyBitLength = ((RsaKeyParameters)Key).Modulus.BitLength;
            int rsaKeyByteLength = (rsaKeyBitLength + 7) / 8;

            if (buffer.Length < rsaKeyByteLength)
            {
                byte[] extended = new byte[rsaKeyByteLength];
                Array.Copy(buffer, 0, extended, extended.Length - buffer.Length, buffer.Length);
                buffer = extended;
            }

            if (buffer.Length > rsaKeyByteLength)
            {
                throw new ArgumentOutOfRangeException(nameof(buffer), "Too long buffer to decrypt");
            }

            IAsymmetricBlockCipher cipher = new RsaEngine();
            cipher.Init(false, Key);
            return TransformInternal(buffer, cipher);
        }

        public byte[]? Transform(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            int rsaKeyBitLength = ((RsaKeyParameters)Key).Modulus.BitLength;

            IAsymmetricBlockCipher cipher = new OaepEncoding(new RsaBlindedEngine(), new BouncyCastleDigest(New<IPaddingHashFactory>().CreatePaddingHash(rsaKeyBitLength)));
            cipher.Init(false, new ParametersWithRandom(Key, BouncyCastleRandomGenerator.CreateSecureRandom()));

            return TransformInternal(buffer, cipher);
        }

        private byte[]? TransformInternal(byte[] buffer, IAsymmetricBlockCipher cipher)
        {
            int rsaKeyBitLength = ((RsaKeyParameters)Key).Modulus.BitLength;

            try
            {
                byte[] transformed = cipher.ProcessBlock(buffer, 0, (rsaKeyBitLength + 7) / 8);
                return transformed;
            }
            catch (Org.BouncyCastle.Crypto.CryptoException cex)
            {
                New<IReport>().Exception(cex);
                return null;
            }
        }

        public override string ToString()
        {
            return ToPem();
        }

        public bool Equals(IAsymmetricPrivateKey? other)
        {
            if (other == null)
            {
                return false;
            }
            return ToString().Equals(other.ToString());
        }

        public override bool Equals(object? obj)
        {
            if (obj is not IAsymmetricPrivateKey other)
            {
                return false;
            }

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(BouncyCastlePrivateKey left, BouncyCastlePrivateKey right)
        {
            if (left is null)
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(BouncyCastlePrivateKey left, BouncyCastlePrivateKey right)
        {
            return !(left == right);
        }
    }
}
