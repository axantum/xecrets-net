using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Service;
using AxCrypt.Core.UI;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestUserKeyPair
    {
        private CryptoImplementation _cryptoImplementation;

        public TestUserKeyPair(CryptoImplementation cryptoImplementation)
        {
            _cryptoImplementation = cryptoImplementation;
        }

        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup();
            SetupAssembly.AssemblySetupCrypto(_cryptoImplementation);

            TypeMap.Register.Singleton<IRandomGenerator>(() => new FakePseudoRandomGenerator());
            TypeMap.Register.Singleton<IAsymmetricFactory>(() => new FakeAsymmetricFactory("MD5"));
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public void TestEquals()
        {
            UserKeyPair userKeyPair1 = new UserKeyPair(EmailAddress.Parse("svante@axantum.com"), 512);
            UserKeyPair userKeyPair1Copy = new UserKeyPair(userKeyPair1.UserEmail, userKeyPair1.Timestamp, userKeyPair1.KeyPair);

            Assert.That(userKeyPair1, Is.EqualTo(userKeyPair1Copy));

            UserKeyPair userKeyPair2 = new UserKeyPair(EmailAddress.Parse("svante@axantum.com"), 512);
            Assert.That(userKeyPair1, Is.Not.EqualTo(userKeyPair2));
        }
    }
}
