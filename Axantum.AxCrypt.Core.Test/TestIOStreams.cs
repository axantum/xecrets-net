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
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestIOStreams
    {
        [Test]
        public static void TestAxCryptDataStream()
        {
            string streamData = "This is some data in the streamEXTRA";
            using (Stream inputStream = new MemoryStream())
            {
                byte[] streamBytes = Encoding.UTF8.GetBytes(streamData);
                inputStream.Write(streamBytes, 0, streamBytes.Length);

                using (Stream hmacStream = new MemoryStream())
                {
                    Assert.Throws<ArgumentNullException>(() =>
                    {
                        using (AxCryptDataStream axCryptDataStream = new AxCryptDataStream(null, hmacStream, inputStream.Length)) { }
                    }, "An input stream must be given, it cannot be null.");
                    Assert.Throws<ArgumentNullException>(() =>
                    {
                        using (AxCryptDataStream axCryptDataStream = new AxCryptDataStream(inputStream, null, inputStream.Length)) { }
                    }, "An HmacStream must be given, it cannot be null.");
                    Assert.Throws<ArgumentOutOfRangeException>(() =>
                    {
                        using (AxCryptDataStream axCryptDataStream = new AxCryptDataStream(inputStream, hmacStream, -inputStream.Length)) { }
                    }, "Negative length is not allowed.");

                    inputStream.Position = 0;
                    using (AxCryptDataStream axCryptDataStream = new AxCryptDataStream(inputStream, hmacStream, inputStream.Length - 5))
                    {
                        Assert.Throws<NotSupportedException>(() =>
                        {
                            axCryptDataStream.Seek(0, SeekOrigin.Begin);
                        }, "Seek is not supported.");

                        Assert.Throws<NotSupportedException>(() =>
                        {
                            axCryptDataStream.SetLength(0);
                        }, "SetLength is not supported.");

                        Assert.Throws<NotSupportedException>(() =>
                        {
                            axCryptDataStream.Write(new byte[1], 0, 1);
                        }, "Write is not supported.");

                        Assert.That(axCryptDataStream.CanRead, Is.True, "AxCryptDataStream can be read.");
                        Assert.That(axCryptDataStream.CanSeek, Is.False, "AxCryptDataStream is a forward only reader stream, it does not support seeking.");
                        Assert.That(axCryptDataStream.CanWrite, Is.False, "AxCryptDataStream is a forward only reader stream, it does not support writing.");
                        Assert.That(axCryptDataStream.Length, Is.EqualTo(inputStream.Length - 5), "The stream should report the length provided in the constructor.");
                        inputStream.Position = 5;
                        Assert.That(axCryptDataStream.Position, Is.EqualTo(0), "The original position should be zero, regardless of the actual position of the input stream.");
                        inputStream.Position = 0;

                        byte[] buffer = new byte[3];
                        int count;
                        int total = 0;
                        while ((count = axCryptDataStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            total += count;
                        }
                        Assert.That(total, Is.EqualTo(inputStream.Length - 5), "The AxCryptDataStream should be limited to the length provided, not the backing stream.");
                        Assert.That(hmacStream.Length, Is.EqualTo(total), "The hmac stream should have all data read written to it.");
                    }
                }
            }
        }
    }
}