using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.Portable;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Mono.Portable;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestV2AsymmetricRecipientsEncryptedHeaderBlock
    {
        [SetUp]
        public static void Setup()
        {
            Factory.Instance.Singleton<CryptoFactory>(() => CreateCryptoFactory());
            Factory.Instance.Singleton<ICryptoPolicy>(() => new ProCryptoPolicy());
            Factory.Instance.Singleton<IPortableFactory>(() => new PortableFactory());
        }

        private static CryptoFactory CreateCryptoFactory()
        {
            CryptoFactory factory = new CryptoFactory();
            factory.Add(() => new V2Aes256CryptoFactory());
            factory.Add(() => new V1Aes128CryptoFactory());

            return factory;
        }

        [TearDown]
        public static void Teardown()
        {
            Factory.Instance.Clear();
        }

        [Test]
        public static void TestGetSet()
        {
            Factory.Instance.Singleton<IRandomGenerator>(() => new FakeRandomGenerator());
            Factory.Instance.Singleton<IRuntimeEnvironment>(() => new FakeRuntimeEnvironment());

            V2AsymmetricRecipientsEncryptedHeaderBlock headerBlock = new V2AsymmetricRecipientsEncryptedHeaderBlock(new V2AesCrypto(SymmetricKey.Zero256, SymmetricIV.Zero128, 0));
            headerBlock.Recipients = new string[] { "alice@email.com", "bob@email.com" };
            Assert.That(headerBlock.Recipients.First(), Is.EqualTo("alice@email.com"));
            Assert.That(headerBlock.Recipients.Skip(1).First(), Is.EqualTo("bob@email.com"));

            V2AsymmetricRecipientsEncryptedHeaderBlock clone = (V2AsymmetricRecipientsEncryptedHeaderBlock)headerBlock.Clone();
            Assert.That(clone.Recipients.First(), Is.EqualTo("alice@email.com"));
            Assert.That(clone.Recipients.Skip(1).First(), Is.EqualTo("bob@email.com"));
        }
    }
}