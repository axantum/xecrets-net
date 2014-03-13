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

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestV2DocumentHeaders
    {
        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestFileTimes()
        {
            using (V2DocumentHeaders headers = new V2DocumentHeaders(new V2AesCrypto(new V2Passphrase("v2passx", 256), new SymmetricIV(128)), 10))
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
        public static void TestCompression()
        {
            using (V2DocumentHeaders headers = new V2DocumentHeaders(new V2AesCrypto(new V2Passphrase("v2pass", 256), new SymmetricIV(128)), 10))
            {
                headers.IsCompressed = true;
                Assert.That(headers.IsCompressed, Is.True);

                headers.IsCompressed = false;
                Assert.That(headers.IsCompressed, Is.False);
            }
        }

        [Test]
        public static void TestUnicodeFileNameShort()
        {
            using (V2DocumentHeaders headers = new V2DocumentHeaders(new V2AesCrypto(new V2Passphrase("v2passz", 256), new SymmetricIV(128)), 10))
            {
                headers.FileName = "My Secret Document.txt";
                Assert.That(headers.FileName, Is.EqualTo("My Secret Document.txt"));
            }
        }

        [Test]
        public static void TestUnicodeFileNameLong()
        {
            using (V2DocumentHeaders headers = new V2DocumentHeaders(new V2AesCrypto(new V2Passphrase("v2passy", 256), new SymmetricIV(128)), 10))
            {
                string longName = "When in the Course of human events, it becomes necessary for one people to dissolve the political bands which have connected them with another, and to assume among the powers of the earth, the separate and equal station to which the Laws of Nature and of Nature's God entitle them, a decent respect to the opinions of mankind requires that they should declare the causes which impel them to the separation.";
                Assert.That(longName.Length, Is.GreaterThan(256));

                headers.FileName = longName;
                Assert.That(headers.FileName, Is.EqualTo(longName));
            }
        }

        [Test]
        public static void TestWriteWithHmac()
        {
            using (V2DocumentHeaders headers = new V2DocumentHeaders(new V2AesCrypto(new V2Passphrase("v2passzz", 256), new SymmetricIV(128)), 20))
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
        public static void TestLoadWithInvalidPassphrase()
        {
            Headers headers = new Headers();

            ICrypto realKeyEncryptingCrypto = new V2AesCrypto(new V2Passphrase("RealKey", 256), new SymmetricIV(128));
            headers.HeaderBlocks.Add(new PreambleHeaderBlock());
            headers.HeaderBlocks.Add(new VersionHeaderBlock(new byte[] { 4, 0, 2, 0, 0 }));
            headers.HeaderBlocks.Add(new V2KeyWrapHeaderBlock(realKeyEncryptingCrypto, 10));
            headers.HeaderBlocks.Add(new FileInfoEncryptedHeaderBlock(new byte[0]));
            headers.HeaderBlocks.Add(new V2CompressionEncryptedHeaderBlock(new byte[1]));
            headers.HeaderBlocks.Add(new V2UnicodeFileNameInfoEncryptedHeaderBlock(new byte[0]));
            headers.HeaderBlocks.Add(new DataHeaderBlock());

            using (V2DocumentHeaders documentHeaders = new V2DocumentHeaders(new V2AesCrypto(new V2Passphrase("WrongKey", 256), new SymmetricIV(128))))
            {
                Assert.That(documentHeaders.Load(headers), Is.False);
            }
        }

        [Test]
        public static void TestWriteStartWithHmacWithNullArgument()
        {
            using (V2DocumentHeaders documentHeaders = new V2DocumentHeaders(new V2AesCrypto(new V2Passphrase("Key", 256), new SymmetricIV(128))))
            {
                Assert.Throws<ArgumentNullException>(() => documentHeaders.WriteStartWithHmac(null));
            }
        }

        [Test]
        public static void TestKeyEncryptingCryptoPropertyGetter()
        {
            ICrypto keyEncryptingCrypto = new V2AesCrypto(new V2Passphrase("Key", 256), new SymmetricIV(128));
            using (V2DocumentHeaders documentHeaders = new V2DocumentHeaders(keyEncryptingCrypto))
            {
                Assert.That(Object.ReferenceEquals(keyEncryptingCrypto, documentHeaders.KeyEncryptingCrypto));
            }
        }

        [Test]
        public static void TestHeadersPropertyGetter()
        {
            ICrypto keyEncryptingCrypto = new V2AesCrypto(new V2Passphrase("Key", 256), new SymmetricIV(128));
            using (V2DocumentHeaders documentHeaders = new V2DocumentHeaders(keyEncryptingCrypto))
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
        public static void TestUnkonwnEncryptedHeader()
        {
            Headers headers = new Headers();

            ICrypto realKeyEncryptingCrypto = new V2AesCrypto(new V2Passphrase("A key", 256), new SymmetricIV(128));
            headers.HeaderBlocks.Add(new PreambleHeaderBlock());
            headers.HeaderBlocks.Add(new VersionHeaderBlock(new byte[] { 4, 0, 2, 0, 0 }));
            headers.HeaderBlocks.Add(new V2KeyWrapHeaderBlock(realKeyEncryptingCrypto, 10));
            headers.HeaderBlocks.Add(new FileInfoEncryptedHeaderBlock(new byte[0]));
            headers.HeaderBlocks.Add(new V2CompressionEncryptedHeaderBlock(new byte[1]));
            headers.HeaderBlocks.Add(new V2UnicodeFileNameInfoEncryptedHeaderBlock(new byte[0]));
            headers.HeaderBlocks.Add(new UnknownEncryptedHeaderBlock(new byte[0]));
            headers.HeaderBlocks.Add(new DataHeaderBlock());

            using (V2DocumentHeaders documentHeaders = new V2DocumentHeaders(new V2AesCrypto(new V2Passphrase("A key", 256), new SymmetricIV(128))))
            {
                Assert.Throws<InternalErrorException>(() => documentHeaders.Load(headers));
            }
        }
    }
}