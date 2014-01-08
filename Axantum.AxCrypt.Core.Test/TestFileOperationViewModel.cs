﻿#region Coypright and License

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
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestFileOperationViewModel
    {
        private static bool _allCompleted;

        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();

            var mockFileSystemState = new Mock<FileSystemState>();
            Factory.Instance.Singleton<FileSystemState>(() => mockFileSystemState.Object);

            var mockParallelFile = new Mock<ParallelFileOperation>(new FakeUIThread());
            _allCompleted = false;
            mockParallelFile.Setup(x => x.DoFiles(It.IsAny<IEnumerable<IRuntimeFileInfo>>(), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()))
                .Callback<IEnumerable<IRuntimeFileInfo>, Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>, Action<FileOperationStatus>>((files, work, allComplete) => { allComplete(FileOperationStatus.Success); _allCompleted = true; });
            Factory.Instance.Singleton<ParallelFileOperation>(() => mockParallelFile.Object);
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestAddRecentFiles()
        {
            string file1 = @"C:\Folder\File3-txt.axx";
            string decrypted1 = @"C:\Folder\File2.txt";
            string file2 = @"C:\Folder\File2-txt.axx";
            string decrypted2 = @"C:\Folder\File1.txt";

            FakeRuntimeFileInfo.AddFile(file1, null);
            FakeRuntimeFileInfo.AddFile(file2, null);
            FakeRuntimeFileInfo.AddFile(decrypted1, null);
            FakeRuntimeFileInfo.AddFile(decrypted2, null);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            PassphraseIdentity id = new PassphraseIdentity("Test", new AesKey());
            Instance.FileSystemState.Identities.Add(id);
            Instance.KnownKeys.DefaultEncryptionKey = id.Key;
            mvm.AddRecentFiles.Execute(new string[] { file1, file2, decrypted1 });
            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()), Times.Once);
            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 2), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()), Times.Once);
        }

        [Test]
        public static void TestDecryptFilesInteractively()
        {
            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.axx");
                e.SelectedFiles.Add(@"C:\Folder\File2.axx");
            };
            mvm.DecryptFiles.Execute(null);

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 2), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()));
        }

        [Test]
        public static void TestDecryptFilesWithCancel()
        {
            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.Cancel = true;
            };

            mvm.DecryptFiles.Execute(null);

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.IsAny<IEnumerable<IRuntimeFileInfo>>(), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()), Times.Never);
        }

        [Test]
        public static void TestDecryptFilesWithList()
        {
            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.axx");
                e.SelectedFiles.Add(@"C:\Folder\File2.axx");
            };

            mvm.DecryptFiles.Execute(new string[] { @"C:\Folder\File3.axx" });

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()));
        }

        [Test]
        public static void TestEncryptFilesInteractively()
        {
            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.txt");
                e.SelectedFiles.Add(@"C:\Folder\File2.txt");
            };

            mvm.EncryptFiles.Execute(null);

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 2), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()));
        }

        [Test]
        public static void TestEncryptFilesWithCancel()
        {
            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.Cancel = true;
            };

            mvm.EncryptFiles.Execute(null);

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.IsAny<IEnumerable<IRuntimeFileInfo>>(), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()), Times.Never);
        }

        [Test]
        public static void TestEncryptFilesWithList()
        {
            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.txt");
                e.SelectedFiles.Add(@"C:\Folder\File2.txt");
            };

            mvm.EncryptFiles.Execute(new string[] { @"C:\Folder\File3.txt" });

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()));
        }

        [Test]
        public static void TestOpenFilesWithList()
        {
            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.OpenFiles.Execute(new string[] { @"C:\Folder\File3.txt" });
            Assert.That(_allCompleted, Is.True);
            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()));
        }

        [Test]
        public static void TestLogOnLogOffWhenLoggedOn()
        {
            PassphraseIdentity id = new PassphraseIdentity("Test", new AesKey());
            Instance.FileSystemState.Identities.Add(id);
            Instance.KnownKeys.DefaultEncryptionKey = id.Key;

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.IdentityViewModel.LogOnLogOff.Execute(null);

            Assert.That(Instance.KnownKeys.Keys.Count(), Is.EqualTo(0));
        }

        [Test]
        public static void TestLogOnLogOffWhenLoggedOffAndIdentityKnown()
        {
            Passphrase passphrase = new Passphrase("a");

            PassphraseIdentity identity = new PassphraseIdentity("Name", passphrase.DerivedPassphrase);
            Instance.FileSystemState.Identities.Add(identity);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "a";
            };

            mvm.IdentityViewModel.LogOnLogOff.Execute(null);

            Assert.That(Instance.KnownKeys.Keys.Count(), Is.EqualTo(1));
            Assert.That(Instance.KnownKeys.IsLoggedOn, Is.True);
        }

        [Test]
        public static void TestLogOnLogOffWhenLoggedOffAndNoIdentityKnown()
        {
            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "b";
                e.Name = "Name";
            };
            mvm.IdentityViewModel.LogOnLogOff.Execute(null);

            Assert.That(Instance.KnownKeys.Keys.Count(), Is.EqualTo(1));
            Assert.That(Instance.KnownKeys.IsLoggedOn, Is.True);
            Assert.That(Instance.FileSystemState.Identities.Count, Is.EqualTo(1));
        }

        [Test]
        public static void TestLogOnLogOffWhenLoggedOffAndNoIdentityKnownAndNoPassphraseGiven()
        {
            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = String.Empty;
            };

            mvm.IdentityViewModel.LogOnLogOff.Execute(null);

            Assert.That(Instance.KnownKeys.Keys.Count(), Is.EqualTo(0));
            Assert.That(Instance.KnownKeys.IsLoggedOn, Is.False);
            Assert.That(Instance.FileSystemState.Identities.Count, Is.EqualTo(0));
        }

        [Test]
        public static void TestDecryptFolders()
        {
            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.DecryptFolders.Execute(new string[] { @"C:\Folder\" });

            Assert.That(_allCompleted, Is.True);
            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()));
        }

        [Test]
        public static void TestWipeFilesInteractively()
        {
            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.txt");
                e.SelectedFiles.Add(@"C:\Folder\File2.txt");
            };

            mvm.WipeFiles.Execute(null);

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 2), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()));
        }

        [Test]
        public static void TestWipeFilesWithCancel()
        {
            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.Cancel = true;
            };

            mvm.WipeFiles.Execute(null);

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.IsAny<IEnumerable<IRuntimeFileInfo>>(), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()), Times.Never);
        }

        [Test]
        public static void TestWipeFilesWithList()
        {
            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.txt");
                e.SelectedFiles.Add(@"C:\Folder\File2.txt");
            };

            mvm.WipeFiles.Execute(new string[] { @"C:\Folder\File3.txt" });

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()));
        }

        [Test]
        public static void TestOpenFilesFromFolderWithCancel()
        {
            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.Cancel = true;
            };

            mvm.OpenFilesFromFolder.Execute(@"C:\Folder\");

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.IsAny<IEnumerable<IRuntimeFileInfo>>(), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()), Times.Never);
        }

        [Test]
        public static void TestOpenFilesFromFolder()
        {
            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Clear();
                e.SelectedFiles.Add(@"C:\Folder\File1.axx");
                e.SelectedFiles.Add(@"C:\Folder\File2.axx");
            };

            mvm.OpenFilesFromFolder.Execute(@"C:\Folder\");

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(files => files.Count() == 2), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()), Times.Once);
        }

        [Test]
        public static void TestEncryptFilesAction()
        {
            Factory.Instance.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>(new FakeUIThread()) { CallBase = true }.Object);
            Factory.Instance.Register<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            Factory.Instance.Register<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.txt");
                e.SelectedFiles.Add(@"C:\Folder\File2.txt");
            };

            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "a";
            };
            mvm.EncryptFiles.Execute(null);

            Assert.That(Instance.FileSystemState.FindEncryptedPath(@"C:\Folder\File1-txt.axx"), Is.Not.Null);
            Assert.That(Instance.FileSystemState.FindEncryptedPath(@"C:\Folder\File2-txt.axx"), Is.Not.Null);
            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 2), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()));
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipe(It.IsAny<IRuntimeFileInfo>(), It.IsAny<IRuntimeFileInfo>(), It.IsAny<AesKey>(), It.IsAny<IProgressContext>()), Times.Exactly(2));
        }

        [Test]
        public static void TestEncryptFilesWithSaveAsAction()
        {
            Factory.Instance.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>(new FakeUIThread()) { CallBase = true }.Object);
            Factory.Instance.Register<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            Factory.Instance.Register<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\Copy of File1-txt.axx");
            };

            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "a";
            };

            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File1-txt.axx", null);
            mvm.EncryptFiles.Execute(new string[] { @"C:\Folder\File1.txt" });

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()));
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipe(It.IsAny<IRuntimeFileInfo>(), It.IsAny<IRuntimeFileInfo>(), It.IsAny<AesKey>(), It.IsAny<IProgressContext>()), Times.Exactly(1));
        }

        [Test]
        public static void TestEncryptFilesWithAlreadyEncryptedFile()
        {
            Factory.Instance.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>(new FakeUIThread()) { CallBase = true }.Object);
            Factory.Instance.Register<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            Factory.Instance.Register<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();

            mvm.EncryptFiles.Execute(new string[] { @"C:\Folder\File1-txt.axx" });

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()));
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipe(It.IsAny<IRuntimeFileInfo>(), It.IsAny<IRuntimeFileInfo>(), It.IsAny<AesKey>(), It.IsAny<IProgressContext>()), Times.Never);
        }

        [Test]
        public static void TestEncryptFilesWithSaveAsCancelledAction()
        {
            Factory.Instance.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>(new FakeUIThread()) { CallBase = true }.Object);
            Factory.Instance.Register<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            Factory.Instance.Register<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.Cancel = true;
            };

            bool logginOn = false;
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                logginOn = true;
            };

            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File1-txt.axx", null);
            mvm.EncryptFiles.Execute(new string[] { @"C:\Folder\File1.txt" });

            Assert.That(logginOn, Is.False);
            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()));
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipe(It.IsAny<IRuntimeFileInfo>(), It.IsAny<IRuntimeFileInfo>(), It.IsAny<AesKey>(), It.IsAny<IProgressContext>()), Times.Never);
        }

        [Test]
        public static void TestEncryptFilesWithCancelledLoggingOnAction()
        {
            Factory.Instance.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>(new FakeUIThread()) { CallBase = true }.Object);
            Factory.Instance.Register<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            Factory.Instance.Register<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();

            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Cancel = true;
            };

            mvm.EncryptFiles.Execute(new string[] { @"C:\Folder\File1.txt" });

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()));
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipe(It.IsAny<IRuntimeFileInfo>(), It.IsAny<IRuntimeFileInfo>(), It.IsAny<AesKey>(), It.IsAny<IProgressContext>()), Times.Never);
        }
    }
}