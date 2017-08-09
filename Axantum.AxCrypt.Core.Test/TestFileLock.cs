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
using Axantum.AxCrypt.Core.Runtime;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
            Assert.Throws<ArgumentNullException>(() => { New<FileLocker>().Acquire(nullInfo); });
            Assert.Throws<ArgumentNullException>(() => { New<FileLocker>().IsLocked(nullInfo); });
            Assert.Throws<ArgumentNullException>(() => { New<FileLocker>().IsLocked(New<IDataStore>(_fileExtPath), nullInfo); });
        }

        [Test]
        public static async Task TestFileLockMethods()
        {
            IDataStore fileInfo = New<IDataStore>(_fileExtPath);

            Assert.That(await Task.Run(() => New<FileLocker>().IsLocked(fileInfo)), Is.False, "There should be no lock for this file yet.");
            using (FileLock lock1 = New<FileLocker>().Acquire(fileInfo))
            {
                Assert.That(await Task.Run(() => New<FileLocker>().IsLocked(fileInfo)), Is.True, "There should be now be a lock for this file.");
            }
            Assert.That(await Task.Run(() => New<FileLocker>().IsLocked(fileInfo)), Is.False, "There should be no lock for this file again.");
        }

        [Test]
        public static async Task TestFileLockWhenLocked()
        {
            IDataStore fileInfo = New<IDataStore>(_fileExtPath);
            Assert.That(await Task.Run(() => New<FileLocker>().IsLocked(fileInfo)), Is.False, "There should be no lock for this file to start with.");
            using (FileLock lock1 = New<FileLocker>().Acquire(fileInfo))
            {
                Assert.That(await Task.Run(() => New<FileLocker>().IsLocked(fileInfo)), Is.True, "There should be a lock for this file.");
            }
            Assert.That(await Task.Run(() => New<FileLocker>().IsLocked(fileInfo)), Is.False, "There should be no lock for this file now.");
        }

        [Test]
        public static async Task TestFileLockWhenLockedOnSameThread()
        {
            IDataStore fileInfo = New<IDataStore>(_fileExtPath);
            Assert.That(New<FileLocker>().IsLocked(fileInfo), Is.False, "There should be no lock for this file to start with.");
            using (FileLock lock1 = New<FileLocker>().Acquire(fileInfo))
            {
                Assert.That(await Task.Run(() => New<FileLocker>().IsLocked(fileInfo)), Is.True, "There should be a lock for this from a different thread.");
                Assert.That(New<FileLocker>().IsLocked(fileInfo), Is.True, "There should still be a lock for this from the same thread.");
            }
            Assert.That(await Task.Run(() => New<FileLocker>().IsLocked(fileInfo)), Is.False, "There should be no lock for this file now.");
            Assert.That(New<FileLocker>().IsLocked(fileInfo), Is.False, "There should be no lock for this file now.");
        }

        [Test]
        public static async Task TestMultipleFileLockOnSameThread()
        {
            IDataStore fileInfo = New<IDataStore>(_fileExtPath);
            Assert.That(New<FileLocker>().IsLocked(fileInfo), Is.False, "There should be no lock for this file to start with.");
            using (FileLock lock1 = New<FileLocker>().Acquire(fileInfo, TimeSpan.Zero))
            {
                Assert.That(await Task.Run(() => New<FileLocker>().IsLocked(fileInfo)), Is.True, "There should be a lock for this from a different thread (1).");
                Assert.That(New<FileLocker>().IsLocked(fileInfo), Is.True, "There should still be a lock for this from the same thread (1).");
                Assert.Throws<InternalErrorException>(() =>
                {
                    bool wasHere = false;
                    using (FileLock lock2 = New<FileLocker>().Acquire(fileInfo, TimeSpan.Zero))
                    {
                        wasHere = true;
                    }
                    Assert.That(wasHere, Is.False, "This point should never be reached! The Acquire() should throw an exception since this is a deadlock situation.");
                });
            }
            Assert.That(await Task.Run(() => New<FileLocker>().IsLocked(fileInfo)), Is.False, "There should be no lock for this file now.");
            Assert.That(New<FileLocker>().IsLocked(fileInfo), Is.False, "There should be no lock for this file now.");
        }

        [Test]
        public static async Task TestFileLockCaseSensitivity()
        {
            IDataStore fileInfo1 = New<IDataStore>(_fileExtPath);
            IDataStore fileInfo2 = New<IDataStore>(_fileExtPath.ToUpper(CultureInfo.InvariantCulture));

            Assert.That(New<FileLocker>().IsLocked(fileInfo1), Is.False, "There should be no lock for this file yet.");
            Assert.That(New<FileLocker>().IsLocked(fileInfo2), Is.False, "There should be no lock for this file yet.");
            using (FileLock lock1 = New<FileLocker>().Acquire(fileInfo1))
            {
                Assert.That(await Task.Run(() => New<FileLocker>().IsLocked(fileInfo1)), Is.True, "There should be now be a lock for this file.");
                Assert.That(await Task.Run(() => New<FileLocker>().IsLocked(fileInfo2)), Is.False, "There should be no lock for this file still.");
            }
            Assert.That(New<FileLocker>().IsLocked(fileInfo1), Is.False, "There should be no lock for this file again.");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times"), Test]
        public static void TestFileLockDoubleDispose()
        {
            Assert.DoesNotThrow(() =>
            {
                using (FileLock aLock = New<FileLocker>().Acquire(New<IDataStore>(_fileExtPath)))
                {
                    aLock.Dispose();
                }
            });
        }
    }
}