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
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;
using System;
using System.IO;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestAxCryptStreamReader
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
                    Assert.Throws<ArgumentNullException>(() =>
                    {
                        axCryptReader.CreateEncryptedDataStream(null, 0, new ProgressContext());
                    }, "A non-null HMAC key must be specified.");

                    Assert.Throws<ArgumentNullException>(() =>
                    {
                        axCryptReader.CreateEncryptedDataStream(new AesKey(128), 0, null);
                    }, "A non-null ProgresContext must be specified.");

                    Assert.Throws<InvalidOperationException>(() =>
                    {
                        axCryptReader.CreateEncryptedDataStream(new AesKey(128), 0, new ProgressContext());
                    }, "The reader is not positioned properly to read encrypted data.");

                    axCryptReader.Dispose();

                    Assert.Throws<ObjectDisposedException>(() =>
                    {
                        axCryptReader.CreateEncryptedDataStream(new AesKey(128), 0, new ProgressContext());
                    }, "The reader is disposed.");
                }
            }
        }

        [Test]
        public static void TestHmac()
        {
            using (Stream inputStream = FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.helloworld_key_a_txt))
            {
                using (AxCryptReader axCryptReader = new AxCryptStreamReader(inputStream))
                {
                    Assert.Throws<InvalidOperationException>(() =>
                    {
                        if (axCryptReader.Hmac == null) { }
                    }, "The reader is not positioned properly to get the HMAC.");

                    Passphrase passphrase = new Passphrase("a");
                    V1DocumentHeaders documentHeaders = new V1DocumentHeaders(new V1AesCrypto(passphrase.DerivedPassphrase));
                    bool keyIsOk = documentHeaders.Load(axCryptReader);
                    Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");

                    using (Stream encrypedDataStream = axCryptReader.CreateEncryptedDataStream(documentHeaders.HmacSubkey.Key, documentHeaders.CipherTextLength, new ProgressContext()))
                    {
                        Assert.Throws<InvalidOperationException>(() =>
                        {
                            if (axCryptReader.Hmac == null) { }
                        }, "We have not read the encrypted data yet.");

                        Assert.That(axCryptReader.Read(), Is.False, "The reader should be at end of stream now, and Read() should return false.");

                        encrypedDataStream.CopyTo(Stream.Null, 4096);
                        Assert.That(documentHeaders.Hmac, Is.EqualTo(axCryptReader.Hmac), "The HMAC should be correct.");

                        axCryptReader.Dispose();

                        Assert.Throws<ObjectDisposedException>(() =>
                        {
                            DataHmac disposedHmac = axCryptReader.Hmac;
                            Object.Equals(disposedHmac, null);
                        }, "The reader is disposed.");
                    }
                }
            }
        }

        [Test]
        public static void TestObjectDisposed()
        {
            using (Stream inputStream = FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.helloworld_key_a_txt))
            {
                using (AxCryptReader axCryptReader = new AxCryptStreamReader(inputStream))
                {
                    axCryptReader.Dispose();

                    Assert.Throws<ObjectDisposedException>(() =>
                    {
                        bool isOk = axCryptReader.Read();
                        Object.Equals(isOk, null);
                    }, "The reader is disposed.");
                }
            }
        }

        private class AxCryptReaderForTest : AxCryptStreamReader
        {
            public AxCryptReaderForTest(Stream inputStream)
                : base(inputStream)
            {
            }

            public void SetCurrentItemType(AxCryptItemType itemType)
            {
                CurrentItemType = itemType;
            }
        }

        [Test]
        public static void TestUndefinedItemType()
        {
            using (MemoryStream testStream = new MemoryStream())
            {
                using (AxCryptReaderForTest axCryptReader = new AxCryptReaderForTest(testStream))
                {
                    axCryptReader.SetCurrentItemType(AxCryptItemType.Undefined);
                    Assert.Throws<InternalErrorException>(() =>
                    {
                        bool isOk = axCryptReader.Read();
                        Object.Equals(isOk, null);
                    });
                }
            }
        }

        private class BadHeaderBlock : HeaderBlock
        {
            public BadHeaderBlock()
                : base(HeaderBlockType.Undefined, new byte[0])
            {
                FakeHeaderBlockLength = 5;
            }

            public override object Clone()
            {
                throw new NotImplementedException();
            }

            public int FakeHeaderBlockLength { get; set; }

            public void SetHeaderBlockType(HeaderBlockType headerBlockType)
            {
                HeaderBlockType = headerBlockType;
            }

            public override void Write(Stream stream)
            {
                byte[] headerBlockPrefix = new byte[4 + 1];
                BitConverter.GetBytes(FakeHeaderBlockLength).CopyTo(headerBlockPrefix, 0);
                headerBlockPrefix[4] = (byte)HeaderBlockType;
                stream.Write(headerBlockPrefix, 0, headerBlockPrefix.Length);
                stream.Write(GetDataBlockBytesReference(), 0, GetDataBlockBytesReference().Length);
            }
        }

        [Test]
        public static void TestNegativeHeaderBlockType()
        {
            using (MemoryStream inputStream = new MemoryStream())
            {
                AxCrypt1Guid.Write(inputStream);

                BadHeaderBlock badHeaderBlock = new BadHeaderBlock();
                badHeaderBlock.SetHeaderBlockType((HeaderBlockType)(-1));
                badHeaderBlock.Write(inputStream);
                inputStream.Position = 0;

                using (AxCryptReaderForTest axCryptReader = new AxCryptReaderForTest(inputStream))
                {
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the Guid");
                    Assert.That(axCryptReader.CurrentItemType, Is.EqualTo(AxCryptItemType.MagicGuid), "We're expecting to have found a MagicGuid");
                    Assert.Throws<FileFormatException>(() =>
                    {
                        axCryptReader.Read();
                    }, "A negative header block type is not valid.");
                }
            }
        }

        [Test]
        public static void TestNegativeHeaderBlockLength()
        {
            using (MemoryStream inputStream = new MemoryStream())
            {
                AxCrypt1Guid.Write(inputStream);

                BadHeaderBlock badHeaderBlock = new BadHeaderBlock();
                badHeaderBlock.FakeHeaderBlockLength = -50;
                badHeaderBlock.Write(inputStream);
                inputStream.Position = 0;

                using (AxCryptReaderForTest axCryptReader = new AxCryptReaderForTest(inputStream))
                {
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the Guid");
                    Assert.That(axCryptReader.CurrentItemType, Is.EqualTo(AxCryptItemType.MagicGuid), "We're expecting to have found a MagicGuid");
                    Assert.Throws<FileFormatException>(() =>
                    {
                        axCryptReader.Read();
                    }, "A negative header block length is not valid.");
                }
            }
        }

        [Test]
        public static void TestTooLargeHeaderBlockLength()
        {
            using (MemoryStream inputStream = new MemoryStream())
            {
                AxCrypt1Guid.Write(inputStream);

                BadHeaderBlock badHeaderBlock = new BadHeaderBlock();
                badHeaderBlock.FakeHeaderBlockLength = 0x1000000;
                badHeaderBlock.Write(inputStream);
                inputStream.Position = 0;

                using (AxCryptReaderForTest axCryptReader = new AxCryptReaderForTest(inputStream))
                {
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the Guid");
                    Assert.That(axCryptReader.CurrentItemType, Is.EqualTo(AxCryptItemType.MagicGuid), "We're expecting to have found a MagicGuid");
                    Assert.Throws<FileFormatException>(() =>
                    {
                        axCryptReader.Read();
                    }, "A too large header block length is not valid.");
                }
            }
        }

        [Test]
        public static void TestTooShortStream()
        {
            using (MemoryStream inputStream = new MemoryStream())
            {
                AxCrypt1Guid.Write(inputStream);

                BadHeaderBlock badHeaderBlock = new BadHeaderBlock();
                badHeaderBlock.FakeHeaderBlockLength = 5 + 1;
                badHeaderBlock.Write(inputStream);
                inputStream.Position = 0;

                using (AxCryptReaderForTest axCryptReader = new AxCryptReaderForTest(inputStream))
                {
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the Guid");
                    Assert.That(axCryptReader.CurrentItemType, Is.EqualTo(AxCryptItemType.MagicGuid), "We're expecting to have found a MagicGuid");
                    Assert.That(axCryptReader.Read(), Is.False, "The stream is too short and end prematurely and should thus be able to read the block");
                    Assert.That(axCryptReader.CurrentItemType, Is.EqualTo(AxCryptItemType.EndOfStream), "The stream is at an end and current item type should reflect this.");
                }
            }
        }

        [Test]
        public static void TestInvalidHeaderBlockType()
        {
            using (MemoryStream inputStream = new MemoryStream())
            {
                AxCrypt1Guid.Write(inputStream);
                PreambleHeaderBlock preambleHeaderBlock = new PreambleHeaderBlock();
                preambleHeaderBlock.Write(inputStream);

                BadHeaderBlock badHeaderBlock = new BadHeaderBlock();
                badHeaderBlock.FakeHeaderBlockLength = 5;
                badHeaderBlock.SetHeaderBlockType(HeaderBlockType.Encrypted);
                badHeaderBlock.Write(inputStream);
                inputStream.Position = 0;

                using (AxCryptReaderForTest axCryptReader = new AxCryptReaderForTest(inputStream))
                {
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the Guid");
                    Assert.That(axCryptReader.CurrentItemType, Is.EqualTo(AxCryptItemType.MagicGuid), "We're expecting to have found a MagicGuid");
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the next HeaderBlock");
                    Assert.That(axCryptReader.CurrentItemType, Is.EqualTo(AxCryptItemType.HeaderBlock), "We're expecting to have found a HeaderBlock");
                    Assert.That(axCryptReader.CurrentHeaderBlock.HeaderBlockType, Is.EqualTo(HeaderBlockType.Preamble), "We're expecting to have found a Preamble specifically");

                    Assert.Throws<FileFormatException>(() =>
                    {
                        axCryptReader.Read();
                    });
                }
            }
        }

        [Test]
        public static void TestKeyWrap2HeaderBlock()
        {
            using (MemoryStream inputStream = new MemoryStream())
            {
                AxCrypt1Guid.Write(inputStream);
                PreambleHeaderBlock preambleHeaderBlock = new PreambleHeaderBlock();
                preambleHeaderBlock.Write(inputStream);
                V1KeyWrap2HeaderBlock keyWrap2HeaderBlock = new V1KeyWrap2HeaderBlock(new byte[0]);
                keyWrap2HeaderBlock.Write(inputStream);
                inputStream.Position = 0;
                using (AxCryptReader axCryptReader = AxCryptReader.Create(inputStream))
                {
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the Guid");
                    Assert.That(axCryptReader.CurrentItemType, Is.EqualTo(AxCryptItemType.MagicGuid), "We're expecting to have found a MagicGuid");
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the next HeaderBlock");
                    Assert.That(axCryptReader.CurrentItemType, Is.EqualTo(AxCryptItemType.HeaderBlock), "We're expecting to have found a HeaderBlock");
                    Assert.That(axCryptReader.CurrentHeaderBlock.HeaderBlockType, Is.EqualTo(HeaderBlockType.Preamble), "We're expecting to have found a Preamble specifically");

                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the next HeaderBlock");
                    Assert.That(axCryptReader.CurrentItemType, Is.EqualTo(AxCryptItemType.HeaderBlock), "We're expecting to have found a HeaderBlock");
                    Assert.That(axCryptReader.CurrentHeaderBlock.HeaderBlockType, Is.EqualTo(HeaderBlockType.KeyWrap2), "We're expecting to have found a KeyWrap2 specifically");
                }
            }
        }

        [Test]
        public static void TestUnrecognizedHeaderBlock()
        {
            using (MemoryStream inputStream = new MemoryStream())
            {
                AxCrypt1Guid.Write(inputStream);
                PreambleHeaderBlock preambleHeaderBlock = new PreambleHeaderBlock();
                preambleHeaderBlock.Write(inputStream);
                UnrecognizedHeaderBlock unrecognizedHeaderBlock = new UnrecognizedHeaderBlock(HeaderBlockType.Unrecognized, new byte[0]);
                unrecognizedHeaderBlock.Write(inputStream);
                inputStream.Position = 0;
                using (AxCryptReader axCryptReader = AxCryptReader.Create(inputStream))
                {
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the Guid");
                    Assert.That(axCryptReader.CurrentItemType, Is.EqualTo(AxCryptItemType.MagicGuid), "We're expecting to have found a MagicGuid");
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the next HeaderBlock");
                    Assert.That(axCryptReader.CurrentItemType, Is.EqualTo(AxCryptItemType.HeaderBlock), "We're expecting to have found a HeaderBlock");
                    Assert.That(axCryptReader.CurrentHeaderBlock.HeaderBlockType, Is.EqualTo(HeaderBlockType.Preamble), "We're expecting to have found a Preamble specifically");

                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the next HeaderBlock");
                    Assert.That(axCryptReader.CurrentItemType, Is.EqualTo(AxCryptItemType.HeaderBlock), "We're expecting to have found a HeaderBlock");
                    Assert.That(axCryptReader.CurrentHeaderBlock.HeaderBlockType, Is.EqualTo(HeaderBlockType.Unrecognized), "We're expecting to have found an unrecognized block specifically");
                }
            }
        }
    }
}