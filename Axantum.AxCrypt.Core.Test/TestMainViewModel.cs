#region Coypright and License

/*
 * AxCrypt - Copyright 2013, Svante Seleborg, All Rights Reserved
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
            Factory.Instance.Singleton<FileSystemState>(() => FileSystemState.Create(FileSystemState.DefaultPathInfo));
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

            MainViewModel mvm = new MainViewModel(Instance.FileSystemState);
            mvm.OpenSelectedFolder.Execute(filePath);

            mockEnvironment.Verify(r => r.Launch(filePath));
        }

        [Test]
        public static void TestCurrentVersionPropertyBind()
        {
            MainViewModel mvm = new MainViewModel(Instance.FileSystemState);
            UpdateCheck mockedUpdateCheck = null;
            Factory.Instance.Register<Version, UpdateCheck>((version) => mockedUpdateCheck = new Mock<UpdateCheck>(version).Object);
            Version ourVersion = new Version(1, 2, 3, 4);
            mvm.CurrentVersion = ourVersion;

            Mock.Get<UpdateCheck>(mockedUpdateCheck).Verify(x => x.CheckInBackground(It.Is<DateTime>((d) => d == Instance.UserSettings.LastUpdateCheckUtc), It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<Uri>()));
        }

        [Test]
        public static void TestDragAndDropFilesPropertyBindSetsDragAndDropFileTypes()
        {
            MainViewModel mvm = new MainViewModel(Instance.FileSystemState);

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

        [Test]
        public static void TestDragAndDropFilesPropertyBindSetsDroppableAsRecent()
        {
            MainViewModel mvm = new MainViewModel(Instance.FileSystemState);

            string encryptedFilePath = @"C:\Folder\File-txt.axx";
            mvm.DragAndDropFiles = new string[] { encryptedFilePath, };
            Assert.That(mvm.DroppableAsRecent, Is.False, "An encrypted file that does not exist is not a candidate for recent.");

            Instance.KnownKeys.DefaultEncryptionKey = new AesKey();
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

            Instance.KnownKeys.DefaultEncryptionKey = new AesKey();
            mvm.DragAndDropFiles = new string[] { decryptedFilePath, };
            Assert.That(mvm.DroppableAsRecent, Is.True, "An encryptable existing file with a valid log on should be droppable as recent.");
        }

        [Test]
        public static void TestDragAndDropFilesPropertyBindSetsDroppableAsWatchedFolder()
        {
            MainViewModel mvm = new MainViewModel(Instance.FileSystemState);

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

        [Test]
        public static void TestSetRecentFilesComparer()
        {
            MainViewModel mvm = new MainViewModel(Instance.FileSystemState);

            string file1 = @"C:\Folder\File3-txt.axx";
            string decrypted1 = @"C:\Folder\File2.txt";
            string file2 = @"C:\Folder\File2-txt.axx";
            string decrypted2 = @"C:\Folder\File1.txt";
            string file3 = @"C:\Folder\File1-txt.axx";
            string decrypted3 = @"C:\Folder\File3.txt";

            ActiveFile activeFile;

            FakeRuntimeEnvironment.Instance.TimeFunction = () => new DateTime(2001, 1, 1);
            FakeRuntimeFileInfo.AddFile(file1, null);
            activeFile = new ActiveFile(OS.Current.FileInfo(file1), OS.Current.FileInfo(decrypted1), new AesKey(), ActiveFileStatus.NotDecrypted);
            Instance.FileSystemState.Add(activeFile);

            FakeRuntimeEnvironment.Instance.TimeFunction = () => new DateTime(2002, 2, 2);
            FakeRuntimeFileInfo.AddFile(file2, null);
            activeFile = new ActiveFile(OS.Current.FileInfo(file2), OS.Current.FileInfo(decrypted2), new AesKey(), ActiveFileStatus.NotDecrypted);
            Instance.FileSystemState.Add(activeFile);

            FakeRuntimeEnvironment.Instance.TimeFunction = () => new DateTime(2003, 3, 3);
            FakeRuntimeFileInfo.AddFile(file3, null);
            activeFile = new ActiveFile(OS.Current.FileInfo(file3), OS.Current.FileInfo(decrypted3), new AesKey(), ActiveFileStatus.NotDecrypted);
            Instance.FileSystemState.Add(activeFile);

            ActiveFileComparer comparer;
            List<ActiveFile> recentFiles;

            comparer = ActiveFileComparer.DateComparer;
            mvm.RecentFilesComparer = comparer;
            recentFiles = mvm.RecentFiles.ToList();

            Assert.That(recentFiles[0].EncryptedFileInfo.FullName, Is.EqualTo(file1), "Sorted by Date, this should be number 1.");
            Assert.That(recentFiles[1].EncryptedFileInfo.FullName, Is.EqualTo(file2), "Sorted by Date, this should be number 2.");
            Assert.That(recentFiles[2].EncryptedFileInfo.FullName, Is.EqualTo(file3), "Sorted by Date, this should be number 3.");

            comparer = ActiveFileComparer.DateComparer;
            comparer.ReverseSort = true;
            mvm.RecentFilesComparer = comparer;
            recentFiles = mvm.RecentFiles.ToList();

            Assert.That(recentFiles[0].EncryptedFileInfo.FullName, Is.EqualTo(file3), "Sorted by Date in reverse, this should be number 1.");
            Assert.That(recentFiles[1].EncryptedFileInfo.FullName, Is.EqualTo(file2), "Sorted by Date, this should be number 2.");
            Assert.That(recentFiles[2].EncryptedFileInfo.FullName, Is.EqualTo(file1), "Sorted by Date, this should be number 3.");
        }

        [Test]
        public static void TestAddWatchedFolder()
        {
            var mock = new Mock<FileSystemState>() { CallBase = true };
            mock.Setup(x => x.Save());

            Factory.Instance.Singleton<FileSystemState>(() => mock.Object);
            MainViewModel mvm = new MainViewModel(mock.Object);

            Assert.Throws<InvalidOperationException>(() => mvm.AddWatchedFolders.Execute(new string[] { }));

            PassphraseIdentity id = new PassphraseIdentity("Logged On User", new AesKey());
            mock.Object.Identities.Add(id);
            Instance.KnownKeys.DefaultEncryptionKey = id.Key;
            Instance.SessionNotification.DoAllNow();

            mvm.AddWatchedFolders.Execute(new string[] { });
            Instance.SessionNotification.DoAllNow();
            mock.Verify(x => x.AddWatchedFolder(It.IsAny<WatchedFolder>()), Times.Never);
            mock.Verify(x => x.Save(), Times.Never);

            mock.ResetCalls();
            mvm.AddWatchedFolders.Execute(new string[] { @"C:\Folder1\", @"C:\Folder2\" });
            Instance.SessionNotification.DoAllNow();
            mock.Verify(x => x.AddWatchedFolder(It.IsAny<WatchedFolder>()), Times.Exactly(2));
            mock.Verify(x => x.Save(), Times.Once);
        }

        [Test]
        public static void TestRemoveRecentFiles()
        {
            var mockFileSystemState = new Mock<FileSystemState>() { CallBase = true };
            mockFileSystemState.Setup(x => x.Save());

            Factory.Instance.Singleton<FileSystemState>(() => mockFileSystemState.Object);
            MainViewModel mvm = new MainViewModel(mockFileSystemState.Object);

            string file1 = @"C:\Folder\File3-txt.axx";
            string decrypted1 = @"C:\Folder\File2.txt";
            string file2 = @"C:\Folder\File2-txt.axx";
            string decrypted2 = @"C:\Folder\File1.txt";
            string file3 = @"C:\Folder\File1-txt.axx";
            string decrypted3 = @"C:\Folder\File3.txt";

            ActiveFile activeFile;

            FakeRuntimeFileInfo.AddFile(file1, null);
            activeFile = new ActiveFile(OS.Current.FileInfo(file1), OS.Current.FileInfo(decrypted1), new AesKey(), ActiveFileStatus.NotDecrypted);
            Instance.FileSystemState.Add(activeFile);

            FakeRuntimeFileInfo.AddFile(file2, null);
            activeFile = new ActiveFile(OS.Current.FileInfo(file2), OS.Current.FileInfo(decrypted2), new AesKey(), ActiveFileStatus.NotDecrypted);
            Instance.FileSystemState.Add(activeFile);

            FakeRuntimeFileInfo.AddFile(file3, null);
            activeFile = new ActiveFile(OS.Current.FileInfo(file3), OS.Current.FileInfo(decrypted3), new AesKey(), ActiveFileStatus.NotDecrypted);
            Instance.FileSystemState.Add(activeFile);

            Assert.That(mvm.RemoveRecentFiles.CanExecute(null), Is.True, "RemoveRecentFiles should be executable by default.");

            mvm.RemoveRecentFiles.Execute(new string[] { file2 });
            mockFileSystemState.Verify(x => x.Remove(It.IsAny<ActiveFile>()), Times.Once, "Exactly one recent file should be removed.");

            mockFileSystemState.ResetCalls();
            mvm.RemoveRecentFiles.Execute(new string[] { file2 });
            mockFileSystemState.Verify(x => x.Remove(It.IsAny<ActiveFile>()), Times.Never, "There is no longer any matching file, so no call to remove should happen.");
        }

        [Test]
        public static void TestPurgeRecentFiles()
        {
            var mockActiveFileAction = new Mock<ActiveFileAction>();

            Factory.Instance.Register<ActiveFileAction>(() => mockActiveFileAction.Object);

            Factory.Instance.Register<SessionNotificationHandler>(() => new SessionNotificationHandler(Instance.FileSystemState, Factory.New<ActiveFileAction>(), Factory.New<AxCryptFile>()));
            Instance.SessionNotification.Notification += (sender, e) => Factory.New<SessionNotificationHandler>().HandleNotification(e.Notification, e.Progress);

            MainViewModel mvm = new MainViewModel(Instance.FileSystemState);

            Assert.That(mvm.PurgeActiveFiles.CanExecute(null), Is.True, "PuregRecentFiles should be executable by default.");

            mvm.PurgeActiveFiles.Execute(null);
            Instance.SessionNotification.DoAllNow();
            mockActiveFileAction.Verify(x => x.PurgeActiveFiles(It.IsAny<IProgressContext>()), Times.Once, "Purge should be called.");
        }
    }
}