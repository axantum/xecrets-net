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
        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();
            Factory.Instance.Singleton<ParallelFileOperation>(() => new ParallelFileOperation(Factory.Instance.Singleton<IUIThread>()));
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

            var mockParallelFile = new Mock<ParallelFileOperation>(new FakeUIThread());
            Factory.Instance.Singleton<ParallelFileOperation>(() => mockParallelFile.Object);
            mockParallelFile.Setup(x => x.DoFiles(It.IsAny<IEnumerable<IRuntimeFileInfo>>(), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()))
                .Callback<IEnumerable<IRuntimeFileInfo>, Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>, Action<FileOperationStatus>>((files, work, allComplete) => allComplete(FileOperationStatus.Success));

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            PassphraseIdentity id = new PassphraseIdentity("Test", new AesKey());
            Instance.FileSystemState.Identities.Add(id);
            Instance.KnownKeys.DefaultEncryptionKey = id.Key;
            mvm.AddRecentFiles.Execute(new string[] { file1, file2, decrypted1 });
            mockParallelFile.Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()), Times.Once);
            mockParallelFile.Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 2), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()), Times.Once);
        }

        [Test]
        public static void TestDecryptFilesInteractively()
        {
            var mockFileSystemState = new Mock<FileSystemState>();

            Factory.Instance.Singleton<FileSystemState>(() => mockFileSystemState.Object);
            var mockParallelFile = new Mock<ParallelFileOperation>(new FakeUIThread());
            Factory.Instance.Singleton<ParallelFileOperation>(() => mockParallelFile.Object);
            mockParallelFile.Setup(x => x.DoFiles(It.IsAny<IEnumerable<IRuntimeFileInfo>>(), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()))
                .Callback<IEnumerable<IRuntimeFileInfo>, Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>, Action<FileOperationStatus>>((files, work, allComplete) => allComplete(FileOperationStatus.Success));

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.axx");
                e.SelectedFiles.Add(@"C:\Folder\File2.axx");
            };
            mvm.DecryptFiles.Execute(null);

            mockParallelFile.Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 2), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()));
        }

        [Test]
        public static void TestDecryptFilesWithCancel()
        {
            var mockFileSystemState = new Mock<FileSystemState>();

            Factory.Instance.Singleton<FileSystemState>(() => mockFileSystemState.Object);
            var mockParallelFile = new Mock<ParallelFileOperation>(new FakeUIThread());
            Factory.Instance.Singleton<ParallelFileOperation>(() => mockParallelFile.Object);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.Cancel = true;
            };

            mvm.DecryptFiles.Execute(null);

            mockParallelFile.Verify(x => x.DoFiles(It.IsAny<IEnumerable<IRuntimeFileInfo>>(), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()), Times.Never);
        }

        [Test]
        public static void TestDecryptFilesWithList()
        {
            var mockFileSystemState = new Mock<FileSystemState>();

            Factory.Instance.Singleton<FileSystemState>(() => mockFileSystemState.Object);
            var mockParallelFile = new Mock<ParallelFileOperation>(new FakeUIThread());
            Factory.Instance.Singleton<ParallelFileOperation>(() => mockParallelFile.Object);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.axx");
                e.SelectedFiles.Add(@"C:\Folder\File2.axx");
            };

            mvm.DecryptFiles.Execute(new string[] { @"C:\Folder\File3.axx" });

            mockParallelFile.Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()));
        }

        [Test]
        public static void TestEncryptFilesInteractively()
        {
            var mockFileSystemState = new Mock<FileSystemState>();

            Factory.Instance.Singleton<FileSystemState>(() => mockFileSystemState.Object);
            var mockParallelFile = new Mock<ParallelFileOperation>(new FakeUIThread());
            Factory.Instance.Singleton<ParallelFileOperation>(() => mockParallelFile.Object);
            mockParallelFile.Setup(x => x.DoFiles(It.IsAny<IEnumerable<IRuntimeFileInfo>>(), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()))
                .Callback<IEnumerable<IRuntimeFileInfo>, Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>, Action<FileOperationStatus>>((files, work, allComplete) => allComplete(FileOperationStatus.Success));

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.txt");
                e.SelectedFiles.Add(@"C:\Folder\File2.txt");
            };

            mvm.EncryptFiles.Execute(null);

            mockParallelFile.Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 2), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()));
        }

        [Test]
        public static void TestEncryptFilesWithCancel()
        {
            var mockFileSystemState = new Mock<FileSystemState>();

            Factory.Instance.Singleton<FileSystemState>(() => mockFileSystemState.Object);
            var mockParallelFile = new Mock<ParallelFileOperation>(new FakeUIThread());
            Factory.Instance.Singleton<ParallelFileOperation>(() => mockParallelFile.Object);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.Cancel = true;
            };

            mvm.EncryptFiles.Execute(null);

            mockParallelFile.Verify(x => x.DoFiles(It.IsAny<IEnumerable<IRuntimeFileInfo>>(), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()), Times.Never);
        }

        [Test]
        public static void TestEncryptFilesWithList()
        {
            var mockFileSystemState = new Mock<FileSystemState>();

            Factory.Instance.Singleton<FileSystemState>(() => mockFileSystemState.Object);
            var mockParallelFile = new Mock<ParallelFileOperation>(new FakeUIThread());
            Factory.Instance.Singleton<ParallelFileOperation>(() => mockParallelFile.Object);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.txt");
                e.SelectedFiles.Add(@"C:\Folder\File2.txt");
            };

            mvm.EncryptFiles.Execute(new string[] { @"C:\Folder\File3.txt" });

            mockParallelFile.Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()));
        }

        [Test]
        public static void TestOpenFilesWithList()
        {
            var mockFileSystemState = new Mock<FileSystemState>();

            Factory.Instance.Singleton<FileSystemState>(() => mockFileSystemState.Object);

            var mockParallelFile = new Mock<ParallelFileOperation>(new FakeUIThread());
            Factory.Instance.Singleton<ParallelFileOperation>(() => mockParallelFile.Object);
            bool allCompleteCalled = false;
            mockParallelFile.Setup(x => x.DoFiles(It.IsAny<IEnumerable<IRuntimeFileInfo>>(), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()))
                .Callback<IEnumerable<IRuntimeFileInfo>, Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>, Action<FileOperationStatus>>((files, work, allComplete) => { allComplete(FileOperationStatus.Success); allCompleteCalled = true; });

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.OpenFiles.Execute(new string[] { @"C:\Folder\File3.txt" });
            Assert.That(allCompleteCalled, Is.True);
            mockParallelFile.Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()));
        }

        [Test]
        public static void TestLogOnLogOffWhenLoggedOn()
        {
            var mockFileSystemState = new Mock<FileSystemState>();
            Factory.Instance.Singleton<FileSystemState>(() => mockFileSystemState.Object);

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
            var mockFileSystemState = new Mock<FileSystemState>();

            Factory.Instance.Singleton<FileSystemState>(() => mockFileSystemState.Object);

            bool allCompleteCalled = false;
            var mockParallelFile = new Mock<ParallelFileOperation>(new FakeUIThread());
            Factory.Instance.Singleton<ParallelFileOperation>(() => mockParallelFile.Object);
            mockParallelFile.Setup(x => x.DoFiles(It.IsAny<IEnumerable<IRuntimeFileInfo>>(), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()))
                .Callback<IEnumerable<IRuntimeFileInfo>, Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>, Action<FileOperationStatus>>((files, work, allComplete) => { allComplete(FileOperationStatus.Success); allCompleteCalled = true; });

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.DecryptFolders.Execute(new string[] { @"C:\Folder\" });

            Assert.That(allCompleteCalled, Is.True);
            mockParallelFile.Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()));
        }

        [Test]
        public static void TestWipeFilesInteractively()
        {
            var mockFileSystemState = new Mock<FileSystemState>();

            Factory.Instance.Singleton<FileSystemState>(() => mockFileSystemState.Object);
            var mockParallelFile = new Mock<ParallelFileOperation>(new FakeUIThread());
            Factory.Instance.Singleton<ParallelFileOperation>(() => mockParallelFile.Object);
            mockParallelFile.Setup(x => x.DoFiles(It.IsAny<IEnumerable<IRuntimeFileInfo>>(), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()))
                .Callback<IEnumerable<IRuntimeFileInfo>, Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>, Action<FileOperationStatus>>((files, work, allComplete) => allComplete(FileOperationStatus.Success));

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.txt");
                e.SelectedFiles.Add(@"C:\Folder\File2.txt");
            };

            mvm.WipeFiles.Execute(null);

            mockParallelFile.Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 2), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()));
        }

        [Test]
        public static void TestWipeFilesWithCancel()
        {
            var mockFileSystemState = new Mock<FileSystemState>();

            Factory.Instance.Singleton<FileSystemState>(() => mockFileSystemState.Object);
            var mockParallelFile = new Mock<ParallelFileOperation>(new FakeUIThread());
            Factory.Instance.Singleton<ParallelFileOperation>(() => mockParallelFile.Object);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.Cancel = true;
            };

            mvm.WipeFiles.Execute(null);

            mockParallelFile.Verify(x => x.DoFiles(It.IsAny<IEnumerable<IRuntimeFileInfo>>(), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()), Times.Never);
        }

        [Test]
        public static void TestWipeFilesWithList()
        {
            var mockFileSystemState = new Mock<FileSystemState>();

            Factory.Instance.Singleton<FileSystemState>(() => mockFileSystemState.Object);
            var mockParallelFile = new Mock<ParallelFileOperation>(new FakeUIThread());
            Factory.Instance.Singleton<ParallelFileOperation>(() => mockParallelFile.Object);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.txt");
                e.SelectedFiles.Add(@"C:\Folder\File2.txt");
            };

            mvm.WipeFiles.Execute(new string[] { @"C:\Folder\File3.txt" });

            mockParallelFile.Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()));
        }

        [Test]
        public static void TestOpenFilesFromFolderWithCancel()
        {
            var mockFileSystemState = new Mock<FileSystemState>();

            Factory.Instance.Singleton<FileSystemState>(() => mockFileSystemState.Object);
            var mockParallelFile = new Mock<ParallelFileOperation>(new FakeUIThread());
            Factory.Instance.Singleton<ParallelFileOperation>(() => mockParallelFile.Object);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.Cancel = true;
            };

            mvm.OpenFilesFromFolder.Execute(@"C:\Folder\");

            mockParallelFile.Verify(x => x.DoFiles(It.IsAny<IEnumerable<IRuntimeFileInfo>>(), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()), Times.Never);
        }

        [Test]
        public static void TestOpenFilesFromFolder()
        {
            var mockFileSystemState = new Mock<FileSystemState>();

            Factory.Instance.Singleton<FileSystemState>(() => mockFileSystemState.Object);
            var mockParallelFile = new Mock<ParallelFileOperation>(new FakeUIThread());
            Factory.Instance.Singleton<ParallelFileOperation>(() => mockParallelFile.Object);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Clear();
                e.SelectedFiles.Add(@"C:\Folder\File1.axx");
                e.SelectedFiles.Add(@"C:\Folder\File2.axx");
            };

            mvm.OpenFilesFromFolder.Execute(@"C:\Folder\");

            mockParallelFile.Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(files => files.Count() == 2), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus>>(), It.IsAny<Action<FileOperationStatus>>()), Times.Once);
        }
    }
}