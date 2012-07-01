using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private const string _encryptedFile1 = @"c:\Documents\Uncompressed.axx";
        private const string _fileSystemStateFilePath = @"C:\Temp\DummyFileSystemState.xml";

        [SetUp]
        public static void Setup()
        {
            _environment = AxCryptEnvironment.Current;
            AxCryptEnvironment.Current = _fakeRuntimeEnvironment = new FakeRuntimeEnvironment();
            _fileSystemState = FileSystemState.Load(AxCryptEnvironment.Current.FileInfo(_fileSystemStateFilePath));
        }

        [TearDown]
        public static void Teardown()
        {
            AxCryptEnvironment.Current = _environment;
            _environment = null;
            _fakeRuntimeEnvironment = null;
            _fileSystemState = null;
            FakeRuntimeFileInfo.ClearFiles();
            KnownKeys.Clear();
        }

        [Test]
        public static void TestCheckActiveFilesIsNotLocked()
        {
            DateTime utcNow = AxCryptEnvironment.Current.UtcNow;
            DateTime utcYesterday = utcNow.AddDays(-1);
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcYesterday, utcYesterday, utcYesterday, Stream.Null);

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
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcYesterday, utcYesterday, utcYesterday, Stream.Null);

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
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcYesterday, utcYesterday, utcYesterday, Stream.Null);

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

        [Test]
        public static void TestCheckActiveFilesKeyIsSet()
        {
            DateTime utcNow = AxCryptEnvironment.Current.UtcNow;
            DateTime utcJustNow = utcNow.AddMinutes(-1);
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcJustNow, utcJustNow, utcJustNow, Stream.Null);

            ActiveFile activeFile;
            activeFile = new ActiveFile(AxCryptEnvironment.Current.FileInfo(_encryptedFile1), AxCryptEnvironment.Current.FileInfo(_decryptedFile1), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted, null);
            _fileSystemState.Add(activeFile);

            IRuntimeFileInfo decryptedFileInfo = AxCryptEnvironment.Current.FileInfo(_decryptedFile1);
            decryptedFileInfo.SetFileTimes(utcNow, utcNow, utcNow);

            _fakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            _fileSystemState.Changed += ((object sender, EventArgs e) =>
            {
                changedWasRaised = true;
            });
            _fileSystemState.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, false, new ProgressContext());
            Assert.That(changedWasRaised, Is.True, "The file should be detected as modified, because it is considered open and decrypted, has a proper key, is modified, no running process so it should be re-encrypted and deleted.");
            activeFile = _fileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(activeFile, Is.Not.Null, "The encrypted file should be found.");
            Assert.That(activeFile.IsModified, Is.False, "The file should no longer be flagged as modified.");
            Assert.That(activeFile.Status.HasFlag(ActiveFileStatus.NotDecrypted), Is.True, "The file should no longer be decrypted, since it was re-encrypted and deleted.");
        }

        [Test]
        public static void TestCheckActiveFilesKeyIsNotSetWithKnownKey()
        {
            DateTime utcNow = AxCryptEnvironment.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, new MemoryStream(Resources.HelloWorld_Key_a_txt));
            Passphrase passphrase = new Passphrase("a");
            AxCryptFile.Decrypt(AxCryptEnvironment.Current.FileInfo(_encryptedFile1), AxCryptEnvironment.Current.FileInfo(_decryptedFile1), passphrase.DerivedPassphrase, AxCryptOptions.None, new ProgressContext());

            ActiveFile activeFile = new ActiveFile(AxCryptEnvironment.Current.FileInfo(_encryptedFile1), AxCryptEnvironment.Current.FileInfo(_decryptedFile1), passphrase.DerivedPassphrase, ActiveFileStatus.AssumedOpenAndDecrypted, null);
            _fileSystemState.Add(activeFile);
            _fileSystemState.Save();
            _fileSystemState = FileSystemState.Load(AxCryptEnvironment.Current.FileInfo(_fileSystemStateFilePath));

            _fakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            _fileSystemState.Changed += ((object sender, EventArgs e) =>
            {
                changedWasRaised = true;
            });

            activeFile = _fileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Key, Is.Null, "The key should be null after loading of new FileSystemState");

            KnownKeys.Add(passphrase.DerivedPassphrase);
            _fileSystemState.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, false, new ProgressContext());
            Assert.That(changedWasRaised, Is.True, "The ActiveFile should be modified because there is now a known key.");

            activeFile = _fileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Key, Is.Not.Null, "The key should not be null after the checking of active files.");
        }

        [Test]
        public static void TestCheckActiveFilesKeyIsNotSetWithoutKnownKey()
        {
            DateTime utcNow = AxCryptEnvironment.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, new MemoryStream(Resources.HelloWorld_Key_a_txt));
            Passphrase passphrase = new Passphrase("a");
            AxCryptFile.Decrypt(AxCryptEnvironment.Current.FileInfo(_encryptedFile1), AxCryptEnvironment.Current.FileInfo(_decryptedFile1), passphrase.DerivedPassphrase, AxCryptOptions.None, new ProgressContext());

            ActiveFile activeFile = new ActiveFile(AxCryptEnvironment.Current.FileInfo(_encryptedFile1), AxCryptEnvironment.Current.FileInfo(_decryptedFile1), passphrase.DerivedPassphrase, ActiveFileStatus.AssumedOpenAndDecrypted, null);
            _fileSystemState.Add(activeFile);
            _fileSystemState.Save();
            _fileSystemState = FileSystemState.Load(AxCryptEnvironment.Current.FileInfo(_fileSystemStateFilePath));

            IRuntimeFileInfo decryptedFileInfo = AxCryptEnvironment.Current.FileInfo(_decryptedFile1);
            decryptedFileInfo.SetFileTimes(utcNow.AddSeconds(30), utcNow.AddSeconds(30), utcNow.AddSeconds(30));

            _fakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            _fileSystemState.Changed += ((object sender, EventArgs e) =>
            {
                changedWasRaised = true;
            });

            activeFile = _fileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Key, Is.Null, "The key should be null after loading of new FileSystemState");

            _fileSystemState.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, false, new ProgressContext());
            Assert.That(changedWasRaised, Is.False, "The ActiveFile should be not be modified because the file was modified as well and thus cannot be deleted.");

            activeFile = _fileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Key, Is.Null, "The key should still be null after the checking of active files.");
            Assert.That(activeFile.Status.HasFlag(ActiveFileStatus.AssumedOpenAndDecrypted), Is.True, "The file should still be there.");
            Assert.That(activeFile.ThumbprintMatch(passphrase.DerivedPassphrase), Is.True, "The active file should still be known to be decryptable with the original passphrase.");
        }

        [Test]
        public static void TestCheckActiveFilesNotDecryptedAndDoesNotExist()
        {
            DateTime utcNow = AxCryptEnvironment.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            ActiveFile activeFile = new ActiveFile(AxCryptEnvironment.Current.FileInfo(_encryptedFile1), AxCryptEnvironment.Current.FileInfo(_decryptedFile1), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted, null);
            AxCryptEnvironment.Current.FileInfo(_decryptedFile1).Delete();
            activeFile = new ActiveFile(activeFile, ActiveFileStatus.NotDecrypted, null);
            _fileSystemState.Add(activeFile);

            bool changedWasRaised = false;
            _fileSystemState.Changed += ((object sender, EventArgs e) =>
            {
                changedWasRaised = true;
            });

            _fakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            _fileSystemState.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, false, new ProgressContext());

            Assert.That(changedWasRaised, Is.False, "The ActiveFile should be not be modified because it's already deleted.");
        }

        [Test]
        public static void TestCheckActiveFilesNoDeleteWhenNotDesktopWindows()
        {
            DateTime utcNow = AxCryptEnvironment.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            ActiveFile activeFile = new ActiveFile(AxCryptEnvironment.Current.FileInfo(_encryptedFile1), AxCryptEnvironment.Current.FileInfo(_decryptedFile1), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted, null);
            _fileSystemState.Add(activeFile);

            _fakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            _fileSystemState.Changed += ((object sender, EventArgs e) =>
            {
                changedWasRaised = true;
            });

            _fakeRuntimeEnvironment.IsDesktopWindows = false;
            _fileSystemState.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, false, new ProgressContext());
            Assert.That(changedWasRaised, Is.False, "No change should be raised when the file is not modified and not Desktop Windows.");
            activeFile = _fileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Status.HasFlag(ActiveFileStatus.AssumedOpenAndDecrypted), Is.True, "Nothing should happen with the file when not running as Desktop Windows.");

            _fakeRuntimeEnvironment.IsDesktopWindows = true;
            changedWasRaised = false;
            _fileSystemState.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, false, new ProgressContext());
            Assert.That(changedWasRaised, Is.True, "Since the file should be deleted because running as Desktop Windows the changed event should be raised.");
            activeFile = _fileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Status.HasFlag(ActiveFileStatus.NotDecrypted), Is.True, "The file should be deleted and marked as Not Decrypted when running as Desktop Windows.");
        }
    }
}