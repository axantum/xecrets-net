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
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Reader;
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
    public class TestV2AxCryptDocument
    {
        private CryptoImplementation _cryptoImplementation;

        public TestV2AxCryptDocument(CryptoImplementation cryptoImplementation)
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
        public void TestEncryptWithHmacSmall()
        {
            TestEncryptWithHmacHelper(23, AxCryptOptions.EncryptWithoutCompression);
        }

        [Test]
        public void TestEncryptWithHmacAlmostChunkSize()
        {
            TestEncryptWithHmacHelper(V2AxCryptDataStream.WriteChunkSize - 1, AxCryptOptions.EncryptWithoutCompression);
        }

        [Test]
        public void TestEncryptWithHmacChunkSize()
        {
            TestEncryptWithHmacHelper(V2AxCryptDataStream.WriteChunkSize, AxCryptOptions.EncryptWithoutCompression);
        }

        [Test]
        public void TestEncryptWithHmacSeveralChunkSizes()
        {
            TestEncryptWithHmacHelper(V2AxCryptDataStream.WriteChunkSize * 5, AxCryptOptions.EncryptWithoutCompression);
        }

        [Test]
        public void TestEncryptWithHmacIncompleteChunkSizes()
        {
            TestEncryptWithHmacHelper(V2AxCryptDataStream.WriteChunkSize * 3 + V2AxCryptDataStream.WriteChunkSize / 2, AxCryptOptions.EncryptWithoutCompression);
        }

        [Test]
        public void TestEncryptWithHmacSmallWithCompression()
        {
            TestEncryptWithHmacHelper(23, AxCryptOptions.EncryptWithCompression);
        }

        [Test]
        public void TestEncryptWithHmacAlmostChunkSizeWithCompression()
        {
            TestEncryptWithHmacHelper(V2AxCryptDataStream.WriteChunkSize - 1, AxCryptOptions.EncryptWithCompression);
        }

        [Test]
        public void TestEncryptWithHmacChunkSizeWithCompression()
        {
            TestEncryptWithHmacHelper(V2AxCryptDataStream.WriteChunkSize, AxCryptOptions.EncryptWithCompression);
        }

        [Test]
        public void TestEncryptWithHmacSeveralChunkSizesWithCompression()
        {
            TestEncryptWithHmacHelper(V2AxCryptDataStream.WriteChunkSize * 5, AxCryptOptions.EncryptWithCompression);
        }

        [Test]
        public void TestEncryptWithHmacIncompleteChunkSizesWithCompression()
        {
            TestEncryptWithHmacHelper(V2AxCryptDataStream.WriteChunkSize * 3 + V2AxCryptDataStream.WriteChunkSize / 2, AxCryptOptions.EncryptWithCompression);
        }

        private static void TestEncryptWithHmacHelper(int length, AxCryptOptions options)
        {
            byte[] output;
            byte[] hmacKey;
            using (MemoryStream inputStream = new MemoryStream())
            {
                byte[] text = Resolve.RandomGenerator.Generate(length);
                inputStream.Write(text, 0, text.Length);
                inputStream.Position = 0;
                using (MemoryStream outputStream = new MemoryStream())
                {
                    using (V2AxCryptDocument document = new V2AxCryptDocument(new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("Secret")), 100))
                    {
                        document.EncryptTo(inputStream, outputStream, options);
                        output = outputStream.ToArray();
                        hmacKey = document.DocumentHeaders.GetHmacKey();
                    }
                }
            }

            byte[] hmacBytesFromHeaders = new byte[V2Hmac.RequiredLength];
            Array.Copy(output, output.Length - V2Hmac.RequiredLength, hmacBytesFromHeaders, 0, V2Hmac.RequiredLength);
            V2Hmac hmacFromHeaders = new V2Hmac(hmacBytesFromHeaders);

            byte[] dataToHmac = new byte[output.Length - (V2Hmac.RequiredLength + 5)];
            Array.Copy(output, 0, dataToHmac, 0, dataToHmac.Length);

            HMACSHA512 hmac = new HMACSHA512(hmacKey);
            hmac.TransformFinalBlock(dataToHmac, 0, dataToHmac.Length);
            V2Hmac hmacFromCalculation = new V2Hmac(hmac.Hash);

            Assert.That(hmacFromHeaders, Is.EqualTo(hmacFromCalculation));
        }

        [Test]
        public void TestEncryptDecryptSmall()
        {
            TestEncryptDecryptHelper(15, AxCryptOptions.EncryptWithoutCompression);
        }

        [Test]
        public void TestEncryptDecryptAlmostChunkSize()
        {
            TestEncryptDecryptHelper(V2AxCryptDataStream.WriteChunkSize - 1, AxCryptOptions.EncryptWithoutCompression);
        }

        [Test]
        public void TestEncryptDecryptChunkSize()
        {
            TestEncryptDecryptHelper(V2AxCryptDataStream.WriteChunkSize, AxCryptOptions.EncryptWithoutCompression);
        }

        [Test]
        public void TestEncryptDecryptChunkSizePlusOne()
        {
            TestEncryptDecryptHelper(V2AxCryptDataStream.WriteChunkSize + 1, AxCryptOptions.EncryptWithoutCompression);
        }

        [Test]
        public void TestEncryptDecryptSeveralChunkSizes()
        {
            TestEncryptDecryptHelper(V2AxCryptDataStream.WriteChunkSize * 5, AxCryptOptions.EncryptWithoutCompression);
        }

        [Test]
        public void TestEncryptDecryptIncompleteChunk()
        {
            TestEncryptDecryptHelper(V2AxCryptDataStream.WriteChunkSize * 3 + V2AxCryptDataStream.WriteChunkSize / 2, AxCryptOptions.EncryptWithoutCompression);
        }

        [Test]
        public void TestEncryptDecryptSmallWithCompression()
        {
            TestEncryptDecryptHelper(15, AxCryptOptions.EncryptWithCompression);
        }

        [Test]
        public void TestEncryptDecryptAlmostChunkSizeWithCompression()
        {
            TestEncryptDecryptHelper(V2AxCryptDataStream.WriteChunkSize - 1, AxCryptOptions.EncryptWithCompression);
        }

        [Test]
        public void TestEncryptDecryptChunkSizeWithCompression()
        {
            TestEncryptDecryptHelper(V2AxCryptDataStream.WriteChunkSize, AxCryptOptions.EncryptWithCompression);
        }

        [Test]
        public void TestEncryptDecryptChunkSizePlusOneWithCompression()
        {
            TestEncryptDecryptHelper(V2AxCryptDataStream.WriteChunkSize + 1, AxCryptOptions.EncryptWithCompression);
        }

        [Test]
        public void TestEncryptDecryptSeveralChunkSizesWithCompression()
        {
            TestEncryptDecryptHelper(V2AxCryptDataStream.WriteChunkSize * 5, AxCryptOptions.EncryptWithCompression);
        }

        [Test]
        public void TestEncryptDecryptIncompleteChunkWithCompression()
        {
            TestEncryptDecryptHelper(V2AxCryptDataStream.WriteChunkSize * 3 + V2AxCryptDataStream.WriteChunkSize / 2, AxCryptOptions.EncryptWithCompression);
        }

        private static void TestEncryptDecryptHelper(int length, AxCryptOptions options)
        {
            using (MemoryStream inputStream = new MemoryStream())
            {
                byte[] text = Resolve.RandomGenerator.Generate(length);
                inputStream.Write(text, 0, text.Length);
                inputStream.Position = 0;
                using (MemoryStream outputStream = new MemoryStream())
                {
                    using (IAxCryptDocument document = new V2AxCryptDocument(new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("passphrase")), 113))
                    {
                        document.EncryptTo(inputStream, outputStream, options);

                        outputStream.Position = 0;
                        using (V2AxCryptDocument decryptedDocument = new V2AxCryptDocument())
                        {
                            Assert.That(decryptedDocument.Load(new Passphrase("passphrase"), new V2Aes256CryptoFactory().Id, outputStream), Is.True);
                            byte[] plain;
                            using (MemoryStream decryptedStream = new MemoryStream())
                            {
                                decryptedDocument.DecryptTo(decryptedStream);
                                plain = decryptedStream.ToArray();
                            }

                            Assert.That(plain.IsEquivalentTo(text));
                        }
                    }
                }
            }
        }

        [Test]
        public void TestEncryptToInvalidArguments()
        {
            Stream nullStream = null;

            using (IAxCryptDocument document = new V2AxCryptDocument())
            {
                Assert.Throws<ArgumentNullException>(() => document.EncryptTo(nullStream, Stream.Null, AxCryptOptions.EncryptWithCompression));
                Assert.Throws<ArgumentNullException>(() => document.EncryptTo(Stream.Null, nullStream, AxCryptOptions.EncryptWithCompression));
                Assert.Throws<ArgumentException>(() => document.EncryptTo(Stream.Null, Stream.Null, AxCryptOptions.None));
                Assert.Throws<ArgumentException>(() => document.EncryptTo(Stream.Null, Stream.Null, AxCryptOptions.EncryptWithCompression | AxCryptOptions.EncryptWithoutCompression));
            }
        }

        [Test]
        public void TestLoadWithInvalidPassphrase()
        {
            using (MemoryStream inputStream = new MemoryStream())
            {
                byte[] text = Resolve.RandomGenerator.Generate(1000);
                inputStream.Write(text, 0, text.Length);
                inputStream.Position = 0;
                using (MemoryStream outputStream = new MemoryStream())
                {
                    using (IAxCryptDocument document = new V2AxCryptDocument(new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("passphrase")), 113))
                    {
                        document.EncryptTo(inputStream, outputStream, AxCryptOptions.EncryptWithCompression);

                        outputStream.Position = 0;
                        using (V2AxCryptDocument decryptedDocument = new V2AxCryptDocument())
                        {
                            Assert.That(decryptedDocument.Load(new Passphrase("incorrect"), V2Aes256CryptoFactory.CryptoId, outputStream), Is.False);
                        }
                    }
                }
            }
        }

        [Test]
        public void TestDecryptToWithInvalidArgument()
        {
            Stream nullStream = null;

            using (IAxCryptDocument document = new V2AxCryptDocument())
            {
                Assert.Throws<ArgumentNullException>(() => document.DecryptTo(nullStream));
            }

            using (MemoryStream inputStream = new MemoryStream())
            {
                byte[] text = Resolve.RandomGenerator.Generate(1000);
                inputStream.Write(text, 0, text.Length);
                inputStream.Position = 0;
                using (MemoryStream outputStream = new MemoryStream())
                {
                    using (IAxCryptDocument document = new V2AxCryptDocument(new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("passphrase")), 113))
                    {
                        document.EncryptTo(inputStream, outputStream, AxCryptOptions.EncryptWithCompression);

                        outputStream.Position = 0;
                        using (V2AxCryptDocument decryptedDocument = new V2AxCryptDocument())
                        {
                            Assert.That(decryptedDocument.Load(new Passphrase("incorrect"), V2Aes256CryptoFactory.CryptoId, outputStream), Is.False);
                            Assert.Throws<InternalErrorException>(() => decryptedDocument.DecryptTo(Stream.Null));
                        }
                    }
                }
            }
        }

        [Test]
        public void TestDecryptWithInvalidHmac()
        {
            using (MemoryStream inputStream = new MemoryStream())
            {
                byte[] text = Resolve.RandomGenerator.Generate(1000);
                inputStream.Write(text, 0, text.Length);
                inputStream.Position = 0;
                using (MemoryStream outputStream = new MemoryStream())
                {
                    using (IAxCryptDocument document = new V2AxCryptDocument(new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("passphrase")), 113))
                    {
                        document.EncryptTo(inputStream, outputStream, AxCryptOptions.EncryptWithoutCompression);

                        outputStream.Position = 1000;
                        int b = outputStream.ReadByte();
                        outputStream.Position = 1000;
                        outputStream.WriteByte((byte)(b + 1));
                        outputStream.Position = 0;

                        using (V2AxCryptDocument decryptedDocument = new V2AxCryptDocument())
                        {
                            Assert.That(decryptedDocument.Load(new Passphrase("passphrase"), new V2Aes256CryptoFactory().Id, outputStream), Is.True);
                            Assert.Throws<Axantum.AxCrypt.Core.Runtime.IncorrectDataException>(() => decryptedDocument.DecryptTo(Stream.Null));
                        }
                    }
                }
            }
        }

        [Test]
        public void TestDecryptToWithReaderWronglyPositioned()
        {
            using (MemoryStream inputStream = new MemoryStream())
            {
                byte[] text = Resolve.RandomGenerator.Generate(1000);
                inputStream.Write(text, 0, text.Length);
                inputStream.Position = 0;
                MemoryStream outputStream = new MemoryStream();
                using (IAxCryptDocument document = new V2AxCryptDocument(new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("passphrase")), 113))
                {
                    document.EncryptTo(inputStream, outputStream, AxCryptOptions.EncryptWithCompression);

                    outputStream.Position = 0;
                    Headers headers = new Headers();
                    AxCryptReader reader = headers.CreateReader(new LookAheadStream(outputStream));
                    using (V2AxCryptDocument decryptedDocument = new V2AxCryptDocument(reader))
                    {
                        Assert.That(decryptedDocument.Load(new Passphrase("passphrase"), new V2Aes256CryptoFactory().Id, headers), Is.True);
                        reader.SetStartOfData();
                        Assert.Throws<InvalidOperationException>(() => decryptedDocument.DecryptTo(Stream.Null));
                    }
                }
            }
        }

        [Test]
        public void TestDocumentHeaderProperties()
        {
            using (MemoryStream inputStream = new MemoryStream())
            {
                byte[] text = Resolve.RandomGenerator.Generate(500);
                inputStream.Write(text, 0, text.Length);
                inputStream.Position = 0;
                using (MemoryStream outputStream = new MemoryStream())
                {
                    using (IAxCryptDocument document = new V2AxCryptDocument(new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("properties")), 15))
                    {
                        DateTime utcNow = OS.Current.UtcNow;
                        DateTime lastWrite = utcNow.AddHours(1);
                        DateTime lastAccess = utcNow.AddHours(2);
                        DateTime create = utcNow.AddHours(3);

                        document.CreationTimeUtc = create;
                        document.LastAccessTimeUtc = lastAccess;
                        document.LastWriteTimeUtc = lastWrite;

                        document.FileName = "Property Test.txt";
                        document.EncryptTo(inputStream, outputStream, AxCryptOptions.EncryptWithCompression);

                        outputStream.Position = 0;
                        using (V2AxCryptDocument decryptedDocument = new V2AxCryptDocument())
                        {
                            Assert.That(decryptedDocument.Load(new Passphrase("properties"), V2Aes256CryptoFactory.CryptoId, outputStream), Is.True);

                            Assert.That(decryptedDocument.CreationTimeUtc, Is.EqualTo(create));
                            Assert.That(decryptedDocument.LastAccessTimeUtc, Is.EqualTo(lastAccess));
                            Assert.That(decryptedDocument.LastWriteTimeUtc, Is.EqualTo(lastWrite));
                            Assert.That(decryptedDocument.FileName, Is.EqualTo("Property Test.txt"));
                        }
                    }
                }
            }
        }
    }
}