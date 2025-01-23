#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions Copyright © 2022-2025, Svante Seleborg, All Rights Reserved.
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

using AxCrypt.Core.Crypto;
using AxCrypt.Core.Header;
using AxCrypt.Core.IO;
using AxCrypt.Core.Reader;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestVXAxCryptReader
    {
        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup(CryptoImplementation.WindowsDesktop);
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
        public static void TestHeaderBlockFactory(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetup(cryptoImplementation);

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
            Assert.Throws<NotImplementedException>(() => reader.Document(new Passphrase("test"), new V1Aes128CryptoFactory().CryptoId, new Headers()));
        }
    }
}
