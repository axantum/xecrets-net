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
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestFileSystemState
    {
        private static readonly string _rootPath = Path.GetPathRoot(Environment.CurrentDirectory);
        private static readonly string _encryptedAxxPath = Path.Combine(_rootPath, "Encrypted-txt.axx");
        private static readonly string _encrypted1AxxPath = Path.Combine(_rootPath, "Encrypted1-txt.axx");
        private static readonly string _encrypted2AxxPath = Path.Combine(_rootPath, "Encrypted2-txt.axx");
        private static readonly string _encrypted3AxxPath = Path.Combine(_rootPath, "Encrypted3-txt.axx");
        private static readonly string _encrypted4AxxPath = Path.Combine(_rootPath, "Encrypted4-txt.axx");
        private static readonly string _decryptedTxtPath = Path.Combine(_rootPath, "Decrypted.txt");
        private static readonly string _decrypted1TxtPath = Path.Combine(_rootPath, "Decrypted1.txt");
        private static readonly string _decrypted2TxtPath = Path.Combine(_rootPath, "Decrypted2.txt");
        private static readonly string _decrypted3TxtPath = Path.Combine(_rootPath, "Decrypted3.txt");
        private static readonly string _decrypted4TxtPath = Path.Combine(_rootPath, "Decrypted4.txt");

        private CryptoImplementation _cryptoImplementation;

        public TestFileSystemState(CryptoImplementation cryptoImplementation)
        {
            _cryptoImplementation = cryptoImplementation;
        }

        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup();
            SetupAssembly.AssemblySetupCrypto(_cryptoImplementation);
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public void TestLoadNew()
        {
            using (FileSystemState state = FileSystemState.Create(Resolve.WorkFolder.FileInfo.FileItemInfo("mystate.txt")))
            {
                Assert.That(state, Is.Not.Null, "An instance should always be instantiated.");
                Assert.That(state.ActiveFiles.Count(), Is.EqualTo(0), "A new state should not have any active files.");
            }
        }

        [Test]
        public async Task TestLoadExistingWithExistingFile()
        {
            ActiveFile activeFile;
            using (FileSystemState state = FileSystemState.Create(Resolve.WorkFolder.FileInfo.FileItemInfo("mystate.txt")))
            {
                Assert.That(state, Is.Not.Null, "An instance should always be instantiated.");
                Assert.That(state.ActiveFiles.Count(), Is.EqualTo(0), "A new state should not have any active files.");

                activeFile = new ActiveFile(New<IDataStore>(_encryptedAxxPath), New<IDataStore>(_decryptedTxtPath), new LogOnIdentity("passphrase"), ActiveFileStatus.AssumedOpenAndDecrypted, new V1Aes128CryptoFactory().CryptoId);
                state.Add(activeFile);
                await state.Save();
            }

            FakeDataStore.AddFile(_encryptedAxxPath, FakeDataStore.TestDate1Utc, FakeDataStore.TestDate2Utc, FakeDataStore.TestDate3Utc, Stream.Null);

            using (FileSystemState reloadedState = FileSystemState.Create(Resolve.WorkFolder.FileInfo.FileItemInfo("mystate.txt")))
            {
                Assert.That(reloadedState, Is.Not.Null, "An instance should always be instantiated.");
                Assert.That(reloadedState.ActiveFiles.Count(), Is.EqualTo(1), "The reloaded state should have one active file.");
                Assert.That(reloadedState.ActiveFiles.First().ThumbprintMatch(activeFile.Identity.Passphrase), Is.True, "The reloaded thumbprint should  match the key.");
            }
        }

        [Test]
        public async Task TestLoadExistingWithNonExistingFile()
        {
            ActiveFile activeFile;
            using (FileSystemState state = FileSystemState.Create(Resolve.WorkFolder.FileInfo.FileItemInfo("mystate.txt")))
            {
                Assert.That(state, Is.Not.Null, "An instance should always be instantiated.");
                Assert.That(state.ActiveFiles.Count(), Is.EqualTo(0), "A new state should not have any active files.");

                activeFile = new ActiveFile(New<IDataStore>(_encryptedAxxPath), New<IDataStore>(_decryptedTxtPath), new LogOnIdentity("passphrase"), ActiveFileStatus.AssumedOpenAndDecrypted, new V1Aes128CryptoFactory().CryptoId);
                state.Add(activeFile);
                await state.Save();
            }

            using (FileSystemState reloadedState = FileSystemState.Create(Resolve.WorkFolder.FileInfo.FileItemInfo("mystate.txt")))
            {
                Assert.That(reloadedState, Is.Not.Null, "An instance should always be instantiated.");
                Assert.That(reloadedState.ActiveFiles.Count(), Is.EqualTo(0), "The reloaded state should not have an active file, since it is non-existing.");
            }
        }

        [Test]
        public async Task TestStatusMaskAtLoad()
        {
            using (FileSystemState state = FileSystemState.Create(Resolve.WorkFolder.FileInfo.FileItemInfo("mystate.txt")))
            {
                FakeDataStore.AddFile(_encryptedAxxPath, FakeDataStore.TestDate1Utc, FakeDataStore.TestDate2Utc, FakeDataStore.TestDate3Utc, new MemoryStream(Resources.helloworld_key_a_txt));
                ActiveFile activeFile = new ActiveFile(New<IDataStore>(_encryptedAxxPath), New<IDataStore>(_decryptedTxtPath), new LogOnIdentity("passphrase"), ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.Error | ActiveFileStatus.IgnoreChange | ActiveFileStatus.NotShareable, new V1Aes128CryptoFactory().CryptoId);
                state.Add(activeFile);
                await state.Save();

                FileSystemState reloadedState = FileSystemState.Create(Resolve.WorkFolder.FileInfo.FileItemInfo("mystate.txt"));
                Assert.That(reloadedState, Is.Not.Null, "An instance should always be instantiated.");
                Assert.That(reloadedState.ActiveFiles.Count(), Is.EqualTo(1), "The reloaded state should have one active file.");
                Assert.That(reloadedState.ActiveFiles.First().Status, Is.EqualTo(ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.NoProcessKnown), "When reloading saved state, some statuses should be masked away and NoProcessKnown added.");
            }
        }

        [Test]
        public void TestFindEncryptedPath()
        {
            using (FileSystemState state = FileSystemState.Create(Resolve.WorkFolder.FileInfo.FileItemInfo("mystate.txt")))
            {
                ActiveFile activeFile = new ActiveFile(New<IDataStore>(_encryptedAxxPath), New<IDataStore>(_decryptedTxtPath), new LogOnIdentity("passphrase"), ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.Error | ActiveFileStatus.IgnoreChange | ActiveFileStatus.NotShareable, new V1Aes128CryptoFactory().CryptoId);
                state.Add(activeFile);

                ActiveFile byEncryptedPath = state.FindActiveFileFromEncryptedPath(_encryptedAxxPath);
                Assert.That(byEncryptedPath.EncryptedFileInfo.FullName, Is.EqualTo(_encryptedAxxPath), "The search should return the same path.");

                ActiveFile notFoundEncrypted = state.FindActiveFileFromEncryptedPath(Path.Combine(_rootPath, "notfoundfile.txt"));
                Assert.That(notFoundEncrypted, Is.Null, "A search that does not succeed should return null.");
            }
        }

        [Test]
        public async Task TestForEach()
        {
            using (FileSystemState state = FileSystemState.Create(Resolve.WorkFolder.FileInfo.FileItemInfo("mystate.txt")))
            {
                ActiveFile activeFile;
                activeFile = new ActiveFile(New<IDataStore>(_encrypted1AxxPath), New<IDataStore>(_decrypted1TxtPath), new LogOnIdentity("passphrase1"), ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.Error | ActiveFileStatus.IgnoreChange | ActiveFileStatus.NotShareable, new V1Aes128CryptoFactory().CryptoId);
                state.Add(activeFile);
                activeFile = new ActiveFile(New<IDataStore>(_encrypted2AxxPath), New<IDataStore>(_decrypted2TxtPath), new LogOnIdentity("passphrase2"), ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.Error | ActiveFileStatus.IgnoreChange | ActiveFileStatus.NotShareable, new V1Aes128CryptoFactory().CryptoId);
                state.Add(activeFile);
                activeFile = new ActiveFile(New<IDataStore>(_encrypted3AxxPath), New<IDataStore>(_decrypted3TxtPath), new LogOnIdentity("passphrase"), ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.Error | ActiveFileStatus.IgnoreChange | ActiveFileStatus.NotShareable, new V1Aes128CryptoFactory().CryptoId);
                state.Add(activeFile);

                Assert.That(state.ActiveFiles.Count(), Is.EqualTo(3), "There should be three.");
                int i = 0;
                await state.ForEach((ActiveFile activeFileArgument) =>
                {
                    ++i;
                    return Task.FromResult(activeFileArgument);
                });
                Assert.That(i, Is.EqualTo(3), "The iteration should have visited three active files.");

                i = 0;
                await state.ForEach((ActiveFile activeFileArgument) =>
                {
                    ++i;
                    return Task.FromResult(new ActiveFile(activeFileArgument, activeFile.Status | ActiveFileStatus.Error));
                });
                Assert.That(i, Is.EqualTo(3), "The iteration should have visited three active files.");
            }
        }

        [Test]
        public void TestDecryptedActiveFiles()
        {
            using (FileSystemState state = FileSystemState.Create(Resolve.WorkFolder.FileInfo.FileItemInfo("mystate.txt")))
            {
                ActiveFile decryptedFile1 = new ActiveFile(New<IDataStore>(_encryptedAxxPath), New<IDataStore>(_decryptedTxtPath), new LogOnIdentity("passphrase1"), ActiveFileStatus.AssumedOpenAndDecrypted, new V1Aes128CryptoFactory().CryptoId);
                state.Add(decryptedFile1);

                ActiveFile decryptedFile2 = new ActiveFile(New<IDataStore>(_encrypted2AxxPath), New<IDataStore>(_decrypted2TxtPath), new LogOnIdentity("passphrase2"), ActiveFileStatus.DecryptedIsPendingDelete, new V1Aes128CryptoFactory().CryptoId);
                state.Add(decryptedFile2);

                ActiveFile notDecryptedFile = new ActiveFile(New<IDataStore>(_encrypted3AxxPath), New<IDataStore>(_decrypted3TxtPath), new LogOnIdentity("passphrase3"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().CryptoId);
                state.Add(notDecryptedFile);

                ActiveFile errorFile = new ActiveFile(New<IDataStore>(_encrypted4AxxPath), New<IDataStore>(_decrypted4TxtPath), new LogOnIdentity("passphrase"), ActiveFileStatus.Error, new V1Aes128CryptoFactory().CryptoId);
                state.Add(errorFile);

                IList<ActiveFile> decryptedFiles = state.DecryptedActiveFiles;
                Assert.That(decryptedFiles.Count, Is.EqualTo(2), "There should be two decrypted files.");
                Assert.That(decryptedFiles.Contains(decryptedFile1), "A file marked as AssumedOpenAndDecrypted should be found.");
                Assert.That(decryptedFiles.Contains(decryptedFile2), "A file marked as DecryptedIsPendingDelete should be found.");
                Assert.That(decryptedFiles.Contains(notDecryptedFile), Is.Not.True, "A file marked as NotDecrypted should not be found.");
            }
        }

        [Test]
        public void TestDoubleDispose()
        {
            FileSystemState state = new FileSystemState();
            state.Dispose();

            Assert.DoesNotThrow(() => { state.Dispose(); });
        }

        [Test]
        public void TestArgumentNull()
        {
            using (FileSystemState state = new FileSystemState())
            {
                ActiveFile nullActiveFile = null;
                string nullPath = null;
                Func<ActiveFile, Task<ActiveFile>> nullAction = null;
                IDataStore nullFileInfo = null;

                Assert.Throws<ArgumentNullException>(() => { state.RemoveActiveFile(nullActiveFile); });
                Assert.Throws<ArgumentNullException>(() => { state.Add(nullActiveFile); });
                Assert.Throws<ArgumentNullException>(() => { state.FindActiveFileFromEncryptedPath(nullPath); });
                Assert.ThrowsAsync<ArgumentNullException>(async () => { await state.ForEach(nullAction); });
                Assert.Throws<ArgumentNullException>(() => { FileSystemState.Create(nullFileInfo); });
            }
        }

        [Test]
        public void TestInvalidJson()
        {
            string badJson = @"{abc:1{}";

            IDataStore stateInfo = Resolve.WorkFolder.FileInfo.FileItemInfo("mystate.txt");
            using (Stream stream = stateInfo.OpenWrite())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(badJson);
                stream.Write(bytes, 0, bytes.Length);
            }

            using (FileSystemState state = FileSystemState.Create(Resolve.WorkFolder.FileInfo.FileItemInfo("mystate.txt")))
            {
                Assert.That(state.ActiveFileCount, Is.EqualTo(0), "After loading damaged state, the count should be zero.");

                ActiveFile decryptedFile1 = new ActiveFile(New<IDataStore>(_encryptedAxxPath), New<IDataStore>(_decryptedTxtPath), new LogOnIdentity("passphrase"), ActiveFileStatus.AssumedOpenAndDecrypted, new V1Aes128CryptoFactory().CryptoId);
                state.Add(decryptedFile1);

                Assert.That(state.ActiveFileCount, Is.EqualTo(1), "After adding a file, the count should be one.");
            }
        }

        [Test]
        public async Task TestWatchedFolders()
        {
            using (FileSystemState state = FileSystemState.Create(Resolve.WorkFolder.FileInfo.FileItemInfo("mystate.txt")))
            {
                Assert.That(state.WatchedFolders, Is.Not.Null, "There should be a Watched Folders instance.");
                Assert.That(state.WatchedFolders.Count(), Is.EqualTo(0), "There should be no Watched folders.");

                FakeDataStore.AddFolder(_rootPath);
                await state.AddWatchedFolderAsync(new WatchedFolder(_rootPath, IdentityPublicTag.Empty));
                Assert.That(state.WatchedFolders.Count(), Is.EqualTo(1), "There should be one Watched Folder.");

                await state.AddWatchedFolderAsync(new WatchedFolder(_rootPath, IdentityPublicTag.Empty));
                Assert.That(state.WatchedFolders.Count(), Is.EqualTo(1), "There should still only be one Watched Folder.");

                await state.Save();
            }

            using (FileSystemState state = FileSystemState.Create(Resolve.WorkFolder.FileInfo.FileItemInfo("mystate.txt")))
            {
                Assert.That(state.WatchedFolders.Count(), Is.EqualTo(1), "There should be one Watched Folder.");

                Assert.That(state.WatchedFolders.First().Matches(_rootPath), "The Watched Folder should be equal to this.");

                await state.RemoveWatchedFolder(Resolve.WorkFolder.FileInfo.FolderItemInfo("mystate.txt"));
                Assert.That(state.WatchedFolders.Count(), Is.EqualTo(1), "There should still be one Watched folders.");

                await state.RemoveWatchedFolder(New<IDataContainer>(_rootPath));
                Assert.That(state.WatchedFolders.Count(), Is.EqualTo(0), "There should be no Watched folders now.");
            }
        }

        [Test]
        public async Task TestAddRemoveKnownWatchedFolder()
        {
            using (FileSystemState state = FileSystemState.Create(Resolve.WorkFolder.FileInfo.FileItemInfo("mystate.txt")))
            {
                KnownFolder knownFolder = New<IKnownFoldersDiscovery>().Discover().First();
                FakeDataStore.AddFolder(knownFolder.My.FullName);

                await state.AddWatchedFolderAsync(new WatchedFolder(knownFolder.My.FullName, IdentityPublicTag.Empty));

                Assert.That(state.WatchedFolders.Count(), Is.EqualTo(1));
                Assert.That(state.AllWatchedFolders.Count(), Is.EqualTo(1));

                await state.RemoveWatchedFolder(knownFolder.My);

                Assert.That(state.WatchedFolders.Count(), Is.EqualTo(0));
                Assert.That(state.AllWatchedFolders.Count(), Is.EqualTo(1));

                await state.AddWatchedFolderAsync(new WatchedFolder(knownFolder.My.FullName, IdentityPublicTag.Empty));

                Assert.That(state.WatchedFolders.Count(), Is.EqualTo(1));
                Assert.That(state.AllWatchedFolders.Count(), Is.EqualTo(1));

                await state.RemoveWatchedFolder(knownFolder.My);

                Assert.That(state.WatchedFolders.Count(), Is.EqualTo(0));
                Assert.That(state.AllWatchedFolders.Count(), Is.EqualTo(1));
            }
        }

        [Test]
        public async Task TestWatchedFolderChanged()
        {
            using (FileSystemState state = FileSystemState.Create(Resolve.WorkFolder.FileInfo.FileItemInfo("mystate.txt")))
            {
                FakeDataStore.AddFolder(_rootPath);
                await state.AddWatchedFolderAsync(new WatchedFolder(_rootPath, IdentityPublicTag.Empty));

                Assert.That(state.ActiveFileCount, Is.EqualTo(0));

                FakeDataStore.AddFile(_encryptedAxxPath, new MemoryStream(Resources.helloworld_key_a_txt));
                Assert.That(state.ActiveFileCount, Is.EqualTo(0));

                state.Add(new ActiveFile(New<IDataStore>(_encryptedAxxPath), New<IDataStore>(_decryptedTxtPath), new LogOnIdentity("passphrase"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().CryptoId));
                Assert.That(state.ActiveFileCount, Is.EqualTo(1));

                New<IDataStore>(_encryptedAxxPath).Delete();
                Assert.That(state.ActiveFileCount, Is.EqualTo(0), "When deleted, the active file count should be zero.");
            }
        }

        [Test]
        public async Task TestWatchedFolderRemoved()
        {
            using (FileSystemState state = FileSystemState.Create(Resolve.WorkFolder.FileInfo.FileItemInfo("mystate.txt")))
            {
                FakeDataStore.AddFolder(_rootPath);
                await state.AddWatchedFolderAsync(new WatchedFolder(_rootPath, IdentityPublicTag.Empty));

                Assert.That(state.WatchedFolders.Count(), Is.EqualTo(1));

                FakeDataStore.RemoveFileOrFolder(_rootPath);

                Assert.That(state.WatchedFolders.Count(), Is.EqualTo(0));
            }
        }

        [Test]
        public async Task TestChangedEvent()
        {
            bool wasHere = false;

            SessionNotify notificationMonitor = new SessionNotify();

            notificationMonitor.AddCommand((SessionNotification notification) => { wasHere = notification.NotificationType == SessionNotificationType.ActiveFileChange; return Constant.CompletedTask; });
            await notificationMonitor.NotifyAsync(new SessionNotification(SessionNotificationType.ActiveFileChange));

            Assert.That(wasHere, Is.True, "The RaiseChanged() method should raise the event immediately.");
        }
    }
}