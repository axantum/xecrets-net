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
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestProgressStreamTest
    {
        [Test]
        public static void TestInvalidArguments()
        {
            Stream nullStream = null;
            ProgressContext nullProgress = null;

            ProgressStream progressStream;
            Assert.Throws<ArgumentNullException>(() => { progressStream = new ProgressStream(nullStream, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { progressStream = new ProgressStream(new MemoryStream(), nullProgress); });

            progressStream = new ProgressStream(new MemoryStream(), new ProgressContext());
            byte[] nullBuffer = null;
            Assert.Throws<ArgumentNullException>(() => { progressStream.Write(nullBuffer, 0, 0); });
            Assert.Throws<ArgumentNullException>(() => { progressStream.Read(nullBuffer, 0, 0); });
        }

        [Test]
        public static void TestPropertiesAndMethods()
        {
            using (MemoryStream memoryStream = new MemoryStream(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }))
            {
                string kilroy = String.Empty;
                using (TestStream testStream = new TestStream(memoryStream, (string wasHere) => { kilroy += wasHere; }))
                {
                    using (ProgressStream progressStream = new ProgressStream(testStream, new ProgressContext()))
                    {
                        kilroy = String.Empty;
                        Assert.That(progressStream.CanRead, Is.True, "The underlying stream is readable.");
                        Assert.That(kilroy, Is.EqualTo("CanRead"), "ProgressStream should delegate to the underlying stream.");
                        kilroy = String.Empty;
                        Assert.That(progressStream.CanWrite, Is.True, "The underlying stream is writable.");
                        Assert.That(kilroy, Is.EqualTo("CanWrite"), "ProgressStream should delegate to the underlying stream.");
                        kilroy = String.Empty;
                        Assert.That(progressStream.CanSeek, Is.True, "The underlying stream is seekable.");
                        Assert.That(kilroy, Is.EqualTo("CanSeek"), "ProgressStream should delegate to the underlying stream.");
                        kilroy = String.Empty;
                        progressStream.Flush();
                        Assert.That(kilroy, Is.EqualTo("Flush"), "ProgressStream should delegate to the underlying stream.");
                        kilroy = String.Empty;
                        Assert.That(progressStream.Length, Is.EqualTo(10), "There are 10 bytes  in the underlying stream.");
                        Assert.That(kilroy, Is.EqualTo("Length"), "ProgressStream should delegate to the underlying stream.");
                        kilroy = String.Empty;
                        Assert.That(progressStream.Seek(-4, SeekOrigin.End), Is.EqualTo(6), "4 bytes from the end of 10 should be 6.");
                        Assert.That(kilroy, Is.EqualTo("Seek"), "ProgressStream should delegate to the underlying stream.");
                        kilroy = String.Empty;
                        Assert.That(progressStream.Position, Is.EqualTo(6), "The position should still be at 6.");
                        Assert.That(kilroy, Is.EqualTo("getPosition"), "ProgressStream should delegate to the underlying stream.");
                        kilroy = String.Empty;
                        progressStream.Position = 0;
                        Assert.That(kilroy, Is.EqualTo("setPosition"), "ProgressStream should delegate to the underlying stream.");
                        kilroy = String.Empty;
                        progressStream.Write(new byte[] { 13 }, 0, 1);
                        Assert.That(kilroy.Contains("Write"), Is.True, "ProgressStream should delegate to the underlying stream.");
                        kilroy = String.Empty;
                        progressStream.Position = 0;
                        byte[] firstByte = new byte[1];
                        progressStream.Read(firstByte, 0, 1);
                        Assert.That(kilroy.Contains("Read"), Is.True, "ProgressStream should delegate to the underlying stream.");
                        kilroy = String.Empty;
                        Assert.That(firstByte[0], Is.EqualTo(13), "13 was just written to the first position.");
                        progressStream.SetLength(5);
                        Assert.That(kilroy, Is.EqualTo("SetLength"), "ProgressStream should delegate to the underlying stream.");
                    }
                }
            }
        }

        [Test]
        public static void TestDoubleDispose()
        {
            using (Stream stream = Stream.Null)
            {
                Assert.DoesNotThrow(() =>
                {
                    using (ProgressStream progressStream = new ProgressStream(stream, new ProgressContext()))
                    {
                        progressStream.Dispose();
                    }
                });
            }
        }
    }
}