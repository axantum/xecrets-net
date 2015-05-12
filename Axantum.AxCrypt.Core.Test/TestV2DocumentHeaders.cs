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
            using (V2DocumentHeaders headers = new V2DocumentHeaders(new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("v2passx")), 12))
            {
                DateTime now = DateTime.UtcNow;
                headers.LastAccessTimeUtc = now;
                headers.LastWriteTimeUtc = now.AddHours(1);
                headers.CreationTimeUtc = now.AddHours(2);

                Assert.That(headers.LastAccessTimeUtc, Is.EqualTo(now));
                Assert.That(headers.LastWriteTimeUtc, Is.EqualTo(now.AddHours(1)));
                Assert.That(headers.CreationTimeUtc, Is.EqualTo(now.AddHours(2)));
            }
        }

        [Test]
        public void TestCompression()
        {
            using (V2DocumentHeaders headers = new V2DocumentHeaders(new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("v2pass")), 10))
            {
                headers.IsCompressed = true;
                Assert.That(headers.IsCompressed, Is.True);

                headers.IsCompressed = false;
                Assert.That(headers.IsCompressed, Is.False);
            }
        }

        [Test]
        public void TestUnicodeFileNameShort()
        {
            using (V2DocumentHeaders headers = new V2DocumentHeaders(new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("v2passz")), 10))
            {
                headers.FileName = "My Secret Document.txt";
                Assert.That(headers.FileName, Is.EqualTo("My Secret Document.txt"));
            }
        }

        [Test]
        public void TestUnicodeFileNameLong()
        {
            using (V2DocumentHeaders headers = new V2DocumentHeaders(new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("v2passy")), 10))
            {
                string longName = "When in the Course of human events, it becomes necessary for one people to dissolve the political bands which have connected them with another, and to assume among the powers of the earth, the separate and equal station to which the Laws of Nature and of Nature's God entitle them, a decent respect to the opinions of mankind requires that they should declare the causes which impel them to the separation.";
                Assert.That(longName.Length, Is.GreaterThan(256));

                headers.FileName = longName;
                Assert.That(headers.FileName, Is.EqualTo(longName));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times"), Test]
        public void TestWriteWithHmac()
        {
            using (V2DocumentHeaders headers = new V2DocumentHeaders(new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("v2passzz")), 20))
            {
                byte[] output;
                using (MemoryStream outputStream = new MemoryStream())
                {
                    using (V2HmacStream hmacStream = new V2HmacStream(headers.GetHmacKey(), outputStream))
                    {
                        headers.WriteStartWithHmac(hmacStream);
                        headers.WriteEndWithHmac(hmacStream, 0, 0);
                    }
                    output = outputStream.ToArray();
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

            using (V2DocumentHeaders documentHeaders = new V2DocumentHeaders(headerKeyWrapBlock))
            {
                Assert.That(documentHeaders.Load(headers), Is.False);
            }

            key = cryptoFactory.RestoreDerivedKey(new Passphrase("AnotherWrongKey"), headerKeyWrapBlock.DerivationSalt, headerKeyWrapBlock.DerivationIterations);
            headerKeyWrapBlock.SetDerivedKey(cryptoFactory, key);

            using (V2DocumentHeaders documentHeaders = new V2DocumentHeaders(headerKeyWrapBlock))
            {
                Assert.That(documentHeaders.Load(headers), Is.False);
            }

            key = cryptoFactory.RestoreDerivedKey(new Passphrase("RealKey"), headerKeyWrapBlock.DerivationSalt, headerKeyWrapBlock.DerivationIterations);
            headerKeyWrapBlock.SetDerivedKey(cryptoFactory, key);

            using (V2DocumentHeaders documentHeaders = new V2DocumentHeaders(headerKeyWrapBlock))
            {
                Assert.That(documentHeaders.Load(headers), Is.True);
            }
        }

        [Test]
        public void TestWriteStartWithHmacWithNullArgument()
        {
            using (V2DocumentHeaders documentHeaders = new V2DocumentHeaders(new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("Key")), 10))
            {
                Assert.Throws<ArgumentNullException>(() => documentHeaders.WriteStartWithHmac(null));
            }
        }

        [Test]
        public void TestHeadersPropertyGetter()
        {
            V2KeyWrapHeaderBlock keyWrap = new V2KeyWrapHeaderBlock(new V2Aes256CryptoFactory(), new V2DerivedKey(new Passphrase("Key"), 256), 256);
            using (V2DocumentHeaders documentHeaders = new V2DocumentHeaders(keyWrap))
            {
                Assert.That(documentHeaders.Headers.HeaderBlocks.Count, Is.EqualTo(0));
            }
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

            using (V2DocumentHeaders documentHeaders = new V2DocumentHeaders(wrapHeader))
            {
                Assert.Throws<InternalErrorException>(() => documentHeaders.Load(headers));
            }
        }

        private const string _privateKey1 = @"-----BEGIN RSA PRIVATE KEY-----
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
-----END RSA PRIVATE KEY-----
";

        private const string _publicKey1 = @"-----BEGIN PUBLIC KEY-----
MIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEAnJ8Tk4OBEL61g5iicA2F
4eEQM2qG+Qoo8BwAVYs8kl6lBNoKGcbyUNcSUw78h05u9OYgMgluFrnbElNxDWa8
K9XcG3mP60baMEnYrCwvpUK7jw2FaMjvfTipEKuB7vUGzRfvR726/qqoBNKYqGe5
INYFUL/JyPCuEdKkmzPCvkREgsEI6wsnfJEdqteQYx5hwqXkm01A1Ov2fRISwaO2
KEZp3mEfZygIaTscpBZyhu/LfsKKvaTe03EgLKciL/oKuHnam4i4NWDOgxvYHdhS
GhOmDyu4DMc1HmtHTWjI53kVKLvqO3Z5oTe8LmAI8I5uzzzI6jhcOD4BdvblgU87
AhFQ4jZTlCUsQ0i3wI4J1iNnEkB4+3g5VEypoPOee7zCeMrZyznMnfsIR49vS+Im
r/A6UhGsAUhQJseZXKkwfTib7xgst+Mz0q0hUw+6z1WRrbMrl6WaDKuk5Q3HDDqN
xq6pI+MEEDNC6OAh6GA+4+4Fu3N4mqwmhNICjDuaMwPco1JUxhhL1G7Xo9q41yKA
NkXVQwlbcX385q9ntUSwDKaPF5kzdSRJIRlfLC5ysS4plKRqXBVO4Frv3ejcQFXN
5x1VQTJJzGHJhsNQ0lVL3MjyFAfiIivJORTk2jNAPkaOl9ei9IS6H7jAQcqrERXI
vEASZTyAeQlcXprhbyZ2nf0CAwEAAQ==
-----END PUBLIC KEY-----
";


        private const string _privateKey2 = @"-----BEGIN RSA PRIVATE KEY-----
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
-----END RSA PRIVATE KEY-----
";

        private const string _publicKey2 = @"-----BEGIN PUBLIC KEY-----
MIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEAoGjcmNAHjUk2l8bMZ/eL
upjEhYFOZCbsNiDwITQFTF7j/9v9t4inz2cW4/DFrfWV+gXVBzWzc5w75K2cafSJ
MbdRfDKzeV9HLRV56SLa1gFphgBtksKadve5pqrn15MozMpfQt8GNYjxHiTyfdm+
4TDSBYn1QjMKHNuRwHu8AygzlQ2dmW12icWDUjTiWUnw/6wLZMMPoU0ZRyHkr+Lc
HxYnkS5Q46t15ziDAmHFijSO14y0L6oI70v6joEEFNUsjHFEFVtWxbz0lfoag9vC
Op9K+ReS7AUFdAifS87HSDHNcevVvRg7MzlG9rcyuFktucOLdQcPv7B86IRvagfJ
IdbE1e9oOw3irLiE9RT60e0ivNhnusB36paZJnf1k7FMcwWkRvcnzcrdSyElSBSs
kkwP1gXnUgRe39R/zDSTkqtE3N+r8hjhnCvsqxzQdmQyDGTNZRF+CNWKpk6k4ToL
nVV9oY3J4GNXQ5NdITUWVMDrUQ2kNO+GdRvPnfRq/r8SJ3rRG2W2co6QTFk5YWvM
MyZzGpI7jici5x/WM5igA+6ifkotngwpqU16M7yS2xMIxKVTw9CaoIX2jgNolgGA
yhkO4poTyn0zanedSJdPn/kW96j0u3WCoRo4K+eOB7Pcv/qC82Az+AQbTEqQnRea
8iTTI65co5+BXSxQvyjM4wMCAwEAAQ==
-----END PUBLIC KEY-----
";

        [Test]
        public void TestAddingSingleV2AsymmetricKeyWrap()
        {
            EncryptionParameters encryptionParameters = new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("allan"));
            IAsymmetricPublicKey publicKey = TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreatePublicKey(_publicKey1);
            encryptionParameters.Add(new IAsymmetricPublicKey[] { publicKey, });

            using (V2DocumentHeaders documentHeaders = new V2DocumentHeaders(encryptionParameters, 1000))
            {
                IEnumerable<V2AsymmetricKeyWrapHeaderBlock> wraps = documentHeaders.Headers.HeaderBlocks.OfType<V2AsymmetricKeyWrapHeaderBlock>();
                Assert.That(wraps.Count(), Is.EqualTo(1), "There should be one V2AsymmetricKeyWrapHeaderBlock found.");

                V2AsymmetricKeyWrapHeaderBlock block1 = wraps.First();

                ICryptoFactory cryptoFactory = Resolve.CryptoFactory.Create(encryptionParameters.CryptoId);

                IAsymmetricPrivateKey privateKey1 = TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreatePrivateKey(_privateKey1);
                block1.SetPrivateKey(cryptoFactory, privateKey1);
                ICrypto cryptoFromAsymmetricKey = block1.Crypto(0);
                
                V2KeyWrapHeaderBlock symmetricKeyWrap = documentHeaders.Headers.HeaderBlocks.OfType<V2KeyWrapHeaderBlock>().First();
                ICrypto cryptoFromSymmetricKey = cryptoFactory.CreateCrypto(symmetricKeyWrap.MasterKey, symmetricKeyWrap.MasterIV, 0);

                Assert.That(cryptoFromAsymmetricKey.Key, Is.EqualTo(cryptoFromSymmetricKey.Key), "The keys from Asymmetric and Symmetric should be equal.");

                IAsymmetricPrivateKey privateKey2 = TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreatePrivateKey(_privateKey2);
                block1.SetPrivateKey(cryptoFactory, privateKey2);
                ICrypto cryptoFromAsymmetricKey1WithKey2 = block1.Crypto(0);
                Assert.That(cryptoFromAsymmetricKey1WithKey2.Key, Is.Not.EqualTo(cryptoFromSymmetricKey.Key), "The keys from Asymmetric key 1 decrypted with key 2 and Symmetric should not be equal.");
            }
        }

        [Test]
        public void TestAddingMultipleV2AsymmetricKeyWraps()
        {
            EncryptionParameters encryptionParameters = new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("niklas"));
            IAsymmetricPublicKey publicKey1 = TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreatePublicKey(_publicKey1);
            IAsymmetricPublicKey publicKey2 = TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreatePublicKey(_publicKey2);
            encryptionParameters.Add(new IAsymmetricPublicKey[] { publicKey1, publicKey2, });

            using (V2DocumentHeaders documentHeaders = new V2DocumentHeaders(encryptionParameters, 1000))
            {
                IEnumerable<V2AsymmetricKeyWrapHeaderBlock> wraps = documentHeaders.Headers.HeaderBlocks.OfType<V2AsymmetricKeyWrapHeaderBlock>();
                Assert.That(wraps.Count(), Is.EqualTo(2), "There should be two V2AsymmetricKeyWrapHeaderBlocks found.");

                V2AsymmetricKeyWrapHeaderBlock block1 = wraps.First();

                ICryptoFactory cryptoFactory = Resolve.CryptoFactory.Create(encryptionParameters.CryptoId);

                IAsymmetricPrivateKey privateKey1 = TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreatePrivateKey(_privateKey1);
                block1.SetPrivateKey(cryptoFactory, privateKey1);
                ICrypto cryptoFromAsymmetricKey1 = block1.Crypto(0);

                V2KeyWrapHeaderBlock symmetricKeyWrap = documentHeaders.Headers.HeaderBlocks.OfType<V2KeyWrapHeaderBlock>().First();
                ICrypto cryptoFromSymmetricKey = cryptoFactory.CreateCrypto(symmetricKeyWrap.MasterKey, symmetricKeyWrap.MasterIV, 0);

                Assert.That(cryptoFromAsymmetricKey1.Key, Is.EqualTo(cryptoFromSymmetricKey.Key), "The keys from Asymmetric key 1 and Symmetric should be equal.");

                V2AsymmetricKeyWrapHeaderBlock block2 = wraps.Last();

                IAsymmetricPrivateKey privateKey2 = TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreatePrivateKey(_privateKey2);
                block2.SetPrivateKey(cryptoFactory, privateKey2);
                ICrypto cryptoFromAsymmetricKey2 = block2.Crypto(0);
                Assert.That(cryptoFromAsymmetricKey2.Key, Is.EqualTo(cryptoFromSymmetricKey.Key), "The keys from Asymmetric key 2 and Symmetric should be equal.");

                block1.SetPrivateKey(cryptoFactory, privateKey2);
                ICrypto cryptoFromAsymmetricKey1WithKey2 = block1.Crypto(0);
                Assert.That(cryptoFromAsymmetricKey1WithKey2.Key, Is.Not.EqualTo(cryptoFromSymmetricKey.Key), "The keys from Asymmetric key 1 decrypted with key 2 and Symmetric should not be equal.");

                block2.SetPrivateKey(cryptoFactory, privateKey1);
                ICrypto cryptoFromAsymmetricKey2WithKey1 = block2.Crypto(0);
                Assert.That(cryptoFromAsymmetricKey2WithKey1.Key, Is.Not.EqualTo(cryptoFromSymmetricKey.Key), "The keys from Asymmetric key 1 decrypted with key 2 and Symmetric should not be equal.");
            }
        }
    }
}