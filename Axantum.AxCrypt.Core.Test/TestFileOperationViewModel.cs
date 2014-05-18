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
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;

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

            var mockParallelFile = new Mock<ParallelFileOperation>();
            _allCompleted = false;
            mockParallelFile.Setup(x => x.DoFiles(It.IsAny<IEnumerable<IRuntimeFileInfo>>(), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()))
                .Callback<IEnumerable<IRuntimeFileInfo>, Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>, Action<FileOperationContext>>((files, work, allComplete) => { allComplete(new FileOperationContext(String.Empty, FileOperationStatus.Success)); _allCompleted = true; });
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
            PassphraseIdentity id = new PassphraseIdentity("Test", new Passphrase("asdf"));
            Instance.FileSystemState.Identities.Add(id);
            Instance.KnownKeys.DefaultEncryptionKey = id.Key;
            mvm.AddRecentFiles.Execute(new string[] { file1, file2, decrypted1 });
            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Once);
            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 2), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Once);
        }

        [Test]
        public static void TestDecryptFilesInteractively()
        {
            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "a";
            };
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.axx");
                e.SelectedFiles.Add(@"C:\Folder\File2.axx");
            };
            mvm.DecryptFiles.Execute(null);

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 2), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
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

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.IsAny<IEnumerable<IRuntimeFileInfo>>(), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Never);
        }

        [Test]
        public static void TestDecryptFilesWithList()
        {
            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "a";
            };
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.axx");
                e.SelectedFiles.Add(@"C:\Folder\File2.axx");
            };

            mvm.DecryptFiles.Execute(new string[] { @"C:\Folder\File3.axx" });

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
        }

        [Test]
        public static void TestEncryptFilesInteractively()
        {
            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "a";
            };
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.txt");
                e.SelectedFiles.Add(@"C:\Folder\File2.txt");
            };

            mvm.EncryptFiles.Execute(null);

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 2), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
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

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.IsAny<IEnumerable<IRuntimeFileInfo>>(), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Never);
        }

        [Test]
        public static void TestEncryptFilesWithList()
        {
            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "a";
            };
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.txt");
                e.SelectedFiles.Add(@"C:\Folder\File2.txt");
            };

            mvm.EncryptFiles.Execute(new string[] { @"C:\Folder\File3.txt" });

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
        }

        [Test]
        public static void TestOpenFilesWithList()
        {
            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.OpenFiles.Execute(new string[] { @"C:\Folder\File3.txt" });
            Assert.That(_allCompleted, Is.True);
            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
        }

        [Test]
        public static void TestLogOnLogOffWhenLoggedOn()
        {
            PassphraseIdentity id = new PassphraseIdentity("Test", new Passphrase("logonwhenloggedon"));
            Instance.FileSystemState.Identities.Add(id);
            Instance.KnownKeys.DefaultEncryptionKey = id.Key;

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.IdentityViewModel.LogOnLogOff.Execute(Guid.Empty);

            Assert.That(Instance.KnownKeys.Keys.Count(), Is.EqualTo(0));
        }

        [Test]
        public static void TestLogOnLogOffWhenLoggedOffAndIdentityKnown()
        {
            Passphrase passphrase = new Passphrase("a");

            PassphraseIdentity identity = new PassphraseIdentity("Name", passphrase);
            Instance.FileSystemState.Identities.Add(identity);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.IdentityViewModel.CryptoId = new V1Aes128CryptoFactory().Id; ;
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "a";
            };

            mvm.IdentityViewModel.LogOnLogOff.Execute(Guid.Empty);

            Assert.That(Instance.KnownKeys.Keys.Count(), Is.EqualTo(1), "There should be one known key now.");
            Assert.That(Instance.KnownKeys.IsLoggedOn, Is.True, "It should be logged on now.");
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
            mvm.IdentityViewModel.LogOnLogOff.Execute(Guid.Empty);

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

            mvm.IdentityViewModel.LogOnLogOff.Execute(Guid.Empty);

            Assert.That(Instance.KnownKeys.Keys.Count(), Is.EqualTo(0));
            Assert.That(Instance.KnownKeys.IsLoggedOn, Is.False);
            Assert.That(Instance.FileSystemState.Identities.Count, Is.EqualTo(0));
        }

        [Test]
        public static void TestDecryptFoldersWhenLoggedIn()
        {
            PassphraseIdentity id = new PassphraseIdentity("Test", new Passphrase("a"));
            Instance.FileSystemState.Identities.Add(id);
            Instance.KnownKeys.DefaultEncryptionKey = id.Key;

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.DecryptFolders.Execute(new string[] { @"C:\Folder\" });

            Assert.That(_allCompleted, Is.True);
            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
        }

        [Test]
        public static void TestDecryptFoldersWhenNotLoggedIn()
        {
            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            Assert.Throws<InvalidOperationException>(() => mvm.DecryptFolders.Execute(new string[] { @"C:\Folder\" }));
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

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 2), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
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

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.IsAny<IEnumerable<IRuntimeFileInfo>>(), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Never);
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

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
        }

        [Test]
        public static void TestOpenFilesFromFolderWithCancelWhenLoggedOn()
        {
            Instance.KnownKeys.DefaultEncryptionKey = new Passphrase("b");

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.Cancel = true;
            };

            mvm.OpenFilesFromFolder.Execute(@"C:\Folder\");

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.IsAny<IEnumerable<IRuntimeFileInfo>>(), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Never);
        }

        [Test]
        public static void TestOpenFilesFromFolderWhenLoggedOn()
        {
            Instance.KnownKeys.DefaultEncryptionKey = new Passphrase("c");

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Clear();
                e.SelectedFiles.Add(@"C:\Folder\File1.axx");
                e.SelectedFiles.Add(@"C:\Folder\File2.axx");
            };

            mvm.OpenFilesFromFolder.Execute(@"C:\Folder\");

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(files => files.Count() == 2), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Once);
        }

        [Test]
        public static void TestEncryptFilesAction()
        {
            Factory.Instance.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
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

            Assert.That(Instance.FileSystemState.FindActiveFileFromEncryptedPath(@"C:\Folder\File1-txt.axx"), Is.Not.Null);
            Assert.That(Instance.FileSystemState.FindActiveFileFromEncryptedPath(@"C:\Folder\File2-txt.axx"), Is.Not.Null);
            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 2), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipe(It.IsAny<IRuntimeFileInfo>(), It.IsAny<IRuntimeFileInfo>(), It.IsAny<Passphrase>(), It.IsAny<Guid>(), It.IsAny<IProgressContext>()), Times.Exactly(2));
        }

        [Test]
        public static void TestEncryptFilesWithSaveAsAction()
        {
            Factory.Instance.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
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

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipe(It.IsAny<IRuntimeFileInfo>(), It.IsAny<IRuntimeFileInfo>(), It.IsAny<Passphrase>(), It.IsAny<Guid>(), It.IsAny<IProgressContext>()), Times.Exactly(1));
        }

        [Test]
        public static void TestEncryptFilesWithAlreadyEncryptedFile()
        {
            Factory.Instance.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            Factory.Instance.Register<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            Factory.Instance.Register<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "a";
            };

            mvm.EncryptFiles.Execute(new string[] { @"C:\Folder\File1-txt.axx" });

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipe(It.IsAny<IRuntimeFileInfo>(), It.IsAny<IRuntimeFileInfo>(), It.IsAny<Passphrase>(), It.IsAny<Guid>(), It.IsAny<IProgressContext>()), Times.Never);
        }

        [Test]
        public static void TestEncryptFilesWithCanceledLoggingOnAction()
        {
            Factory.Instance.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            Factory.Instance.Register<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            Factory.Instance.Register<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Cancel = true;
            };

            mvm.EncryptFiles.Execute(new string[] { @"C:\Folder\File1.txt" });

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Never);
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipe(It.IsAny<IRuntimeFileInfo>(), It.IsAny<IRuntimeFileInfo>(), It.IsAny<Passphrase>(), It.IsAny<Guid>(), It.IsAny<IProgressContext>()), Times.Never);
        }

        [Test]
        public static void TestDecryptFileAction()
        {
            Factory.Instance.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            Factory.Instance.Register<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            axCryptFileMock.Setup<IAxCryptDocument>(m => m.Document(It.IsAny<IRuntimeFileInfo>(), It.IsAny<Passphrase>(), It.IsAny<IProgressContext>())).Returns((IRuntimeFileInfo fileInfo, Passphrase passphrase, IProgressContext progress) =>
            {
                V1AxCryptDocument acd = new V1AxCryptDocument(passphrase, 117);
                acd.PassphraseIsValid = true;
                acd.FileName = fileInfo.FullName.Replace("-txt.axx", ".txt");
                return acd;
            });
            axCryptFileMock.Setup<IAxCryptDocument>(m => m.Document(It.IsAny<IRuntimeFileInfo>(), It.IsAny<Passphrase>(), It.IsAny<IProgressContext>())).Returns((IRuntimeFileInfo fileInfo, Passphrase key, IProgressContext progress) =>
            {
                V1AxCryptDocument acd = new V1AxCryptDocument(key, 117);
                acd.PassphraseIsValid = true;
                acd.FileName = fileInfo.FullName.Replace("-txt.axx", ".txt");
                return acd;
            });
            Factory.Instance.Register<AxCryptFile>(() => axCryptFileMock.Object);

            Mock<AxCryptFactory> axCryptFactoryMock = new Mock<AxCryptFactory>();
            axCryptFactoryMock.Setup<Guid>(m => m.TryFindCryptoId(It.IsAny<Passphrase>(), It.IsAny<IRuntimeFileInfo>(), It.IsAny<IEnumerable<Guid>>())).Returns((Passphrase passphrase, IRuntimeFileInfo fileInfo, IEnumerable<Guid> cryptoIds) =>
            {
                return V1Aes128CryptoFactory.CryptoId;
            });
            Factory.Instance.Register<AxCryptFactory>(() => axCryptFactoryMock.Object);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.IdentityViewModel.CryptoId = new V1Aes128CryptoFactory().Id;
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1-txt.axx");
                e.SelectedFiles.Add(@"C:\Folder\File2-txt.axx");
            };

            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "a";
            };
            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File1-txt.axx", null);
            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File2-txt.axx", null);
            mvm.DecryptFiles.Execute(null);

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 2), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
            axCryptFileMock.Verify(m => m.DecryptFile(It.IsAny<IAxCryptDocument>(), It.IsAny<string>(), It.IsAny<IProgressContext>()), Times.Exactly(2));
            axCryptFileMock.Verify(m => m.Wipe(It.IsAny<IRuntimeFileInfo>(), It.IsAny<IProgressContext>()), Times.Exactly(2));
            Assert.That(Instance.KnownKeys.IsLoggedOn, Is.True);
            Assert.That(Instance.KnownKeys.Keys.Count(), Is.EqualTo(1));
        }

        [Test]
        public static void TestDecryptFileFileSaveAsAction()
        {
            Factory.Instance.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            Factory.Instance.Register<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            axCryptFileMock.Setup<IAxCryptDocument>(m => m.Document(It.IsAny<IRuntimeFileInfo>(), It.IsAny<Passphrase>(), It.IsAny<IProgressContext>())).Returns((IRuntimeFileInfo fileInfo, Passphrase passphrase, IProgressContext progress) =>
            {
                V1AxCryptDocument acd = new V1AxCryptDocument(passphrase, 31);
                acd.PassphraseIsValid = true;
                acd.DocumentHeaders.FileName = Path.GetFileName(fileInfo.FullName.Replace("-txt.axx", ".txt"));
                return acd;
            });
            Factory.Instance.Register<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.IdentityViewModel.CryptoId = new V1Aes128CryptoFactory().Id;
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Clear();
                e.SelectedFiles.Add(@"C:\Folder\Copy of File1.txt".NormalizeFilePath());
            };

            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "a";
            };
            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File1-txt.axx", new MemoryStream(Resources.helloworld_key_a_txt));
            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File1.txt", null);
            mvm.DecryptFiles.Execute(new string[] { @"C:\Folder\File1-txt.axx".NormalizeFilePath() });

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
            axCryptFileMock.Verify(m => m.DecryptFile(It.Is<IAxCryptDocument>((a) => a.FileName == @"File1.txt"), It.Is<string>((s) => s == @"C:\Folder\Copy of File1.txt".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Once);
            axCryptFileMock.Verify(m => m.Wipe(It.Is<IRuntimeFileInfo>((i) => i.FullName == @"C:\Folder\File1-txt.axx".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Once);
            Assert.That(Instance.KnownKeys.IsLoggedOn, Is.True);
        }

        [Test]
        public static void TestDecryptFileFileSaveAsCanceledAction()
        {
            Factory.Instance.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            Factory.Instance.Register<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            axCryptFileMock.Setup<IAxCryptDocument>(m => m.Document(It.IsAny<IRuntimeFileInfo>(), It.IsAny<Passphrase>(), It.IsAny<IProgressContext>())).Returns((IRuntimeFileInfo fileInfo, Passphrase passphrase, IProgressContext progress) =>
            {
                V1AxCryptDocument acd = new V1AxCryptDocument(passphrase, 10);
                acd.PassphraseIsValid = true;
                acd.DocumentHeaders.FileName = Path.GetFileName(fileInfo.FullName.Replace("-txt.axx", ".txt"));
                return acd;
            });
            Factory.Instance.Register<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.IdentityViewModel.CryptoId = new V1Aes128CryptoFactory().Id;
            mvm.SelectingFiles += (sender, e) =>
            {
                e.Cancel = true;
            };

            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "a";
            };
            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File1-txt.axx", null);
            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File1.txt", null);
            mvm.DecryptFiles.Execute(new string[] { @"C:\Folder\File1-txt.axx" });

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 0), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Never);
            axCryptFileMock.Verify(m => m.DecryptFile(It.IsAny<IAxCryptDocument>(), It.IsAny<string>(), It.IsAny<IProgressContext>()), Times.Never);
            axCryptFileMock.Verify(m => m.Wipe(It.IsAny<IRuntimeFileInfo>(), It.IsAny<IProgressContext>()), Times.Never);
            Assert.That(Instance.KnownKeys.IsLoggedOn, Is.True);
            Assert.That(Instance.KnownKeys.Keys.Count(), Is.EqualTo(1));
        }

        [Test]
        public static void TestDecryptLoggingOnCanceledAction()
        {
            Factory.Instance.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            Factory.Instance.Register<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            Factory.Instance.Register<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Cancel = true;
            };

            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File1-txt.axx", null);
            mvm.DecryptFiles.Execute(new string[] { @"C:\Folder\File1-txt.axx" });

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 0), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Never);
            axCryptFileMock.Verify(m => m.DecryptFile(It.IsAny<IAxCryptDocument>(), It.IsAny<string>(), It.IsAny<IProgressContext>()), Times.Never);
            axCryptFileMock.Verify(m => m.Wipe(It.IsAny<IRuntimeFileInfo>(), It.IsAny<IProgressContext>()), Times.Never);
            Assert.That(Instance.KnownKeys.IsLoggedOn, Is.False);
            Assert.That(Instance.KnownKeys.Keys.Count(), Is.EqualTo(0));
        }

        [Test]
        public static void TestAddRecentFilesActionAddingEncryptedWithWork()
        {
            Factory.Instance.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            Factory.Instance.Register<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            axCryptFileMock.Setup<IAxCryptDocument>(m => m.Document(It.IsAny<IRuntimeFileInfo>(), It.IsAny<Passphrase>(), It.IsAny<IProgressContext>())).Returns((IRuntimeFileInfo fileInfo, Passphrase passphrase, IProgressContext progress) =>
            {
                V1AxCryptDocument acd = new V1AxCryptDocument(passphrase, 10);
                acd.PassphraseIsValid = true;
                acd.DocumentHeaders.FileName = Path.GetFileName(fileInfo.FullName.Replace("-txt.axx", ".txt"));
                return acd;
            });
            Factory.Instance.Register<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.IdentityViewModel.CryptoId = new V1Aes128CryptoFactory().Id;
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "b";
            };

            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File1-txt.axx", null);
            mvm.AddRecentFiles.Execute(new string[] { @"C:\Folder\File1-txt.axx" });

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 1), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Once);
            Assert.That(Instance.KnownKeys.IsLoggedOn, Is.True);
            Assert.That(Instance.KnownKeys.Keys.Count(), Is.EqualTo(1));
        }

        [Test]
        public static void TestAddRecentFilesActionAddingEncryptedButCancelingWithWork()
        {
            Factory.Instance.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            Factory.Instance.Register<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            axCryptFileMock.Setup<IAxCryptDocument>(m => m.Document(It.IsAny<IRuntimeFileInfo>(), It.IsAny<Passphrase>(), It.IsAny<IProgressContext>())).Returns((IRuntimeFileInfo fileInfo, Passphrase key, IProgressContext progress) =>
            {
                V1AxCryptDocument acd = new V1AxCryptDocument();
                acd.PassphraseIsValid = true;
                acd.DocumentHeaders.FileName = Path.GetFileName(fileInfo.FullName.Replace("-txt.axx", ".txt"));
                return acd;
            });
            Factory.Instance.Register<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            int logonTries = 0;
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Cancel = true;
                logonTries++;
            };

            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File1-txt.axx", null);
            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File2-txt.axx", null);
            mvm.AddRecentFiles.Execute(new string[] { @"C:\Folder\File1-txt.axx", @"C:\Folder\File2-txt.axx" });

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 2), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Once);
            Assert.That(Instance.KnownKeys.IsLoggedOn, Is.False);
            Assert.That(Instance.KnownKeys.Keys.Count(), Is.EqualTo(0));
            Assert.That(logonTries, Is.EqualTo(1), "There should be only one logon try, since the first one cancels.");
        }

        [Test]
        public static void TestWipeFilesWithWork()
        {
            Factory.Instance.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            Factory.Instance.Register<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            Factory.Instance.Register<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
            };

            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File1-txt.axx", null);
            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File2-txt.axx", null);
            mvm.WipeFiles.Execute(new string[] { @"C:\Folder\File1-txt.axx", @"C:\Folder\File2-txt.axx" });

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 2), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Once);
            axCryptFileMock.Verify(m => m.Wipe(It.IsAny<IRuntimeFileInfo>(), It.IsAny<IProgressContext>()), Times.Exactly(2));
        }

        [Test]
        public static void TestWipeFilesSkippingOneWithWork()
        {
            Factory.Instance.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            Factory.Instance.Register<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            Factory.Instance.Register<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                if (e.SelectedFiles[0].Contains("File2"))
                {
                    e.Skip = true;
                }
            };

            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File1-txt.axx", null);
            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File2-txt.axx", null);
            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File3-txt.axx", null);
            mvm.WipeFiles.Execute(new string[] { @"C:\Folder\File1-txt.axx", @"C:\Folder\File2-txt.axx", @"C:\Folder\File3-txt.axx" });

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 3), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Once);
            axCryptFileMock.Verify(m => m.Wipe(It.Is<IRuntimeFileInfo>(r => r.FullName == @"C:\Folder\File1-txt.axx".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Once);
            axCryptFileMock.Verify(m => m.Wipe(It.Is<IRuntimeFileInfo>(r => r.FullName == @"C:\Folder\File2-txt.axx".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Never);
            axCryptFileMock.Verify(m => m.Wipe(It.Is<IRuntimeFileInfo>(r => r.FullName == @"C:\Folder\File3-txt.axx".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Once);
        }

        [Test]
        public static void TestWipeFilesCancelingAfterOneWithWork()
        {
            Factory.Instance.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            Factory.Instance.Register<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            Factory.Instance.Register<AxCryptFile>(() => axCryptFileMock.Object);

            int callTimes = 0;
            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                ++callTimes;
                if (e.SelectedFiles[0].Contains("File2"))
                {
                    SimulateDelayOfUserInteractionAllowingEarlierThreadsToReallyStartTheAsyncPart();
                    e.Cancel = true;
                }
            };

            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File1-txt.axx", null);
            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File2-txt.axx", null);
            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File3-txt.axx", null);
            mvm.WipeFiles.Execute(new string[] { @"C:\Folder\File1-txt.axx", @"C:\Folder\File2-txt.axx", @"C:\Folder\File3-txt.axx" });

            Assert.That(callTimes, Is.EqualTo(2), "Only the first two calls should be made.");
            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 3), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Once);
            axCryptFileMock.Verify(m => m.Wipe(It.IsAny<IRuntimeFileInfo>(), It.IsAny<IProgressContext>()), Times.Once);
            axCryptFileMock.Verify(m => m.Wipe(It.Is<IRuntimeFileInfo>(r => r.FullName == @"C:\Folder\File1-txt.axx".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Once);
            axCryptFileMock.Verify(m => m.Wipe(It.Is<IRuntimeFileInfo>(r => r.FullName == @"C:\Folder\File2-txt.axx".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Never);
            axCryptFileMock.Verify(m => m.Wipe(It.Is<IRuntimeFileInfo>(r => r.FullName == @"C:\Folder\File3-txt.axx".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Never);
        }

        private static void SimulateDelayOfUserInteractionAllowingEarlierThreadsToReallyStartTheAsyncPart()
        {
            Thread.Sleep(1);
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Encryptable", Justification = "Encryptable *is* a word.")]
        [Test]
        public static void TestAddRecentFilesActionWithEncryptableFilesNonInteractive()
        {
            Factory.Instance.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            Factory.Instance.Register<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            PassphraseIdentity id = new PassphraseIdentity("Test", new Passphrase("d"));
            Instance.FileSystemState.Identities.Add(id);
            Instance.KnownKeys.DefaultEncryptionKey = id.Key;

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            Factory.Instance.Register<AxCryptFile>(() => axCryptFileMock.Object);

            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File1-txt.axx", null);
            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File3-txt.axx", null);
            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File1.txt", null);
            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File2.txt", null);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.AddRecentFiles.Execute(new string[] { @"C:\Folder\File1.txt", @"C:\Folder\File2.txt", @"C:\Folder\File3-txt.axx" });

            Assert.That(Instance.FileSystemState.FindActiveFileFromEncryptedPath(@"C:\Folder\File1-txt.axx"), Is.Null);
            Assert.That(Instance.FileSystemState.FindActiveFileFromEncryptedPath(@"C:\Folder\File1-txt.1.axx"), Is.Not.Null);
            Assert.That(Instance.FileSystemState.FindActiveFileFromEncryptedPath(@"C:\Folder\File2-txt.axx"), Is.Not.Null);
            Assert.That(Instance.FileSystemState.FindActiveFileFromEncryptedPath(@"C:\Folder\File3-txt.axx"), Is.Null);
            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 2), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipe(It.IsAny<IRuntimeFileInfo>(), It.Is<IRuntimeFileInfo>(r => r.FullName == @"C:\Folder\File1-txt.1.axx".NormalizeFilePath()), It.IsAny<Passphrase>(), It.IsAny<Guid>(), It.IsAny<IProgressContext>()), Times.Once);
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipe(It.IsAny<IRuntimeFileInfo>(), It.Is<IRuntimeFileInfo>(r => r.FullName == @"C:\Folder\File2-txt.axx".NormalizeFilePath()), It.IsAny<Passphrase>(), It.IsAny<Guid>(), It.IsAny<IProgressContext>()), Times.Once);
        }

        [Test]
        public static void TestOpenFilesActionWithWork()
        {
            Factory.Instance.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            Factory.Instance.Register<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            axCryptFileMock.Setup<IAxCryptDocument>(m => m.Document(It.IsAny<IRuntimeFileInfo>(), It.IsAny<Passphrase>(), It.IsAny<IProgressContext>())).Returns((IRuntimeFileInfo fileInfo, Passphrase passphrase, IProgressContext progress) =>
            {
                V1AxCryptDocument acd = new V1AxCryptDocument(passphrase, 87);
                acd.PassphraseIsValid = true;
                acd.DocumentHeaders.FileName = fileInfo.FullName.Replace("-txt.axx", ".txt");
                return acd;
            });
            axCryptFileMock.Setup<IAxCryptDocument>(m => m.Document(It.IsAny<IRuntimeFileInfo>(), It.IsAny<Passphrase>(), It.IsAny<IProgressContext>())).Returns((IRuntimeFileInfo fileInfo, Passphrase key, IProgressContext progress) =>
            {
                V1AxCryptDocument acd = new V1AxCryptDocument(key, 87);
                acd.PassphraseIsValid = true;
                acd.DocumentHeaders.FileName = fileInfo.FullName.Replace("-txt.axx", ".txt");
                return acd;
            });
            Factory.Instance.Register<AxCryptFile>(() => axCryptFileMock.Object);

            Mock<AxCryptFactory> axCryptFactoryMock = new Mock<AxCryptFactory>();
            axCryptFactoryMock.Setup<Guid>(m => m.TryFindCryptoId(It.IsAny<Passphrase>(), It.IsAny<IRuntimeFileInfo>(), It.IsAny<IEnumerable<Guid>>())).Returns((Passphrase passphrase, IRuntimeFileInfo fileInfo, IEnumerable<Guid> cryptoIds) =>
            {
                return V1Aes128CryptoFactory.CryptoId;
            });
            Factory.Instance.Register<AxCryptFactory>(() => axCryptFactoryMock.Object);

            Mock<FileOperation> fileOperationMock = new Mock<FileOperation>(Instance.FileSystemState, Instance.SessionNotify);
            Factory.Instance.Register<FileOperation>(() => fileOperationMock.Object);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.IdentityViewModel.CryptoId = new V1Aes128CryptoFactory().Id;
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = "b";
            };

            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File1-txt.axx", null);
            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File2-txt.axx", null);
            mvm.OpenFiles.Execute(new string[] { @"C:\Folder\File1-txt.axx", @"C:\Folder\File2-txt.axx" });

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 2), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
            fileOperationMock.Verify(f => f.OpenAndLaunchApplication(It.Is<string>(s => s == @"C:\Folder\File1-txt.axx".NormalizeFilePath()), It.IsAny<Passphrase>(), It.IsAny<IAxCryptDocument>(), It.IsAny<IProgressContext>()), Times.Once);
            fileOperationMock.Verify(f => f.OpenAndLaunchApplication(It.Is<string>(s => s == @"C:\Folder\File2-txt.axx".NormalizeFilePath()), It.IsAny<Passphrase>(), It.IsAny<IAxCryptDocument>(), It.IsAny<IProgressContext>()), Times.Once);
            Assert.That(Instance.KnownKeys.IsLoggedOn, Is.True);
            Assert.That(Instance.KnownKeys.Keys.Count(), Is.EqualTo(1));
        }

        [Test]
        public static void TestOpenFilesActionCancelingWithWork()
        {
            Factory.Instance.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            Factory.Instance.Register<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            int count = 0;
            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            axCryptFileMock.Setup<IAxCryptDocument>(m => m.Document(It.IsAny<IRuntimeFileInfo>(), It.IsAny<Passphrase>(), It.IsAny<IProgressContext>())).Returns((IRuntimeFileInfo fileInfo, Passphrase passphrase, IProgressContext progress) =>
            {
                V1AxCryptDocument acd = new V1AxCryptDocument(passphrase, 17);
                count++;
                acd.PassphraseIsValid = true;
                acd.DocumentHeaders.FileName = fileInfo.FullName.Replace("-txt.axx", ".txt");
                return acd;
            });
            Factory.Instance.Register<AxCryptFile>(() => axCryptFileMock.Object);

            Mock<FileOperation> fileOperationMock = new Mock<FileOperation>(Instance.FileSystemState, Instance.SessionNotify);
            Factory.Instance.Register<FileOperation>(() => fileOperationMock.Object);

            Mock<AxCryptFactory> axCryptFactoryMock = new Mock<AxCryptFactory>();
            axCryptFactoryMock.Setup(acf => acf.TryFindCryptoId(It.IsAny<Passphrase>(), It.IsAny<IRuntimeFileInfo>(), It.IsAny<IEnumerable<Guid>>())).Returns((Passphrase passphrase, IRuntimeFileInfo fileInfo, IEnumerable<Guid> cryptoIds) =>
            {
                count++;
                return Guid.Empty;
            });
            Factory.Instance.Register<AxCryptFactory>(() => axCryptFactoryMock.Object);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.IdentityViewModel.CryptoId = new V1Aes128CryptoFactory().Id;
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                if (count == 2)
                {
                    e.Cancel = true;
                    return;
                }
                e.Passphrase = "b";
            };

            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File1-txt.axx", null);
            FakeRuntimeFileInfo.AddFile(@"C:\Folder\File2-txt.axx", null);
            mvm.OpenFiles.Execute(new string[] { @"C:\Folder\File1-txt.axx", @"C:\Folder\File2-txt.axx" });

            Mock.Get(Instance.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IRuntimeFileInfo>>(f => f.Count() == 2), It.IsAny<Func<IRuntimeFileInfo, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
            fileOperationMock.Verify(f => f.OpenAndLaunchApplication(It.Is<string>(s => s == @"C:\Folder\File1-txt.axx".NormalizeFilePath()), It.IsAny<Passphrase>(), It.IsAny<IAxCryptDocument>(), It.IsAny<IProgressContext>()), Times.Once);
            fileOperationMock.Verify(f => f.OpenAndLaunchApplication(It.Is<string>(s => s == @"C:\Folder\File2-txt.axx".NormalizeFilePath()), It.IsAny<Passphrase>(), It.IsAny<IAxCryptDocument>(), It.IsAny<IProgressContext>()), Times.Never);
            Assert.That(Instance.KnownKeys.IsLoggedOn, Is.True, "Should be logged on.");
            Assert.That(Instance.KnownKeys.Keys.Count(), Is.EqualTo(1), "One known key.");
        }

        [Test]
        public static void TestDecryptFoldersWithWork()
        {
            Factory.Instance.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            Factory.Instance.Register<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            Factory.Instance.Register<AxCryptFile>(() => axCryptFileMock.Object);

            PassphraseIdentity id = new PassphraseIdentity("Test", new Passphrase("e"));
            Instance.FileSystemState.Identities.Add(id);
            Instance.KnownKeys.DefaultEncryptionKey = id.Key;

            FakeRuntimeFileInfo.AddFile(@"C:\Folder1\File1-txt.axx", null);
            FakeRuntimeFileInfo.AddFile(@"C:\Folder1\File2-txt.axx", null);
            FakeRuntimeFileInfo.AddFile(@"C:\Folder2\File1-txt.axx", null);

            FileOperationViewModel mvm = Factory.New<FileOperationViewModel>();
            mvm.DecryptFolders.Execute(new string[] { @"C:\Folder1\", @"C:\Folder2\" });

            axCryptFileMock.Verify(m => m.DecryptFilesInsideFolderUniqueWithWipeOfOriginal(It.Is<IRuntimeFileInfo>(r => r.FullName == @"C:\Folder1\".NormalizeFilePath()), It.IsAny<Passphrase>(), It.IsAny<IStatusChecker>(), It.IsAny<IProgressContext>()), Times.Once);
            axCryptFileMock.Verify(m => m.DecryptFilesInsideFolderUniqueWithWipeOfOriginal(It.Is<IRuntimeFileInfo>(r => r.FullName == @"C:\Folder2\".NormalizeFilePath()), It.IsAny<Passphrase>(), It.IsAny<IStatusChecker>(), It.IsAny<IProgressContext>()), Times.Once);
        }
    }
}