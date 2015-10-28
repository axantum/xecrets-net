#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Portable;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Mono;
using Axantum.AxCrypt.Mono.Portable;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Desktop.Test
{
    [TestFixture]
    public static class TestFileWatcher
    {
        private static string _tempPath;

        [SetUp]
        public static void Setup()
        {
            TypeMap.Register.Singleton<IPortableFactory>(() => new PortableFactory());

            _tempPath = Path.Combine(Path.GetTempPath(), @"Axantum.AxCrypt.Mono.Test.TestFileWatcher").NormalizeFolderPath();
            Directory.CreateDirectory(_tempPath);

            TypeMap.Register.New<string, IFileWatcher>((path) => new FileWatcher(path, new DelayedAction(new DelayTimer(), TimeSpan.FromMilliseconds(1))));
            TypeMap.Register.New<string, IDataStore>((path) => new DataStore(path));
            TypeMap.Register.Singleton<IRuntimeEnvironment>(() => new RuntimeEnvironment(".axx"));
            TypeMap.Register.Singleton<ILogging>(() => new Logging());
        }

        [TearDown]
        public static void Teardown()
        {
            TypeMap.Register.Clear();
            Directory.Delete(_tempPath, true);
        }

        [Test]
        public static void TestFileWatcherSimple()
        {
            bool wasHere = false;
            using (IFileWatcher fileWatcher = New<IFileWatcher>(_tempPath))
            {
                fileWatcher.FileChanged += (object sender, FileWatcherEventArgs e) =>
                {
                    wasHere = true;
                };
                IDataStore tempFileInfo = New<IDataStore>(Path.Combine(_tempPath, "AxCryptTestTemp.tmp"));
                try
                {
                    using (Stream stream = tempFileInfo.OpenWrite())
                    {
                    }
                    for (int i = 0; !wasHere && i < 20; ++i)
                    {
                        new Sleep().Time(new TimeSpan(0, 0, 0, 0, 100));
                    }
                    Assert.That(wasHere, "The FileWatcher should have noticed the creation of a file.");
                }
                finally
                {
                    wasHere = false;
                    tempFileInfo.Delete();
                }
                for (int i = 0; !wasHere && i < 20; ++i)
                {
                    new Sleep().Time(new TimeSpan(0, 0, 0, 0, 100));
                }
                Assert.That(wasHere, "The FileWatcher should have noticed the deletion of a file.");
            }
        }

        [Test]
        public static void TestCreated()
        {
            using (IFileWatcher fileWatcher = New<IFileWatcher>(_tempPath))
            {
                string fileName = String.Empty;
                fileWatcher.FileChanged += (object sender, FileWatcherEventArgs e) => { fileName = Path.GetFileName(e.FullNames.First()); };
                using (Stream stream = File.Create(Path.Combine(_tempPath, "CreatedFile.txt")))
                {
                }
                for (int i = 0; String.IsNullOrEmpty(fileName) && i < 20; ++i)
                {
                    new Sleep().Time(new TimeSpan(0, 0, 0, 0, 100));
                }
                Assert.That(fileName, Is.EqualTo("CreatedFile.txt"), "The watcher should detect the newly created file.");
            }
        }

        [Test]
        public static void TestMoved()
        {
            using (IFileWatcher fileWatcher = New<IFileWatcher>(_tempPath))
            {
                List<string> fileNames = null;
                fileWatcher.FileChanged += (object sender, FileWatcherEventArgs e) => { fileNames = e.FullNames.Select(f => Path.GetFileName(f)).ToList(); };
                using (Stream stream = File.Create(Path.Combine(_tempPath, "NewFile.txt")))
                {
                }
                for (int i = 0; fileNames == null && i < 20; ++i)
                {
                    new Sleep().Time(new TimeSpan(0, 0, 0, 0, 100));
                }
                fileNames = null;
                File.Move(Path.Combine(_tempPath, "NewFile.txt"), Path.Combine(_tempPath, "MovedFile.txt"));
                for (int i = 0; fileNames == null && i < 20; ++i)
                {
                    new Sleep().Time(new TimeSpan(0, 0, 0, 0, 100));
                }
                Assert.That(fileNames.Contains("MovedFile.txt"), "The watcher should detect the newly created file.");
                Assert.That(fileNames.Contains("NewFile.txt"), "The watcher should detect the original file.");
                Assert.That(fileNames.Count, Is.EqualTo(2), "There should be exactly two files detectedd.");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times"), Test]
        public static void TestDoubleDispose()
        {
            Assert.DoesNotThrow(() =>
            {
                using (IFileWatcher fileWatcher = New<IFileWatcher>(_tempPath))
                {
                    fileWatcher.Dispose();
                }
            });
        }
    }
}