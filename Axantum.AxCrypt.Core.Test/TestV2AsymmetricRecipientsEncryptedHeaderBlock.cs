using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Portable;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Mono.Portable;
using NUnit.Framework;
using System.Collections.Generic;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestV2AsymmetricRecipientsEncryptedHeaderBlock
    {
        [SetUp]
        public static void Setup()
        {
            TypeMap.Register.Singleton<IAsymmetricFactory>(() => new FakeAsymmetricFactory("MD5"));
            TypeMap.Register.Singleton<IPortableFactory>(() => new PortableFactory());
            TypeMap.Register.New<IStringSerializer>(() => new StringSerializer(TypeMap.Resolve.Singleton<IAsymmetricFactory>().GetConverters()));
            TypeMap.Register.Singleton<IRandomGenerator>(() => new FakeRandomGenerator());
            TypeMap.Register.Singleton<IRuntimeEnvironment>(() => new FakeRuntimeEnvironment());
        }

        private static CryptoFactory CreateCryptoFactory()
        {
            CryptoFactory factory = new CryptoFactory();
            factory.Add(() => new V2Aes256CryptoFactory());

            return factory;
        }

        [TearDown]
        public static void Teardown()
        {
            TypeMap.Register.Clear();
        }

        [Test]
        public static void TestGetSetRecipientsAndClone()
        {
            V2AsymmetricRecipientsEncryptedHeaderBlock headerBlock = new V2AsymmetricRecipientsEncryptedHeaderBlock(new V2AesCrypto(SymmetricKey.Zero256, SymmetricIV.Zero128, 0));
            IAsymmetricKeyPair aliceKeyPair = TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreateKeyPair(512);
            IAsymmetricKeyPair bobKeyPair = TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreateKeyPair(512);

            List<UserPublicKey> publicKeys = new List<UserPublicKey>();
            publicKeys.Add(new UserPublicKey(new EmailAddress("alice@email.com"), aliceKeyPair.PublicKey));
            publicKeys.Add(new UserPublicKey(new EmailAddress("bob@email.com"), bobKeyPair.PublicKey));
            Recipients recipients = new Recipients(publicKeys);
            headerBlock.Recipients = recipients;
            Assert.That(headerBlock.Recipients.PublicKeys[0].Email, Is.EqualTo(new EmailAddress("alice@email.com")));
            Assert.That(headerBlock.Recipients.PublicKeys[1].Email, Is.EqualTo(new EmailAddress("bob@email.com")));

            V2AsymmetricRecipientsEncryptedHeaderBlock clone = (V2AsymmetricRecipientsEncryptedHeaderBlock)headerBlock.Clone();
            Assert.That(clone.Recipients.PublicKeys[0].Email, Is.EqualTo(new EmailAddress("alice@email.com")));
            Assert.That(clone.Recipients.PublicKeys[0].PublicKey.ToString(), Is.EqualTo(aliceKeyPair.PublicKey.ToString()));
            Assert.That(clone.Recipients.PublicKeys[1].Email, Is.EqualTo(new EmailAddress("bob@email.com")));
            Assert.That(clone.Recipients.PublicKeys[1].PublicKey.ToString(), Is.EqualTo(bobKeyPair.PublicKey.ToString()));
        }
    }
}