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
 * The source is maintained at http://bitbucket.org/AxCrypt.Desktop.Window-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using Xecrets.Net.Core.Test.Properties;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestAsymmetricCrypto
    {
        [SetUp]
        public static void Setup()
        {
            TypeMap.Register.Singleton<INow>(() => new FakeNow());
            TypeMap.Register.Singleton<IReport>(() => new FakeReport());
            TypeMap.Register.Singleton<IRandomGenerator>(() => new FakePseudoRandomGenerator());
            TypeMap.Register.Singleton<IAsymmetricFactory>(() => new FakeAsymmetricFactory("MD5"));
        }

        [TearDown]
        public static void Teardown()
        {
            TypeMap.Register.Clear();
        }

        [Test]
        public static void TestKeyPairPem()
        {
            IAsymmetricKeyPair keyPair = New<IAsymmetricFactory>().CreateKeyPair(512);

            string privatePem = keyPair.PrivateKey.ToString();

            Assert.That(privatePem.StartsWith("-----BEGIN RSA PRIVATE KEY-----", StringComparison.OrdinalIgnoreCase));
            Assert.That(privatePem.EndsWith("-----END RSA PRIVATE KEY-----" + Environment.NewLine, StringComparison.OrdinalIgnoreCase));
            Assert.That(privatePem.Length, Is.GreaterThan(490));
            Assert.That(privatePem.Length, Is.LessThan(600));

            string publicPem = keyPair.PublicKey.ToString();

            Assert.That(publicPem.StartsWith("-----BEGIN PUBLIC KEY-----", StringComparison.OrdinalIgnoreCase));
            Assert.That(publicPem.EndsWith("-----END PUBLIC KEY-----" + Environment.NewLine, StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public static void TestAsymmetricEncryption()
        {
            IAsymmetricPublicKey key = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey1);

            string text = "AxCrypt is Great!";
            byte[] encryptedBytes = key.Transform(Encoding.UTF8.GetBytes(text));

            Assert.That(encryptedBytes.Length, Is.EqualTo(512));
        }

        [Test]
        public static void TestAsymmetricEncryptionDecryption()
        {
            IAsymmetricPublicKey publicKey = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey1);

            string text = "AxCrypt is really very great!";
            byte[] encryptedBytes = publicKey.Transform(Encoding.UTF8.GetBytes(text));
            Assert.That(encryptedBytes.Length, Is.EqualTo(512));

            IAsymmetricPrivateKey privateKey = New<IAsymmetricFactory>().CreatePrivateKey(Resources.PrivateKey1);
            byte[] decryptedBytes = privateKey.Transform(encryptedBytes);
            string decryptedText = Encoding.UTF8.GetString(decryptedBytes);

            Assert.That(decryptedText, Is.EqualTo("AxCrypt is really very great!"));
        }

        [Test]
        public static void TestAsymmetricEncryptionFailedDecryptionWrongKey1()
        {
            IAsymmetricPublicKey publicKey = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey1);

            string text = "AxCrypt is really very great!";
            byte[] encryptedBytes = publicKey.Transform(Encoding.UTF8.GetBytes(text));
            Assert.That(encryptedBytes.Length, Is.EqualTo(512));

            IAsymmetricPrivateKey privateKey = New<IAsymmetricFactory>().CreatePrivateKey(Resources.PrivateKey2);
            byte[] decryptedBytes = privateKey.Transform(encryptedBytes);
            Assert.That(decryptedBytes, Is.Null);
        }

        [Test]
        public static void TestAsymmetricEncryptionFailedDecryptionWrongKey2()
        {
            IAsymmetricPublicKey publicKey = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey2);

            string text = "AxCrypt is really very great!";
            byte[] encryptedBytes = publicKey.Transform(Encoding.UTF8.GetBytes(text));
            Assert.That(encryptedBytes.Length, Is.EqualTo(512));

            IAsymmetricPrivateKey privateKey = New<IAsymmetricFactory>().CreatePrivateKey(Resources.PrivateKey1);
            byte[] decryptedBytes = privateKey.Transform(encryptedBytes);
            Assert.That(decryptedBytes, Is.Null);
        }
    }
}
