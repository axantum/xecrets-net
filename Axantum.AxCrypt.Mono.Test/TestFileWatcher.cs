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

using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.Mono.Test
{
    [TestFixture]
    public static class TestFileWatcher
    {
        private static string _tempPath;

        [SetUp]
        public static void Setup()
        {
            FactoryRegistry.Instance.Singleton((IRuntimeEnvironment)new RuntimeEnvironment());
            FactoryRegistry.Instance.Singleton<ISleep>(new Sleep());
            _tempPath = Path.Combine(Path.GetTempPath(), "Axantum.AxCrypt.Core.Test.TestFileWatcher");
            Directory.CreateDirectory(_tempPath);
        }

        [TearDown]
        public static void Teardown()
        {
            Directory.Delete(_tempPath, true);
            FactoryRegistry.Instance.Clear();
        }

        [Test]
        public static void TestCreated()
        {
            using (IFileWatcher fileWatcher = OS.Current.CreateFileWatcher(_tempPath))
            {
                string fileName = String.Empty;
                fileWatcher.FileChanged += (object sender, FileWatcherEventArgs e) => { fileName = Path.GetFileName(e.FullName); };
                using (Stream stream = File.Create(Path.Combine(_tempPath, "CreatedFile.txt")))
                {
                }
                for (int i = 0; String.IsNullOrEmpty(fileName) && i < 20; ++i)
                {
                    Instance.Sleep.Time(new TimeSpan(0, 0, 0, 0, 100));
                }
                Assert.That(fileName, Is.EqualTo("CreatedFile.txt"), "The watcher should detect the newly created file.");
            }
        }

        [Test]
        public static void TestMoved()
        {
            using (IFileWatcher fileWatcher = OS.Current.CreateFileWatcher(_tempPath))
            {
                string fileName = String.Empty;
                fileWatcher.FileChanged += (object sender, FileWatcherEventArgs e) => { fileName = Path.GetFileName(e.FullName); };
                using (Stream stream = File.Create(Path.Combine(_tempPath, "NewFile.txt")))
                {
                }
                for (int i = 0; String.IsNullOrEmpty(fileName) && i < 20; ++i)
                {
                    Instance.Sleep.Time(new TimeSpan(0, 0, 0, 0, 100));
                }
                fileName = String.Empty;
                File.Move(Path.Combine(_tempPath, "NewFile.txt"), Path.Combine(_tempPath, "MovedFile.txt"));
                for (int i = 0; String.IsNullOrEmpty(fileName) && i < 20; ++i)
                {
                    Instance.Sleep.Time(new TimeSpan(0, 0, 0, 0, 100));
                }
                Assert.That(fileName, Is.EqualTo("MovedFile.txt"), "The watcher should detect the newly created file.");
            }
        }

        [Test]
        public static void TestDoubleDispose()
        {
            Assert.DoesNotThrow(() =>
            {
                using (IFileWatcher fileWatcher = OS.Current.CreateFileWatcher(_tempPath))
                {
                    fileWatcher.Dispose();
                }
            });
        }
    }
}