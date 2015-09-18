using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Abstractions.Rest;
using Axantum.AxCrypt.Api.Implementation;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Api.Response;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Algorithm;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Mono;
using Axantum.AxCrypt.Mono.Portable;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Api.Test
{
    [TestFixture]
    public class TestAxCryptApiClient
    {
        [SetUp]
        public void Setup()
        {
            TypeMap.Register.Singleton<IAsymmetricFactory>(() => new BouncyCastleAsymmetricFactory());
            TypeMap.Register.Singleton<IRandomGenerator>(() => new RandomGenerator());
            TypeMap.Register.New<IStringSerializer>(() => new StringSerializer(new BouncyCastleAsymmetricFactory().GetSerializers()));
            TypeMap.Register.Singleton<IEmailParser>(() => new EmailParser());
            TypeMap.Register.New<Sha256>(() => PortableFactory.SHA256Managed());
            TypeMap.Register.New<RandomNumberGenerator>(() => PortableFactory.RandomNumberGenerator());
            RuntimeEnvironment.RegisterTypeFactories();
        }

        [TearDown]
        public void Teardown()
        {
            TypeMap.Register.Clear();
        }

        [Test]
        public void TestSimpleSummary()
        {
            RestIdentity identity = new RestIdentity("svante@axcrypt.net", "a");

            UserInformation summary = new UserInformation(identity.User, "Free", new string[] { Convert.ToBase64String(new byte[16]) }, new Model.UserPublicKey());
            UserInformationResponse response = new UserInformationResponse(summary);
            string content = Resolve.Serializer.Serialize(response);

            RestResponse restResponse = new RestResponse(HttpStatusCode.OK, content);

            Mock<IRestCaller> mockRestCaller = new Mock<IRestCaller>();
            mockRestCaller.Setup<RestResponse>(wc => wc.Send(It.Is<RestIdentity>((i) => i.User == identity.User), It.Is<RestRequest>((r) => r.Url == new Uri("http://localhost/api/User/svante%40axcrypt.net")))).Returns(() => new RestResponse(HttpStatusCode.OK, content));
            mockRestCaller.Setup<string>(wc => wc.UrlEncode(It.IsAny<string>())).Returns<string>((url) => WebUtility.UrlEncode(url));
            TypeMap.Register.New<IRestCaller>(() => mockRestCaller.Object);

            AxCryptApiClient client = new AxCryptApiClient(identity, new Uri("http://localhost/api/"));
            UserInformation userSummary = client.GetUserInformation("svante@axcrypt.net");

            Assert.That(userSummary.UserName, Is.EqualTo(identity.User));
            Assert.That(userSummary.PublicKeyThumbprints.Count(), Is.EqualTo(1));
            Assert.That(userSummary.PublicKeyThumbprints.First(), Is.EqualTo(summary.PublicKeyThumbprints.First()));
        }
    }
}