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

        [TestFixtureSetUp]
        public static void SetupFixture()
        {
            _environment = AxCryptEnvironment.Current;
            AxCryptEnvironment.Current = new FakeRuntimeEnvironment();

            FakeRuntimeFileInfo.AddFile(@"c:\test.txt", FakeRuntimeFileInfo.TestDate1Utc, FakeRuntimeFileInfo.TestDate2Utc, FakeRuntimeFileInfo.TestDate1Utc, new MemoryStream(Encoding.UTF8.GetBytes("This is a short file")));
            FakeRuntimeFileInfo.AddFile(@"c:\Users\AxCrypt\David Copperfield.txt", FakeRuntimeFileInfo.TestDate4Utc, FakeRuntimeFileInfo.TestDate5Utc, FakeRuntimeFileInfo.TestDate6Utc, new MemoryStream(Resources.David_Copperfield));
            FakeRuntimeFileInfo.AddFile(@"c:\Documents\Uncompressed.axx", new MemoryStream(Resources.Uncompressable_zip));
            FakeRuntimeFileInfo.AddFile(@"c:\Documents\HelloWorld.axx", new MemoryStream(Resources.HelloWorld_Key_a_txt));
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

            IRuntimeFileInfo decryptedFileInfo = AxCryptEnvironment.Current.FileInfo(@"c:\test.txt");
            IRuntimeFileInfo encryptedFileInfo = AxCryptEnvironment.Current.FileInfo(@"c:\Documents\HelloWorld.axx");
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
            IRuntimeFileInfo decryptedFileInfo1 = AxCryptEnvironment.Current.FileInfo(@"c:\test1.txt");
            IRuntimeFileInfo encryptedFileInfo1 = AxCryptEnvironment.Current.FileInfo(@"c:\test1-txt.axx");
            IRuntimeFileInfo decryptedFileInfo2 = AxCryptEnvironment.Current.FileInfo(@"c:\test2.txt");
            IRuntimeFileInfo encryptedFileInfo2 = AxCryptEnvironment.Current.FileInfo(@"c:\test2-text.axx");
            ActiveFile activeFile1 = new ActiveFile(encryptedFileInfo1, decryptedFileInfo1, new AesKey(), ActiveFileStatus.None, null);
            ActiveFile activeFile2 = new ActiveFile(encryptedFileInfo2, decryptedFileInfo2, new AesKey(), ActiveFileStatus.None, null);

            ActiveFileCollection collection = new ActiveFileCollection(new ActiveFile[] { activeFile1, activeFile2 });

            Assert.That(collection.Count, Is.EqualTo(2), "There should be two entries in the collection.");

            Assert.That(collection.First(), Is.EqualTo(activeFile1), "This should be the first in the collection.");
            Assert.That(collection.Last(), Is.EqualTo(activeFile2), "This should be the last in the collection.");
        }
    }
}