using AxCrypt.Core.Crypto;
using AxCrypt.Core.Service;
using AxCrypt.Core.Session;
using AxCrypt.Core.UI;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
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
        public void TestSimpleThumbprintMatches()
        {
            IdentityPublicTag tag1 = new IdentityPublicTag(new LogOnIdentity(new Passphrase("allan")));
            IdentityPublicTag tag2 = new IdentityPublicTag(new LogOnIdentity(new Passphrase("allan")));

            Assert.That(tag1.Matches(tag2), "tag1 should match tag2 since they are based on the same passphrase.");
            Assert.That(tag2.Matches(tag1), "tag2 should match tag1 since they are based on the same passphrase.");
            Assert.That(tag1.Matches(tag1), "tag1 should match tag1 since they are the same instance.");
            Assert.That(tag2.Matches(tag2), "tag2 should match tag2 since they are the same instance.");
        }

        [Test]
        public void TestSimpleAsymmetricIdentityMatches()
        {
            UserKeyPair key1 = new UserKeyPair(EmailAddress.Parse("svante@axantum.com"), 512);
            UserKeyPair key2 = new UserKeyPair(EmailAddress.Parse("svante@axantum.com"), 512);

            IdentityPublicTag tag1 = new IdentityPublicTag(new LogOnIdentity(new UserKeyPair[] { key1 }, new Passphrase("allan")));
            IdentityPublicTag tag2 = new IdentityPublicTag(new LogOnIdentity(new UserKeyPair[] { key2 }, new Passphrase("allan")));

            Assert.That(tag1.Matches(tag2), "tag1 should match tag2 since they are based on the same asymmetric key and passphrase.");
            Assert.That(tag2.Matches(tag1), "tag2 should match tag1 since they are based on the same asymmetric key and passphrase.");
            Assert.That(tag1.Matches(tag1), "tag1 should match tag1 since they are the same instance.");
            Assert.That(tag2.Matches(tag2), "tag2 should match tag2 since they are the same instance.");
        }

        [Test]
        public void TestAsymmetricIdentityButDifferentPassphraseMatches()
        {
            UserKeyPair key1 = new UserKeyPair(EmailAddress.Parse("svante@axantum.com"), 512);
            UserKeyPair key2 = new UserKeyPair(EmailAddress.Parse("svante@axantum.com"), 512);

            IdentityPublicTag tag1 = new IdentityPublicTag(new LogOnIdentity(new UserKeyPair[] { key1 }, new Passphrase("allan")));
            IdentityPublicTag tag2 = new IdentityPublicTag(new LogOnIdentity(new UserKeyPair[] { key2 }, new Passphrase("niklas")));

            Assert.That(tag1.Matches(tag2), "tag1 should match tag2 since they are based on the same asymmetric user email and passphrase.");
            Assert.That(tag2.Matches(tag1), "tag2 should match tag1 since they are based on the same asymmetric user email and passphrase.");
            Assert.That(tag1.Matches(tag1), "tag1 should match tag1 since they are the same instance.");
            Assert.That(tag2.Matches(tag2), "tag2 should match tag2 since they are the same instance.");
        }

        [Test]
        public void TestDifferentAsymmetricIdentityAndSamePassphraseDoesNotMatch()
        {
            UserKeyPair key1 = new UserKeyPair(EmailAddress.Parse("svante1@axantum.com"), 512);
            UserKeyPair key2 = new UserKeyPair(EmailAddress.Parse("svante2@axantum.com"), 512);

            IdentityPublicTag tag1 = new IdentityPublicTag(new LogOnIdentity(new UserKeyPair[] { key1 }, new Passphrase("allan")));
            IdentityPublicTag tag2 = new IdentityPublicTag(new LogOnIdentity(new UserKeyPair[] { key2 }, new Passphrase("allan")));

            Assert.That(!tag1.Matches(tag2), "tag1 should not match tag2 since they are based on different asymmetric user email even if passphrase is the same.");
            Assert.That(!tag2.Matches(tag1), "tag2 should not match tag1 since they are based on different asymmetric user email even if passphrase is the same.");
            Assert.That(tag1.Matches(tag1), "tag1 should match tag1 since they are the same instance.");
            Assert.That(tag2.Matches(tag2), "tag2 should match tag2 since they are the same instance.");
        }
    }
}