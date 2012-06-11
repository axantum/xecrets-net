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
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.IO;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestFileLock
    {
        [Test]
        public static void TestFileLockInvalidArguments()
        {
            string nullString = null;
            Assert.Throws<ArgumentNullException>(() => { FileLock.Lock(nullString); });
            Assert.Throws<ArgumentNullException>(() => { FileLock.IsLocked(nullString); });
            Assert.Throws<ArgumentNullException>(() => { FileLock.IsLocked(@"\\server\path\file.ext", nullString); });
        }

        [Test]
        public static void TestFileLockMethods()
        {
            string file1 = @"c:\dir\file.ext";

            Assert.That(FileLock.IsLocked(file1), Is.False, "There should be no lock for this file yet.");
            using (FileLock lock1 = FileLock.Lock(file1))
            {
                Assert.That(FileLock.IsLocked(file1), Is.True, "There should be now be a lock for this file.");
            }
            Assert.That(FileLock.IsLocked(file1), Is.False, "There should be no lock for this file again.");
        }

        [Test]
        public static void TestFileLockWhenLocked()
        {
            string file1 = @"c:\dir\file.ext";
            Assert.That(FileLock.IsLocked(file1), Is.False, "There should be no lock for this file to start with.");
            using (FileLock lock1 = FileLock.Lock(file1))
            {
                Assert.That(FileLock.IsLocked(file1), Is.True, "There should be a lock for this file.");
                using (FileLock lock1a = FileLock.Lock(file1))
                {
                    Assert.That(lock1a, Is.Null, "When trying to get a lock for a locked file, this should return null.");
                    Assert.That(FileLock.IsLocked(file1), Is.True, "There should still be a lock for this file.");
                }
                Assert.That(FileLock.IsLocked(file1), Is.True, "There should still be a lock for this file.");
            }
            Assert.That(FileLock.IsLocked(file1), Is.False, "There should be no lock for this file now.");
        }

        [Test]
        public static void TestFileLockCaseSensitivity()
        {
            string file1 = @"c:\dir\file.ext";
            string file2 = @"C:\dir\file.ext";

            Assert.That(FileLock.IsLocked(file1), Is.False, "There should be no lock for this file yet.");
            Assert.That(FileLock.IsLocked(file2), Is.False, "There should be no lock for this file yet.");
            using (FileLock lock1 = FileLock.Lock(file1))
            {
                Assert.That(FileLock.IsLocked(file1), Is.True, "There should be now be a lock for this file.");
                Assert.That(FileLock.IsLocked(file2), Is.False, "There should be no lock for this file still.");
            }
            Assert.That(FileLock.IsLocked(file1), Is.False, "There should be no lock for this file again.");
        }
    }
}