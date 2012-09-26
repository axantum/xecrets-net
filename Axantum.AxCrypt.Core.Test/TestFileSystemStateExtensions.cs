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

        private static readonly string _pathRoot = Path.GetPathRoot(Environment.CurrentDirectory);
        private static readonly string _decryptedFile1 = _pathRoot.PathCombine("Documents", "test.txt");
        private static readonly string _encryptedFile1 = _pathRoot.PathCombine("Documents", "Uncompressed.axx");
        private static readonly string _fileSystemStateFilePath = Path.Combine(Path.GetTempPath(), "DummyFileSystemState.xml");

        [SetUp]
        public static void Setup()
        {
            _environment = Os.Current;
            Os.Current = _fakeRuntimeEnvironment = new FakeRuntimeEnvironment();
            _fileSystemState = FileSystemState.Load(Os.Current.FileInfo(_fileSystemStateFilePath));
        }

        [TearDown]
        public static void Teardown()
        {
            Os.Current = _environment;
            _environment = null;
            _fakeRuntimeEnvironment = null;
            _fileSystemState = null;
            FakeRuntimeFileInfo.ClearFiles();
        }

        [Test]
        public static void TestCheckActiveFilesIsNotLocked()
        {
            DateTime utcNow = Os.Current.UtcNow;
            DateTime utcYesterday = utcNow.AddDays(-1);
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcYesterday, utcYesterday, utcYesterday, Stream.Null);

            ActiveFile activeFile = new ActiveFile(Os.Current.FileInfo(_encryptedFile1), Os.Current.FileInfo(_decryptedFile1), new AesKey(), ActiveFileStatus.NotDecrypted, null);
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
            DateTime utcNow = Os.Current.UtcNow;
            DateTime utcYesterday = utcNow.AddDays(-1);
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcYesterday, utcYesterday, utcYesterday, Stream.Null);

            ActiveFile activeFile = new ActiveFile(Os.Current.FileInfo(_encryptedFile1), Os.Current.FileInfo(_decryptedFile1), new AesKey(), ActiveFileStatus.NotDecrypted, null);
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
            DateTime utcNow = Os.Current.UtcNow;
            DateTime utcYesterday = utcNow.AddDays(-1);
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcYesterday, utcYesterday, utcYesterday, Stream.Null);

            ActiveFile activeFile = new ActiveFile(Os.Current.FileInfo(_encryptedFile1), Os.Current.FileInfo(_decryptedFile1), new AesKey(), ActiveFileStatus.NotDecrypted, null);
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
            DateTime utcNow = Os.Current.UtcNow;
            DateTime utcJustNow = utcNow.AddMinutes(-1);
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcJustNow, utcJustNow, utcJustNow, Stream.Null);

            ActiveFile activeFile;
            activeFile = new ActiveFile(Os.Current.FileInfo(_encryptedFile1), Os.Current.FileInfo(_decryptedFile1), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted, null);
            _fileSystemState.Add(activeFile);

            IRuntimeFileInfo decryptedFileInfo = Os.Current.FileInfo(_decryptedFile1);
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
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotDecrypted), Is.True, "The file should no longer be decrypted, since it was re-encrypted and deleted.");
        }

        [Test]
        public static void TestCheckActiveFilesKeyIsNotSetWithKnownKey()
        {
            DateTime utcNow = Os.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, new MemoryStream(Resources.helloworld_key_a_txt));
            Passphrase passphrase = new Passphrase("a");
            AxCryptFile.Decrypt(Os.Current.FileInfo(_encryptedFile1), Os.Current.FileInfo(_decryptedFile1), passphrase.DerivedPassphrase, AxCryptOptions.None, new ProgressContext());

            ActiveFile activeFile = new ActiveFile(Os.Current.FileInfo(_encryptedFile1), Os.Current.FileInfo(_decryptedFile1), passphrase.DerivedPassphrase, ActiveFileStatus.AssumedOpenAndDecrypted, null);
            _fileSystemState.Add(activeFile);
            _fileSystemState.Save();
            _fileSystemState = FileSystemState.Load(Os.Current.FileInfo(_fileSystemStateFilePath));

            _fakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            _fileSystemState.Changed += ((object sender, EventArgs e) =>
            {
                changedWasRaised = true;
            });

            activeFile = _fileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Key, Is.Null, "The key should be null after loading of new FileSystemState");

            _fileSystemState.KnownKeys.Add(passphrase.DerivedPassphrase);
            _fileSystemState.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, false, new ProgressContext());
            Assert.That(changedWasRaised, Is.True, "The ActiveFile should be modified because there is now a known key.");

            activeFile = _fileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Key, Is.Not.Null, "The key should not be null after the checking of active files.");
        }

        [Test]
        public static void TestCheckActiveFilesKeyIsNotSetWithoutKnownKey()
        {
            DateTime utcNow = Os.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, new MemoryStream(Resources.helloworld_key_a_txt));
            Passphrase passphrase = new Passphrase("a");
            AxCryptFile.Decrypt(Os.Current.FileInfo(_encryptedFile1), Os.Current.FileInfo(_decryptedFile1), passphrase.DerivedPassphrase, AxCryptOptions.None, new ProgressContext());

            ActiveFile activeFile = new ActiveFile(Os.Current.FileInfo(_encryptedFile1), Os.Current.FileInfo(_decryptedFile1), passphrase.DerivedPassphrase, ActiveFileStatus.AssumedOpenAndDecrypted, null);
            _fileSystemState.Add(activeFile);
            _fileSystemState.Save();
            _fileSystemState = FileSystemState.Load(Os.Current.FileInfo(_fileSystemStateFilePath));

            IRuntimeFileInfo decryptedFileInfo = Os.Current.FileInfo(_decryptedFile1);
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
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted), Is.True, "The file should still be there.");
            Assert.That(activeFile.ThumbprintMatch(passphrase.DerivedPassphrase), Is.True, "The active file should still be known to be decryptable with the original passphrase.");
        }

        [Test]
        public static void TestCheckActiveFilesNotDecryptedAndDoesNotExist()
        {
            DateTime utcNow = Os.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            ActiveFile activeFile = new ActiveFile(Os.Current.FileInfo(_encryptedFile1), Os.Current.FileInfo(_decryptedFile1), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted, null);
            Os.Current.FileInfo(_decryptedFile1).Delete();
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
            DateTime utcNow = Os.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            ActiveFile activeFile = new ActiveFile(Os.Current.FileInfo(_encryptedFile1), Os.Current.FileInfo(_decryptedFile1), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted, null);
            _fileSystemState.Add(activeFile);

            _fakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            _fileSystemState.Changed += ((object sender, EventArgs e) =>
            {
                changedWasRaised = true;
            });

            _fakeRuntimeEnvironment.Platform = Platform.Unknown;
            _fileSystemState.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, false, new ProgressContext());
            Assert.That(changedWasRaised, Is.False, "No change should be raised when the file is not modified and not Desktop Windows.");
            activeFile = _fileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted), Is.True, "Nothing should happen with the file when not running as Desktop Windows.");

            _fakeRuntimeEnvironment.Platform = Platform.WindowsDesktop;
            changedWasRaised = false;
            _fileSystemState.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, false, new ProgressContext());
            Assert.That(changedWasRaised, Is.True, "Since the file should be deleted because running as Desktop Windows the changed event should be raised.");
            activeFile = _fileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotDecrypted), Is.True, "The file should be deleted and marked as Not Decrypted when running as Desktop Windows.");
        }

        [Test]
        public static void TestCheckActiveFilesUpdateButWithTargetLockedForSharing()
        {
            DateTime utcNow = Os.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, new MemoryStream(Resources.helloworld_key_a_txt));
            Passphrase passphrase = new Passphrase("a");
            AxCryptFile.Decrypt(Os.Current.FileInfo(_encryptedFile1), Os.Current.FileInfo(_decryptedFile1), passphrase.DerivedPassphrase, AxCryptOptions.None, new ProgressContext());

            ActiveFile activeFile = new ActiveFile(Os.Current.FileInfo(_encryptedFile1), Os.Current.FileInfo(_decryptedFile1), passphrase.DerivedPassphrase, ActiveFileStatus.AssumedOpenAndDecrypted, null);
            _fileSystemState.Add(activeFile);

            IRuntimeFileInfo decryptedFileInfo = Os.Current.FileInfo(_decryptedFile1);
            decryptedFileInfo.SetFileTimes(utcNow.AddSeconds(30), utcNow.AddSeconds(30), utcNow.AddSeconds(30));

            _fakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            _fileSystemState.Changed += ((object sender, EventArgs e) =>
            {
                changedWasRaised = true;
            });

            _fileSystemState.KnownKeys.Add(passphrase.DerivedPassphrase);

            EventHandler eventHandler = ((object sender, EventArgs e) =>
            {
                FakeRuntimeFileInfo fileInfo = (FakeRuntimeFileInfo)sender;
                if (fileInfo.FullName == _decryptedFile1)
                {
                    throw new IOException("Faked sharing violation.");
                }
            });
            FakeRuntimeFileInfo.OpeningForRead += eventHandler;
            try
            {
                _fileSystemState.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, false, new ProgressContext());
            }
            finally
            {
                FakeRuntimeFileInfo.OpeningForRead -= eventHandler;
            }

            Assert.That(changedWasRaised, Is.True, "The ActiveFile should be modified because it should now be marked as not shareable.");
            activeFile = _fileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotShareable), Is.True, "The ActiveFile should be marked as not shareable after the checking of active files.");
        }

        [Test]
        public static void TestTryDeleteButProcessHasNotExited()
        {
            DateTime utcNow = Os.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            FakeLauncher fakeLauncher = new FakeLauncher(_decryptedFile1);
            ActiveFile activeFile = new ActiveFile(Os.Current.FileInfo(_encryptedFile1), Os.Current.FileInfo(_decryptedFile1), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted, fakeLauncher);
            _fileSystemState.Add(activeFile);

            _fakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            _fileSystemState.Changed += ((object sender, EventArgs e) =>
            {
                changedWasRaised = true;
            });
            _fakeRuntimeEnvironment.Platform = Platform.WindowsDesktop;
            _fileSystemState.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, true, new ProgressContext());

            activeFile = _fileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(changedWasRaised, Is.False, "No changed event should be raised because no change should occur since the process is active.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted), Is.True, "The ActiveFile plain text should not be deleted after the checking of active files because the launcher is active.");
        }

        [Test]
        public static void TestCheckProcessExitedWhenExited()
        {
            DateTime utcNow = Os.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            FakeLauncher fakeLauncher = new FakeLauncher(_decryptedFile1);
            ActiveFile activeFile = new ActiveFile(Os.Current.FileInfo(_encryptedFile1), Os.Current.FileInfo(_decryptedFile1), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable, fakeLauncher);
            _fileSystemState.Add(activeFile);

            _fakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            _fileSystemState.Changed += ((object sender, EventArgs e) =>
            {
                changedWasRaised = true;
            });
            _fakeRuntimeEnvironment.Platform = Platform.WindowsDesktop;
            fakeLauncher.HasExited = true;
            _fileSystemState.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, true, new ProgressContext());

            activeFile = _fileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(changedWasRaised, Is.True, "A changed event should be raised because the process has exited.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotDecrypted), Is.True, "The ActiveFile plain text should be deleted after the checking of active files because the launcher is no longer active.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotShareable), Is.False, "The file should be shareable after checking of active files because the launcher is no longer active.");
        }

        [Test]
        public static void TestTryDeleteButDecryptedSharingLocked()
        {
            DateTime utcNow = Os.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            ActiveFile activeFile = new ActiveFile(Os.Current.FileInfo(_encryptedFile1), Os.Current.FileInfo(_decryptedFile1), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted, null);
            _fileSystemState.Add(activeFile);

            _fakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            _fileSystemState.Changed += ((object sender, EventArgs e) =>
            {
                changedWasRaised = true;
            });
            _fakeRuntimeEnvironment.Platform = Platform.WindowsDesktop;

            EventHandler eventHandler = ((object sender, EventArgs e) =>
            {
                FakeRuntimeFileInfo fileInfo = (FakeRuntimeFileInfo)sender;
                if (fileInfo.FullName == _decryptedFile1)
                {
                    throw new IOException("Faked sharing violation.");
                }
            });
            FakeRuntimeFileInfo.Deleting += eventHandler;
            try
            {
                _fileSystemState.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, false, new ProgressContext());
            }
            finally
            {
                FakeRuntimeFileInfo.Deleting -= eventHandler;
            }

            activeFile = _fileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(changedWasRaised, Is.True, "A changed event should be raised because it should now be NotShareable.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted), Is.True, "The ActiveFile plain text should still be there after the checking of active files because the file is NotShareable.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotShareable), Is.True, "The ActiveFile plain text should be NotShareable after the checking of active files because the file could not be deleted.");
        }

        [Test]
        public static void TestPurgeActiveFilesWhenFileIsLocked()
        {
            DateTime utcNow = Os.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            IRuntimeFileInfo encryptedFileInfo = Os.Current.FileInfo(_encryptedFile1);
            IRuntimeFileInfo decryptedFileInfo = Os.Current.FileInfo(_decryptedFile1);
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted, null);
            _fileSystemState.Add(activeFile);

            bool changedWasRaised = false;
            _fileSystemState.Changed += ((object sender, EventArgs e) =>
            {
                changedWasRaised = true;
            });

            using (FileLock fileLock = FileLock.Lock(decryptedFileInfo))
            {
                _fileSystemState.PurgeActiveFiles(new ProgressContext());
            }

            Assert.That(changedWasRaised, Is.False, "A changed event should not be raised because the decrypted file is locked.");
        }

        [Test]
        public static void TestPurgeActiveFilesWhenFileIsModified()
        {
            DateTime utcNow = Os.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            IRuntimeFileInfo encryptedFileInfo = Os.Current.FileInfo(_encryptedFile1);
            IRuntimeFileInfo decryptedFileInfo = Os.Current.FileInfo(_decryptedFile1);
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable, null);
            _fileSystemState.Add(activeFile);

            _fakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            DateTime utcLater = Os.Current.UtcNow;

            decryptedFileInfo.SetFileTimes(utcLater, utcLater, utcLater);

            bool changedWasRaised = false;
            _fileSystemState.Changed += ((object sender, EventArgs e) =>
            {
                changedWasRaised = true;
            });

            _fileSystemState.PurgeActiveFiles(new ProgressContext());

            activeFile = _fileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(changedWasRaised, Is.True, "A changed event should be raised because the decrypted file is modified.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotDecrypted), Is.True, "The NotShareable not withstanding, the purge should have updated the file and removed the decrypted file.");
        }

        [Test]
        public static void TestUpdateActiveFileWithKeyIfKeyMatchesThumbprintWithKnownKey()
        {
            IRuntimeFileInfo encryptedFileInfo = Os.Current.FileInfo(_encryptedFile1);
            IRuntimeFileInfo decryptedFileInfo = Os.Current.FileInfo(_decryptedFile1);
            AesKey key = new AesKey();
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable, null);
            _fileSystemState.Add(activeFile);

            bool updateWasMade = _fileSystemState.UpdateActiveFileWithKeyIfKeyMatchesThumbprint(key);
            Assert.That(updateWasMade, Is.False, "Since there are only ActiveFiles with known keys in the list, no update should be made.");
        }

        [Test]
        public static void TestUpdateActiveFileWithKeyIfKeyMatchesThumbprintWithWrongThumbprint()
        {
            IRuntimeFileInfo encryptedFileInfo = Os.Current.FileInfo(_encryptedFile1);
            IRuntimeFileInfo decryptedFileInfo = Os.Current.FileInfo(_decryptedFile1);
            AesKey key = new AesKey();
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable, null);
            _fileSystemState.Add(activeFile);
            _fileSystemState.Save();

            _fileSystemState = FileSystemState.Load(Os.Current.FileInfo(_fileSystemStateFilePath));

            AesKey wrongKey = new AesKey();
            bool updateWasMade = _fileSystemState.UpdateActiveFileWithKeyIfKeyMatchesThumbprint(wrongKey);
            Assert.That(updateWasMade, Is.False, "Since there are only ActiveFiles with wrong keys in the list, no update should be made.");
        }

        [Test]
        public static void TestUpdateActiveFileWithKeyIfKeyMatchesThumbprintWithMatchingThumbprint()
        {
            IRuntimeFileInfo encryptedFileInfo = Os.Current.FileInfo(_encryptedFile1);
            IRuntimeFileInfo decryptedFileInfo = Os.Current.FileInfo(_decryptedFile1);
            AesKey key = new AesKey();
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable, null);
            _fileSystemState.Add(activeFile);
            _fileSystemState.Save();

            _fileSystemState = FileSystemState.Load(Os.Current.FileInfo(_fileSystemStateFilePath));

            bool updateWasMade = _fileSystemState.UpdateActiveFileWithKeyIfKeyMatchesThumbprint(key);
            Assert.That(updateWasMade, Is.True, "Since there is an ActiveFile with the right thumbprint in the list, an update should be made.");
        }

        [Test]
        public static void TestRemoveRecentFile()
        {
            IRuntimeFileInfo encryptedFileInfo = Os.Current.FileInfo(_encryptedFile1);
            IRuntimeFileInfo decryptedFileInfo = Os.Current.FileInfo(_decryptedFile1);
            AesKey key = new AesKey();
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable, null);
            _fileSystemState.Add(activeFile);
            _fileSystemState.Save();

            ActiveFile beforeRemoval = _fileSystemState.FindEncryptedPath(encryptedFileInfo.FullName);
            Assert.That(beforeRemoval, Is.Not.Null, "Before being removed, the ActiveFile should be possible to find.");

            _fileSystemState.RemoveRecentFile(encryptedFileInfo.FullName);

            ActiveFile afterRemoval = _fileSystemState.FindEncryptedPath(encryptedFileInfo.FullName);
            Assert.That(afterRemoval, Is.Null, "After being removed, the ActiveFile should not be possible to find.");
        }
    }
}