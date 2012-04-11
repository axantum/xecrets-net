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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Test.Properties;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestAxCryptDocument
    {
        [Test]
        public static void TestAnsiFileNameFromSimpleFile()
        {
            using (Stream testStream = new MemoryStream(Resources.HelloWorld_Key_a_txt))
            {
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    AxCryptReaderSettings settings = new AxCryptReaderSettings("a");
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                    {
                        bool keyIsOk = document.Load(axCryptReader, settings);
                        Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                        string fileName = document.DocumentHeaders.AnsiFileName;
                        Assert.That(fileName, Is.EqualTo("HelloWorld-Key-a.txt"), "Wrong file name");
                    }
                }
            }
        }

        [Test]
        public static void TestUnicodeFileNameFromSimpleFile()
        {
            using (Stream testStream = new MemoryStream(Resources.HelloWorld_Key_a_txt))
            {
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    AxCryptReaderSettings settings = new AxCryptReaderSettings("a");
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                    {
                        bool keyIsOk = document.Load(axCryptReader, settings);
                        Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                        string fileName = document.DocumentHeaders.UnicodeFileName;
                        Assert.That(fileName, Is.EqualTo("HelloWorld-Key-a.txt"), "Wrong file name");
                    }
                }
            }
        }

        [Test]
        public static void TestFileNameFromSimpleFileWithUnicode()
        {
            using (Stream testStream = new MemoryStream(Resources.HelloWorld_Key_a_txt))
            {
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    AxCryptReaderSettings settings = new AxCryptReaderSettings("a");
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                    {
                        bool keyIsOk = document.Load(axCryptReader, settings);
                        Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                        string fileName = document.DocumentHeaders.FileName;
                        Assert.That(fileName, Is.EqualTo("HelloWorld-Key-a.txt"), "Wrong file name");
                    }
                }
            }
        }

        [Test]
        public static void TestHmacFromSimpleFile()
        {
            DataHmac expectedHmac = new DataHmac(new byte[] { 0xF9, 0xAF, 0x2E, 0x67, 0x7D, 0xCF, 0xC9, 0xFE, 0x06, 0x4B, 0x39, 0x08, 0xE7, 0x5A, 0x87, 0x81 });
            using (Stream testStream = new MemoryStream(Resources.HelloWorld_Key_a_txt))
            {
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    AxCryptReaderSettings settings = new AxCryptReaderSettings("a");
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                    {
                        bool keyIsOk = document.Load(axCryptReader, settings);
                        Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                        DataHmac hmac = document.DocumentHeaders.GetHmac();
                        Assert.That(hmac.GetBytes(), Is.EqualTo(expectedHmac.GetBytes()), "Wrong HMAC");
                    }
                }
            }
        }

        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "This is a test, and they should start with 'Test'.")]
        public static void TestIsCompressedFromSimpleFile()
        {
            using (Stream testStream = new MemoryStream(Resources.HelloWorld_Key_a_txt))
            {
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    AxCryptReaderSettings settings = new AxCryptReaderSettings("a");
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                    {
                        bool keyIsOk = document.Load(axCryptReader, settings);
                        Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                        bool isCompressed = document.DocumentHeaders.IsCompressed;
                        Assert.That(isCompressed, Is.False, "This file should not be compressed.");
                    }
                }
            }
        }

        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "This is a test, and they should start with 'Test'.")]
        public static void TestInvalidPassphraseWithSimpleFile()
        {
            using (Stream testStream = new MemoryStream(Resources.HelloWorld_Key_a_txt))
            {
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    AxCryptReaderSettings settings = new AxCryptReaderSettings("b");
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                    {
                        bool keyIsOk = document.Load(axCryptReader, settings);
                        Assert.That(keyIsOk, Is.False, "The passphrase provided is wrong!");
                    }
                }
            }
        }

        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "This is a test, and they should start with 'Test'.")]
        public static void TestIsCompressedFromLargerFile()
        {
            using (Stream testStream = new MemoryStream(Resources.David_Copperfield_Key_Å_ä_Ö_txt))
            {
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    AxCryptReaderSettings settings = new AxCryptReaderSettings("Å ä Ö");
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                    {
                        bool keyIsOk = document.Load(axCryptReader, settings);
                        Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                        bool isCompressed = document.DocumentHeaders.IsCompressed;
                        Assert.That(isCompressed, Is.True, "This file should be compressed.");
                    }
                }
            }
        }

        [Test]
        public static void TestDecryptUncompressedFromSimpleFile()
        {
            using (Stream testStream = new MemoryStream(Resources.HelloWorld_Key_a_txt))
            {
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    AxCryptReaderSettings settings = new AxCryptReaderSettings("a");
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                    {
                        bool keyIsOk = document.Load(axCryptReader, settings);
                        Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                        using (MemoryStream plaintextStream = new MemoryStream())
                        {
                            document.DecryptTo(axCryptReader, plaintextStream);
                            Assert.That(Encoding.ASCII.GetString(plaintextStream.GetBuffer(), 0, (int)plaintextStream.Length), Is.EqualTo("HelloWorld"), "Unexpected result of decryption.");
                            Assert.That(document.DocumentHeaders.PlaintextLength, Is.EqualTo(10), "'HelloWorld' should be 10 bytes uncompressed plaintext.");
                        }
                    }
                }
            }
        }

        [Test]
        public static void TestDecryptCompressedFromLegacy0B6()
        {
            using (Stream testStream = new MemoryStream(Resources.Tst_0_0b6___Key__åäö____Medium____html__))
            {
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    AxCryptReaderSettings settings = new AxCryptReaderSettings("åäö");
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                    {
                        bool keyIsOk = document.Load(axCryptReader, settings);
                        Assert.That(keyIsOk, Is.True, "A correct passphrase was provided, but it was not accepted.");
                        Assert.That(document.DocumentHeaders.IsCompressed, Is.True, "The file is compressed.");
                        Assert.That(document.DocumentHeaders.UnicodeFileName, Is.EqualTo(String.Empty), "This is a legacy file and it should not have the Unicode file header.");
                        Assert.That(document.DocumentHeaders.AnsiFileName, Is.EqualTo("readme.html"), "The file name should be 'readme.html'.");
                        Assert.That(document.DocumentHeaders.FileName, Is.EqualTo("readme.html"), "The file name should be 'readme.html'.");
                        using (MemoryStream plaintextStream = new MemoryStream())
                        {
                            document.DecryptTo(axCryptReader, plaintextStream);
                            Assert.That(document.DocumentHeaders.PlaintextLength, Is.EqualTo(3736), "The compressed content should be recorded as 3736 bytes in the headers.");
                            Assert.That(plaintextStream.Length, Is.EqualTo(9528), "The file should be 9528 bytes uncompressed plaintext in actual fact.");
                        }
                    }
                }
            }
        }

        [Test]
        public static void TestDecryptWithoutLoadFirstFromEmptyFile()
        {
            using (AxCryptReader axCryptReader = new AxCryptStreamReader(new MemoryStream()))
            {
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    using (MemoryStream plaintextStream = new MemoryStream())
                    {
                        Assert.Throws<InternalErrorException>(() => { document.DecryptTo(axCryptReader, plaintextStream); });
                    }
                }
            }
        }

        [Test]
        public static void TestDecryptAfterFailedLoad()
        {
            using (Stream testStream = new MemoryStream())
            {
                AxCrypt1Guid.Write(testStream);
                testStream.Position = 0;
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    AxCryptReaderSettings settings = new AxCryptReaderSettings("Å ä Ö");
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                    {
                        Assert.Throws<FileFormatException>(() => { document.Load(axCryptReader, settings); });
                        using (MemoryStream plaintextStream = new MemoryStream())
                        {
                            Assert.Throws<InternalErrorException>(() => { document.DecryptTo(axCryptReader, plaintextStream); });
                        }
                    }
                }
            }
        }

        [Test]
        public static void TestDecryptCompressedFromLargerFile()
        {
            using (Stream testStream = new MemoryStream(Resources.David_Copperfield_Key_Å_ä_Ö_txt))
            {
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    AxCryptReaderSettings settings = new AxCryptReaderSettings("Å ä Ö");
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                    {
                        bool keyIsOk = document.Load(axCryptReader, settings);
                        Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                        using (MemoryStream plaintextStream = new MemoryStream())
                        {
                            document.DecryptTo(axCryptReader, plaintextStream);
                            string text = Encoding.UTF8.GetString(plaintextStream.GetBuffer(), 0, (int)plaintextStream.Length);
                            Assert.That(text, Is.StringStarting("The Project Gutenberg EBook of David Copperfield, by Charles Dickens"), "Unexpected start of David Copperfield.");
                            Assert.That(text, Is.StringEnding("subscribe to our email newsletter to hear about new eBooks." + (Char)13 + (Char)10), "Unexpected end of David Copperfield.");
                            Assert.That(text.Length, Is.EqualTo(1992490), "Wrong length of full text of David Copperfield.");
                            Assert.That(document.DocumentHeaders.PlaintextLength, Is.EqualTo(795855), "Wrong expected length of compressed text of David Copperfield.");
                        }
                    }
                }
            }
        }

        [Test]
        public static void TestHmacCalculationFromSimpleFile()
        {
            using (Stream testStream = new MemoryStream(Resources.HelloWorld_Key_a_txt))
            {
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    AxCryptReaderSettings settings = new AxCryptReaderSettings("a");
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                    {
                        bool keyIsOk = document.Load(axCryptReader, settings);
                        Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                        document.DecryptTo(axCryptReader, Stream.Null);
                    }
                }
            }
        }

        [Test]
        public static void TestFailedHmacCalculationFromSimpleFile()
        {
            using (Stream testStream = new MemoryStream(Resources.HelloWorld_Key_a_txt))
            {
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    AxCryptReaderSettings settings = new AxCryptReaderSettings("a");
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                    {
                        bool keyIsOk = document.Load(axCryptReader, settings);
                        Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                        document.DocumentHeaders.SetHmac(new DataHmac(new byte[document.DocumentHeaders.GetHmac().Length]));
                        Assert.Throws<InvalidDataException>(() =>
                        {
                            document.DecryptTo(axCryptReader, Stream.Null);
                        });
                    }
                }
            }
        }

        [Test]
        public static void TestNoMagicGuidFound()
        {
            byte[] dummy = Encoding.ASCII.GetBytes("This is a string that generates some bytes, none of which will match the magic GUID");
            using (Stream testStream = new MemoryStream(dummy))
            {
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    AxCryptReaderSettings settings = new AxCryptReaderSettings("a");
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                    {
                        Assert.Throws<FileFormatException>(() => { document.Load(axCryptReader, settings); }, "Calling with dummy data that does not contain a GUID.");
                    }
                }
            }
        }

        [Test]
        public static void TestInputStreamTooShort()
        {
            using (MemoryStream testStream = new MemoryStream())
            {
                byte[] guid = AxCrypt1Guid.GetBytes();
                testStream.Write(guid, 0, guid.Length);
                testStream.Position = 0;
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                    {
                        Assert.Throws<FileFormatException>(() => { document.Load(axCryptReader, new AxCryptReaderSettings(String.Empty)); }, "Calling with too short a stream, only containing a GUID.");
                    }
                }
            }
        }

        [Test]
        public static void TestFileTimesFromSimpleFile()
        {
            using (Stream testStream = new MemoryStream(Resources.HelloWorld_Key_a_txt))
            {
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    AxCryptReaderSettings settings = new AxCryptReaderSettings("a");
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                    {
                        bool keyIsOk = document.Load(axCryptReader, settings);
                        Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");

                        string creationTime = document.DocumentHeaders.CreationTimeUtc.ToString(CultureInfo.InvariantCulture);
                        Assert.That(creationTime, Is.EqualTo("01/13/2012 17:17:18"), "Checking creation time.");
                        string lastAccessTime = document.DocumentHeaders.LastAccessTimeUtc.ToString(CultureInfo.InvariantCulture);
                        Assert.That(lastAccessTime, Is.EqualTo("01/13/2012 17:17:18"), "Checking last access time.");
                        string lastWriteTime = document.DocumentHeaders.LastWriteTimeUtc.ToString(CultureInfo.InvariantCulture);
                        Assert.That(lastWriteTime, Is.EqualTo("01/13/2012 17:17:45"), "Checking last modify time.");
                    }
                }
            }
        }

        [Test]
        public static void TestChangePassphraseForSimpleFile()
        {
            using (Stream testStream = new MemoryStream(Resources.HelloWorld_Key_a_txt))
            {
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    AxCryptReaderSettings settings = new AxCryptReaderSettings("a");
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                    {
                        bool keyIsOk = document.Load(axCryptReader, settings);
                        Assert.That(keyIsOk, Is.True, "The passphrase provided is correct and should work!");

                        AxCryptReaderSettings newSettings = new AxCryptReaderSettings("b");
                        using (Stream changedStream = new MemoryStream())
                        {
                            using (DocumentHeaders outputDocumentHeaders = new DocumentHeaders(document.DocumentHeaders))
                            {
                                outputDocumentHeaders.RewrapMasterKey(newSettings.DerivedPassphrase);

                                document.CopyEncryptedTo(axCryptReader, outputDocumentHeaders, changedStream);
                                changedStream.Position = 0;
                                using (AxCryptDocument changedDocument = new AxCryptDocument())
                                {
                                    using (AxCryptReader changedAxCryptReader = AxCryptReader.Create(changedStream))
                                    {
                                        bool changedKeyIsOk = changedDocument.Load(changedAxCryptReader, newSettings);
                                        Assert.That(changedKeyIsOk, Is.True, "The changed passphrase provided is correct and should work!");

                                        using (MemoryStream plaintextStream = new MemoryStream())
                                        {
                                            changedDocument.DecryptTo(changedAxCryptReader, plaintextStream);
                                            Assert.That(Encoding.ASCII.GetString(plaintextStream.GetBuffer(), 0, (int)plaintextStream.Length), Is.EqualTo("HelloWorld"), "Unexpected result of decryption.");
                                            Assert.That(changedDocument.DocumentHeaders.PlaintextLength, Is.EqualTo(10), "'HelloWorld' should be 10 bytes uncompressed plaintext.");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [Test]
        public static void TestSimpleEncryptTo()
        {
            using (Stream inputStream = new MemoryStream(Encoding.UTF8.GetBytes("AxCrypt is Great!")))
            {
                using (Stream outputStream = new MemoryStream())
                {
                    using (AxCryptDocument document = new AxCryptDocument())
                    {
                        AxCryptReaderSettings settings = new AxCryptReaderSettings("a");
                        using (DocumentHeaders headers = new DocumentHeaders(settings.DerivedPassphrase))
                        {
                            headers.UnicodeFileName = "MyFile.txt";
                            DateTime fileTime = new DateTime(2012, 1, 1, 1, 2, 3);
                            headers.CreationTimeUtc = fileTime;
                            headers.LastAccessTimeUtc = fileTime + new TimeSpan(1, 0, 0);
                            headers.LastWriteTimeUtc = fileTime + new TimeSpan(2, 0, 0); ;
                            document.DocumentHeaders = headers;
                            document.EncryptTo(headers, inputStream, outputStream);
                        }
                    }
                }
            }
        }
    }
}