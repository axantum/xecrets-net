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
using System.Globalization;
using System.IO;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.System;
using NUnit.Framework;

namespace Axantum.AxCrypt.Mono.Test
{
    [TestFixture]
    public static class TestRuntimeFileInfo
    {
        private static IRuntimeEnvironment _previousEnvironment;

        [SetUp]
        public static void Setup()
        {
            _previousEnvironment = AxCryptEnvironment.Current;
            AxCryptEnvironment.Current = new RuntimeEnvironment();
        }

        [TearDown]
        public static void Teardown()
        {
            AxCryptEnvironment.Current = _previousEnvironment;
        }

        [Test]
        public static void TestBadArguments()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                RuntimeFileInfo rfi = new RuntimeFileInfo(null);

                // Avoid FxCop error
                Object.Equals(rfi, null);
            });
        }

        [Test]
        public static void TestCreateDirectory()
        {
            string testTempFolder = Path.Combine(Path.GetTempPath(), "AxantumTestCreateDirectory" + Path.DirectorySeparatorChar);
            if (Directory.Exists(testTempFolder))
            {
                Directory.Delete(testTempFolder, true);
            }
            Assert.That(Directory.Exists(testTempFolder), Is.False, "The test folder should not exist now.");
            IRuntimeFileInfo directoryInfo = new RuntimeFileInfo(testTempFolder);
            directoryInfo.CreateDirectory();
            Assert.That(Directory.Exists(testTempFolder), Is.True, "The test folder should exist now.");
            if (Directory.Exists(testTempFolder))
            {
                Directory.Delete(testTempFolder, true);
            }
        }

        [Test]
        public static void TestMethods()
        {
            string tempFileName = Path.GetTempFileName();
            IRuntimeFileInfo runtimeFileInfo = new RuntimeFileInfo(tempFileName);
            try
            {
                using (Stream writeStream = runtimeFileInfo.OpenWrite())
                {
                    using (TextWriter writer = new StreamWriter(writeStream))
                    {
                        writer.Write("This is AxCrypt!");
                    }
                }
                using (Stream readStream = runtimeFileInfo.OpenRead())
                {
                    using (TextReader reader = new StreamReader(readStream))
                    {
                        string text = reader.ReadToEnd();

                        Assert.That(text, Is.EqualTo("This is AxCrypt!"), "What was written should be read.");
                    }
                }

                DateTime dateTime = DateTime.Parse("2012-02-29 12:00:00", CultureInfo.GetCultureInfo("sv-SE"), DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                runtimeFileInfo.SetFileTimes(dateTime, dateTime + new TimeSpan(1, 0, 0), dateTime + new TimeSpan(2, 0, 0));
                Assert.That(runtimeFileInfo.CreationTimeUtc, Is.EqualTo(dateTime), "The creation time should be as set.");
                Assert.That(runtimeFileInfo.LastAccessTimeUtc, Is.EqualTo(dateTime + new TimeSpan(1, 0, 0)), "The last access time should be as set.");
                Assert.That(runtimeFileInfo.LastWriteTimeUtc, Is.EqualTo(dateTime + new TimeSpan(2, 0, 0)), "The last write time should be as set.");

                Assert.That(runtimeFileInfo.FullName, Is.EqualTo(tempFileName), "The FullName should be the same as the underlying FileInfo.FullName.");

                string otherTempFileName = runtimeFileInfo.FullName + ".copy";
                IRuntimeFileInfo otherTempRuntimeFileInfo = new RuntimeFileInfo(otherTempFileName);
                Assert.That(otherTempRuntimeFileInfo.Exists, Is.False, "The new temp file should not exist.");
                Assert.That(runtimeFileInfo.Exists, Is.True, "The old temp file should exist.");
                runtimeFileInfo.MoveTo(otherTempRuntimeFileInfo.FullName);
                Assert.That(otherTempRuntimeFileInfo.Exists, Is.True, "The new temp file should exist after moving the old here.");
                Assert.That(runtimeFileInfo.Exists, Is.True, "The old temp file should exist still because it has changed to refer to the new file.");
            }
            finally
            {
                runtimeFileInfo.Delete();
            }
            Assert.That(runtimeFileInfo.Exists, Is.False, "The file should have been deleted now.");

            IRuntimeFileInfo notEncryptedRuntimeFileInfo = new RuntimeFileInfo("file.txt");
            IRuntimeFileInfo encryptedRuntimeFileInfo = notEncryptedRuntimeFileInfo.CreateEncryptedName();
            Assert.That(encryptedRuntimeFileInfo.Name, Is.EqualTo("file-txt.axx"), "The encrypted name should be as expected.");
        }
    }
}