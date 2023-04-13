#region Coypright and GPL License

/*
 * Xecrets File Core - Copyright 2022, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets File Core, parts of which in turn are derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * However, this code is not derived from AxCrypt and is separately copyrighted and only licensed as follows unless
 * explicitly licensed otherwise. If you use any part of this code in your software, please see https://www.gnu.org/licenses/
 * for details of what this means for you.
 *
 * Xecrets File Core is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets File Core is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets File Core.  If not, see <https://www.gnu.org/licenses/>.
 *
 * The source repository can be found at https://github.com/ please go there for more information, suggestions and
 * contributions. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Coypright and GPL License

using AxCrypt.Abstractions;
using AxCrypt.Core;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Fake;

using NUnit.Framework;

using Xecrets.File.Api.Implementation;

using static AxCrypt.Abstractions.TypeResolve;

namespace Xecrets.File.Core.Test
{
    [TestFixture]
    internal class TestBouncyCastleKeyPairJsonSerialization
    {
        [SetUp]
        public void Setup()
        {
            TypeMap.Register.Singleton<IRandomGenerator>(() => new FakeRandomGenerator());
            TypeMap.Register.Singleton<IAsymmetricFactory>(() => new FakeAsymmetricFactory("MD5"));
            TypeMap.Register.Singleton<IStringSerializer>(() => new SystemTextJsonStringSerializer(JsonSourceGenerationContext.CreateJsonSerializerContext()));
        }

        private const string _keyPairTest = @"{
  ""publickey"": {
    ""pem"": ""-----BEGIN PUBLIC KEY-----\r\nMFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAJFfzw3x4wXq8nUwMqGvurbB\u002B9SqWOlD\r\nD7d0r\u002BViluKok9\u002BcSttlQWTxU0pPaylkYrf5OMr2WexQQRMYoJJ/pOkCAwEAAQ==\r\n-----END PUBLIC KEY-----\r\n""
  },
  ""privatekey"": {
    ""pem"": ""-----BEGIN RSA PRIVATE KEY-----\r\nMIIBOQIBAAJBAJFfzw3x4wXq8nUwMqGvurbB\u002B9SqWOlDD7d0r\u002BViluKok9\u002BcSttl\r\nQWTxU0pPaylkYrf5OMr2WexQQRMYoJJ/pOkCAwEAAQJAL6Ywrshu\u002BhyVVhXzMo3v\r\n7EFO8tjXBbYGa8JieRREkovgKhlWLR\u002BKQCcdfmEXM4zi7j13WVFG9C/Tzb5P7caS\r\ndQIhAOhpKmpsbW5/cHFzc3Rxdmd4OXt7eH1uf8CBgocdHgnnAiEAoCEiIiQhJjco\r\naSsrLCkuPzBxMjM0NTY3eDg6OzU2IK8CIBvgiXLsgI/Rf3ZMR9v\u002BxBoTJKn4HHLH\r\nL1Gy9yonxvOfAiAgA9/eMZkLGfYTaNUd5DHh4l6PadtjVC6s85j443\u002Bp8wIgeq/6\r\nsDt16\u002B14ryh2Ahbn9Cj\u002BO63kkrIIdHG20AsNYPM=\r\n-----END RSA PRIVATE KEY-----\r\n""
  }
}";

        [Test]
        public void TestSerialize()
        {
            var serializer = New<IStringSerializer>();

            IAsymmetricKeyPair keyPair = new BouncyCastleKeyPair(512);
            string json = serializer.Serialize(keyPair);

            Assert.That(json, Is.EqualTo(_keyPairTest));
        }

        [Test]
        public void TestDeserialize()
        {
            var serializer = New<IStringSerializer>();

            IAsymmetricKeyPair keyPair = new BouncyCastleKeyPair(512);

            IAsymmetricKeyPair deserialized = serializer.Deserialize<IAsymmetricKeyPair>(_keyPairTest);

            Assert.That(deserialized, Is.EqualTo(keyPair));
        }
    }
}
