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

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Fake;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestFileOperationViewModel
    {
        private bool _allCompleted;

        private CryptoImplementation _cryptoImplementation;

        public TestFileOperationViewModel(CryptoImplementation cryptoImplementation)
        {
            _cryptoImplementation = cryptoImplementation;
        }

        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup();
            SetupAssembly.AssemblySetupCrypto(_cryptoImplementation);

            var mockParallelFile = new Mock<ParallelFileOperation>();
            _allCompleted = false;
            mockParallelFile.Setup(x => x.DoFiles<IDataContainer>(It.IsAny<IEnumerable<IDataContainer>>(), It.IsAny<Func<IDataContainer, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()))
                .Callback<IEnumerable<IDataContainer>, Func<IDataContainer, IProgressContext, FileOperationContext>, Action<FileOperationContext>>((files, work, allComplete) => { allComplete(new FileOperationContext(String.Empty, ErrorStatus.Success)); _allCompleted = true; });
            mockParallelFile.Setup(x => x.DoFiles<IDataStore>(It.IsAny<IEnumerable<IDataStore>>(), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()))
                .Callback<IEnumerable<IDataStore>, Func<IDataStore, IProgressContext, FileOperationContext>, Action<FileOperationContext>>((files, work, allComplete) => { allComplete(new FileOperationContext(String.Empty, ErrorStatus.Success)); _allCompleted = true; });
            TypeMap.Register.Singleton<ParallelFileOperation>(() => mockParallelFile.Object);
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public void TestAddRecentFiles()
        {
            string file1 = @"C:\Folder\File3-txt.axx";
            string decrypted1 = @"C:\Folder\File2.txt";
            string file2 = @"C:\Folder\File2-txt.axx";
            string decrypted2 = @"C:\Folder\File1.txt";

            FakeDataStore.AddFile(file1, null);
            FakeDataStore.AddFile(file2, null);
            FakeDataStore.AddFile(decrypted1, null);
            FakeDataStore.AddFile(decrypted2, null);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            LogOnIdentity id = new LogOnIdentity("asdf");
            Resolve.FileSystemState.KnownPassphrases.Add(id.Passphrase);
            Resolve.KnownIdentities.DefaultEncryptionIdentity = id;
            mvm.AddRecentFiles.Execute(new string[] { file1, file2, decrypted1 });
            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 1), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Once);
            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 2), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Once);
        }

        [Test]
        public void TestDecryptFilesInteractively()
        {
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            Resolve.KnownIdentities.DefaultEncryptionIdentity = new LogOnIdentity(EmailAddress.Parse("id@axcrypt.net"), Passphrase.Create("b"));
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                throw new InvalidOperationException("Log on should not be called in this scenario.");
            };
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.axx");
                e.SelectedFiles.Add(@"C:\Folder\File2.axx");
            };
            mvm.DecryptFiles.Execute(null);

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 2), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
        }

        [Test]
        public void TestDecryptFilesWithCancel()
        {
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.Cancel = true;
            };

            mvm.DecryptFiles.Execute(null);

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.IsAny<IEnumerable<IDataStore>>(), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Never);
        }

        [Test]
        public void TestDecryptFilesWithList()
        {
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            Resolve.KnownIdentities.DefaultEncryptionIdentity = new LogOnIdentity(EmailAddress.Parse("id@axcrypt.net"), Passphrase.Create("b"));
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                throw new InvalidOperationException("Log on should not be called in this scenario.");
            };
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.axx");
                e.SelectedFiles.Add(@"C:\Folder\File2.axx");
            };

            mvm.DecryptFiles.Execute(new string[] { @"C:\Folder\File3.axx" });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 1), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
        }

        [Test]
        public void TestEncryptFilesInteractively()
        {
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = new Passphrase("a");
            };
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.txt");
                e.SelectedFiles.Add(@"C:\Folder\File2.txt");
            };

            Resolve.FileSystemState.KnownPassphrases.Add(Passphrase.Create("a"));
            mvm.EncryptFiles.Execute(null);

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 2), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
        }

        [Test]
        public void TestEncryptFilesWithCancel()
        {
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.Cancel = true;
            };

            mvm.EncryptFiles.Execute(null);

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.IsAny<IEnumerable<IDataStore>>(), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Never);
        }

        [Test]
        public void TestEncryptFilesWithList()
        {
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = new Passphrase("a");
            };
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.txt");
                e.SelectedFiles.Add(@"C:\Folder\File2.txt");
            };

            Resolve.FileSystemState.KnownPassphrases.Add(Passphrase.Create("a"));
            mvm.EncryptFiles.Execute(new string[] { @"C:\Folder\File3.txt" });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 1), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
        }

        [Test]
        public void TestOpenFilesWithList()
        {
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.OpenFiles.Execute(new string[] { @"C:\Folder\File3.txt" });
            Assert.That(_allCompleted, Is.True);
            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 1), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
        }

        [Test]
        public void TestLogOnLogOffWhenLoggedOn()
        {
            LogOnIdentity id = new LogOnIdentity("logonwhenloggedon");
            Resolve.FileSystemState.KnownPassphrases.Add(id.Passphrase);
            Resolve.KnownIdentities.DefaultEncryptionIdentity = id;

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.IdentityViewModel.LogOnLogOff.Execute(null);

            Assert.That(Resolve.KnownIdentities.Identities.Count(), Is.EqualTo(0));
        }

        [Test]
        public void TestLogOnLogOffWhenLoggedOffAndIdentityKnown()
        {
            Passphrase passphrase = new Passphrase("a");

            Passphrase identity = passphrase;
            Resolve.FileSystemState.KnownPassphrases.Add(identity);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = new Passphrase("a");
            };

            mvm.IdentityViewModel.LogOnLogOff.Execute(null);

            Assert.That(Resolve.KnownIdentities.Identities.Count(), Is.EqualTo(1), "There should be one known key now.");
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.True, "It should be logged on now.");
        }

        [Test]
        public void TestLogOnLogOffWhenLoggedOffAndNoIdentityKnown()
        {
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = new Passphrase("b");
                e.Name = "Name";
            };

            Resolve.FileSystemState.KnownPassphrases.Add(Passphrase.Create("b"));
            mvm.IdentityViewModel.LogOnLogOff.Execute(null);

            Assert.That(Resolve.KnownIdentities.Identities.Count(), Is.EqualTo(1));
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.True);
            Assert.That(Resolve.FileSystemState.KnownPassphrases.Count, Is.EqualTo(1));
        }

        [Test]
        public void TestLogOnLogOffWhenLoggedOffAndNoIdentityKnownAndNoPassphraseGiven()
        {
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = Passphrase.Empty;
            };

            mvm.IdentityViewModel.LogOnLogOff.Execute(null);

            Assert.That(Resolve.KnownIdentities.Identities.Count(), Is.EqualTo(0));
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.False);
            Assert.That(Resolve.FileSystemState.KnownPassphrases.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestDecryptFoldersWhenLoggedIn()
        {
            LogOnIdentity id = new LogOnIdentity("a");
            Resolve.FileSystemState.KnownPassphrases.Add(id.Passphrase);
            Resolve.KnownIdentities.DefaultEncryptionIdentity = id;

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.DecryptFolders.Execute(new string[] { @"C:\Folder\" });

            Assert.That(_allCompleted, Is.True);
            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataContainer>>(f => f.Count() == 1), It.IsAny<Func<IDataContainer, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
        }

        [Test]
        public void TestDecryptFoldersWhenNotLoggedIn()
        {
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            Assert.Throws<InvalidOperationException>(() => mvm.DecryptFolders.Execute(new string[] { @"C:\Folder\" }));
        }

        [Test]
        public void TestWipeFilesInteractively()
        {
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.txt");
                e.SelectedFiles.Add(@"C:\Folder\File2.txt");
            };

            mvm.WipeFiles.Execute(null);

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 2), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
        }

        [Test]
        public void TestWipeFilesWithCancel()
        {
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.Cancel = true;
            };

            mvm.WipeFiles.Execute(null);

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.IsAny<IEnumerable<IDataStore>>(), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Never);
        }

        [Test]
        public void TestWipeFilesWithList()
        {
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.txt");
                e.SelectedFiles.Add(@"C:\Folder\File2.txt");
            };

            mvm.WipeFiles.Execute(new string[] { @"C:\Folder\File3.txt" });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 1), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
        }

        [Test]
        public void TestOpenFilesFromFolderWithCancelWhenLoggedOn()
        {
            Resolve.KnownIdentities.DefaultEncryptionIdentity = new LogOnIdentity("b");

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.Cancel = true;
            };

            mvm.OpenFilesFromFolder.Execute(@"C:\Folder\");

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.IsAny<IEnumerable<IDataStore>>(), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Never);
        }

        [Test]
        public void TestOpenFilesFromFolderWhenLoggedOn()
        {
            Resolve.KnownIdentities.DefaultEncryptionIdentity = new LogOnIdentity("c");

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Clear();
                e.SelectedFiles.Add(@"C:\Folder\File1.axx");
                e.SelectedFiles.Add(@"C:\Folder\File2.axx");
            };

            mvm.OpenFilesFromFolder.Execute(@"C:\Folder\");

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataStore>>(files => files.Count() == 2), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Once);
        }

        [Test]
        public void TestEncryptFilesAction()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            TypeMap.Register.New<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.txt");
                e.SelectedFiles.Add(@"C:\Folder\File2.txt");
            };

            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = new Passphrase("a");
            };
            Resolve.FileSystemState.KnownPassphrases.Add(Passphrase.Create("a"));
            mvm.EncryptFiles.Execute(null);

            Assert.That(Resolve.FileSystemState.FindActiveFileFromEncryptedPath(@"C:\Folder\File1-txt.axx"), Is.Not.Null);
            Assert.That(Resolve.FileSystemState.FindActiveFileFromEncryptedPath(@"C:\Folder\File2-txt.axx"), Is.Not.Null);
            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 2), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipe(It.IsAny<IDataStore>(), It.IsAny<IDataStore>(), It.IsAny<EncryptionParameters>(), It.IsAny<IProgressContext>()), Times.Exactly(2));
        }

        [Test]
        public void TestEncryptFilesWithSaveAsAction()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            TypeMap.Register.New<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\Copy of File1-txt.axx");
            };

            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = new Passphrase("a");
            };

            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", null);
            Resolve.FileSystemState.KnownPassphrases.Add(Passphrase.Create("a"));
            mvm.EncryptFiles.Execute(new string[] { @"C:\Folder\File1.txt" });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 1), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipe(It.IsAny<IDataStore>(), It.IsAny<IDataStore>(), It.IsAny<EncryptionParameters>(), It.IsAny<IProgressContext>()), Times.Exactly(1));
        }

        [Test]
        public void TestEncryptFilesWithAlreadyEncryptedFile()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            TypeMap.Register.New<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = new Passphrase("a");
            };

            Resolve.FileSystemState.KnownPassphrases.Add(Passphrase.Create("a"));
            mvm.EncryptFiles.Execute(new string[] { @"C:\Folder\File1-txt.axx" });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 1), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipe(It.IsAny<IDataStore>(), It.IsAny<IDataStore>(), It.IsAny<EncryptionParameters>(), It.IsAny<IProgressContext>()), Times.Never);
        }

        [Test]
        public void TestEncryptFilesWithCanceledLoggingOnAction()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            TypeMap.Register.New<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Cancel = true;
            };

            mvm.EncryptFiles.Execute(new string[] { @"C:\Folder\File1.txt" });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 1), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Never);
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipe(It.IsAny<IDataStore>(), It.IsAny<IDataStore>(), It.IsAny<EncryptionParameters>(), It.IsAny<IProgressContext>()), Times.Never);
        }

        [Test]
        public void TestDecryptFileAction()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            TypeMap.Register.New<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            axCryptFileMock.Setup<IAxCryptDocument>(m => m.Document(It.IsAny<IDataStore>(), It.IsAny<LogOnIdentity>(), It.IsAny<IProgressContext>())).Returns((IDataStore fileInfo, LogOnIdentity passphrase, IProgressContext progress) =>
            {
                V1AxCryptDocument acd = new V1AxCryptDocument(passphrase.Passphrase, 117);
                acd.PassphraseIsValid = true;
                acd.FileName = fileInfo.FullName.Replace("-txt.axx", ".txt");
                return acd;
            });
            axCryptFileMock.Setup<EncryptedProperties>(m => m.CreateEncryptedProperties(It.IsAny<IDataStore>(), It.IsAny<LogOnIdentity>())).Returns((IDataStore fileInfo, LogOnIdentity passphrase) =>
            {
                EncryptedProperties properties = new EncryptedProperties(fileInfo.FullName.Replace("-txt.axx", ".txt"));
                properties.DecryptionParameter = new DecryptionParameter(passphrase.Passphrase, V1Aes128CryptoFactory.CryptoId);
                properties.IsValid = true;
                return properties;
            });
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            Mock<AxCryptFactory> axCryptFactoryMock = new Mock<AxCryptFactory>();
            axCryptFactoryMock.Setup<DecryptionParameter>(m => m.FindDecryptionParameter(It.IsAny<IEnumerable<DecryptionParameter>>(), It.IsAny<IDataStore>())).Returns((IEnumerable<DecryptionParameter> decryptionParameters, IDataStore fileInfo) =>
            {
                return new DecryptionParameter(Passphrase.Empty, V1Aes128CryptoFactory.CryptoId);
            });
            TypeMap.Register.New<AxCryptFactory>(() => axCryptFactoryMock.Object);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1-txt.axx");
                e.SelectedFiles.Add(@"C:\Folder\File2-txt.axx");
            };

            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                throw new InvalidOperationException("Log on should not be called in this scenario.");
            };
            Resolve.KnownIdentities.DefaultEncryptionIdentity = new LogOnIdentity(EmailAddress.Parse("test@axcrypt.net"), Passphrase.Create("bbb"));

            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", null);
            FakeDataStore.AddFile(@"C:\Folder\File2-txt.axx", null);
            mvm.DecryptFiles.Execute(null);

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 2), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
            // Intermittent only invoked once below. Needs investigation.
            axCryptFileMock.Verify(m => m.DecryptFile(It.IsAny<IAxCryptDocument>(), It.IsAny<string>(), It.IsAny<IProgressContext>()), Times.Exactly(2));
            axCryptFileMock.Verify(m => m.Wipe(It.IsAny<IDataStore>(), It.IsAny<IProgressContext>()), Times.Exactly(2));
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.True);
            Assert.That(Resolve.KnownIdentities.Identities.Count(), Is.EqualTo(1));
        }

        [Test]
        public void TestDecryptFileFileSaveAsAction()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            TypeMap.Register.New<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            axCryptFileMock.Setup<IAxCryptDocument>(m => m.Document(It.IsAny<IDataStore>(), It.IsAny<LogOnIdentity>(), It.IsAny<IProgressContext>())).Returns((IDataStore fileInfo, LogOnIdentity passphrase, IProgressContext progress) =>
            {
                V1AxCryptDocument acd = new V1AxCryptDocument(passphrase.Passphrase, 31);
                acd.PassphraseIsValid = true;
                acd.DocumentHeaders.FileName = Path.GetFileName(fileInfo.FullName.Replace("-txt.axx", ".txt"));
                return acd;
            });
            axCryptFileMock.Setup<EncryptedProperties>(m => m.CreateEncryptedProperties(It.IsAny<IDataStore>(), It.IsAny<LogOnIdentity>())).Returns((IDataStore fileInfo, LogOnIdentity passphrase) =>
            {
                EncryptedProperties properties = new EncryptedProperties(fileInfo.Name.Replace("-txt.axx", ".txt"));
                properties.DecryptionParameter = new DecryptionParameter(passphrase.Passphrase, V1Aes128CryptoFactory.CryptoId);
                properties.IsValid = true;
                return properties;
            });
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Clear();
                e.SelectedFiles.Add(@"C:\Folder\Copy of File1.txt".NormalizeFilePath());
            };

            Resolve.KnownIdentities.DefaultEncryptionIdentity = new LogOnIdentity(EmailAddress.Parse("testing@axcrypt.net"), Passphrase.Create("a"));
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                throw new InvalidOperationException("Log on should not be called in this scenario.");
            };
            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", new MemoryStream(Resources.helloworld_key_a_txt));
            FakeDataStore.AddFile(@"C:\Folder\File1.txt", null);
            mvm.DecryptFiles.Execute(new string[] { @"C:\Folder\File1-txt.axx".NormalizeFilePath() });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 1), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
            axCryptFileMock.Verify(m => m.DecryptFile(It.Is<IAxCryptDocument>((a) => a.FileName == @"File1.txt"), It.Is<string>((s) => s == @"C:\Folder\Copy of File1.txt".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Once);
            axCryptFileMock.Verify(m => m.Wipe(It.Is<IDataStore>((i) => i.FullName == @"C:\Folder\File1-txt.axx".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Once);
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.True);
        }

        [Test]
        public void TestDecryptFileFileSaveAsCanceledAction()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            TypeMap.Register.New<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            axCryptFileMock.Setup<IAxCryptDocument>(m => m.Document(It.IsAny<IDataStore>(), It.IsAny<LogOnIdentity>(), It.IsAny<IProgressContext>())).Returns((IDataStore fileInfo, Passphrase passphrase, IProgressContext progress) =>
            {
                V1AxCryptDocument acd = new V1AxCryptDocument(passphrase, 10);
                acd.PassphraseIsValid = true;
                acd.DocumentHeaders.FileName = Path.GetFileName(fileInfo.FullName.Replace("-txt.axx", ".txt"));
                return acd;
            });
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.Cancel = true;
            };

            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = new Passphrase("a");
            };
            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", null);
            FakeDataStore.AddFile(@"C:\Folder\File1.txt", null);
            mvm.DecryptFiles.Execute(new string[] { @"C:\Folder\File1-txt.axx" });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 0), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Never);
            axCryptFileMock.Verify(m => m.DecryptFile(It.IsAny<IAxCryptDocument>(), It.IsAny<string>(), It.IsAny<IProgressContext>()), Times.Never);
            axCryptFileMock.Verify(m => m.Wipe(It.IsAny<IDataStore>(), It.IsAny<IProgressContext>()), Times.Never);
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.False);
            Assert.That(Resolve.KnownIdentities.Identities.Count(), Is.EqualTo(1));
        }

        [Test]
        public void TestDecryptLoggingOnCanceledAction()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            TypeMap.Register.New<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Cancel = true;
            };

            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", null);
            mvm.DecryptFiles.Execute(new string[] { @"C:\Folder\File1-txt.axx" });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 0), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Never);
            axCryptFileMock.Verify(m => m.DecryptFile(It.IsAny<IAxCryptDocument>(), It.IsAny<string>(), It.IsAny<IProgressContext>()), Times.Never);
            axCryptFileMock.Verify(m => m.Wipe(It.IsAny<IDataStore>(), It.IsAny<IProgressContext>()), Times.Never);
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.False);
            Assert.That(Resolve.KnownIdentities.Identities.Count(), Is.EqualTo(0));
        }

        [Test]
        public void TestAddRecentFilesActionAddingEncryptedWithWork()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            TypeMap.Register.New<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            axCryptFileMock.Setup<IAxCryptDocument>(m => m.Document(It.IsAny<IDataStore>(), It.IsAny<LogOnIdentity>(), It.IsAny<IProgressContext>())).Returns((IDataStore fileInfo, LogOnIdentity passphrase, IProgressContext progress) =>
            {
                V1AxCryptDocument acd = new V1AxCryptDocument(passphrase.Passphrase, 10);
                acd.PassphraseIsValid = true;
                acd.DocumentHeaders.FileName = Path.GetFileName(fileInfo.FullName.Replace("-txt.axx", ".txt"));
                return acd;
            });
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = new Passphrase("a");
            };

            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", null);
            mvm.AddRecentFiles.Execute(new string[] { @"C:\Folder\File1-txt.axx" });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 1), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Once);
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.False);
            Assert.That(Resolve.KnownIdentities.Identities.Count(), Is.EqualTo(1));
        }

        [Test]
        public void TestAddRecentFilesActionAddingEncryptedButCancelingWithWork()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            TypeMap.Register.New<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            axCryptFileMock.Setup<IAxCryptDocument>(m => m.Document(It.IsAny<IDataStore>(), It.IsAny<LogOnIdentity>(), It.IsAny<IProgressContext>())).Returns((IDataStore fileInfo, Passphrase key, IProgressContext progress) =>
            {
                V1AxCryptDocument acd = new V1AxCryptDocument();
                acd.PassphraseIsValid = true;
                acd.DocumentHeaders.FileName = Path.GetFileName(fileInfo.FullName.Replace("-txt.axx", ".txt"));
                return acd;
            });
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            int logonTries = 0;
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Cancel = true;
                logonTries++;
            };

            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", null);
            FakeDataStore.AddFile(@"C:\Folder\File2-txt.axx", null);
            mvm.AddRecentFiles.Execute(new string[] { @"C:\Folder\File1-txt.axx", @"C:\Folder\File2-txt.axx" });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 2), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Once);
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.False);
            Assert.That(Resolve.KnownIdentities.Identities.Count(), Is.EqualTo(0));
            Assert.That(logonTries, Is.EqualTo(1), "There should be only one logon try, since the first one cancels.");
        }

        [Test]
        public void TestWipeFilesWithWork()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            TypeMap.Register.New<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
            };

            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", null);
            FakeDataStore.AddFile(@"C:\Folder\File2-txt.axx", null);
            mvm.WipeFiles.Execute(new string[] { @"C:\Folder\File1-txt.axx", @"C:\Folder\File2-txt.axx" });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 2), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Once);
            axCryptFileMock.Verify(m => m.Wipe(It.IsAny<IDataStore>(), It.IsAny<IProgressContext>()), Times.Exactly(2));
        }

        [Test]
        public void TestWipeFilesSkippingOneWithWork()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            TypeMap.Register.New<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                if (e.SelectedFiles[0].Contains("File2"))
                {
                    e.Skip = true;
                }
            };

            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", null);
            FakeDataStore.AddFile(@"C:\Folder\File2-txt.axx", null);
            FakeDataStore.AddFile(@"C:\Folder\File3-txt.axx", null);
            mvm.WipeFiles.Execute(new string[] { @"C:\Folder\File1-txt.axx", @"C:\Folder\File2-txt.axx", @"C:\Folder\File3-txt.axx" });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 3), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Once);
            axCryptFileMock.Verify(m => m.Wipe(It.Is<IDataStore>(r => r.FullName == @"C:\Folder\File1-txt.axx".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Once);
            axCryptFileMock.Verify(m => m.Wipe(It.Is<IDataStore>(r => r.FullName == @"C:\Folder\File2-txt.axx".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Never);
            axCryptFileMock.Verify(m => m.Wipe(It.Is<IDataStore>(r => r.FullName == @"C:\Folder\File3-txt.axx".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Once);
        }

        [Test]
        public void TestWipeFilesCancelingAfterOneWithWork()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            TypeMap.Register.New<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            int callTimes = 0;
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                ++callTimes;
                if (e.SelectedFiles[0].Contains("File2"))
                {
                    SimulateDelayOfUserInteractionAllowingEarlierThreadsToReallyStartTheAsyncPart();
                    e.Cancel = true;
                }
            };

            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", null);
            FakeDataStore.AddFile(@"C:\Folder\File2-txt.axx", null);
            FakeDataStore.AddFile(@"C:\Folder\File3-txt.axx", null);
            mvm.WipeFiles.Execute(new string[] { @"C:\Folder\File1-txt.axx", @"C:\Folder\File2-txt.axx", @"C:\Folder\File3-txt.axx" });

            Assert.That(callTimes, Is.EqualTo(2), "Only the first two calls should be made.");
            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 3), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()), Times.Once);
            axCryptFileMock.Verify(m => m.Wipe(It.IsAny<IDataStore>(), It.IsAny<IProgressContext>()), Times.Once);
            axCryptFileMock.Verify(m => m.Wipe(It.Is<IDataStore>(r => r.FullName == @"C:\Folder\File1-txt.axx".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Once);
            axCryptFileMock.Verify(m => m.Wipe(It.Is<IDataStore>(r => r.FullName == @"C:\Folder\File2-txt.axx".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Never);
            axCryptFileMock.Verify(m => m.Wipe(It.Is<IDataStore>(r => r.FullName == @"C:\Folder\File3-txt.axx".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Never);
        }

        private static void SimulateDelayOfUserInteractionAllowingEarlierThreadsToReallyStartTheAsyncPart()
        {
            Thread.Sleep(1);
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Encryptable", Justification = "Encryptable *is* a word.")]
        [Test]
        public void TestAddRecentFilesActionWithEncryptableFilesNonInteractive()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            TypeMap.Register.New<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            LogOnIdentity id = new LogOnIdentity("d");
            Resolve.FileSystemState.KnownPassphrases.Add(id.Passphrase);
            Resolve.KnownIdentities.DefaultEncryptionIdentity = id;

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", null);
            FakeDataStore.AddFile(@"C:\Folder\File3-txt.axx", null);
            FakeDataStore.AddFile(@"C:\Folder\File1.txt", null);
            FakeDataStore.AddFile(@"C:\Folder\File2.txt", null);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.AddRecentFiles.Execute(new string[] { @"C:\Folder\File1.txt", @"C:\Folder\File2.txt", @"C:\Folder\File3-txt.axx" });

            Assert.That(Resolve.FileSystemState.FindActiveFileFromEncryptedPath(@"C:\Folder\File1-txt.axx"), Is.Null);
            Assert.That(Resolve.FileSystemState.FindActiveFileFromEncryptedPath(@"C:\Folder\File1-txt.1.axx"), Is.Not.Null);
            Assert.That(Resolve.FileSystemState.FindActiveFileFromEncryptedPath(@"C:\Folder\File2-txt.axx"), Is.Not.Null);
            Assert.That(Resolve.FileSystemState.FindActiveFileFromEncryptedPath(@"C:\Folder\File3-txt.axx"), Is.Null);
            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 2), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipe(It.IsAny<IDataStore>(), It.Is<IDataStore>(r => r.FullName == @"C:\Folder\File1-txt.1.axx".NormalizeFilePath()), It.IsAny<EncryptionParameters>(), It.IsAny<IProgressContext>()), Times.Once);
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipe(It.IsAny<IDataStore>(), It.Is<IDataStore>(r => r.FullName == @"C:\Folder\File2-txt.axx".NormalizeFilePath()), It.IsAny<EncryptionParameters>(), It.IsAny<IProgressContext>()), Times.Once);
        }

        [Test]
        public void TestOpenFilesActionWithWork()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            TypeMap.Register.New<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            axCryptFileMock.Setup<IAxCryptDocument>(m => m.Document(It.IsAny<IDataStore>(), It.IsAny<LogOnIdentity>(), It.IsAny<IProgressContext>())).Returns((IDataStore fileInfo, LogOnIdentity passphrase, IProgressContext progress) =>
            {
                V1AxCryptDocument acd = new V1AxCryptDocument(passphrase.Passphrase, 87);
                acd.PassphraseIsValid = true;
                acd.DocumentHeaders.FileName = fileInfo.FullName.Replace("-txt.axx", ".txt");
                return acd;
            });
            axCryptFileMock.Setup<EncryptedProperties>(m => m.CreateEncryptedProperties(It.IsAny<IDataStore>(), It.IsAny<LogOnIdentity>())).Returns((IDataStore fileInfo, LogOnIdentity passphrase) =>
            {
                EncryptedProperties properties = new EncryptedProperties(fileInfo.FullName.Replace("-txt.axx", ".txt"));
                properties.DecryptionParameter = new DecryptionParameter(passphrase.Passphrase, V1Aes128CryptoFactory.CryptoId);
                properties.IsValid = true;
                return properties;
            });
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            Mock<AxCryptFactory> axCryptFactoryMock = new Mock<AxCryptFactory>();
            axCryptFactoryMock.Setup<DecryptionParameter>(m => m.FindDecryptionParameter(It.IsAny<IEnumerable<DecryptionParameter>>(), It.IsAny<IDataStore>())).Returns((IEnumerable<DecryptionParameter> decryptionParameters, IDataStore fileInfo) =>
            {
                return new DecryptionParameter(Passphrase.Empty, V1Aes128CryptoFactory.CryptoId);
            });
            TypeMap.Register.New<AxCryptFactory>(() => axCryptFactoryMock.Object);

            Mock<FileOperation> fileOperationMock = new Mock<FileOperation>(Resolve.FileSystemState, Resolve.SessionNotify);
            TypeMap.Register.New<FileOperation>(() => fileOperationMock.Object);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                e.Passphrase = new Passphrase("b");
            };

            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", null);
            FakeDataStore.AddFile(@"C:\Folder\File2-txt.axx", null);
            mvm.OpenFiles.Execute(new string[] { @"C:\Folder\File1-txt.axx", @"C:\Folder\File2-txt.axx" });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 2), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
            fileOperationMock.Verify(f => f.OpenAndLaunchApplication(It.IsAny<LogOnIdentity>(), It.Is<IDataStore>(ds => ds.FullName == @"C:\Folder\File1-txt.axx".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Once);
            fileOperationMock.Verify(f => f.OpenAndLaunchApplication(It.IsAny<LogOnIdentity>(), It.Is<IDataStore>(ds => ds.FullName == @"C:\Folder\File2-txt.axx".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Once);
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.False);
            Assert.That(Resolve.KnownIdentities.Identities.Count(), Is.EqualTo(1));
        }

        [Test]
        public void TestOpenFilesActionCancelingWithWork()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            TypeMap.Register.New<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            int count = 0;
            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            axCryptFileMock.Setup<EncryptedProperties>(m => m.CreateEncryptedProperties(It.IsAny<IDataStore>(), It.IsAny<LogOnIdentity>())).Returns((IDataStore fileInfo, LogOnIdentity passphrase) =>
            {
                EncryptedProperties properties = new EncryptedProperties(fileInfo.FullName.Replace("-txt.axx", ".txt"));
                properties.DecryptionParameter = new DecryptionParameter(passphrase.Passphrase, V1Aes128CryptoFactory.CryptoId);
                properties.IsValid = true;
                return properties;
            });

            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            Mock<FileOperation> fileOperationMock = new Mock<FileOperation>(Resolve.FileSystemState, Resolve.SessionNotify);
            TypeMap.Register.New<FileOperation>(() => fileOperationMock.Object);

            Mock<AxCryptFactory> axCryptFactoryMock = new Mock<AxCryptFactory>();
            axCryptFactoryMock.Setup(acf => acf.FindDecryptionParameter(It.IsAny<IEnumerable<DecryptionParameter>>(), It.IsAny<IDataStore>())).Returns((IEnumerable<DecryptionParameter> decryptionParameters, IDataStore fileInfo) =>
            {
                count++;
                return null;
            });
            TypeMap.Register.New<AxCryptFactory>(() => axCryptFactoryMock.Object);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOn += (sender, e) =>
            {
                if (count == 1)
                {
                    e.Cancel = true;
                    return;
                }
                e.Passphrase = new Passphrase("b");
            };

            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", Stream.Null);
            FakeDataStore.AddFile(@"C:\Folder\File2-txt.axx", Stream.Null);
            mvm.OpenFiles.Execute(new string[] { @"C:\Folder\File1-txt.axx", @"C:\Folder\File2-txt.axx" });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFiles(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 2), It.IsAny<Func<IDataStore, IProgressContext, FileOperationContext>>(), It.IsAny<Action<FileOperationContext>>()));
            fileOperationMock.Verify(f => f.OpenAndLaunchApplication(It.IsAny<LogOnIdentity>(), It.Is<IDataStore>(ds => ds.FullName == @"C:\Folder\File1-txt.axx".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Once);
            fileOperationMock.Verify(f => f.OpenAndLaunchApplication(It.IsAny<LogOnIdentity>(), It.Is<IDataStore>(ds => ds.FullName == @"C:\Folder\File2-txt.axx".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Never);
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.False, "Should be logged on.");
            Assert.That(Resolve.KnownIdentities.Identities.Count(), Is.EqualTo(1), "One known key.");
        }

        [Test]
        public void TestDecryptFoldersWithWork()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            TypeMap.Register.New<IProgressContext, FileOperationsController>((progress) => new FileOperationsController(progress));

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            LogOnIdentity id = new LogOnIdentity("e");
            Resolve.FileSystemState.KnownPassphrases.Add(id.Passphrase);
            Resolve.KnownIdentities.DefaultEncryptionIdentity = id;

            FakeDataStore.AddFile(@"C:\Folder1\File1-txt.axx", null);
            FakeDataStore.AddFile(@"C:\Folder1\File2-txt.axx", null);
            FakeDataStore.AddFile(@"C:\Folder2\File1-txt.axx", null);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.DecryptFolders.Execute(new string[] { @"C:\Folder1\", @"C:\Folder2\" });

            axCryptFileMock.Verify(m => m.DecryptFilesInsideFolderUniqueWithWipeOfOriginal(It.Is<IDataContainer>(r => r.FullName == @"C:\Folder1\".NormalizeFilePath()), It.IsAny<LogOnIdentity>(), It.IsAny<IStatusChecker>(), It.IsAny<IProgressContext>()), Times.Once);
            axCryptFileMock.Verify(m => m.DecryptFilesInsideFolderUniqueWithWipeOfOriginal(It.Is<IDataContainer>(r => r.FullName == @"C:\Folder2\".NormalizeFilePath()), It.IsAny<LogOnIdentity>(), It.IsAny<IStatusChecker>(), It.IsAny<IProgressContext>()), Times.Once);
        }
    }
}