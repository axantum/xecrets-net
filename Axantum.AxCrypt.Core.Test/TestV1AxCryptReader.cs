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
    public static class TestV1AxCryptReader
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
        [TestCase(CryptoImplementation.WindowsDesktop)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public static void TestPrematureEndOfFile(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetupCrypto(cryptoImplementation);

            V1DocumentHeaders headers = new V1DocumentHeaders(new Passphrase("passphrase"), 10);
            using (MemoryStream stream = new MemoryStream())
            {
                headers.WriteWithoutHmac(stream);
                stream.Position = 0;

                using (V1AxCryptReader reader = new V1AxCryptReader(new LookAheadStream(stream)))
                {
                    AxCryptItemType lastItemType = AxCryptItemType.Undefined;
                    while (reader.Read())
                    {
                        lastItemType = reader.CurrentItemType;
                    }
                    Assert.That(lastItemType, Is.EqualTo(AxCryptItemType.Data));
                    Assert.That(reader.CurrentItemType, Is.EqualTo(AxCryptItemType.Data));

                    reader.SetStartOfData();
                    Assert.That(reader.Read(), Is.False);
                    Assert.That(reader.CurrentItemType, Is.EqualTo(AxCryptItemType.EndOfStream));

                    Assert.That(reader.Read(), Is.False);
                    Assert.That(reader.CurrentItemType, Is.EqualTo(AxCryptItemType.EndOfStream));
                }
            }
        }
    }
}