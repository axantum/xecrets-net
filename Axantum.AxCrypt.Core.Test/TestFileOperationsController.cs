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
using System.IO;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestFileOperationsController
    {
        private static readonly string _rootPath = Path.GetPathRoot(Environment.CurrentDirectory);
        private static readonly string _davidCopperfieldTxtPath = _rootPath.PathCombine("Users", "AxCrypt", "David Copperfield.txt");
        private static readonly string _uncompressedAxxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Uncompressed.axx");
        private static readonly string _helloWorldAxxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HelloWorld.axx");

        private static FileSystemState _fileSystemState;

        [SetUp]
        public static void Setup()
        {
            FakeRuntimeFileInfo.AddFile(_davidCopperfieldTxtPath, FakeRuntimeFileInfo.TestDate4Utc, FakeRuntimeFileInfo.TestDate5Utc, FakeRuntimeFileInfo.TestDate6Utc, new MemoryStream(Encoding.GetEncoding(1252).GetBytes(Resources.david_copperfield)));
            FakeRuntimeFileInfo.AddFile(_uncompressedAxxPath, new MemoryStream(Resources.uncompressable_zip));
            FakeRuntimeFileInfo.AddFile(_helloWorldAxxPath, new MemoryStream(Resources.helloworld_key_a_txt));

            _fileSystemState = new FileSystemState();
            _fileSystemState.Load(FileSystemState.DefaultPathInfo);
        }

        [TearDown]
        public static void Teardown()
        {
            _fileSystemState.Dispose();
            FakeRuntimeFileInfo.ClearFiles();
        }

        [Test]
        public static void TestSimpleEncryptFile()
        {
            FileOperationsController controller = new FileOperationsController(_fileSystemState, "Testing Simple EncryptFile()");
            string destinationPath = String.Empty;
            controller.QueryEncryptionPassphrase += (object sender, FileOperationEventArgs e) =>
                {
                    e.Passphrase = "allan";
                };
            controller.ProcessFile += (object sender, FileOperationEventArgs e) =>
                {
                    destinationPath = e.SaveFileFullName;
                    FileOperationsController c = (FileOperationsController)sender;
                    c.DoProcessFile(e);
                };

            bool encryptionOperationIsOk = controller.EncryptFile(_davidCopperfieldTxtPath);
            Assert.That(encryptionOperationIsOk, "The encrypting operation should indicate success by returning 'true'.");
            Assert.That(controller.Status, Is.EqualTo(FileOperationStatus.Success), "The status should also indicate success.");

            IRuntimeFileInfo destinationInfo = OS.Current.FileInfo(destinationPath);
            Assert.That(destinationInfo.Exists, "After encryption the destination file should be created.");
            using (AxCryptDocument document = new AxCryptDocument())
            {
                using (Stream stream = destinationInfo.OpenRead())
                {
                    document.Load(stream, new Passphrase("allan").DerivedPassphrase);
                    Assert.That(document.PassphraseIsValid, "The encrypted document should be valid and encrypted with the passphrase given.");
                }
            }
        }

        [Test]
        public static void TestSimleDecryptFile()
        {
            FileOperationsController controller = new FileOperationsController(_fileSystemState, "Testing Simple DecryptFile()");
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
                {
                    e.Passphrase = "a";
                };
            string destinationPath = String.Empty;
            controller.ProcessFile += (object sender, FileOperationEventArgs e) =>
                {
                    destinationPath = e.SaveFileFullName;
                    FileOperationsController c = (FileOperationsController)sender;
                    c.DoProcessFile(e);
                };
            bool knownKeyWasAdded = false;
            controller.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
                {
                    knownKeyWasAdded = e.Key == new Passphrase("a").DerivedPassphrase;
                };
            bool decryptFileIsOk = controller.DecryptFile(_helloWorldAxxPath);

            Assert.That(decryptFileIsOk, "The operation should return true to indicate success.");
            Assert.That(controller.Status, Is.EqualTo(FileOperationStatus.Success), "The status should indicate success.");
            Assert.That(knownKeyWasAdded, "A new known key was used, so the KnownKeyAdded event should have been raised.");
            IRuntimeFileInfo destinationInfo = OS.Current.FileInfo(destinationPath);
            Assert.That(destinationInfo.Exists, "After decryption the destination file should be created.");

            string fileContent;
            using (Stream stream = destinationInfo.OpenRead())
            {
                fileContent = new StreamReader(stream).ReadToEnd();
            }
            Assert.That(fileContent.Contains("Hello"), "A file named Hello World should contain that text when decrypted.");
        }

        [Test]
        public static void TestSimpleDecryptAndLaunch()
        {
            FakeLauncher launcher = null;
            FakeRuntimeEnvironment environment = (FakeRuntimeEnvironment)OS.Current;
            environment.Launcher = ((string path) =>
            {
                launcher = new FakeLauncher(path);
                return launcher;
            });

            FileOperationsController controller = new FileOperationsController(_fileSystemState, "Testing Simple DecryptAndLaunch()");
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.Passphrase = "a";
            };
            string destinationPath = String.Empty;
            controller.ProcessFile += (object sender, FileOperationEventArgs e) =>
            {
                destinationPath = e.SaveFileFullName;
                FileOperationsController c = (FileOperationsController)sender;
                c.DoProcessFile(e);
            };
            bool decryptFileIsOk = controller.DecryptAndLaunch(_helloWorldAxxPath);

            Assert.That(decryptFileIsOk, "The operation should return true to indicate success.");
            Assert.That(controller.Status, Is.EqualTo(FileOperationStatus.Success), "The status should indicate success.");

            Assert.That(launcher, Is.Not.Null, "There should be a call to launch.");
            Assert.That(Path.GetFileName(launcher.Path), Is.EqualTo("HelloWorld-Key-a.txt"), "The file should be decrypted and the name should be the original from the encrypted headers.");

            IRuntimeFileInfo destinationInfo = OS.Current.FileInfo(launcher.Path);
            Assert.That(destinationInfo.Exists, "After decryption the destination file should be created.");

            string fileContent;
            using (Stream stream = destinationInfo.OpenRead())
            {
                fileContent = new StreamReader(stream).ReadToEnd();
            }

            Assert.That(fileContent.Contains("Hello"), "A file named Hello World should contain that text when decrypted.");
        }

        [Test]
        public static void TestCanceledDecryptAndLaunch()
        {
            FileOperationsController controller = new FileOperationsController(_fileSystemState, "Testing Simple DecryptAndLaunch()");
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.Cancel = true;
            };
            controller.ProcessFile += (object sender, FileOperationEventArgs e) =>
            {
                FileOperationsController c = (FileOperationsController)sender;
                c.DoProcessFile(e);
            };
            bool decryptFileIsOk = controller.DecryptAndLaunch(_helloWorldAxxPath);
            Assert.That(decryptFileIsOk, Is.False, "The operation should return false to indicate failure.");
            Assert.That(controller.Status, Is.EqualTo(FileOperationStatus.Canceled), "The status should indicate cancellation.");
        }
    }
}