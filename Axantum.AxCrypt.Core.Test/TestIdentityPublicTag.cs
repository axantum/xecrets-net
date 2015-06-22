using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Session;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestIdentityPublicTag
    {
        private CryptoImplementation _cryptoImplementation;

        public TestIdentityPublicTag(CryptoImplementation cryptoImplementation)
        {
            _cryptoImplementation = cryptoImplementation;
        }

        [SetUp]
        public void SetUp()
        {
            SetupAssembly.AssemblySetup();
            SetupAssembly.AssemblySetupCrypto(_cryptoImplementation);
        }

        [TearDown]
        public void TearDown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public void TestSimpleThumbprintEquals()
        {
            IdentityPublicTag tag1 = new IdentityPublicTag(new LogOnIdentity(new Passphrase("allan")));
            IdentityPublicTag tag2 = new IdentityPublicTag(new LogOnIdentity(new Passphrase("allan")));

            Assert.That(tag1.Matches(tag2), "tag1 should match tag2 since they are based on the same passphrase.");
            Assert.That(tag2.Matches(tag1), "tag2 should match tag1 since they are based on the same passphrase.");
            Assert.That(tag1.Matches(tag1), "tag1 should match tag1 since they are the same instance.");
            Assert.That(tag2.Matches(tag2), "tag2 should match tag2 since they are the same instance.");
        }
    }
}