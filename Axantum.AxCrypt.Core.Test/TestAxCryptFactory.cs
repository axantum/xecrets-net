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
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestAxCryptFactory
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
        public static void TestCreateDocumentBadArgument()
        {
            AxCryptFactory axFactory = new AxCryptFactory();

            IAxCryptDocument document = null;
            Assert.Throws<ArgumentException>(() => document = axFactory.CreateDocument(new Passphrase("toohigh"), Guid.NewGuid()));
            Assert.That(document, Is.Null);
        }

        [Test]
        public static void TestV2Document()
        {
            using (MemoryStream inputStream = new MemoryStream())
            {
                byte[] text = Instance.RandomGenerator.Generate(500);
                inputStream.Write(text, 0, text.Length);
                inputStream.Position = 0;
                using (MemoryStream outputStream = new MemoryStream())
                {
                    using (V2AxCryptDocument document = new V2AxCryptDocument(new Passphrase("properties"), CryptoFactory.Aes256Id, 15))
                    {
                        document.EncryptTo(inputStream, outputStream, AxCryptOptions.EncryptWithCompression);
                        outputStream.Position = 0;

                        AxCryptFactory axFactory = new AxCryptFactory();
                        IAxCryptDocument decryptedDocument = axFactory.CreateDocument(new Passphrase("properties"), new V2Aes256CryptoFactory().Id, outputStream);
                        Assert.That(decryptedDocument.PassphraseIsValid);
                    }
                }
            }
        }
    }
}