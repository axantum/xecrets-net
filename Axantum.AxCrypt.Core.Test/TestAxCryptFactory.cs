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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestAxCryptFactory
    {
        private CryptoImplementation _cryptoImplementation;

        public TestAxCryptFactory(CryptoImplementation cryptoImplementation)
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
        public void TestCreateDocumentBadArgument()
        {
            AxCryptFactory axFactory = new AxCryptFactory();

            IAxCryptDocument document = null;
            Assert.Throws<ArgumentException>(() => document = axFactory.CreateDocument(new EncryptionParameters(Guid.NewGuid(), new Passphrase("toohigh"))));
            Assert.That(document, Is.Null);
        }

        [Test]
        public void TestV2Document()
        {
            using (MemoryStream inputStream = new MemoryStream())
            {
                byte[] text = Resolve.RandomGenerator.Generate(500);
                inputStream.Write(text, 0, text.Length);
                inputStream.Position = 0;
                byte[] buffer = new byte[2500];
                using (V2AxCryptDocument document = new V2AxCryptDocument(new EncryptionParameters(new V2Aes256CryptoFactory().CryptoId, new Passphrase("properties")), 15))
                {
                    document.EncryptTo(inputStream, new MemoryStream(buffer), AxCryptOptions.EncryptWithCompression);
                }
                AxCryptFactory axFactory = new AxCryptFactory();
                IEnumerable<DecryptionParameter> decryptionParameters = DecryptionParameter.CreateAll(new Passphrase[] { new Passphrase("properties") }, null, new Guid[] { new V2Aes256CryptoFactory().CryptoId });
                using (IAxCryptDocument decryptedDocument = axFactory.CreateDocument(decryptionParameters, new MemoryStream(buffer)))
                {
                    Assert.That(decryptedDocument.PassphraseIsValid);
                }
            }
        }
    }
}