using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Abstractions.Algorithm;
using Axantum.AxCrypt.Abstractions.Rest;
using Axantum.AxCrypt.Api.Implementation;
using Axantum.AxCrypt.Api.Model;
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

            UserAccount summary = new UserAccount(identity.User, SubscriptionLevel.Free, DateTime.UtcNow.AddMonths(1), AccountStatus.Verified, Offers.None, new AccountKey[] { new AccountKey("svante@axcrypt.net", Convert.ToBase64String(new byte[16]), KeyPair.Empty, DateTime.MinValue), });
            string content = Resolve.Serializer.Serialize(summary);

            Mock<IRestCaller> mockRestCaller = new Mock<IRestCaller>();
            mockRestCaller.Setup<Task<RestResponse>>(wc => wc.SendAsync(It.Is<RestIdentity>((i) => i.User == identity.User), It.Is<RestRequest>((r) => r.Url == new Uri("http://localhost/api/users/my/account")))).Returns(() => Task.FromResult(new RestResponse(HttpStatusCode.OK, content)));
            mockRestCaller.Setup<string>(wc => wc.UrlEncode(It.IsAny<string>())).Returns<string>((url) => WebUtility.UrlEncode(url));
            TypeMap.Register.New<IRestCaller>(() => mockRestCaller.Object);

            AxCryptApiClient client = new AxCryptApiClient(identity, new Uri("http://localhost/api/"), TimeSpan.Zero);
            UserAccount userSummary = client.MyAccountAsync().Result;

            Assert.That(userSummary.UserName, Is.EqualTo(identity.User));
            Assert.That(userSummary.AccountKeys.Count(), Is.EqualTo(1));
            Assert.That(userSummary.AccountKeys.First(), Is.EqualTo(summary.AccountKeys.First()));
        }

        [Test]
        public void TestAnonymousSummary()
        {
            RestIdentity identity = new RestIdentity();

            UserAccount summary = new UserAccount(identity.User, SubscriptionLevel.Free, DateTime.UtcNow.AddMonths(1), AccountStatus.Verified, Offers.None, new AccountKey[] { new AccountKey("svante@axcrypt.net", Convert.ToBase64String(new byte[16]), new KeyPair("public-key-fake-PEM", String.Empty), DateTime.MinValue), });
            string content = Resolve.Serializer.Serialize(summary);

            Mock<IRestCaller> mockRestCaller = new Mock<IRestCaller>();
            mockRestCaller.Setup<Task<RestResponse>>(wc => wc.SendAsync(It.Is<RestIdentity>((i) => i.User.Length == 0), It.Is<RestRequest>((r) => r.Url == new Uri("http://localhost/api/users/all/accounts/svante@axcrypt.net")))).Returns(() => Task.FromResult(new RestResponse(HttpStatusCode.OK, content)));
            mockRestCaller.Setup<string>(wc => wc.UrlEncode(It.IsAny<string>())).Returns<string>((url) => WebUtility.UrlEncode(url));
            TypeMap.Register.New<IRestCaller>(() => mockRestCaller.Object);

            AxCryptApiClient client = new AxCryptApiClient(identity, new Uri("http://localhost/api/"), TimeSpan.Zero);
            UserAccount userSummary = client.GetAllAccountsUserAccountAsync("svante@axcrypt.net").Result;

            Assert.That(userSummary.UserName, Is.EqualTo(identity.User));
            Assert.That(userSummary.AccountKeys.Count(), Is.EqualTo(1), "There should be an AccountKey here.");
            Assert.That(userSummary.AccountKeys.First(), Is.EqualTo(summary.AccountKeys.First()));
        }
    }
}