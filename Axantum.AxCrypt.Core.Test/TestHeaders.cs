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

using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Runtime;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestHeaders
    {
        [SetUp]
        public static void Setup()
        {
            TypeMap.Register.Singleton<IRuntimeEnvironment>(() => new FakeRuntimeEnvironment());
            TypeMap.Register.Singleton<ILogging>(() => new FakeLogging());
        }

        [TearDown]
        public static void Teardown()
        {
            TypeMap.Register.Clear();
        }

        private class NewHeaderBlock : HeaderBlock
        {
            public NewHeaderBlock(byte[] dataBlock)
                : base((HeaderBlockType)99, dataBlock)
            {
            }

            public override object Clone()
            {
                NewHeaderBlock block = new NewHeaderBlock((byte[])GetDataBlockBytesReference().Clone());
                return block;
            }
        }

        [Test]
        public static void TestTooNewVersionTooHigh()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                AxCrypt1Guid.Write(stream);
                new PreambleHeaderBlock().Write(stream);
                new VersionHeaderBlock(new byte[] { 5, 0, 2, 0, 0 }).Write(stream);
                new NewHeaderBlock(new byte[0]).Write(stream);
                new DataHeaderBlock().Write(stream);
                stream.Position = 0;

                Assert.Throws<FileFormatException>(() => new Headers().CreateReader(new LookAheadStream(stream)));
            }
        }

        [Test]
        public static void TestTooNewVersionTooLow()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                AxCrypt1Guid.Write(stream);
                new PreambleHeaderBlock().Write(stream);
                new VersionHeaderBlock(new byte[] { 0, 0, 2, 0, 0 }).Write(stream);
                new NewHeaderBlock(new byte[0]).Write(stream);
                new DataHeaderBlock().Write(stream);
                stream.Position = 0;

                Assert.Throws<FileFormatException>(() => new Headers().CreateReader(new LookAheadStream(stream)));
            }
        }

        [Test]
        public static void TestEnsureFileVersion()
        {
            Headers headers = new Headers();
            headers.HeaderBlocks.Add(new PreambleHeaderBlock());
            headers.HeaderBlocks.Add(new VersionHeaderBlock(new byte[] { 4, 1, 2, 0, 0 }));
            headers.HeaderBlocks.Add(new DataHeaderBlock());

            Assert.DoesNotThrow(() => headers.EnsureFileFormatVersion(4, 4));
            Assert.Throws<FileFormatException>(() => headers.EnsureFileFormatVersion(5, 5));
            Assert.Throws<FileFormatException>(() => headers.EnsureFileFormatVersion(1, 3));
        }

        [Test]
        public static void TestFindHeaderBlockNotFound()
        {
            Headers headers = new Headers();
            headers.HeaderBlocks.Add(new PreambleHeaderBlock());
            headers.HeaderBlocks.Add(new VersionHeaderBlock(new byte[] { 4, 1, 2, 0, 0 }));
            headers.HeaderBlocks.Add(new DataHeaderBlock());

            FileInfoEncryptedHeaderBlock fileInfoHeaderBlock = headers.FindHeaderBlock<FileInfoEncryptedHeaderBlock>();
            Assert.That(fileInfoHeaderBlock, Is.Null);
        }

        [Test]
        public static void TestBadAxCryptReader()
        {
            Mock<AxCryptReaderBase> mockAxCryptReader = new Mock<AxCryptReaderBase>(new LookAheadStream(Stream.Null));
            mockAxCryptReader.Setup(r => r.Read()).Returns(true);
            mockAxCryptReader.Setup(r => r.CurrentItemType).Returns(AxCryptItemType.MagicGuid);

            Headers headers = new Headers();
            Assert.Throws<InternalErrorException>(() => headers.Load(mockAxCryptReader.Object));
        }

        [Test]
        public static void TestEndOfFileAxCryptReader()
        {
            Mock<AxCryptReaderBase> mockAxCryptReader = new Mock<AxCryptReaderBase>(new LookAheadStream(Stream.Null));
            mockAxCryptReader.Setup(r => r.Read()).Returns(false);

            Headers headers = new Headers();
            Assert.Throws<FileFormatException>(() => headers.Load(mockAxCryptReader.Object));
        }
    }
}