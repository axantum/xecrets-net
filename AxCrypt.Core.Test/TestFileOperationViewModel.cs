﻿#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions Copyright © 2022-2025, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets Cli, but is derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * The changes and additions are separately copyrighted and only licensed under GPL v3 or later as detailed below,
 * unless explicitly licensed otherwise. If you use any part of these changes and additions in your software,
 * please see https://www.gnu.org/licenses/ for details of what this means for you.
 * 
 * Warning: If you are using the original AxCrypt code under a non-GPL v3 or later license, these changes and additions
 * are not included in that license. If you use these changes under those circumstances, all your code becomes subject to
 * the GPL v3 or later license, according to the principle of strong copyleft as applied to GPL v3 or later.
 *
 * Xecrets Cli is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets Cli is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets Cli. If not, see
 * https://www.gnu.org/licenses/.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions, as well for commit history detailing changes and additions that fall under the strong
 * copyleft provisions mentioned above. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Xecrets Cli Copyright and GPL License notice
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
 * The source is maintained at http://bitbucket.org/AxCrypt.Desktop.Window-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.Header;
using AxCrypt.Core.IO;
using AxCrypt.Core.Session;
using Xecrets.Net.Core.Test.Properties;
using AxCrypt.Core.UI;
using AxCrypt.Core.UI.ViewModel;
using AxCrypt.Fake;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace AxCrypt.Core.Test
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
            SetupAssembly.AssemblySetup(_cryptoImplementation);

            var mockParallelFile = new Mock<ParallelFileOperation>();
            _allCompleted = false;
            mockParallelFile.Setup(x => x.DoFilesAsync<IDataContainer>(It.IsAny<IEnumerable<IDataContainer>>(), It.IsAny<Func<IDataContainer, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()))
                .Callback<IEnumerable<IDataContainer>, Func<IDataContainer, IProgressContext, Task<FileOperationContext>>, Func<FileOperationContext, Task>>((files, work, allComplete) => { allComplete(new FileOperationContext(String.Empty, ErrorStatus.Success)); _allCompleted = true; }).Returns(Task.FromResult<FileOperationContext>(new FileOperationContext(String.Empty, ErrorStatus.Success)));
            mockParallelFile.Setup(x => x.DoFilesAsync<IDataStore>(It.IsAny<IEnumerable<IDataStore>>(), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()))
                .Callback<IEnumerable<IDataStore>, Func<IDataStore, IProgressContext, Task<FileOperationContext>>, Func<FileOperationContext, Task>>((files, work, allComplete) => { allComplete(new FileOperationContext(String.Empty, ErrorStatus.Success)); _allCompleted = true; }).Returns(Task.FromResult<FileOperationContext>(new FileOperationContext(String.Empty, ErrorStatus.Success)));
            TypeMap.Register.Singleton<ParallelFileOperation>(() => mockParallelFile.Object);
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public async Task TestAddRecentFiles()
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
            await Resolve.KnownIdentities.SetDefaultEncryptionIdentity(id);
            await mvm.AddRecentFiles.ExecuteAsync(new string[] { file1, file2, decrypted1 });
            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 3), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()), Times.Once);
            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 2), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()), Times.Once);
        }

        [Test]
        public async Task TestDecryptFilesInteractively()
        {
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            await Resolve.KnownIdentities.SetDefaultEncryptionIdentity(new LogOnIdentity(EmailAddress.Parse("id@axcrypt.net"), Passphrase.Create("b")));
            mvm.IdentityViewModel.LoggingOnAsync = (e) =>
            {
                throw new InvalidOperationException("Log on should not be called in this scenario.");
            };
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.axx");
                e.SelectedFiles.Add(@"C:\Folder\File2.axx");
            };
            await mvm.DecryptFiles.ExecuteAsync(null);

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 2), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()));
        }

        [Test]
        public async Task TestDecryptFilesWithCancel()
        {
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.Cancel = true;
            };

            await mvm.DecryptFiles.ExecuteAsync(null);

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.IsAny<IEnumerable<IDataStore>>(), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()), Times.Never);
        }

        [Test]
        public async Task TestDecryptFilesWithList()
        {
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            await Resolve.KnownIdentities.SetDefaultEncryptionIdentity(new LogOnIdentity(EmailAddress.Parse("id@axcrypt.net"), Passphrase.Create("b")));
            mvm.IdentityViewModel.LoggingOnAsync = (e) =>
            {
                throw new InvalidOperationException("Log on should not be called in this scenario.");
            };
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.axx");
                e.SelectedFiles.Add(@"C:\Folder\File2.axx");
            };

            await mvm.DecryptFiles.ExecuteAsync(new string[] { @"C:\Folder\File3.axx" });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 1), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()));
        }

        [Test]
        public async Task TestEncryptFilesInteractively()
        {
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            FakeDataStore.AddFile(@"C:\Folder\File1.txt", Stream.Null);
            FakeDataStore.AddFile(@"C:\Folder\File2.txt", Stream.Null);

            mvm.IdentityViewModel.LoggingOnAsync = (e) =>
            {
                e.Passphrase = new Passphrase("a");
                return Task.FromResult<object>(null);
            };
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.txt");
                e.SelectedFiles.Add(@"C:\Folder\File2.txt");
            };

            Resolve.FileSystemState.KnownPassphrases.Add(Passphrase.Create("a"));
            await mvm.EncryptFiles.ExecuteAsync(null);

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 2), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()));
        }

        [Test]
        public async Task TestEncryptFilesWithCancel()
        {
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.Cancel = true;
            };

            await mvm.EncryptFiles.ExecuteAsync(null);

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.IsAny<IEnumerable<IDataStore>>(), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()), Times.Never);
        }

        [Test]
        public async Task TestEncryptFilesWithList()
        {
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            FakeDataStore.AddFile(@"C:\Folder\File1.txt", Stream.Null);
            FakeDataStore.AddFile(@"C:\Folder\File2.txt", Stream.Null);
            FakeDataStore.AddFile(@"C:\Folder\File3.txt", Stream.Null);

            mvm.IdentityViewModel.LoggingOnAsync = (e) =>
            {
                e.Passphrase = new Passphrase("a");
                return Task.FromResult<object>(null);
            };
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.txt");
                e.SelectedFiles.Add(@"C:\Folder\File2.txt");
            };

            Resolve.FileSystemState.KnownPassphrases.Add(Passphrase.Create("a"));
            await mvm.EncryptFiles.ExecuteAsync(new string[] { @"C:\Folder\File3.txt" });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 1), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()));
        }

        [Test]
        public async Task TestOpenFilesWithList()
        {
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            await mvm.OpenFiles.ExecuteAsync(new string[] { @"C:\Folder\File3.txt" });
            Assert.That(_allCompleted, Is.True);
            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 1), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()));
        }

        [Test]
        public async Task TestLogOnLogOffWhenLoggedOn()
        {
            LogOnIdentity id = new LogOnIdentity("logonwhenloggedon");
            Resolve.FileSystemState.KnownPassphrases.Add(id.Passphrase);
            await Resolve.KnownIdentities.SetDefaultEncryptionIdentity(id);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            await mvm.IdentityViewModel.LogOnLogOff.ExecuteAsync(null);

            Assert.That(Resolve.KnownIdentities.Identities.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task TestLogOnLogOffWhenLoggedOffAndIdentityKnown()
        {
            Passphrase passphrase = new Passphrase("a");

            Passphrase identity = passphrase;
            Resolve.FileSystemState.KnownPassphrases.Add(identity);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOnAsync = (e) =>
            {
                e.Passphrase = new Passphrase("a");
                return Task.FromResult<object>(null);
            };

            await mvm.IdentityViewModel.LogOnLogOff.ExecuteAsync(null);

            Assert.That(Resolve.KnownIdentities.Identities.Count(), Is.EqualTo(1), "There should be one known key now.");
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.True, "It should be logged on now.");
        }

        [Test]
        public async Task TestLogOnLogOffWhenLoggedOffAndNoIdentityKnown()
        {
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOnAsync = (e) =>
            {
                e.Passphrase = new Passphrase("b");
                e.Name = "Name";
                return Task.FromResult<object>(null);
            };

            Resolve.FileSystemState.KnownPassphrases.Add(Passphrase.Create("b"));
            await mvm.IdentityViewModel.LogOnLogOff.ExecuteAsync(null);

            Assert.That(Resolve.KnownIdentities.Identities.Count(), Is.EqualTo(1));
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.True);
            Assert.That(Resolve.FileSystemState.KnownPassphrases.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task TestLogOnLogOffWhenLoggedOffAndNoIdentityKnownAndNoPassphraseGiven()
        {
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOnAsync = (e) =>
            {
                e.Passphrase = Passphrase.Empty;
                return Task.FromResult<object>(null);
            };

            await mvm.IdentityViewModel.LogOnLogOff.ExecuteAsync(null);

            Assert.That(Resolve.KnownIdentities.Identities.Count(), Is.EqualTo(0));
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.False);
            Assert.That(Resolve.FileSystemState.KnownPassphrases.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task TestDecryptFoldersWhenLoggedIn()
        {
            LogOnIdentity id = new LogOnIdentity("a");
            Resolve.FileSystemState.KnownPassphrases.Add(id.Passphrase);
            await Resolve.KnownIdentities.SetDefaultEncryptionIdentity(id);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            await mvm.DecryptFolders.ExecuteAsync(new string[] { @"C:\Folder\" });

            Assert.That(_allCompleted, Is.True);
            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataContainer>>(f => f.Count() == 1), It.IsAny<Func<IDataContainer, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()));
        }

        [Test]
        public void TestDecryptFoldersWhenNotLoggedIn()
        {
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            Assert.ThrowsAsync<InvalidOperationException>(async () => await mvm.DecryptFolders.ExecuteAsync(new string[] { @"C:\Folder\" }));
        }

        [Test]
        public async Task TestWipeFilesInteractively()
        {
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.txt");
                e.SelectedFiles.Add(@"C:\Folder\File2.txt");
            };

            await mvm.WipeFiles.ExecuteAsync(null);

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 2), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()));
        }

        [Test]
        public async Task TestWipeFilesWithCancel()
        {
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.Cancel = true;
            };

            await mvm.WipeFiles.ExecuteAsync(null);

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.IsAny<IEnumerable<IDataStore>>(), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()), Times.Never);
        }

        [Test]
        public async Task TestWipeFilesWithListTask()
        {
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.txt");
                e.SelectedFiles.Add(@"C:\Folder\File2.txt");
            };

            await mvm.WipeFiles.ExecuteAsync(new string[] { @"C:\Folder\File3.txt" });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 1), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()));
        }

        [Test]
        public async Task TestOpenFilesFromFolderWithCancelWhenLoggedOn()
        {
            await Resolve.KnownIdentities.SetDefaultEncryptionIdentity(new LogOnIdentity("b"));

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.Cancel = true;
            };

            await mvm.OpenFilesFromFolder.ExecuteAsync(@"C:\Folder\");

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.IsAny<IEnumerable<IDataStore>>(), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()), Times.Never);
        }

        [Test]
        public async Task TestOpenFilesFromFolderWhenLoggedOn()
        {
            await Resolve.KnownIdentities.SetDefaultEncryptionIdentity(new LogOnIdentity("c"));

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Clear();
                e.SelectedFiles.Add(@"C:\Folder\File1.axx");
                e.SelectedFiles.Add(@"C:\Folder\File2.axx");
            };

            await mvm.OpenFilesFromFolder.ExecuteAsync(@"C:\Folder\");

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(files => files.Count() == 2), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()), Times.Once);
        }

        [Test]
        public async Task TestEncryptFilesAction()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            FakeDataStore.AddFile(@"C:\Folder\File1.txt", Stream.Null);
            FakeDataStore.AddFile(@"C:\Folder\File2.txt", Stream.Null);

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.txt");
                e.SelectedFiles.Add(@"C:\Folder\File2.txt");
            };

            mvm.IdentityViewModel.LoggingOnAsync = (e) =>
            {
                e.Passphrase = new Passphrase("a");
                return Task.FromResult<object>(null);
            };
            Resolve.FileSystemState.KnownPassphrases.Add(Passphrase.Create("a"));
            await mvm.EncryptFiles.ExecuteAsync(null);

            Assert.That(Resolve.FileSystemState.FindActiveFileFromEncryptedPath(@"C:\Folder\File1-txt.axx"), Is.Not.Null);
            Assert.That(Resolve.FileSystemState.FindActiveFileFromEncryptedPath(@"C:\Folder\File2-txt.axx"), Is.Not.Null);
            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 2), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()));
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipeAsync(It.IsAny<IDataStore>(), It.IsAny<FileLock>(), It.IsAny<EncryptionParameters>(), It.IsAny<IProgressContext>()), Times.Exactly(2));
        }

        [Test]
        public async Task TestEncryptFileAction()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            FakeDataStore.AddFile(@"C:\Folder\File1.txt", Stream.Null);

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1.txt");
            };

            mvm.IdentityViewModel.LoggingOnAsync = (e) =>
            {
                e.Passphrase = new Passphrase("a");
                return Task.FromResult<object>(null);
            };
            Resolve.FileSystemState.KnownPassphrases.Add(Passphrase.Create("a"));
            await mvm.EncryptFiles.ExecuteAsync(null);

            Assert.That(Resolve.FileSystemState.FindActiveFileFromEncryptedPath(@"C:\Folder\File1-txt.axx"), Is.Not.Null);
            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 1), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()));
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipeAsync(It.IsAny<IDataStore>(), It.IsAny<FileLock>(), It.IsAny<EncryptionParameters>(), It.IsAny<IProgressContext>()), Times.Exactly(1));
        }

        [Test]
        public async Task TestEncryptFilesWithSaveAsAction()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            FakeDataStore.AddFile(@"C:\Folder\Copy of File1-txt.axx", Stream.Null);
            FakeDataStore.AddFile(@"C:\Folder\File1.txt", Stream.Null);

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\Copy of File1-txt.axx");
            };

            mvm.IdentityViewModel.LoggingOnAsync = (e) =>
            {
                e.Passphrase = new Passphrase("a");
                return Task.FromResult<object>(null);
            };

            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", null);
            Resolve.FileSystemState.KnownPassphrases.Add(Passphrase.Create("a"));
            await mvm.EncryptFiles.ExecuteAsync(new string[] { @"C:\Folder\File1.txt" });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 1), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()));
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipeAsync(It.IsAny<IDataStore>(), It.IsAny<FileLock>(), It.IsAny<EncryptionParameters>(), It.IsAny<IProgressContext>()), Times.Exactly(1));
        }

        [Test]
        public async Task TestEncryptFilesWithAlreadyEncryptedFile()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", Stream.Null);
            FakeDataStore.AddFile(@"C:\Folder\File2.txt", Stream.Null);

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOnAsync = (e) =>
            {
                e.Passphrase = new Passphrase("a");
                return Task.FromResult<object>(null);
            };

            Resolve.FileSystemState.KnownPassphrases.Add(Passphrase.Create("a"));
            await mvm.EncryptFiles.ExecuteAsync(new string[] { @"C:\Folder\File1-txt.axx", @"C:\Folder\File2.txt", });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 2), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()));
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipeAsync(It.IsAny<IDataStore>(), It.IsAny<FileLock>(), It.IsAny<EncryptionParameters>(), It.IsAny<IProgressContext>()), Times.Once);
        }

        [Test]
        public async Task TestEncryptFilesWithCanceledLoggingOnAction()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOnAsync = (e) =>
            {
                e.Cancel = true;
                return Task.FromResult<object>(null);
            };

            await mvm.EncryptFiles.ExecuteAsync(new string[] { @"C:\Folder\File1.txt" });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 1), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()), Times.Never);
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipeAsync(It.IsAny<IDataStore>(), It.IsAny<FileLock>(), It.IsAny<EncryptionParameters>(), It.IsAny<IProgressContext>()), Times.Never);
        }

        [Test]
        public async Task TestDecryptFileAction()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);

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
                properties.DecryptionParameter = new DecryptionParameter(passphrase.Passphrase, new V1Aes128CryptoFactory().CryptoId);
                properties.IsValid = true;
                return properties;
            });
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            Mock<AxCryptFactory> axCryptFactoryMock = new Mock<AxCryptFactory>();
            axCryptFactoryMock.Setup<DecryptionParameter>(m => m.FindDecryptionParameter(It.IsAny<IEnumerable<DecryptionParameter>>(), It.IsAny<IDataStore>())).Returns((IEnumerable<DecryptionParameter> decryptionParameters, IDataStore fileInfo) =>
            {
                return new DecryptionParameter(Passphrase.Empty, new V1Aes128CryptoFactory().CryptoId);
            });
            axCryptFactoryMock.Setup<Headers>(m => m.Headers(It.IsAny<Stream>())).Returns((Stream s) =>
            {
                return new Headers();
            });
            TypeMap.Register.New<AxCryptFactory>(() => axCryptFactoryMock.Object);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
                e.SelectedFiles.Add(@"C:\Folder\File1-txt.axx");
                e.SelectedFiles.Add(@"C:\Folder\File2-txt.axx");
            };

            mvm.IdentityViewModel.LoggingOnAsync = (e) =>
            {
                throw new InvalidOperationException("Log on should not be called in this scenario.");
            };
            await Resolve.KnownIdentities.SetDefaultEncryptionIdentity(new LogOnIdentity(EmailAddress.Parse("test@axcrypt.net"), Passphrase.Create("bbb")));

            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", FakeDataStore.ExpandableMemoryStream(Resources.helloworld_key_a_txt));
            FakeDataStore.AddFile(@"C:\Folder\File2-txt.axx", FakeDataStore.ExpandableMemoryStream(Resources.helloworld_key_a_txt));

            await mvm.DecryptFiles.ExecuteAsync(null);
            Assert.That((New<IStatusChecker>() as FakeStatusChecker).ErrorSeen, Is.False, "Oops, an erorror happened.");

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 2), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()));
            // Intermittent only invoked once below. Needs investigation.
            axCryptFileMock.Verify(m => m.DecryptFile(It.IsAny<IAxCryptDocument>(), It.IsAny<string>(), It.IsAny<IProgressContext>()), Times.Exactly(2));
            axCryptFileMock.Verify(m => m.Wipe(It.IsAny<FileLock>(), It.IsAny<IProgressContext>()), Times.Exactly(2));
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.True);
            Assert.That(Resolve.KnownIdentities.Identities.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task TestDecryptFileFileSaveAsAction()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);

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
                properties.DecryptionParameter = new DecryptionParameter(passphrase.Passphrase, new V1Aes128CryptoFactory().CryptoId);
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

            await Resolve.KnownIdentities.SetDefaultEncryptionIdentity(new LogOnIdentity(EmailAddress.Parse("testing@axcrypt.net"), Passphrase.Create("a")));
            mvm.IdentityViewModel.LoggingOnAsync = (e) =>
            {
                throw new InvalidOperationException("Log on should not be called in this scenario.");
            };
            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", new MemoryStream(Resources.helloworld_key_a_txt));
            FakeDataStore.AddFile(@"C:\Folder\File1.txt", null);
            await mvm.DecryptFiles.ExecuteAsync(new string[] { @"C:\Folder\File1-txt.axx".NormalizeFilePath() });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 1), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()));
            axCryptFileMock.Verify(m => m.DecryptFile(It.Is<IAxCryptDocument>((a) => a.FileName == @"File1.txt"), It.Is<string>((s) => s == @"C:\Folder\Copy of File1.txt".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Once);
            axCryptFileMock.Verify(m => m.Wipe(It.Is<FileLock>((i) => i.DataStore.FullName == @"C:\Folder\File1-txt.axx".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Once);
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.True);
        }

        [Test]
        public async Task TestDecryptFileFileSaveAsCanceledAction()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);

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

            mvm.IdentityViewModel.LoggingOnAsync = (e) =>
            {
                e.Passphrase = new Passphrase("a");
                return Task.FromResult<object>(null);
            };
            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", null);
            FakeDataStore.AddFile(@"C:\Folder\File1.txt", null);
            await mvm.DecryptFiles.ExecuteAsync(new string[] { @"C:\Folder\File1-txt.axx" });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 0), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()), Times.Never);
            axCryptFileMock.Verify(m => m.DecryptFile(It.IsAny<IAxCryptDocument>(), It.IsAny<string>(), It.IsAny<IProgressContext>()), Times.Never);
            axCryptFileMock.Verify(m => m.Wipe(It.IsAny<FileLock>(), It.IsAny<IProgressContext>()), Times.Never);
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.False);
            Assert.That(Resolve.KnownIdentities.Identities.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task TestDecryptLoggingOnCanceledAction()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOnAsync = (e) =>
            {
                e.Cancel = true;
                return Task.FromResult<object>(null);
            };

            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", null);
            await mvm.DecryptFiles.ExecuteAsync(new string[] { @"C:\Folder\File1-txt.axx" });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 0), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()), Times.Never);
            axCryptFileMock.Verify(m => m.DecryptFile(It.IsAny<IAxCryptDocument>(), It.IsAny<string>(), It.IsAny<IProgressContext>()), Times.Never);
            axCryptFileMock.Verify(m => m.Wipe(It.IsAny<FileLock>(), It.IsAny<IProgressContext>()), Times.Never);
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.False);
            Assert.That(Resolve.KnownIdentities.Identities.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task TestAddRecentFilesActionAddingEncryptedWithWork()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);

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
            mvm.IdentityViewModel.LoggingOnAsync = (e) =>
            {
                e.Passphrase = new Passphrase("a");
                return Task.FromResult<object>(null);
            };

            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", null);
            await mvm.AddRecentFiles.ExecuteAsync(new string[] { @"C:\Folder\File1-txt.axx" });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 1), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()), Times.Once);
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.False);
            Assert.That(Resolve.KnownIdentities.Identities.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task TestAddRecentFilesActionAddingEncryptedButCancelingWithWork()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);

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
            mvm.IdentityViewModel.LoggingOnAsync = (e) =>
            {
                e.Cancel = true;
                logonTries++;
                return Task.FromResult<object>(null);
            };

            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", null);
            FakeDataStore.AddFile(@"C:\Folder\File2-txt.axx", null);
            await mvm.AddRecentFiles.ExecuteAsync(new string[] { @"C:\Folder\File1-txt.axx", @"C:\Folder\File2-txt.axx" });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 2), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()), Times.Once);
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.False);
            Assert.That(Resolve.KnownIdentities.Identities.Count(), Is.EqualTo(0));
            Assert.That(logonTries, Is.EqualTo(1), "There should be only one logon try, since the first one cancels.");
        }

        [Test]
        public async Task TestWipeFilesWithWork()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.SelectingFiles += (sender, e) =>
            {
            };

            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", null);
            FakeDataStore.AddFile(@"C:\Folder\File2-txt.axx", null);
            await mvm.WipeFiles.ExecuteAsync(new string[] { @"C:\Folder\File1-txt.axx", @"C:\Folder\File2-txt.axx" });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 2), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()), Times.Once);
            axCryptFileMock.Verify(m => m.Wipe(It.IsAny<FileLock>(), It.IsAny<IProgressContext>()), Times.Exactly(2));
        }

        [Test]
        public async Task TestWipeFilesSkippingOneWithWork()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);

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
            await mvm.WipeFiles.ExecuteAsync(new string[] { @"C:\Folder\File1-txt.axx", @"C:\Folder\File2-txt.axx", @"C:\Folder\File3-txt.axx" });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 3), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()), Times.Once);
            axCryptFileMock.Verify(m => m.Wipe(It.Is<FileLock>(r => r.DataStore.FullName == @"C:\Folder\File1-txt.axx".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Once);
            axCryptFileMock.Verify(m => m.Wipe(It.Is<FileLock>(r => r.DataStore.FullName == @"C:\Folder\File2-txt.axx".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Never);
            axCryptFileMock.Verify(m => m.Wipe(It.Is<FileLock>(r => r.DataStore.FullName == @"C:\Folder\File3-txt.axx".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Once);
        }

        // Intermittent. See intermittent issue with TestDecryptFilesUniqueWithWipeOfOriginal.
        // Expected invocation on the mock once, but was 0 times: m => m.Wipe(It.IsAny<IDataStore>(), It.IsAny<IProgressContext>())
        // No setups configured.
        // No invocations performed.
        // Last failure: 2016-12-20
        [Test]
        public async Task TestWipeFilesCancelingAfterOneWithWork()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            int callTimes = 0;
            FileOperationViewModel mvm = New<FileOperationViewModel>();
            string selectedFile = string.Empty;
            mvm.SelectingFiles += (sender, e) =>
            {
                ++callTimes;
                if (callTimes == 2)
                {
                    e.Cancel = true;
                    return;
                }
                selectedFile = e.SelectedFiles.First();
            };

            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", null);
            FakeDataStore.AddFile(@"C:\Folder\File2-txt.axx", null);
            FakeDataStore.AddFile(@"C:\Folder\File3-txt.axx", null);
            await mvm.WipeFiles.ExecuteAsync(new string[] { @"C:\Folder\File1-txt.axx", @"C:\Folder\File2-txt.axx", @"C:\Folder\File3-txt.axx" });

            Assert.That(callTimes, Is.EqualTo(2), "Only the first two calls should be made.");
            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 3), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()), Times.Once);
            axCryptFileMock.Verify(m => m.Wipe(It.IsAny<FileLock>(), It.IsAny<IProgressContext>()), Times.Once);
            axCryptFileMock.Verify(m => m.Wipe(It.Is<FileLock>(r => r.DataStore.FullName == selectedFile.NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Once);
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Encryptable", Justification = "Encryptable *is* a word.")]
        [Test]
        public async Task TestAddRecentFilesActionWithFewEncryptableFilesNonInteractive()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);

            LogOnIdentity id = new LogOnIdentity("d");
            Resolve.FileSystemState.KnownPassphrases.Add(id.Passphrase);
            await Resolve.KnownIdentities.SetDefaultEncryptionIdentity(id);

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", null);
            FakeDataStore.AddFile(@"C:\Folder\File3-txt.axx", null);
            FakeDataStore.AddFile(@"C:\Folder\File1.txt", null);
            FakeDataStore.AddFile(@"C:\Folder\File2.txt", null);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            await mvm.AddRecentFiles.ExecuteAsync(new string[] { @"C:\Folder\File1.txt", @"C:\Folder\File2.txt", @"C:\Folder\File3-txt.axx" });

            Assert.That(Resolve.FileSystemState.FindActiveFileFromEncryptedPath(@"C:\Folder\File1-txt.axx"), Is.Null);
            Assert.That(Resolve.FileSystemState.FindActiveFileFromEncryptedPath(@"C:\Folder\File1-txt.1.axx"), Is.Not.Null);
            Assert.That(Resolve.FileSystemState.FindActiveFileFromEncryptedPath(@"C:\Folder\File2-txt.axx"), Is.Not.Null);
            Assert.That(Resolve.FileSystemState.FindActiveFileFromEncryptedPath(@"C:\Folder\File3-txt.axx"), Is.Null);
            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 3), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()));
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipeAsync(It.IsAny<IDataStore>(), It.Is<FileLock>(r => r.DataStore.FullName == @"C:\Folder\File1-txt.1.axx".NormalizeFilePath()), It.IsAny<EncryptionParameters>(), It.IsAny<IProgressContext>()), Times.Once);
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipeAsync(It.IsAny<IDataStore>(), It.Is<FileLock>(r => r.DataStore.FullName == @"C:\Folder\File2-txt.axx".NormalizeFilePath()), It.IsAny<EncryptionParameters>(), It.IsAny<IProgressContext>()), Times.Once);
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Encryptable", Justification = "Encryptable *is* a word.")]
        [Test]
        public async Task TestAddRecentFilesActionWithManyEncryptableFilesNonInteractive()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);
            New<UserSettings>().FewFilesThreshold = 1;

            LogOnIdentity id = new LogOnIdentity("d");
            Resolve.FileSystemState.KnownPassphrases.Add(id.Passphrase);
            await Resolve.KnownIdentities.SetDefaultEncryptionIdentity(id);

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", null);
            FakeDataStore.AddFile(@"C:\Folder\File3-txt.axx", null);
            FakeDataStore.AddFile(@"C:\Folder\File1.txt", null);
            FakeDataStore.AddFile(@"C:\Folder\File2.txt", null);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            await mvm.AddRecentFiles.ExecuteAsync(new string[] { @"C:\Folder\File1.txt", @"C:\Folder\File2.txt", @"C:\Folder\File3-txt.axx" });

            Assert.That(Resolve.FileSystemState.FindActiveFileFromEncryptedPath(@"C:\Folder\File1-txt.axx"), Is.Null);
            Assert.That(Resolve.FileSystemState.FindActiveFileFromEncryptedPath(@"C:\Folder\File1-txt.1.axx"), Is.Null);
            Assert.That(Resolve.FileSystemState.FindActiveFileFromEncryptedPath(@"C:\Folder\File2-txt.axx"), Is.Null);
            Assert.That(Resolve.FileSystemState.FindActiveFileFromEncryptedPath(@"C:\Folder\File3-txt.axx"), Is.Null);
            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 3), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()));
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipeAsync(It.IsAny<IDataStore>(), It.Is<FileLock>(r => r.DataStore.FullName == @"C:\Folder\File1-txt.1.axx".NormalizeFilePath()), It.IsAny<EncryptionParameters>(), It.IsAny<IProgressContext>()), Times.Once);
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipeAsync(It.IsAny<IDataStore>(), It.Is<FileLock>(r => r.DataStore.FullName == @"C:\Folder\File2-txt.axx".NormalizeFilePath()), It.IsAny<EncryptionParameters>(), It.IsAny<IProgressContext>()), Times.Once);
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Encryptable", Justification = "Encryptable *is* a word.")]
        [Test]
        public async Task TestAddRecentFileActionWithEncryptableFileNonInteractive()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);

            LogOnIdentity id = new LogOnIdentity("d");
            Resolve.FileSystemState.KnownPassphrases.Add(id.Passphrase);
            await Resolve.KnownIdentities.SetDefaultEncryptionIdentity(id);

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", null);
            FakeDataStore.AddFile(@"C:\Folder\File3-txt.axx", null);
            FakeDataStore.AddFile(@"C:\Folder\File1.txt", null);
            FakeDataStore.AddFile(@"C:\Folder\File2.txt", null);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            await mvm.AddRecentFiles.ExecuteAsync(new string[] { @"C:\Folder\File1.txt" });

            Assert.That(Resolve.FileSystemState.FindActiveFileFromEncryptedPath(@"C:\Folder\File1-txt.1.axx"), Is.Not.Null);
            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 1), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()));
            axCryptFileMock.Verify(m => m.EncryptFileWithBackupAndWipeAsync(It.IsAny<IDataStore>(), It.Is<FileLock>(r => r.DataStore.FullName == @"C:\Folder\File1-txt.1.axx".NormalizeFilePath()), It.IsAny<EncryptionParameters>(), It.IsAny<IProgressContext>()), Times.Once);
        }

        [Test]
        public async Task TestOpenFilesActionWithWork()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);

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
                properties.DecryptionParameter = new DecryptionParameter(passphrase.Passphrase, new V1Aes128CryptoFactory().CryptoId);
                properties.IsValid = true;
                return properties;
            });
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            Mock<AxCryptFactory> axCryptFactoryMock = new Mock<AxCryptFactory>();
            axCryptFactoryMock.Setup<DecryptionParameter>(m => m.FindDecryptionParameter(It.IsAny<IEnumerable<DecryptionParameter>>(), It.IsAny<IDataStore>())).Returns((IEnumerable<DecryptionParameter> decryptionParameters, IDataStore fileInfo) =>
            {
                return new DecryptionParameter(Passphrase.Empty, new V1Aes128CryptoFactory().CryptoId);
            });
            axCryptFactoryMock.Setup<Headers>(m => m.Headers(It.IsAny<Stream>())).Returns((Stream s) =>
            {
                return new Headers();
            });
            TypeMap.Register.New<AxCryptFactory>(() => axCryptFactoryMock.Object);

            Mock<FileOperation> fileOperationMock = new Mock<FileOperation>(Resolve.FileSystemState, Resolve.SessionNotify);
            fileOperationMock.Setup(fo => fo.OpenAndLaunchApplication(It.IsAny<LogOnIdentity>(), It.IsAny<IDataStore>(), It.IsAny<IProgressContext>())).Returns(Task.FromResult(new FileOperationContext(string.Empty, ErrorStatus.Success)));
            TypeMap.Register.New<FileOperation>(() => fileOperationMock.Object);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            mvm.IdentityViewModel.LoggingOnAsync = (e) =>
            {
                e.Passphrase = new Passphrase("b");
                return Task.FromResult<object>(null);
            };

            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", FakeDataStore.ExpandableMemoryStream(Resources.helloworld_key_a_txt));
            FakeDataStore.AddFile(@"C:\Folder\File2-txt.axx", FakeDataStore.ExpandableMemoryStream(Resources.helloworld_key_a_txt));

            await mvm.OpenFiles.ExecuteAsync(new string[] { @"C:\Folder\File1-txt.axx", @"C:\Folder\File2-txt.axx" });
            Assert.That((New<IStatusChecker>() as FakeStatusChecker).ErrorSeen, Is.False, "Oops, an erorror happened.");

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 2), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()));
            fileOperationMock.Verify(f => f.OpenAndLaunchApplication(It.IsAny<LogOnIdentity>(), It.Is<IDataStore>(ds => ds.FullName == @"C:\Folder\File1-txt.axx".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Once);
            fileOperationMock.Verify(f => f.OpenAndLaunchApplication(It.IsAny<LogOnIdentity>(), It.Is<IDataStore>(ds => ds.FullName == @"C:\Folder\File2-txt.axx".NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Once);
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.False);
            Assert.That(Resolve.KnownIdentities.Identities.Count(), Is.EqualTo(1));
        }

        // Intermittently failed 2016-07-01 - the OpenAndLaunchApplication expected once was not called.
        [Test]
        public async Task TestOpenFilesActionCancelingWithWork()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);

            int count = 0;
            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            axCryptFileMock.Setup<EncryptedProperties>(m => m.CreateEncryptedProperties(It.IsAny<IDataStore>(), It.IsAny<LogOnIdentity>())).Returns((IDataStore fileInfo, LogOnIdentity passphrase) =>
            {
                EncryptedProperties properties = new EncryptedProperties(fileInfo.FullName.Replace("-txt.axx", ".txt"));
                properties.DecryptionParameter = new DecryptionParameter(passphrase.Passphrase, new V1Aes128CryptoFactory().CryptoId);
                properties.IsValid = true;
                return properties;
            });

            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            Mock<FileOperation> fileOperationMock = new Mock<FileOperation>(Resolve.FileSystemState, Resolve.SessionNotify);
            fileOperationMock.Setup(fo => fo.OpenAndLaunchApplication(It.IsAny<LogOnIdentity>(), It.IsAny<IDataStore>(), It.IsAny<IProgressContext>())).Returns(Task.FromResult(new FileOperationContext(string.Empty, ErrorStatus.Success)));
            TypeMap.Register.New<FileOperation>(() => fileOperationMock.Object);

            Mock<AxCryptFactory> axCryptFactoryMock = new Mock<AxCryptFactory>();
            axCryptFactoryMock.Setup(acf => acf.FindDecryptionParameter(It.IsAny<IEnumerable<DecryptionParameter>>(), It.IsAny<IDataStore>())).Returns((IEnumerable<DecryptionParameter> decryptionParameters, IDataStore fileInfo) =>
            {
                count++;
                return null;
            });
            TypeMap.Register.New<AxCryptFactory>(() => axCryptFactoryMock.Object);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            string nameExcecuted = string.Empty;
            string nameSkipped = string.Empty;
            mvm.IdentityViewModel.LoggingOnAsync = (e) =>
            {
                if (count == 1)
                {
                    nameSkipped = e.EncryptedFileFullName;
                    e.Cancel = true;
                    return Task.FromResult<object>(null);
                }
                nameExcecuted = e.EncryptedFileFullName;
                e.Passphrase = new Passphrase("b");
                return Task.FromResult<object>(null);
            };

            FakeDataStore.AddFile(@"C:\Folder\File1-txt.axx", Stream.Null);
            FakeDataStore.AddFile(@"C:\Folder\File2-txt.axx", Stream.Null);
            await mvm.OpenFiles.ExecuteAsync(new string[] { @"C:\Folder\File1-txt.axx", @"C:\Folder\File2-txt.axx" });

            Mock.Get(Resolve.ParallelFileOperation).Verify(x => x.DoFilesAsync(It.Is<IEnumerable<IDataStore>>(f => f.Count() == 2), It.IsAny<Func<IDataStore, IProgressContext, Task<FileOperationContext>>>(), It.IsAny<Func<FileOperationContext, Task>>()));
            fileOperationMock.Verify(f => f.OpenAndLaunchApplication(It.IsAny<LogOnIdentity>(), It.Is<IDataStore>(ds => ds.FullName == nameExcecuted.NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Once);
            fileOperationMock.Verify(f => f.OpenAndLaunchApplication(It.IsAny<LogOnIdentity>(), It.Is<IDataStore>(ds => ds.FullName == nameSkipped.NormalizeFilePath()), It.IsAny<IProgressContext>()), Times.Never);
            Assert.That(Resolve.KnownIdentities.IsLoggedOn, Is.False, "Should be logged on.");
            Assert.That(Resolve.KnownIdentities.Identities.Count(), Is.EqualTo(1), "One known key.");
        }

        [Test]
        public async Task TestDecryptFoldersWithWork()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new Mock<ParallelFileOperation>() { CallBase = true }.Object);

            Mock<AxCryptFile> axCryptFileMock = new Mock<AxCryptFile>();
            TypeMap.Register.New<AxCryptFile>(() => axCryptFileMock.Object);

            LogOnIdentity id = new LogOnIdentity("e");
            Resolve.FileSystemState.KnownPassphrases.Add(id.Passphrase);
            await Resolve.KnownIdentities.SetDefaultEncryptionIdentity(id);

            FakeDataStore.AddFile(@"C:\Folder1\File1-txt.axx", null);
            FakeDataStore.AddFile(@"C:\Folder1\File2-txt.axx", null);
            FakeDataStore.AddFile(@"C:\Folder2\File1-txt.axx", null);

            FileOperationViewModel mvm = New<FileOperationViewModel>();
            await mvm.DecryptFolders.ExecuteAsync(new string[] { @"C:\Folder1\", @"C:\Folder2\" });

            axCryptFileMock.Verify(m => m.DecryptFilesInsideFolderUniqueWithWipeOfOriginalAsync(It.Is<IDataContainer>(r => r.FullName == @"C:\Folder1\".NormalizeFilePath()), It.IsAny<LogOnIdentity>(), It.IsAny<IStatusChecker>(), It.IsAny<IProgressContext>()), Times.Once);
            axCryptFileMock.Verify(m => m.DecryptFilesInsideFolderUniqueWithWipeOfOriginalAsync(It.Is<IDataContainer>(r => r.FullName == @"C:\Folder2\".NormalizeFilePath()), It.IsAny<LogOnIdentity>(), It.IsAny<IStatusChecker>(), It.IsAny<IProgressContext>()), Times.Once);
        }
    }
}
