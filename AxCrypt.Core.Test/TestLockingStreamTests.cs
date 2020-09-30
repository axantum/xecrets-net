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

using AxCrypt.Abstractions;
using AxCrypt.Core.IO;
using AxCrypt.Core.Test.Properties;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Test
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
        public static async Task TestSimple()
        {
            IDataStore fileInfo = New<IDataStore>(_davidCopperfieldTxtPath);
            LockedStream lockedStreamCopy;
            using (LockedStream lockedStream = LockedStream.OpenWrite(fileInfo))
            {
                lockedStreamCopy = lockedStream;
                Assert.That(await Task.Run(() => New<FileLocker>().IsLocked(fileInfo)), "The file should be locked now.");
                Assert.That(lockedStream.CanRead, "The stream should be readable.");
                Assert.That(lockedStream.CanSeek, "The stream should be seekable.");
                Assert.That(lockedStream.CanWrite, "The stream should be writeable.");
                Assert.That(lockedStream.Length, Is.EqualTo(Resources.david_copperfield.Length), "The length should be the same as the string.");

                byte[] b = new byte[1];
                int read = lockedStream.Read(b, 0, 1);
                Assert.That(read, Is.EqualTo(1), "There should be one byte read.");
                Assert.That(b[0], Is.EqualTo(Encoding.UTF8.GetBytes(Resources.david_copperfield.Substring(0, 1))[0]), "The byte read should be the first character of the resource.");
                Assert.That(lockedStream.Position, Is.EqualTo(1), "After reading the first byte, the position should be at one.");

                lockedStream.Write(b, 0, 1);
                lockedStream.Position = 1;
                read = lockedStream.Read(b, 0, 1);
                Assert.That(read, Is.EqualTo(1), "There should be one byte read.");
                Assert.That(b[0], Is.EqualTo(Encoding.UTF8.GetBytes(Resources.david_copperfield.Substring(0, 1))[0]), "The byte read should be the first character of the resource.");
                Assert.That(lockedStream.Position, Is.EqualTo(2), "After reading the second byte, the position should be at two.");

                lockedStream.Seek(-1, SeekOrigin.End);
                Assert.That(lockedStream.Position, Is.EqualTo(lockedStream.Length - 1), "The position should be set by the Seek().");

                lockedStream.SetLength(5);
                lockedStream.Seek(0, SeekOrigin.End);
                Assert.That(lockedStream.Position, Is.EqualTo(5), "After setting the length to 5, seeking to the end should set the position at 5.");

                Assert.DoesNotThrow(() => { lockedStream.Flush(); }, "It's hard to test Flush() behavior here, not worth the trouble, but it should not throw!");
            }
            Assert.That(!await Task.Run(() => New<FileLocker>().IsLocked(fileInfo)), "The file should be unlocked now.");
            Assert.Throws<ObjectDisposedException>(() => { lockedStreamCopy.Position = 0; }, "The underlying stream should be disposed.");
        }
    }
}