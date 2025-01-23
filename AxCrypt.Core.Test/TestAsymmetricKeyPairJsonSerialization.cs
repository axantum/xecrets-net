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
using AxCrypt.Core;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Test;
using AxCrypt.Fake;

using NUnit.Framework;

using Xecrets.Net.Api.Implementation;
using Xecrets.Net.Core.Crypto.Asymmetric;
using Xecrets.Net.Core.Test.LegacyImplementation;

using static AxCrypt.Abstractions.TypeResolve;

namespace Xecrets.Net.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    internal class TestAsymmetricKeyPairJsonSerialization
    {
        private CryptoImplementation _cryptoImplementation;

        public TestAsymmetricKeyPairJsonSerialization(CryptoImplementation cryptoImplementation)
        {
            _cryptoImplementation = cryptoImplementation;
        }

        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup(_cryptoImplementation);
        }

        [TearDown]
        public void Teardown()
        {
            TypeMap.Register.Clear();
        }

        private static readonly string _keyPairTestWindowsLineEndings = """
        {
          "publickey": {
            "pem": "-----BEGIN PUBLIC KEY-----\r\nMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCwDVOxC9IW2MoLjd52AmKpd59d\r\nUud6NGVuecCYBdivCGzaL7WBGj1/ANWGkdr721n6eLgIk4IYFQ5suTCmp7m1AXy1\r\nt266hn+/jjbCu0Z9X8CqIgfBQRHABAslZSNk2x1D3i4KiP/29z5oUYaFivwnzjNL\r\nSoXM7ibtyihFsrqKOQIDAQAB\r\n-----END PUBLIC KEY-----\r\n"
          },
          "privatekey": {
            "pem": "-----BEGIN RSA PRIVATE KEY-----\r\nMIICXQIBAAKBgQCwDVOxC9IW2MoLjd52AmKpd59dUud6NGVuecCYBdivCGzaL7WB\r\nGj1/ANWGkdr721n6eLgIk4IYFQ5suTCmp7m1AXy1t266hn+/jjbCu0Z9X8CqIgfB\r\nQRHABAslZSNk2x1D3i4KiP/29z5oUYaFivwnzjNLSoXM7ibtyihFsrqKOQIDAQAB\r\nAoGBAKjSvtTinv6luWrHCvNjajVUrxARNkSsBjCgtJ2TzaxbifbZFVbOUPZ/WEOJ\r\njtNCD9Du/pvKyFBLsN913z+RS7S+kfr5FjHrvxw6ojD9hF1NRrV0Vb7KGD3H2q+O\r\n36w7fLtO08+kSaQSHIGvSKFAt0JLjj0D1PWB9exgPtHPpDkZAkEA2FlaG1xdX19g\r\nYWMjZGVnZ2hpaytsbW4vcHFyM3R1dzd4eXt7fH1/f4CBg4OEhYeHiImOi4yNio+Q\r\nkZeTZGVhhwJBANBRUxNUVVcXWFlbG1xdXx9gYWMjZGVmJ2hpamtsbW8vcHFzc3R1\r\ndnd4eXt7fH1/f4CBg4OEhYOHiImOi6Wmpj8CQFyKs9laQ/JkuLRPlLcADSAVcGpQ\r\nE7wnUpF1ZVUPLqPFDXphUGvqvpvKWxvnKMt5Moc718ZnY6/uQveT748VMocCQQCO\r\n7Hz28hFrI1U2WU+4W0d8G4A1qH6lIyU+ebTN5yJd+kpHTFKWQFD7Puj6U4rh/6vW\r\n2wUEItQHneGLnwUoreG5AkAIkt1tiyS7jN1uGGV6fej8wRAAiq6j5W+FVdDB8S/j\r\nnymErOCkxKjCRn867iLvX7cM871d+7sUiz53xbdKNg5K\r\n-----END RSA PRIVATE KEY-----\r\n"
          }
        }
        """;
        
        private static readonly string _keyPairTestEnvironmentLineEndings = _keyPairTestWindowsLineEndings
            .Replace("\r\n", Environment.NewLine).Replace("\\r\\n", Environment.NewLine.Replace("\r", "\\r").Replace("\n", "\\n"));

        private static readonly string _keyPairTestUnixLineEndings = _keyPairTestWindowsLineEndings
            .Replace("\r\n", "\n").Replace("\\r\\n", "\\n");

        [Test]
        public void TestSerialize()
        {
            var serializer = New<IStringSerializer>();

            IAsymmetricKeyPair keyPair = serializer.Deserialize<IAsymmetricKeyPair>(_keyPairTestEnvironmentLineEndings);
            string json = serializer.Serialize(keyPair);

            Assert.That(json, Is.EqualTo(_keyPairTestEnvironmentLineEndings));
        }

        [Test]
        public void TestDeserialize()
        {
            var serializer = New<IStringSerializer>();

            IAsymmetricKeyPair deserializedKeyPair = serializer.Deserialize<IAsymmetricKeyPair>(_keyPairTestEnvironmentLineEndings);
            string serialized = serializer.Serialize(deserializedKeyPair);
            IAsymmetricKeyPair reDeserializedKeyPair = serializer.Deserialize<IAsymmetricKeyPair>(serialized);

            Assert.That(deserializedKeyPair, Is.EqualTo(reDeserializedKeyPair));
        }

        [Test]
        public void TestDeserializeWindowsLineEndings()
        {
            var serializer = New<IStringSerializer>();

            IAsymmetricKeyPair keyPair = serializer.Deserialize<IAsymmetricKeyPair>(_keyPairTestWindowsLineEndings);
            string json = serializer.Serialize(keyPair);

            Assert.That(json, Is.EqualTo(_keyPairTestEnvironmentLineEndings));
        }

        [Test]
        public void TestDeserializeUnixLineEndings()
        {
            var serializer = New<IStringSerializer>();

            IAsymmetricKeyPair keyPair = serializer.Deserialize<IAsymmetricKeyPair>(_keyPairTestUnixLineEndings);
            string json = serializer.Serialize(keyPair);

            Assert.That(json, Is.EqualTo(_keyPairTestEnvironmentLineEndings));
        }
    }
}
