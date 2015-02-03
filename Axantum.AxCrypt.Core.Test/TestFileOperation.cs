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

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestFileOperation
    {
        private static readonly string _rootPath = Path.GetPathRoot(Environment.CurrentDirectory);
        private static readonly string _testTextPath = Path.Combine(_rootPath, "test.txt");
        private static readonly string _davidCopperfieldTxtPath = _rootPath.PathCombine("Users", "AxCrypt", "David Copperfield.txt");
        private static readonly string _uncompressedAxxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Uncompressed.axx");
        private static readonly string _helloWorldAxxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HelloWorld.axx");

        private CryptoImplementation _cryptoImplementation;

        public TestFileOperation(CryptoImplementation cryptoImplementation)
        {
            _cryptoImplementation = cryptoImplementation;
        }

        [TestFixtureSetUp]
        public void SetupFixture()
        {
        }

        [TestFixtureTearDown]
        public void TeardownFixture()
        {
        }

        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup();
            SetupAssembly.AssemblySetupCrypto(_cryptoImplementation);

            FakeDataStore.AddFile(_testTextPath, FakeDataStore.TestDate1Utc, FakeDataStore.TestDate2Utc, FakeDataStore.TestDate1Utc, FakeDataStore.ExpandableMemoryStream(Encoding.UTF8.GetBytes("This is a short file")));
            FakeDataStore.AddFile(_davidCopperfieldTxtPath, FakeDataStore.TestDate4Utc, FakeDataStore.TestDate5Utc, FakeDataStore.TestDate6Utc, FakeDataStore.ExpandableMemoryStream(Encoding.GetEncoding(1252).GetBytes(Resources.david_copperfield)));
            FakeDataStore.AddFile(_uncompressedAxxPath, FakeDataStore.ExpandableMemoryStream(Resources.uncompressable_zip));
            FakeDataStore.AddFile(_helloWorldAxxPath, FakeDataStore.ExpandableMemoryStream(Resources.helloworld_key_a_txt));
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public void TestInvalidArguments()
        {
            string file = _helloWorldAxxPath;
            string nullFile = null;

            IEnumerable<LogOnIdentity> keys = new LogOnIdentity[] { new LogOnIdentity("a") };
            IEnumerable<LogOnIdentity> nullKeys = null;

            ProgressContext context = new ProgressContext();
            ProgressContext nullContext = null;

            FileOperation fileOperation = new FileOperation(Resolve.FileSystemState, new SessionNotify());
            Assert.Throws<ArgumentNullException>(() => { fileOperation.OpenAndLaunchApplication(nullFile, keys, context); }, "The file string is null.");
            Assert.Throws<ArgumentNullException>(() => { fileOperation.OpenAndLaunchApplication(file, nullKeys, context); }, "The keys are null.");
            Assert.Throws<ArgumentNullException>(() => { fileOperation.OpenAndLaunchApplication(file, keys, nullContext); }, "The context is null.");
        }

        [Test]
        public void TestSimpleOpenAndLaunch()
        {
            IEnumerable<LogOnIdentity> keys = new LogOnIdentity[] { new LogOnIdentity("a") };

            var mock = new Mock<ILauncher>() { CallBase = true };
            string launcherPath = null;
            mock.Setup(x => x.Launch(It.IsAny<string>())).Callback((string path) => launcherPath = path);
            TypeMap.Register.New<ILauncher>(() => mock.Object);

            FileOperation fileOperation = new FileOperation(Resolve.FileSystemState, new SessionNotify());
            FileOperationContext status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());

            Assert.That(status.Status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed.");
            Assert.DoesNotThrow(() => mock.Verify(x => x.Launch(launcherPath)));
        }

        [Test]
        public void TestOpenAndLaunchOfAxCryptDocument()
        {
            FakeLauncher launcher = new FakeLauncher();
            bool called = false;
            TypeMap.Register.New<ILauncher>(() => { called = true; return launcher; });

            FileOperationContext status;
            FileOperation fileOperation = new FileOperation(Resolve.FileSystemState, new SessionNotify());
            using (IAxCryptDocument document = new V1AxCryptDocument())
            {
                using (Stream stream = TypeMap.Resolve.New<IDataStore>(_helloWorldAxxPath).OpenRead())
                {
                    document.Load(new Passphrase("a"), V1Aes128CryptoFactory.CryptoId, stream);
                    status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, new LogOnIdentity("a"), document, new ProgressContext());
                }
            }

            Assert.That(status.Status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed.");
            Assert.That(called, Is.True, "There should be a call to launch.");
            Assert.That(Path.GetFileName(launcher.Path), Is.EqualTo("HelloWorld-Key-a.txt"), "The file should be decrypted and the name should be the original from the encrypted headers.");
        }

        [Test]
        public void TestOpenAndLaunchOfAxCryptDocumentWhenAlreadyDecrypted()
        {
            TestOpenAndLaunchOfAxCryptDocument();

            bool called = false;
            FakeLauncher launcher = new FakeLauncher();
            TypeMap.Register.New<ILauncher>(() => { called = true; return launcher; });
            FileOperationContext status;
            FileOperation fileOperation = new FileOperation(Resolve.FileSystemState, new SessionNotify());
            using (IAxCryptDocument document = new V1AxCryptDocument())
            {
                using (Stream stream = TypeMap.Resolve.New<IDataStore>(_helloWorldAxxPath).OpenRead())
                {
                    document.Load(new Passphrase("a"), V1Aes128CryptoFactory.CryptoId, stream);
                    status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, new LogOnIdentity("a"), document, new ProgressContext());
                }
            }

            Assert.That(status.Status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed.");
            Assert.That(called, Is.True, "There should be a call to launch.");
            Assert.That(Path.GetFileName(launcher.Path), Is.EqualTo("HelloWorld-Key-a.txt"), "The file should be decrypted and the name should be the original from the encrypted headers.");
        }

        [Test]
        public void TestOpenAndLaunchOfAxCryptDocumentArgumentNullException()
        {
            string nullString = null;
            IAxCryptDocument nullDocument = null;
            ProgressContext nullProgressContext = null;
            FileOperation fileOperation = new FileOperation(Resolve.FileSystemState, new SessionNotify());

            Assert.Throws<ArgumentNullException>(() => { fileOperation.OpenAndLaunchApplication(nullString, LogOnIdentity.Empty, new V1AxCryptDocument(), new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, LogOnIdentity.Empty, nullDocument, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, LogOnIdentity.Empty, new V1AxCryptDocument(), nullProgressContext); });
        }

        [Test]
        public void TestFileDoesNotExist()
        {
            IEnumerable<LogOnIdentity> keys = new LogOnIdentity[] { new LogOnIdentity("a") };
            FileOperation fileOperation = new FileOperation(Resolve.FileSystemState, new SessionNotify());

            FileOperationContext status = fileOperation.OpenAndLaunchApplication(_rootPath.PathCombine("Documents", "HelloWorld-NotThere.axx"), keys, new ProgressContext());
            Assert.That(status.Status, Is.EqualTo(FileOperationStatus.FileDoesNotExist), "The launch should fail with status FileDoesNotExist.");
        }

        [Test]
        public void TestFileAlreadyDecryptedWithKnownKey()
        {
            TypeMap.Register.New<ILauncher>(() => new FakeLauncher());

            IEnumerable<LogOnIdentity> keys = new LogOnIdentity[] { new LogOnIdentity("a") };

            DateTime utcNow = DateTime.UtcNow;
            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = () => { return utcNow; };
            FileOperation fileOperation = new FileOperation(Resolve.FileSystemState, new SessionNotify());
            FileOperationContext status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());

            Assert.That(status.Status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed.");

            IDataStore fileInfo = TypeMap.Resolve.New<IDataStore>(_helloWorldAxxPath);
            ActiveFile destinationActiveFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(fileInfo.FullName);
            Assert.That(destinationActiveFile.DecryptedFileInfo.LastWriteTimeUtc, Is.Not.EqualTo(utcNow), "The decryption should restore the time stamp of the original file, and this is not now.");
            destinationActiveFile.DecryptedFileInfo.SetFileTimes(utcNow, utcNow, utcNow);
            status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());
            Assert.That(status.Status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed this time too.");
            destinationActiveFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(fileInfo.FullName);
            Assert.That(destinationActiveFile.DecryptedFileInfo.LastWriteTimeUtc, Is.EqualTo(utcNow), "There should be no decryption again necessary, and thus the time stamp should be as just set.");
        }

        [Test]
        public void TestFileAlreadyDecryptedButWithUnknownKey()
        {
            TypeMap.Register.New<ILauncher>(() => new FakeLauncher());
            IEnumerable<LogOnIdentity> keys = new LogOnIdentity[] { new LogOnIdentity("a") };

            DateTime utcNow = DateTime.UtcNow;
            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = () => { return utcNow; };
            FileOperation fileOperation = new FileOperation(Resolve.FileSystemState, new SessionNotify());
            FileOperationContext status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());

            Assert.That(status.Status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed.");

            IDataStore fileInfo = TypeMap.Resolve.New<IDataStore>(_helloWorldAxxPath);
            ActiveFile destinationActiveFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(fileInfo.FullName);
            Assert.That(destinationActiveFile.DecryptedFileInfo.LastWriteTimeUtc, Is.Not.EqualTo(utcNow), "The decryption should restore the time stamp of the original file, and this is not now.");
            destinationActiveFile.DecryptedFileInfo.SetFileTimes(utcNow, utcNow, utcNow);

            IEnumerable<LogOnIdentity> badKeys = new LogOnIdentity[] { new LogOnIdentity("b") };

            status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, badKeys, new ProgressContext());
            Assert.That(status.Status, Is.EqualTo(FileOperationStatus.InvalidKey), "The launch should fail this time, since the key is not known.");
            destinationActiveFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(fileInfo.FullName);
            Assert.That(destinationActiveFile.DecryptedFileInfo.LastWriteTimeUtc, Is.EqualTo(utcNow), "There should be no decryption, and thus the time stamp should be as just set.");
        }

        [Test]
        public void TestInvalidKey()
        {
            IEnumerable<LogOnIdentity> keys = new LogOnIdentity[] { new LogOnIdentity("b") };
            FileOperation fileOperation = new FileOperation(Resolve.FileSystemState, new SessionNotify());

            FileOperationContext status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());
            Assert.That(status.Status, Is.EqualTo(FileOperationStatus.InvalidKey), "The key is invalid, so the launch should fail with that status.");
        }

        [Test]
        public void TestNoProcessLaunched()
        {
            IEnumerable<LogOnIdentity> keys = new LogOnIdentity[] { new LogOnIdentity("a") };

            FakeLauncher launcher = new FakeLauncher();
            bool called = false;
            TypeMap.Register.New<ILauncher>(() => { called = true; launcher.WasStarted = false; return launcher; });
            FileOperation fileOperation = new FileOperation(Resolve.FileSystemState, new SessionNotify());

            FileOperationContext status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());

            Assert.That(status.Status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed even if no process was actually launched.");
            Assert.That(called, Is.True, "There should be a call to launch to try launching.");
            Assert.That(Path.GetFileName(launcher.Path), Is.EqualTo("HelloWorld-Key-a.txt"), "The file should be decrypted and the name should be the original from the encrypted headers.");
        }

        [Test]
        public void TestWin32Exception()
        {
            IEnumerable<LogOnIdentity> keys = new LogOnIdentity[] { new LogOnIdentity("a") };

            SetupAssembly.FakeRuntimeEnvironment.Launcher = ((string path) =>
            {
                throw new Win32Exception("Fake Win32Exception from Unit Test.");
            });

            FileOperation fileOperation = new FileOperation(Resolve.FileSystemState, new SessionNotify());
            FileOperationContext status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());

            Assert.That(status.Status, Is.EqualTo(FileOperationStatus.CannotStartApplication), "The launch should fail since the launch throws a Win32Exception.");
        }

        [Test]
        public void TestImmediateExit()
        {
            IEnumerable<LogOnIdentity> keys = new LogOnIdentity[] { new LogOnIdentity("a") };

            FakeLauncher launcher = new FakeLauncher();
            bool called = false;
            TypeMap.Register.New<ILauncher>(() => { called = true; launcher.WasStarted = true; launcher.HasExited = true; return launcher; });

            FileOperation fileOperation = new FileOperation(Resolve.FileSystemState, new SessionNotify());
            FileOperationContext status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());

            Assert.That(status.Status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed even if the process exits immediately.");
            Assert.That(called, Is.True, "There should be a call to launch to try launching.");
            Assert.That(Path.GetFileName(launcher.Path), Is.EqualTo("HelloWorld-Key-a.txt"), "The file should be decrypted and the name should be the original from the encrypted headers.");
        }

        [Test]
        public void TestExitEvent()
        {
            IEnumerable<LogOnIdentity> keys = new LogOnIdentity[] { new LogOnIdentity("a") };

            FakeLauncher launcher = new FakeLauncher();
            bool called = false;
            TypeMap.Register.New<ILauncher>(() => { called = true; launcher.WasStarted = true; return launcher; });

            SessionNotify notificationMonitor = new SessionNotify();

            FileOperation fileOperation = new FileOperation(Resolve.FileSystemState, notificationMonitor);
            FileOperationContext status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());

            Assert.That(status.Status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed.");
            Assert.That(called, Is.True, "There should be a call to launch to try launching.");
            Assert.That(Path.GetFileName(launcher.Path), Is.EqualTo("HelloWorld-Key-a.txt"), "The file should be decrypted and the name should be the original from the encrypted headers.");

            bool changedWasRaised = false;

            notificationMonitor.Notification += (object sender, SessionNotificationEventArgs e) => { changedWasRaised = true; };
            Assert.That(changedWasRaised, Is.False, "The global changed event should not have been raised yet.");

            launcher.RaiseExited();
            Assert.That(changedWasRaised, Is.True, "The global changed event should be raised when the process exits.");
        }

        [Test]
        public void TestFileContainedByActiveFilesButNotDecrypted()
        {
            TypeMap.Register.New<ILauncher>(() => new FakeLauncher());

            IEnumerable<LogOnIdentity> keys = new LogOnIdentity[] { new LogOnIdentity("a") };

            FileOperation fileOperation = new FileOperation(Resolve.FileSystemState, new SessionNotify());
            FileOperationContext status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());

            Assert.That(status.Status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed.");

            IDataStore fileInfo = TypeMap.Resolve.New<IDataStore>(_helloWorldAxxPath);
            ActiveFile destinationActiveFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(fileInfo.FullName);
            destinationActiveFile.DecryptedFileInfo.Delete();
            destinationActiveFile = new ActiveFile(destinationActiveFile, ActiveFileStatus.NotDecrypted);
            Resolve.FileSystemState.Add(destinationActiveFile);
            Resolve.FileSystemState.Save();

            status = fileOperation.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());
            Assert.That(status.Status, Is.EqualTo(FileOperationStatus.Success), "The launch should once again succeed.");
        }

        [Test]
        public void TestGetTemporaryDestinationName()
        {
            string temporaryDestinationName = FileOperation.GetTemporaryDestinationName(_davidCopperfieldTxtPath);

            Assert.That(temporaryDestinationName.StartsWith(Path.GetDirectoryName(TypeMap.Resolve.Singleton<WorkFolder>().FileInfo.FullName), StringComparison.OrdinalIgnoreCase), "The temporary destination should be in the temporary directory.");
            Assert.That(Path.GetFileName(temporaryDestinationName), Is.EqualTo(Path.GetFileName(_davidCopperfieldTxtPath)), "The temporary destination should have the same file name.");
        }
    }
}