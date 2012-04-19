﻿#region Coypright and License

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

                        Assert.Throws<NotSupportedException>(() =>
                        {
                            axCryptDataStream.Position = 0;
                        }, "Setting the position is not supported.");

                        Assert.Throws<NotSupportedException>(() =>
                        {
                            axCryptDataStream.Flush();
                        }, "Flush is not supported, and not meaningful on a read-only stream.");

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

        [Test]
        public static void TestHmacStream()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                using (HmacStream hmacStream = new HmacStream(null)) { }
            });

            AesKey key = new AesKey(new byte[16]);
            using (HmacStream hmacStream = new HmacStream(key))
            {
                Assert.That(hmacStream.CanRead, Is.False, "HmacStream does not support reading.");
                Assert.That(hmacStream.CanSeek, Is.False, "HmacStream does not support seeking.");
                Assert.That(hmacStream.CanWrite, Is.True, "HmacStream does support writing.");

                Assert.Throws<NotSupportedException>(() =>
                {
                    byte[] buffer = new byte[5];
                    hmacStream.Read(buffer, 0, buffer.Length);
                });

                Assert.Throws<NotSupportedException>(() =>
                {
                    hmacStream.Seek(0, SeekOrigin.Begin);
                });

                Assert.Throws<NotSupportedException>(() =>
                {
                    hmacStream.SetLength(0);
                });

                Assert.Throws<ArgumentNullException>(() =>
                {
                    hmacStream.ReadFrom(null);
                });

                hmacStream.Write(new byte[10], 0, 10);
                using (Stream dataStream = new MemoryStream())
                {
                    dataStream.Write(new byte[10], 0, 10);
                    dataStream.Position = 0;
                    hmacStream.ReadFrom(dataStream);
                }
                Assert.That(hmacStream.Position, Is.EqualTo(20), "There are 20 bytes written so the position should be 20.");
                Assert.That(hmacStream.Length, Is.EqualTo(20), "There are 20 bytes written so the position should be 20.");
                hmacStream.Flush();
                Assert.That(hmacStream.Position, Is.EqualTo(20), "Nothing should change after Flush(), this is not a buffered stream.");
                Assert.That(hmacStream.Length, Is.EqualTo(20), "Nothing should change after Flush(), this is not a buffered stream.");

                Assert.Throws<NotSupportedException>(() =>
                {
                    hmacStream.Position = 0;
                }, "Position is not supported.");

                DataHmac dataHmac = hmacStream.HmacResult;
                Assert.That(dataHmac.GetBytes(), Is.EquivalentTo(new byte[] { 0x62, 0x6f, 0x2c, 0x61, 0xc7, 0x68, 0x00, 0xb3, 0xa6, 0x8d, 0xf9, 0x55, 0x95, 0xbc, 0x1f, 0xd1 }), "The HMAC of 20 bytes of zero with 128-bit AesKey all zero should be this.");

                Assert.Throws<InvalidOperationException>(() =>
                {
                    hmacStream.Write(new byte[1], 0, 1);
                }, "Can't write to the stream after checking and thus finalizing the HMAC");

                // This also implicitly covers double-dispose since we're in a using block.
                hmacStream.Dispose();

                Assert.Throws<ObjectDisposedException>(() =>
                {
                    DataHmac invalidDataHmac = hmacStream.HmacResult;
                    // Remove FxCop warning
                    Object.Equals(invalidDataHmac, null);
                });
                Assert.Throws<ObjectDisposedException>(() =>
                {
                    hmacStream.Write(new byte[1], 0, 1);
                });
                Assert.Throws<ObjectDisposedException>(() =>
                {
                    using (Stream stream = new MemoryStream())
                    {
                        hmacStream.ReadFrom(stream);
                    }
                });
            }
        }
    }
}