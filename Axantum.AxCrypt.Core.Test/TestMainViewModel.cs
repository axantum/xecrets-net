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
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text.RegularExpressions;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestMainViewModel
    {
        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();
            Factory.Instance.Singleton<ParallelFileOperation>(() => new ParallelFileOperation());
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestOpenSelectedFolderAction()
        {
            string filePath = @"C:\Folder\File.txt";

            var mockEnvironment = new Mock<FakeRuntimeEnvironment>() { CallBase = true };
            Factory.Instance.Singleton<IRuntimeEnvironment>(() => mockEnvironment.Object);

            using (MainViewModel mvm = Factory.New<MainViewModel>())
            {
                mvm.OpenSelectedFolder.Execute(filePath);
            }
            mockEnvironment.Verify(r => r.Launch(filePath));
        }

        [Test]
        public static void TestCurrentVersionPropertyBind()
        {
            UpdateCheck mockedUpdateCheck = null;
            Factory.Instance.Register<Version, UpdateCheck>((version) => mockedUpdateCheck = new Mock<UpdateCheck>(version).Object);
            Version ourVersion = new Version(1, 2, 3, 4);
            using (MainViewModel mvm = Factory.New<MainViewModel>())
            {
                mvm.CurrentVersion = ourVersion;
            }

            Mock.Get<UpdateCheck>(mockedUpdateCheck).Verify(x => x.CheckInBackground(It.Is<DateTime>((d) => d == Instance.UserSettings.LastUpdateCheckUtc), It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<Uri>()));
        }

        [Test]
        public static void TestUpdateCheckWhenNotExecutable()
        {
            var mockUpdateCheck = new Mock<UpdateCheck>(new Version(1, 2, 3, 4));
            Factory.Instance.Register<Version, UpdateCheck>((version) => mockUpdateCheck.Object);

            using (MainViewModel mvm = Factory.New<MainViewModel>())
            {
                Assert.That(mvm.UpdateCheck.CanExecute(null), Is.False);
                Assert.Throws<InvalidOperationException>(() => mvm.UpdateCheck.Execute(new DateTime(2001, 2, 3)));
            }
        }

        [Test]
        public static void TestUpdateCheckWhenExecutable()
        {
            Version ourVersion = new Version(1, 2, 3, 4);
            var mockUpdateCheck = new Mock<UpdateCheck>(ourVersion);
            Factory.Instance.Register<Version, UpdateCheck>((version) => mockUpdateCheck.Object);

            using (MainViewModel mvm = Factory.New<MainViewModel>())
            {
                mvm.CurrentVersion = ourVersion;
                Assert.That(mvm.UpdateCheck.CanExecute(null), Is.True);
                mvm.UpdateCheck.Execute(new DateTime(2001, 2, 3));
            }

            mockUpdateCheck.Verify(x => x.CheckInBackground(It.Is<DateTime>(d => d == new DateTime(2001, 2, 3)), It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<Uri>()));
        }

        [Test]
        public static void TestVersionUpdate()
        {
            Version ourVersion = new Version(1, 2, 3, 4);
            var mockUpdateCheck = new Mock<UpdateCheck>(ourVersion);
            Factory.Instance.Register<Version, UpdateCheck>((version) => mockUpdateCheck.Object);

            using (MainViewModel mvm = Factory.New<MainViewModel>())
            {
                mvm.CurrentVersion = ourVersion;

                mockUpdateCheck.Raise(m => m.VersionUpdate += null, new VersionEventArgs(new Version(1, 2, 4, 4), new Uri("http://localhost/"), VersionUpdateStatus.NewerVersionIsAvailable));
                Assert.That(mvm.VersionUpdateStatus, Is.EqualTo(VersionUpdateStatus.NewerVersionIsAvailable));
            }
        }

        [Test]
        public static void TestDragAndDropFilesPropertyBindSetsDragAndDropFileTypes()
        {
            using (MainViewModel mvm = Factory.New<MainViewModel>())
            {
                string encryptedFilePath = @"C:\Folder\File-txt.axx";
                mvm.DragAndDropFiles = new string[] { encryptedFilePath, };
                Assert.That(mvm.DragAndDropFilesTypes, Is.EqualTo(FileInfoTypes.None));

                string decryptedFilePath = @"C:\Folder\File.txt";
                mvm.DragAndDropFiles = new string[] { decryptedFilePath, };
                Assert.That(mvm.DragAndDropFilesTypes, Is.EqualTo(FileInfoTypes.None));

                FakeRuntimeFileInfo.AddFile(encryptedFilePath, null);
                mvm.DragAndDropFiles = new string[] { encryptedFilePath, decryptedFilePath, };
                Assert.That(mvm.DragAndDropFilesTypes, Is.EqualTo(FileInfoTypes.EncryptedFile));

                FakeRuntimeFileInfo.AddFile(decryptedFilePath, null);
                mvm.DragAndDropFiles = new string[] { encryptedFilePath, decryptedFilePath, };
                Assert.That(mvm.DragAndDropFilesTypes, Is.EqualTo(FileInfoTypes.EncryptableFile | FileInfoTypes.EncryptedFile));

                string folderPath = @"C:\Folder\";
                mvm.DragAndDropFiles = new string[] { encryptedFilePath, decryptedFilePath, folderPath };
                Assert.That(mvm.DragAndDropFilesTypes, Is.EqualTo(FileInfoTypes.EncryptableFile | FileInfoTypes.EncryptedFile));

                FakeRuntimeFileInfo.AddFolder(folderPath);
                mvm.DragAndDropFiles = new string[] { encryptedFilePath, decryptedFilePath, folderPath };
                Assert.That(mvm.DragAndDropFilesTypes, Is.EqualTo(FileInfoTypes.EncryptableFile | FileInfoTypes.EncryptedFile));
            }
        }

        [Test]
        public static void TestDragAndDropFilesPropertyBindSetsDroppableAsRecent()
        {
            using (MainViewModel mvm = Factory.New<MainViewModel>())
            {
                string encryptedFilePath = @"C:\Folder\File-txt.axx";
                mvm.DragAndDropFiles = new string[] { encryptedFilePath, };
                Assert.That(mvm.DroppableAsRecent, Is.False, "An encrypted file that does not exist is not a candidate for recent.");

                PassphraseIdentity id = new PassphraseIdentity("Test", new AesKey());
                Instance.FileSystemState.Identities.Add(id);
                Instance.KnownKeys.DefaultEncryptionKey = id.Key;
                mvm.DragAndDropFiles = new string[] { encryptedFilePath, };
                Assert.That(mvm.DroppableAsRecent, Is.False, "An encrypted file that does not exist, even when logged on, is not droppable as recent.");

                FakeRuntimeFileInfo.AddFile(encryptedFilePath, null);
                Instance.KnownKeys.DefaultEncryptionKey = null;
                mvm.DragAndDropFiles = new string[] { encryptedFilePath, };
                Assert.That(mvm.DroppableAsRecent, Is.True, "An encrypted file that exist is droppable as recent even when not logged on.");

                string decryptedFilePath = @"C:\Folder\File.txt";
                mvm.DragAndDropFiles = new string[] { decryptedFilePath, };
                Assert.That(mvm.DroppableAsRecent, Is.False, "An encryptable file that does not exist is not droppable as recent.");

                FakeRuntimeFileInfo.AddFile(decryptedFilePath, null);
                mvm.DragAndDropFiles = new string[] { decryptedFilePath, };
                Assert.That(mvm.DroppableAsRecent, Is.False, "An encrpytable file without a valid log on is not droppable as recent.");

                id = new PassphraseIdentity("Test2", new AesKey());
                Instance.FileSystemState.Identities.Add(id);
                Instance.KnownKeys.DefaultEncryptionKey = id.Key;
                mvm.DragAndDropFiles = new string[] { decryptedFilePath, };
                Assert.That(mvm.DroppableAsRecent, Is.True, "An encryptable existing file with a valid log on should be droppable as recent.");
            }
        }

        [Test]
        public static void TestDragAndDropFilesPropertyBindSetsDroppableAsWatchedFolder()
        {
            using (MainViewModel mvm = Factory.New<MainViewModel>())
            {
                string folder1Path = @"C:\Folder1\FilesFolder\";
                mvm.DragAndDropFiles = new string[] { folder1Path, };
                Assert.That(mvm.DroppableAsWatchedFolder, Is.False, "A folder that does not exist is not a candidate for watched folders.");

                FakeRuntimeFileInfo.AddFolder(folder1Path);
                mvm.DragAndDropFiles = new string[] { folder1Path, };
                Assert.That(mvm.DroppableAsWatchedFolder, Is.True, "This is a candidate for watched folders.");

                OS.PathFilters.Add(new Regex(@"^C:\\Folder1\\"));
                mvm.DragAndDropFiles = new string[] { folder1Path, };
                Assert.That(mvm.DroppableAsWatchedFolder, Is.False, "A folder that matches a path filter is not a candidate for watched folders.");

                string folder2Path = @"C:\Folder1\FilesFolder2\";
                FakeRuntimeFileInfo.AddFolder(folder2Path);
                OS.PathFilters.Clear();

                mvm.DragAndDropFiles = new string[] { folder1Path, folder2Path, };
                Assert.That(mvm.DroppableAsWatchedFolder, Is.False, "Although both folders are ok, only a single folder is a candidate for watched folders.");
            }
        }

        [Test]
        public static void TestSetRecentFilesComparer()
        {
            string file1 = @"C:\Folder\File3-txt.axx";
            string decrypted1 = @"C:\Folder\File2.txt";
            string file2 = @"C:\Folder\File2-txt.axx";
            string decrypted2 = @"C:\Folder\File1.txt";
            string file3 = @"C:\Folder\File1-txt.axx";
            string decrypted3 = @"C:\Folder\File3.txt";

            ActiveFile activeFile;

            FakeRuntimeEnvironment.Instance.TimeFunction = () => new DateTime(2001, 1, 1);
            FakeRuntimeFileInfo.AddFile(file1, null);
            activeFile = new ActiveFile(Factory.New<IRuntimeFileInfo>(file1), Factory.New<IRuntimeFileInfo>(decrypted1), new AesKey(), ActiveFileStatus.NotDecrypted);
            Instance.FileSystemState.Add(activeFile);

            FakeRuntimeEnvironment.Instance.TimeFunction = () => new DateTime(2002, 2, 2);
            FakeRuntimeFileInfo.AddFile(file2, null);
            activeFile = new ActiveFile(Factory.New<IRuntimeFileInfo>(file2), Factory.New<IRuntimeFileInfo>(decrypted2), new AesKey(), ActiveFileStatus.NotDecrypted);
            Instance.FileSystemState.Add(activeFile);

            FakeRuntimeEnvironment.Instance.TimeFunction = () => new DateTime(2003, 3, 3);
            FakeRuntimeFileInfo.AddFile(file3, null);
            activeFile = new ActiveFile(Factory.New<IRuntimeFileInfo>(file3), Factory.New<IRuntimeFileInfo>(decrypted3), new AesKey(), ActiveFileStatus.NotDecrypted);
            Instance.FileSystemState.Add(activeFile);

            ActiveFileComparer comparer;
            List<ActiveFile> recentFiles;

            comparer = ActiveFileComparer.DateComparer;
            using (MainViewModel mvm = Factory.New<MainViewModel>())
            {
                mvm.RecentFilesComparer = comparer;
                recentFiles = mvm.RecentFiles.ToList();

                Assert.That(recentFiles[0].EncryptedFileInfo.FullName, Is.EqualTo(file1), "Sorted by Date, this should be number 1.");
                Assert.That(recentFiles[1].EncryptedFileInfo.FullName, Is.EqualTo(file2), "Sorted by Date, this should be number 2.");
                Assert.That(recentFiles[2].EncryptedFileInfo.FullName, Is.EqualTo(file3), "Sorted by Date, this should be number 3.");

                comparer = ActiveFileComparer.DateComparer;
                comparer.ReverseSort = true;
                mvm.RecentFilesComparer = comparer;
                recentFiles = mvm.RecentFiles.ToList();
            }

            Assert.That(recentFiles[0].EncryptedFileInfo.FullName, Is.EqualTo(file3), "Sorted by Date in reverse, this should be number 1.");
            Assert.That(recentFiles[1].EncryptedFileInfo.FullName, Is.EqualTo(file2), "Sorted by Date, this should be number 2.");
            Assert.That(recentFiles[2].EncryptedFileInfo.FullName, Is.EqualTo(file1), "Sorted by Date, this should be number 3.");
        }

        [Test]
        public static void TestOpenFiles()
        {
            string file1 = @"C:\Folder\File3-txt.axx";
            string decrypted1 = @"C:\Folder\File2.txt";
            string file2 = @"C:\Folder\File2-txt.axx";
            string decrypted2 = @"C:\Folder\File1.txt";
            string file3 = @"C:\Folder\File1-txt.axx";
            string decrypted3 = @"C:\Folder\File3.txt";

            ActiveFile activeFile;

            using (MainViewModel mvm = Factory.New<MainViewModel>())
            {
                FakeRuntimeEnvironment.Instance.TimeFunction = () => new DateTime(2001, 1, 1);
                FakeRuntimeFileInfo.AddFile(file1, null);
                activeFile = new ActiveFile(Factory.New<IRuntimeFileInfo>(file1), Factory.New<IRuntimeFileInfo>(decrypted1), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted);
                Instance.FileSystemState.Add(activeFile);

                FakeRuntimeEnvironment.Instance.TimeFunction = () => new DateTime(2002, 2, 2);
                FakeRuntimeFileInfo.AddFile(file2, null);
                activeFile = new ActiveFile(Factory.New<IRuntimeFileInfo>(file2), Factory.New<IRuntimeFileInfo>(decrypted2), new AesKey(), ActiveFileStatus.NotDecrypted);
                Instance.FileSystemState.Add(activeFile);

                FakeRuntimeEnvironment.Instance.TimeFunction = () => new DateTime(2003, 3, 3);
                FakeRuntimeFileInfo.AddFile(file3, null);
                activeFile = new ActiveFile(Factory.New<IRuntimeFileInfo>(file3), Factory.New<IRuntimeFileInfo>(decrypted3), new AesKey(), ActiveFileStatus.NotDecrypted);
                Instance.FileSystemState.Add(activeFile);

                Assert.That(mvm.FilesAreOpen, Is.True);

                activeFile = new ActiveFile(Factory.New<IRuntimeFileInfo>(file1), Factory.New<IRuntimeFileInfo>(decrypted1), new AesKey(), ActiveFileStatus.NotDecrypted);
                Instance.FileSystemState.Add(activeFile);

                Assert.That(mvm.FilesAreOpen, Is.False);
            }
        }

        [Test]
        public static void TestRemoveRecentFiles()
        {
            var mockFileSystemState = new Mock<FileSystemState>() { CallBase = true };
            mockFileSystemState.Setup(x => x.Save());

            Factory.Instance.Singleton<FileSystemState>(() => mockFileSystemState.Object);

            string file1 = @"C:\Folder\File3-txt.axx";
            string decrypted1 = @"C:\Folder\File2.txt";
            string file2 = @"C:\Folder\File2-txt.axx";
            string decrypted2 = @"C:\Folder\File1.txt";
            string file3 = @"C:\Folder\File1-txt.axx";
            string decrypted3 = @"C:\Folder\File3.txt";

            ActiveFile activeFile;

            FakeRuntimeFileInfo.AddFile(file1, null);
            activeFile = new ActiveFile(Factory.New<IRuntimeFileInfo>(file1), Factory.New<IRuntimeFileInfo>(decrypted1), new AesKey(), ActiveFileStatus.NotDecrypted);
            Instance.FileSystemState.Add(activeFile);

            FakeRuntimeFileInfo.AddFile(file2, null);
            activeFile = new ActiveFile(Factory.New<IRuntimeFileInfo>(file2), Factory.New<IRuntimeFileInfo>(decrypted2), new AesKey(), ActiveFileStatus.NotDecrypted);
            Instance.FileSystemState.Add(activeFile);

            FakeRuntimeFileInfo.AddFile(file3, null);
            activeFile = new ActiveFile(Factory.New<IRuntimeFileInfo>(file3), Factory.New<IRuntimeFileInfo>(decrypted3), new AesKey(), ActiveFileStatus.NotDecrypted);
            Instance.FileSystemState.Add(activeFile);

            using (MainViewModel mvm = Factory.New<MainViewModel>())
            {
                Assert.That(mvm.RemoveRecentFiles.CanExecute(null), Is.True, "RemoveRecentFiles should be executable by default.");

                mvm.RemoveRecentFiles.Execute(new string[] { file2 });
                mockFileSystemState.Verify(x => x.Remove(It.IsAny<ActiveFile>()), Times.Once, "Exactly one recent file should be removed.");

                mockFileSystemState.ResetCalls();
                mvm.RemoveRecentFiles.Execute(new string[] { file2 });
            }
            mockFileSystemState.Verify(x => x.Remove(It.IsAny<ActiveFile>()), Times.Never, "There is no longer any matching file, so no call to remove should happen.");
        }

        [Test]
        public static void TestPurgeRecentFiles()
        {
            var mockActiveFileAction = new Mock<ActiveFileAction>();

            Factory.Instance.Register<ActiveFileAction>(() => mockActiveFileAction.Object);

            Factory.Instance.Register<SessionNotificationHandler>(() => new SessionNotificationHandler(Instance.FileSystemState, Factory.New<ActiveFileAction>(), Factory.New<AxCryptFile>()));
            Instance.SessionNotify.Notification += (sender, e) => Factory.New<SessionNotificationHandler>().HandleNotification(e.Notification);

            using (MainViewModel mvm = Factory.New<MainViewModel>())
            {
                Assert.That(mvm.PurgeActiveFiles.CanExecute(null), Is.True, "PuregRecentFiles should be executable by default.");

                mvm.PurgeActiveFiles.Execute(null);
            }

            mockActiveFileAction.Verify(x => x.PurgeActiveFiles(It.IsAny<IProgressContext>()), Times.Once, "Purge should be called.");
        }

        [Test]
        public static void TestClearPassphraseMemory()
        {
            string file1 = @"C:\Folder\File3-txt.axx";
            string decrypted1 = @"C:\Folder\File2.txt";

            ActiveFile activeFile;

            FakeRuntimeFileInfo.AddFile(file1, null);
            activeFile = new ActiveFile(Factory.New<IRuntimeFileInfo>(file1), Factory.New<IRuntimeFileInfo>(decrypted1), new AesKey(), ActiveFileStatus.NotDecrypted);
            Instance.FileSystemState.Add(activeFile);

            Instance.KnownKeys.Add(new AesKey());
            PassphraseIdentity id = new PassphraseIdentity("Test", new AesKey());
            Instance.FileSystemState.Identities.Add(id);
            Instance.KnownKeys.DefaultEncryptionKey = id.Key;

            Assert.That(Instance.FileSystemState.ActiveFileCount, Is.EqualTo(1), "One ActiveFile is expected.");
            Assert.That(Instance.KnownKeys.Keys.Count(), Is.EqualTo(2), "Two known keys are expected.");
            Assert.That(Instance.KnownKeys.DefaultEncryptionKey, Is.Not.Null, "There should be a non-null default encryption key");

            var sessionNotificationMonitorMock = new Mock<SessionNotify>();
            Factory.Instance.Singleton<SessionNotify>(() => sessionNotificationMonitorMock.Object);
            using (MainViewModel mvm = Factory.New<MainViewModel>())
            {
                mvm.ClearPassphraseMemory.Execute(null);
            }

            Assert.That(Instance.FileSystemState.ActiveFileCount, Is.EqualTo(0));
            Assert.That(Instance.KnownKeys.Keys.Count(), Is.EqualTo(0));
            Assert.That(Instance.KnownKeys.DefaultEncryptionKey, Is.Null);

            sessionNotificationMonitorMock.Verify(x => x.Notify(It.Is<SessionNotification>(sn => sn.NotificationType == SessionNotificationType.SessionStart)), Times.Once);
        }

        [Test]
        public static void TestRemoveWatchedFolders()
        {
            var mockFileSystemState = new Mock<FileSystemState>() { CallBase = true };
            mockFileSystemState.Setup(x => x.Save());

            Factory.Instance.Singleton<FileSystemState>(() => mockFileSystemState.Object);

            using (MainViewModel mvm = Factory.New<MainViewModel>())
            {
                PassphraseIdentity id = new PassphraseIdentity("Logged On User", new AesKey());
                mockFileSystemState.Object.Identities.Add(id);
                Instance.KnownKeys.DefaultEncryptionKey = id.Key;

                mvm.RemoveWatchedFolders.Execute(new string[] { "File1.txt", "file2.txt" });
            }

            mockFileSystemState.Verify(x => x.RemoveWatchedFolder(It.IsAny<IRuntimeFileInfo>()), Times.Exactly(2));
            mockFileSystemState.Verify(x => x.Save(), Times.Once);
        }

        [Test]
        public static void TestAddAndRemoveWatchedFolderState()
        {
            var fileSystemStateMock = new Mock<FileSystemState>() { CallBase = true };
            fileSystemStateMock.Setup(x => x.Save());

            Factory.Instance.Singleton<FileSystemState>(() => fileSystemStateMock.Object);
            using (MainViewModel mvm = Factory.New<MainViewModel>())
            {
                Assert.Throws<InvalidOperationException>(() => mvm.RemoveWatchedFolders.Execute(new string[] { }));

                PassphraseIdentity id = new PassphraseIdentity("Logged On User", new AesKey());
                fileSystemStateMock.Object.Identities.Add(id);
                Instance.KnownKeys.DefaultEncryptionKey = id.Key;

                mvm.RemoveWatchedFolders.Execute(new string[] { });

                fileSystemStateMock.Verify(x => x.RemoveWatchedFolder(It.IsAny<IRuntimeFileInfo>()), Times.Never);
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

            fileSystemStateMock.Verify(x => x.RemoveWatchedFolder(It.IsAny<IRuntimeFileInfo>()), Times.Exactly(1));
            fileSystemStateMock.Verify(x => x.Save(), Times.Once);
        }

        [Test]
        public static void TestSetDefaultEncryptionKeyWithoutIdentity()
        {
            var fileSystemStateMock = new Mock<FileSystemState>() { CallBase = true };
            fileSystemStateMock.Setup(x => x.Save());

            Factory.Instance.Singleton<FileSystemState>(() => fileSystemStateMock.Object);
            using (MainViewModel mvm = Factory.New<MainViewModel>())
            {
            }
            Assert.Throws<InvalidOperationException>(() => Instance.KnownKeys.DefaultEncryptionKey = new AesKey(), "Should fail since there is no matching identity.");
        }

        [Test]
        public static void TestSetDebugMode()
        {
            var fileSystemStateMock = new Mock<FileSystemState>();
            var logMock = new Mock<ILogging>();

            Factory.Instance.Singleton<FileSystemState>(() => fileSystemStateMock.Object);
            Factory.Instance.Singleton<ILogging>(() => logMock.Object);

            using (MainViewModel mvm = Factory.New<MainViewModel>())
            {
                logMock.ResetCalls();

                mvm.DebugMode = true;
                logMock.Verify(x => x.SetLevel(It.Is<LogLevel>(ll => ll == LogLevel.Debug)));
                Assert.That(ServicePointManager.ServerCertificateValidationCallback, Is.Not.Null);
                Assert.That(ServicePointManager.ServerCertificateValidationCallback(null, null, null, SslPolicyErrors.RemoteCertificateChainErrors | SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateNotAvailable), Is.True);

                logMock.ResetCalls();

                mvm.DebugMode = false;
            }
            logMock.Verify(x => x.SetLevel(It.Is<LogLevel>(ll => ll == LogLevel.Error)));
            Assert.That(ServicePointManager.ServerCertificateValidationCallback, Is.Null);
        }

        [Test]
        public static void TestSelectedRecentFiles()
        {
            using (MainViewModel mvm = Factory.New<MainViewModel>())
            {
                Assert.That(mvm.SelectedRecentFiles.Any(), Is.False);

                mvm.SelectedRecentFiles = new string[] { @"C:\Folder\Test1.axx", @"C:\Folder\Test2.axx" };

                Assert.That(mvm.SelectedRecentFiles.Count(), Is.EqualTo(2));
            }
        }

        [Test]
        public static void TestSelectedWatchedFolders()
        {
            using (MainViewModel mvm = Factory.New<MainViewModel>())
            {
                Assert.That(mvm.SelectedWatchedFolders.Any(), Is.False);

                mvm.SelectedWatchedFolders = new string[] { @"C:\Folder1\", @"C:\Folder2\" };

                Assert.That(mvm.SelectedWatchedFolders.Count(), Is.EqualTo(2));
            }
        }

        [Test]
        public static void TestTitle()
        {
            using (MainViewModel mvm = Factory.New<MainViewModel>())
            {
                Assert.That(mvm.Title.Length, Is.EqualTo(0));

                mvm.Title = "AxCrypt Title";

                Assert.That(mvm.Title, Is.EqualTo("AxCrypt Title"));
            }
        }
    }
}