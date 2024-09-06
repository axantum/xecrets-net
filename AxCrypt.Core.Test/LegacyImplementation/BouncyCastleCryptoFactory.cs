﻿#region Coypright and License

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

using AxCrypt.Abstractions.Algorithm;
using AxCrypt.Core.Algorithm;
using AxCrypt.Core.Portable;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Xecrets.Net.Core.Test.LegacyImplementation
{
    public sealed class BouncyCastleCryptoFactory
    {
        private BouncyCastleCryptoFactory()
        {
        }

        public static AxCryptHMACSHA1 AxCryptHMACSHA1()
        {
            AxCryptHMACSHA1 hmac = new BouncyCastleAxCryptHmacSha1Wrapper();
            return hmac;
        }

        public static HMACSHA512 HMACSHA512()
        {
            HMACSHA512 hmac = new BouncyCastleHmacSha512Wrapper();
            return hmac;
        }

        public static Aes Aes()
        {
            Aes aes = new BouncyCastleAesWrapper();
            return aes;
        }

        public static CryptoStreamBase CryptoStream()
        {
            return new CryptoTransformingStream();
        }

        public static Sha1 Sha1()
        {
            Sha1 sha1 = new BouncyCastleSha1Wrapper();
            return sha1;
        }

        public static Sha256 Sha256()
        {
            Sha256 sha256 = new BouncyCastleSha256Wrapper();
            return sha256;
        }

        public static RandomNumberGenerator RandomNumberGenerator()
        {
            RandomNumberGenerator rng = new BouncyCastleRandomNumberGenerator();
            return rng;
        }
    }
}
