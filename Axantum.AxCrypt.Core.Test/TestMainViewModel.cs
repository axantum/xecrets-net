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

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Moq;
using NUnit.Framework;

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

            MainViewModel mvm = new MainViewModel();
            mvm.OpenSelectedFolder.Execute(filePath);

            mockEnvironment.Verify(r => r.Launch(filePath));
        }

        [Test]
        public static void TestCurrentVersionPropertyBind()
        {
            MainViewModel mvm = new MainViewModel();
            UpdateCheck mockedUpdateCheck = null;
            Factory.Instance.Register<Version, UpdateCheck>((version) => mockedUpdateCheck = new Mock<UpdateCheck>(version).Object);
            Version ourVersion = new Version(1, 2, 3, 4);
            mvm.CurrentVersion = ourVersion;

            Mock.Get<UpdateCheck>(mockedUpdateCheck).Verify(x => x.CheckInBackground(It.Is<DateTime>((d) => d == Instance.UserSettings.LastUpdateCheckUtc), It.IsAny<string>(), It.IsAny<Uri>(), It.IsAny<Uri>()));
        }

        [Test]
        public static void TestDragAndDropFilesPropertyBindSetsDragAndDropFileTypes()
        {
            MainViewModel mvm = new MainViewModel();

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
            MainViewModel mvm = new MainViewModel();

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
            MainViewModel mvm = new MainViewModel();

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
}