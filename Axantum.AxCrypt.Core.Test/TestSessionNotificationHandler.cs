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
using Axantum.AxCrypt.Core.UI;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestSessionNotificationHandler
    {
        private static readonly string _fileSystemStateFilePath = Path.Combine(Path.GetTempPath(), "DummyFileSystemState.txt");

        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();

            TypeMap.Register.Singleton<FileSystemState>(() => FileSystemState.Create(TypeMap.Resolve.New<IDataStore>(_fileSystemStateFilePath)));
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestHandleSessionEventWatchedFolderAdded()
        {
            MockAxCryptFile mock = new MockAxCryptFile();
            bool called = false;
            mock.EncryptFilesUniqueWithBackupAndWipeMock = (IEnumerable<IDataContainer> folderInfos, EncryptionParameters encryptionParameters, IProgressContext progress) => { called = folderInfos.First().FullName == @"C:\My Documents\".NormalizeFilePath(); };

            Mock<IStatusChecker> mockStatusChecker = new Mock<IStatusChecker>();

            SessionNotificationHandler handler = new SessionNotificationHandler(Resolve.FileSystemState, Resolve.KnownIdentities, TypeMap.Resolve.New<ActiveFileAction>(), mock, mockStatusChecker.Object);

            handler.HandleNotification(new SessionNotification(SessionNotificationType.WatchedFolderAdded, new LogOnIdentity("passphrase"), @"C:\My Documents\"));

            Assert.That(called, Is.True);
        }

        [Test]
        public static void TestHandleSessionEventWatchedFolderRemoved()
        {
            FakeDataStore.AddFolder(@"C:\My Documents\");
            MockAxCryptFile mock = new MockAxCryptFile();
            Mock<IStatusChecker> mockStatusChecker = new Mock<IStatusChecker>();
            bool called = false;
            mock.DecryptFilesUniqueWithWipeOfOriginalMock = (IDataContainer fileInfo, LogOnIdentity decryptionKey, IStatusChecker statusChecker, IProgressContext progress) => { called = fileInfo.FullName == @"C:\My Documents\".NormalizeFilePath(); };

            TypeMap.Register.New<AxCryptFile>(() => mock);

            SessionNotificationHandler handler = new SessionNotificationHandler(Resolve.FileSystemState, Resolve.KnownIdentities, TypeMap.Resolve.New<ActiveFileAction>(), mock, mockStatusChecker.Object);

            handler.HandleNotification(new SessionNotification(SessionNotificationType.WatchedFolderRemoved, new LogOnIdentity("passphrase"), @"C:\My Documents\"));

            Assert.That(called, Is.True);
        }

        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.WindowsDesktop)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public static void TestHandleSessionEventLogOn(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetupCrypto(cryptoImplementation);

            MockAxCryptFile mock = new MockAxCryptFile();
            bool called = false;
            int folderCount = -1;
            mock.EncryptFilesUniqueWithBackupAndWipeMock = (IEnumerable<IDataContainer> folderInfos, EncryptionParameters encryptionParameters, IProgressContext progress) =>
            {
                folderCount = folderInfos.Count();
                called = true;
            };

            Mock<IStatusChecker> mockStatusChecker = new Mock<IStatusChecker>();

            SessionNotificationHandler handler = new SessionNotificationHandler(Resolve.FileSystemState, Resolve.KnownIdentities, TypeMap.Resolve.New<ActiveFileAction>(), mock, mockStatusChecker.Object);
            FakeDataStore.AddFolder(@"C:\WatchedFolder");
            LogOnIdentity key = new LogOnIdentity("passphrase");
            Resolve.FileSystemState.AddWatchedFolder(new WatchedFolder(@"C:\WatchedFolder", key.Tag));

            handler.HandleNotification(new SessionNotification(SessionNotificationType.LogOn, key));

            Assert.That(called, Is.True);
            Assert.That(folderCount, Is.EqualTo(1), "There should be one folder passed for encryption as a result of the event.");
        }

        [Test]
        public static void TestHandleSessionEventLogOff()
        {
            MockAxCryptFile mock = new MockAxCryptFile();
            bool called = false;
            mock.EncryptFilesUniqueWithBackupAndWipeMock = (IEnumerable<IDataContainer> folderInfos, EncryptionParameters encryptionParameters, IProgressContext progress) => { called = true; };

            Mock<IStatusChecker> mockStatusChecker = new Mock<IStatusChecker>();

            SessionNotificationHandler handler = new SessionNotificationHandler(Resolve.FileSystemState, Resolve.KnownIdentities, TypeMap.Resolve.New<ActiveFileAction>(), mock, mockStatusChecker.Object);

            handler.HandleNotification(new SessionNotification(SessionNotificationType.LogOff, new LogOnIdentity("passphrase")));

            Assert.That(called, Is.True);
        }

        [Test]
        public static void TestHandleSessionEventActiveFileChange()
        {
            MockFileSystemStateActions mock = new MockFileSystemStateActions();
            bool called = false;
            mock.CheckActiveFilesMock = (ChangedEventMode mode, IProgressContext progress) => { called = true; };

            Mock<IStatusChecker> mockStatusChecker = new Mock<IStatusChecker>();

            SessionNotificationHandler handler = new SessionNotificationHandler(Resolve.FileSystemState, Resolve.KnownIdentities, mock, TypeMap.Resolve.New<AxCryptFile>(), mockStatusChecker.Object);

            handler.HandleNotification(new SessionNotification(SessionNotificationType.ActiveFileChange, new LogOnIdentity("passphrase")));

            Assert.That(called, Is.True);
        }

        [Test]
        public static void TestHandleSessionEventSessionStart()
        {
            MockFileSystemStateActions mock = new MockFileSystemStateActions();
            bool called = false;
            mock.CheckActiveFilesMock = (ChangedEventMode mode, IProgressContext progress) => { called = true; };

            Mock<IStatusChecker> mockStatusChecker = new Mock<IStatusChecker>();

            SessionNotificationHandler handler = new SessionNotificationHandler(Resolve.FileSystemState, Resolve.KnownIdentities, mock, TypeMap.Resolve.New<AxCryptFile>(), mockStatusChecker.Object);

            handler.HandleNotification(new SessionNotification(SessionNotificationType.SessionStart, new LogOnIdentity("passphrase")));

            Assert.That(called, Is.True);
        }

        [Test]
        public static void TestHandleSessionEventPurgeActiveFiles()
        {
            MockFileSystemStateActions mock = new MockFileSystemStateActions();
            bool called = false;
            mock.PurgeActiveFilesMock = (IProgressContext progress) => { called = true; };

            Mock<IStatusChecker> mockStatusChecker = new Mock<IStatusChecker>();

            SessionNotificationHandler handler = new SessionNotificationHandler(Resolve.FileSystemState, Resolve.KnownIdentities, mock, TypeMap.Resolve.New<AxCryptFile>(), mockStatusChecker.Object);

            handler.HandleNotification(new SessionNotification(SessionNotificationType.EncryptPendingFiles));

            Assert.That(called, Is.True);
        }

        [Test]
        public static void TestHandleSessionEventThatCauseNoSpecificAction()
        {
            MockFileSystemStateActions mock = new MockFileSystemStateActions();

            Mock<IStatusChecker> mockStatusChecker = new Mock<IStatusChecker>();

            SessionNotificationHandler handler = new SessionNotificationHandler(Resolve.FileSystemState, Resolve.KnownIdentities, mock, TypeMap.Resolve.New<AxCryptFile>(), mockStatusChecker.Object);

            Assert.DoesNotThrow(() =>
            {
                handler.HandleNotification(new SessionNotification(SessionNotificationType.ProcessExit));
                handler.HandleNotification(new SessionNotification(SessionNotificationType.SessionChange));
                handler.HandleNotification(new SessionNotification(SessionNotificationType.KnownKeyChange));
                handler.HandleNotification(new SessionNotification(SessionNotificationType.WorkFolderChange));
            });
        }

        [Test]
        public static void TestHandleSessionEventThatIsNotHandled()
        {
            MockFileSystemStateActions mock = new MockFileSystemStateActions();

            Mock<IStatusChecker> mockStatusChecker = new Mock<IStatusChecker>();

            SessionNotificationHandler handler = new SessionNotificationHandler(Resolve.FileSystemState, Resolve.KnownIdentities, mock, TypeMap.Resolve.New<AxCryptFile>(), mockStatusChecker.Object);

            Assert.Throws<InvalidOperationException>(() =>
            {
                handler.HandleNotification(new SessionNotification((SessionNotificationType)(-1)));
            });
        }

        [Test]
        public static void TestHandleSessionEvents()
        {
            MockAxCryptFile mock = new MockAxCryptFile();
            int callTimes = 0;
            mock.EncryptFilesUniqueWithBackupAndWipeMock = (IEnumerable<IDataContainer> folderInfos, EncryptionParameters encryptionParameters, IProgressContext progress) => { if (folderInfos.First().FullName == @"C:\My Documents\".NormalizeFilePath()) ++callTimes; };

            Mock<IStatusChecker> mockStatusChecker = new Mock<IStatusChecker>();

            SessionNotificationHandler handler = new SessionNotificationHandler(Resolve.FileSystemState, Resolve.KnownIdentities, TypeMap.Resolve.New<ActiveFileAction>(), mock, mockStatusChecker.Object);

            List<SessionNotification> sessionEvents = new List<SessionNotification>();
            sessionEvents.Add(new SessionNotification(SessionNotificationType.WatchedFolderAdded, new LogOnIdentity("passphrase1"), @"C:\My Documents\"));
            sessionEvents.Add(new SessionNotification(SessionNotificationType.WatchedFolderAdded, new LogOnIdentity("passphrase"), @"C:\My Documents\"));

            foreach (SessionNotification sessionEvent in sessionEvents)
            {
                handler.HandleNotification(sessionEvent);
            }
            Assert.That(callTimes, Is.EqualTo(2));
        }

        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.WindowsDesktop)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public static void TestNotificationEncryptPendingFilesInLoggedOnFolders(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetupCrypto(cryptoImplementation);

            FakeDataStore.AddFolder(@"C:\My Documents\");
            Mock<AxCryptFile> mock = new Mock<AxCryptFile>();
            mock.Setup(acf => acf.EncryptFoldersUniqueWithBackupAndWipe(It.IsAny<IEnumerable<IDataContainer>>(), It.IsAny<EncryptionParameters>(), It.IsAny<IProgressContext>()));

            Mock<IStatusChecker> mockStatusChecker = new Mock<IStatusChecker>();

            SessionNotificationHandler handler = new SessionNotificationHandler(Resolve.FileSystemState, Resolve.KnownIdentities, TypeMap.Resolve.New<ActiveFileAction>(), mock.Object, mockStatusChecker.Object);
            LogOnIdentity defaultKey = new LogOnIdentity("default");
            Resolve.KnownIdentities.DefaultEncryptionIdentity = defaultKey;
            Resolve.FileSystemState.AddWatchedFolder(new WatchedFolder(@"C:\My Documents\", defaultKey.Tag));

            List<SessionNotification> sessionEvents = new List<SessionNotification>();
            sessionEvents.Add(new SessionNotification(SessionNotificationType.EncryptPendingFiles));

            foreach (SessionNotification sessionEvent in sessionEvents)
            {
                handler.HandleNotification(sessionEvent);
            }
            mock.Verify(acf => acf.EncryptFoldersUniqueWithBackupAndWipe(It.Is<IEnumerable<IDataContainer>>(infos => infos.Any((i) => i.FullName == @"C:\My Documents\".NormalizeFolderPath())), It.IsAny<EncryptionParameters>(), It.IsAny<IProgressContext>()), Times.Exactly(1));
        }
    }
}