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
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestFileSystemStateExtensions
    {
        private static IRuntimeEnvironment _environment;
        private static FileSystemState _fileSystemState;
        private static FakeRuntimeEnvironment _fakeRuntimeEnvironment;

        private const string _decryptedFile1 = @"c:\Documents\test.txt";
        private const string _decryptedFile2 = @"c:\Documents\David Copperfield.txt";
        private const string _encryptedFile1 = @"c:\Documents\Uncompressed.axx";
        private const string _encryptedFile2 = @"c:\Documents\HelloWorld.axx";

        [SetUp]
        public static void Setup()
        {
            _environment = AxCryptEnvironment.Current;
            AxCryptEnvironment.Current = _fakeRuntimeEnvironment = new FakeRuntimeEnvironment();
            _fileSystemState = FileSystemState.Load(AxCryptEnvironment.Current.FileInfo(@"C:\Temp\DummyFileSystemState.xml"));
        }

        [TearDown]
        public static void Teardown()
        {
            AxCryptEnvironment.Current = _environment;
            _environment = null;
            _fileSystemState = null;
            FakeRuntimeFileInfo.ClearFiles();
        }

        [Test]
        public static void TestCheckActiveFilesIsNotLocked()
        {
            DateTime utcNow = AxCryptEnvironment.Current.UtcNow;
            DateTime utcYesterday = utcNow.AddDays(-1);
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, new MemoryStream(Encoding.UTF8.GetBytes("This is a short file")));
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcYesterday, utcYesterday, utcYesterday, new MemoryStream(Resources.HelloWorld_Key_a_txt));

            ActiveFile activeFile = new ActiveFile(AxCryptEnvironment.Current.FileInfo(_encryptedFile1), AxCryptEnvironment.Current.FileInfo(_decryptedFile1), new AesKey(), ActiveFileStatus.NotDecrypted, null);
            _fakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(10); });
            bool changedWasRaised = false;
            _fileSystemState.Add(activeFile);
            _fileSystemState.Changed += ((object sender, EventArgs e) =>
            {
                changedWasRaised = true;
            });
            _fileSystemState.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, false, new ProgressContext());
            Assert.That(changedWasRaised, Is.True, "The file should be detected as decrypted being created.");
        }

        [Test]
        public static void TestCheckActiveFilesIsLocked()
        {
            DateTime utcNow = AxCryptEnvironment.Current.UtcNow;
            DateTime utcYesterday = utcNow.AddDays(-1);
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, new MemoryStream(Encoding.UTF8.GetBytes("This is a short file")));
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcYesterday, utcYesterday, utcYesterday, new MemoryStream(Resources.HelloWorld_Key_a_txt));

            ActiveFile activeFile = new ActiveFile(AxCryptEnvironment.Current.FileInfo(_encryptedFile1), AxCryptEnvironment.Current.FileInfo(_decryptedFile1), new AesKey(), ActiveFileStatus.NotDecrypted, null);
            _fakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(10); });
            bool changedWasRaised = false;
            _fileSystemState.Add(activeFile);
            _fileSystemState.Changed += ((object sender, EventArgs e) =>
            {
                changedWasRaised = true;
            });
            using (FileLock fileLock = FileLock.Lock(activeFile.EncryptedFileInfo))
            {
                _fileSystemState.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, false, new ProgressContext());
            }
            Assert.That(changedWasRaised, Is.False, "The file should be not be detected as decrypted being created because the encrypted file is locked.");
            using (FileLock fileLock = FileLock.Lock(activeFile.DecryptedFileInfo))
            {
                _fileSystemState.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, false, new ProgressContext());
            }
            Assert.That(changedWasRaised, Is.False, "The file should be not be detected as decrypted being created because the decrypted file is locked.");
        }

        [Test]
        public static void TestCheckActiveFilesSkipIfTooRecent()
        {
            DateTime utcNow = AxCryptEnvironment.Current.UtcNow;
            DateTime utcYesterday = utcNow.AddDays(-1);
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, new MemoryStream(Encoding.UTF8.GetBytes("This is a short file")));
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcYesterday, utcYesterday, utcYesterday, new MemoryStream(Resources.HelloWorld_Key_a_txt));

            ActiveFile activeFile = new ActiveFile(AxCryptEnvironment.Current.FileInfo(_encryptedFile1), AxCryptEnvironment.Current.FileInfo(_decryptedFile1), new AesKey(), ActiveFileStatus.NotDecrypted, null);
            _fakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddSeconds(1); });
            bool changedWasRaised = false;
            _fileSystemState.Add(activeFile);
            _fileSystemState.Changed += ((object sender, EventArgs e) =>
            {
                changedWasRaised = true;
            });
            _fileSystemState.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, false, new ProgressContext());
            Assert.That(changedWasRaised, Is.False, "The file should not be detected as decrypted being created, because the current time is too close to LastAccessTimeUtc.");
        }
    }
}