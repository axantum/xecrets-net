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
using Axantum.AxCrypt.Core.IO;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestRuntimeFileInfo
    {
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
        public static void TestMethods()
        {
            string tempFileName = Path.GetTempFileName();
            try
            {
                FileInfo tempFileInfo = new FileInfo(tempFileName);
                IRuntimeFileInfo runtimeFileInfo = new RuntimeFileInfo(tempFileInfo);

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
            }
            finally
            {
                File.Delete(tempFileName);
            }

            FileInfo notEncryptedFileInfo = new FileInfo("file.txt");
            IRuntimeFileInfo notEncryptedRuntimeFileInfo = new RuntimeFileInfo(notEncryptedFileInfo);
            IRuntimeFileInfo encryptedRuntimeFileInfo = notEncryptedRuntimeFileInfo.CreateEncryptedName();
            Assert.That(encryptedRuntimeFileInfo.Name, Is.EqualTo("file-txt.axx"), "The encrypted name should be as expected.");
        }
    }
}