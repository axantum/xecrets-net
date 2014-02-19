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
        private static readonly string _fileSystemStateFilePath = Path.Combine(Path.GetTempPath(), "DummyFileSystemState.xml");

        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();

            Factory.Instance.Singleton<FileSystemState>(() => FileSystemState.Create(Factory.New<IRuntimeFileInfo>(_fileSystemStateFilePath)));
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
            mock.EncryptFilesUniqueWithBackupAndWipeMock = (IEnumerable<IRuntimeFileInfo> folderInfos, IPassphrase encryptionKey, IProgressContext progress) => { called = folderInfos.First().FullName == @"C:\My Documents\"; };

            SessionNotificationHandler handler = new SessionNotificationHandler(Instance.FileSystemState, Instance.KnownKeys, Factory.New<ActiveFileAction>(), mock);

            handler.HandleNotification(new SessionNotification(SessionNotificationType.WatchedFolderAdded, new GenericPassphrase(new SymmetricKey(128)), @"C:\My Documents\"));

            Assert.That(called, Is.True);
        }

        [Test]
        public static void TestHandleSessionEventWatchedFolderRemoved()
        {
            FakeRuntimeFileInfo.AddFolder(@"C:\My Documents\");
            MockAxCryptFile mock = new MockAxCryptFile();
            bool called = false;
            mock.DecryptFilesUniqueWithWipeOfOriginalMock = (IRuntimeFileInfo fileInfo, IPassphrase decryptionKey, IProgressContext progress) => { called = fileInfo.FullName == @"C:\My Documents\"; };

            Factory.Instance.Register<AxCryptFile>(() => mock);

            SessionNotificationHandler handler = new SessionNotificationHandler(Instance.FileSystemState, Instance.KnownKeys, Factory.New<ActiveFileAction>(), mock);

            handler.HandleNotification(new SessionNotification(SessionNotificationType.WatchedFolderRemoved, new GenericPassphrase(new SymmetricKey(128)), @"C:\My Documents\"));

            Assert.That(called, Is.True);
        }

        [Test]
        public static void TestHandleSessionEventLogOn()
        {
            MockAxCryptFile mock = new MockAxCryptFile();
            bool called = false;
            int folderCount = -1;
            mock.EncryptFilesUniqueWithBackupAndWipeMock = (IEnumerable<IRuntimeFileInfo> folderInfos, IPassphrase encryptionKey, IProgressContext progress) =>
            {
                folderCount = folderInfos.Count();
                called = true;
            };

            SessionNotificationHandler handler = new SessionNotificationHandler(Instance.FileSystemState, Instance.KnownKeys, Factory.New<ActiveFileAction>(), mock);
            FakeRuntimeFileInfo.AddFolder(@"C:\WatchedFolder");
            IPassphrase key = new GenericPassphrase(new SymmetricKey(128));
            Instance.FileSystemState.AddWatchedFolder(new WatchedFolder(@"C:\WatchedFolder", key.DerivedKey.Thumbprint));

            handler.HandleNotification(new SessionNotification(SessionNotificationType.LogOn, key));

            Assert.That(called, Is.True);
            Assert.That(folderCount, Is.EqualTo(1), "There should be one folder passed for encryption as a result of the event.");
        }

        [Test]
        public static void TestHandleSessionEventLogOff()
        {
            MockAxCryptFile mock = new MockAxCryptFile();
            bool called = false;
            mock.EncryptFilesUniqueWithBackupAndWipeMock = (IEnumerable<IRuntimeFileInfo> folderInfos, IPassphrase encryptionKey, IProgressContext progress) => { called = true; };

            SessionNotificationHandler handler = new SessionNotificationHandler(Instance.FileSystemState, Instance.KnownKeys, Factory.New<ActiveFileAction>(), mock);

            handler.HandleNotification(new SessionNotification(SessionNotificationType.LogOff, new GenericPassphrase(new SymmetricKey(128))));

            Assert.That(called, Is.True);
        }

        [Test]
        public static void TestHandleSessionEventActiveFileChange()
        {
            MockFileSystemStateActions mock = new MockFileSystemStateActions();
            bool called = false;
            mock.CheckActiveFilesMock = (ChangedEventMode mode, IProgressContext progress) => { called = true; };

            SessionNotificationHandler handler = new SessionNotificationHandler(Instance.FileSystemState, Instance.KnownKeys, mock, Factory.New<AxCryptFile>());

            handler.HandleNotification(new SessionNotification(SessionNotificationType.ActiveFileChange, new GenericPassphrase(new SymmetricKey(128))));

            Assert.That(called, Is.True);
        }

        [Test]
        public static void TestHandleSessionEventSessionStart()
        {
            MockFileSystemStateActions mock = new MockFileSystemStateActions();
            bool called = false;
            mock.CheckActiveFilesMock = (ChangedEventMode mode, IProgressContext progress) => { called = true; };

            SessionNotificationHandler handler = new SessionNotificationHandler(Instance.FileSystemState, Instance.KnownKeys, mock, Factory.New<AxCryptFile>());

            handler.HandleNotification(new SessionNotification(SessionNotificationType.SessionStart, new GenericPassphrase(new SymmetricKey(128))));

            Assert.That(called, Is.True);
        }

        [Test]
        public static void TestHandleSessionEventPurgeActiveFiles()
        {
            MockFileSystemStateActions mock = new MockFileSystemStateActions();
            bool called = false;
            mock.PurgeActiveFilesMock = (IProgressContext progress) => { called = true; };

            SessionNotificationHandler handler = new SessionNotificationHandler(Instance.FileSystemState, Instance.KnownKeys, mock, Factory.New<AxCryptFile>());

            handler.HandleNotification(new SessionNotification(SessionNotificationType.EncryptPendingFiles, new GenericPassphrase(new SymmetricKey(128))));

            Assert.That(called, Is.True);
        }

        [Test]
        public static void TestHandleSessionEventThatCauseNoSpecificAction()
        {
            MockFileSystemStateActions mock = new MockFileSystemStateActions();

            SessionNotificationHandler handler = new SessionNotificationHandler(Instance.FileSystemState, Instance.KnownKeys, mock, Factory.New<AxCryptFile>());

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

            SessionNotificationHandler handler = new SessionNotificationHandler(Instance.FileSystemState, Instance.KnownKeys, mock, Factory.New<AxCryptFile>());

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
            mock.EncryptFilesUniqueWithBackupAndWipeMock = (IEnumerable<IRuntimeFileInfo> folderInfos, IPassphrase decryptionKey, IProgressContext progress) => { if (folderInfos.First().FullName == @"C:\My Documents\") ++callTimes; };

            SessionNotificationHandler handler = new SessionNotificationHandler(Instance.FileSystemState, Instance.KnownKeys, Factory.New<ActiveFileAction>(), mock);

            List<SessionNotification> sessionEvents = new List<SessionNotification>();
            sessionEvents.Add(new SessionNotification(SessionNotificationType.WatchedFolderAdded, new GenericPassphrase(new SymmetricKey(128)), @"C:\My Documents\"));
            sessionEvents.Add(new SessionNotification(SessionNotificationType.WatchedFolderAdded, new GenericPassphrase(new SymmetricKey(128)), @"C:\My Documents\"));

            foreach (SessionNotification sessionEvent in sessionEvents)
            {
                handler.HandleNotification(sessionEvent);
            }
            Assert.That(callTimes, Is.EqualTo(2));
        }
    }
}