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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Test.Properties;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestAxCryptDocument
    {
        [Test]
        public static void TestFileNameFromSimpleFile()
        {
            using (Stream testStream = new MemoryStream(Resources.HelloWorld_Key_a_txt))
            {
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    AxCryptReaderSettings settings = new AxCryptReaderSettings("a");
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream, settings))
                    {
                        bool keyIsOk = document.Load(axCryptReader);
                        Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                        string fileName = document.FileName;
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
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream, settings))
                    {
                        bool keyIsOk = document.Load(axCryptReader);
                        Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                        string fileName = document.UnicodeFileName;
                        Assert.That(fileName, Is.EqualTo("HelloWorld-Key-a.txt"), "Wrong file name");
                    }
                }
            }
        }

        [Test]
        public static void TestHmacFromSimpleFile()
        {
            byte[] expectedHmac = new byte[] { 0xF9, 0xAF, 0x2E, 0x67, 0x7D, 0xCF, 0xC9, 0xFE, 0x06, 0x4B, 0x39, 0x08, 0xE7, 0x5A, 0x87, 0x81 };
            using (Stream testStream = new MemoryStream(Resources.HelloWorld_Key_a_txt))
            {
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    AxCryptReaderSettings settings = new AxCryptReaderSettings("a");
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream, settings))
                    {
                        bool keyIsOk = document.Load(axCryptReader);
                        Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                        byte[] hmac = document.GetHmac();
                        Assert.That(hmac, Is.EqualTo(expectedHmac), "Wrong HMAC");
                    }
                }
            }
        }

        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompundWordsShouldBeCasedCorrectly", Justification = "This is a test, and they should start with 'Test'.")]
        public static void TestIsCompressedFromSimpleFile()
        {
            using (Stream testStream = new MemoryStream(Resources.HelloWorld_Key_a_txt))
            {
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    AxCryptReaderSettings settings = new AxCryptReaderSettings("a");
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream, settings))
                    {
                        bool keyIsOk = document.Load(axCryptReader);
                        Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                        bool isCompressed = document.IsCompressed;
                        Assert.That(isCompressed, Is.False, "This file should not be compressed.");
                    }
                }
            }
        }

        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompundWordsShouldBeCasedCorrectly", Justification = "This is a test, and they should start with 'Test'.")]
        public static void TestInvalidPassphraseWithSimpleFile()
        {
            using (Stream testStream = new MemoryStream(Resources.HelloWorld_Key_a_txt))
            {
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    AxCryptReaderSettings settings = new AxCryptReaderSettings("b");
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream, settings))
                    {
                        bool keyIsOk = document.Load(axCryptReader);
                        Assert.That(keyIsOk, Is.False, "The passphrase provided is wrong!");
                    }
                }
            }
        }

        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompundWordsShouldBeCasedCorrectly", Justification = "This is a test, and they should start with 'Test'.")]
        public static void TestIsCompressedFromLargerFile()
        {
            using (Stream testStream = new MemoryStream(Resources.David_Copperfield_Key_Å_ä_Ö_txt))
            {
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    AxCryptReaderSettings settings = new AxCryptReaderSettings("Å ä Ö");
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream, settings))
                    {
                        bool keyIsOk = document.Load(axCryptReader);
                        Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                        bool isCompressed = document.IsCompressed;
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
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream, settings))
                    {
                        bool keyIsOk = document.Load(axCryptReader);
                        Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                        using (MemoryStream plaintextStream = new MemoryStream())
                        {
                            document.DecryptTo(plaintextStream);
                            Assert.That(Encoding.ASCII.GetString(plaintextStream.GetBuffer(), 0, (int)plaintextStream.Length), Is.EqualTo("HelloWorld"), "Unexpected result of decryption.");
                            Assert.That(document.PlaintextLength, Is.EqualTo(10), "'HelloWorld' should be 10 bytes uncompressed plaintext.");
                        }
                    }
                }
            }
        }

        [Test]
        public static void TestDecryptWithoutLoadFirstFromEmptyFile()
        {
            using (AxCryptDocument document = new AxCryptDocument())
            {
                using (MemoryStream plaintextStream = new MemoryStream())
                {
                    Assert.Throws<InvalidOperationException>(() => { document.DecryptTo(plaintextStream); });
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
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream, settings))
                    {
                        Assert.Throws<FileFormatException>(() => { document.Load(axCryptReader); });
                        using (MemoryStream plaintextStream = new MemoryStream())
                        {
                            Assert.Throws<InvalidOperationException>(() => { document.DecryptTo(plaintextStream); });
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
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream, settings))
                    {
                        bool keyIsOk = document.Load(axCryptReader);
                        Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                        using (MemoryStream plaintextStream = new MemoryStream())
                        {
                            document.DecryptTo(plaintextStream);
                            string text = Encoding.UTF8.GetString(plaintextStream.GetBuffer(), 0, (int)plaintextStream.Length);
                            Assert.That(text, Is.StringStarting("The Project Gutenberg EBook of David Copperfield, by Charles Dickens"), "Unexpected start of David Copperfield.");
                            Assert.That(text, Is.StringEnding("subscribe to our email newsletter to hear about new eBooks." + (Char)13 + (Char)10), "Unexpected end of David Copperfield.");
                            Assert.That(text.Length, Is.EqualTo(1992490), "Wrong length of full text of David Copperfield.");
                            Assert.That(document.PlaintextLength, Is.EqualTo(795855), "Wrong expected length of compressed text of David Copperfield.");
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
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream, settings))
                    {
                        bool keyIsOk = document.Load(axCryptReader);
                        Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                        document.DecryptTo(Stream.Null);
                        byte[] calculatedHmac = document.GetCalculatedHmac();
                        byte[] hmac = document.GetHmac();

                        Assert.That(calculatedHmac, Is.EqualTo(hmac), "Calculated HMAC differs from HMAC in headers.");
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
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream, settings))
                    {
                        Assert.Throws<FileFormatException>(() => { document.Load(axCryptReader); }, "Calling with dummy data that does not contain a GUID.");
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
                        Assert.Throws<FileFormatException>(() => { document.Load(axCryptReader); }, "Calling with too short a stream, only containing a GUID.");
                    }
                }
            }
        }
    }
}