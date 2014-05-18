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
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestV2AxCryptReader
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times"), Test]
        public static void TestGetCryptoFromHeaders()
        {
            Headers headers = new Headers();
            V2DocumentHeaders documentHeaders = new V2DocumentHeaders(new Passphrase("passphrase"), CryptoFactory.Aes256Id, 10);
            using (Stream chainedStream = new MemoryStream())
            {
                using (V2HmacStream stream = new V2HmacStream(new byte[0], chainedStream))
                {
                    documentHeaders.WriteStartWithHmac(stream);
                    stream.Flush();
                    chainedStream.Position = 0;

                    using (V2AxCryptReader reader = new V2AxCryptReader(chainedStream))
                    {
                        while (reader.Read())
                        {
                            if (reader.CurrentItemType == AxCryptItemType.HeaderBlock)
                            {
                                headers.HeaderBlocks.Add(reader.CurrentHeaderBlock);
                            }
                        }
                        V2KeyWrapHeaderBlock keyWrap = headers.FindHeaderBlock<V2KeyWrapHeaderBlock>();
                        SymmetricKey key = new V2Aes256CryptoFactory().CreatePassphrase(new Passphrase("passphrase"), keyWrap.DerivationSalt, keyWrap.DerivationIterations).DerivedKey;
                        ICrypto cryptoFromReader = reader.Crypto(headers, new Passphrase("passphrase"), CryptoFactory.Aes256Id);
                        Assert.That(key.Equals(cryptoFromReader.Key));
                    }
                }
            }
        }
    }
}