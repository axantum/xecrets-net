using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Algorithm;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace Axantum.AxCrypt.Core.Test.CryptoValidation
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestSha256
    {
        private CryptoImplementation _cryptoImplementation;

        public TestSha256(CryptoImplementation cryptoImplementation)
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
        public void TestInformalAbc()
        {
            byte[] bytes = Encoding.ASCII.GetBytes("abc");

            Sha256 hash = New<Sha256>();

            byte[] actual;
            actual = hash.ComputeHash(bytes);
            Assert.That(actual, Is.EquivalentTo("BA7816BF 8F01CFEA 414140DE 5DAE2223 B00361A3 96177A9C B410FF61 F20015AD".FromHex()));
        }

        [Test]
        public void TestInformalLongerText()
        {
            byte[] bytes = Encoding.ASCII.GetBytes("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq");

            Sha256 hash = New<Sha256>();

            byte[] actual;
            actual = hash.ComputeHash(bytes);
            Assert.That(actual, Is.EquivalentTo("248D6A61 D20638B8 E5C02693 0C3E6039 A33CE459 64FF2167 F6ECEDD4 19DB06C1".FromHex()));
        }

        [Test]
        public void TestInformalMillionA()
        {
            byte[] bytes = new byte[] { 97, 97, 97, 97, 97, 97, 97 };

            Sha256 hash = New<Sha256>();

            for (int i = 0; i < 1000000; i += bytes.Length)
            {
                if (1000000 - i > bytes.Length)
                {
                    hash.TransformBlock(bytes, 0, bytes.Length, null, 0);
                }
                else
                {
                    hash.TransformBlock(bytes, 0, 1000000 - i, null, 0);
                }
            }
            hash.TransformFinalBlock(new byte[0], 0, 0);

            byte[] actual;
            actual = hash.Hash();
            Assert.That(actual, Is.EquivalentTo("CDC76E5C 9914FB92 81A1C7E2 84D73E67 F1809A48 A497200E 046D39CC C7112CD0".FromHex()));
        }
    }
}