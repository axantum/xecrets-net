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
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;
using System;
using System.IO;
using System.Text;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestDocumentHeaders
    {
        private class AxCryptReaderForTest : V1AxCryptReader
        {
            public AxCryptReaderForTest(Stream inputStream)
                : base(inputStream)
            {
            }

            public override bool Read()
            {
                bool isOk = base.Read();
                if (CurrentItemType != AxCryptItemType.MagicGuid)
                {
                    CurrentItemType = (AxCryptItemType)666;
                }
                return isOk;
            }
        }

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
        public static void TestInvalidItemType()
        {
            using (MemoryStream inputStream = new MemoryStream())
            {
                AxCrypt1Guid.Write(inputStream);
                new PreambleHeaderBlock().Write(inputStream);
                inputStream.Position = 0;
                using (AxCryptReaderForTest axCryptReader = new AxCryptReaderForTest(inputStream))
                {
                    V1DocumentHeaders documentHeaders = new V1DocumentHeaders(new V1AesCrypto(new AesKey(128)));
                    Assert.Throws<InternalErrorException>(() =>
                    {
                        documentHeaders.Load(axCryptReader);
                    });
                }
            }
        }

        [Test]
        public static void TestBadArguments()
        {
            V1DocumentHeaders documentHeaders = new V1DocumentHeaders(new V1AesCrypto(new AesKey(128)));
            Assert.Throws<ArgumentNullException>(() =>
            {
                documentHeaders.WriteWithHmac(null);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                documentHeaders.WriteWithoutHmac(null);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                documentHeaders.Headers.Hmac = null;
            });
        }

        [Test]
        public static void TestKeyEncryptingKey()
        {
            AesKey keyEncryptingKey = new AesKey(128);
            V1DocumentHeaders headers = new V1DocumentHeaders(new V1AesCrypto(keyEncryptingKey));
            Assert.That(headers.KeyEncryptingKey, Is.EqualTo(keyEncryptingKey), "Unexpected key encrypting key retrieved.");
        }

        [Test]
        public static void TestBadKey()
        {
            using (Stream testStream = FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.helloworld_key_a_txt))
            {
                using (AxCryptReader reader = new V1AxCryptReader(testStream))
                {
                    Passphrase passphrase = new Passphrase("b");
                    V1DocumentHeaders documentHeaders = new V1DocumentHeaders(new V1AesCrypto(passphrase.DerivedPassphrase));
                    bool isPassphraseValid = documentHeaders.Load(reader);

                    Assert.That(isPassphraseValid, Is.False, "The passphrase is intentionally wrong for this test case.");
                    Assert.That(documentHeaders.HmacSubkey, Is.Null, "Since the passphrase is wrong, HmacSubkey should return null.");
                    Assert.That(documentHeaders.DataSubkey, Is.Null, "Since the passphrase is wrong, DataSubkey should return null.");
                    Assert.That(documentHeaders.HeadersSubkey, Is.Null, "Since the passphrase is wrong, HeadersSubkey should return null.");
                }
            }
        }

        [Test]
        public static void TestDecryptOfTooNewFileVersion()
        {
            DateTime creationTimeUtc = new DateTime(2012, 1, 1, 1, 2, 3, DateTimeKind.Utc);
            DateTime lastAccessTimeUtc = creationTimeUtc + new TimeSpan(1, 0, 0);
            DateTime lastWriteTimeUtc = creationTimeUtc + new TimeSpan(2, 0, 0); ;
            using (Stream inputStream = FakeRuntimeFileInfo.ExpandableMemoryStream(Encoding.UTF8.GetBytes("AxCrypt is Great!")))
            {
                using (Stream outputStream = new MemoryStream())
                {
                    Passphrase passphrase = new Passphrase("a");
                    using (V1AxCryptDocument document = new V1AxCryptDocument(new V1AesCrypto(passphrase.DerivedPassphrase)))
                    {
                        document.DocumentHeaders.FileName = "MyFile.txt";
                        document.DocumentHeaders.CreationTimeUtc = creationTimeUtc;
                        document.DocumentHeaders.LastAccessTimeUtc = lastAccessTimeUtc;
                        document.DocumentHeaders.LastWriteTimeUtc = lastWriteTimeUtc;
                        VersionHeaderBlock versionHeaderBlock = document.DocumentHeaders.VersionHeaderBlock;
                        versionHeaderBlock.FileVersionMajor = (byte)(versionHeaderBlock.FileVersionMajor + 1);
                        document.EncryptTo(inputStream, outputStream, AxCryptOptions.EncryptWithoutCompression, new ProgressContext());
                    }
                    outputStream.Position = 0;
                    using (V1AxCryptDocument document = new V1AxCryptDocument(new V1AesCrypto(passphrase.DerivedPassphrase)))
                    {
                        Assert.Throws<FileFormatException>(() => { document.Load(outputStream); });
                    }
                }
            }
        }
    }
}