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

using AxCrypt.Core.Header;
using AxCrypt.Core.IO;
using AxCrypt.Core.Reader;
using AxCrypt.Core.Runtime;
using Xecrets.Net.Core.Test.Properties;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.IO;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestAxCryptStreamReader
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times"), Test]
        public static void TestConstructor()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                using (V1AxCryptReader axCryptReader = new V1AxCryptReader(null)) { }
            }, "A non-null input-stream must be specified.");

            using (MemoryStream inputStream = new MemoryStream())
            {
                AxCrypt1Guid.Write(inputStream);
                inputStream.Position = 0;
                using (V1AxCryptReader axCryptReader = new V1AxCryptReader(new LookAheadStream(inputStream)))
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
                using (V1AxCryptReader axCryptReader = new V1AxCryptReader(new LookAheadStream(inputStream)))
                {
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the Guid");
                    Assert.That(axCryptReader.CurrentItemType, Is.EqualTo(AxCryptItemType.MagicGuid), "We're expecting to have found a MagicGuid");
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times"), Test]
        public static void TestFactoryMethod()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                using (V1AxCryptReader axCryptReader = new V1AxCryptReader(null)) { }
            }, "A non-null input-stream must be specified.");

            using (MemoryStream inputStream = new MemoryStream())
            {
                AxCrypt1Guid.Write(inputStream);
                inputStream.Position = 0;
                using (V1AxCryptReader axCryptReader = new V1AxCryptReader(new LookAheadStream(inputStream)))
                {
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the Guid");
                    Assert.That(axCryptReader.CurrentItemType, Is.EqualTo(AxCryptItemType.MagicGuid), "We're expecting to have found a MagicGuid");
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times"), Test]
        public static void TestObjectDisposed()
        {
            using (Stream inputStream = FakeDataStore.ExpandableMemoryStream(Resources.helloworld_key_a_txt))
            {
                using (V1AxCryptReader axCryptReader = new V1AxCryptReader(new LookAheadStream(inputStream)))
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

        private class AxCryptReaderForTest : V1AxCryptReader
        {
            public AxCryptReaderForTest(LookAheadStream inputStream)
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
                using (AxCryptReaderForTest axCryptReader = new AxCryptReaderForTest(new LookAheadStream(testStream)))
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

                using (AxCryptReaderForTest axCryptReader = new AxCryptReaderForTest(new LookAheadStream(inputStream)))
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

                using (AxCryptReaderForTest axCryptReader = new AxCryptReaderForTest(new LookAheadStream(inputStream)))
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

                using (AxCryptReaderForTest axCryptReader = new AxCryptReaderForTest(new LookAheadStream(inputStream)))
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times"), Test]
        public static void TestTooShortStream()
        {
            using (MemoryStream inputStream = new MemoryStream())
            {
                AxCrypt1Guid.Write(inputStream);

                BadHeaderBlock badHeaderBlock = new BadHeaderBlock();
                badHeaderBlock.FakeHeaderBlockLength = 5 + 1;
                badHeaderBlock.Write(inputStream);
                inputStream.Position = 0;

                using (AxCryptReaderForTest axCryptReader = new AxCryptReaderForTest(new LookAheadStream(inputStream)))
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

                using (AxCryptReaderForTest axCryptReader = new AxCryptReaderForTest(new LookAheadStream(inputStream)))
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times"), Test]
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
                using (V1AxCryptReader axCryptReader = new V1AxCryptReader(new LookAheadStream(inputStream)))
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times"), Test]
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
                using (V1AxCryptReader axCryptReader = new V1AxCryptReader(new LookAheadStream(inputStream)))
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
