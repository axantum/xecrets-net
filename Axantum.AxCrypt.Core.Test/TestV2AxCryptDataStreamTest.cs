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

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestV2AxCryptDataStreamTest
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

        private class TestingAxCryptReader : AxCryptReader
        {
            public TestingAxCryptReader(LookAheadStream inputStream)
                : base(inputStream)
            {
            }

            protected override IAxCryptDocument Document()
            {
                throw new NotImplementedException();
            }

            protected override HeaderBlock HeaderBlockFactory(HeaderBlockType headerBlockType, byte[] dataBlock)
            {
                switch (headerBlockType)
                {
                    case HeaderBlockType.Preamble:
                        return new PreambleHeaderBlock(dataBlock);

                    case HeaderBlockType.Version:
                        return new VersionHeaderBlock(dataBlock);

                    case HeaderBlockType.Data:
                        return new DataHeaderBlock(dataBlock);

                    case HeaderBlockType.EncryptedDataPart:
                        return new EncryptedDataPartBlock(dataBlock);
                }
                return new UnrecognizedHeaderBlock(headerBlockType, dataBlock);
            }

            public override IAxCryptDocument Document(Passphrase key, Guid cryptoId, Headers headers)
            {
                throw new NotImplementedException();
            }

            public override IAxCryptDocument Document(IAsymmetricPrivateKey privateKey, Guid cryptoId, Headers headers)
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public static void TestSimpleReadWrite()
        {
            byte[] buffer = new byte[2000];

            using (V2AxCryptDataStream axCryptDataStreamWriter = V2AxCryptDataStream.Create(new MemoryStream(buffer)))
            {
                AxCrypt1Guid.Write(axCryptDataStreamWriter.Chained);
                new PreambleHeaderBlock().Write(axCryptDataStreamWriter.Chained);
                new DataHeaderBlock().Write(axCryptDataStreamWriter.Chained);

                byte[] bytes = Encoding.UTF8.GetBytes("This is a short text.");
                axCryptDataStreamWriter.Write(bytes, 0, bytes.Length);
                axCryptDataStreamWriter.Flush();

                new V2HmacHeaderBlock().Write(axCryptDataStreamWriter.Chained);
            }

            using (AxCryptReader reader = new TestingAxCryptReader(new LookAheadStream(new MemoryStream(buffer))))
            {
                while (reader.Read()) ;
                reader.SetStartOfData();
                using (TextReader textReader = new StreamReader(V2AxCryptDataStream.Create(reader, Stream.Null), Encoding.UTF8))
                {
                    string text = textReader.ReadToEnd();
                    Assert.That(text, Is.EqualTo("This is a short text."));
                }
            }
        }

        [Test]
        public static void TestLongerReadWrite()
        {
            byte[] bytesToWrite = new FakeRandomGenerator().Generate(V2AxCryptDataStream.WriteChunkSize + V2AxCryptDataStream.WriteChunkSize / 2);
            byte[] buffer = new byte[bytesToWrite.Length + 2000];
            using (V2AxCryptDataStream axCryptDataStreamWriter = V2AxCryptDataStream.Create(new MemoryStream(buffer)))
            {
                AxCrypt1Guid.Write(axCryptDataStreamWriter.Chained);
                new PreambleHeaderBlock().Write(axCryptDataStreamWriter.Chained);
                new DataHeaderBlock().Write(axCryptDataStreamWriter.Chained);

                axCryptDataStreamWriter.Write(bytesToWrite, 0, bytesToWrite.Length);
                axCryptDataStreamWriter.Flush();
                new V2HmacHeaderBlock().Write(axCryptDataStreamWriter.Chained);
            }

            using (AxCryptReader reader = new TestingAxCryptReader(new LookAheadStream(new MemoryStream(buffer))))
            {
                while (reader.Read()) ;
                reader.SetStartOfData();
                using (V2AxCryptDataStream axCryptDataStreamReader = V2AxCryptDataStream.Create(reader, Stream.Null))
                {
                    byte[] bytesRead = new byte[bytesToWrite.Length];
                    int offset = 0;
                    int count;
                    do
                    {
                        count = axCryptDataStreamReader.Read(bytesRead, offset, 100);
                        offset += count;
                    } while (count > 0);
                    Assert.That(bytesRead, Is.EquivalentTo(bytesToWrite));
                }
            }
        }

        [Test]
        public static void TestUnexpectedEndOfFile()
        {
            byte[] buffer = new byte[2000];
            using (V2AxCryptDataStream axCryptDataStreamWriter = V2AxCryptDataStream.Create(new MemoryStream(buffer)))
            {
                AxCrypt1Guid.Write(axCryptDataStreamWriter.Chained);
                new PreambleHeaderBlock().Write(axCryptDataStreamWriter.Chained);
                new DataHeaderBlock().Write(axCryptDataStreamWriter.Chained);

                byte[] bytes = Encoding.UTF8.GetBytes("This is a short text.");
                axCryptDataStreamWriter.Write(bytes, 0, bytes.Length);
            }

            using (AxCryptReader reader = new TestingAxCryptReader(new LookAheadStream(new MemoryStream(buffer))))
            {
                while (reader.Read()) ;
                reader.SetStartOfData();
                using (V2AxCryptDataStream axCryptDataStreamReader = V2AxCryptDataStream.Create(reader, Stream.Null))
                {
                    using (TextReader textReader = new StreamReader(axCryptDataStreamReader, Encoding.UTF8))
                    {
                        string text = null;
                        Assert.Throws<FileFormatException>(() => text = textReader.ReadToEnd());
                        Assert.That(text, Is.Null);
                    }
                }
            }
        }

        [Test]
        public static void TestUnexpectedBlockType()
        {
            byte[] buffer = new byte[2000];
            using (V2AxCryptDataStream axCryptDataStreamWriter = V2AxCryptDataStream.Create(new MemoryStream(buffer)))
            {
                AxCrypt1Guid.Write(axCryptDataStreamWriter.Chained);
                new PreambleHeaderBlock().Write(axCryptDataStreamWriter.Chained);
                new DataHeaderBlock().Write(axCryptDataStreamWriter.Chained);

                byte[] bytes = Encoding.UTF8.GetBytes("This is a short text.");
                axCryptDataStreamWriter.Write(bytes, 0, bytes.Length);
                axCryptDataStreamWriter.Flush();
                new DataHeaderBlock().Write(axCryptDataStreamWriter.Chained);
            }
            using (AxCryptReader reader = new TestingAxCryptReader(new LookAheadStream(new MemoryStream(buffer))))
            {
                while (reader.Read()) ;
                reader.SetStartOfData();
                using (TextReader textReader = new StreamReader(V2AxCryptDataStream.Create(reader, Stream.Null), Encoding.UTF8))
                {
                    string text = null;
                    Assert.Throws<FileFormatException>(() => text = textReader.ReadToEnd());
                    Assert.That(text, Is.Null);
                }
            }
        }

        [Test]
        public static void TestNotSupportedMethods()
        {
            using (V2AxCryptDataStream stream = V2AxCryptDataStream.Create(Stream.Null))
            {
                long position;
                Assert.Throws<NotSupportedException>(() => position = stream.Position);
                Assert.Throws<NotSupportedException>(() => stream.Position = 0);

                Assert.That(stream.CanSeek, Is.False);

                long length;
                Assert.Throws<NotSupportedException>(() => length = stream.Length);
                Assert.Throws<NotSupportedException>(() => stream.SetLength(0));

                Assert.Throws<NotSupportedException>(() => position = stream.Seek(0, SeekOrigin.Begin));
            }
        }
    }
}