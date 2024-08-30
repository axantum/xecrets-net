#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions copyright © 2022-2024, Svante Seleborg, All Rights Reserved.
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
using Xecrets.Net.Core.Test.LegacyImplementation;

namespace AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestAsymmetricCrypto(CryptoImplementation cryptoImplementation)
    {
        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup(cryptoImplementation);
        }

        [TearDown]
        public void Teardown()
        {
            TypeMap.Register.Clear();
        }

        [Test]
        public void TestKeyPairPem()
        {
            IAsymmetricKeyPair keyPair = New<IAsymmetricFactory>().CreateKeyPair(1024);

            string privatePem = keyPair.PrivateKey.ToString();

            Assert.That(privatePem.StartsWith("-----BEGIN RSA PRIVATE KEY-----", StringComparison.OrdinalIgnoreCase));
            Assert.That(privatePem.EndsWith("-----END RSA PRIVATE KEY-----" + Environment.NewLine, StringComparison.OrdinalIgnoreCase));
            Assert.That(privatePem.Length, Is.GreaterThan(800));
            Assert.That(privatePem.Length, Is.LessThan(1000));

            string publicPem = keyPair.PublicKey.ToString();

            Assert.That(publicPem.StartsWith("-----BEGIN PUBLIC KEY-----", StringComparison.OrdinalIgnoreCase));
            Assert.That(publicPem.EndsWith("-----END PUBLIC KEY-----" + Environment.NewLine, StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void TestAsymmetricEncryption()
        {
            IAsymmetricPublicKey key = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey1);

            string text = "AxCrypt is Great!";
            byte[] encryptedBytes = key.Transform(Encoding.UTF8.GetBytes(text));

            Assert.That(encryptedBytes.Length, Is.EqualTo(512));
        }

        [Test]
        public void TestAsymmetricEncryptionDecryption()
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
        public void TestAsymmetricEncryptionFailedDecryptionWrongKey1()
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
        public void TestAsymmetricEncryptionFailedDecryptionWrongKey2()
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
