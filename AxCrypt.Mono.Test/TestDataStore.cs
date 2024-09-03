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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using AxCrypt.Core;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.IO;
using AxCrypt.Core.Portable;
using AxCrypt.Core.Runtime;
using AxCrypt.Fake;
using AxCrypt.Mono.Portable;
using NUnit.Framework;
using System;
using System.Globalization;
using System.IO;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Mono.Test
{
    [TestFixture]
    public static class TestDataStore
    {
        private static string _tempPath = null!;

        [SetUp]
        public static void Setup()
        {
            _tempPath = Path.Combine(Path.GetTempPath(), "AxCrypt.Mono.Test.TestDataStore");
            _ = Directory.CreateDirectory(_tempPath);

            TypeMap.Register.Singleton<INow>(() => new FakeNow());
            TypeMap.Register.Singleton<IReport>(() => new FakeReport());
            TypeMap.Register.New<string, IDataStore>((path) => new DataStore(path));
            TypeMap.Register.Singleton<IRuntimeEnvironment>(() => new RuntimeEnvironment(".axx"));
            TypeMap.Register.Singleton<IPlatform>(() => new MonoPlatform());
            TypeMap.Register.Singleton<IPortableFactory>(() => new PortableFactory());
            TypeMap.Register.Singleton<WorkFolder>(() => new WorkFolder(_tempPath));
            TypeMap.Register.Singleton<ILogging>(() => new Logging());
        }

        [TearDown]
        public static void Teardown()
        {
            TypeMap.Register.Clear();
            Directory.Delete(_tempPath, true);
        }

        [Test]
        public static void TestDataStoreNullArgument()
        {
            _ = Assert.Throws<ArgumentNullException>(() =>
            {
                DataStore ds = new DataStore(null!);
            });
        }

        [Test]
        public static void TestDataStoreInvalidPath()
        {
            if (New<IPlatform>().Platform == Platform.MacOsx)
            {
                Assert.That(true, Is.True, "Mac OS X File System allows all characters, so this test makes no sense there.");
                return;
            }

            Assert.DoesNotThrow(() =>
            {
                string goodFileName = Path.Combine(Path.GetTempPath(), "GoodName.txt");
                DataStore ds = new DataStore(goodFileName);
                try
                {
                    using (ds.OpenWrite())
                    {
                    }
                }
                finally
                {
                    ds.Delete();
                }
            });

            _ = Assert.Throws<IOException>(() =>
            {

                string badFileName = Path.Combine(Path.GetTempPath(), "A?bad*filename.txt");
                DataStore ds = new DataStore(badFileName);
                try
                {
                    using (ds.OpenWrite())
                    {
                    }
                }
                finally
                {
                    ds.Delete();
                }
            });
        }

        [Test]
        public static void TestDataStoreCreateDirectory()
        {
            string testTempFolder = Path.Combine(Path.GetTempPath(), "AxantumTestCreateDirectory" + Path.DirectorySeparatorChar);
            if (Directory.Exists(testTempFolder))
            {
                Directory.Delete(testTempFolder, true);
            }
            Assert.That(Directory.Exists(testTempFolder), Is.False, "The test folder should not exist now.");
            DataContainer directoryInfo = new DataContainer(testTempFolder);
            directoryInfo.CreateFolder();
            Assert.That(Directory.Exists(testTempFolder), Is.True, "The test folder should exist now.");
            if (Directory.Exists(testTempFolder))
            {
                Directory.Delete(testTempFolder, true);
            }
        }

        [Test]
        public static void TestDataStoreMethods()
        {
            string tempFileName = Path.GetTempFileName();
            IDataStore ds = new DataStore(tempFileName);
            try
            {
                using (Stream writeStream = ds.OpenWrite())
                {
                    using TextWriter writer = new StreamWriter(writeStream);
                    writer.Write("This is AxCrypt!");
                }
                using (Stream readStream = ds.OpenRead())
                {
                    using TextReader reader = new StreamReader(readStream);
                    string text = reader.ReadToEnd();

                    Assert.That(text, Is.EqualTo("This is AxCrypt!"), "What was written should be read.");
                }

                DateTime dateTime = DateTime.Parse("2012-02-29 12:00:00", CultureInfo.GetCultureInfo("sv-SE"), DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                ds.SetFileTimes(dateTime, dateTime + new TimeSpan(3, 0, 0), dateTime + new TimeSpan(5, 0, 0));
                // if (OS.Current.Platform == Platform.WindowsDesktop)
                // {
                     Assert.That(ds.CreationTimeUtc, Is.EqualTo(dateTime), "The creation time should be as set.");
                // }
                // else
                // {
                //     Assert.That(ds.CreationTimeUtc, Is.EqualTo(dateTime + new TimeSpan(5, 0, 0)), "The creation time should be as last write time due to bug in Mono.");
                // }
                Assert.That(ds.LastAccessTimeUtc, Is.EqualTo(dateTime + new TimeSpan(3, 0, 0)), "The last access time should be as set.");
                Assert.That(ds.LastWriteTimeUtc, Is.EqualTo(dateTime + new TimeSpan(5, 0, 0)), "The last write time should be as set.");

                Assert.That(ds.FullName, Is.EqualTo(tempFileName), "The FullName should be the same as the underlying FileInfo.FullName.");

                string otherTempFileName = ds.FullName + ".copy";
                IDataStore otherTempDataStore = new DataStore(otherTempFileName);
                Assert.That(otherTempDataStore.IsAvailable, Is.False, "The new temp file should not exist.");
                Assert.That(ds.IsAvailable, Is.True, "The old temp file should exist.");
                ds.MoveTo(otherTempDataStore.FullName);
                Assert.That(otherTempDataStore.IsAvailable, Is.True, "The new temp file should exist after moving the old here.");
                Assert.That(ds.IsAvailable, Is.True, "The old temp file should exist still because it has changed to refer to the new file.");
            }
            finally
            {
                ds.Delete();
            }
            Assert.That(ds.IsAvailable, Is.False, "The file should have been deleted now.");

            IDataStore notEncryptedDataStore = new DataStore("file.txt");
            IDataStore encryptedDataStore = notEncryptedDataStore.CreateEncryptedName();
            Assert.That(encryptedDataStore.Name, Is.EqualTo("file-txt.axx"), "The encrypted name should be as expected.");
        }
    }
}
