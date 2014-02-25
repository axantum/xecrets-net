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
using Axantum.AxCrypt.Core.IO;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestV2AxCryptDocument
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
        public static void TestEncryptWithHmacSmall()
        {
            TestEncryptWithHmacHelper(23, AxCryptOptions.EncryptWithoutCompression);
        }

        [Test]
        public static void TestEncryptWithHmacAlmostChunkSize()
        {
            TestEncryptWithHmacHelper(V2AxCryptDataStream.WriteChunkSize - 1, AxCryptOptions.EncryptWithoutCompression);
        }

        [Test]
        public static void TestEncryptWithHmacChunkSize()
        {
            TestEncryptWithHmacHelper(V2AxCryptDataStream.WriteChunkSize, AxCryptOptions.EncryptWithoutCompression);
        }

        [Test]
        public static void TestEncryptWithHmacSeveralChunkSizes()
        {
            TestEncryptWithHmacHelper(V2AxCryptDataStream.WriteChunkSize * 5, AxCryptOptions.EncryptWithoutCompression);
        }

        [Test]
        public static void TestEncryptWithHmacIncompleteChunkSizes()
        {
            TestEncryptWithHmacHelper(V2AxCryptDataStream.WriteChunkSize * 3 + V2AxCryptDataStream.WriteChunkSize / 2, AxCryptOptions.EncryptWithoutCompression);
        }

        [Test]
        public static void TestEncryptWithHmacSmallWithCompression()
        {
            TestEncryptWithHmacHelper(23, AxCryptOptions.EncryptWithCompression);
        }

        [Test]
        public static void TestEncryptWithHmacAlmostChunkSizeWithCompression()
        {
            TestEncryptWithHmacHelper(V2AxCryptDataStream.WriteChunkSize - 1, AxCryptOptions.EncryptWithCompression);
        }

        [Test]
        public static void TestEncryptWithHmacChunkSizeWithCompression()
        {
            TestEncryptWithHmacHelper(V2AxCryptDataStream.WriteChunkSize, AxCryptOptions.EncryptWithCompression);
        }

        [Test]
        public static void TestEncryptWithHmacSeveralChunkSizesWithCompression()
        {
            TestEncryptWithHmacHelper(V2AxCryptDataStream.WriteChunkSize * 5, AxCryptOptions.EncryptWithCompression);
        }

        [Test]
        public static void TestEncryptWithHmacIncompleteChunkSizesWithCompression()
        {
            TestEncryptWithHmacHelper(V2AxCryptDataStream.WriteChunkSize * 3 + V2AxCryptDataStream.WriteChunkSize / 2, AxCryptOptions.EncryptWithCompression);
        }

        private static void TestEncryptWithHmacHelper(int length, AxCryptOptions options)
        {
            byte[] output;
            byte[] hmacKey;
            using (MemoryStream inputStream = new MemoryStream())
            {
                byte[] text = Instance.RandomGenerator.Generate(length);
                inputStream.Write(text, 0, text.Length);
                inputStream.Position = 0;
                using (MemoryStream outputStream = new MemoryStream())
                {
                    using (V2AxCryptDocument document = new V2AxCryptDocument(new V2AesCrypto(new V2Passphrase("Secret", 256)), 100))
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
        public static void TestEncryptDecryptSmall()
        {
            TestEncryptDecryptHelper(15, AxCryptOptions.EncryptWithoutCompression);
        }

        [Test]
        public static void TestEncryptDecryptAlmostChunkSize()
        {
            TestEncryptDecryptHelper(V2AxCryptDataStream.WriteChunkSize - 1, AxCryptOptions.EncryptWithoutCompression);
        }

        [Test]
        public static void TestEncryptDecryptChunkSize()
        {
            TestEncryptDecryptHelper(V2AxCryptDataStream.WriteChunkSize, AxCryptOptions.EncryptWithoutCompression);
        }

        [Test]
        public static void TestEncryptDecryptChunkSizePlusOne()
        {
            TestEncryptDecryptHelper(V2AxCryptDataStream.WriteChunkSize + 1, AxCryptOptions.EncryptWithoutCompression);
        }

        [Test]
        public static void TestEncryptDecryptSeveralChunkSizes()
        {
            TestEncryptDecryptHelper(V2AxCryptDataStream.WriteChunkSize * 5, AxCryptOptions.EncryptWithoutCompression);
        }

        [Test]
        public static void TestEncryptDecryptIncompleteChunk()
        {
            TestEncryptDecryptHelper(V2AxCryptDataStream.WriteChunkSize * 3 + V2AxCryptDataStream.WriteChunkSize / 2, AxCryptOptions.EncryptWithoutCompression);
        }

        [Test]
        public static void TestEncryptDecryptSmallWithCompression()
        {
            TestEncryptDecryptHelper(15, AxCryptOptions.EncryptWithCompression);
        }

        [Test]
        public static void TestEncryptDecryptAlmostChunkSizeWithCompression()
        {
            TestEncryptDecryptHelper(V2AxCryptDataStream.WriteChunkSize - 1, AxCryptOptions.EncryptWithCompression);
        }

        [Test]
        public static void TestEncryptDecryptChunkSizeWithCompression()
        {
            TestEncryptDecryptHelper(V2AxCryptDataStream.WriteChunkSize, AxCryptOptions.EncryptWithCompression);
        }

        [Test]
        public static void TestEncryptDecryptChunkSizePlusOneWithCompression()
        {
            TestEncryptDecryptHelper(V2AxCryptDataStream.WriteChunkSize + 1, AxCryptOptions.EncryptWithCompression);
        }

        [Test]
        public static void TestEncryptDecryptSeveralChunkSizesWithCompression()
        {
            TestEncryptDecryptHelper(V2AxCryptDataStream.WriteChunkSize * 5, AxCryptOptions.EncryptWithCompression);
        }

        [Test]
        public static void TestEncryptDecryptIncompleteChunkWithCompression()
        {
            TestEncryptDecryptHelper(V2AxCryptDataStream.WriteChunkSize * 3 + V2AxCryptDataStream.WriteChunkSize / 2, AxCryptOptions.EncryptWithCompression);
        }

        private static void TestEncryptDecryptHelper(int length, AxCryptOptions options)
        {
            IPassphrase key = new V2Passphrase("passphrase", 256);
            using (MemoryStream inputStream = new MemoryStream())
            {
                byte[] text = Instance.RandomGenerator.Generate(length);
                inputStream.Write(text, 0, text.Length);
                inputStream.Position = 0;
                using (MemoryStream outputStream = new MemoryStream())
                {
                    using (IAxCryptDocument document = new V2AxCryptDocument(new V2AesCrypto(key), 113))
                    {
                        document.EncryptTo(inputStream, outputStream, options);

                        outputStream.Position = 0;
                        using (IAxCryptDocument decryptedDocument = new V2AxCryptDocument())
                        {
                            Assert.That(decryptedDocument.Load(key, outputStream), Is.True);
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
    }
}