using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace Axantum.AxCrypt.Core.Test
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
            SetupAssembly.AssemblySetup();
            SetupAssembly.AssemblySetupCrypto(_cryptoImplementation);

            TypeMap.Register.Singleton<IRandomGenerator>(() => new FakePseudoRandomGenerator());
            TypeMap.Register.Singleton<IAsymmetricFactory>(() => new FakeAsymmetricFactory("MD5"));
            Resolve.UserSettings.AsymmetricKeyBits = 512;
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public void TestSimplePublicKey()
        {
            IAsymmetricKeyPair keyPair = New<IAsymmetricFactory>().CreateKeyPair(512);

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