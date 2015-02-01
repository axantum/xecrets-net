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
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestMainViewModel
    {
        private CryptoImplementation _cryptoImplementation;

        public TestMainViewModel(CryptoImplementation cryptoImplementation)
        {
            _cryptoImplementation = cryptoImplementation;
        }

        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup();
            SetupAssembly.AssemblySetupCrypto(_cryptoImplementation);

            TypeMap.Register.Singleton<ParallelFileOperation>(() => new ParallelFileOperation());
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public void TestOpenSelectedFolderAction()
        {
            string filePath = @"C:\Folder\File.txt";

            var mock = new Mock<FakeLauncher>() { CallBase = true };
            TypeMap.Register.New<ILauncher>(() => mock.Object);

            using (MainViewModel mvm = TypeMap.Resolve.New<MainViewModel>())
            {
                mvm.OpenSelectedFolder.Execute(filePath);
            }
            mock.Verify(r => r.Launch(filePath));
        }

        [Test]
        public void TestCurrentVersionPropertyBind()
        {
            UpdateCheck mockedUpdateCheck = null;
            TypeMap.Register.New<Version, UpdateCheck>((version) => mockedUpdateCheck = new Mock<UpdateCheck>(version).Object);
            Version ourVersion = new Version(1, 2, 3, 4);
            using (MainViewModel mvm = TypeMap.Resolve.New<MainViewModel>())
            {
                mvm.CurrentVersion = ourVersion;
            }

            Mock.Get<UpdateCheck>(mockedUpdateCheck).Verify(x => x.CheckInBackground(It.Is<DateTime>((d) => d == Resolve.UserSettings.LastUpdateCheckUtc), It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<Uri>()));
        }

        [Test]
        public void TestUpdateCheckWhenNotExecutable()
        {
            var mockUpdateCheck = new Mock<UpdateCheck>(new Version(1, 2, 3, 4));
            TypeMap.Register.New<Version, UpdateCheck>((version) => mockUpdateCheck.Object);

            using (MainViewModel mvm = TypeMap.Resolve.New<MainViewModel>())
            {
                Assert.That(mvm.UpdateCheck.CanExecute(null), Is.False);
                Assert.Throws<InvalidOperationException>(() => mvm.UpdateCheck.Execute(new DateTime(2001, 2, 3)));
            }
        }

        [Test]
        public void TestUpdateCheckWhenExecutable()
        {
            Version ourVersion = new Version(1, 2, 3, 4);
            var mockUpdateCheck = new Mock<UpdateCheck>(ourVersion);
            TypeMap.Register.New<Version, UpdateCheck>((version) => mockUpdateCheck.Object);

            using (MainViewModel mvm = TypeMap.Resolve.New<MainViewModel>())
            {
                mvm.CurrentVersion = ourVersion;
                Assert.That(mvm.UpdateCheck.CanExecute(null), Is.True);
                mvm.UpdateCheck.Execute(new DateTime(2001, 2, 3));
            }

            mockUpdateCheck.Verify(x => x.CheckInBackground(It.Is<DateTime>(d => d == new DateTime(2001, 2, 3)), It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<Uri>()));
        }

        [Test]
        public void TestVersionUpdate()
        {
            Version ourVersion = new Version(1, 2, 3, 4);
            var mockUpdateCheck = new Mock<UpdateCheck>(ourVersion);
            TypeMap.Register.New<Version, UpdateCheck>((version) => mockUpdateCheck.Object);

            using (MainViewModel mvm = TypeMap.Resolve.New<MainViewModel>())
            {
                mvm.CurrentVersion = ourVersion;

                mockUpdateCheck.Raise(m => m.VersionUpdate += null, new VersionEventArgs(new Version(1, 2, 4, 4), new Uri("http://localhost/"), VersionUpdateStatus.NewerVersionIsAvailable));
                Assert.That(mvm.VersionUpdateStatus, Is.EqualTo(VersionUpdateStatus.NewerVersionIsAvailable));
            }
        }

        [Test]
        public void TestDragAndDropFilesPropertyBindSetsDragAndDropFileTypes()
        {
            using (MainViewModel mvm = TypeMap.Resolve.New<MainViewModel>())
            {
                string encryptedFilePath = @"C:\Folder\File-txt.axx";
                mvm.DragAndDropFiles = new string[] { encryptedFilePath, };
                Assert.That(mvm.DragAndDropFilesTypes, Is.EqualTo(FileInfoTypes.None));

                string decryptedFilePath = @"C:\Folder\File.txt";
                mvm.DragAndDropFiles = new string[] { decryptedFilePath, };
                Assert.That(mvm.DragAndDropFilesTypes, Is.EqualTo(FileInfoTypes.None));

                FakeDataStore.AddFile(encryptedFilePath, null);
                mvm.DragAndDropFiles = new string[] { encryptedFilePath, decryptedFilePath, };
                Assert.That(mvm.DragAndDropFilesTypes, Is.EqualTo(FileInfoTypes.EncryptedFile));

                FakeDataStore.AddFile(decryptedFilePath, null);
                mvm.DragAndDropFiles = new string[] { encryptedFilePath, decryptedFilePath, };
                Assert.That(mvm.DragAndDropFilesTypes, Is.EqualTo(FileInfoTypes.EncryptableFile | FileInfoTypes.EncryptedFile));

                string folderPath = @"C:\Folder\";
                mvm.DragAndDropFiles = new string[] { encryptedFilePath, decryptedFilePath, folderPath };
                Assert.That(mvm.DragAndDropFilesTypes, Is.EqualTo(FileInfoTypes.EncryptableFile | FileInfoTypes.EncryptedFile));

                FakeDataStore.AddFolder(folderPath);
                mvm.DragAndDropFiles = new string[] { encryptedFilePath, decryptedFilePath, folderPath };
                Assert.That(mvm.DragAndDropFilesTypes, Is.EqualTo(FileInfoTypes.EncryptableFile | FileInfoTypes.EncryptedFile));
            }
        }

        [Test]
        public void TestDragAndDropFilesPropertyBindSetsDroppableAsRecent()
        {
            using (MainViewModel mvm = TypeMap.Resolve.New<MainViewModel>())
            {
                string encryptedFilePath = @"C:\Folder\File-txt.axx";
                mvm.DragAndDropFiles = new string[] { encryptedFilePath, };
                Assert.That(mvm.DroppableAsRecent, Is.False, "An encrypted file that does not exist is not a candidate for recent.");

                Passphrase id = new Passphrase("passphrase1");
                Resolve.FileSystemState.Identities.Add(id);
                Resolve.KnownKeys.DefaultEncryptionKey = id;
                mvm.DragAndDropFiles = new string[] { encryptedFilePath, };
                Assert.That(mvm.DroppableAsRecent, Is.False, "An encrypted file that does not exist, even when logged on, is not droppable as recent.");

                FakeDataStore.AddFile(encryptedFilePath, null);
                Resolve.KnownKeys.DefaultEncryptionKey = null;
                mvm.DragAndDropFiles = new string[] { encryptedFilePath, };
                Assert.That(mvm.DroppableAsRecent, Is.True, "An encrypted file that exist is droppable as recent even when not logged on.");

                string decryptedFilePath = @"C:\Folder\File.txt";
                mvm.DragAndDropFiles = new string[] { decryptedFilePath, };
                Assert.That(mvm.DroppableAsRecent, Is.False, "An encryptable file that does not exist is not droppable as recent.");

                FakeDataStore.AddFile(decryptedFilePath, null);
                mvm.DragAndDropFiles = new string[] { decryptedFilePath, };
                Assert.That(mvm.DroppableAsRecent, Is.False, "An encrpytable file without a valid log on is not droppable as recent.");

                id = new Passphrase("passphrase");
                Resolve.FileSystemState.Identities.Add(id);
                Resolve.KnownKeys.DefaultEncryptionKey = id;
                mvm.DragAndDropFiles = new string[] { decryptedFilePath, };
                Assert.That(mvm.DroppableAsRecent, Is.True, "An encryptable existing file with a valid log on should be droppable as recent.");
            }
        }

        [Test]
        public void TestDragAndDropFilesPropertyBindSetsDroppableAsWatchedFolder()
        {
            using (MainViewModel mvm = TypeMap.Resolve.New<MainViewModel>())
            {
                string folder1Path = @"C:\Folder1\FilesFolder\".NormalizeFilePath();
                mvm.DragAndDropFiles = new string[] { folder1Path, };
                Assert.That(mvm.DroppableAsWatchedFolder, Is.False, "A folder that does not exist is not a candidate for watched folders.");

                FakeDataStore.AddFolder(folder1Path);
                mvm.DragAndDropFiles = new string[] { folder1Path, };
                Assert.That(mvm.DroppableAsWatchedFolder, Is.True, "This is a candidate for watched folders.");

                OS.PathFilters.Add(new Regex(@"^C:\{0}Folder1\{0}".InvariantFormat(Path.DirectorySeparatorChar)));
                mvm.DragAndDropFiles = new string[] { folder1Path, };
                Assert.That(mvm.DroppableAsWatchedFolder, Is.False, "A folder that matches a path filter is not a candidate for watched folders.");

                string folder2Path = @"C:\Folder1\FilesFolder2\".NormalizeFilePath();
                FakeDataStore.AddFolder(folder2Path);
                OS.PathFilters.Clear();

                mvm.DragAndDropFiles = new string[] { folder1Path, folder2Path, };
                Assert.That(mvm.DroppableAsWatchedFolder, Is.False, "Although both folders are ok, only a single folder is a candidate for watched folders.");
            }
        }

        [Test]
        public void TestSetRecentFilesComparer()
        {
            string file1 = @"C:\Folder\File3-txt.axx";
            string decrypted1 = @"C:\Folder\File2.txt";
            string file2 = @"C:\Folder\File2-txt.axx";
            string decrypted2 = @"C:\Folder\File1.txt";
            string file3 = @"C:\Folder\File1-txt.axx";
            string decrypted3 = @"C:\Folder\File3.txt";

            ActiveFile activeFile;

            FakeRuntimeEnvironment.Instance.TimeFunction = () => new DateTime(2001, 1, 1);
            FakeDataStore.AddFile(file1, null);
            activeFile = new ActiveFile(TypeMap.Resolve.New<IDataStore>(file1), TypeMap.Resolve.New<IDataStore>(decrypted1), new Passphrase("passphrase1"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().Id);
            Resolve.FileSystemState.Add(activeFile);

            FakeRuntimeEnvironment.Instance.TimeFunction = () => new DateTime(2002, 2, 2);
            FakeDataStore.AddFile(file2, null);
            activeFile = new ActiveFile(TypeMap.Resolve.New<IDataStore>(file2), TypeMap.Resolve.New<IDataStore>(decrypted2), new Passphrase("passphrase2"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().Id);
            Resolve.FileSystemState.Add(activeFile);

            FakeRuntimeEnvironment.Instance.TimeFunction = () => new DateTime(2003, 3, 3);
            FakeDataStore.AddFile(file3, null);
            activeFile = new ActiveFile(TypeMap.Resolve.New<IDataStore>(file3), TypeMap.Resolve.New<IDataStore>(decrypted3), new Passphrase("passphrase"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().Id);
            Resolve.FileSystemState.Add(activeFile);

            ActiveFileComparer comparer;
            List<ActiveFile> recentFiles;

            comparer = ActiveFileComparer.DateComparer;
            using (MainViewModel mvm = TypeMap.Resolve.New<MainViewModel>())
            {
                mvm.RecentFilesComparer = comparer;
                recentFiles = mvm.RecentFiles.ToList();

                Assert.That(recentFiles[0].EncryptedFileInfo.FullName, Is.EqualTo(file1.NormalizeFilePath()), "Sorted by Date, this should be number 1.");
                Assert.That(recentFiles[1].EncryptedFileInfo.FullName, Is.EqualTo(file2.NormalizeFilePath()), "Sorted by Date, this should be number 2.");
                Assert.That(recentFiles[2].EncryptedFileInfo.FullName, Is.EqualTo(file3.NormalizeFilePath()), "Sorted by Date, this should be number 3.");

                comparer = ActiveFileComparer.DateComparer;
                comparer.ReverseSort = true;
                mvm.RecentFilesComparer = comparer;
                recentFiles = mvm.RecentFiles.ToList();
            }

            Assert.That(recentFiles[0].EncryptedFileInfo.FullName, Is.EqualTo(file3.NormalizeFilePath()), "Sorted by Date in reverse, this should be number 1.");
            Assert.That(recentFiles[1].EncryptedFileInfo.FullName, Is.EqualTo(file2.NormalizeFilePath()), "Sorted by Date, this should be number 2.");
            Assert.That(recentFiles[2].EncryptedFileInfo.FullName, Is.EqualTo(file1.NormalizeFilePath()), "Sorted by Date, this should be number 3.");
        }

        [Test]
        public void TestOpenFiles()
        {
            string file1 = @"C:\Folder\File3-txt.axx";
            string decrypted1 = @"C:\Folder\File2.txt";
            string file2 = @"C:\Folder\File2-txt.axx";
            string decrypted2 = @"C:\Folder\File1.txt";
            string file3 = @"C:\Folder\File1-txt.axx";
            string decrypted3 = @"C:\Folder\File3.txt";

            ActiveFile activeFile;

            using (MainViewModel mvm = TypeMap.Resolve.New<MainViewModel>())
            {
                FakeRuntimeEnvironment.Instance.TimeFunction = () => new DateTime(2001, 1, 1);
                FakeDataStore.AddFile(file1, null);
                activeFile = new ActiveFile(TypeMap.Resolve.New<IDataStore>(file1), TypeMap.Resolve.New<IDataStore>(decrypted1), new Passphrase("passphrase1"), ActiveFileStatus.AssumedOpenAndDecrypted, new V1Aes128CryptoFactory().Id);
                Resolve.FileSystemState.Add(activeFile);

                FakeRuntimeEnvironment.Instance.TimeFunction = () => new DateTime(2002, 2, 2);
                FakeDataStore.AddFile(file2, null);
                activeFile = new ActiveFile(TypeMap.Resolve.New<IDataStore>(file2), TypeMap.Resolve.New<IDataStore>(decrypted2), new Passphrase("passphrase2"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().Id);
                Resolve.FileSystemState.Add(activeFile);

                FakeRuntimeEnvironment.Instance.TimeFunction = () => new DateTime(2003, 3, 3);
                FakeDataStore.AddFile(file3, null);
                activeFile = new ActiveFile(TypeMap.Resolve.New<IDataStore>(file3), TypeMap.Resolve.New<IDataStore>(decrypted3), new Passphrase("passphrase3"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().Id);
                Resolve.FileSystemState.Add(activeFile);

                Assert.That(mvm.FilesArePending, Is.True);

                activeFile = new ActiveFile(TypeMap.Resolve.New<IDataStore>(file1), TypeMap.Resolve.New<IDataStore>(decrypted1), new Passphrase("passphrase"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().Id);
                Resolve.FileSystemState.Add(activeFile);

                Assert.That(mvm.FilesArePending, Is.False);
            }
        }

        [Test]
        public void TestRemoveRecentFiles()
        {
            var mockFileSystemState = new Mock<FileSystemState>() { CallBase = true };
            mockFileSystemState.Setup(x => x.Save());

            TypeMap.Register.Singleton<FileSystemState>(() => mockFileSystemState.Object);

            string file1 = @"C:\Folder\File3-txt.axx";
            string decrypted1 = @"C:\Folder\File2.txt";
            string file2 = @"C:\Folder\File2-txt.axx";
            string decrypted2 = @"C:\Folder\File1.txt";
            string file3 = @"C:\Folder\File1-txt.axx";
            string decrypted3 = @"C:\Folder\File3.txt";

            ActiveFile activeFile;

            FakeDataStore.AddFile(file1, null);
            activeFile = new ActiveFile(TypeMap.Resolve.New<IDataStore>(file1), TypeMap.Resolve.New<IDataStore>(decrypted1), new Passphrase("passphrase1"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().Id);
            Resolve.FileSystemState.Add(activeFile);

            FakeDataStore.AddFile(file2, null);
            activeFile = new ActiveFile(TypeMap.Resolve.New<IDataStore>(file2), TypeMap.Resolve.New<IDataStore>(decrypted2), new Passphrase("passphrase2"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().Id);
            Resolve.FileSystemState.Add(activeFile);

            FakeDataStore.AddFile(file3, null);
            activeFile = new ActiveFile(TypeMap.Resolve.New<IDataStore>(file3), TypeMap.Resolve.New<IDataStore>(decrypted3), new Passphrase("passphrase"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().Id);
            Resolve.FileSystemState.Add(activeFile);

            using (MainViewModel mvm = TypeMap.Resolve.New<MainViewModel>())
            {
                Assert.That(mvm.RemoveRecentFiles.CanExecute(null), Is.True, "RemoveRecentFiles should be executable by default.");

                mvm.RemoveRecentFiles.Execute(new string[] { file2 });
                mockFileSystemState.Verify(x => x.RemoveActiveFile(It.IsAny<ActiveFile>()), Times.Once, "Exactly one recent file should be removed.");

                mockFileSystemState.ResetCalls();
                mvm.RemoveRecentFiles.Execute(new string[] { file2 });
            }
            mockFileSystemState.Verify(x => x.RemoveActiveFile(It.IsAny<ActiveFile>()), Times.Never, "There is no longer any matching file, so no call to remove should happen.");
        }

        [Test]
        public void TestPurgeRecentFiles()
        {
            var mockActiveFileAction = new Mock<ActiveFileAction>();

            TypeMap.Register.New<ActiveFileAction>(() => mockActiveFileAction.Object);

            Mock<IStatusChecker> mockStatusChecker = new Mock<IStatusChecker>();

            TypeMap.Register.New<SessionNotificationHandler>(() => new SessionNotificationHandler(Resolve.FileSystemState, Resolve.KnownKeys, TypeMap.Resolve.New<ActiveFileAction>(), TypeMap.Resolve.New<AxCryptFile>(), mockStatusChecker.Object));
            Resolve.SessionNotify.Notification += (sender, e) => TypeMap.Resolve.New<SessionNotificationHandler>().HandleNotification(e.Notification);

            using (MainViewModel mvm = TypeMap.Resolve.New<MainViewModel>())
            {
                Assert.That(mvm.EncryptPendingFiles.CanExecute(null), Is.True, "PuregRecentFiles should be executable by default.");

                mvm.EncryptPendingFiles.Execute(null);
            }

            mockActiveFileAction.Verify(x => x.PurgeActiveFiles(It.IsAny<IProgressContext>()), Times.Once, "Purge should be called.");
        }

        [Test]
        public void TestClearPassphraseMemory()
        {
            string file1 = @"C:\Folder\File3-txt.axx";
            string decrypted1 = @"C:\Folder\File2.txt";

            ActiveFile activeFile;

            FakeDataStore.AddFile(file1, null);
            activeFile = new ActiveFile(TypeMap.Resolve.New<IDataStore>(file1), TypeMap.Resolve.New<IDataStore>(decrypted1), new Passphrase("passphrase1"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().Id);
            Resolve.FileSystemState.Add(activeFile);

            Resolve.KnownKeys.Add(new Passphrase("passphrase2"));
            Passphrase id = new Passphrase("passphrase");
            Resolve.FileSystemState.Identities.Add(id);
            Resolve.KnownKeys.DefaultEncryptionKey = id;

            Assert.That(Resolve.FileSystemState.ActiveFileCount, Is.EqualTo(1), "One ActiveFile is expected.");
            Assert.That(Resolve.KnownKeys.Keys.Count(), Is.EqualTo(2), "Two known keys are expected.");
            Assert.That(Resolve.KnownKeys.DefaultEncryptionKey, Is.Not.Null, "There should be a non-null default encryption key");

            var sessionNotificationMonitorMock = new Mock<SessionNotify>();
            TypeMap.Register.Singleton<SessionNotify>(() => sessionNotificationMonitorMock.Object);
            using (MainViewModel mvm = TypeMap.Resolve.New<MainViewModel>())
            {
                mvm.ClearPassphraseMemory.Execute(null);
            }

            Assert.That(Resolve.FileSystemState.ActiveFileCount, Is.EqualTo(0));
            Assert.That(Resolve.KnownKeys.Keys.Count(), Is.EqualTo(0));
            Assert.That(Resolve.KnownKeys.DefaultEncryptionKey, Is.Null);

            sessionNotificationMonitorMock.Verify(x => x.Notify(It.Is<SessionNotification>(sn => sn.NotificationType == SessionNotificationType.SessionStart)), Times.Once);
        }

        [Test]
        public void TestRemoveWatchedFolders()
        {
            var mockFileSystemState = new Mock<FileSystemState>() { CallBase = true };
            mockFileSystemState.Setup(x => x.Save());

            TypeMap.Register.Singleton<FileSystemState>(() => mockFileSystemState.Object);

            using (MainViewModel mvm = TypeMap.Resolve.New<MainViewModel>())
            {
                Passphrase id = new Passphrase("passphrase");
                mockFileSystemState.Object.Identities.Add(id);
                Resolve.KnownKeys.DefaultEncryptionKey = id;

                mvm.RemoveWatchedFolders.Execute(new string[] { "File1.txt", "file2.txt" });
            }

            mockFileSystemState.Verify(x => x.RemoveWatchedFolder(It.IsAny<IDataContainer>()), Times.Exactly(2));
            mockFileSystemState.Verify(x => x.Save(), Times.Once);
        }

        [Test]
        public void TestAddAndRemoveWatchedFolderState()
        {
            var fileSystemStateMock = new Mock<FileSystemState>() { CallBase = true };
            fileSystemStateMock.Setup(x => x.Save());

            TypeMap.Register.Singleton<FileSystemState>(() => fileSystemStateMock.Object);
            using (MainViewModel mvm = TypeMap.Resolve.New<MainViewModel>())
            {
                Assert.Throws<InvalidOperationException>(() => mvm.RemoveWatchedFolders.Execute(new string[] { }));

                Passphrase id = new Passphrase("passphrase");
                fileSystemStateMock.Object.Identities.Add(id);
                Resolve.KnownKeys.DefaultEncryptionKey = id;

                mvm.RemoveWatchedFolders.Execute(new string[] { });

                fileSystemStateMock.Verify(x => x.RemoveWatchedFolder(It.IsAny<IDataContainer>()), Times.Never);
                fileSystemStateMock.Verify(x => x.Save(), Times.Never);

                fileSystemStateMock.ResetCalls();
                mvm.AddWatchedFolders.Execute(new string[] { });
                fileSystemStateMock.Verify(x => x.AddWatchedFolder(It.IsAny<WatchedFolder>()), Times.Never);
                fileSystemStateMock.Verify(x => x.Save(), Times.Never);

                mvm.AddWatchedFolders.Execute(new string[] { @"C:\Folder1\", @"C:\Folder2\" });

                fileSystemStateMock.Verify(x => x.AddWatchedFolder(It.IsAny<WatchedFolder>()), Times.Exactly(2));
                fileSystemStateMock.Verify(x => x.Save(), Times.Once);

                fileSystemStateMock.ResetCalls();
                mvm.RemoveWatchedFolders.Execute(new string[] { @"C:\Folder1\" });
            }

            fileSystemStateMock.Verify(x => x.RemoveWatchedFolder(It.IsAny<IDataContainer>()), Times.Exactly(1));
            fileSystemStateMock.Verify(x => x.Save(), Times.Once);
        }

        [Test]
        public void TestSetDefaultEncryptionKeyWithoutIdentity()
        {
            var fileSystemStateMock = new Mock<FileSystemState>() { CallBase = true };
            fileSystemStateMock.Setup(x => x.Save());

            TypeMap.Register.Singleton<FileSystemState>(() => fileSystemStateMock.Object);
            using (MainViewModel mvm = TypeMap.Resolve.New<MainViewModel>())
            {
            }
            Assert.Throws<InvalidOperationException>(() => Resolve.KnownKeys.DefaultEncryptionKey = new Passphrase("passphrase"), "Should fail since there is no matching identity.");
        }

        [Test]
        public void TestSetDebugMode()
        {
            var fileSystemStateMock = new Mock<FileSystemState>();
            var logMock = new Mock<ILogging>();

            TypeMap.Register.Singleton<FileSystemState>(() => fileSystemStateMock.Object);
            TypeMap.Register.Singleton<ILogging>(() => logMock.Object);

            using (MainViewModel mvm = TypeMap.Resolve.New<MainViewModel>())
            {
                logMock.ResetCalls();

                mvm.DebugMode = true;
                logMock.Verify(x => x.SetLevel(It.Is<LogLevel>(ll => ll == LogLevel.Debug)));
                Assert.That(FakeRuntimeEnvironment.Instance.IsDebugModeEnabled, Is.True);

                logMock.ResetCalls();

                mvm.DebugMode = false;
            }
            logMock.Verify(x => x.SetLevel(It.Is<LogLevel>(ll => ll == LogLevel.Error)));
            Assert.That(FakeRuntimeEnvironment.Instance.IsDebugModeEnabled, Is.False);
        }

        [Test]
        public void TestSelectedRecentFiles()
        {
            using (MainViewModel mvm = TypeMap.Resolve.New<MainViewModel>())
            {
                Assert.That(mvm.SelectedRecentFiles.Any(), Is.False);

                mvm.SelectedRecentFiles = new string[] { @"C:\Folder\Test1.axx", @"C:\Folder\Test2.axx" };

                Assert.That(mvm.SelectedRecentFiles.Count(), Is.EqualTo(2));
            }
        }

        [Test]
        public void TestSelectedWatchedFolders()
        {
            using (MainViewModel mvm = TypeMap.Resolve.New<MainViewModel>())
            {
                Assert.That(mvm.SelectedWatchedFolders.Any(), Is.False);

                mvm.SelectedWatchedFolders = new string[] { @"C:\Folder1\", @"C:\Folder2\" };

                Assert.That(mvm.SelectedWatchedFolders.Count(), Is.EqualTo(2));
            }
        }

        [Test]
        public void TestTitle()
        {
            using (MainViewModel mvm = TypeMap.Resolve.New<MainViewModel>())
            {
                Assert.That(mvm.Title.Length, Is.EqualTo(0));

                mvm.Title = "AxCrypt Title";

                Assert.That(mvm.Title, Is.EqualTo("AxCrypt Title"));
            }
        }

        [Test]
        public void TestNotifyWatchedFolderAdded()
        {
            Resolve.KnownKeys.DefaultEncryptionKey = new Passphrase("passphrase");
            FakeDataStore.AddFolder(@"C:\MyFolders\Folder1");
            using (MainViewModel mvm = TypeMap.Resolve.New<MainViewModel>())
            {
                Assert.That(mvm.WatchedFolders.Count(), Is.EqualTo(0));

                Resolve.FileSystemState.AddWatchedFolder(new WatchedFolder(@"C:\MyFolders\Folder1", Resolve.KnownKeys.DefaultEncryptionKey.Thumbprint));

                Assert.That(mvm.WatchedFolders.Count(), Is.EqualTo(1));
                Assert.That(mvm.WatchedFolders.First(), Is.EqualTo(@"C:\MyFolders\Folder1".NormalizeFolderPath()));
            }
        }

        [Test]
        public void TestSetFilesArePending()
        {
            Resolve.KnownKeys.DefaultEncryptionKey = new Passphrase("passphrase");
            FakeDataStore.AddFolder(@"C:\MyFolders\Folder1");
            using (MainViewModel mvm = TypeMap.Resolve.New<MainViewModel>())
            {
                Assert.That(mvm.FilesArePending, Is.False);
                Resolve.FileSystemState.AddWatchedFolder(new WatchedFolder(@"C:\MyFolders\Folder1", Resolve.KnownKeys.DefaultEncryptionKey.Thumbprint));
                FakeDataStore.AddFile(@"C:\MyFolders\Folder1\Encryptable.txt", Stream.Null);
                Assert.That(mvm.FilesArePending, Is.True);
            }
        }
    }
}