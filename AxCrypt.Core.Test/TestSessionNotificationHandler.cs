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
using AxCrypt.Core.IO;
using AxCrypt.Core.Session;
using AxCrypt.Core.UI;
using AxCrypt.Fake;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace AxCrypt.Core.Test
{
    public class TestSessionNotificationHandler
    {
        private readonly string _fileSystemStateFilePath = Path.Combine(Path.GetTempPath(), "DummyFileSystemState.txt");

        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup();

            TypeMap.Register.Singleton<FileSystemState>(() => FileSystemState.Create(New<IDataStore>(_fileSystemStateFilePath)));
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.WindowsDesktop)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public async Task TestHandleSessionEventWatchedFolderAdded(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetupCrypto(cryptoImplementation);

            MockAxCryptFile mock = new MockAxCryptFile();
            bool called = false;
            mock.EncryptFilesUniqueWithBackupAndWipeMockAsync = async (IEnumerable<IDataContainer> folderInfos, EncryptionParameters encryptionParameters, IProgressContext progress) => { await Task.Delay(0); called = folderInfos.First().FullName == @"C:\My Documents\".NormalizeFilePath(); };

            Mock<IStatusChecker> mockStatusChecker = new Mock<IStatusChecker>();

            SessionNotificationHandler handler = new SessionNotificationHandler(Resolve.FileSystemState, Resolve.KnownIdentities, New<ActiveFileAction>(), mock, mockStatusChecker.Object);
            FakeDataStore.AddFolder(@"C:\My Documents");
            LogOnIdentity key = new LogOnIdentity("passphrase");
            await Resolve.FileSystemState.AddWatchedFolderAsync(new WatchedFolder(@"C:\My Documents", key.Tag));

            await handler.HandleNotificationAsync(new SessionNotification(SessionNotificationType.WatchedFolderAdded, new LogOnIdentity("passphrase"), @"C:\My Documents\"));

            Assert.That(called, Is.True);
        }

        [Test]
        public async Task TestHandleSessionEventWatchedFolderRemoved()
        {
            FakeDataStore.AddFolder(@"C:\My Documents\");
            MockAxCryptFile mock = new MockAxCryptFile();
            Mock<IStatusChecker> mockStatusChecker = new Mock<IStatusChecker>();
            bool called = false;
            mock.DecryptFilesUniqueWithWipeOfOriginalMockAsync = (IDataContainer fileInfo, LogOnIdentity decryptionKey, IStatusChecker statusChecker, IProgressContext progress) => { called = fileInfo.FullName == @"C:\My Documents\".NormalizeFilePath(); };

            TypeMap.Register.New<AxCryptFile>(() => mock);

            SessionNotificationHandler handler = new SessionNotificationHandler(Resolve.FileSystemState, Resolve.KnownIdentities, New<ActiveFileAction>(), mock, mockStatusChecker.Object);

            await handler.HandleNotificationAsync(new SessionNotification(SessionNotificationType.WatchedFolderRemoved, new LogOnIdentity("passphrase"), @"C:\My Documents\"));

            Assert.That(called, Is.True);
        }

        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.WindowsDesktop)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public async Task TestHandleSessionEventLogOn(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetupCrypto(cryptoImplementation);

            MockAxCryptFile mock = new MockAxCryptFile();
            bool called = false;
            int folderCount = -1;
            mock.EncryptFilesUniqueWithBackupAndWipeMockAsync = async (IEnumerable<IDataContainer> folderInfos, EncryptionParameters encryptionParameters, IProgressContext progress) =>
            {
                folderCount = folderInfos.Count();
                called = true;
                await Task.Delay(0);
            };

            Mock<IStatusChecker> mockStatusChecker = new Mock<IStatusChecker>();

            SessionNotificationHandler handler = new SessionNotificationHandler(Resolve.FileSystemState, Resolve.KnownIdentities, New<ActiveFileAction>(), mock, mockStatusChecker.Object);
            FakeDataStore.AddFolder(@"C:\WatchedFolder");
            LogOnIdentity key = new LogOnIdentity("passphrase");
            await Resolve.FileSystemState.AddWatchedFolderAsync(new WatchedFolder(@"C:\WatchedFolder", key.Tag));

            await handler.HandleNotificationAsync(new SessionNotification(SessionNotificationType.SignIn, key));

            Assert.That(called, Is.True);
            Assert.That(folderCount, Is.EqualTo(1), "There should be one folder passed for encryption as a result of the event.");
        }

        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.WindowsDesktop)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public async Task TestHandleSessionEventLogOffWithWatchedFolders(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetupCrypto(cryptoImplementation);

            MockAxCryptFile mock = new MockAxCryptFile();
            bool called = false;
            mock.EncryptFilesUniqueWithBackupAndWipeMockAsync = async (IEnumerable<IDataContainer> folderInfos, EncryptionParameters encryptionParameters, IProgressContext progress) => { await Task.Delay(0); called = true; };

            Mock<IStatusChecker> mockStatusChecker = new Mock<IStatusChecker>();

            await Resolve.KnownIdentities.SetDefaultEncryptionIdentity(new LogOnIdentity("passphrase"));
            SessionNotificationHandler handler = new SessionNotificationHandler(Resolve.FileSystemState, Resolve.KnownIdentities, New<ActiveFileAction>(), mock, mockStatusChecker.Object);
            FakeDataStore.AddFolder(@"C:\WatchedFolder");
            await Resolve.FileSystemState.AddWatchedFolderAsync(new WatchedFolder(@"C:\WatchedFolder", Resolve.KnownIdentities.DefaultEncryptionIdentity.Tag));

            called = false;
            await handler.HandleNotificationAsync(new SessionNotification(SessionNotificationType.EncryptPendingFiles));
            await handler.HandleNotificationAsync(new SessionNotification(SessionNotificationType.SignOut, Resolve.KnownIdentities.DefaultEncryptionIdentity));
            Assert.That(called, Is.True, nameof(AxCryptFile.EncryptFoldersUniqueWithBackupAndWipeAsync) + " should be called when a signing out.");
        }

        [Test]
        public async Task TestHandleSessionEventLogOffWithNoWatchedFolders()
        {
            MockAxCryptFile mock = new MockAxCryptFile();
            bool called = false;
            mock.EncryptFilesUniqueWithBackupAndWipeMockAsync = async (IEnumerable<IDataContainer> folderInfos, EncryptionParameters encryptionParameters, IProgressContext progress) => { await Task.Delay(0); called = true; };

            Mock<IStatusChecker> mockStatusChecker = new Mock<IStatusChecker>();

            SessionNotificationHandler handler = new SessionNotificationHandler(Resolve.FileSystemState, Resolve.KnownIdentities, New<ActiveFileAction>(), mock, mockStatusChecker.Object);

            await handler.HandleNotificationAsync(new SessionNotification(SessionNotificationType.SignOut, new LogOnIdentity("passphrase")));

            Assert.That(called, Is.False);
        }

        [Test]
        public async Task TestHandleSessionEventActiveFileChange()
        {
            MockFileSystemStateActions mock = new MockFileSystemStateActions();
            bool called = false;
            mock.CheckActiveFilesMock = (IProgressContext progress) => { return Task.FromResult(called = true); };

            Mock<IStatusChecker> mockStatusChecker = new Mock<IStatusChecker>();

            SessionNotificationHandler handler = new SessionNotificationHandler(Resolve.FileSystemState, Resolve.KnownIdentities, mock, New<AxCryptFile>(), mockStatusChecker.Object);

            await handler.HandleNotificationAsync(new SessionNotification(SessionNotificationType.ActiveFileChange, new LogOnIdentity("passphrase")));

            Assert.That(called, Is.True);
        }

        [Test]
        public async Task TestHandleSessionEventSessionStart()
        {
            MockFileSystemStateActions mock = new MockFileSystemStateActions();
            bool called = false;
            mock.CheckActiveFilesMock = (IProgressContext progress) => { return Task.FromResult(called = true); };

            Mock<IStatusChecker> mockStatusChecker = new Mock<IStatusChecker>();

            SessionNotificationHandler handler = new SessionNotificationHandler(Resolve.FileSystemState, Resolve.KnownIdentities, mock, New<AxCryptFile>(), mockStatusChecker.Object);

            await handler.HandleNotificationAsync(new SessionNotification(SessionNotificationType.SessionStart, new LogOnIdentity("passphrase")));

            Assert.That(called, Is.True);
        }

        [Test]
        public async Task TestHandleSessionEventPurgeActiveFiles()
        {
            MockFileSystemStateActions mock = new MockFileSystemStateActions();
            bool called = false;
            mock.PurgeActiveFilesMock = (IProgressContext progress) => { return Task.FromResult(called = true); };

            Mock<IStatusChecker> mockStatusChecker = new Mock<IStatusChecker>();

            SessionNotificationHandler handler = new SessionNotificationHandler(Resolve.FileSystemState, Resolve.KnownIdentities, mock, New<AxCryptFile>(), mockStatusChecker.Object);

            await handler.HandleNotificationAsync(new SessionNotification(SessionNotificationType.EncryptPendingFiles));

            Assert.That(called, Is.True);
        }

        [Test]
        public void TestHandleSessionEventThatCauseNoSpecificAction()
        {
            MockFileSystemStateActions mock = new MockFileSystemStateActions();

            Mock<IStatusChecker> mockStatusChecker = new Mock<IStatusChecker>();

            SessionNotificationHandler handler = new SessionNotificationHandler(Resolve.FileSystemState, Resolve.KnownIdentities, mock, New<AxCryptFile>(), mockStatusChecker.Object);

            Assert.DoesNotThrowAsync(async () =>
            {
                await handler.HandleNotificationAsync(new SessionNotification(SessionNotificationType.ProcessExit));
                await handler.HandleNotificationAsync(new SessionNotification(SessionNotificationType.SessionChange));
                await handler.HandleNotificationAsync(new SessionNotification(SessionNotificationType.KnownKeyChange));
                await handler.HandleNotificationAsync(new SessionNotification(SessionNotificationType.WorkFolderChange));
            });
        }

        [Test]
        public void TestHandleSessionEventThatIsNotHandled()
        {
            MockFileSystemStateActions mock = new MockFileSystemStateActions();

            Mock<IStatusChecker> mockStatusChecker = new Mock<IStatusChecker>();

            SessionNotificationHandler handler = new SessionNotificationHandler(Resolve.FileSystemState, Resolve.KnownIdentities, mock, New<AxCryptFile>(), mockStatusChecker.Object);

            Assert.DoesNotThrowAsync(async () =>
            {
                await handler.HandleNotificationAsync(new SessionNotification((SessionNotificationType)(-1)));
            });
            mockStatusChecker.Verify(msc => msc.CheckStatusAndShowMessage(It.Is<ErrorStatus>(es => es == ErrorStatus.Exception), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.WindowsDesktop)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public async Task TestHandleSessionEvents(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetupCrypto(cryptoImplementation);

            MockAxCryptFile mock = new MockAxCryptFile();
            int callTimes = 0;
            mock.EncryptFilesUniqueWithBackupAndWipeMockAsync = async (IEnumerable<IDataContainer> folderInfos, EncryptionParameters encryptionParameters, IProgressContext progress) => { await Task.Delay(0); if (folderInfos.First().FullName == @"C:\My Documents\".NormalizeFilePath()) ++callTimes; };

            Mock<IStatusChecker> mockStatusChecker = new Mock<IStatusChecker>();

            SessionNotificationHandler handler = new SessionNotificationHandler(Resolve.FileSystemState, Resolve.KnownIdentities, New<ActiveFileAction>(), mock, mockStatusChecker.Object);
            FakeDataStore.AddFolder(@"C:\My Documents");
            LogOnIdentity key = new LogOnIdentity("passphrase");
            await Resolve.FileSystemState.AddWatchedFolderAsync(new WatchedFolder(@"C:\My Documents", key.Tag));

            List<SessionNotification> sessionEvents = new List<SessionNotification>();
            sessionEvents.Add(new SessionNotification(SessionNotificationType.WatchedFolderAdded, new LogOnIdentity("passphrase1"), @"C:\My Documents\"));
            sessionEvents.Add(new SessionNotification(SessionNotificationType.WatchedFolderAdded, new LogOnIdentity("passphrase"), @"C:\My Documents\"));

            foreach (SessionNotification sessionEvent in sessionEvents)
            {
                await handler.HandleNotificationAsync(sessionEvent);
            }
            Assert.That(callTimes, Is.EqualTo(2));
        }

        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.WindowsDesktop)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public async Task TestNotificationEncryptPendingFilesInLoggedOnFolders(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetupCrypto(cryptoImplementation);

            FakeDataStore.AddFolder(@"C:\My Documents\");
            Mock<AxCryptFile> mock = new Mock<AxCryptFile>();
            mock.Setup(acf => acf.EncryptFoldersUniqueWithBackupAndWipeAsync(It.IsAny<IEnumerable<IDataContainer>>(), It.IsAny<EncryptionParameters>(), It.IsAny<IProgressContext>()));

            Mock<IStatusChecker> mockStatusChecker = new Mock<IStatusChecker>();

            SessionNotificationHandler handler = new SessionNotificationHandler(Resolve.FileSystemState, Resolve.KnownIdentities, New<ActiveFileAction>(), mock.Object, mockStatusChecker.Object);
            LogOnIdentity defaultKey = new LogOnIdentity("default");
            await Resolve.KnownIdentities.SetDefaultEncryptionIdentity(defaultKey);
            await Resolve.FileSystemState.AddWatchedFolderAsync(new WatchedFolder(@"C:\My Documents\", defaultKey.Tag));

            List<SessionNotification> sessionEvents = new List<SessionNotification>();
            sessionEvents.Add(new SessionNotification(SessionNotificationType.EncryptPendingFiles));

            foreach (SessionNotification sessionEvent in sessionEvents)
            {
                await handler.HandleNotificationAsync(sessionEvent);
            }
            mock.Verify(acf => acf.EncryptFoldersUniqueWithBackupAndWipeAsync(It.Is<IEnumerable<IDataContainer>>(infos => infos.Any((i) => i.FullName == @"C:\My Documents\".NormalizeFolderPath())), It.IsAny<EncryptionParameters>(), It.IsAny<IProgressContext>()), Times.Exactly(1));
        }
    }
}