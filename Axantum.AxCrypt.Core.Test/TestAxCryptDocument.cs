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
 * The source is maintained at http://AxCrypt.codeplex.com/ please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Test.Properties;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public class TestAxCryptDocument
    {
        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This is test, readability and coding ease is a concern, not performance.")]
        public void TestFileNameFromSimpleFile()
        {
            using (Stream testStream = new MemoryStream(Resources.HelloWorld_Key_a_txt))
            {
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    AxCryptReaderSettings settings = new AxCryptReaderSettings("a");
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream, settings))
                    {
                        document.Load(axCryptReader);
                        string fileName = document.FileName;
                        Assert.That(fileName, Is.EqualTo("HelloWorld-Key-a.txt"), "Wrong file name");
                    }
                }
            }
        }

        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This is test, readability and coding ease is a concern, not performance.")]
        public static void TestUnicodeFileNameFromSimpleFile()
        {
            using (Stream testStream = new MemoryStream(Resources.HelloWorld_Key_a_txt))
            {
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    AxCryptReaderSettings settings = new AxCryptReaderSettings("a");
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream, settings))
                    {
                        document.Load(axCryptReader);
                        string fileName = document.UnicodeFileName;
                        Assert.That(fileName, Is.EqualTo("HelloWorld-Key-a.txt"), "Wrong file name");
                    }
                }
            }
        }

        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This is test, readability and coding ease is a concern, not performance.")]
        public static void TestHmacFromSimpleFile()
        {
            byte[] expectedHmac = new byte[] { 0xF9, 0xAF, 0x2E, 0x67, 0x7D, 0xCF, 0xC9, 0xFE, 0x06, 0x4B, 0x39, 0x08, 0xE7, 0x5A, 0x87, 0x81 };
            using (Stream testStream = new MemoryStream(Resources.HelloWorld_Key_a_txt))
            {
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    AxCryptReaderSettings settings = new AxCryptReaderSettings("a");
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream, settings))
                    {
                        document.Load(axCryptReader);
                        byte[] hmac = document.GetHmac();
                        Assert.That(hmac, Is.EqualTo(expectedHmac), "Wrong HMAC");
                    }
                }
            }
        }

        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This is test, readability and coding ease is a concern, not performance.")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompundWordsShouldBeCasedCorrectly", Justification = "This is a test, and they should start with 'Test'.")]
        public static void TestIsCompressedFromSimpleFile()
        {
            using (Stream testStream = new MemoryStream(Resources.HelloWorld_Key_a_txt))
            {
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    AxCryptReaderSettings settings = new AxCryptReaderSettings("a");
                    using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream, settings))
                    {
                        document.Load(axCryptReader);
                        bool isCompressed = document.IsCompressed;
                        Assert.That(isCompressed, Is.False, "This file should not be compressed.");
                    }
                }
            }
        }
    }
}