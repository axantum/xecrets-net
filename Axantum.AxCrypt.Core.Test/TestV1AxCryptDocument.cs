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
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestV1AxCryptDocument
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
        public static void TestAnsiFileNameFromSimpleFile()
        {
            using (Stream testStream = FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.helloworld_key_a_txt))
            {
                Passphrase passphrase = new Passphrase("a");
                using (V1AxCryptDocument document = new V1AxCryptDocument())
                {
                    bool keyIsOk = document.Load(passphrase, CryptoFactory.Aes128V1Id, testStream);
                    Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                    string fileName = document.DocumentHeaders.FileName;
                    Assert.That(fileName, Is.EqualTo("HelloWorld-Key-a.txt"), "Wrong file name");
                }
            }
        }

        [Test]
        public static void TestUnicodeFileNameFromSimpleFile()
        {
            using (Stream testStream = FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.helloworld_key_a_txt))
            {
                Passphrase passphrase = new Passphrase("a");
                using (V1AxCryptDocument document = new V1AxCryptDocument())
                {
                    bool keyIsOk = document.Load(passphrase, CryptoFactory.Aes128V1Id, testStream);
                    Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                    string fileName = document.DocumentHeaders.FileName;
                    Assert.That(fileName, Is.EqualTo("HelloWorld-Key-a.txt"), "Wrong file name");
                }
            }
        }

        [Test]
        public static void TestFileNameFromSimpleFileWithUnicode()
        {
            using (Stream testStream = FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.helloworld_key_a_txt))
            {
                Passphrase passphrase = new Passphrase("a");
                using (V1AxCryptDocument document = new V1AxCryptDocument())
                {
                    bool keyIsOk = document.Load(passphrase, CryptoFactory.Aes128V1Id, testStream);
                    Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                    string fileName = document.DocumentHeaders.FileName;
                    Assert.That(fileName, Is.EqualTo("HelloWorld-Key-a.txt"), "Wrong file name");
                }
            }
        }

        [Test]
        public static void TestHmacFromSimpleFile()
        {
            V1Hmac expectedHmac = new V1Hmac(new byte[] { 0xF9, 0xAF, 0x2E, 0x67, 0x7D, 0xCF, 0xC9, 0xFE, 0x06, 0x4B, 0x39, 0x08, 0xE7, 0x5A, 0x87, 0x81 });
            Passphrase passphrase = new Passphrase("a");
            using (V1AxCryptDocument document = new V1AxCryptDocument())
            {
                bool keyIsOk = document.Load(passphrase, CryptoFactory.Aes128V1Id, FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.helloworld_key_a_txt));
                Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                Hmac hmac = document.DocumentHeaders.Headers.Hmac;
                Assert.That(hmac.GetBytes(), Is.EqualTo(expectedHmac.GetBytes()), "Wrong HMAC");
            }
        }

        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "This is a test, and they should start with 'Test'.")]
        public static void TestIsCompressedFromSimpleFile()
        {
            Passphrase passphrase = new Passphrase("a");
            using (V1AxCryptDocument document = new V1AxCryptDocument())
            {
                bool keyIsOk = document.Load(passphrase, CryptoFactory.Aes128V1Id, FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.helloworld_key_a_txt));
                Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                bool isCompressed = document.DocumentHeaders.IsCompressed;
                Assert.That(isCompressed, Is.False, "This file should not be compressed.");
            }
        }

        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "This is a test, and they should start with 'Test'.")]
        public static void TestInvalidPassphraseWithSimpleFile()
        {
            Passphrase passphrase = new Passphrase("b");
            using (V1AxCryptDocument document = new V1AxCryptDocument())
            {
                bool keyIsOk = document.Load(passphrase, CryptoFactory.Aes128V1Id, FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.helloworld_key_a_txt));
                Assert.That(keyIsOk, Is.False, "The passphrase provided is wrong!");
            }
        }

        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "This is a test, and they should start with 'Test'.")]
        public static void TestIsCompressedFromLargerFile()
        {
            Passphrase passphrase = new Passphrase("Å ä Ö");
            using (V1AxCryptDocument document = new V1AxCryptDocument())
            {
                bool keyIsOk = document.Load(passphrase, CryptoFactory.Aes128V1Id, FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.david_copperfield_key__aa_ae_oe__ulu_txt));
                Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                bool isCompressed = document.DocumentHeaders.IsCompressed;
                Assert.That(isCompressed, Is.True, "This file should be compressed.");
            }
        }

        [Test]
        public static void TestDecryptUncompressedFromSimpleFile()
        {
            Passphrase passphrase = new Passphrase("a");
            using (V1AxCryptDocument document = new V1AxCryptDocument())
            {
                bool keyIsOk = document.Load(passphrase, CryptoFactory.Aes128V1Id, FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.helloworld_key_a_txt));
                Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                using (MemoryStream plaintextStream = new MemoryStream())
                {
                    document.DecryptTo(plaintextStream);
                    Assert.That(Encoding.ASCII.GetString(plaintextStream.GetBuffer(), 0, (int)plaintextStream.Length), Is.EqualTo("HelloWorld"), "Unexpected result of decryption.");
                    Assert.That(document.DocumentHeaders.PlaintextLength, Is.EqualTo(10), "'HelloWorld' should be 10 bytes uncompressed plaintext.");
                }
            }
        }

        [Test]
        public static void TestDecryptUncompressedWithCancel()
        {
            Passphrase passphrase = new Passphrase("a");
            using (V1AxCryptDocument document = new V1AxCryptDocument())
            {
                IProgressContext progress = new CancelProgressContext(new ProgressContext(new TimeSpan(0, 0, 0, 0, 100)));
                progress.Progressing += (object sender, ProgressEventArgs e) =>
                {
                    progress.Cancel = true;
                };
                bool keyIsOk = document.Load(passphrase, CryptoFactory.Aes128V1Id, new ProgressStream(FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.helloworld_key_a_txt), progress));
                Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                using (MemoryStream plaintextStream = new MemoryStream())
                {
                    FakeRuntimeEnvironment environment = (FakeRuntimeEnvironment)OS.Current;
                    environment.CurrentTiming.CurrentTiming = new TimeSpan(0, 0, 0, 0, 100);
                    Assert.Throws<OperationCanceledException>(() => { document.DecryptTo(plaintextStream); });
                }
            }
        }

        [Test]
        public static void TestDecryptUncompressedWithPaddingError()
        {
            Passphrase passphrase = new Passphrase("a");
            using (V1AxCryptDocument document = new V1AxCryptDocument())
            {
                using (MemoryStream encryptedFile = FakeRuntimeFileInfo.ExpandableMemoryStream((byte[])Resources.helloworld_key_a_txt.Clone()))
                {
                    encryptedFile.Seek(-1, SeekOrigin.End);
                    byte lastByte = (byte)encryptedFile.ReadByte();
                    ++lastByte;
                    encryptedFile.Seek(-1, SeekOrigin.End);
                    encryptedFile.WriteByte(lastByte);
                    encryptedFile.Position = 0;
                    bool keyIsOk = document.Load(passphrase, CryptoFactory.Aes128V1Id, encryptedFile);
                    Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                    using (MemoryStream plaintextStream = new MemoryStream())
                    {
                        Assert.Throws<CryptographicException>(() => { document.DecryptTo(plaintextStream); });
                    }
                }
            }
        }

        [Test]
        public static void TestDecryptCompressedWithTruncatedFile()
        {
            Passphrase passphrase = new Passphrase("Å ä Ö");
            using (V1AxCryptDocument document = new V1AxCryptDocument())
            {
                using (MemoryStream encryptedFile = FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.david_copperfield_key__aa_ae_oe__ulu_txt))
                {
                    encryptedFile.SetLength(encryptedFile.Length / 2);
                    encryptedFile.Position = 0;
                    bool keyIsOk = document.Load(passphrase, CryptoFactory.Aes128V1Id, encryptedFile);
                    Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                    using (MemoryStream plaintextStream = new MemoryStream())
                    {
                        Assert.That(() => { document.DecryptTo(plaintextStream); }, Throws.InstanceOf<Exception>());
                    }
                }
            }
        }

        [Test]
        public static void TestDecryptCompressedWithCancel()
        {
            Passphrase passphrase = new Passphrase("Å ä Ö");
            using (V1AxCryptDocument document = new V1AxCryptDocument())
            {
                IProgressContext progress = new CancelProgressContext(new ProgressContext(new TimeSpan(0, 0, 0, 0, 100)));
                bool keyIsOk = document.Load(passphrase, CryptoFactory.Aes128V1Id, new ProgressStream(FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.david_copperfield_key__aa_ae_oe__ulu_txt), progress));
                Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                using (MemoryStream plaintextStream = new MemoryStream())
                {
                    progress.Progressing += (object sender, ProgressEventArgs e) =>
                    {
                        progress.Cancel = true;
                    };
                    FakeRuntimeEnvironment environment = (FakeRuntimeEnvironment)OS.Current;
                    environment.CurrentTiming.CurrentTiming = new TimeSpan(0, 0, 0, 0, 100);
                    Assert.Throws<OperationCanceledException>(() => { document.DecryptTo(plaintextStream); });
                }
            }
        }

        [Test]
        public static void TestDecryptCompressedFromLegacy0B6()
        {
            Passphrase passphrase = new Passphrase("åäö");
            using (V1AxCryptDocument document = new V1AxCryptDocument())
            {
                bool keyIsOk = document.Load(passphrase, CryptoFactory.Aes128V1Id, FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.tst_0_0b6_key__aaaeoe__medium_html));
                Assert.That(keyIsOk, Is.True, "A correct passphrase was provided, but it was not accepted.");
                Assert.That(document.DocumentHeaders.IsCompressed, Is.True, "The file is compressed.");
                Assert.That(document.DocumentHeaders.FileName, Is.EqualTo("readme.html"), "The file name should be 'readme.html'.");
                using (MemoryStream plaintextStream = new MemoryStream())
                {
                    document.DecryptTo(plaintextStream);
                    Assert.That(document.DocumentHeaders.PlaintextLength, Is.EqualTo(3736), "The compressed content should be recorded as 3736 bytes in the headers.");
                    Assert.That(plaintextStream.Length, Is.EqualTo(9528), "The file should be 9528 bytes uncompressed plaintext in actual fact.");
                }
            }
        }

        [Test]
        public static void TestDecryptWithoutLoadFirstFromEmptyFile()
        {
            using (V1AxCryptDocument document = new V1AxCryptDocument())
            {
                using (MemoryStream plaintextStream = new MemoryStream())
                {
                    Assert.Throws<InternalErrorException>(() => { document.DecryptTo(plaintextStream); });
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
                Passphrase passphrase = new Passphrase("Å ä Ö");
                using (V1AxCryptDocument document = new V1AxCryptDocument())
                {
                    Assert.Throws<FileFormatException>(() => { document.Load(passphrase, CryptoFactory.Aes128V1Id, testStream); });
                    using (MemoryStream plaintextStream = new MemoryStream())
                    {
                        Assert.Throws<InternalErrorException>(() => { document.DecryptTo(plaintextStream); });
                    }
                }
            }
        }

        [Test]
        public static void TestDecryptCompressedFromLargerFile()
        {
            Passphrase passphrase = new Passphrase("Å ä Ö");
            using (V1AxCryptDocument document = new V1AxCryptDocument())
            {
                bool keyIsOk = document.Load(passphrase, CryptoFactory.Aes128V1Id, FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.david_copperfield_key__aa_ae_oe__ulu_txt));
                Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                using (MemoryStream plaintextStream = new MemoryStream())
                {
                    document.DecryptTo(plaintextStream);
                    string text = Encoding.UTF8.GetString(plaintextStream.GetBuffer(), 0, (int)plaintextStream.Length);
                    Assert.That(text, Is.StringStarting("The Project Gutenberg EBook of David Copperfield, by Charles Dickens"), "Unexpected start of David Copperfield.");
                    Assert.That(text, Is.StringEnding("subscribe to our email newsletter to hear about new eBooks." + (Char)13 + (Char)10), "Unexpected end of David Copperfield.");
                    Assert.That(text.Length, Is.EqualTo(1992490), "Wrong length of full text of David Copperfield.");
                    Assert.That(document.DocumentHeaders.PlaintextLength, Is.EqualTo(795855), "Wrong expected length of compressed text of David Copperfield.");
                }
            }
        }

        [Test]
        public static void TestHmacCalculationFromSimpleFile()
        {
            Passphrase passphrase = new Passphrase("a");
            using (V1AxCryptDocument document = new V1AxCryptDocument())
            {
                bool keyIsOk = document.Load(passphrase, CryptoFactory.Aes128V1Id, FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.helloworld_key_a_txt));
                Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                document.DecryptTo(Stream.Null);
            }
        }

        [Test]
        public static void TestFailedHmacCalculationFromSimpleFile()
        {
            Passphrase passphrase = new Passphrase("a");
            using (V1AxCryptDocument document = new V1AxCryptDocument())
            {
                bool keyIsOk = document.Load(passphrase, CryptoFactory.Aes128V1Id, FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.helloworld_key_a_txt));
                Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                document.DocumentHeaders.Headers.Hmac = new V1Hmac(new byte[V1Hmac.RequiredLength]);
                Assert.Throws<Axantum.AxCrypt.Core.Runtime.IncorrectDataException>(() =>
                {
                    document.DecryptTo(Stream.Null);
                });
            }
        }

        [Test]
        public static void TestNoMagicGuidFound()
        {
            byte[] dummy = Encoding.ASCII.GetBytes("This is a string that generates some bytes, none of which will match the magic GUID");
            Passphrase passphrase = new Passphrase("a");
            using (V1AxCryptDocument document = new V1AxCryptDocument())
            {
                Assert.Throws<FileFormatException>(() => { document.Load(passphrase, CryptoFactory.Aes128V1Id, FakeRuntimeFileInfo.ExpandableMemoryStream(dummy)); }, "Calling with dummy data that does not contain a GUID.");
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
                using (V1AxCryptDocument document = new V1AxCryptDocument())
                {
                    Assert.Throws<FileFormatException>(() => { document.Load(Passphrase.Empty, CryptoFactory.Aes128V1Id, testStream); }, "Calling with too short a stream, only containing a GUID.");
                }
            }
        }

        [Test]
        public static void TestFileTimesFromSimpleFile()
        {
            Passphrase passphrase = new Passphrase("a");
            using (V1AxCryptDocument document = new V1AxCryptDocument())
            {
                bool keyIsOk = document.Load(passphrase, CryptoFactory.Aes128V1Id, FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.helloworld_key_a_txt));
                Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");

                string creationTime = document.DocumentHeaders.CreationTimeUtc.ToString(CultureInfo.InvariantCulture);
                Assert.That(creationTime, Is.EqualTo("01/13/2012 17:17:18"), "Checking creation time.");
                string lastAccessTime = document.DocumentHeaders.LastAccessTimeUtc.ToString(CultureInfo.InvariantCulture);
                Assert.That(lastAccessTime, Is.EqualTo("01/13/2012 17:17:18"), "Checking last access time.");
                string lastWriteTime = document.DocumentHeaders.LastWriteTimeUtc.ToString(CultureInfo.InvariantCulture);
                Assert.That(lastWriteTime, Is.EqualTo("01/13/2012 17:17:45"), "Checking last modify time.");
            }
        }

        [Test]
        public static void TestChangePassphraseForSimpleFile()
        {
            Passphrase passphrase = new Passphrase("a");
            using (V1AxCryptDocument document = new V1AxCryptDocument())
            {
                bool keyIsOk = document.Load(passphrase, CryptoFactory.Aes128V1Id, FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.helloworld_key_a_txt));
                Assert.That(keyIsOk, Is.True, "The passphrase provided is correct and should work!");

                Passphrase newPassphrase = new Passphrase("b");
                using (Stream changedStream = new MemoryStream())
                {
                    V1DocumentHeaders outputDocumentHeaders = new V1DocumentHeaders(document.DocumentHeaders);
                    outputDocumentHeaders.RewrapMasterKey(new V1AesCrypto(new V1Aes128CryptoFactory(), new V1Passphrase(newPassphrase), SymmetricIV.Zero128));

                    document.CopyEncryptedTo(outputDocumentHeaders, changedStream);
                    changedStream.Position = 0;
                    using (V1AxCryptDocument changedDocument = new V1AxCryptDocument())
                    {
                        bool changedKeyIsOk = changedDocument.Load(newPassphrase, CryptoFactory.Aes128V1Id, changedStream);
                        Assert.That(changedKeyIsOk, Is.True, "The changed passphrase provided is correct and should work!");

                        using (MemoryStream plaintextStream = new MemoryStream())
                        {
                            changedDocument.DecryptTo(plaintextStream);
                            Assert.That(Encoding.ASCII.GetString(plaintextStream.GetBuffer(), 0, (int)plaintextStream.Length), Is.EqualTo("HelloWorld"), "Unexpected result of decryption.");
                            Assert.That(changedDocument.DocumentHeaders.PlaintextLength, Is.EqualTo(10), "'HelloWorld' should be 10 bytes uncompressed plaintext.");
                        }
                    }
                }
            }
        }

        [Test]
        public static void TestSimpleEncryptToWithCompression()
        {
            DateTime creationTimeUtc = new DateTime(2012, 1, 1, 1, 2, 3, DateTimeKind.Utc);
            DateTime lastAccessTimeUtc = creationTimeUtc + new TimeSpan(1, 0, 0);
            DateTime lastWriteTimeUtc = creationTimeUtc + new TimeSpan(2, 0, 0); ;
            using (Stream inputStream = FakeRuntimeFileInfo.ExpandableMemoryStream(Encoding.UTF8.GetBytes("AxCrypt is Great!")))
            {
                using (Stream outputStream = new MemoryStream())
                {
                    Passphrase passphrase = new Passphrase("a");
                    using (V1AxCryptDocument document = new V1AxCryptDocument(new V1AesCrypto(new V1Aes128CryptoFactory(), new V1Passphrase(passphrase), SymmetricIV.Zero128), 47))
                    {
                        document.DocumentHeaders.FileName = "MyFile.txt";
                        document.DocumentHeaders.CreationTimeUtc = creationTimeUtc;
                        document.DocumentHeaders.LastAccessTimeUtc = lastAccessTimeUtc;
                        document.DocumentHeaders.LastWriteTimeUtc = lastWriteTimeUtc;
                        document.EncryptTo(inputStream, outputStream, AxCryptOptions.EncryptWithCompression);
                    }
                    outputStream.Position = 0;
                    using (V1AxCryptDocument document = new V1AxCryptDocument())
                    {
                        bool keyIsOk = document.Load(passphrase, CryptoFactory.Aes128V1Id, outputStream);
                        Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                        Assert.That(document.DocumentHeaders.FileName, Is.EqualTo("MyFile.txt"));
                        Assert.That(document.DocumentHeaders.CreationTimeUtc, Is.EqualTo(creationTimeUtc));
                        Assert.That(document.DocumentHeaders.LastAccessTimeUtc, Is.EqualTo(lastAccessTimeUtc));
                        Assert.That(document.DocumentHeaders.LastWriteTimeUtc, Is.EqualTo(lastWriteTimeUtc));
                        using (MemoryStream plaintextStream = new MemoryStream())
                        {
                            document.DecryptTo(plaintextStream);
                            Assert.That(document.DocumentHeaders.UncompressedLength, Is.EqualTo(17), "'AxCrypt is Great!' should be 17 bytes uncompressed plaintext.");
                            Assert.That(Encoding.ASCII.GetString(plaintextStream.GetBuffer(), 0, (int)plaintextStream.Length), Is.EqualTo("AxCrypt is Great!"), "Unexpected result of decryption.");
                        }
                    }
                }
            }
        }

        [Test]
        public static void TestSimpleEncryptToWithoutCompression()
        {
            DateTime creationTimeUtc = new DateTime(2012, 1, 1, 1, 2, 3, DateTimeKind.Utc);
            DateTime lastAccessTimeUtc = creationTimeUtc + new TimeSpan(1, 0, 0);
            DateTime lastWriteTimeUtc = creationTimeUtc + new TimeSpan(2, 0, 0); ;
            using (Stream inputStream = FakeRuntimeFileInfo.ExpandableMemoryStream(Encoding.UTF8.GetBytes("AxCrypt is Great!")))
            {
                using (Stream outputStream = new MemoryStream())
                {
                    Passphrase passphrase = new Passphrase("a");
                    using (V1AxCryptDocument document = new V1AxCryptDocument(new V1AesCrypto(new V1Aes128CryptoFactory(), new V1Passphrase(passphrase), SymmetricIV.Zero128), 53))
                    {
                        document.DocumentHeaders.FileName = "MyFile.txt";
                        document.DocumentHeaders.CreationTimeUtc = creationTimeUtc;
                        document.DocumentHeaders.LastAccessTimeUtc = lastAccessTimeUtc;
                        document.DocumentHeaders.LastWriteTimeUtc = lastWriteTimeUtc;
                        document.EncryptTo(inputStream, outputStream, AxCryptOptions.EncryptWithoutCompression);
                    }
                    outputStream.Position = 0;
                    using (V1AxCryptDocument document = new V1AxCryptDocument())
                    {
                        bool keyIsOk = document.Load(passphrase, CryptoFactory.Aes128V1Id, outputStream);
                        Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                        Assert.That(document.DocumentHeaders.FileName, Is.EqualTo("MyFile.txt"));
                        Assert.That(document.DocumentHeaders.CreationTimeUtc, Is.EqualTo(creationTimeUtc));
                        Assert.That(document.DocumentHeaders.LastAccessTimeUtc, Is.EqualTo(lastAccessTimeUtc));
                        Assert.That(document.DocumentHeaders.LastWriteTimeUtc, Is.EqualTo(lastWriteTimeUtc));
                        using (MemoryStream plaintextStream = new MemoryStream())
                        {
                            document.DecryptTo(plaintextStream);
                            Assert.That(document.DocumentHeaders.UncompressedLength, Is.EqualTo(-1), "'AxCrypt is Great!' should not return a value at all for uncompressed, since it was not compressed.");
                            Assert.That(document.DocumentHeaders.PlaintextLength, Is.EqualTo(17), "'AxCrypt is Great!' is 17 bytes plaintext length.");
                            Assert.That(Encoding.ASCII.GetString(plaintextStream.GetBuffer(), 0, (int)plaintextStream.Length), Is.EqualTo("AxCrypt is Great!"), "Unexpected result of decryption.");
                        }
                    }
                }
            }
        }

        private class NonSeekableStream : Stream
        {
            public override bool CanRead
            {
                get { return false; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            public override void Flush()
            {
            }

            public override long Length
            {
                get { return 0; }
            }

            public override long Position
            {
                get
                {
                    return 0;
                }
                set
                {
                }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return 0;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
            }
        }

        [Test]
        public static void TestInvalidArguments()
        {
            using (Stream inputStream = FakeRuntimeFileInfo.ExpandableMemoryStream(Encoding.UTF8.GetBytes("AxCrypt is Great!")))
            {
                using (Stream outputStream = new MemoryStream())
                {
                    using (V1AxCryptDocument document = new V1AxCryptDocument())
                    {
                        Assert.Throws<ArgumentNullException>(() => { document.EncryptTo(inputStream, null, AxCryptOptions.EncryptWithCompression); });
                        Assert.Throws<ArgumentNullException>(() => { document.EncryptTo(null, outputStream, AxCryptOptions.EncryptWithCompression); });
                        Assert.Throws<ArgumentException>(() => { document.EncryptTo(inputStream, new NonSeekableStream(), AxCryptOptions.EncryptWithCompression); });
                        Assert.Throws<ArgumentException>(() => { document.EncryptTo(inputStream, outputStream, AxCryptOptions.EncryptWithCompression | AxCryptOptions.EncryptWithoutCompression); });
                        Assert.Throws<ArgumentException>(() => { document.EncryptTo(inputStream, outputStream, AxCryptOptions.None); });

                        Passphrase passphrase = new Passphrase("a");
                        V1DocumentHeaders headers = new V1DocumentHeaders(new V1AesCrypto(new V1Aes128CryptoFactory(), new V1Passphrase(passphrase), SymmetricIV.Zero128), 13);

                        Assert.Throws<ArgumentNullException>(() => { document.CopyEncryptedTo(null, outputStream); });
                        Assert.Throws<ArgumentNullException>(() => { document.CopyEncryptedTo(headers, null); });
                        Assert.Throws<ArgumentException>(() => { document.CopyEncryptedTo(headers, new NonSeekableStream()); });
                        Assert.Throws<InternalErrorException>(() => { document.CopyEncryptedTo(headers, outputStream); });
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times"), Test]
        public static void TestDoubleDispose()
        {
            V1AxCryptDocument document = new V1AxCryptDocument();
            document.Dispose();
            document.Dispose();
        }

        [Test]
        public static void TestInvalidHmacInCopyEncryptedTo()
        {
            Passphrase passphrase = new Passphrase("a");
            using (V1AxCryptDocument document = new V1AxCryptDocument())
            {
                bool keyIsOk = document.Load(passphrase, CryptoFactory.Aes128V1Id, FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.helloworld_key_a_txt));
                Assert.That(keyIsOk, Is.True, "The passphrase provided is correct and should work!");

                Passphrase newPassphrase = new Passphrase("b");
                using (Stream changedStream = new MemoryStream())
                {
                    V1DocumentHeaders outputDocumentHeaders = new V1DocumentHeaders(document.DocumentHeaders);
                    outputDocumentHeaders.RewrapMasterKey(new V1AesCrypto(new V1Aes128CryptoFactory(), new V1Passphrase(newPassphrase), SymmetricIV.Zero128));

                    byte[] modifiedHmacBytes = document.DocumentHeaders.Headers.Hmac.GetBytes();
                    modifiedHmacBytes[0] += 1;
                    document.DocumentHeaders.Headers.Hmac = new V1Hmac(modifiedHmacBytes);
                    Assert.Throws<Axantum.AxCrypt.Core.Runtime.IncorrectDataException>(() =>
                    {
                        document.CopyEncryptedTo(outputDocumentHeaders, changedStream);
                    });
                }
            }
        }

        [Test]
        public static void TestReaderNotPositionedAtData()
        {
            using (MemoryStream encryptedFile = new MemoryStream(Resources.david_copperfield_key__aa_ae_oe__ulu_txt))
            {
                Headers headers = new Headers();
                AxCryptReader reader = headers.Load(encryptedFile);
                using (V1AxCryptDocument document = new V1AxCryptDocument())
                {
                    Passphrase key = new Passphrase("Å ä Ö");
                    bool keyIsOk = document.Load(key, reader, headers);
                    Assert.That(keyIsOk, Is.True);

                    reader.SetStartOfData();
                    Assert.Throws<InvalidOperationException>(() => document.DecryptTo(Stream.Null));
                }
            }
        }

        [Test]
        public static void TestHmacThrowsWhenTooLittleData()
        {
            using (MemoryStream plaintext = new MemoryStream(Resources.uncompressable_zip))
            {
                using (MemoryStream encryptedFile = new MemoryStream())
                {
                    using (V1AxCryptDocument encryptingDocument = new V1AxCryptDocument(new V1AesCrypto(new V1Aes128CryptoFactory(), new V1Passphrase(new Passphrase("a")), SymmetricIV.Zero128), 10))
                    {
                        encryptingDocument.EncryptTo(plaintext, encryptedFile, AxCryptOptions.EncryptWithoutCompression);
                    }

                    encryptedFile.Position = 0;
                    Headers headers = new Headers();
                    AxCryptReader reader = headers.Load(encryptedFile);
                    using (V1AxCryptDocument document = new V1AxCryptDocument())
                    {
                        Passphrase key = new Passphrase("a");
                        bool keyIsOk = document.Load(key, reader, headers);
                        Assert.That(keyIsOk, Is.True);

                        reader.InputStream.Read(new byte[16], 0, 16);
                        Assert.Throws<InvalidOperationException>(() => document.DecryptTo(Stream.Null));
                    }
                }
            }
        }
    }
}