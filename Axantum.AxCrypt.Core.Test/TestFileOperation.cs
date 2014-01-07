#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Core.UI;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestFileOperation
    {
        private static readonly string _rootPath = Path.GetPathRoot(Environment.CurrentDirectory);
        private static readonly string _testTextPath = Path.Combine(_rootPath, "test.txt");
        private static readonly string _davidCopperfieldTxtPath = _rootPath.PathCombine("Users", "AxCrypt", "David Copperfield.txt");
        private static readonly string _uncompressedAxxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Uncompressed.axx");
        private static readonly string _helloWorldAxxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HelloWorld.axx");

        [TestFixtureSetUp]
        public static void SetupFixture()
        {
        }

        [TestFixtureTearDown]
        public static void TeardownFixture()
        {
        }

        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();

            FakeRuntimeFileInfo.AddFile(_testTextPath, FakeRuntimeFileInfo.TestDate1Utc, FakeRuntimeFileInfo.TestDate2Utc, FakeRuntimeFileInfo.TestDate1Utc, FakeRuntimeFileInfo.ExpandableMemoryStream(Encoding.UTF8.GetBytes("This is a short file")));
            FakeRuntimeFileInfo.AddFile(_davidCopperfieldTxtPath, FakeRuntimeFileInfo.TestDate4Utc, FakeRuntimeFileInfo.TestDate5Utc, FakeRuntimeFileInfo.TestDate6Utc, FakeRuntimeFileInfo.ExpandableMemoryStream(Encoding.GetEncoding(1252).GetBytes(Resources.david_copperfield)));
            FakeRuntimeFileInfo.AddFile(_uncompressedAxxPath, FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.uncompressable_zip));
            FakeRuntimeFileInfo.AddFile(_helloWorldAxxPath, FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.helloworld_key_a_txt));
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestInvalidArguments()
        {
            string file = _helloWorldAxxPath;
            string nullFile = null;

            IEnumerable<AesKey> keys = new AesKey[] { new Passphrase("a").DerivedPassphrase };
            IEnumerable<AesKey> nullKeys = null;

            ProgressContext context = new ProgressContext();
            ProgressContext nullContext = null;

            FileOperation fileOperation = new FileOperation(Instance.FileSystemState, new SessionNotify());
            Assert.Throws<ArgumentNullException>(() => { fileOperation.OpenAndLaunchApplication(nullFile, keys, context); }, "The file string is null.");
            Assert.Throws<ArgumentNullException>(() => { fileOperation.OpenAndLaunchApplication(file, nullKeys, context); }, "The keys are null.");
            Assert.Throws<ArgumentNullException>(() => { fileOperation.OpenAndLaunchApplication(file, keys, nullContext); }, "The context is null.");
        }

        [Test]
        public static void TestSimpleOpenAndLaunch()
        {
            IEnumerable<AesKey> keys = new AesKey[] { new Passphrase("a").DerivedPassphrase };

            var mock = new Mock<FakeRuntimeEnvironment>() { CallBase = true };
            string launcherPath = null;
            mock.Setup(x => x.Launch(It.IsAny<string>())).Callback((string path) => launcherPath = path).Returns((string path) => new FakeLauncher(path));
            Factory.Instance.Singleton<IRuntimeEnvironment>(() => mock.Object);

            FileOperation fileOperation = new FileOperation(Instance.FileSystemState, new SessionNotify());
            FileOperationStatus status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed.");
            Assert.DoesNotThrow(() => mock.Verify(x => x.Launch(launcherPath)));
        }

        [Test]
        public static void TestOpenAndLaunchOfAxCryptDocument()
        {
            FakeLauncher launcher = null;
            SetupAssembly.FakeRuntimeEnvironment.Launcher = ((string path) =>
            {
                launcher = new FakeLauncher(path);
                return launcher;
            });

            FileOperationStatus status;
            FileOperation fileOperation = new FileOperation(Instance.FileSystemState, new SessionNotify());

            using (AxCryptDocument document = new AxCryptDocument())
            {
                using (Stream stream = Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath).OpenRead())
                {
                    document.Load(stream, new Passphrase("a").DerivedPassphrase);
                    status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, document, new ProgressContext());
                }
            }

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed.");
            Assert.That(launcher, Is.Not.Null, "There should be a call to launch.");
            Assert.That(Path.GetFileName(launcher.Path), Is.EqualTo("HelloWorld-Key-a.txt"), "The file should be decrypted and the name should be the original from the encrypted headers.");
        }

        [Test]
        public static void TestOpenAndLaunchOfAxCryptDocumentWhenAlreadyDecrypted()
        {
            TestOpenAndLaunchOfAxCryptDocument();

            FakeLauncher launcher = null;
            SetupAssembly.FakeRuntimeEnvironment.Launcher = ((string path) =>
            {
                launcher = new FakeLauncher(path);
                return launcher;
            });

            FileOperationStatus status;
            FileOperation fileOperation = new FileOperation(Instance.FileSystemState, new SessionNotify());
            using (AxCryptDocument document = new AxCryptDocument())
            {
                using (Stream stream = Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath).OpenRead())
                {
                    document.Load(stream, new Passphrase("a").DerivedPassphrase);
                    status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, document, new ProgressContext());
                }
            }

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed.");
            Assert.That(launcher, Is.Not.Null, "There should be a call to launch.");
            Assert.That(Path.GetFileName(launcher.Path), Is.EqualTo("HelloWorld-Key-a.txt"), "The file should be decrypted and the name should be the original from the encrypted headers.");
        }

        [Test]
        public static void TestOpenAndLaunchOfAxCryptDocumentArgumentNullException()
        {
            string nullString = null;
            AxCryptDocument nullDocument = null;
            ProgressContext nullProgressContext = null;
            FileOperation fileOperation = new FileOperation(Instance.FileSystemState, new SessionNotify());

            Assert.Throws<ArgumentNullException>(() => { fileOperation.OpenAndLaunchApplication(nullString, new AxCryptDocument(), new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, nullDocument, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, new AxCryptDocument(), nullProgressContext); });
        }

        [Test]
        public static void TestFileDoesNotExist()
        {
            IEnumerable<AesKey> keys = new AesKey[] { new Passphrase("a").DerivedPassphrase };
            FileOperation fileOperation = new FileOperation(Instance.FileSystemState, new SessionNotify());

            FileOperationStatus status = fileOperation.OpenAndLaunchApplication(_rootPath.PathCombine("Documents", "HelloWorld-NotThere.axx"), keys, new ProgressContext());
            Assert.That(status, Is.EqualTo(FileOperationStatus.FileDoesNotExist), "The launch should fail with status FileDoesNotExist.");
        }

        [Test]
        public static void TestFileAlreadyDecryptedWithKnownKey()
        {
            IEnumerable<AesKey> keys = new AesKey[] { new Passphrase("a").DerivedPassphrase };

            DateTime utcNow = DateTime.UtcNow;
            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = () => { return utcNow; };
            FileOperation fileOperation = new FileOperation(Instance.FileSystemState, new SessionNotify());
            FileOperationStatus status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed.");

            IRuntimeFileInfo fileInfo = Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath);
            ActiveFile destinationActiveFile = Instance.FileSystemState.FindEncryptedPath(fileInfo.FullName);
            Assert.That(destinationActiveFile.DecryptedFileInfo.LastWriteTimeUtc, Is.Not.EqualTo(utcNow), "The decryption should restore the time stamp of the original file, and this is not now.");
            destinationActiveFile.DecryptedFileInfo.SetFileTimes(utcNow, utcNow, utcNow);
            status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());
            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed this time too.");
            destinationActiveFile = Instance.FileSystemState.FindEncryptedPath(fileInfo.FullName);
            Assert.That(destinationActiveFile.DecryptedFileInfo.LastWriteTimeUtc, Is.EqualTo(utcNow), "There should be no decryption again necessary, and thus the time stamp should be as just set.");
        }

        [Test]
        public static void TestFileAlreadyDecryptedButWithUnknownKey()
        {
            IEnumerable<AesKey> keys = new AesKey[] { new Passphrase("a").DerivedPassphrase };

            DateTime utcNow = DateTime.UtcNow;
            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = () => { return utcNow; };
            FileOperation fileOperation = new FileOperation(Instance.FileSystemState, new SessionNotify());
            FileOperationStatus status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed.");

            IRuntimeFileInfo fileInfo = Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath);
            ActiveFile destinationActiveFile = Instance.FileSystemState.FindEncryptedPath(fileInfo.FullName);
            Assert.That(destinationActiveFile.DecryptedFileInfo.LastWriteTimeUtc, Is.Not.EqualTo(utcNow), "The decryption should restore the time stamp of the original file, and this is not now.");
            destinationActiveFile.DecryptedFileInfo.SetFileTimes(utcNow, utcNow, utcNow);

            IEnumerable<AesKey> badKeys = new AesKey[] { new Passphrase("b").DerivedPassphrase };

            status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, badKeys, new ProgressContext());
            Assert.That(status, Is.EqualTo(FileOperationStatus.InvalidKey), "The launch should fail this time, since the key is not known.");
            destinationActiveFile = Instance.FileSystemState.FindEncryptedPath(fileInfo.FullName);
            Assert.That(destinationActiveFile.DecryptedFileInfo.LastWriteTimeUtc, Is.EqualTo(utcNow), "There should be no decryption, and thus the time stamp should be as just set.");
        }

        [Test]
        public static void TestInvalidKey()
        {
            IEnumerable<AesKey> keys = new AesKey[] { new Passphrase("b").DerivedPassphrase };
            FileOperation fileOperation = new FileOperation(Instance.FileSystemState, new SessionNotify());

            FileOperationStatus status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());
            Assert.That(status, Is.EqualTo(FileOperationStatus.InvalidKey), "The key is invalid, so the launch should fail with that status.");
        }

        [Test]
        public static void TestNoProcessLaunched()
        {
            IEnumerable<AesKey> keys = new AesKey[] { new Passphrase("a").DerivedPassphrase };

            FakeLauncher launcher = null;
            SetupAssembly.FakeRuntimeEnvironment.Launcher = ((string path) =>
            {
                launcher = new FakeLauncher(path);
                launcher.WasStarted = false;
                return launcher;
            });
            FileOperation fileOperation = new FileOperation(Instance.FileSystemState, new SessionNotify());

            FileOperationStatus status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed even if no process was actually launched.");
            Assert.That(launcher, Is.Not.Null, "There should be a call to launch to try launching.");
            Assert.That(Path.GetFileName(launcher.Path), Is.EqualTo("HelloWorld-Key-a.txt"), "The file should be decrypted and the name should be the original from the encrypted headers.");
        }

        [Test]
        public static void TestWin32Exception()
        {
            IEnumerable<AesKey> keys = new AesKey[] { new Passphrase("a").DerivedPassphrase };

            SetupAssembly.FakeRuntimeEnvironment.Launcher = ((string path) =>
            {
                throw new Win32Exception("Fake Win32Exception from Unit Test.");
            });

            FileOperation fileOperation = new FileOperation(Instance.FileSystemState, new SessionNotify());
            FileOperationStatus status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());

            Assert.That(status, Is.EqualTo(FileOperationStatus.CannotStartApplication), "The launch should fail since the launch throws a Win32Exception.");
        }

        [Test]
        public static void TestImmediateExit()
        {
            IEnumerable<AesKey> keys = new AesKey[] { new Passphrase("a").DerivedPassphrase };

            FakeLauncher launcher = null;
            SetupAssembly.FakeRuntimeEnvironment.Launcher = ((string path) =>
            {
                launcher = new FakeLauncher(path);
                launcher.WasStarted = true;
                launcher.HasExited = true;
                return launcher;
            });

            FileOperation fileOperation = new FileOperation(Instance.FileSystemState, new SessionNotify());
            FileOperationStatus status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed even if the process exits immediately.");
            Assert.That(launcher, Is.Not.Null, "There should be a call to launch to try launching.");
            Assert.That(Path.GetFileName(launcher.Path), Is.EqualTo("HelloWorld-Key-a.txt"), "The file should be decrypted and the name should be the original from the encrypted headers.");
        }

        [Test]
        public static void TestExitEvent()
        {
            IEnumerable<AesKey> keys = new AesKey[] { new Passphrase("a").DerivedPassphrase };

            FakeLauncher launcher = null;
            SetupAssembly.FakeRuntimeEnvironment.Launcher = ((string path) =>
            {
                launcher = new FakeLauncher(path);
                launcher.WasStarted = true;
                return launcher;
            });

            SessionNotify notificationMonitor = new SessionNotify();

            FileOperation fileOperation = new FileOperation(Instance.FileSystemState, notificationMonitor);
            FileOperationStatus status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed.");
            Assert.That(launcher, Is.Not.Null, "There should be a call to launch to try launching.");
            Assert.That(Path.GetFileName(launcher.Path), Is.EqualTo("HelloWorld-Key-a.txt"), "The file should be decrypted and the name should be the original from the encrypted headers.");

            bool changedWasRaised = false;

            notificationMonitor.Notification += (object sender, SessionNotificationEventArgs e) => { changedWasRaised = true; };
            Assert.That(changedWasRaised, Is.False, "The global changed event should not have been raised yet.");

            launcher.RaiseExited();
            Assert.That(changedWasRaised, Is.True, "The global changed event should be raised when the process exits.");
        }

        [Test]
        public static void TestFileContainedByActiveFilesButNotDecrypted()
        {
            IEnumerable<AesKey> keys = new AesKey[] { new Passphrase("a").DerivedPassphrase };

            FileOperation fileOperation = new FileOperation(Instance.FileSystemState, new SessionNotify());
            FileOperationStatus status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed.");

            IRuntimeFileInfo fileInfo = Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath);
            ActiveFile destinationActiveFile = Instance.FileSystemState.FindEncryptedPath(fileInfo.FullName);
            destinationActiveFile.DecryptedFileInfo.Delete();
            destinationActiveFile = new ActiveFile(destinationActiveFile, ActiveFileStatus.NotDecrypted);
            Instance.FileSystemState.Add(destinationActiveFile);
            Instance.FileSystemState.Save();

            status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());
            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The launch should once again succeed.");
        }

        [Test]
        public static void TestGetTemporaryDestinationName()
        {
            string temporaryDestinationName = FileOperation.GetTemporaryDestinationName(_davidCopperfieldTxtPath);

            Assert.That(temporaryDestinationName.StartsWith(Path.GetDirectoryName(Factory.Instance.Singleton<WorkFolder>().FileInfo.FullName), StringComparison.OrdinalIgnoreCase), "The temporary destination should be in the temporary directory.");
            Assert.That(Path.GetFileName(temporaryDestinationName), Is.EqualTo(Path.GetFileName(_davidCopperfieldTxtPath)), "The temporary destination should have the same file name.");
        }
    }
}