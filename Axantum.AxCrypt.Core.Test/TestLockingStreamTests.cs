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

using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Test.Properties;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestLockingStreamTests
    {
        private static readonly string _rootPath = Path.GetPathRoot(Environment.CurrentDirectory);
        private static readonly string _davidCopperfieldTxtPath = _rootPath.PathCombine("Users", "AxCrypt", "David Copperfield.txt");
        private static readonly string _uncompressedAxxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Uncompressed.axx");
        private static readonly string _helloWorldAxxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HelloWorld.axx");

        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();

            FakeDataStore.AddFile(_davidCopperfieldTxtPath, FakeDataStore.TestDate4Utc, FakeDataStore.TestDate5Utc, FakeDataStore.TestDate6Utc, FakeDataStore.ExpandableMemoryStream(Encoding.GetEncoding(1252).GetBytes(Resources.david_copperfield)));
            FakeDataStore.AddFile(_uncompressedAxxPath, FakeDataStore.ExpandableMemoryStream(Resources.uncompressable_zip));
            FakeDataStore.AddFile(_helloWorldAxxPath, FakeDataStore.ExpandableMemoryStream(Resources.helloworld_key_a_txt));
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestSimple()
        {
            using (MemoryStream stream = FakeDataStore.ExpandableMemoryStream(Encoding.UTF8.GetBytes("A short dummy stream")))
            {
                IDataStore fileInfo = TypeMap.Resolve.New<IDataStore>(_davidCopperfieldTxtPath);
                using (LockingStream lockingStream = new LockingStream(fileInfo, stream))
                {
                    Assert.That(FileLock.IsLocked(fileInfo), "The file should be locked now.");
                    Assert.That(lockingStream.CanRead, "The stream should be readable.");
                    Assert.That(lockingStream.CanSeek, "The stream should be seekable.");
                    Assert.That(lockingStream.CanWrite, "The stream should be writeable.");
                    Assert.That(lockingStream.Length, Is.EqualTo("A short dummy stream".Length), "The length should be the same as the string.");

                    byte[] b = new byte[1];
                    int read = lockingStream.Read(b, 0, 1);
                    Assert.That(read, Is.EqualTo(1), "There should be one byte read.");
                    Assert.That(b[0], Is.EqualTo(Encoding.UTF8.GetBytes("A")[0]), "The byte read should be an 'A'.");
                    Assert.That(lockingStream.Position, Is.EqualTo(1), "After reading the first byte, the position should be at one.");

                    lockingStream.Write(b, 0, 1);
                    lockingStream.Position = 1;
                    read = lockingStream.Read(b, 0, 1);
                    Assert.That(read, Is.EqualTo(1), "There should be one byte read.");
                    Assert.That(b[0], Is.EqualTo(Encoding.UTF8.GetBytes("A")[0]), "The byte read should be an 'A'.");

                    lockingStream.Seek(-1, SeekOrigin.End);
                    Assert.That(lockingStream.Position, Is.EqualTo(lockingStream.Length - 1), "The position should be set by the Seek().");

                    lockingStream.SetLength(5);
                    lockingStream.Seek(0, SeekOrigin.End);
                    Assert.That(lockingStream.Position, Is.EqualTo(5), "After setting the length to 5, seeking to the end should set the position at 5.");

                    Assert.DoesNotThrow(() => { lockingStream.Flush(); }, "It's hard to test Flush() behavior here, not worth the trouble, but it should not throw!");
                }
                Assert.That(!FileLock.IsLocked(fileInfo), "The file should be unlocked now.");
                Assert.Throws<ObjectDisposedException>(() => { stream.Position = 0; }, "The underlying stream should be disposed.");
            }
        }
    }
}