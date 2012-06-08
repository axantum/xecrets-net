#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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

using System;
using System.Security.Cryptography;
using Axantum.AxCrypt.Core.Crypto;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestAesCrypto
    {
        [Test]
        public static void TestInvalidArguments()
        {
            AesKey key = new AesKey();
            AesIV iv = new AesIV();
            AesCrypto crypto;

            Assert.Throws<ArgumentNullException>(() =>
            {
                crypto = new AesCrypto(null);
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                crypto = new AesCrypto(null, iv, CipherMode.CBC, PaddingMode.None);
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                crypto = new AesCrypto(key, null, CipherMode.CBC, PaddingMode.None);
            });

            crypto = new AesCrypto(key, iv, CipherMode.CBC, PaddingMode.None);
        }

        [Test]
        public static void TestDoubleDispose()
        {
            AesKey key = new AesKey();
            AesIV iv = new AesIV();
            ICryptoTransform transform;

            AesCrypto crypto = new AesCrypto(key, iv, CipherMode.CBC, PaddingMode.None);
            crypto.Dispose();
            Assert.Throws<ObjectDisposedException>(() =>
            {
                transform = crypto.CreateDecryptingTransform();
            });
            crypto.Dispose();
            Assert.Throws<ObjectDisposedException>(() =>
            {
                transform = crypto.CreateDecryptingTransform();
            });
        }

        [Test]
        public static void TestObjectDisposed()
        {
            AesKey key = new AesKey();
            AesIV iv = new AesIV();

            AesCrypto crypto = new AesCrypto(key, iv, CipherMode.CBC, PaddingMode.None);
            crypto.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
            {
                crypto.Decrypt(new byte[16]);
            });

            Assert.Throws<ObjectDisposedException>(() =>
            {
                crypto.Encrypt(new byte[16]);
            });

            Assert.Throws<ObjectDisposedException>(() =>
            {
                crypto.CreateDecryptingTransform();
            });

            Assert.Throws<ObjectDisposedException>(() =>
            {
                crypto.CreateEncryptingTransform();
            });
        }
    }
}