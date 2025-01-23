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

using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Extensions;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xecrets.Net.Core.Test.LegacyImplementation;

using static AxCrypt.Abstractions.TypeResolve;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestPublicKeyThumbprint
    {
        private CryptoImplementation _cryptoImplementation;

        public TestPublicKeyThumbprint(CryptoImplementation cryptoImplementation)
        {
            _cryptoImplementation = cryptoImplementation;
        }

        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup(_cryptoImplementation);

            TypeMap.Register.Singleton<IRandomGenerator>(() => new FakePseudoRandomGenerator());
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        private const string KeyPairPem = """
            {
              "publickey": {
                "pem": "-----BEGIN PUBLIC KEY-----\r\nMFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAKbdJI2IXKsZiiuiD9F+xR/Bp5tcwd7k\r\nAIcLP0eOo3louRnr377FKv2QpzuxP3HE5kF9wf7C1PDykhbAbop7DJECAwEAAQ==\r\n-----END PUBLIC KEY-----\r\n"
              },
              "privatekey": {
                "pem": "-----BEGIN RSA PRIVATE KEY-----\r\nMIIBOwIBAAJBAKbdJI2IXKsZiiuiD9F+xR/Bp5tcwd7kAIcLP0eOo3louRnr377F\r\nKv2QpzuxP3HE5kF9wf7C1PDykhbAbop7DJECAwEAAQJBAKUsWKeFKP3xbRVd+byN\r\neUHDJ08iFYK2PVNwLbZ+mpFtHwovjXMGpoqP8k/OaKJ+rNxBRbBYdJCML5uhtQFG\r\nwcUCIQDfZb2/n7cv99ZU5Ea7yq0RZ4Vr1W+w8F84mmwGsdH/nwIhAL83Rgtu5akI\r\nLCpN+Vcb09hsSiYZO1FlIgbXeiX1CMXPAiBBMtlhLsTxC/0Sw5jdP/aoyLTI1v8E\r\n/fJce70haw5l8wIgA3gYQDrZ1dA9JONXQ7pQhJuqWLiad+aS0Hb2U1v3tccCIQCT\r\nWH6lMUf5Jtu6IAXIOUnN/XtNXkOlSCdFdkUJDTDp6A==\r\n-----END RSA PRIVATE KEY-----\r\n"
              }
            }
            """;

        [Test]
        public void TestSimplePublicKey()
        {
            var serializer = New<IStringSerializer>();
            IAsymmetricKeyPair keyPair = serializer.Deserialize<IAsymmetricKeyPair>(KeyPairPem);

            string actual = keyPair.PublicKey.Thumbprint.ToFileString();
            Assert.That(actual, Is.EqualTo("JYh9b6JLYKDxr1sA75ZUWg"));

            PublicKeyThumbprint fromStringThumbprint = actual.ToPublicKeyThumbprint();
            Assert.That(fromStringThumbprint, Is.EqualTo(keyPair.PublicKey.Thumbprint));
        }

        [Test]
        public void TestArgumentExceptions()
        {
            byte[] nullBytes = null;
            byte[] dummyModulus = new byte[50];

            Assert.Throws<ArgumentNullException>(() => new PublicKeyThumbprint(nullBytes, nullBytes));
            Assert.Throws<ArgumentNullException>(() => new PublicKeyThumbprint(dummyModulus, nullBytes));
            Assert.Throws<ArgumentNullException>(() => new PublicKeyThumbprint(nullBytes, dummyModulus));
            Assert.Throws<ArgumentNullException>(() => new PublicKeyThumbprint(nullBytes));
        }
    }
}
