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
using System.Collections.Generic;
using System.IO;
using System.Linq;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestFileSystemStateActions
    {
        private static readonly string _pathRoot = Path.GetPathRoot(Environment.CurrentDirectory);
        private static readonly string _documentsFolder = _pathRoot.PathCombine("Documents");
        private static readonly string _decryptedFile1 = _documentsFolder.PathCombine("test.txt");
        private static readonly string _encryptedFile1 = _documentsFolder.PathCombine("Uncompressed.axx");
        private static readonly string _fileSystemStateFilePath = Path.Combine(Path.GetTempPath(), "DummyFileSystemState.xml");

        private CryptoImplementation _cryptoImplementation;

        public TestFileSystemStateActions(CryptoImplementation cryptoImplementation)
        {
            _cryptoImplementation = cryptoImplementation;
        }

        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup();
            SetupAssembly.AssemblySetupCrypto(_cryptoImplementation);

            TypeMap.Register.Singleton<FileSystemState>(() => FileSystemState.Create(TypeMap.Resolve.New<IDataStore>(_fileSystemStateFilePath)));
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public void TestCheckActiveFilesIsNotLocked()
        {
            DateTime utcNow = OS.Current.UtcNow;
            DateTime utcYesterday = utcNow.AddDays(-1);
            FakeDataStore.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeDataStore.AddFile(_decryptedFile1, utcYesterday, utcYesterday, utcYesterday, Stream.Null);

            ActiveFile activeFile = new ActiveFile(TypeMap.Resolve.New<IDataStore>(_encryptedFile1), TypeMap.Resolve.New<IDataStore>(_decryptedFile1), new LogOnIdentity("passphrase"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().Id);
            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(10); });
            bool changedWasRaised = false;
            Resolve.FileSystemState.Add(activeFile);
            Resolve.FileSystemState.ActiveFileChanged += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });
            TypeMap.Resolve.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            Assert.That(changedWasRaised, Is.True, "The file should be detected as decrypted being created.");
        }

        [Test]
        public void TestCheckActiveFilesIsLocked()
        {
            DateTime utcNow = OS.Current.UtcNow;
            DateTime utcYesterday = utcNow.AddDays(-1);
            FakeDataStore.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeDataStore.AddFile(_decryptedFile1, utcYesterday, utcYesterday, utcYesterday, Stream.Null);

            ActiveFile activeFile = new ActiveFile(TypeMap.Resolve.New<IDataStore>(_encryptedFile1), TypeMap.Resolve.New<IDataStore>(_decryptedFile1), new LogOnIdentity("passphrase"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().Id);
            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(10); });
            bool changedWasRaised = false;
            Resolve.FileSystemState.Add(activeFile);
            Resolve.FileSystemState.ActiveFileChanged += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });
            using (FileLock fileLock = FileLock.Lock(activeFile.EncryptedFileInfo))
            {
                TypeMap.Resolve.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            }
            Assert.That(changedWasRaised, Is.False, "The file should be not be detected as decrypted being created because the encrypted file is locked.");
            using (FileLock fileLock = FileLock.Lock(activeFile.DecryptedFileInfo))
            {
                TypeMap.Resolve.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            }
            Assert.That(changedWasRaised, Is.False, "The file should be not be detected as decrypted being created because the decrypted file is locked.");
        }

        [Test]
        public void TestCheckActiveFilesKeyIsSet()
        {
            DateTime utcNow = OS.Current.UtcNow;
            DateTime utcJustNow = utcNow.AddMinutes(-1);
            FakeDataStore.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeDataStore.AddFile(_decryptedFile1, utcJustNow, utcJustNow, utcJustNow, Stream.Null);

            Mock<AxCryptFactory> axCryptFactoryMock = new Mock<AxCryptFactory>() { CallBase = true };
            axCryptFactoryMock.Setup<DecryptionParameter>(m => m.FindDecryptionParameter(It.IsAny<IEnumerable<DecryptionParameter>>(), It.IsAny<IDataStore>())).Returns((IEnumerable<DecryptionParameter> decryptionParameters, IDataStore fileInfo) =>
            {
                return new DecryptionParameter(Passphrase.Empty, V1Aes128CryptoFactory.CryptoId);
            });
            TypeMap.Register.New<AxCryptFactory>(() => axCryptFactoryMock.Object);

            ActiveFile activeFile;
            activeFile = new ActiveFile(TypeMap.Resolve.New<IDataStore>(_encryptedFile1), TypeMap.Resolve.New<IDataStore>(_decryptedFile1), new LogOnIdentity("passphrase"), ActiveFileStatus.AssumedOpenAndDecrypted, new V1Aes128CryptoFactory().Id);
            Resolve.FileSystemState.Add(activeFile);
            Resolve.KnownKeys.Add(activeFile.Key);

            IDataStore decryptedFileInfo = TypeMap.Resolve.New<IDataStore>(_decryptedFile1);
            decryptedFileInfo.SetFileTimes(utcNow, utcNow, utcNow);

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Resolve.FileSystemState.ActiveFileChanged += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });
            FakeRuntimeEnvironment.Instance.TimeFunction = () => DateTime.UtcNow;
            TypeMap.Resolve.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            Assert.That(changedWasRaised, Is.True, "The file should be detected as modified, because it is considered open and decrypted, has a proper key, is modified, no running process so it should be re-encrypted and deleted.");
            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_encryptedFile1);
            Assert.That(activeFile, Is.Not.Null, "The encrypted file should be found.");
            Assert.That(activeFile.IsModified, Is.False, "The file should no longer be flagged as modified.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotDecrypted), Is.True, "The file should no longer be decrypted, since it was re-encrypted and deleted.");
        }

        [Test]
        public void TestCheckActiveFilesKeyIsNotSetWithKnownKey()
        {
            DateTime utcNow = OS.Current.UtcNow;
            FakeDataStore.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, FakeDataStore.ExpandableMemoryStream(Resources.helloworld_key_a_txt));
            LogOnIdentity passphrase = new LogOnIdentity("a");
            TypeMap.Resolve.New<AxCryptFile>().Decrypt(TypeMap.Resolve.New<IDataStore>(_encryptedFile1), TypeMap.Resolve.New<IDataStore>(_decryptedFile1), passphrase, AxCryptOptions.None, new ProgressContext());

            ActiveFile activeFile = new ActiveFile(TypeMap.Resolve.New<IDataStore>(_encryptedFile1), TypeMap.Resolve.New<IDataStore>(_decryptedFile1), passphrase, ActiveFileStatus.AssumedOpenAndDecrypted, new V1Aes128CryptoFactory().Id);
            Resolve.FileSystemState.Add(activeFile);
            Resolve.FileSystemState.Save();

            TypeMap.Register.Singleton<FileSystemState>(() => FileSystemState.Create(TypeMap.Resolve.New<IDataStore>(_fileSystemStateFilePath)));

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Resolve.FileSystemState.ActiveFileChanged += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });

            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Key, Is.Null, "The key should be null after loading of new FileSystemState");

            Resolve.KnownKeys.Add(passphrase);
            TypeMap.Resolve.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            Assert.That(changedWasRaised, Is.True, "The ActiveFile should be modified because there is now a known key.");

            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Key, Is.Not.Null, "The key should not be null after the checking of active files.");
        }

        [Test]
        public void TestCheckActiveFilesKeyIsNotSetWithoutKnownKey()
        {
            DateTime utcNow = OS.Current.UtcNow;
            FakeDataStore.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, FakeDataStore.ExpandableMemoryStream(Resources.helloworld_key_a_txt));
            LogOnIdentity passphrase = new LogOnIdentity("a");
            TypeMap.Resolve.New<AxCryptFile>().Decrypt(TypeMap.Resolve.New<IDataStore>(_encryptedFile1), TypeMap.Resolve.New<IDataStore>(_decryptedFile1), passphrase, AxCryptOptions.None, new ProgressContext());

            ActiveFile activeFile = new ActiveFile(TypeMap.Resolve.New<IDataStore>(_encryptedFile1), TypeMap.Resolve.New<IDataStore>(_decryptedFile1), passphrase, ActiveFileStatus.AssumedOpenAndDecrypted, new V1Aes128CryptoFactory().Id);
            Resolve.FileSystemState.Add(activeFile);
            Resolve.FileSystemState.Save();

            TypeMap.Register.Singleton<FileSystemState>(() => FileSystemState.Create(TypeMap.Resolve.New<IDataStore>(_fileSystemStateFilePath)));

            IDataStore decryptedFileInfo = TypeMap.Resolve.New<IDataStore>(_decryptedFile1);
            decryptedFileInfo.SetFileTimes(utcNow.AddSeconds(30), utcNow.AddSeconds(30), utcNow.AddSeconds(30));

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Resolve.FileSystemState.ActiveFileChanged += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });

            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Key, Is.Null, "The key should be null after loading of new FileSystemState");

            TypeMap.Resolve.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            Assert.That(changedWasRaised, Is.False, "The ActiveFile should be not be modified because the file was modified as well and thus cannot be deleted.");

            Resolve.KnownKeys.Add(new LogOnIdentity("x"));
            Resolve.KnownKeys.Add(new LogOnIdentity("y"));
            TypeMap.Resolve.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            Assert.That(changedWasRaised, Is.False, "The ActiveFile should be not be modified because the file was modified as well and thus cannot be deleted.");

            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Key, Is.Null, "The key should still be null after the checking of active files.");

            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted), Is.True, "The file should still be there.");
            Assert.That(activeFile.ThumbprintMatch(passphrase.Passphrase), Is.True, "The active file should still be known to be decryptable with the original passphrase.");
        }

        [Test]
        public void TestCheckActiveFilesNotDecryptedAndDoesNotExist()
        {
            DateTime utcNow = OS.Current.UtcNow;
            FakeDataStore.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeDataStore.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            ActiveFile activeFile = new ActiveFile(TypeMap.Resolve.New<IDataStore>(_encryptedFile1), TypeMap.Resolve.New<IDataStore>(_decryptedFile1), new LogOnIdentity("passphrase"), ActiveFileStatus.AssumedOpenAndDecrypted, new V1Aes128CryptoFactory().Id);
            TypeMap.Resolve.New<IDataStore>(_decryptedFile1).Delete();
            activeFile = new ActiveFile(activeFile, ActiveFileStatus.NotDecrypted);
            Resolve.FileSystemState.Add(activeFile);
            Resolve.KnownKeys.Add(activeFile.Key);

            bool changedWasRaised = false;
            Resolve.FileSystemState.ActiveFileChanged += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            TypeMap.Resolve.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());

            Assert.That(changedWasRaised, Is.False, "The ActiveFile should be not be modified because it's already deleted.");
        }

        [Test]
        public void TestCheckActiveFilesNoDeleteWhenNotDesktopWindows()
        {
            DateTime utcNow = OS.Current.UtcNow;
            FakeDataStore.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeDataStore.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            ActiveFile activeFile = new ActiveFile(TypeMap.Resolve.New<IDataStore>(_encryptedFile1), TypeMap.Resolve.New<IDataStore>(_decryptedFile1), new LogOnIdentity("passphrase"), ActiveFileStatus.AssumedOpenAndDecrypted, new V1Aes128CryptoFactory().Id);
            Resolve.FileSystemState.Add(activeFile);
            Resolve.KnownKeys.Add(activeFile.Key);

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Resolve.FileSystemState.ActiveFileChanged += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });

            SetupAssembly.FakeRuntimeEnvironment.Platform = Platform.Unknown;
            TypeMap.Resolve.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            Assert.That(changedWasRaised, Is.False, "No change should be raised when the file is not modified and not Desktop Windows.");
            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted), Is.True, "Nothing should happen with the file when not running as Desktop Windows.");

            SetupAssembly.FakeRuntimeEnvironment.Platform = Platform.WindowsDesktop;
            changedWasRaised = false;
            TypeMap.Resolve.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            Assert.That(changedWasRaised, Is.True, "Since the file should be deleted because running as Desktop Windows the changed event should be raised.");
            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotDecrypted), Is.True, "The file should be deleted and marked as Not Decrypted when running as Desktop Windows.");
        }

        [Test]
        public void TestCheckActiveFilesUpdateButWithTargetLockedForSharing()
        {
            DateTime utcNow = OS.Current.UtcNow;
            FakeDataStore.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, FakeDataStore.ExpandableMemoryStream(Resources.helloworld_key_a_txt));
            LogOnIdentity passphrase = new LogOnIdentity("a");
            TypeMap.Resolve.New<AxCryptFile>().Decrypt(TypeMap.Resolve.New<IDataStore>(_encryptedFile1), TypeMap.Resolve.New<IDataStore>(_decryptedFile1), passphrase, AxCryptOptions.None, new ProgressContext());

            ActiveFile activeFile = new ActiveFile(TypeMap.Resolve.New<IDataStore>(_encryptedFile1), TypeMap.Resolve.New<IDataStore>(_decryptedFile1), passphrase, ActiveFileStatus.AssumedOpenAndDecrypted, new V1Aes128CryptoFactory().Id);
            Resolve.FileSystemState.Add(activeFile);

            IDataStore decryptedFileInfo = TypeMap.Resolve.New<IDataStore>(_decryptedFile1);
            decryptedFileInfo.SetFileTimes(utcNow.AddSeconds(30), utcNow.AddSeconds(30), utcNow.AddSeconds(30));

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Resolve.FileSystemState.ActiveFileChanged += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });

            Resolve.KnownKeys.Add(passphrase);

            EventHandler eventHandler = ((object sender, EventArgs e) =>
            {
                FakeDataStore fileInfo = (FakeDataStore)sender;
                if (fileInfo.FullName == _decryptedFile1)
                {
                    throw new IOException("Faked sharing violation.");
                }
            });
            FakeDataStore.OpeningForRead += eventHandler;
            try
            {
                TypeMap.Resolve.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            }
            finally
            {
                FakeDataStore.OpeningForRead -= eventHandler;
            }

            Assert.That(changedWasRaised, Is.True, "The ActiveFile should be modified because it should now be marked as not shareable.");
            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_encryptedFile1);
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotShareable), Is.True, "The ActiveFile should be marked as not shareable after the checking of active files.");
        }

        [Test]
        public void TestTryDeleteButProcessHasNotExited()
        {
            DateTime utcNow = OS.Current.UtcNow;
            FakeDataStore.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeDataStore.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            FakeLauncher fakeLauncher = new FakeLauncher();
            fakeLauncher.Launch(_decryptedFile1);
            ActiveFile activeFile = new ActiveFile(TypeMap.Resolve.New<IDataStore>(_encryptedFile1), TypeMap.Resolve.New<IDataStore>(_decryptedFile1), new LogOnIdentity("passphrase"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().Id);
            activeFile = new ActiveFile(activeFile, ActiveFileStatus.AssumedOpenAndDecrypted);
            Resolve.FileSystemState.Add(activeFile, fakeLauncher);
            Resolve.KnownKeys.Add(activeFile.Key);

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Resolve.FileSystemState.ActiveFileChanged += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });
            SetupAssembly.FakeRuntimeEnvironment.Platform = Platform.WindowsDesktop;
            TypeMap.Resolve.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());

            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_encryptedFile1);
            Assert.That(changedWasRaised, Is.False, "No changed event should be raised because no change should occur since the process is active.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted), Is.True, "The ActiveFile plain text should not be deleted after the checking of active files because the launcher is active.");
        }

        [Test]
        public void TestCheckProcessExitedWhenExited()
        {
            DateTime utcNow = OS.Current.UtcNow;
            FakeDataStore.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeDataStore.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            FakeLauncher fakeLauncher = new FakeLauncher();
            fakeLauncher.Launch(_decryptedFile1);
            ActiveFile activeFile = new ActiveFile(TypeMap.Resolve.New<IDataStore>(_encryptedFile1), TypeMap.Resolve.New<IDataStore>(_decryptedFile1), new LogOnIdentity("passphrase"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().Id);
            activeFile = new ActiveFile(activeFile, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable);
            Resolve.FileSystemState.Add(activeFile, fakeLauncher);

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Resolve.FileSystemState.ActiveFileChanged += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });
            SetupAssembly.FakeRuntimeEnvironment.Platform = Platform.WindowsDesktop;
            fakeLauncher.HasExited = true;
            TypeMap.Resolve.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());

            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_encryptedFile1);
            Assert.That(changedWasRaised, Is.True, "A changed event should be raised because the process has exited.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotDecrypted), Is.True, "The ActiveFile plain text should be deleted after the checking of active files because the launcher is no longer active.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotShareable), Is.False, "The file should be shareable after checking of active files because the launcher is no longer active.");
        }

        [Test]
        public void TestTryDeleteButDecryptedSharingLocked()
        {
            DateTime utcNow = OS.Current.UtcNow;
            FakeDataStore.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeDataStore.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            ActiveFile activeFile = new ActiveFile(TypeMap.Resolve.New<IDataStore>(_encryptedFile1), TypeMap.Resolve.New<IDataStore>(_decryptedFile1), new LogOnIdentity("passphrase"), ActiveFileStatus.AssumedOpenAndDecrypted, new V1Aes128CryptoFactory().Id);
            Resolve.FileSystemState.Add(activeFile);

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Resolve.FileSystemState.ActiveFileChanged += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });
            SetupAssembly.FakeRuntimeEnvironment.Platform = Platform.WindowsDesktop;

            EventHandler eventHandler = ((object sender, EventArgs e) =>
            {
                FakeDataStore fileInfo = (FakeDataStore)sender;
                if (fileInfo.FullName == _decryptedFile1)
                {
                    throw new IOException("Faked sharing violation.");
                }
            });
            FakeDataStore.Moving += eventHandler;
            FakeDataStore.Deleting += eventHandler;
            FakeDataStore.OpeningForWrite += eventHandler;
            try
            {
                TypeMap.Resolve.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            }
            finally
            {
                FakeDataStore.Deleting -= eventHandler;
                FakeDataStore.OpeningForWrite -= eventHandler;
                FakeDataStore.Moving -= eventHandler;
            }

            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_encryptedFile1);
            Assert.That(changedWasRaised, Is.True, "A changed event should be raised because it should now be NotShareable.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted), Is.True, "The ActiveFile plain text should still be there after the checking of active files because the file is NotShareable.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotShareable), Is.True, "The ActiveFile plain text should be NotShareable after the checking of active files because the file could not be deleted.");
        }

        [Test]
        public void TestPurgeActiveFilesWhenFileIsLocked()
        {
            DateTime utcNow = OS.Current.UtcNow;
            FakeDataStore.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeDataStore.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            IDataStore encryptedFileInfo = TypeMap.Resolve.New<IDataStore>(_encryptedFile1);
            IDataStore decryptedFileInfo = TypeMap.Resolve.New<IDataStore>(_decryptedFile1);
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, new LogOnIdentity("passphrase"), ActiveFileStatus.AssumedOpenAndDecrypted, new V1Aes128CryptoFactory().Id);
            Resolve.FileSystemState.Add(activeFile);

            bool changedWasRaised = false;
            Resolve.FileSystemState.ActiveFileChanged += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });

            using (FileLock fileLock = FileLock.Lock(decryptedFileInfo))
            {
                TypeMap.Resolve.New<ActiveFileAction>().PurgeActiveFiles(new ProgressContext());
            }

            Assert.That(changedWasRaised, Is.False, "A changed event should not be raised because the decrypted file is locked.");
        }

        [Test]
        public void TestPurgeActiveFilesWhenFileIsModified()
        {
            DateTime utcNow = OS.Current.UtcNow;
            FakeDataStore.AddFile(_encryptedFile1, utcNow, utcNow, utcNow, Stream.Null);
            FakeDataStore.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            Mock<AxCryptFactory> axCryptFactoryMock = new Mock<AxCryptFactory>() { CallBase = true };
            axCryptFactoryMock.Setup<DecryptionParameter>(m => m.FindDecryptionParameter(It.IsAny<IEnumerable<DecryptionParameter>>(), It.IsAny<IDataStore>())).Returns((IEnumerable<DecryptionParameter> decryptionParameters, IDataStore fileInfo) =>
            {
                return new DecryptionParameter(Passphrase.Empty, V1Aes128CryptoFactory.CryptoId);
            });
            TypeMap.Register.New<AxCryptFactory>(() => axCryptFactoryMock.Object);

            IDataStore encryptedFileInfo = TypeMap.Resolve.New<IDataStore>(_encryptedFile1);
            IDataStore decryptedFileInfo = TypeMap.Resolve.New<IDataStore>(_decryptedFile1);
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, new LogOnIdentity("passphrase"), ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable, new V1Aes128CryptoFactory().Id);
            Resolve.FileSystemState.Add(activeFile);

            int timeCalls = 0;
            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return utcNow.AddMinutes(1).AddMilliseconds(100 * timeCalls++); });
            DateTime utcLater = OS.Current.UtcNow;

            decryptedFileInfo.SetFileTimes(utcLater, utcLater, utcLater);

            bool changedWasRaised = false;
            Resolve.FileSystemState.ActiveFileChanged += ((object sender, ActiveFileChangedEventArgs e) =>
            {
                changedWasRaised = true;
            });

            TypeMap.Resolve.New<ActiveFileAction>().PurgeActiveFiles(new ProgressContext());

            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_encryptedFile1);
            Assert.That(changedWasRaised, Is.True, "A changed event should be raised because the decrypted file is modified.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotDecrypted), Is.True, "The NotShareable not withstanding, the purge should have updated the file and removed the decrypted file.");
        }

        [Test]
        public void TestUpdateActiveFileWithKeyIfKeyMatchesThumbprintWithKnownKey()
        {
            IDataStore encryptedFileInfo = TypeMap.Resolve.New<IDataStore>(_encryptedFile1);
            IDataStore decryptedFileInfo = TypeMap.Resolve.New<IDataStore>(_decryptedFile1);
            LogOnIdentity key = new LogOnIdentity("passphrase");
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable, new V1Aes128CryptoFactory().Id);
            Resolve.FileSystemState.Add(activeFile);

            bool updateWasMade = TypeMap.Resolve.New<ActiveFileAction>().UpdateActiveFileWithKeyIfKeyMatchesThumbprint(key);
            Assert.That(updateWasMade, Is.False, "Since there are only ActiveFiles with known keys in the list, no update should be made.");
        }

        [Test]
        public void TestUpdateActiveFileWithKeyIfKeyMatchesThumbprintWithWrongThumbprint()
        {
            IDataStore encryptedFileInfo = TypeMap.Resolve.New<IDataStore>(_encryptedFile1);
            IDataStore decryptedFileInfo = TypeMap.Resolve.New<IDataStore>(_decryptedFile1);
            LogOnIdentity key = new LogOnIdentity("a");
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable, new V1Aes128CryptoFactory().Id);
            Resolve.FileSystemState.Add(activeFile);
            Resolve.FileSystemState.Save();

            TypeMap.Register.Singleton<FileSystemState>(() => FileSystemState.Create(TypeMap.Resolve.New<IDataStore>(_fileSystemStateFilePath)));

            LogOnIdentity wrongKey = new LogOnIdentity("b");
            bool updateWasMade = TypeMap.Resolve.New<ActiveFileAction>().UpdateActiveFileWithKeyIfKeyMatchesThumbprint(wrongKey);
            Assert.That(updateWasMade, Is.False, "Since there are only ActiveFiles with wrong keys in the list, no update should be made.");
        }

        [Test]
        public void TestUpdateActiveFileWithKeyIfKeyMatchesThumbprintWithMatchingThumbprint()
        {
            IDataStore encryptedFileInfo = TypeMap.Resolve.New<IDataStore>(_encryptedFile1);
            IDataStore decryptedFileInfo = TypeMap.Resolve.New<IDataStore>(_decryptedFile1);
            LogOnIdentity key = new LogOnIdentity("passphrase");
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable, new V1Aes128CryptoFactory().Id);
            Resolve.FileSystemState.Add(activeFile);
            Resolve.FileSystemState.Save();

            TypeMap.Register.Singleton<FileSystemState>(() => FileSystemState.Create(TypeMap.Resolve.New<IDataStore>(_fileSystemStateFilePath)));

            bool updateWasMade = TypeMap.Resolve.New<ActiveFileAction>().UpdateActiveFileWithKeyIfKeyMatchesThumbprint(key);
            Assert.That(updateWasMade, Is.True, "Since there is an ActiveFile with the right thumbprint in the list, an update should be made.");
        }

        [Test]
        public void TestUpdateActiveFileButWithNoChangeDueToIrrelevantStatus()
        {
            IDataStore encryptedFileInfo = TypeMap.Resolve.New<IDataStore>(_encryptedFile1);
            IDataStore decryptedFileInfo = TypeMap.Resolve.New<IDataStore>(_decryptedFile1);
            LogOnIdentity key = new LogOnIdentity("passphrase");
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.None, new V1Aes128CryptoFactory().Id);
            Resolve.FileSystemState.Add(activeFile);

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return DateTime.UtcNow.AddMinutes(1); });

            bool somethingWasChanged = false;
            Resolve.FileSystemState.ActiveFileChanged += (object sender, ActiveFileChangedEventArgs e) =>
                {
                    somethingWasChanged = true;
                };
            TypeMap.Resolve.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            Assert.That(somethingWasChanged, Is.False, "No event should be raised, because nothing should change.");
        }

        [Test]
        public void TestUpdateActiveFileWithEventRaisedSinceItAppearsAProcessHasExited()
        {
            IDataStore encryptedFileInfo = TypeMap.Resolve.New<IDataStore>(_encryptedFile1);
            IDataStore decryptedFileInfo = TypeMap.Resolve.New<IDataStore>(_decryptedFile1);
            LogOnIdentity key = new LogOnIdentity("passphrase");
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.NotShareable, new V1Aes128CryptoFactory().Id);
            Resolve.FileSystemState.Add(activeFile);

            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return DateTime.UtcNow.AddMinutes(1); });

            bool somethingWasChanged = false;
            Resolve.FileSystemState.ActiveFileChanged += (object sender, ActiveFileChangedEventArgs e) =>
            {
                somethingWasChanged = true;
            };
            TypeMap.Resolve.New<ActiveFileAction>().CheckActiveFiles(ChangedEventMode.RaiseOnlyOnModified, new ProgressContext());
            Assert.That(somethingWasChanged, Is.True, "An event should be raised, because status was NotShareable, but no process is active.");
        }

        [Test]
        public void TestRemoveRecentFile()
        {
            IDataStore encryptedFileInfo = TypeMap.Resolve.New<IDataStore>(_encryptedFile1);
            IDataStore decryptedFileInfo = TypeMap.Resolve.New<IDataStore>(_decryptedFile1);
            LogOnIdentity key = new LogOnIdentity("passphrase");
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable, new V1Aes128CryptoFactory().Id);
            Resolve.FileSystemState.Add(activeFile);
            Resolve.FileSystemState.Save();

            ActiveFile beforeRemoval = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(encryptedFileInfo.FullName);
            Assert.That(beforeRemoval, Is.Not.Null, "Before being removed, the ActiveFile should be possible to find.");

            TypeMap.Resolve.New<ActiveFileAction>().RemoveRecentFiles(new IDataStore[] { TypeMap.Resolve.New<IDataStore>(encryptedFileInfo.FullName) }, new ProgressContext());

            ActiveFile afterRemoval = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(encryptedFileInfo.FullName);
            Assert.That(afterRemoval, Is.Not.Null, "After being removed, the ActiveFile should still be possible to find.");
            Assert.That(afterRemoval.Status.HasFlag(ActiveFileStatus.Inactive), "But after the Remove(), the active file should have the inactive flag set.");
        }

        [Test]
        public void TestRemoveRecentFileWhenFileDoesNotExist()
        {
            IDataStore encryptedFileInfo = TypeMap.Resolve.New<IDataStore>(_encryptedFile1);
            IDataStore decryptedFileInfo = TypeMap.Resolve.New<IDataStore>(_decryptedFile1);
            LogOnIdentity key = new LogOnIdentity("passphrase");
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable, new V2Aes256CryptoFactory().Id);
            Resolve.FileSystemState.Add(activeFile);
            Resolve.FileSystemState.Save();

            ActiveFile beforeRemoval = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(encryptedFileInfo.FullName);
            Assert.That(beforeRemoval, Is.Not.Null, "Before being removed, the ActiveFile should be possible to find.");

            Assert.DoesNotThrow(() => { TypeMap.Resolve.New<ActiveFileAction>().RemoveRecentFiles(new IDataStore[] { TypeMap.Resolve.New<IDataStore>(encryptedFileInfo.FullName + ".notfound") }, new ProgressContext()); });

            ActiveFile afterFailedRemoval = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(encryptedFileInfo.FullName);
            Assert.That(afterFailedRemoval, Is.Not.Null, "After failed removal, the ActiveFile should still be possible to find.");
        }
    }
}