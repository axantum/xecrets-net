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
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Fake;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestFileSystemStateActions
    {
        private static readonly string _pathRoot = Path.GetPathRoot(Environment.CurrentDirectory);
        private static readonly string _documentsFolder = _pathRoot.PathCombine("Documents");
        private static readonly string _decryptedFile1 = _documentsFolder.PathCombine("test.txt");
        private static readonly string _uncompressedAxxPath = _documentsFolder.PathCombine("Uncompressed.axx");
        private static readonly string _fileSystemStateFilePath = Path.Combine(Path.GetTempPath(), "DummyFileSystemState.txt");

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

            TypeMap.Register.Singleton<FileSystemState>(() => FileSystemState.Create(New<IDataStore>(_fileSystemStateFilePath)));
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public async Task TestCheckActiveFilesIsNotLocked()
        {
            DateTime utcNow = New<INow>().Utc;
            DateTime utcYesterday = utcNow.AddDays(-1);
            FakeDataStore.AddFile(_uncompressedAxxPath, utcNow, utcNow, utcNow, Stream.Null);
            FakeDataStore.AddFile(_decryptedFile1, utcYesterday, utcYesterday, utcYesterday, Stream.Null);

            ActiveFile activeFile = new ActiveFile(New<IDataStore>(_uncompressedAxxPath), New<IDataStore>(_decryptedFile1), new LogOnIdentity("passphrase"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().CryptoId);
            ((FakeNow)New<INow>()).TimeFunction = (() => { return utcNow.AddMinutes(10); });
            bool changedWasRaised = false;
            Resolve.FileSystemState.Add(activeFile);
            Resolve.SessionNotify.AddCommand((SessionNotification notification) =>
            {
                changedWasRaised = notification.NotificationType == SessionNotificationType.ActiveFileChange;
                return Constant.CompletedTask;
            });
            await New<ActiveFileAction>().CheckActiveFiles(new ProgressContext());
            Assert.That(changedWasRaised, Is.True, "The file should be detected as decrypted being created.");
        }

        [Test]
        public void TestCheckActiveFilesIsLocked()
        {
            DateTime utcNow = New<INow>().Utc;
            DateTime utcYesterday = utcNow.AddDays(-1);
            FakeDataStore.AddFile(_uncompressedAxxPath, utcNow, utcNow, utcNow, Stream.Null);
            FakeDataStore.AddFile(_decryptedFile1, utcYesterday, utcYesterday, utcYesterday, Stream.Null);

            ActiveFile activeFile = new ActiveFile(New<IDataStore>(_uncompressedAxxPath), New<IDataStore>(_decryptedFile1), new LogOnIdentity("passphrase"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().CryptoId);
            ((FakeNow)New<INow>()).TimeFunction = (() => { return utcNow.AddMinutes(10); });
            bool changedWasRaised = false;
            Resolve.FileSystemState.Add(activeFile);
            Resolve.SessionNotify.AddCommand((SessionNotification notification) =>
            {
                changedWasRaised = notification.NotificationType == SessionNotificationType.ActiveFileChange;
                return Constant.CompletedTask;
            });
            using (FileLock fileLock = New<FileLocker>().Acquire(activeFile.EncryptedFileInfo))
            {
                Task.Run((Action)(async () =>
                {
                    await New<ActiveFileAction>().CheckActiveFiles(new ProgressContext());
                })).Wait();
            }
            Assert.That(changedWasRaised, Is.False, "The file should be not be detected as decrypted being created because the encrypted file is locked.");
            using (FileLock fileLock = New<FileLocker>().Acquire(activeFile.DecryptedFileInfo))
            {
                Task.Run((Action)(async () =>
                {
                    await New<ActiveFileAction>().CheckActiveFiles(new ProgressContext());
                })).Wait();
            }
            Assert.That(changedWasRaised, Is.False, "The file should be not be detected as decrypted being created because the decrypted file is locked.");
        }

        [Test]
        public async Task TestCheckActiveFilesKeyIsSet()
        {
            DateTime utcNow = New<INow>().Utc;
            DateTime utcJustNow = utcNow.AddMinutes(-1);
            FakeDataStore.AddFile(_uncompressedAxxPath, utcNow, utcNow, utcNow, Stream.Null);
            FakeDataStore.AddFile(_decryptedFile1, utcJustNow, utcJustNow, utcJustNow, Stream.Null);

            Mock<AxCryptFactory> axCryptFactoryMock = new Mock<AxCryptFactory>() { CallBase = true };
            axCryptFactoryMock.Setup<DecryptionParameter>(m => m.FindDecryptionParameter(It.IsAny<IEnumerable<DecryptionParameter>>(), It.IsAny<IDataStore>())).Returns((Func<IEnumerable<DecryptionParameter>, IDataStore, DecryptionParameter>)((IEnumerable<DecryptionParameter> decryptionParameters, IDataStore fileInfo) =>
            {
                return new DecryptionParameter(Passphrase.Empty, new V1Aes128CryptoFactory().CryptoId);
            }));
            TypeMap.Register.New<AxCryptFactory>(() => axCryptFactoryMock.Object);

            ActiveFile activeFile;
            activeFile = new ActiveFile(New<IDataStore>(_uncompressedAxxPath), New<IDataStore>(_decryptedFile1), new LogOnIdentity("passphrase"), ActiveFileStatus.AssumedOpenAndDecrypted, new V1Aes128CryptoFactory().CryptoId);
            Resolve.FileSystemState.Add(activeFile);
            await Resolve.KnownIdentities.Add(activeFile.Identity);

            IDataStore decryptedFileInfo = New<IDataStore>(_decryptedFile1);
            decryptedFileInfo.SetFileTimes(utcNow, utcNow, utcNow);

            ((FakeNow)New<INow>()).TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Resolve.SessionNotify.AddCommand((SessionNotification notification) =>
            {
                changedWasRaised = notification.NotificationType == SessionNotificationType.ActiveFileChange;
                return Constant.CompletedTask;
            });
            ((FakeNow)New<INow>()).TimeFunction = () => DateTime.UtcNow;
            await New<ActiveFileAction>().CheckActiveFiles(new ProgressContext());
            Assert.That(changedWasRaised, Is.True, "The file should be detected as modified, because it is considered open and decrypted, has a proper key, is modified, no running process so it should be re-encrypted and deleted.");
            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_uncompressedAxxPath);
            Assert.That(activeFile, Is.Not.Null, "The encrypted file should be found.");
            Assert.That(activeFile.IsModified, Is.False, "The file should no longer be flagged as modified.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotDecrypted), Is.True, "The file should no longer be decrypted, since it was re-encrypted and deleted.");
        }

        [Test]
        public async Task TestCheckActiveFilesKeyIsNotSetWithKnownKey()
        {
            DateTime utcNow = New<INow>().Utc;
            FakeDataStore.AddFile(_uncompressedAxxPath, utcNow, utcNow, utcNow, FakeDataStore.ExpandableMemoryStream(Resources.helloworld_key_a_txt));
            LogOnIdentity passphrase = new LogOnIdentity("a");
            New<AxCryptFile>().Decrypt(New<IDataStore>(_uncompressedAxxPath), New<IDataStore>(_decryptedFile1), passphrase, AxCryptOptions.None, new ProgressContext());

            ActiveFile activeFile = new ActiveFile(New<IDataStore>(_uncompressedAxxPath), New<IDataStore>(_decryptedFile1), passphrase, ActiveFileStatus.AssumedOpenAndDecrypted, new V1Aes128CryptoFactory().CryptoId);
            Resolve.FileSystemState.Add(activeFile);
            await Resolve.FileSystemState.Save();

            TypeMap.Register.Singleton<FileSystemState>(() => FileSystemState.Create(New<IDataStore>(_fileSystemStateFilePath)));

            ((FakeNow)New<INow>()).TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Resolve.SessionNotify.AddCommand((SessionNotification notification) =>
            {
                changedWasRaised = notification.NotificationType == SessionNotificationType.ActiveFileChange;
                return Constant.CompletedTask;
            });

            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_uncompressedAxxPath);
            Assert.That(activeFile.Identity == LogOnIdentity.Empty, "The key should be null after loading of new FileSystemState");

            await Resolve.KnownIdentities.Add(passphrase);
            await New<ActiveFileAction>().CheckActiveFiles(new ProgressContext());
            Assert.That(changedWasRaised, Is.True, "The ActiveFile should be modified because there is now a known key.");

            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_uncompressedAxxPath);
            Assert.That(activeFile.Identity != LogOnIdentity.Empty, "The key should not be null after the checking of active files.");
        }

        [Test]
        public async Task TestCheckActiveFilesKeyIsNotSetWithoutKnownKey()
        {
            DateTime utcNow = New<INow>().Utc;
            FakeDataStore.AddFile(_uncompressedAxxPath, utcNow, utcNow, utcNow, FakeDataStore.ExpandableMemoryStream(Resources.helloworld_key_a_txt));
            LogOnIdentity passphrase = new LogOnIdentity("a");
            New<AxCryptFile>().Decrypt(New<IDataStore>(_uncompressedAxxPath), New<IDataStore>(_decryptedFile1), passphrase, AxCryptOptions.None, new ProgressContext());

            ActiveFile activeFile = new ActiveFile(New<IDataStore>(_uncompressedAxxPath), New<IDataStore>(_decryptedFile1), passphrase, ActiveFileStatus.AssumedOpenAndDecrypted, new V1Aes128CryptoFactory().CryptoId);
            Resolve.FileSystemState.Add(activeFile);
            await Resolve.FileSystemState.Save();

            TypeMap.Register.Singleton<FileSystemState>(() => FileSystemState.Create(New<IDataStore>(_fileSystemStateFilePath)));

            IDataStore decryptedFileInfo = New<IDataStore>(_decryptedFile1);
            decryptedFileInfo.SetFileTimes(utcNow.AddSeconds(30), utcNow.AddSeconds(30), utcNow.AddSeconds(30));

            ((FakeNow)New<INow>()).TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Resolve.SessionNotify.AddCommand((SessionNotification notification) =>
            {
                changedWasRaised = notification.NotificationType == SessionNotificationType.ActiveFileChange;
                return Constant.CompletedTask;
            });

            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_uncompressedAxxPath);
            Assert.That(activeFile.Identity == LogOnIdentity.Empty, "The key should be null after loading of new FileSystemState");

            await New<ActiveFileAction>().CheckActiveFiles(new ProgressContext());
            Assert.That(changedWasRaised, Is.False, "The ActiveFile should be not be modified because the file was modified as well and thus cannot be deleted.");

            await Resolve.KnownIdentities.Add(new LogOnIdentity("x"));
            await Resolve.KnownIdentities.Add(new LogOnIdentity("y"));
            await New<ActiveFileAction>().CheckActiveFiles(new ProgressContext());
            Assert.That(changedWasRaised, Is.False, "The ActiveFile should be not be modified because the file was modified as well and thus cannot be deleted.");

            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_uncompressedAxxPath);
            Assert.That(activeFile.Identity == LogOnIdentity.Empty, "The key should still be null after the checking of active files.");

            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted), Is.True, "The file should still be there.");
            Assert.That(activeFile.ThumbprintMatch(passphrase.Passphrase), Is.True, "The active file should still be known to be decryptable with the original passphrase.");
        }

        [Test]
        public async Task TestCheckActiveFilesNotDecryptedAndDoesNotExist()
        {
            DateTime utcNow = New<INow>().Utc;
            FakeDataStore.AddFile(_uncompressedAxxPath, utcNow, utcNow, utcNow, Stream.Null);
            FakeDataStore.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            ActiveFile activeFile = new ActiveFile(New<IDataStore>(_uncompressedAxxPath), New<IDataStore>(_decryptedFile1), new LogOnIdentity("passphrase"), ActiveFileStatus.AssumedOpenAndDecrypted, new V1Aes128CryptoFactory().CryptoId);
            New<IDataStore>(_decryptedFile1).Delete();
            activeFile = new ActiveFile(activeFile, ActiveFileStatus.NotDecrypted);
            Resolve.FileSystemState.Add(activeFile);
            await Resolve.KnownIdentities.Add(activeFile.Identity);

            bool changedWasRaised = false;
            Resolve.SessionNotify.AddCommand((SessionNotification notification) =>
            {
                changedWasRaised = notification.NotificationType == SessionNotificationType.ActiveFileChange;
                return Constant.CompletedTask;
            });

            ((FakeNow)New<INow>()).TimeFunction = (() => { return utcNow.AddMinutes(1); });
            await New<ActiveFileAction>().CheckActiveFiles(new ProgressContext());

            Assert.That(changedWasRaised, Is.False, "The ActiveFile should be not be modified because it's already deleted.");
        }

        [Test]
        public async Task TestCheckActiveFilesNoDeleteWhenNotDesktopWindows()
        {
            DateTime utcNow = New<INow>().Utc;
            FakeDataStore.AddFile(_uncompressedAxxPath, utcNow, utcNow, utcNow, Stream.Null);
            FakeDataStore.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            ActiveFile activeFile = new ActiveFile(New<IDataStore>(_uncompressedAxxPath), New<IDataStore>(_decryptedFile1), new LogOnIdentity("passphrase"), ActiveFileStatus.AssumedOpenAndDecrypted, new V1Aes128CryptoFactory().CryptoId);
            Resolve.FileSystemState.Add(activeFile);
            await Resolve.KnownIdentities.Add(activeFile.Identity);

            ((FakeNow)New<INow>()).TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Resolve.SessionNotify.AddCommand((SessionNotification notification) =>
            {
                changedWasRaised = notification.NotificationType == SessionNotificationType.ActiveFileChange;
                return Constant.CompletedTask;
            });

            SetupAssembly.FakeRuntimeEnvironment.Platform = Platform.Unknown;
            await New<ActiveFileAction>().CheckActiveFiles(new ProgressContext());
            Assert.That(changedWasRaised, Is.False, "No change should be raised when the file is not modified and not Desktop Windows.");
            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_uncompressedAxxPath);
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted), Is.True, "Nothing should happen with the file when not running as Desktop Windows.");

            SetupAssembly.FakeRuntimeEnvironment.Platform = Platform.WindowsDesktop;
            changedWasRaised = false;
            await New<ActiveFileAction>().CheckActiveFiles(new ProgressContext());
            Assert.That(changedWasRaised, Is.True, "Since the file should be deleted because running as Desktop Windows the changed event should be raised.");
            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_uncompressedAxxPath);
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotDecrypted), Is.True, "The file should be deleted and marked as Not Decrypted when running as Desktop Windows.");
        }

        [Test]
        public async Task TestCheckActiveFilesUpdateButWithTargetLockedForSharing()
        {
            DateTime utcNow = New<INow>().Utc;
            FakeDataStore.AddFile(_uncompressedAxxPath, utcNow, utcNow, utcNow, FakeDataStore.ExpandableMemoryStream(Resources.helloworld_key_a_txt));
            LogOnIdentity passphrase = new LogOnIdentity("a");
            New<AxCryptFile>().Decrypt(New<IDataStore>(_uncompressedAxxPath), New<IDataStore>(_decryptedFile1), passphrase, AxCryptOptions.None, new ProgressContext());

            ActiveFile activeFile = new ActiveFile(New<IDataStore>(_uncompressedAxxPath), New<IDataStore>(_decryptedFile1), passphrase, ActiveFileStatus.AssumedOpenAndDecrypted, new V1Aes128CryptoFactory().CryptoId);
            Resolve.FileSystemState.Add(activeFile);

            IDataStore decryptedFileInfo = New<IDataStore>(_decryptedFile1);
            decryptedFileInfo.SetFileTimes(utcNow.AddSeconds(30), utcNow.AddSeconds(30), utcNow.AddSeconds(30));

            ((FakeNow)New<INow>()).TimeFunction = (() => { return utcNow.AddMinutes(1); });

            await Resolve.KnownIdentities.Add(passphrase);
            bool changedWasRaised = false;
            Resolve.SessionNotify.AddCommand((SessionNotification notification) =>
            {
                changedWasRaised = notification.NotificationType == SessionNotificationType.ActiveFileChange;
                return Constant.CompletedTask;
            });

            try
            {
                FakeDataStore.IsLockedFunc = (fds) => fds.FullName == _decryptedFile1;

                await New<ActiveFileAction>().CheckActiveFiles(new ProgressContext());
            }
            finally
            {
                FakeDataStore.IsLockedFunc = (fds) => false;
            }

            Assert.That(changedWasRaised, Is.True, "The ActiveFile should be modified because it should now be marked as not shareable.");
            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_uncompressedAxxPath);
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotShareable), Is.True, "The ActiveFile should be marked as not shareable after the checking of active files.");
        }

        [Test]
        public async Task TestCheckActiveFilesUpdateButWithTargetInaccessible()
        {
            DateTime utcNow = New<INow>().Utc;
            FakeDataStore.AddFile(_uncompressedAxxPath, utcNow, utcNow, utcNow, FakeDataStore.ExpandableMemoryStream(Resources.helloworld_key_a_txt));
            LogOnIdentity passphrase = new LogOnIdentity("a");
            New<AxCryptFile>().Decrypt(New<IDataStore>(_uncompressedAxxPath), New<IDataStore>(_decryptedFile1), passphrase, AxCryptOptions.None, new ProgressContext());

            ActiveFile activeFile = new ActiveFile(New<IDataStore>(_uncompressedAxxPath), New<IDataStore>(_decryptedFile1), passphrase, ActiveFileStatus.AssumedOpenAndDecrypted, new V1Aes128CryptoFactory().CryptoId);
            Resolve.FileSystemState.Add(activeFile);

            IDataStore decryptedFileInfo = New<IDataStore>(_decryptedFile1);
            decryptedFileInfo.SetFileTimes(utcNow.AddSeconds(30), utcNow.AddSeconds(30), utcNow.AddSeconds(30));

            ((FakeNow)New<INow>()).TimeFunction = (() => { return utcNow.AddMinutes(1); });

            await Resolve.KnownIdentities.Add(passphrase);

            bool changedWasRaised = false;
            Resolve.SessionNotify.AddCommand((SessionNotification notification) =>
            {
                changedWasRaised = notification.NotificationType == SessionNotificationType.ActiveFileChange && notification.FullNames.Contains(_uncompressedAxxPath);
                return Constant.CompletedTask;
            });

            EventHandler eventHandler = ((object sender, EventArgs e) =>
            {
                FakeDataStore fileInfo = (FakeDataStore)sender;
                if (fileInfo.FullName == Path.ChangeExtension(_uncompressedAxxPath, ".tmp"))
                {
                    throw new IOException("Faked access denied.");
                }
            });
            FakeDataStore.OpeningForWrite += eventHandler;
            try
            {
                Assert.ThrowsAsync<FileOperationException>(async () => await New<ActiveFileAction>().CheckActiveFiles(new ProgressContext()));
            }
            finally
            {
                FakeDataStore.OpeningForWrite -= eventHandler;
            }

            Assert.That(changedWasRaised, Is.False, "The ActiveFile should not be modified because it was not accessible.");
            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_uncompressedAxxPath);
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotShareable), Is.False, "The ActiveFile should not be marked as not shareable after the checking of active files.");
        }

        [Test]
        public async Task TestTryDeleteButProcessHasNotExited()
        {
            DateTime utcNow = New<INow>().Utc;
            FakeDataStore.AddFile(_uncompressedAxxPath, utcNow, utcNow, utcNow, Stream.Null);
            FakeDataStore.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            FakeLauncher fakeLauncher = new FakeLauncher();
            fakeLauncher.Launch(_decryptedFile1);
            ActiveFile activeFile = new ActiveFile(New<IDataStore>(_uncompressedAxxPath), New<IDataStore>(_decryptedFile1), new LogOnIdentity("passphrase"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().CryptoId);
            activeFile = new ActiveFile(activeFile, ActiveFileStatus.AssumedOpenAndDecrypted);
            Resolve.FileSystemState.Add(activeFile, fakeLauncher);
            await Resolve.KnownIdentities.Add(activeFile.Identity);

            ((FakeNow)New<INow>()).TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Resolve.SessionNotify.AddCommand((SessionNotification notification) =>
            {
                changedWasRaised = notification.NotificationType == SessionNotificationType.ActiveFileChange;
                return Constant.CompletedTask;
            });
            SetupAssembly.FakeRuntimeEnvironment.Platform = Platform.WindowsDesktop;
            await New<ActiveFileAction>().CheckActiveFiles(new ProgressContext());

            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_uncompressedAxxPath);
            Assert.That(changedWasRaised, Is.False, "No changed event should be raised because no change should occur since the process is active.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted), Is.True, "The ActiveFile plain text should not be deleted after the checking of active files because the launcher is active.");
        }

        [Test]
        public async Task TestDeleteModifiedWhenSignedIn()
        {
            DateTime utcNow = New<INow>().Utc;
            FakeDataStore.AddFile(_uncompressedAxxPath, utcNow, utcNow, utcNow, FakeDataStore.ExpandableMemoryStream(Resources.david_copperfield_key__aa_ae_oe__ulu_txt));
            FakeDataStore.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            ActiveFile activeFile = new ActiveFile(New<IDataStore>(_uncompressedAxxPath), New<IDataStore>(_decryptedFile1), LogOnIdentity.Empty, ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().CryptoId);
            activeFile = new ActiveFile(activeFile, utcNow.AddMinutes(-1), ActiveFileStatus.AssumedOpenAndDecrypted);
            Resolve.FileSystemState.Add(activeFile);

            await New<KnownIdentities>().SetDefaultEncryptionIdentity(new LogOnIdentity(EmailAddress.Parse("test@axcrypt.net"), new Passphrase("test")));
            await New<ActiveFileAction>().CheckActiveFiles(new ProgressContext());

            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_uncompressedAxxPath);
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotDecrypted), Is.True, "The ActiveFile plain text should be deleted after the checking of active files because the user is signed in.");
        }

        [Test]
        public async Task TestDoNotDeleteModifiedIfNotSignedIn()
        {
            DateTime utcNow = New<INow>().Utc;
            FakeDataStore.AddFile(_uncompressedAxxPath, utcNow, utcNow, utcNow, Stream.Null);
            FakeDataStore.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            ActiveFile activeFile = new ActiveFile(New<IDataStore>(_uncompressedAxxPath), New<IDataStore>(_decryptedFile1), LogOnIdentity.Empty, ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().CryptoId);
            activeFile = new ActiveFile(activeFile, utcNow.AddMinutes(-1), ActiveFileStatus.AssumedOpenAndDecrypted);
            Resolve.FileSystemState.Add(activeFile);

            await New<ActiveFileAction>().CheckActiveFiles(new ProgressContext());

            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_uncompressedAxxPath);
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted), Is.True, "The ActiveFile plain text should not be deleted after the checking of active files because the user is not signed in.");
        }

        [Test]
        public async Task TestCheckProcessExitedWhenExited()
        {
            DateTime utcNow = New<INow>().Utc;
            FakeDataStore.AddFile(_uncompressedAxxPath, utcNow, utcNow, utcNow, Stream.Null);
            FakeDataStore.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            await New<KnownIdentities>().SetDefaultEncryptionIdentity(new LogOnIdentity(EmailAddress.Parse("test@axcrypt.net"), new Passphrase("test")));
            FakeLauncher fakeLauncher = new FakeLauncher();
            fakeLauncher.Launch(_decryptedFile1);
            ActiveFile activeFile = new ActiveFile(New<IDataStore>(_uncompressedAxxPath), New<IDataStore>(_decryptedFile1), new LogOnIdentity("passphrase"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().CryptoId);
            activeFile = new ActiveFile(activeFile, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable);
            Resolve.FileSystemState.Add(activeFile, fakeLauncher);

            ((FakeNow)New<INow>()).TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Resolve.SessionNotify.AddCommand((SessionNotification notification) =>
            {
                changedWasRaised = notification.NotificationType == SessionNotificationType.ActiveFileChange;
                return Constant.CompletedTask;
            });
            SetupAssembly.FakeRuntimeEnvironment.Platform = Platform.WindowsDesktop;
            fakeLauncher.HasExited = true;
            await New<ActiveFileAction>().CheckActiveFiles(new ProgressContext());

            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_uncompressedAxxPath);
            Assert.That(changedWasRaised, Is.True, "A changed event should be raised because the process has exited.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotDecrypted), Is.True, "The ActiveFile plain text should be deleted after the checking of active files because the launcher is no longer active.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotShareable), Is.False, "The file should be shareable after checking of active files because the launcher is no longer active.");
        }

        [Test]
        public async Task TestTryDeleteButDecryptedSharingLocked()
        {
            DateTime utcNow = New<INow>().Utc;
            FakeDataStore.AddFile(_uncompressedAxxPath, utcNow, utcNow, utcNow, Stream.Null);
            FakeDataStore.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            ActiveFile activeFile = new ActiveFile(New<IDataStore>(_uncompressedAxxPath), New<IDataStore>(_decryptedFile1), new LogOnIdentity("passphrase"), ActiveFileStatus.AssumedOpenAndDecrypted, new V1Aes128CryptoFactory().CryptoId);
            Resolve.FileSystemState.Add(activeFile);

            ((FakeNow)New<INow>()).TimeFunction = (() => { return utcNow.AddMinutes(1); });
            bool changedWasRaised = false;
            Resolve.SessionNotify.AddCommand((SessionNotification notification) =>
            {
                changedWasRaised = notification.NotificationType == SessionNotificationType.ActiveFileChange;
                return Constant.CompletedTask;
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
                await New<ActiveFileAction>().CheckActiveFiles(new ProgressContext());
            }
            finally
            {
                FakeDataStore.Deleting -= eventHandler;
                FakeDataStore.OpeningForWrite -= eventHandler;
                FakeDataStore.Moving -= eventHandler;
            }

            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_uncompressedAxxPath);
            Assert.That(changedWasRaised, Is.True, "A changed event should be raised because it should now be NotShareable.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted), Is.True, "The ActiveFile plain text should still be there after the checking of active files because the file is NotShareable.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotShareable), Is.True, "The ActiveFile plain text should be NotShareable after the checking of active files because the file could not be deleted.");
        }

        [Test]
        public void TestPurgeActiveFilesWhenFileIsLocked()
        {
            DateTime utcNow = New<INow>().Utc;
            FakeDataStore.AddFile(_uncompressedAxxPath, utcNow, utcNow, utcNow, Stream.Null);
            FakeDataStore.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            IDataStore encryptedFileInfo = New<IDataStore>(_uncompressedAxxPath);
            IDataStore decryptedFileInfo = New<IDataStore>(_decryptedFile1);
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, new LogOnIdentity("passphrase"), ActiveFileStatus.AssumedOpenAndDecrypted, new V1Aes128CryptoFactory().CryptoId);
            Resolve.FileSystemState.Add(activeFile);

            bool changedWasRaised = false;
            Resolve.SessionNotify.AddCommand((SessionNotification notification) =>
            {
                changedWasRaised = notification.NotificationType == SessionNotificationType.ActiveFileChange;
                return Constant.CompletedTask;
            });

            using (FileLock fileLock = New<FileLocker>().Acquire(decryptedFileInfo))
            {
                Task.Run(() => New<ActiveFileAction>().PurgeActiveFiles(new ProgressContext())).Wait();
            }

            Assert.That(changedWasRaised, Is.False, "A changed event should not be raised because the decrypted file is locked.");
        }

        [Test]
        public async Task TestPurgeActiveFilesWhenFileIsModified()
        {
            DateTime utcNow = New<INow>().Utc;
            FakeDataStore.AddFile(_uncompressedAxxPath, utcNow, utcNow, utcNow, Stream.Null);
            FakeDataStore.AddFile(_decryptedFile1, utcNow, utcNow, utcNow, Stream.Null);

            Mock<AxCryptFactory> axCryptFactoryMock = new Mock<AxCryptFactory>() { CallBase = true };
            axCryptFactoryMock.Setup<DecryptionParameter>(m => m.FindDecryptionParameter(It.IsAny<IEnumerable<DecryptionParameter>>(), It.IsAny<IDataStore>())).Returns((Func<IEnumerable<DecryptionParameter>, IDataStore, DecryptionParameter>)((IEnumerable<DecryptionParameter> decryptionParameters, IDataStore fileInfo) =>
            {
                return new DecryptionParameter(Passphrase.Empty, new V1Aes128CryptoFactory().CryptoId);
            }));
            TypeMap.Register.New<AxCryptFactory>(() => axCryptFactoryMock.Object);

            IDataStore encryptedFileInfo = New<IDataStore>(_uncompressedAxxPath);
            IDataStore decryptedFileInfo = New<IDataStore>(_decryptedFile1);
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, new LogOnIdentity("passphrase"), ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable, new V1Aes128CryptoFactory().CryptoId);
            Resolve.FileSystemState.Add(activeFile);

            int timeCalls = 0;
            ((FakeNow)New<INow>()).TimeFunction = (() => { return utcNow.AddMinutes(1).AddMilliseconds(100 * timeCalls++); });
            DateTime utcLater = New<INow>().Utc;

            decryptedFileInfo.SetFileTimes(utcLater, utcLater, utcLater);

            bool changedWasRaised = false;
            Resolve.SessionNotify.AddCommand((SessionNotification notification) =>
            {
                changedWasRaised = notification.NotificationType == SessionNotificationType.ActiveFileChange;
                return Constant.CompletedTask;
            });

            await New<ActiveFileAction>().PurgeActiveFiles(new ProgressContext());

            activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(_uncompressedAxxPath);
            Assert.That(changedWasRaised, Is.True, "A changed event should be raised because the decrypted file is modified.");
            Assert.That(activeFile.Status.HasMask(ActiveFileStatus.NotDecrypted), Is.True, "The NotShareable not withstanding, the purge should have updated the file and removed the decrypted file.");
        }

        [Test]
        public async Task TestUpdateActiveFileWithKeyIfKeyMatchesThumbprintWithKnownKey()
        {
            IDataStore encryptedFileInfo = New<IDataStore>(_uncompressedAxxPath);
            IDataStore decryptedFileInfo = New<IDataStore>(_decryptedFile1);
            LogOnIdentity key = new LogOnIdentity("passphrase");
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable, new V1Aes128CryptoFactory().CryptoId);
            Resolve.FileSystemState.Add(activeFile);

            bool updateWasMade = await New<ActiveFileAction>().UpdateActiveFileWithKeyIfKeyMatchesThumbprint(key);
            Assert.That(updateWasMade, Is.False, "Since there are only ActiveFiles with known keys in the list, no update should be made.");
        }

        [Test]
        public async Task TestUpdateActiveFileWithKeyIfKeyMatchesThumbprintWithWrongThumbprint()
        {
            IDataStore encryptedFileInfo = New<IDataStore>(_uncompressedAxxPath);
            IDataStore decryptedFileInfo = New<IDataStore>(_decryptedFile1);
            LogOnIdentity key = new LogOnIdentity("a");
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable, new V1Aes128CryptoFactory().CryptoId);
            Resolve.FileSystemState.Add(activeFile);
            await Resolve.FileSystemState.Save();

            TypeMap.Register.Singleton<FileSystemState>(() => FileSystemState.Create(New<IDataStore>(_fileSystemStateFilePath)));

            LogOnIdentity wrongKey = new LogOnIdentity("b");
            bool updateWasMade = await New<ActiveFileAction>().UpdateActiveFileWithKeyIfKeyMatchesThumbprint(wrongKey);
            Assert.That(updateWasMade, Is.False, "Since there are only ActiveFiles with wrong keys in the list, no update should be made.");
        }

        [Test]
        public async Task TestUpdateActiveFileWithKeyIfKeyMatchesThumbprintWithMatchingThumbprint()
        {
            IDataStore encryptedFileInfo = New<IDataStore>(_uncompressedAxxPath);
            IDataStore decryptedFileInfo = New<IDataStore>(_decryptedFile1);
            LogOnIdentity key = new LogOnIdentity("passphrase");
            FakeDataStore.AddFile(_uncompressedAxxPath, FakeDataStore.TestDate1Utc, FakeDataStore.TestDate2Utc, FakeDataStore.TestDate3Utc, Stream.Null);
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable, new V1Aes128CryptoFactory().CryptoId);
            Resolve.FileSystemState.Add(activeFile);
            await Resolve.FileSystemState.Save();

            TypeMap.Register.Singleton<FileSystemState>(() => FileSystemState.Create(New<IDataStore>(_fileSystemStateFilePath)));

            bool updateWasMade = await New<ActiveFileAction>().UpdateActiveFileWithKeyIfKeyMatchesThumbprint(key);
            Assert.That(updateWasMade, Is.True, "Since there is an ActiveFile with the right thumbprint in the list, an update should be made.");
        }

        [Test]
        public async Task TestUpdateActiveFileButWithNoChangeDueToIrrelevantStatus()
        {
            IDataStore encryptedFileInfo = New<IDataStore>(_uncompressedAxxPath);
            IDataStore decryptedFileInfo = New<IDataStore>(_decryptedFile1);
            LogOnIdentity key = new LogOnIdentity("passphrase");
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.None, new V1Aes128CryptoFactory().CryptoId);
            Resolve.FileSystemState.Add(activeFile);

            ((FakeNow)New<INow>()).TimeFunction = (() => { return DateTime.UtcNow.AddMinutes(1); });

            bool changedWasRaised = false;
            Resolve.SessionNotify.AddCommand((SessionNotification notification) =>
            {
                changedWasRaised = notification.NotificationType == SessionNotificationType.ActiveFileChange;
                return Constant.CompletedTask;
            });
            await New<ActiveFileAction>().CheckActiveFiles(new ProgressContext());
            Assert.That(changedWasRaised, Is.False, "No event should be raised, because nothing should change.");
        }

        [Test]
        public async Task TestUpdateActiveFileWithEventRaisedSinceItAppearsAProcessHasExited()
        {
            IDataStore encryptedFileInfo = New<IDataStore>(_uncompressedAxxPath);
            IDataStore decryptedFileInfo = New<IDataStore>(_decryptedFile1);
            LogOnIdentity key = new LogOnIdentity("passphrase");
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.NotShareable, new V1Aes128CryptoFactory().CryptoId);
            Resolve.FileSystemState.Add(activeFile);

            ((FakeNow)New<INow>()).TimeFunction = (() => { return DateTime.UtcNow.AddMinutes(1); });

            bool changedWasRaised = false;
            Resolve.SessionNotify.AddCommand((SessionNotification notification) =>
            {
                changedWasRaised = notification.NotificationType == SessionNotificationType.ActiveFileChange;
                return Constant.CompletedTask;
            });
            await New<ActiveFileAction>().CheckActiveFiles(new ProgressContext());
            Assert.That(changedWasRaised, Is.True, "An event should be raised, because status was NotShareable, but no process is active.");
        }

        [Test]
        public async Task TestRemoveRecentFile()
        {
            IDataStore encryptedFileInfo = New<IDataStore>(_uncompressedAxxPath);
            IDataStore decryptedFileInfo = New<IDataStore>(_decryptedFile1);
            LogOnIdentity key = new LogOnIdentity("passphrase");
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable, new V1Aes128CryptoFactory().CryptoId);
            Resolve.FileSystemState.Add(activeFile);
            await Resolve.FileSystemState.Save();

            ActiveFile beforeRemoval = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(encryptedFileInfo.FullName);
            Assert.That(beforeRemoval, Is.Not.Null, "Before being removed, the ActiveFile should be possible to find.");

            activeFile = new ActiveFile(activeFile, ActiveFileStatus.NotDecrypted);
            Resolve.FileSystemState.Add(activeFile);

            await New<ActiveFileAction>().RemoveRecentFiles(new IDataStore[] { New<IDataStore>(encryptedFileInfo.FullName) }, new ProgressContext());

            ActiveFile afterRemoval = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(encryptedFileInfo.FullName);
            Assert.That(afterRemoval, Is.Null, "After being removed, the ActiveFile should not be possible to find.");
        }

        [Test]
        public async Task TestRemoveRecentFileWhenFileDoesNotExist()
        {
            IDataStore encryptedFileInfo = New<IDataStore>(_uncompressedAxxPath);
            IDataStore decryptedFileInfo = New<IDataStore>(_decryptedFile1);
            LogOnIdentity key = new LogOnIdentity("passphrase");
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NotShareable, new V2Aes256CryptoFactory().CryptoId);
            Resolve.FileSystemState.Add(activeFile);
            await Resolve.FileSystemState.Save();

            ActiveFile beforeRemoval = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(encryptedFileInfo.FullName);
            Assert.That(beforeRemoval, Is.Not.Null, "Before being removed, the ActiveFile should be possible to find.");

            Assert.DoesNotThrowAsync(async () => { await New<ActiveFileAction>().RemoveRecentFiles(new IDataStore[] { New<IDataStore>(encryptedFileInfo.FullName + ".notfound") }, new ProgressContext()); });

            ActiveFile afterFailedRemoval = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(encryptedFileInfo.FullName);
            Assert.That(afterFailedRemoval, Is.Not.Null, "After failed removal, the ActiveFile should still be possible to find.");
        }
    }
}