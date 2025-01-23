#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions Copyright © 2022-2025, Svante Seleborg, All Rights Reserved.
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

using AxCrypt.Core.IO;
using AxCrypt.Core.UI;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestProgressStreamTest
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times"), Test]
        public static void TestPropertiesAndMethods()
        {
            using (MemoryStream memoryStream = FakeDataStore.ExpandableMemoryStream(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }))
            {
                string kilroy = String.Empty;
                using (FakeStream testStream = new FakeStream(memoryStream, (string wasHere) => { kilroy += wasHere; }))
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times"), Test]
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
