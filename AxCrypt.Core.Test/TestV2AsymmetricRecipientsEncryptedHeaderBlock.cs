using AxCrypt.Abstractions;
using AxCrypt.Abstractions.Algorithm;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Header;
using AxCrypt.Core.Portable;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.UI;
using AxCrypt.Fake;
using AxCrypt.Mono;
using AxCrypt.Mono.Portable;

using NUnit.Framework;

using Xecrets.File.Api.Implementation;
using Xecrets.File.Core;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestV2AsymmetricRecipientsEncryptedHeaderBlock
    {
        [SetUp]
        public static void Setup()
        {
            TypeMap.Register.Singleton<IAsymmetricFactory>(() => new FakeAsymmetricFactory("MD5"));
            TypeMap.Register.Singleton<IPortableFactory>(() => new PortableFactory());
            TypeMap.Register.Singleton<IStringSerializer>(() => new SystemTextJsonStringSerializer(JsonSourceGenerationContext.CreateJsonSerializerContext()));
            TypeMap.Register.Singleton<IRandomGenerator>(() => new FakeRandomGenerator());
            TypeMap.Register.Singleton<IRuntimeEnvironment>(() => new FakeRuntimeEnvironment());
            TypeMap.Register.Singleton<IEmailParser>(() => new EmailParser());
            TypeMap.Register.New<Aes>(() => PortableFactory.AesManaged());
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
            IAsymmetricKeyPair aliceKeyPair = New<IAsymmetricFactory>().CreateKeyPair(512);
            IAsymmetricKeyPair bobKeyPair = New<IAsymmetricFactory>().CreateKeyPair(512);

            List<UserPublicKey> publicKeys = new List<UserPublicKey>();
            publicKeys.Add(new UserPublicKey(EmailAddress.Parse("alice@email.com"), aliceKeyPair.PublicKey));
            publicKeys.Add(new UserPublicKey(EmailAddress.Parse("bob@email.com"), bobKeyPair.PublicKey));
            Recipients recipients = new Recipients(publicKeys);
            headerBlock.Recipients = recipients;
            Assert.That(headerBlock.Recipients.PublicKeys.ToList()[0].Email, Is.EqualTo(EmailAddress.Parse("alice@email.com")));
            Assert.That(headerBlock.Recipients.PublicKeys.ToList()[1].Email, Is.EqualTo(EmailAddress.Parse("bob@email.com")));

            V2AsymmetricRecipientsEncryptedHeaderBlock clone = (V2AsymmetricRecipientsEncryptedHeaderBlock)headerBlock.Clone();
            Assert.That(clone.Recipients.PublicKeys.ToList()[0].Email, Is.EqualTo(EmailAddress.Parse("alice@email.com")));
            Assert.That(clone.Recipients.PublicKeys.ToList()[0].PublicKey.ToString(), Is.EqualTo(aliceKeyPair.PublicKey.ToString()));
            Assert.That(clone.Recipients.PublicKeys.ToList()[1].Email, Is.EqualTo(EmailAddress.Parse("bob@email.com")));
            Assert.That(clone.Recipients.PublicKeys.ToList()[1].PublicKey.ToString(), Is.EqualTo(bobKeyPair.PublicKey.ToString()));
        }
    }
}
