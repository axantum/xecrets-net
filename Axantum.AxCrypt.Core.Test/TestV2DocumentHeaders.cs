#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
 *
 * This file is part of AxCrypt.
 *
 * AxCrypt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AxCrypt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AxCrypt.  If not, see <http://www.gnu.org/licenses/>.
 *
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestV2DocumentHeaders
    {
        private CryptoImplementation _cryptoImplementation;

        public TestV2DocumentHeaders(CryptoImplementation cryptoImplementation)
        {
            _cryptoImplementation = cryptoImplementation;
        }

        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup();
            SetupAssembly.AssemblySetupCrypto(_cryptoImplementation);
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public void TestFileTimes()
        {
            V2DocumentHeaders headers = new V2DocumentHeaders(new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("v2passx")), 12);
            DateTime now = DateTime.UtcNow;
            headers.LastAccessTimeUtc = now;
            headers.LastWriteTimeUtc = now.AddHours(1);
            headers.CreationTimeUtc = now.AddHours(2);

            Assert.That(headers.LastAccessTimeUtc, Is.EqualTo(now));
            Assert.That(headers.LastWriteTimeUtc, Is.EqualTo(now.AddHours(1)));
            Assert.That(headers.CreationTimeUtc, Is.EqualTo(now.AddHours(2)));
        }

        [Test]
        public void TestCompression()
        {
            V2DocumentHeaders headers = new V2DocumentHeaders(new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("v2pass")), 10);
            headers.IsCompressed = true;
            Assert.That(headers.IsCompressed, Is.True);

            headers.IsCompressed = false;
            Assert.That(headers.IsCompressed, Is.False);
        }

        [Test]
        public void TestUnicodeFileNameShort()
        {
            V2DocumentHeaders headers = new V2DocumentHeaders(new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("v2passz")), 10);
            headers.FileName = "My Secret Document.txt";
            Assert.That(headers.FileName, Is.EqualTo("My Secret Document.txt"));
        }

        [Test]
        public void TestUnicodeFileNameLong()
        {
            V2DocumentHeaders headers = new V2DocumentHeaders(new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("v2passy")), 10);
            string longName = "When in the Course of human events, it becomes necessary for one people to dissolve the political bands which have connected them with another, and to assume among the powers of the earth, the separate and equal station to which the Laws of Nature and of Nature's God entitle them, a decent respect to the opinions of mankind requires that they should declare the causes which impel them to the separation.";
            Assert.That(longName.Length, Is.GreaterThan(256));

            headers.FileName = longName;
            Assert.That(headers.FileName, Is.EqualTo(longName));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times"), Test]
        public void TestWriteWithHmac()
        {
            V2DocumentHeaders headers = new V2DocumentHeaders(new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("v2passzz")), 20);
            byte[] output;
            V2HmacCalculator hmacCalculator = new V2HmacCalculator(new SymmetricKey(headers.GetHmacKey()));
            using (V2HmacStream<MemoryStream> hmacStream = V2HmacStream.Create<MemoryStream>(hmacCalculator, new MemoryStream()))
            {
                headers.WriteStartWithHmac(hmacStream);
                headers.WriteEndWithHmac(hmacCalculator, hmacStream, 0, 0);
                hmacStream.Flush();
                output = hmacStream.Chained.ToArray();
            }

            byte[] hmacBytesFromHeaders = new byte[V2Hmac.RequiredLength];
            Array.Copy(output, output.Length - V2Hmac.RequiredLength, hmacBytesFromHeaders, 0, V2Hmac.RequiredLength);
            V2Hmac hmacFromHeaders = new V2Hmac(hmacBytesFromHeaders);

            byte[] dataToHmac = new byte[output.Length - (V2Hmac.RequiredLength + 5)];
            Array.Copy(output, 0, dataToHmac, 0, dataToHmac.Length);

            HMACSHA512 hmac = new HMACSHA512(headers.GetHmacKey());
            hmac.TransformFinalBlock(dataToHmac, 0, dataToHmac.Length);
            V2Hmac hmacFromCalculation = new V2Hmac(hmac.Hash);

            Assert.That(hmacFromHeaders, Is.EqualTo(hmacFromCalculation));
        }

        [Test]
        public void TestLoadWithInvalidPassphrase()
        {
            Headers headers = new Headers();
            V2Aes256CryptoFactory cryptoFactory = new V2Aes256CryptoFactory();

            headers.HeaderBlocks.Add(new PreambleHeaderBlock());
            headers.HeaderBlocks.Add(new VersionHeaderBlock(new byte[] { 4, 0, 2, 0, 0 }));
            V2KeyWrapHeaderBlock originalKeyWrapBlock = new V2KeyWrapHeaderBlock(cryptoFactory, new V2DerivedKey(new Passphrase("RealKey"), 256), 10);
            V2KeyWrapHeaderBlock headerKeyWrapBlock = new V2KeyWrapHeaderBlock(originalKeyWrapBlock.GetDataBlockBytes());
            headers.HeaderBlocks.Add(headerKeyWrapBlock);
            headers.HeaderBlocks.Add(new FileInfoEncryptedHeaderBlock(new byte[0]));
            headers.HeaderBlocks.Add(new V2CompressionEncryptedHeaderBlock(new byte[1]));
            headers.HeaderBlocks.Add(new V2UnicodeFileNameInfoEncryptedHeaderBlock(new byte[0]));
            headers.HeaderBlocks.Add(new DataHeaderBlock());

            IDerivedKey key;

            key = cryptoFactory.RestoreDerivedKey(new Passphrase("WrongKey"), headerKeyWrapBlock.DerivationSalt, headerKeyWrapBlock.DerivationIterations);
            headerKeyWrapBlock.SetDerivedKey(cryptoFactory, key);

            V2DocumentHeaders documentHeaders = new V2DocumentHeaders(headerKeyWrapBlock);
            Assert.That(documentHeaders.Load(headers), Is.False);

            key = cryptoFactory.RestoreDerivedKey(new Passphrase("AnotherWrongKey"), headerKeyWrapBlock.DerivationSalt, headerKeyWrapBlock.DerivationIterations);
            headerKeyWrapBlock.SetDerivedKey(cryptoFactory, key);

            documentHeaders = new V2DocumentHeaders(headerKeyWrapBlock);
            Assert.That(documentHeaders.Load(headers), Is.False);

            key = cryptoFactory.RestoreDerivedKey(new Passphrase("RealKey"), headerKeyWrapBlock.DerivationSalt, headerKeyWrapBlock.DerivationIterations);
            headerKeyWrapBlock.SetDerivedKey(cryptoFactory, key);

            documentHeaders = new V2DocumentHeaders(headerKeyWrapBlock);
            Assert.That(documentHeaders.Load(headers), Is.True);
        }

        [Test]
        public void TestWriteStartWithHmacWithNullArgument()
        {
            V2DocumentHeaders documentHeaders = new V2DocumentHeaders(new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("Key")), 10);
            Assert.Throws<ArgumentNullException>(() => documentHeaders.WriteStartWithHmac(null));
        }

        [Test]
        public void TestHeadersPropertyGetter()
        {
            V2KeyWrapHeaderBlock keyWrap = new V2KeyWrapHeaderBlock(new V2Aes256CryptoFactory(), new V2DerivedKey(new Passphrase("Key"), 256), 256);
            V2DocumentHeaders documentHeaders = new V2DocumentHeaders(keyWrap);
            Assert.That(documentHeaders.Headers.HeaderBlocks.Count, Is.EqualTo(0));
        }

        private class UnknownEncryptedHeaderBlock : EncryptedHeaderBlock
        {
            public UnknownEncryptedHeaderBlock(byte[] dataBlock)
                : base((HeaderBlockType)199, dataBlock)
            {
            }

            public override object Clone()
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void TestUnknownEncryptedHeader()
        {
            Headers headers = new Headers();
            IDerivedKey key = new V2DerivedKey(new Passphrase("A key"), 256);
            headers.HeaderBlocks.Add(new PreambleHeaderBlock());
            headers.HeaderBlocks.Add(new VersionHeaderBlock(new byte[] { 4, 0, 2, 0, 0 }));
            V2KeyWrapHeaderBlock wrapHeader = new V2KeyWrapHeaderBlock(new V2Aes256CryptoFactory(), key, 10);
            headers.HeaderBlocks.Add(wrapHeader);
            headers.HeaderBlocks.Add(new FileInfoEncryptedHeaderBlock(new byte[0]));
            headers.HeaderBlocks.Add(new V2CompressionEncryptedHeaderBlock(new byte[1]));
            headers.HeaderBlocks.Add(new V2UnicodeFileNameInfoEncryptedHeaderBlock(new byte[0]));
            headers.HeaderBlocks.Add(new UnknownEncryptedHeaderBlock(new byte[0]));
            headers.HeaderBlocks.Add(new DataHeaderBlock());

            V2DocumentHeaders documentHeaders = new V2DocumentHeaders(wrapHeader);
            Assert.Throws<InternalErrorException>(() => documentHeaders.Load(headers));
        }

        [Test]
        public void TestAddingSingleV2AsymmetricKeyWrap()
        {
            EncryptionParameters encryptionParameters = new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("allan"));
            IAsymmetricPublicKey publicKey = TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey1);
            encryptionParameters.Add(new UserPublicKey[] { new UserPublicKey(new EmailAddress("test@test.com"), publicKey), });

            V2DocumentHeaders documentHeaders = new V2DocumentHeaders(encryptionParameters, 1000);
            IEnumerable<V2AsymmetricKeyWrapHeaderBlock> wraps = documentHeaders.Headers.HeaderBlocks.OfType<V2AsymmetricKeyWrapHeaderBlock>();
            Assert.That(wraps.Count(), Is.EqualTo(1), "There should be one V2AsymmetricKeyWrapHeaderBlock found.");

            V2AsymmetricKeyWrapHeaderBlock block1 = wraps.First();

            ICryptoFactory cryptoFactory = Resolve.CryptoFactory.Create(encryptionParameters.CryptoId);

            IAsymmetricPrivateKey privateKey1 = TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreatePrivateKey(Resources.PrivateKey1);
            block1.SetPrivateKey(cryptoFactory, privateKey1);
            ICrypto cryptoFromAsymmetricKey = block1.Crypto(0);

            V2KeyWrapHeaderBlock symmetricKeyWrap = documentHeaders.Headers.HeaderBlocks.OfType<V2KeyWrapHeaderBlock>().First();
            ICrypto cryptoFromSymmetricKey = cryptoFactory.CreateCrypto(symmetricKeyWrap.MasterKey, symmetricKeyWrap.MasterIV, 0);

            Assert.That(cryptoFromAsymmetricKey.Key, Is.EqualTo(cryptoFromSymmetricKey.Key), "The keys from Asymmetric and Symmetric should be equal.");

            IAsymmetricPrivateKey privateKey2 = TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreatePrivateKey(Resources.PrivateKey2);
            block1.SetPrivateKey(cryptoFactory, privateKey2);
            ICrypto cryptoFromAsymmetricKey1WithKey2 = block1.Crypto(0);
            Assert.That(cryptoFromAsymmetricKey1WithKey2, Is.Null, "There should be no valid key set and thus no ICrypto instance returned.");
        }

        [Test]
        public void TestAddingMultipleV2AsymmetricKeyWraps()
        {
            EncryptionParameters encryptionParameters = new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("niklas"));
            IAsymmetricPublicKey publicKey1 = TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey1);
            IAsymmetricPublicKey publicKey2 = TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey2);
            encryptionParameters.Add(new UserPublicKey[] { new UserPublicKey(new EmailAddress("test1@test.com"), publicKey1), new UserPublicKey(new EmailAddress("test2@test.com"), publicKey2), });

            V2DocumentHeaders documentHeaders = new V2DocumentHeaders(encryptionParameters, 1000);
            IEnumerable<V2AsymmetricKeyWrapHeaderBlock> wraps = documentHeaders.Headers.HeaderBlocks.OfType<V2AsymmetricKeyWrapHeaderBlock>();
            Assert.That(wraps.Count(), Is.EqualTo(2), "There should be two V2AsymmetricKeyWrapHeaderBlocks found.");

            V2AsymmetricKeyWrapHeaderBlock block1 = wraps.First();

            ICryptoFactory cryptoFactory = Resolve.CryptoFactory.Create(encryptionParameters.CryptoId);

            IAsymmetricPrivateKey privateKey1 = TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreatePrivateKey(Resources.PrivateKey1);
            block1.SetPrivateKey(cryptoFactory, privateKey1);
            ICrypto cryptoFromAsymmetricKey1 = block1.Crypto(0);

            V2KeyWrapHeaderBlock symmetricKeyWrap = documentHeaders.Headers.HeaderBlocks.OfType<V2KeyWrapHeaderBlock>().First();
            ICrypto cryptoFromSymmetricKey = cryptoFactory.CreateCrypto(symmetricKeyWrap.MasterKey, symmetricKeyWrap.MasterIV, 0);

            Assert.That(cryptoFromAsymmetricKey1.Key, Is.EqualTo(cryptoFromSymmetricKey.Key), "The keys from Asymmetric key 1 and Symmetric should be equal.");

            V2AsymmetricKeyWrapHeaderBlock block2 = wraps.Last();

            IAsymmetricPrivateKey privateKey2 = TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreatePrivateKey(Resources.PrivateKey2);
            block2.SetPrivateKey(cryptoFactory, privateKey2);
            ICrypto cryptoFromAsymmetricKey2 = block2.Crypto(0);
            Assert.That(cryptoFromAsymmetricKey2.Key, Is.EqualTo(cryptoFromSymmetricKey.Key), "The keys from Asymmetric key 2 and Symmetric should be equal.");

            block1.SetPrivateKey(cryptoFactory, privateKey2);
            ICrypto cryptoFromAsymmetricKey1WithKey2 = block1.Crypto(0);
            Assert.That(cryptoFromAsymmetricKey1WithKey2, Is.Null, "There should be no valid key set and thus no ICrypto instance returned.");

            block2.SetPrivateKey(cryptoFactory, privateKey1);
            ICrypto cryptoFromAsymmetricKey2WithKey1 = block2.Crypto(0);
            Assert.That(cryptoFromAsymmetricKey2WithKey1, Is.Null, "There should be no valid key set and thus no ICrypto instance returned.");
        }
    }
}