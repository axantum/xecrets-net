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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.IO;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestFileLock
    {
        private static readonly string _fileExtPath = Path.Combine(Path.GetPathRoot(Environment.CurrentDirectory), "file.ext");

        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestFileLockInvalidArguments()
        {
            IDataStore nullInfo = null;
            Assert.Throws<ArgumentNullException>(() => { FileLock.Lock(nullInfo); });
            Assert.Throws<ArgumentNullException>(() => { FileLock.IsLocked(nullInfo); });
            Assert.Throws<ArgumentNullException>(() => { FileLock.IsLocked(New<IDataStore>(_fileExtPath), nullInfo); });
        }

        [Test]
        public static void TestFileLockMethods()
        {
            IDataStore fileInfo = New<IDataStore>(_fileExtPath);

            Assert.That(Task.Run(() => FileLock.IsLocked(fileInfo)).Result, Is.False, "There should be no lock for this file yet.");
            using (FileLock lock1 = FileLock.Lock(fileInfo))
            {
                Assert.That(Task.Run(() => FileLock.IsLocked(fileInfo)).Result, Is.True, "There should be now be a lock for this file.");
            }
            Assert.That(Task.Run(() => FileLock.IsLocked(fileInfo)).Result, Is.False, "There should be no lock for this file again.");
        }

        [Test]
        public static void TestFileLockWhenLocked()
        {
            IDataStore fileInfo = New<IDataStore>(_fileExtPath);
            Assert.That(Task.Run(() => FileLock.IsLocked(fileInfo)).Result, Is.False, "There should be no lock for this file to start with.");
            using (FileLock lock1 = FileLock.Lock(fileInfo))
            {
                Assert.That(Task.Run(() => FileLock.IsLocked(fileInfo)).Result, Is.True, "There should be a lock for this file.");
            }
            Assert.That(Task.Run(() => FileLock.IsLocked(fileInfo)).Result, Is.False, "There should be no lock for this file now.");
        }

        [Test]
        public static void TestFileLockWhenLockedOnSameThread()
        {
            IDataStore fileInfo = New<IDataStore>(_fileExtPath);
            Assert.That(FileLock.IsLocked(fileInfo), Is.False, "There should be no lock for this file to start with.");
            using (FileLock lock1 = FileLock.Lock(fileInfo))
            {
                Assert.That(Task.Run(() => FileLock.IsLocked(fileInfo)).Result, Is.True, "There should be a lock for this from a different thread.");
                Assert.That(FileLock.IsLocked(fileInfo), Is.False, "There should be no lock for this from the same thread.");
            }
            Assert.That(Task.Run(() => FileLock.IsLocked(fileInfo)).Result, Is.False, "There should be no lock for this file now.");
            Assert.That(FileLock.IsLocked(fileInfo), Is.False, "There should be no lock for this file now.");
        }

        [Test]
        public static void TestMultipleFileLockOnSameThread()
        {
            IDataStore fileInfo = New<IDataStore>(_fileExtPath);
            Assert.That(FileLock.IsLocked(fileInfo), Is.False, "There should be no lock for this file to start with.");
            using (FileLock lock1 = FileLock.Lock(fileInfo))
            {
                Assert.That(Task.Run(() => FileLock.IsLocked(fileInfo)).Result, Is.True, "There should be a lock for this from a different thread.");
                Assert.That(FileLock.IsLocked(fileInfo), Is.False, "There should be no lock for this from the same thread.");
                using (FileLock lock2 = FileLock.Lock(fileInfo))
                {
                    Assert.That(Task.Run(() => FileLock.IsLocked(fileInfo)).Result, Is.True, "There should be a lock for this from a different thread.");
                    Assert.That(FileLock.IsLocked(fileInfo), Is.False, "There should be no lock for this from the same thread.");
                }
            }
            Assert.That(Task.Run(() => FileLock.IsLocked(fileInfo)).Result, Is.False, "There should be no lock for this file now.");
            Assert.That(FileLock.IsLocked(fileInfo), Is.False, "There should be no lock for this file now.");
        }

        [Test]
        public static void TestFileLockCaseSensitivity()
        {
            IDataStore fileInfo1 = New<IDataStore>(_fileExtPath);
            IDataStore fileInfo2 = New<IDataStore>(_fileExtPath.ToUpper(CultureInfo.InvariantCulture));

            Assert.That(FileLock.IsLocked(fileInfo1), Is.False, "There should be no lock for this file yet.");
            Assert.That(FileLock.IsLocked(fileInfo2), Is.False, "There should be no lock for this file yet.");
            using (FileLock lock1 = FileLock.Lock(fileInfo1))
            {
                Assert.That(Task.Run(() => FileLock.IsLocked(fileInfo1)).Result, Is.True, "There should be now be a lock for this file.");
                Assert.That(Task.Run(() => FileLock.IsLocked(fileInfo2)).Result, Is.False, "There should be no lock for this file still.");
            }
            Assert.That(FileLock.IsLocked(fileInfo1), Is.False, "There should be no lock for this file again.");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times"), Test]
        public static void TestFileLockDoubleDispose()
        {
            Assert.DoesNotThrow(() =>
            {
                using (FileLock aLock = FileLock.Lock(New<IDataStore>(_fileExtPath)))
                {
                    aLock.Dispose();
                }
            });
        }
    }
}