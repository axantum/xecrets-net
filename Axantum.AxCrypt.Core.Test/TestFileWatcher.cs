using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Axantum.AxCrypt.Core.IO;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestFileWatcher
    {
        private static string _tempPath;

        [SetUp]
        public static void SetUp()
        {
            _tempPath = Path.Combine(Path.GetTempPath(), "Axantum.AxCrypt.Core.Test.TestFileWatcher");
            Directory.CreateDirectory(_tempPath);
        }

        [TearDown]
        public static void TearDown()
        {
            Directory.Delete(_tempPath, true);
        }

        [Test]
        public static void TestCreated()
        {
            using (IFileWatcher fileWatcher = AxCryptEnvironment.Current.FileWatcher(_tempPath))
            {
                string fileName = String.Empty;
                fileWatcher.FileChanged += (object sender, FileWatcherEventArgs e) => { fileName = Path.GetFileName(e.FullName); };
                using (Stream stream = File.Create(Path.Combine(_tempPath, "CreatedFile.txt")))
                {
                }
                Thread.Sleep(100);
                Assert.That(fileName, Is.EqualTo("CreatedFile.txt"), "The watcher should detect the newly created file.");
            }
        }

        [Test]
        public static void TestMoved()
        {
            using (IFileWatcher fileWatcher = AxCryptEnvironment.Current.FileWatcher(_tempPath))
            {
                string fileName = String.Empty;
                fileWatcher.FileChanged += (object sender, FileWatcherEventArgs e) => { fileName = Path.GetFileName(e.FullName); };
                using (Stream stream = File.Create(Path.Combine(_tempPath, "NewFile.txt")))
                {
                }
                Thread.Sleep(100);
                fileName = String.Empty;
                File.Move(Path.Combine(_tempPath, "NewFile.txt"), Path.Combine(_tempPath, "MovedFile.txt"));
                Thread.Sleep(100);
                Assert.That(fileName, Is.EqualTo("MovedFile.txt"), "The watcher should detect the newly created file.");
            }
        }

        [Test]
        public static void TestDoubleDispose()
        {
            Assert.DoesNotThrow(() =>
            {
                using (IFileWatcher fileWatcher = AxCryptEnvironment.Current.FileWatcher(_tempPath))
                {
                    fileWatcher.Dispose();
                }
            });
        }
    }
}