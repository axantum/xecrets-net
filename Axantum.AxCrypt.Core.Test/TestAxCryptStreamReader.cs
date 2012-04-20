#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Test.Properties;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestAxCryptStreamReader
    {
        [Test]
        public static void TestConstructor()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                using (AxCryptReader axCryptReader = new AxCryptStreamReader(null)) { }
            }, "A non-null input-stream must be specified.");

            using (MemoryStream inputStream = new MemoryStream())
            {
                AxCrypt1Guid.Write(inputStream);
                inputStream.Position = 0;
                using (AxCryptReader axCryptReader = new AxCryptStreamReader(inputStream))
                {
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the Guid");
                    Assert.That(axCryptReader.CurrentItemType, Is.EqualTo(AxCryptItemType.MagicGuid), "We're expecting to have found a MagicGuid");
                }
            }

            using (MemoryStream inputStream = new MemoryStream())
            {
                AxCrypt1Guid.Write(inputStream);
                inputStream.Position = 0;
                // The stream reader supports both externally supplied LookAheadStream or will wrap it if it is not.
                using (AxCryptReader axCryptReader = new AxCryptStreamReader(new LookAheadStream(inputStream)))
                {
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the Guid");
                    Assert.That(axCryptReader.CurrentItemType, Is.EqualTo(AxCryptItemType.MagicGuid), "We're expecting to have found a MagicGuid");
                }
            }
        }

        [Test]
        public static void TestFactoryMethod()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                using (AxCryptReader axCryptReader = AxCryptReader.Create(null)) { }
            }, "A non-null input-stream must be specified.");

            using (MemoryStream inputStream = new MemoryStream())
            {
                AxCrypt1Guid.Write(inputStream);
                inputStream.Position = 0;
                using (AxCryptReader axCryptReader = AxCryptReader.Create(inputStream))
                {
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the Guid");
                    Assert.That(axCryptReader.CurrentItemType, Is.EqualTo(AxCryptItemType.MagicGuid), "We're expecting to have found a MagicGuid");
                }
            }
        }

        [Test]
        public static void TestCreateEncryptedDataStreamErrorChecks()
        {
            using (MemoryStream inputStream = new MemoryStream())
            {
                using (AxCryptReader axCryptReader = new AxCryptStreamReader(inputStream))
                {
                    Stream encryptedDataStream;
                    Assert.Throws<ArgumentNullException>(() =>
                    {
                        encryptedDataStream = axCryptReader.CreateEncryptedDataStream(null);
                    }, "A non-null HMAC key must be specified.");

                    Assert.Throws<InvalidOperationException>(() =>
                    {
                        encryptedDataStream = axCryptReader.CreateEncryptedDataStream(new AesKey());
                    }, "The reader is not positioned properly to read encrypted data.");

                    axCryptReader.Dispose();

                    Assert.Throws<ObjectDisposedException>(() =>
                    {
                        encryptedDataStream = axCryptReader.CreateEncryptedDataStream(new AesKey());
                    }, "The reader is disposed.");
                }
            }
        }

        [Test]
        public static void TestHmac()
        {
            using (Stream inputStream = new MemoryStream(Resources.HelloWorld_Key_a_txt))
            {
                using (AxCryptReader axCryptReader = new AxCryptStreamReader(inputStream))
                {
                    DataHmac hmac;
                    Assert.Throws<InvalidOperationException>(() =>
                    {
                        hmac = axCryptReader.Hmac;
                    }, "The reader is not positioned properly to get the HMAC.");

                    Passphrase passphrase = new Passphrase("a");
                    DocumentHeaders documentHeaders = new DocumentHeaders(passphrase.DerivedPassphrase);
                    bool keyIsOk = documentHeaders.Load(axCryptReader);
                    Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");

                    using (Stream encrypedDataStream = axCryptReader.CreateEncryptedDataStream(documentHeaders.HmacSubkey.Key))
                    {
                        Assert.Throws<InvalidOperationException>(() =>
                        {
                            hmac = axCryptReader.Hmac;
                        }, "We have not read the encrypted data yet.");

                        encrypedDataStream.CopyTo(Stream.Null);
                        Assert.That(documentHeaders.Hmac, Is.EqualTo(axCryptReader.Hmac), "The HMAC should be correct.");
                    }
                }
            }
        }
    }
}