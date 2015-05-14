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
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestVXAxCryptReader
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

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public static void TestHeaderBlockFactory(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetupCrypto(cryptoImplementation);

            V1DocumentHeaders headers = new V1DocumentHeaders(new Passphrase("passphrase"), 10);
            using (MemoryStream stream = new MemoryStream())
            {
                headers.WriteWithoutHmac(stream);
                stream.Position = 0;

                UnversionedAxCryptReader reader = new UnversionedAxCryptReader(new LookAheadStream(stream));
                bool unexpectedHeaderTypeFound = false;
                while (reader.Read())
                {
                    if (reader.CurrentItemType != AxCryptItemType.HeaderBlock)
                    {
                        continue;
                    }
                    switch (reader.CurrentHeaderBlock.HeaderBlockType)
                    {
                        case HeaderBlockType.Preamble:
                        case HeaderBlockType.Version:
                        case HeaderBlockType.Data:
                        case HeaderBlockType.Unrecognized:
                            break;

                        default:
                            unexpectedHeaderTypeFound = !(reader.CurrentHeaderBlock is UnrecognizedHeaderBlock);
                            break;
                    }
                }
                Assert.That(unexpectedHeaderTypeFound, Is.False);
            }
        }

        [Test]
        public static void TestNotImplemented()
        {
            UnversionedAxCryptReader reader = new UnversionedAxCryptReader(new LookAheadStream(Stream.Null));
            Assert.Throws<NotImplementedException>(() => reader.Document(new Passphrase("test"), V1Aes128CryptoFactory.CryptoId, new Headers()));
        }
    }
}