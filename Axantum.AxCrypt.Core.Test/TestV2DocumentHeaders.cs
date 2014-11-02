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
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using NUnit.Framework;
using System;
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
            using (V2DocumentHeaders headers = new V2DocumentHeaders(new Passphrase("v2passx"), V2Aes256CryptoFactory.CryptoId, 12))
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
            using (V2DocumentHeaders headers = new V2DocumentHeaders(new Passphrase("v2pass"), V2Aes256CryptoFactory.CryptoId, 10))
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
            using (V2DocumentHeaders headers = new V2DocumentHeaders(new Passphrase("v2passz"), V2Aes256CryptoFactory.CryptoId, 10))
            {
                headers.FileName = "My Secret Document.txt";
                Assert.That(headers.FileName, Is.EqualTo("My Secret Document.txt"));
            }
        }

        [Test]
        public void TestUnicodeFileNameLong()
        {
            using (V2DocumentHeaders headers = new V2DocumentHeaders(new Passphrase("v2passy"), V2Aes256CryptoFactory.CryptoId, 10))
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
            using (V2DocumentHeaders headers = new V2DocumentHeaders(new Passphrase("v2passzz"), V2Aes256CryptoFactory.CryptoId, 20))
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

            headers.HeaderBlocks.Add(new PreambleHeaderBlock());
            headers.HeaderBlocks.Add(new VersionHeaderBlock(new byte[] { 4, 0, 2, 0, 0 }));
            headers.HeaderBlocks.Add(new V2KeyWrapHeaderBlock(new V2Aes256CryptoFactory(), new V2DerivedKey(new Passphrase("RealKey"), 256), 10));
            headers.HeaderBlocks.Add(new FileInfoEncryptedHeaderBlock(new byte[0]));
            headers.HeaderBlocks.Add(new V2CompressionEncryptedHeaderBlock(new byte[1]));
            headers.HeaderBlocks.Add(new V2UnicodeFileNameInfoEncryptedHeaderBlock(new byte[0]));
            headers.HeaderBlocks.Add(new DataHeaderBlock());

            using (V2DocumentHeaders documentHeaders = new V2DocumentHeaders(new Passphrase("WrongKey"), V2Aes256CryptoFactory.CryptoId, 10))
            {
                Assert.That(documentHeaders.Load(headers), Is.False);
            }
        }

        [Test]
        public void TestWriteStartWithHmacWithNullArgument()
        {
            using (V2DocumentHeaders documentHeaders = new V2DocumentHeaders(new Passphrase("Key"), V2Aes256CryptoFactory.CryptoId, 10))
            {
                Assert.Throws<ArgumentNullException>(() => documentHeaders.WriteStartWithHmac(null));
            }
        }

        [Test]
        public void TestHeadersPropertyGetter()
        {
            using (V2DocumentHeaders documentHeaders = new V2DocumentHeaders(new V2DerivedKey(new Passphrase("Key"), 256), V2Aes256CryptoFactory.CryptoId))
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
            headers.HeaderBlocks.Add(new V2KeyWrapHeaderBlock(new V2Aes256CryptoFactory(), key, 10));
            headers.HeaderBlocks.Add(new FileInfoEncryptedHeaderBlock(new byte[0]));
            headers.HeaderBlocks.Add(new V2CompressionEncryptedHeaderBlock(new byte[1]));
            headers.HeaderBlocks.Add(new V2UnicodeFileNameInfoEncryptedHeaderBlock(new byte[0]));
            headers.HeaderBlocks.Add(new UnknownEncryptedHeaderBlock(new byte[0]));
            headers.HeaderBlocks.Add(new DataHeaderBlock());

            using (V2DocumentHeaders documentHeaders = new V2DocumentHeaders(key, V2Aes256CryptoFactory.CryptoId))
            {
                Assert.Throws<InternalErrorException>(() => documentHeaders.Load(headers));
            }
        }
    }
}