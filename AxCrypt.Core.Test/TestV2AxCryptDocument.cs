#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions copyright © 2022-2024, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets Cli, but is derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * The changes and additions are separately copyrighted and only licensed under GPL v3 or later as detailed below,
 * unless explicitly licensed otherwise. If you use any part of these changes and additions in your software,
 * please see https://www.gnu.org/licenses/ for details of what this means for you.
 * 
 * Warning: If you are using the original AxCrypt code under a non-GPL v3 or later license, these changes and additions
 * are not included in that license. If you use these changes under those circumstances, all your code becomes subject to
 * the GPL v3 or later license, according to the principle of strong copyleft as applied to GPL v3 or later.
 *
 * Xecrets Cli is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets Cli is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets Cli. If not, see
 * https://www.gnu.org/licenses/.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions, as well for commit history detailing changes and additions that fall under the strong
 * copyleft provisions mentioned above. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Xecrets Cli Copyright and GPL License notice
#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
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
 * The source is maintained at http://bitbucket.org/AxCrypt.Desktop.Window-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.Header;
using AxCrypt.Core.IO;
using AxCrypt.Core.Reader;
using AxCrypt.Core.Runtime;
using Xecrets.Net.Core.Test.Properties;
using AxCrypt.Core.UI;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
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
            SetupAssembly.AssemblySetup(_cryptoImplementation);
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
                    using (V2AxCryptDocument document = new V2AxCryptDocument(new EncryptionParameters(new V2Aes256CryptoFactory().CryptoId, new Passphrase("Secret")), 100))
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

        private static byte[] EncrytionHelper(EncryptionParameters encryptionParameters, string fileName, AxCryptOptions options, byte[] plainText)
        {
            byte[] output;
            using (MemoryStream inputStream = new MemoryStream(plainText))
            {
                using (MemoryStream outputStream = new MemoryStream())
                {
                    using (V2AxCryptDocument document = new V2AxCryptDocument(encryptionParameters, 1000))
                    {
                        document.FileName = fileName;
                        document.CreationTimeUtc = New<INow>().Utc;
                        document.LastAccessTimeUtc = document.CreationTimeUtc;
                        document.LastWriteTimeUtc = document.CreationTimeUtc;

                        document.EncryptTo(inputStream, outputStream, options);
                        output = outputStream.ToArray();
                    }
                }
            }
            return output;
        }

        private static byte[] DecryptionHelper(IEnumerable<DecryptionParameter> decryptionParameters, byte[] input)
        {
            using (MemoryStream inputStream = new MemoryStream(input))
            {
                using (IAxCryptDocument document = New<AxCryptFactory>().CreateDocument(decryptionParameters, inputStream))
                {
                    if (!document.PassphraseIsValid)
                    {
                        return null;
                    }

                    using (MemoryStream outputStream = new MemoryStream())
                    {
                        document.DecryptTo(outputStream);

                        return outputStream.ToArray();
                    }
                }
            }
        }

        [Test]
        public static async Task TestEncryptWithOneAsymmetricKey()
        {
            EncryptionParameters encryptionParameters = new EncryptionParameters(new V2Aes256CryptoFactory().CryptoId, new Passphrase("allan"));
            IAsymmetricPublicKey publicKey = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey1);
            await encryptionParameters.AddAsync(new UserPublicKey[] { new UserPublicKey(EmailAddress.Parse("test@test.com"), publicKey), });

            byte[] plainText = Resolve.RandomGenerator.Generate(25000);

            byte[] output = EncrytionHelper(encryptionParameters, "TestEncryptWithOneAsymmetricKey.txt", AxCryptOptions.EncryptWithCompression, plainText);

            IAsymmetricPrivateKey privateKey1 = New<IAsymmetricFactory>().CreatePrivateKey(Resources.PrivateKey1);
            DecryptionParameter decryptionParameter = new DecryptionParameter(privateKey1, new V2Aes256CryptoFactory().CryptoId);
            byte[] decryptedText = DecryptionHelper(new DecryptionParameter[] { decryptionParameter }, output);

            Assert.That(decryptedText, Is.Not.Null, "The deryption failed because no valid decryption parameter was found.");
            Assert.That(decryptedText, Is.EquivalentTo(plainText), "The decrypted text should be the same as was originally encrypted.");
        }

        [Test]
        public static async Task TestEncryptWithOneAsymmetricKeyAndWrongPassphraseButCorrectPrivateKey()
        {
            EncryptionParameters encryptionParameters = new EncryptionParameters(new V2Aes256CryptoFactory().CryptoId, new Passphrase("allan"));
            IAsymmetricPublicKey publicKey = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey1);
            await encryptionParameters.AddAsync(new UserPublicKey[] { new UserPublicKey(EmailAddress.Parse("test@test.com"), publicKey), });

            byte[] plainText = Resolve.RandomGenerator.Generate(25000);

            byte[] output = EncrytionHelper(encryptionParameters, "TestEncryptWithOneAsymmetricKeyAndWrongPassphraseButCorrectPrivateKey.txt", AxCryptOptions.EncryptWithCompression, plainText);

            DecryptionParameter decryptionParameter1 = new DecryptionParameter(new Passphrase("niklas"), new V2Aes256CryptoFactory().CryptoId);
            IAsymmetricPrivateKey privateKey1 = New<IAsymmetricFactory>().CreatePrivateKey(Resources.PrivateKey1);
            DecryptionParameter decryptionParameter2 = new DecryptionParameter(privateKey1, new V2Aes256CryptoFactory().CryptoId);
            byte[] decryptedText = DecryptionHelper(new DecryptionParameter[] { decryptionParameter1, decryptionParameter2, }, output);

            Assert.That(decryptedText, Is.Not.Null, "The deryption failed because no valid decryption parameter was found.");
            Assert.That(decryptedText, Is.EquivalentTo(plainText), "The decrypted text should be the same as was originally encrypted.");
        }

        [Test]
        public static async Task TestEncryptWithOneAsymmetricKeyAndCorrectPassphraseButWrongPrivateKey()
        {
            EncryptionParameters encryptionParameters = new EncryptionParameters(new V2Aes256CryptoFactory().CryptoId, new Passphrase("allan"));
            IAsymmetricPublicKey publicKey1 = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey1);
            await encryptionParameters.AddAsync(new UserPublicKey[] { new UserPublicKey(EmailAddress.Parse("tes1t@test.com"), publicKey1), });

            byte[] plainText = Resolve.RandomGenerator.Generate(25000);

            byte[] output = EncrytionHelper(encryptionParameters, "TestEncryptWithOneAsymmetricKeyAndCorrectPassphraseButWrongPrivateKey.txt", AxCryptOptions.EncryptWithCompression, plainText);

            DecryptionParameter decryptionParameter1 = new DecryptionParameter(new Passphrase("allan"), new V2Aes256CryptoFactory().CryptoId);
            IAsymmetricPrivateKey privateKey2 = New<IAsymmetricFactory>().CreatePrivateKey(Resources.PrivateKey2);
            DecryptionParameter decryptionParameter2 = new DecryptionParameter(privateKey2, new V2Aes256CryptoFactory().CryptoId);
            byte[] decryptedText = DecryptionHelper(new DecryptionParameter[] { decryptionParameter1, decryptionParameter2, }, output);

            Assert.That(decryptedText, Is.Not.Null, "The deryption failed because no valid decryption parameter was found.");
            Assert.That(decryptedText, Is.EquivalentTo(plainText), "The decrypted text should be the same as was originally encrypted.");
        }

        [Test]
        public static async Task TestEncryptWithTwoAsymmetricKeysAndOneCorrectPrivateKey()
        {
            EncryptionParameters encryptionParameters = new EncryptionParameters(new V2Aes256CryptoFactory().CryptoId, new Passphrase("allan"));
            IAsymmetricPublicKey publicKey1 = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey1);
            await encryptionParameters.AddAsync(new UserPublicKey[] { new UserPublicKey(EmailAddress.Parse("test1@test.com"), publicKey1), });
            IAsymmetricPublicKey publicKey2 = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey2);
            await encryptionParameters.AddAsync(new UserPublicKey[] { new UserPublicKey(EmailAddress.Parse("test2@test.com"), publicKey2), });

            byte[] plainText = Resolve.RandomGenerator.Generate(25000);

            byte[] output = EncrytionHelper(encryptionParameters, "TestEncryptWithTwoAsymmetricKeysAndOneCorrectPrivateKey.txt", AxCryptOptions.EncryptWithCompression, plainText);

            DecryptionParameter decryptionParameter0 = new DecryptionParameter(new Passphrase("niklas"), new V2Aes256CryptoFactory().CryptoId);
            IAsymmetricPrivateKey privateKey1 = New<IAsymmetricFactory>().CreatePrivateKey(Resources.PrivateKey1);
            DecryptionParameter decryptionParameter1 = new DecryptionParameter(privateKey1, new V2Aes256CryptoFactory().CryptoId);
            IAsymmetricPrivateKey privateKey2 = New<IAsymmetricFactory>().CreatePrivateKey(Resources.PrivateKey2);
            DecryptionParameter decryptionParameter2 = new DecryptionParameter(privateKey2, new V2Aes256CryptoFactory().CryptoId);
            byte[] decryptedText = DecryptionHelper(new DecryptionParameter[] { decryptionParameter0, decryptionParameter1, decryptionParameter2, }, output);

            Assert.That(decryptedText, Is.Not.Null, "The deryption failed because no valid decryption parameter was found.");
            Assert.That(decryptedText, Is.EquivalentTo(plainText), "The decrypted text should be the same as was originally encrypted.");
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
                byte[] buffer = new byte[5500 + length];
                using (IAxCryptDocument document = new V2AxCryptDocument(new EncryptionParameters(new V2Aes256CryptoFactory().CryptoId, new Passphrase("passphrase")), 113))
                {
                    document.EncryptTo(inputStream, new MemoryStream(buffer), options);
                }
                using (V2AxCryptDocument decryptedDocument = new V2AxCryptDocument())
                {
                    Assert.That(decryptedDocument.Load(new Passphrase("passphrase"), new V2Aes256CryptoFactory().CryptoId, new MemoryStream(buffer)), Is.True);
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
                byte[] buffer = new byte[2500];
                using (IAxCryptDocument document = new V2AxCryptDocument(new EncryptionParameters(new V2Aes256CryptoFactory().CryptoId, new Passphrase("passphrase")), 113))
                {
                    document.EncryptTo(inputStream, new MemoryStream(buffer), AxCryptOptions.EncryptWithCompression);
                }

                using (V2AxCryptDocument decryptedDocument = new V2AxCryptDocument())
                {
                    Assert.That(decryptedDocument.Load(new Passphrase("incorrect"), new V2Aes256CryptoFactory().CryptoId, new MemoryStream(buffer)), Is.False);
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
                byte[] buffer = new byte[2500];
                using (IAxCryptDocument document = new V2AxCryptDocument(new EncryptionParameters(new V2Aes256CryptoFactory().CryptoId, new Passphrase("passphrase")), 113))
                {
                    document.EncryptTo(inputStream, new MemoryStream(buffer), AxCryptOptions.EncryptWithCompression);
                    using (V2AxCryptDocument decryptedDocument = new V2AxCryptDocument())
                    {
                        Assert.That(decryptedDocument.Load(new Passphrase("incorrect"), new V2Aes256CryptoFactory().CryptoId, new MemoryStream(buffer)), Is.False);
                        Assert.Throws<InternalErrorException>(() => decryptedDocument.DecryptTo(Stream.Null));
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
                byte[] buffer = new byte[3000];
                using (IAxCryptDocument document = new V2AxCryptDocument(new EncryptionParameters(new V2Aes256CryptoFactory().CryptoId, new Passphrase("passphrase")), 113))
                {
                    document.EncryptTo(inputStream, new MemoryStream(buffer), AxCryptOptions.EncryptWithoutCompression);

                    buffer[1000] = (byte)(buffer[1000] + 1);

                    using (V2AxCryptDocument decryptedDocument = new V2AxCryptDocument())
                    {
                        Assert.That(decryptedDocument.Load(new Passphrase("passphrase"), new V2Aes256CryptoFactory().CryptoId, new MemoryStream(buffer)), Is.True);
                        Assert.Throws<AxCrypt.Core.Runtime.IncorrectDataException>(() => decryptedDocument.DecryptTo(Stream.Null));
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
                byte[] buffer = new byte[2500];
                using (IAxCryptDocument document = new V2AxCryptDocument(new EncryptionParameters(new V2Aes256CryptoFactory().CryptoId, new Passphrase("passphrase")), 113))
                {
                    document.EncryptTo(inputStream, new MemoryStream(buffer), AxCryptOptions.EncryptWithCompression);

                    Headers headers = new Headers();
                    AxCryptReader reader = headers.CreateReader(new LookAheadStream(new MemoryStream(buffer)));
                    using (V2AxCryptDocument decryptedDocument = new V2AxCryptDocument(reader))
                    {
                        Assert.That(decryptedDocument.Load(new Passphrase("passphrase"), new V2Aes256CryptoFactory().CryptoId, headers), Is.True);
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
                byte[] buffer = new byte[3000];
                using (IAxCryptDocument document = new V2AxCryptDocument(new EncryptionParameters(new V2Aes256CryptoFactory().CryptoId, new Passphrase("properties")), 15))
                {
                    DateTime utcNow = New<INow>().Utc;
                    DateTime lastWrite = utcNow.AddHours(1);
                    DateTime lastAccess = utcNow.AddHours(2);
                    DateTime create = utcNow.AddHours(3);

                    document.CreationTimeUtc = create;
                    document.LastAccessTimeUtc = lastAccess;
                    document.LastWriteTimeUtc = lastWrite;

                    document.FileName = "Property Test.txt";
                    document.EncryptTo(inputStream, new MemoryStream(buffer), AxCryptOptions.EncryptWithCompression);

                    using (V2AxCryptDocument decryptedDocument = new V2AxCryptDocument())
                    {
                        Assert.That(decryptedDocument.Load(new Passphrase("properties"), new V2Aes256CryptoFactory().CryptoId, new MemoryStream(buffer)), Is.True);

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
