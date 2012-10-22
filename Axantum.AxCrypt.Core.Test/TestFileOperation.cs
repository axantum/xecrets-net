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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Core.UI;
using Microsoft;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestFileOperation
    {
        private static FileSystemState _fileSystemState;

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

            FakeRuntimeFileInfo.AddFile(_testTextPath, FakeRuntimeFileInfo.TestDate1Utc, FakeRuntimeFileInfo.TestDate2Utc, FakeRuntimeFileInfo.TestDate1Utc, new MemoryStream(Encoding.UTF8.GetBytes("This is a short file")));
            FakeRuntimeFileInfo.AddFile(_davidCopperfieldTxtPath, FakeRuntimeFileInfo.TestDate4Utc, FakeRuntimeFileInfo.TestDate5Utc, FakeRuntimeFileInfo.TestDate6Utc, new MemoryStream(Encoding.GetEncoding(1252).GetBytes(Resources.david_copperfield)));
            FakeRuntimeFileInfo.AddFile(_uncompressedAxxPath, new MemoryStream(Resources.uncompressable_zip));
            FakeRuntimeFileInfo.AddFile(_helloWorldAxxPath, new MemoryStream(Resources.helloworld_key_a_txt));

            _fileSystemState = new FileSystemState();
            _fileSystemState.Load(OS.Current.FileInfo(Path.Combine(Path.GetTempPath(), "FileSystemState.xml")));
        }

        [TearDown]
        public static void Teardown()
        {
            _fileSystemState.Dispose();
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestInvalidArguments()
        {
            FileSystemState nullFileSystemState = null;

            string file = _helloWorldAxxPath;
            string nullFile = null;

            IEnumerable<AesKey> keys = new AesKey[] { new Passphrase("a").DerivedPassphrase };
            IEnumerable<AesKey> nullKeys = null;

            ProgressContext context = new ProgressContext();
            ProgressContext nullContext = null;

            Assert.Throws<ArgumentNullException>(() => { nullFileSystemState.OpenAndLaunchApplication(file, keys, context); }, "The FileSystemState is null.");
            Assert.Throws<ArgumentNullException>(() => { _fileSystemState.OpenAndLaunchApplication(nullFile, keys, context); }, "The file string is null.");
            Assert.Throws<ArgumentNullException>(() => { _fileSystemState.OpenAndLaunchApplication(file, nullKeys, context); }, "The keys are null.");
            Assert.Throws<ArgumentNullException>(() => { _fileSystemState.OpenAndLaunchApplication(file, keys, nullContext); }, "The context is null.");
        }

        [Test]
        public static void TestSimpleOpenAndLaunch()
        {
            IEnumerable<AesKey> keys = new AesKey[] { new Passphrase("a").DerivedPassphrase };

            FakeLauncher launcher = null;
            SetupAssembly.FakeRuntimeEnvironment.Launcher = ((string path) =>
            {
                launcher = new FakeLauncher(path);
                return launcher;
            });

            FileOperationStatus status = _fileSystemState.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed.");
            Assert.That(launcher, Is.Not.Null, "There should be a call to launch.");
            Assert.That(Path.GetFileName(launcher.Path), Is.EqualTo("HelloWorld-Key-a.txt"), "The file should be decrypted and the name should be the original from the encrypted headers.");
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
            using (AxCryptDocument document = new AxCryptDocument())
            {
                using (Stream stream = OS.Current.FileInfo(_helloWorldAxxPath).OpenRead())
                {
                    document.Load(stream, new Passphrase("a").DerivedPassphrase);
                    status = _fileSystemState.OpenAndLaunchApplication(_helloWorldAxxPath, document, new ProgressContext());
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
            using (AxCryptDocument document = new AxCryptDocument())
            {
                using (Stream stream = OS.Current.FileInfo(_helloWorldAxxPath).OpenRead())
                {
                    document.Load(stream, new Passphrase("a").DerivedPassphrase);
                    status = _fileSystemState.OpenAndLaunchApplication(_helloWorldAxxPath, document, new ProgressContext());
                }
            }

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed.");
            Assert.That(launcher, Is.Not.Null, "There should be a call to launch.");
            Assert.That(Path.GetFileName(launcher.Path), Is.EqualTo("HelloWorld-Key-a.txt"), "The file should be decrypted and the name should be the original from the encrypted headers.");
        }

        [Test]
        public static void TestOpenAndLaunchOfAxCryptDocumentArgumentNullException()
        {
            FileSystemState nullFileSystemState = null;
            string nullString = null;
            AxCryptDocument nullDocument = null;
            ProgressContext nullProgressContext = null;

            Assert.Throws<ArgumentNullException>(() => { nullFileSystemState.OpenAndLaunchApplication(_helloWorldAxxPath, new AxCryptDocument(), new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { _fileSystemState.OpenAndLaunchApplication(nullString, new AxCryptDocument(), new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { _fileSystemState.OpenAndLaunchApplication(_helloWorldAxxPath, nullDocument, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { _fileSystemState.OpenAndLaunchApplication(_helloWorldAxxPath, new AxCryptDocument(), nullProgressContext); });
        }

        [Test]
        public static void TestFileDoesNotExist()
        {
            IEnumerable<AesKey> keys = new AesKey[] { new Passphrase("a").DerivedPassphrase };

            FileOperationStatus status = _fileSystemState.OpenAndLaunchApplication(_rootPath.PathCombine("Documents", "HelloWorld-NotThere.axx"), keys, new ProgressContext());
            Assert.That(status, Is.EqualTo(FileOperationStatus.FileDoesNotExist), "The launch should fail with status FileDoesNotExist.");
        }

        [Test]
        public static void TestFileAlreadyDecryptedWithKnownKey()
        {
            IEnumerable<AesKey> keys = new AesKey[] { new Passphrase("a").DerivedPassphrase };

            DateTime utcNow = DateTime.UtcNow;
            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = () => { return utcNow; };
            FileOperationStatus status = _fileSystemState.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed.");

            IRuntimeFileInfo fileInfo = OS.Current.FileInfo(_helloWorldAxxPath);
            ActiveFile destinationActiveFile = _fileSystemState.FindEncryptedPath(fileInfo.FullName);
            Assert.That(destinationActiveFile.DecryptedFileInfo.LastWriteTimeUtc, Is.Not.EqualTo(utcNow), "The decryption should restore the time stamp of the original file, and this is not now.");
            destinationActiveFile.DecryptedFileInfo.SetFileTimes(utcNow, utcNow, utcNow);
            status = _fileSystemState.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());
            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed this time too.");
            destinationActiveFile = _fileSystemState.FindEncryptedPath(fileInfo.FullName);
            Assert.That(destinationActiveFile.DecryptedFileInfo.LastWriteTimeUtc, Is.EqualTo(utcNow), "There should be no decryption again necessary, and thus the time stamp should be as just set.");
        }

        [Test]
        public static void TestFileAlreadyDecryptedButWithUnknownKey()
        {
            IEnumerable<AesKey> keys = new AesKey[] { new Passphrase("a").DerivedPassphrase };

            DateTime utcNow = DateTime.UtcNow;
            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = () => { return utcNow; };
            FileOperationStatus status = _fileSystemState.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed.");

            IRuntimeFileInfo fileInfo = OS.Current.FileInfo(_helloWorldAxxPath);
            ActiveFile destinationActiveFile = _fileSystemState.FindEncryptedPath(fileInfo.FullName);
            Assert.That(destinationActiveFile.DecryptedFileInfo.LastWriteTimeUtc, Is.Not.EqualTo(utcNow), "The decryption should restore the time stamp of the original file, and this is not now.");
            destinationActiveFile.DecryptedFileInfo.SetFileTimes(utcNow, utcNow, utcNow);

            IEnumerable<AesKey> badKeys = new AesKey[] { new Passphrase("b").DerivedPassphrase };

            status = _fileSystemState.OpenAndLaunchApplication(_helloWorldAxxPath, badKeys, new ProgressContext());
            Assert.That(status, Is.EqualTo(FileOperationStatus.InvalidKey), "The launch should fail this time, since the key is not known.");
            destinationActiveFile = _fileSystemState.FindEncryptedPath(fileInfo.FullName);
            Assert.That(destinationActiveFile.DecryptedFileInfo.LastWriteTimeUtc, Is.EqualTo(utcNow), "There should be no decryption, and thus the time stamp should be as just set.");
        }

        [Test]
        public static void TestInvalidKey()
        {
            IEnumerable<AesKey> keys = new AesKey[] { new Passphrase("b").DerivedPassphrase };

            FileOperationStatus status = _fileSystemState.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());
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

            FileOperationStatus status = _fileSystemState.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());

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

            FileOperationStatus status = _fileSystemState.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());

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

            FileOperationStatus status = _fileSystemState.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());

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

            FileOperationStatus status = _fileSystemState.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed.");
            Assert.That(launcher, Is.Not.Null, "There should be a call to launch to try launching.");
            Assert.That(Path.GetFileName(launcher.Path), Is.EqualTo("HelloWorld-Key-a.txt"), "The file should be decrypted and the name should be the original from the encrypted headers.");

            bool changedWasRaised = false;
            OS.Current.FileChanged += (object sender, EventArgs e) => { changedWasRaised = true; };
            Assert.That(changedWasRaised, Is.False, "The global changed event should not have been raised yet.");

            launcher.RaiseExited();
            Assert.That(changedWasRaised, Is.True, "The global changed event should be raised when the process exits.");
        }

        [Test]
        public static void TestFileContainedByActiveFilesButNotDecrypted()
        {
            IEnumerable<AesKey> keys = new AesKey[] { new Passphrase("a").DerivedPassphrase };

            FileOperationStatus status = _fileSystemState.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The launch should succeed.");

            IRuntimeFileInfo fileInfo = OS.Current.FileInfo(_helloWorldAxxPath);
            ActiveFile destinationActiveFile = _fileSystemState.FindEncryptedPath(fileInfo.FullName);
            destinationActiveFile.DecryptedFileInfo.Delete();
            destinationActiveFile = new ActiveFile(destinationActiveFile, ActiveFileStatus.NotDecrypted);
            _fileSystemState.Add(destinationActiveFile);
            _fileSystemState.Save();

            status = _fileSystemState.OpenAndLaunchApplication(_helloWorldAxxPath, keys, new ProgressContext());
            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The launch should once again succeed.");
        }

        [Test]
        public static void TestGetTemporaryDestinationName()
        {
            string temporaryDestinationName = FileOperation.GetTemporaryDestinationName(_davidCopperfieldTxtPath);

            Assert.That(temporaryDestinationName.StartsWith(Path.GetDirectoryName(OS.Current.TemporaryDirectoryInfo.FullName), StringComparison.OrdinalIgnoreCase), "The temporary destination should be in the temporary directory.");
            Assert.That(Path.GetFileName(temporaryDestinationName), Is.EqualTo(Path.GetFileName(_davidCopperfieldTxtPath)), "The temporary destination should have the same file name.");
        }
    }
}