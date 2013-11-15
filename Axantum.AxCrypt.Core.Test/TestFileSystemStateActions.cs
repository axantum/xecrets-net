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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestFileSystemStateActions
    {
        private static readonly string _pathRoot = Path.GetPathRoot(Environment.CurrentDirectory);
        private static readonly string _documentsFolder = _pathRoot.PathCombine("Documents");
        private static readonly string _decryptedFile1 = _documentsFolder.PathCombine("test.txt");
        private static readonly string _encryptedFile1 = _documentsFolder.PathCombine("Uncompressed.axx");
        private static readonly string _underDocumentsFolder = _documentsFolder.PathCombine("Under");
        private static readonly string _decryptedFile11 = _underDocumentsFolder.PathCombine("undertest.txt");
        private static readonly string _encryptedFile11 = _underDocumentsFolder.PathCombine("underUncompressed.axx");
        private static readonly string _fileSystemStateFilePath = Path.Combine(Path.GetTempPath(), "DummyFileSystemState.xml");

        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();

            FactoryRegistry.Instance.Singleton<FileSystemState>(FileSystemState.Create(OS.Current.FileInfo(_fileSystemStateFilePath)));
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestCheckActiveFilesIsNotLocked()
        {
            DateTime utcNow = OS.Current.UtcNow;
            DateTime utcYesterday = utcNow.AddDays(-1);
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcYesterday, utcYesterday, utcYesterday, Stream.Null);

            ActiveFile activeFile = new ActiveFile(OS.Current.FileInfo(_encryptedFile1), OS.Current.FileInfo(_decryptedFile1), new AesKey(), ActiveFileStatus.NotDecrypted);
            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(10); });
            bool changedWasRaised = false;
            Instance.FileSystemState.Add(activeFile);
            Instance.FileSystemState.Changed += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });
            Instance.FileSystemState.Actions.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            Assert.That(changedWasRaised, Is.True, "The file should be detected as decrypted being created.");
        }

        [Test]
        public static void TestCheckActiveFilesIsLocked()
        {
            DateTime utcNow = OS.Current.UtcNow;
            DateTime utcYesterday = utcNow.AddDays(-1);
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcYesterday, utcYesterday, utcYesterday, Stream.Null);

            ActiveFile activeFile = new ActiveFile(OS.Current.FileInfo(_encryptedFile1), OS.Current.FileInfo(_decryptedFile1), new AesKey(), ActiveFileStatus.NotDecrypted);
            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(10); });
            bool changedWasRaised = false;
            Instance.FileSystemState.Add(activeFile);
            Instance.FileSystemState.Changed += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });
            using (FileLock fileLock = FileLock.Lock(activeFile.EncryptedFileInfo))
            {
                Instance.FileSystemState.Actions.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            }
            Assert.That(changedWasRaised, Is.False, "The file should be not be detected as decrypted being created because the encrypted file is locked.");
            using (FileLock fileLock = FileLock.Lock(activeFile.DecryptedFileInfo))
            {
                Instance.FileSystemState.Actions.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            }
            Assert.That(changedWasRaised, Is.False, "The file should be not be detected as decrypted being created because the decrypted file is locked.");
        }

        [Test]
        public static void TestCheckActiveFilesKeyIsSet()
        {
            DateTime utcNow = OS.Current.UtcNow;
            DateTime utcJustNow = utcNow.AddMinutes(-1);
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcJustNow, utcJustNow, utcJustNow, Stream.Null);

            ActiveFile activeFile;
            activeFile = new ActiveFile(OS.Current.FileInfo(_encryptedFile1), OS.Current.FileInfo(_decryptedFile1), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted);
            Instance.FileSystemState.Add(activeFile);
            Instance.KnownKeys.Add(activeFile.Key);

            IRuntimeFileInfo decryptedFileInfo = OS.Current.FileInfo(_decryptedFile1);
            decryptedFileInfo.SetFileTimes(utcNow, utcNow, utcNow);

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Instance.FileSystemState.Changed += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });
            Instance.FileSystemState.Actions.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            Assert.That(changedWasRaised, Is.True, "The file should be detected as modified, because it is considered open and decrypted, has a proper key, is modified, no running process so it should be re-encrypted and deleted.");
            activeFile = Instance.FileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(activeFile, Is.Not.Null, "The encrypted file should be found.");
            Assert.That(activeFile.IsModified, Is.False, "The file should no longer be flagged as modified.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotDecrypted), Is.True, "The file should no longer be decrypted, since it was re-encrypted and deleted.");
        }

        [Test]
        public static void TestCheckActiveFilesKeyIsNotSetWithKnownKey()
        {
            DateTime utcNow = OS.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.helloworld_key_a_txt));
            Passphrase passphrase = new Passphrase("a");
            AxCryptFile.Decrypt(OS.Current.FileInfo(_encryptedFile1), OS.Current.FileInfo(_decryptedFile1), passphrase.DerivedPassphrase, AxCryptOptions.None, new ProgressContext());

            ActiveFile activeFile = new ActiveFile(OS.Current.FileInfo(_encryptedFile1), OS.Current.FileInfo(_decryptedFile1), passphrase.DerivedPassphrase, ActiveFileStatus.AssumedOpenAndDecrypted);
            Instance.FileSystemState.Add(activeFile);
            Instance.FileSystemState.Save();

            FactoryRegistry.Instance.Singleton<FileSystemState>(FileSystemState.Create(OS.Current.FileInfo(_fileSystemStateFilePath)));

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Instance.FileSystemState.Changed += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });

            activeFile = Instance.FileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Key, Is.Null, "The key should be null after loading of new FileSystemState");

            Instance.KnownKeys.Add(passphrase.DerivedPassphrase);
            Instance.FileSystemState.Actions.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            Assert.That(changedWasRaised, Is.True, "The ActiveFile should be modified because there is now a known key.");

            activeFile = Instance.FileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Key, Is.Not.Null, "The key should not be null after the checking of active files.");
        }

        [Test]
        public static void TestCheckActiveFilesKeyIsNotSetWithoutKnownKey()
        {
            DateTime utcNow = OS.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.helloworld_key_a_txt));
            Passphrase passphrase = new Passphrase("a");
            AxCryptFile.Decrypt(OS.Current.FileInfo(_encryptedFile1), OS.Current.FileInfo(_decryptedFile1), passphrase.DerivedPassphrase, AxCryptOptions.None, new ProgressContext());

            ActiveFile activeFile = new ActiveFile(OS.Current.FileInfo(_encryptedFile1), OS.Current.FileInfo(_decryptedFile1), passphrase.DerivedPassphrase, ActiveFileStatus.AssumedOpenAndDecrypted);
            Instance.FileSystemState.Add(activeFile);
            Instance.FileSystemState.Save();

            FactoryRegistry.Instance.Singleton<FileSystemState>(FileSystemState.Create(OS.Current.FileInfo(_fileSystemStateFilePath)));

            IRuntimeFileInfo decryptedFileInfo = OS.Current.FileInfo(_decryptedFile1);
            decryptedFileInfo.SetFileTimes(utcNow.AddSeconds(30), utcNow.AddSeconds(30), utcNow.AddSeconds(30));

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Instance.FileSystemState.Changed += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });

            activeFile = Instance.FileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Key, Is.Null, "The key should be null after loading of new FileSystemState");

            Instance.FileSystemState.Actions.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            Assert.That(changedWasRaised, Is.False, "The ActiveFile should be not be modified because the file was modified as well and thus cannot be deleted.");

            Instance.KnownKeys.Add(Passphrase.Derive("x"));
            Instance.KnownKeys.Add(new Passphrase("y").DerivedPassphrase);
            Instance.FileSystemState.Actions.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            Assert.That(changedWasRaised, Is.False, "The ActiveFile should be not be modified because the file was modified as well and thus cannot be deleted.");

            activeFile = Instance.FileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Key, Is.Null, "The key should still be null after the checking of active files.");

            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted), Is.True, "The file should still be there.");
            Assert.That(activeFile.ThumbprintMatch(passphrase.DerivedPassphrase), Is.True, "The active file should still be known to be decryptable with the original passphrase.");
        }

        [Test]
        public static void TestCheckActiveFilesNotDecryptedAndDoesNotExist()
        {
            DateTime utcNow = OS.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            ActiveFile activeFile = new ActiveFile(OS.Current.FileInfo(_encryptedFile1), OS.Current.FileInfo(_decryptedFile1), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted);
            OS.Current.FileInfo(_decryptedFile1).Delete();
            activeFile = new ActiveFile(activeFile, ActiveFileStatus.NotDecrypted, null);
            Instance.FileSystemState.Add(activeFile);
            Instance.KnownKeys.Add(activeFile.Key);

            bool changedWasRaised = false;
            Instance.FileSystemState.Changed += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            Instance.FileSystemState.Actions.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());

            Assert.That(changedWasRaised, Is.False, "The ActiveFile should be not be modified because it's already deleted.");
        }

        [Test]
        public static void TestCheckActiveFilesNoDeleteWhenNotDesktopWindows()
        {
            DateTime utcNow = OS.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            ActiveFile activeFile = new ActiveFile(OS.Current.FileInfo(_encryptedFile1), OS.Current.FileInfo(_decryptedFile1), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted);
            Instance.FileSystemState.Add(activeFile);
            Instance.KnownKeys.Add(activeFile.Key);

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Instance.FileSystemState.Changed += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });

            SetupAssembly.FakeRuntimeEnvironment.Platform = Platform.Unknown;
            Instance.FileSystemState.Actions.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            Assert.That(changedWasRaised, Is.False, "No change should be raised when the file is not modified and not Desktop Windows.");
            activeFile = Instance.FileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted), Is.True, "Nothing should happen with the file when not running as Desktop Windows.");

            SetupAssembly.FakeRuntimeEnvironment.Platform = Platform.WindowsDesktop;
            changedWasRaised = false;
            Instance.FileSystemState.Actions.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            Assert.That(changedWasRaised, Is.True, "Since the file should be deleted because running as Desktop Windows the changed event should be raised.");
            activeFile = Instance.FileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotDecrypted), Is.True, "The file should be deleted and marked as Not Decrypted when running as Desktop Windows.");
        }

        [Test]
        public static void TestCheckActiveFilesUpdateButWithTargetLockedForSharing()
        {
            DateTime utcNow = OS.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.helloworld_key_a_txt));
            Passphrase passphrase = new Passphrase("a");
            AxCryptFile.Decrypt(OS.Current.FileInfo(_encryptedFile1), OS.Current.FileInfo(_decryptedFile1), passphrase.DerivedPassphrase, AxCryptOptions.None, new ProgressContext());

            ActiveFile activeFile = new ActiveFile(OS.Current.FileInfo(_encryptedFile1), OS.Current.FileInfo(_decryptedFile1), passphrase.DerivedPassphrase, ActiveFileStatus.AssumedOpenAndDecrypted);
            Instance.FileSystemState.Add(activeFile);

            IRuntimeFileInfo decryptedFileInfo = OS.Current.FileInfo(_decryptedFile1);
            decryptedFileInfo.SetFileTimes(utcNow.AddSeconds(30), utcNow.AddSeconds(30), utcNow.AddSeconds(30));

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Instance.FileSystemState.Changed += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });

            Instance.KnownKeys.Add(passphrase.DerivedPassphrase);

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
                Instance.FileSystemState.Actions.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            }
            finally
            {
                FakeRuntimeFileInfo.OpeningForRead -= eventHandler;
            }

            Assert.That(changedWasRaised, Is.True, "The ActiveFile should be modified because it should now be marked as not shareable.");
            activeFile = Instance.FileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotShareable), Is.True, "The ActiveFile should be marked as not shareable after the checking of active files.");
        }

        [Test]
        public static void TestTryDeleteButProcessHasNotExited()
        {
            DateTime utcNow = OS.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            FakeLauncher fakeLauncher = new FakeLauncher(_decryptedFile1);
            ActiveFile activeFile = new ActiveFile(OS.Current.FileInfo(_encryptedFile1), OS.Current.FileInfo(_decryptedFile1), new AesKey(), ActiveFileStatus.NotDecrypted);
            activeFile = new ActiveFile(activeFile, ActiveFileStatus.AssumedOpenAndDecrypted, fakeLauncher);
            Instance.FileSystemState.Add(activeFile);
            Instance.KnownKeys.Add(activeFile.Key);

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Instance.FileSystemState.Changed += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });
            SetupAssembly.FakeRuntimeEnvironment.Platform = Platform.WindowsDesktop;
            Instance.FileSystemState.Actions.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());

            activeFile = Instance.FileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(changedWasRaised, Is.False, "No changed event should be raised because no change should occur since the process is active.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted), Is.True, "The ActiveFile plain text should not be deleted after the checking of active files because the launcher is active.");
        }

        [Test]
        public static void TestCheckProcessExitedWhenExited()
        {
            DateTime utcNow = OS.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            FakeLauncher fakeLauncher = new FakeLauncher(_decryptedFile1);
            ActiveFile activeFile = new ActiveFile(OS.Current.FileInfo(_encryptedFile1), OS.Current.FileInfo(_decryptedFile1), new AesKey(), ActiveFileStatus.NotDecrypted);
            activeFile = new ActiveFile(activeFile, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable, fakeLauncher);
            Instance.FileSystemState.Add(activeFile);

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Instance.FileSystemState.Changed += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });
            SetupAssembly.FakeRuntimeEnvironment.Platform = Platform.WindowsDesktop;
            fakeLauncher.HasExited = true;
            Instance.FileSystemState.Actions.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());

            activeFile = Instance.FileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(changedWasRaised, Is.True, "A changed event should be raised because the process has exited.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotDecrypted), Is.True, "The ActiveFile plain text should be deleted after the checking of active files because the launcher is no longer active.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotShareable), Is.False, "The file should be shareable after checking of active files because the launcher is no longer active.");
        }

        [Test]
        public static void TestTryDeleteButDecryptedSharingLocked()
        {
            DateTime utcNow = OS.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            ActiveFile activeFile = new ActiveFile(OS.Current.FileInfo(_encryptedFile1), OS.Current.FileInfo(_decryptedFile1), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted);
            Instance.FileSystemState.Add(activeFile);

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Instance.FileSystemState.Changed += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });
            SetupAssembly.FakeRuntimeEnvironment.Platform = Platform.WindowsDesktop;

            EventHandler eventHandler = ((object sender, EventArgs e) =>
            {
                FakeRuntimeFileInfo fileInfo = (FakeRuntimeFileInfo)sender;
                if (fileInfo.FullName == _decryptedFile1)
                {
                    throw new IOException("Faked sharing violation.");
                }
            });
            FakeRuntimeFileInfo.Deleting += eventHandler;
            FakeRuntimeFileInfo.OpeningForWrite += eventHandler;
            try
            {
                Instance.FileSystemState.Actions.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            }
            finally
            {
                FakeRuntimeFileInfo.Deleting -= eventHandler;
                FakeRuntimeFileInfo.OpeningForWrite -= eventHandler;
            }

            activeFile = Instance.FileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(changedWasRaised, Is.True, "A changed event should be raised because it should now be NotShareable.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted), Is.True, "The ActiveFile plain text should still be there after the checking of active files because the file is NotShareable.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotShareable), Is.True, "The ActiveFile plain text should be NotShareable after the checking of active files because the file could not be deleted.");
        }

        [Test]
        public static void TestPurgeActiveFilesWhenFileIsLocked()
        {
            DateTime utcNow = OS.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            IRuntimeFileInfo encryptedFileInfo = OS.Current.FileInfo(_encryptedFile1);
            IRuntimeFileInfo decryptedFileInfo = OS.Current.FileInfo(_decryptedFile1);
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted);
            Instance.FileSystemState.Add(activeFile);

            bool changedWasRaised = false;
            Instance.FileSystemState.Changed += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });

            using (FileLock fileLock = FileLock.Lock(decryptedFileInfo))
            {
                Instance.FileSystemState.Actions.PurgeActiveFiles(new ProgressContext());
            }

            Assert.That(changedWasRaised, Is.False, "A changed event should not be raised because the decrypted file is locked.");
        }

        [Test]
        public static void TestPurgeActiveFilesWhenFileIsModified()
        {
            DateTime utcNow = OS.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            IRuntimeFileInfo encryptedFileInfo = OS.Current.FileInfo(_encryptedFile1);
            IRuntimeFileInfo decryptedFileInfo = OS.Current.FileInfo(_decryptedFile1);
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable);
            Instance.FileSystemState.Add(activeFile);

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            DateTime utcLater = OS.Current.UtcNow;

            decryptedFileInfo.SetFileTimes(utcLater, utcLater, utcLater);

            bool changedWasRaised = false;
            Instance.FileSystemState.Changed += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });

            Instance.FileSystemState.Actions.PurgeActiveFiles(new ProgressContext());

            activeFile = Instance.FileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(changedWasRaised, Is.True, "A changed event should be raised because the decrypted file is modified.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotDecrypted), Is.True, "The NotShareable not withstanding, the purge should have updated the file and removed the decrypted file.");
        }

        [Test]
        public static void TestUpdateActiveFileWithKeyIfKeyMatchesThumbprintWithKnownKey()
        {
            IRuntimeFileInfo encryptedFileInfo = OS.Current.FileInfo(_encryptedFile1);
            IRuntimeFileInfo decryptedFileInfo = OS.Current.FileInfo(_decryptedFile1);
            AesKey key = new AesKey();
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable);
            Instance.FileSystemState.Add(activeFile);

            bool updateWasMade = Instance.FileSystemState.Actions.UpdateActiveFileWithKeyIfKeyMatchesThumbprint(key);
            Assert.That(updateWasMade, Is.False, "Since there are only ActiveFiles with known keys in the list, no update should be made.");
        }

        [Test]
        public static void TestUpdateActiveFileWithKeyIfKeyMatchesThumbprintWithWrongThumbprint()
        {
            IRuntimeFileInfo encryptedFileInfo = OS.Current.FileInfo(_encryptedFile1);
            IRuntimeFileInfo decryptedFileInfo = OS.Current.FileInfo(_decryptedFile1);
            AesKey key = new AesKey();
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable);
            Instance.FileSystemState.Add(activeFile);
            Instance.FileSystemState.Save();

            FactoryRegistry.Instance.Singleton<FileSystemState>(FileSystemState.Create(OS.Current.FileInfo(_fileSystemStateFilePath)));

            AesKey wrongKey = new AesKey();
            bool updateWasMade = Instance.FileSystemState.Actions.UpdateActiveFileWithKeyIfKeyMatchesThumbprint(wrongKey);
            Assert.That(updateWasMade, Is.False, "Since there are only ActiveFiles with wrong keys in the list, no update should be made.");
        }

        [Test]
        public static void TestUpdateActiveFileWithKeyIfKeyMatchesThumbprintWithMatchingThumbprint()
        {
            IRuntimeFileInfo encryptedFileInfo = OS.Current.FileInfo(_encryptedFile1);
            IRuntimeFileInfo decryptedFileInfo = OS.Current.FileInfo(_decryptedFile1);
            AesKey key = new AesKey();
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable);
            Instance.FileSystemState.Add(activeFile);
            Instance.FileSystemState.Save();

            FactoryRegistry.Instance.Singleton<FileSystemState>(FileSystemState.Create(OS.Current.FileInfo(_fileSystemStateFilePath)));

            bool updateWasMade = Instance.FileSystemState.Actions.UpdateActiveFileWithKeyIfKeyMatchesThumbprint(key);
            Assert.That(updateWasMade, Is.True, "Since there is an ActiveFile with the right thumbprint in the list, an update should be made.");
        }

        [Test]
        public static void TestUpdateActiveFileButSkippingKeyCheckDueToIrrelevantStatus()
        {
            IRuntimeFileInfo encryptedFileInfo = OS.Current.FileInfo(_encryptedFile1);
            IRuntimeFileInfo decryptedFileInfo = OS.Current.FileInfo(_decryptedFile1);
            AesKey key = new AesKey();
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.NotShareable);
            Instance.FileSystemState.Add(activeFile);

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return DateTime.UtcNow.AddMinutes(1); });

            bool somethingWasChanged = false;
            Instance.FileSystemState.Changed += (object sender, ActiveFileChangedEventArgs e) =>
                {
                    somethingWasChanged = true;
                };
            Instance.FileSystemState.Actions.CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            Assert.That(!somethingWasChanged, "No event should be raised, because nothing should change.");
        }

        [Test]
        public static void TestRemoveRecentFile()
        {
            IRuntimeFileInfo encryptedFileInfo = OS.Current.FileInfo(_encryptedFile1);
            IRuntimeFileInfo decryptedFileInfo = OS.Current.FileInfo(_decryptedFile1);
            AesKey key = new AesKey();
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable);
            Instance.FileSystemState.Add(activeFile);
            Instance.FileSystemState.Save();

            ActiveFile beforeRemoval = Instance.FileSystemState.FindEncryptedPath(encryptedFileInfo.FullName);
            Assert.That(beforeRemoval, Is.Not.Null, "Before being removed, the ActiveFile should be possible to find.");

            Instance.FileSystemState.Actions.RemoveRecentFiles(new string[] { encryptedFileInfo.FullName }, new ProgressContext());

            ActiveFile afterRemoval = Instance.FileSystemState.FindEncryptedPath(encryptedFileInfo.FullName);
            Assert.That(afterRemoval, Is.Null, "After being removed, the ActiveFile should not be possible to find.");
        }

        [Test]
        public static void TestRemoveRecentFileWhenFileDoesNotExist()
        {
            IRuntimeFileInfo encryptedFileInfo = OS.Current.FileInfo(_encryptedFile1);
            IRuntimeFileInfo decryptedFileInfo = OS.Current.FileInfo(_decryptedFile1);
            AesKey key = new AesKey();
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable);
            Instance.FileSystemState.Add(activeFile);
            Instance.FileSystemState.Save();

            ActiveFile beforeRemoval = Instance.FileSystemState.FindEncryptedPath(encryptedFileInfo.FullName);
            Assert.That(beforeRemoval, Is.Not.Null, "Before being removed, the ActiveFile should be possible to find.");

            Assert.DoesNotThrow(() => { Instance.FileSystemState.Actions.RemoveRecentFiles(new string[] { encryptedFileInfo.FullName + ".notfound" }, new ProgressContext()); });

            ActiveFile afterFailedRemoval = Instance.FileSystemState.FindEncryptedPath(encryptedFileInfo.FullName);
            Assert.That(afterFailedRemoval, Is.Not.Null, "After failed removal, the ActiveFile should still be possible to find.");
        }

        [Test]
        public static void TestDecryptedFilesInWatchedFolders()
        {
            DateTime utcNow = OS.Current.UtcNow;
            FakeRuntimeFileInfo.AddFolder(_documentsFolder);
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFolder(_underDocumentsFolder);
            FakeRuntimeFileInfo.AddFile(_encryptedFile11, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile11, utcNow, utcNow, utcNow, Stream.Null);
            Instance.FileSystemState.AddWatchedFolder(new WatchedFolder(_underDocumentsFolder, AesKeyThumbprint.Zero));

            IEnumerable<IRuntimeFileInfo> encryptableFiles = Instance.FileSystemState.Actions.ListEncryptableInWatchedFolders();

            Assert.That(encryptableFiles.Count(), Is.EqualTo(1), "There should be exactly one decrypted file here.");
            Assert.That(encryptableFiles.First().FullName, Is.EqualTo(_decryptedFile11), "This is the file that is decrypted here.");
        }

        [Test]
        public static void TestEncryptFilesInWatchedFoldersBadArguments()
        {
            AesKey NullAesKey = null;
            ProgressContext NullProgressContext = null;

            Assert.Throws<ArgumentNullException>(() => { Instance.FileSystemState.Actions.EncryptFilesInWatchedFolders(NullAesKey, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { Instance.FileSystemState.Actions.EncryptFilesInWatchedFolders(new AesKey(), NullProgressContext); });
        }

        [Test]
        public static void TestEncryptFilesInWatchedFolders()
        {
            MockAxCryptFile mock = new MockAxCryptFile();
            int callTimes = 0;
            mock.EncryptFileUniqueWithBackupAndWipeMock = (IRuntimeFileInfo fileInfo, AesKey encryptionKey, ProgressContext progress) => { ++callTimes; };

            FactoryRegistry.Instance.Register<AxCryptFile>(() => mock);

            FakeRuntimeFileInfo.AddFolder(@"C:\My Documents\");
            FakeRuntimeFileInfo.AddFile(@"C:\My Documents\Plaintext 1.txt", Stream.Null);
            FakeRuntimeFileInfo.AddFile(@"C:\My Documents\Plaintext 2.txt", Stream.Null);
            FakeRuntimeFileInfo.AddFile(@"C:\My Documents\Encrypted.axx", Stream.Null);

            Instance.FileSystemState.AddWatchedFolder(new WatchedFolder(@"C:\My Documents\", AesKeyThumbprint.Zero));

            Instance.FileSystemState.Actions.EncryptFilesInWatchedFolders(new AesKey(), new ProgressContext());

            Assert.That(callTimes, Is.EqualTo(2));
        }

        [Test]
        public static void TestHandleSessionEventWatchedFolderAdded()
        {
            MockAxCryptFile mock = new MockAxCryptFile();
            bool called = false;
            mock.EncryptFilesUniqueWithBackupAndWipeMock = (IRuntimeFileInfo fileInfo, AesKey encryptionKey, ProgressContext progress) => { called = fileInfo.FullName == @"C:\My Documents\"; };

            FactoryRegistry.Instance.Register<AxCryptFile>(() => mock);

            Instance.FileSystemState.Actions.HandleSessionEvent(new SessionEvent(SessionEventType.WatchedFolderAdded, new AesKey(), @"C:\My Documents\"), new ProgressContext());

            Assert.That(called, Is.True);
        }

        [Test]
        public static void TestHandleSessionEventWatchedFolderRemoved()
        {
            MockAxCryptFile mock = new MockAxCryptFile();
            bool called = false;
            mock.DecryptFilesUniqueWithWipeOfOriginalMock = (IRuntimeFileInfo fileInfo, AesKey decryptionKey, ProgressContext progress) => { called = fileInfo.FullName == @"C:\My Documents\"; };

            FactoryRegistry.Instance.Register<AxCryptFile>(() => mock);

            Instance.FileSystemState.Actions.HandleSessionEvent(new SessionEvent(SessionEventType.WatchedFolderRemoved, new AesKey(), @"C:\My Documents\"), new ProgressContext());

            Assert.That(called, Is.True);
        }

        [Test]
        public static void TestHandleSessionEventLogOn()
        {
            MockFileSystemStateActions mock = new MockFileSystemStateActions(Instance.FileSystemState);
            bool called = false;
            mock.EncryptFilesInWatchedFoldersMock = (AesKey encryptionKey, ProgressContext progress) => { called = true; };

            FactoryRegistry.Instance.Register<FileSystemState, FileSystemStateActions>((fileSystemState) => mock);

            Instance.FileSystemState.Actions.HandleSessionEvent(new SessionEvent(SessionEventType.LogOn, new AesKey()), new ProgressContext());

            Assert.That(called, Is.True);
        }

        [Test]
        public static void TestHandleSessionEventLogOff()
        {
            MockFileSystemStateActions mock = new MockFileSystemStateActions(Instance.FileSystemState);
            bool called = false;
            mock.EncryptFilesInWatchedFoldersMock = (AesKey encryptionKey, ProgressContext progress) => { called = true; };

            FactoryRegistry.Instance.Register<FileSystemState, FileSystemStateActions>((fileSystemState) => mock);

            Instance.FileSystemState.Actions.HandleSessionEvent(new SessionEvent(SessionEventType.LogOff, new AesKey()), new ProgressContext());

            Assert.That(called, Is.True);
        }

        [Test]
        public static void TestHandleSessionEventThatCauseNoSpecificAction()
        {
            MockFileSystemStateActions mock = new MockFileSystemStateActions(Instance.FileSystemState);

            FactoryRegistry.Instance.Register<FileSystemState, FileSystemStateActions>((fileSystemState) => mock);

            Assert.DoesNotThrow(() =>
            {
                Instance.FileSystemState.Actions.HandleSessionEvent(new SessionEvent(SessionEventType.ProcessExit), new ProgressContext());
                Instance.FileSystemState.Actions.HandleSessionEvent(new SessionEvent(SessionEventType.SessionChange), new ProgressContext());
                Instance.FileSystemState.Actions.HandleSessionEvent(new SessionEvent(SessionEventType.KnownKeyChange), new ProgressContext());
                Instance.FileSystemState.Actions.HandleSessionEvent(new SessionEvent(SessionEventType.WorkFolderChange), new ProgressContext());
            });
        }

        [Test]
        public static void TestHandleSessionEventThatIsNotHandled()
        {
            MockFileSystemStateActions mock = new MockFileSystemStateActions(Instance.FileSystemState);

            FactoryRegistry.Instance.Register<FileSystemState, FileSystemStateActions>((fileSystemState) => mock);

            Assert.Throws<InvalidOperationException>(() =>
            {
                Instance.FileSystemState.Actions.HandleSessionEvent(new SessionEvent((SessionEventType)(-1)), new ProgressContext());
            });
        }

        [Test]
        public static void TestHandleSessionEvents()
        {
            MockAxCryptFile mock = new MockAxCryptFile();
            int callTimes = 0;
            mock.EncryptFilesUniqueWithBackupAndWipeMock = (IRuntimeFileInfo fileInfo, AesKey decryptionKey, ProgressContext progress) => { if (fileInfo.FullName == @"C:\My Documents\") ++callTimes; };
            FactoryRegistry.Instance.Register<AxCryptFile>(() => mock);

            List<SessionEvent> sessionEvents = new List<SessionEvent>();
            sessionEvents.Add(new SessionEvent(SessionEventType.WatchedFolderAdded, new AesKey(), @"C:\My Documents\"));
            sessionEvents.Add(new SessionEvent(SessionEventType.WatchedFolderAdded, new AesKey(), @"C:\My Documents\"));

            Instance.FileSystemState.Actions.HandleSessionEvents(sessionEvents, new ProgressContext());
            Assert.That(callTimes, Is.EqualTo(2));
        }
    }
}