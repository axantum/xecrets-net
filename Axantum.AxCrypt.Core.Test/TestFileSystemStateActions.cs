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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Core.UI;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestFileSystemStateActions
    {
        private static readonly string _pathRoot = Path.GetPathRoot(Environment.CurrentDirectory);
        private static readonly string _documentsFolder = _pathRoot.PathCombine("Documents");
        private static readonly string _decryptedFile1 = _documentsFolder.PathCombine("test.txt");
        private static readonly string _encryptedFile1 = _documentsFolder.PathCombine("Uncompressed.axx");
        private static readonly string _fileSystemStateFilePath = Path.Combine(Path.GetTempPath(), "DummyFileSystemState.xml");

        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();

            Factory.Instance.Singleton<FileSystemState>(() => FileSystemState.Create(Factory.New<IRuntimeFileInfo>(_fileSystemStateFilePath)));
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

            ActiveFile activeFile = new ActiveFile(Factory.New<IRuntimeFileInfo>(_encryptedFile1), Factory.New<IRuntimeFileInfo>(_decryptedFile1), new GenericPassphrase("passphrase"), ActiveFileStatus.NotDecrypted);
            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(10); });
            bool changedWasRaised = false;
            Instance.FileSystemState.Add(activeFile);
            Instance.FileSystemState.ActiveFileChanged += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });
            Factory.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            Assert.That(changedWasRaised, Is.True, "The file should be detected as decrypted being created.");
        }

        [Test]
        public static void TestCheckActiveFilesIsLocked()
        {
            DateTime utcNow = OS.Current.UtcNow;
            DateTime utcYesterday = utcNow.AddDays(-1);
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcYesterday, utcYesterday, utcYesterday, Stream.Null);

            ActiveFile activeFile = new ActiveFile(Factory.New<IRuntimeFileInfo>(_encryptedFile1), Factory.New<IRuntimeFileInfo>(_decryptedFile1), new GenericPassphrase("passphrase"), ActiveFileStatus.NotDecrypted);
            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(10); });
            bool changedWasRaised = false;
            Instance.FileSystemState.Add(activeFile);
            Instance.FileSystemState.ActiveFileChanged += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });
            using (FileLock fileLock = FileLock.Lock(activeFile.EncryptedFileInfo))
            {
                Factory.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            }
            Assert.That(changedWasRaised, Is.False, "The file should be not be detected as decrypted being created because the encrypted file is locked.");
            using (FileLock fileLock = FileLock.Lock(activeFile.DecryptedFileInfo))
            {
                Factory.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
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

            Mock<AxCryptFactory> axCryptFactoryMock = new Mock<AxCryptFactory>();
            axCryptFactoryMock.Setup<IPassphrase>(m => m.CreatePassphrase(It.IsAny<string>(), It.IsAny<IRuntimeFileInfo>())).Returns((string passphrase, IRuntimeFileInfo fileInfo) =>
            {
                return new V1Passphrase(passphrase);
            });
            Factory.Instance.Register<AxCryptFactory>(() => axCryptFactoryMock.Object);

            ActiveFile activeFile;
            activeFile = new ActiveFile(Factory.New<IRuntimeFileInfo>(_encryptedFile1), Factory.New<IRuntimeFileInfo>(_decryptedFile1), new V1Passphrase("passphrase"), ActiveFileStatus.AssumedOpenAndDecrypted);
            Instance.FileSystemState.Add(activeFile);
            Instance.KnownKeys.Add(activeFile.Key);

            IRuntimeFileInfo decryptedFileInfo = Factory.New<IRuntimeFileInfo>(_decryptedFile1);
            decryptedFileInfo.SetFileTimes(utcNow, utcNow, utcNow);

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Instance.FileSystemState.ActiveFileChanged += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });
            FakeRuntimeEnvironment.Instance.TimeFunction = () => DateTime.UtcNow;
            Factory.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
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
            V1Passphrase passphrase = new V1Passphrase("a");
            Factory.New<AxCryptFile>().Decrypt(Factory.New<IRuntimeFileInfo>(_encryptedFile1), Factory.New<IRuntimeFileInfo>(_decryptedFile1), passphrase, AxCryptOptions.None, new ProgressContext());

            ActiveFile activeFile = new ActiveFile(Factory.New<IRuntimeFileInfo>(_encryptedFile1), Factory.New<IRuntimeFileInfo>(_decryptedFile1), passphrase, ActiveFileStatus.AssumedOpenAndDecrypted);
            Instance.FileSystemState.Add(activeFile);
            Instance.FileSystemState.Save();

            Factory.Instance.Singleton<FileSystemState>(() => FileSystemState.Create(Factory.New<IRuntimeFileInfo>(_fileSystemStateFilePath)));

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Instance.FileSystemState.ActiveFileChanged += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });

            activeFile = Instance.FileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Key, Is.Null, "The key should be null after loading of new FileSystemState");

            Instance.KnownKeys.Add(passphrase);
            Factory.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            Assert.That(changedWasRaised, Is.True, "The ActiveFile should be modified because there is now a known key.");

            activeFile = Instance.FileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Key, Is.Not.Null, "The key should not be null after the checking of active files.");
        }

        [Test]
        public static void TestCheckActiveFilesKeyIsNotSetWithoutKnownKey()
        {
            DateTime utcNow = OS.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.helloworld_key_a_txt));
            V1Passphrase passphrase = new V1Passphrase("a");
            Factory.New<AxCryptFile>().Decrypt(Factory.New<IRuntimeFileInfo>(_encryptedFile1), Factory.New<IRuntimeFileInfo>(_decryptedFile1), passphrase, AxCryptOptions.None, new ProgressContext());

            ActiveFile activeFile = new ActiveFile(Factory.New<IRuntimeFileInfo>(_encryptedFile1), Factory.New<IRuntimeFileInfo>(_decryptedFile1), passphrase, ActiveFileStatus.AssumedOpenAndDecrypted);
            Instance.FileSystemState.Add(activeFile);
            Instance.FileSystemState.Save();

            Factory.Instance.Singleton<FileSystemState>(() => FileSystemState.Create(Factory.New<IRuntimeFileInfo>(_fileSystemStateFilePath)));

            IRuntimeFileInfo decryptedFileInfo = Factory.New<IRuntimeFileInfo>(_decryptedFile1);
            decryptedFileInfo.SetFileTimes(utcNow.AddSeconds(30), utcNow.AddSeconds(30), utcNow.AddSeconds(30));

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Instance.FileSystemState.ActiveFileChanged += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });

            activeFile = Instance.FileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Key, Is.Null, "The key should be null after loading of new FileSystemState");

            Factory.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            Assert.That(changedWasRaised, Is.False, "The ActiveFile should be not be modified because the file was modified as well and thus cannot be deleted.");

            Instance.KnownKeys.Add(new V1Passphrase("x"));
            Instance.KnownKeys.Add(new V1Passphrase("y"));
            Factory.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            Assert.That(changedWasRaised, Is.False, "The ActiveFile should be not be modified because the file was modified as well and thus cannot be deleted.");

            activeFile = Instance.FileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Key, Is.Null, "The key should still be null after the checking of active files.");

            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted), Is.True, "The file should still be there.");
            Assert.That(activeFile.ThumbprintMatch(passphrase), Is.True, "The active file should still be known to be decryptable with the original passphrase.");
        }

        [Test]
        public static void TestCheckActiveFilesNotDecryptedAndDoesNotExist()
        {
            DateTime utcNow = OS.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            ActiveFile activeFile = new ActiveFile(Factory.New<IRuntimeFileInfo>(_encryptedFile1), Factory.New<IRuntimeFileInfo>(_decryptedFile1), new GenericPassphrase("passphrase"), ActiveFileStatus.AssumedOpenAndDecrypted);
            Factory.New<IRuntimeFileInfo>(_decryptedFile1).Delete();
            activeFile = new ActiveFile(activeFile, ActiveFileStatus.NotDecrypted);
            Instance.FileSystemState.Add(activeFile);
            Instance.KnownKeys.Add(activeFile.Key);

            bool changedWasRaised = false;
            Instance.FileSystemState.ActiveFileChanged += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            Factory.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());

            Assert.That(changedWasRaised, Is.False, "The ActiveFile should be not be modified because it's already deleted.");
        }

        [Test]
        public static void TestCheckActiveFilesNoDeleteWhenNotDesktopWindows()
        {
            DateTime utcNow = OS.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            ActiveFile activeFile = new ActiveFile(Factory.New<IRuntimeFileInfo>(_encryptedFile1), Factory.New<IRuntimeFileInfo>(_decryptedFile1), new GenericPassphrase("passphrase"), ActiveFileStatus.AssumedOpenAndDecrypted);
            Instance.FileSystemState.Add(activeFile);
            Instance.KnownKeys.Add(activeFile.Key);

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Instance.FileSystemState.ActiveFileChanged += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });

            SetupAssembly.FakeRuntimeEnvironment.Platform = Platform.Unknown;
            Factory.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            Assert.That(changedWasRaised, Is.False, "No change should be raised when the file is not modified and not Desktop Windows.");
            activeFile = Instance.FileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted), Is.True, "Nothing should happen with the file when not running as Desktop Windows.");

            SetupAssembly.FakeRuntimeEnvironment.Platform = Platform.WindowsDesktop;
            changedWasRaised = false;
            Factory.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            Assert.That(changedWasRaised, Is.True, "Since the file should be deleted because running as Desktop Windows the changed event should be raised.");
            activeFile = Instance.FileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotDecrypted), Is.True, "The file should be deleted and marked as Not Decrypted when running as Desktop Windows.");
        }

        [Test]
        public static void TestCheckActiveFilesUpdateButWithTargetLockedForSharing()
        {
            DateTime utcNow = OS.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.helloworld_key_a_txt));
            V1Passphrase passphrase = new V1Passphrase("a");
            Factory.New<AxCryptFile>().Decrypt(Factory.New<IRuntimeFileInfo>(_encryptedFile1), Factory.New<IRuntimeFileInfo>(_decryptedFile1), passphrase, AxCryptOptions.None, new ProgressContext());

            ActiveFile activeFile = new ActiveFile(Factory.New<IRuntimeFileInfo>(_encryptedFile1), Factory.New<IRuntimeFileInfo>(_decryptedFile1), passphrase, ActiveFileStatus.AssumedOpenAndDecrypted);
            Instance.FileSystemState.Add(activeFile);

            IRuntimeFileInfo decryptedFileInfo = Factory.New<IRuntimeFileInfo>(_decryptedFile1);
            decryptedFileInfo.SetFileTimes(utcNow.AddSeconds(30), utcNow.AddSeconds(30), utcNow.AddSeconds(30));

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Instance.FileSystemState.ActiveFileChanged += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });

            Instance.KnownKeys.Add(passphrase);

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
                Factory.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
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
            ActiveFile activeFile = new ActiveFile(Factory.New<IRuntimeFileInfo>(_encryptedFile1), Factory.New<IRuntimeFileInfo>(_decryptedFile1), new GenericPassphrase("passphrase"), ActiveFileStatus.NotDecrypted);
            activeFile = new ActiveFile(activeFile, ActiveFileStatus.AssumedOpenAndDecrypted);
            Instance.FileSystemState.Add(activeFile, fakeLauncher);
            Instance.KnownKeys.Add(activeFile.Key);

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Instance.FileSystemState.ActiveFileChanged += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });
            SetupAssembly.FakeRuntimeEnvironment.Platform = Platform.WindowsDesktop;
            Factory.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());

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
            ActiveFile activeFile = new ActiveFile(Factory.New<IRuntimeFileInfo>(_encryptedFile1), Factory.New<IRuntimeFileInfo>(_decryptedFile1), new GenericPassphrase("passphrase"), ActiveFileStatus.NotDecrypted);
            activeFile = new ActiveFile(activeFile, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable);
            Instance.FileSystemState.Add(activeFile, fakeLauncher);

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Instance.FileSystemState.ActiveFileChanged += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });
            SetupAssembly.FakeRuntimeEnvironment.Platform = Platform.WindowsDesktop;
            fakeLauncher.HasExited = true;
            Factory.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());

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

            ActiveFile activeFile = new ActiveFile(Factory.New<IRuntimeFileInfo>(_encryptedFile1), Factory.New<IRuntimeFileInfo>(_decryptedFile1), new GenericPassphrase("passphrase"), ActiveFileStatus.AssumedOpenAndDecrypted);
            Instance.FileSystemState.Add(activeFile);

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Instance.FileSystemState.ActiveFileChanged += ((object sender, ActiveFileChangedEventArgs e) =>
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
            FakeRuntimeFileInfo.Moving += eventHandler;
            FakeRuntimeFileInfo.Deleting += eventHandler;
            FakeRuntimeFileInfo.OpeningForWrite += eventHandler;
            try
            {
                Factory.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
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

            IRuntimeFileInfo encryptedFileInfo = Factory.New<IRuntimeFileInfo>(_encryptedFile1);
            IRuntimeFileInfo decryptedFileInfo = Factory.New<IRuntimeFileInfo>(_decryptedFile1);
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, new GenericPassphrase("passphrase"), ActiveFileStatus.AssumedOpenAndDecrypted);
            Instance.FileSystemState.Add(activeFile);

            bool changedWasRaised = false;
            Instance.FileSystemState.ActiveFileChanged += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });

            using (FileLock fileLock = FileLock.Lock(decryptedFileInfo))
            {
                Factory.New<ActiveFileAction>().PurgeActiveFiles(new ProgressContext());
            }

            Assert.That(changedWasRaised, Is.False, "A changed event should not be raised because the decrypted file is locked.");
        }

        [Test]
        public static void TestPurgeActiveFilesWhenFileIsModified()
        {
            DateTime utcNow = OS.Current.UtcNow;
            FakeRuntimeFileInfo.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeRuntimeFileInfo.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            Mock<AxCryptFactory> axCryptFactoryMock = new Mock<AxCryptFactory>();
            axCryptFactoryMock.Setup<IPassphrase>(m => m.CreatePassphrase(It.IsAny<string>(), It.IsAny<IRuntimeFileInfo>())).Returns((string passphrase, IRuntimeFileInfo fileInfo) =>
            {
                return new V1Passphrase(passphrase);
            });
            Factory.Instance.Register<AxCryptFactory>(() => axCryptFactoryMock.Object);

            IRuntimeFileInfo encryptedFileInfo = Factory.New<IRuntimeFileInfo>(_encryptedFile1);
            IRuntimeFileInfo decryptedFileInfo = Factory.New<IRuntimeFileInfo>(_decryptedFile1);
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, new V2Passphrase("passphrase", 256), ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable);
            Instance.FileSystemState.Add(activeFile);

            int timeCalls = 0;
            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1).AddMilliseconds(100 * timeCalls++); });
            DateTime utcLater = OS.Current.UtcNow;

            decryptedFileInfo.SetFileTimes(utcLater, utcLater, utcLater);

            bool changedWasRaised = false;
            Instance.FileSystemState.ActiveFileChanged += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });

            Factory.New<ActiveFileAction>().PurgeActiveFiles(new ProgressContext());

            activeFile = Instance.FileSystemState.FindEncryptedPath(_encryptedFile1);
            Assert.That(changedWasRaised, Is.True, "A changed event should be raised because the decrypted file is modified.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotDecrypted), Is.True, "The NotShareable not withstanding, the purge should have updated the file and removed the decrypted file.");
        }

        [Test]
        public static void TestUpdateActiveFileWithKeyIfKeyMatchesThumbprintWithKnownKey()
        {
            IRuntimeFileInfo encryptedFileInfo = Factory.New<IRuntimeFileInfo>(_encryptedFile1);
            IRuntimeFileInfo decryptedFileInfo = Factory.New<IRuntimeFileInfo>(_decryptedFile1);
            IPassphrase key = new GenericPassphrase("passphrase");
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable);
            Instance.FileSystemState.Add(activeFile);

            bool updateWasMade = Factory.New<ActiveFileAction>().UpdateActiveFileWithKeyIfKeyMatchesThumbprint(key);
            Assert.That(updateWasMade, Is.False, "Since there are only ActiveFiles with known keys in the list, no update should be made.");
        }

        [Test]
        public static void TestUpdateActiveFileWithKeyIfKeyMatchesThumbprintWithWrongThumbprint()
        {
            IRuntimeFileInfo encryptedFileInfo = Factory.New<IRuntimeFileInfo>(_encryptedFile1);
            IRuntimeFileInfo decryptedFileInfo = Factory.New<IRuntimeFileInfo>(_decryptedFile1);
            IPassphrase key = new GenericPassphrase("a");
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable);
            Instance.FileSystemState.Add(activeFile);
            Instance.FileSystemState.Save();

            Factory.Instance.Singleton<FileSystemState>(() => FileSystemState.Create(Factory.New<IRuntimeFileInfo>(_fileSystemStateFilePath)));

            IPassphrase wrongKey = new GenericPassphrase("b");
            bool updateWasMade = Factory.New<ActiveFileAction>().UpdateActiveFileWithKeyIfKeyMatchesThumbprint(wrongKey);
            Assert.That(updateWasMade, Is.False, "Since there are only ActiveFiles with wrong keys in the list, no update should be made.");
        }

        [Test]
        public static void TestUpdateActiveFileWithKeyIfKeyMatchesThumbprintWithMatchingThumbprint()
        {
            IRuntimeFileInfo encryptedFileInfo = Factory.New<IRuntimeFileInfo>(_encryptedFile1);
            IRuntimeFileInfo decryptedFileInfo = Factory.New<IRuntimeFileInfo>(_decryptedFile1);
            IPassphrase key = new GenericPassphrase("passphrase");
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable);
            Instance.FileSystemState.Add(activeFile);
            Instance.FileSystemState.Save();

            Factory.Instance.Singleton<FileSystemState>(() => FileSystemState.Create(Factory.New<IRuntimeFileInfo>(_fileSystemStateFilePath)));

            bool updateWasMade = Factory.New<ActiveFileAction>().UpdateActiveFileWithKeyIfKeyMatchesThumbprint(key);
            Assert.That(updateWasMade, Is.True, "Since there is an ActiveFile with the right thumbprint in the list, an update should be made.");
        }

        [Test]
        public static void TestUpdateActiveFileButWithNoChangeDueToIrrelevantStatus()
        {
            IRuntimeFileInfo encryptedFileInfo = Factory.New<IRuntimeFileInfo>(_encryptedFile1);
            IRuntimeFileInfo decryptedFileInfo = Factory.New<IRuntimeFileInfo>(_decryptedFile1);
            IPassphrase key = new GenericPassphrase("passphrase");
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.None);
            Instance.FileSystemState.Add(activeFile);

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return DateTime.UtcNow.AddMinutes(1); });

            bool somethingWasChanged = false;
            Instance.FileSystemState.ActiveFileChanged += (object sender, ActiveFileChangedEventArgs e) =>
                {
                    somethingWasChanged = true;
                };
            Factory.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            Assert.That(somethingWasChanged, Is.False, "No event should be raised, because nothing should change.");
        }

        [Test]
        public static void TestUpdateActiveFileWithEventRaisedSinceItAppearsAProcessHasExited()
        {
            IRuntimeFileInfo encryptedFileInfo = Factory.New<IRuntimeFileInfo>(_encryptedFile1);
            IRuntimeFileInfo decryptedFileInfo = Factory.New<IRuntimeFileInfo>(_decryptedFile1);
            IPassphrase key = new GenericPassphrase("passphrase");
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.NotShareable);
            Instance.FileSystemState.Add(activeFile);

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return DateTime.UtcNow.AddMinutes(1); });

            bool somethingWasChanged = false;
            Instance.FileSystemState.ActiveFileChanged += (object sender, ActiveFileChangedEventArgs e) =>
            {
                somethingWasChanged = true;
            };
            Factory.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            Assert.That(somethingWasChanged, Is.True, "An event should be raised, because status was NotShareable, but no process is active.");
        }

        [Test]
        public static void TestRemoveRecentFile()
        {
            IRuntimeFileInfo encryptedFileInfo = Factory.New<IRuntimeFileInfo>(_encryptedFile1);
            IRuntimeFileInfo decryptedFileInfo = Factory.New<IRuntimeFileInfo>(_decryptedFile1);
            IPassphrase key = new GenericPassphrase("passphrase");
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable);
            Instance.FileSystemState.Add(activeFile);
            Instance.FileSystemState.Save();

            ActiveFile beforeRemoval = Instance.FileSystemState.FindEncryptedPath(encryptedFileInfo.FullName);
            Assert.That(beforeRemoval, Is.Not.Null, "Before being removed, the ActiveFile should be possible to find.");

            Factory.New<ActiveFileAction>().RemoveRecentFiles(new IRuntimeFileInfo[] { Factory.New<IRuntimeFileInfo>(encryptedFileInfo.FullName) }, new ProgressContext());

            ActiveFile afterRemoval = Instance.FileSystemState.FindEncryptedPath(encryptedFileInfo.FullName);
            Assert.That(afterRemoval, Is.Null, "After being removed, the ActiveFile should not be possible to find.");
        }

        [Test]
        public static void TestRemoveRecentFileWhenFileDoesNotExist()
        {
            IRuntimeFileInfo encryptedFileInfo = Factory.New<IRuntimeFileInfo>(_encryptedFile1);
            IRuntimeFileInfo decryptedFileInfo = Factory.New<IRuntimeFileInfo>(_decryptedFile1);
            IPassphrase key = new GenericPassphrase("passphrase");
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable);
            Instance.FileSystemState.Add(activeFile);
            Instance.FileSystemState.Save();

            ActiveFile beforeRemoval = Instance.FileSystemState.FindEncryptedPath(encryptedFileInfo.FullName);
            Assert.That(beforeRemoval, Is.Not.Null, "Before being removed, the ActiveFile should be possible to find.");

            Assert.DoesNotThrow(() => { Factory.New<ActiveFileAction>().RemoveRecentFiles(new IRuntimeFileInfo[] { Factory.New<IRuntimeFileInfo>(encryptedFileInfo.FullName + ".notfound") }, new ProgressContext()); });

            ActiveFile afterFailedRemoval = Instance.FileSystemState.FindEncryptedPath(encryptedFileInfo.FullName);
            Assert.That(afterFailedRemoval, Is.Not.Null, "After failed removal, the ActiveFile should still be possible to find.");
        }
    }
}