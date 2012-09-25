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
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.System;
using Axantum.AxCrypt.Core.Test.Properties;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestActiveFileCollectionTests
    {
        private static IRuntimeEnvironment _environment;

        private static readonly string _rootPath = Path.GetPathRoot(Environment.SystemDirectory);
        private static readonly string _testTextPath = Path.Combine(_rootPath, "test.txt");
        private static readonly string _davidCopperfieldTxtPath = _rootPath.PathCombine("Users", "AxCrypt", "David Copperfield.txt");
        private static readonly string _uncompressedAxxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Uncompressed.axx");
        private static readonly string _helloWorldAxxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HelloWorld.axx");

        [TestFixtureSetUp]
        public static void SetupFixture()
        {
            _environment = AxCryptEnvironment.Current;
            AxCryptEnvironment.Current = new FakeRuntimeEnvironment();

            FakeRuntimeFileInfo.AddFile(_testTextPath, FakeRuntimeFileInfo.TestDate1Utc, FakeRuntimeFileInfo.TestDate2Utc, FakeRuntimeFileInfo.TestDate1Utc, new MemoryStream(Encoding.UTF8.GetBytes("This is a short file")));
            FakeRuntimeFileInfo.AddFile(_davidCopperfieldTxtPath, FakeRuntimeFileInfo.TestDate4Utc, FakeRuntimeFileInfo.TestDate5Utc, FakeRuntimeFileInfo.TestDate6Utc, new MemoryStream(Encoding.GetEncoding(1252).GetBytes(Resources.David_Copperfield)));
            FakeRuntimeFileInfo.AddFile(_uncompressedAxxPath, new MemoryStream(Resources.Uncompressable_zip));
            FakeRuntimeFileInfo.AddFile(_helloWorldAxxPath, new MemoryStream(Resources.HelloWorld_Key_a_txt));
        }

        [TestFixtureTearDown]
        public static void TeardownFixture()
        {
            AxCryptEnvironment.Current = _environment;
            FakeRuntimeFileInfo.ClearFiles();
        }

        [Test]
        public static void TestActiveFileCollectionSimpleConstructor()
        {
            ActiveFileCollection collection = new ActiveFileCollection();

            IRuntimeFileInfo decryptedFileInfo = AxCryptEnvironment.Current.FileInfo(_testTextPath);
            IRuntimeFileInfo encryptedFileInfo = AxCryptEnvironment.Current.FileInfo(_helloWorldAxxPath);
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, new AesKey(), ActiveFileStatus.None, null);

            collection.Add(activeFile);

            Assert.That(collection.Count, Is.EqualTo(1), "There should be one entry in the collection.");

            foreach (ActiveFile member in collection)
            {
                Assert.That(member, Is.EqualTo(activeFile), "The one we added should now be in the collection.");
            }
        }

        [Test]
        public static void TestActiveFileCollectionEnumerationConstructor()
        {
            IRuntimeFileInfo decryptedFileInfo1 = AxCryptEnvironment.Current.FileInfo(Path.Combine(_rootPath, "test1.txt"));
            IRuntimeFileInfo encryptedFileInfo1 = AxCryptEnvironment.Current.FileInfo(Path.Combine(_rootPath, "test1-txt.axx"));
            IRuntimeFileInfo decryptedFileInfo2 = AxCryptEnvironment.Current.FileInfo(Path.Combine(_rootPath, "test2.txt"));
            IRuntimeFileInfo encryptedFileInfo2 = AxCryptEnvironment.Current.FileInfo(Path.Combine(_rootPath, "test2-text.axx"));
            ActiveFile activeFile1 = new ActiveFile(encryptedFileInfo1, decryptedFileInfo1, new AesKey(), ActiveFileStatus.None, null);
            ActiveFile activeFile2 = new ActiveFile(encryptedFileInfo2, decryptedFileInfo2, new AesKey(), ActiveFileStatus.None, null);

            ActiveFileCollection collection = new ActiveFileCollection(new ActiveFile[] { activeFile1, activeFile2 });

            Assert.That(collection.Count, Is.EqualTo(2), "There should be two entries in the collection.");

            Assert.That(collection.First(), Is.EqualTo(activeFile1), "This should be the first in the collection.");
            Assert.That(collection.Last(), Is.EqualTo(activeFile2), "This should be the last in the collection.");
        }
    }
}