#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Test.Properties;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestDocumentHeaders
    {
        private class AxCryptReaderForTest : AxCryptStreamReader
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
                    DocumentHeaders documentHeaders = new DocumentHeaders(new AesKey());
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
            DocumentHeaders documentHeaders = new DocumentHeaders(new AesKey());
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
                documentHeaders.Hmac = null;
            });
        }

        [Test]
        public static void TestBadKey()
        {
            using (Stream testStream = new MemoryStream(Resources.HelloWorld_Key_a_txt))
            {
                using (AxCryptReader reader = AxCryptReader.Create(testStream))
                {
                    Passphrase passphrase = new Passphrase("b");
                    DocumentHeaders documentHeaders = new DocumentHeaders(passphrase.DerivedPassphrase);
                    bool isPassphraseValid = documentHeaders.Load(reader);

                    Assert.That(isPassphraseValid, Is.False, "The passphrase is intentionally wrong for this test case.");
                    Assert.That(documentHeaders.HmacSubkey, Is.Null, "Since the passphrase is wrong, HmacSubkey should return null.");
                    Assert.That(documentHeaders.DataSubkey, Is.Null, "Since the passphrase is wrong, DataSubkey should return null.");
                    Assert.That(documentHeaders.HeadersSubkey, Is.Null, "Since the passphrase is wrong, HeadersSubkey should return null.");
                }
            }
        }

        private class DocumentHeadersForTest : DocumentHeaders
        {
            public DocumentHeadersForTest(AesKey keyEncryptingKey)
                : base(keyEncryptingKey)
            {
            }

            public void SetNextFileVersionMajor()
            {
                VersionHeaderBlock versionHeaderBlock = VersionHeaderBlock;
                versionHeaderBlock.FileVersionMajor = (byte)(versionHeaderBlock.FileVersionMajor + 1);
            }
        }

        [Test]
        public static void TestDecryptOfTooNewFileVersion()
        {
            DateTime creationTimeUtc = new DateTime(2012, 1, 1, 1, 2, 3, DateTimeKind.Utc);
            DateTime lastAccessTimeUtc = creationTimeUtc + new TimeSpan(1, 0, 0);
            DateTime lastWriteTimeUtc = creationTimeUtc + new TimeSpan(2, 0, 0); ;
            using (Stream inputStream = new MemoryStream(Encoding.UTF8.GetBytes("AxCrypt is Great!")))
            {
                using (Stream outputStream = new MemoryStream())
                {
                    using (AxCryptDocument document = new AxCryptDocument())
                    {
                        Passphrase passphrase = new Passphrase("a");
                        DocumentHeadersForTest headers = new DocumentHeadersForTest(passphrase.DerivedPassphrase);
                        headers.FileName = "MyFile.txt";
                        headers.CreationTimeUtc = creationTimeUtc;
                        headers.LastAccessTimeUtc = lastAccessTimeUtc;
                        headers.LastWriteTimeUtc = lastWriteTimeUtc;
                        headers.SetNextFileVersionMajor();
                        document.DocumentHeaders = headers;
                        document.EncryptTo(headers, inputStream, outputStream, AxCryptOptions.EncryptWithoutCompression);
                    }
                    outputStream.Position = 0;
                    using (AxCryptDocument document = new AxCryptDocument())
                    {
                        Passphrase passphrase = new Passphrase("a");
                        Assert.Throws<FileFormatException>(() => { document.Load(outputStream, passphrase); });
                    }
                }
            }
        }
    }
}