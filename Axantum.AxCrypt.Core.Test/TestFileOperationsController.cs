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
using Axantum.AxCrypt.Core.Runtime;
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
            SetupAssembly.AssemblySetup();

            FakeRuntimeFileInfo.AddFile(_davidCopperfieldTxtPath, FakeRuntimeFileInfo.TestDate4Utc, FakeRuntimeFileInfo.TestDate5Utc, FakeRuntimeFileInfo.TestDate6Utc, FakeRuntimeFileInfo.ExpandableMemoryStream(Encoding.GetEncoding(1252).GetBytes(Resources.david_copperfield)));
            FakeRuntimeFileInfo.AddFile(_uncompressedAxxPath, FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.uncompressable_zip));
            FakeRuntimeFileInfo.AddFile(_helloWorldAxxPath, FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.helloworld_key_a_txt));

            _fileSystemState = new FileSystemState();
            _fileSystemState.Load(FileSystemState.DefaultPathInfo);
        }

        [TearDown]
        public static void Teardown()
        {
            _fileSystemState.Dispose();
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestSimpleEncryptFile()
        {
            FileOperationsController controller = new FileOperationsController(_fileSystemState);
            string destinationPath = String.Empty;
            controller.QueryEncryptionPassphrase += (object sender, FileOperationEventArgs e) =>
                {
                    e.Passphrase = "allan";
                };
            controller.Completed += (object sender, FileOperationEventArgs e) =>
            {
                destinationPath = e.SaveFileFullName;
            };

            FileOperationStatus status = controller.EncryptFile(_davidCopperfieldTxtPath);
            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The status should indicate success.");

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
        public static void TestSimpleEncryptFileOnThreadWorker()
        {
            FileOperationsController controller = new FileOperationsController(_fileSystemState);
            controller.QueryEncryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.Passphrase = "allan";
            };
            string destinationPath = String.Empty;
            FileOperationStatus status = FileOperationStatus.Unknown;
            controller.Completed += (object sender, FileOperationEventArgs e) =>
            {
                destinationPath = e.SaveFileFullName;
                status = e.Status;
            };

            using (ThreadWorker worker = new ThreadWorker(new ProgressContext()))
            {
                controller.EncryptFile(_davidCopperfieldTxtPath, worker);
                worker.Join();
            }
            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The status should indicate success.");

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
        public static void TestEncryptFileWithDefaultEncryptionKey()
        {
            _fileSystemState.KnownKeys.DefaultEncryptionKey = new Passphrase("default").DerivedPassphrase;
            FileOperationsController controller = new FileOperationsController(_fileSystemState);
            bool queryEncryptionPassphraseWasCalled = false;
            controller.QueryEncryptionPassphrase += (object sender, FileOperationEventArgs e) =>
                {
                    queryEncryptionPassphraseWasCalled = true;
                };
            string destinationPath = String.Empty;
            controller.Completed += (object sender, FileOperationEventArgs e) =>
                {
                    destinationPath = e.SaveFileFullName;
                };

            FileOperationStatus status = controller.EncryptFile(_davidCopperfieldTxtPath);
            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The status should indicate success.");
            Assert.That(!queryEncryptionPassphraseWasCalled, "No query of encryption passphrase should be needed since there is a default set.");

            IRuntimeFileInfo destinationInfo = OS.Current.FileInfo(destinationPath);
            Assert.That(destinationInfo.Exists, "After encryption the destination file should be created.");
            using (AxCryptDocument document = new AxCryptDocument())
            {
                using (Stream stream = destinationInfo.OpenRead())
                {
                    document.Load(stream, new Passphrase("default").DerivedPassphrase);
                    Assert.That(document.PassphraseIsValid, "The encrypted document should be valid and encrypted with the default passphrase given.");
                }
            }
        }

        [Test]
        public static void TestEncryptFileWhenDestinationExists()
        {
            IRuntimeFileInfo sourceInfo = OS.Current.FileInfo(_davidCopperfieldTxtPath);
            IRuntimeFileInfo expectedDestinationInfo = OS.Current.FileInfo(AxCryptFile.MakeAxCryptFileName(sourceInfo));
            using (Stream stream = expectedDestinationInfo.OpenWrite())
            {
            }

            FileOperationsController controller = new FileOperationsController(_fileSystemState);
            string destinationPath = String.Empty;
            controller.QueryEncryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.Passphrase = "allan";
            };
            controller.QuerySaveFileAs += (object sender, FileOperationEventArgs e) =>
            {
                e.SaveFileFullName = Path.Combine(Path.GetDirectoryName(e.SaveFileFullName), "alternative-name.axx");
            };
            controller.Completed += (object sender, FileOperationEventArgs e) =>
            {
                destinationPath = e.SaveFileFullName;
            };

            FileOperationStatus status = controller.EncryptFile(_davidCopperfieldTxtPath);
            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The status should indicate success.");

            Assert.That(Path.GetFileName(destinationPath), Is.EqualTo("alternative-name.axx"), "The alternative name should be used, since the default existed.");
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
        public static void TestEncryptFileWhenCanceledDuringQuerySaveAs()
        {
            IRuntimeFileInfo sourceInfo = OS.Current.FileInfo(_davidCopperfieldTxtPath);
            IRuntimeFileInfo expectedDestinationInfo = OS.Current.FileInfo(AxCryptFile.MakeAxCryptFileName(sourceInfo));
            using (Stream stream = expectedDestinationInfo.OpenWrite())
            {
            }

            FileOperationsController controller = new FileOperationsController(_fileSystemState);
            controller.QuerySaveFileAs += (object sender, FileOperationEventArgs e) =>
            {
                e.Cancel = true;
            };

            FileOperationStatus status = controller.EncryptFile(_davidCopperfieldTxtPath);
            Assert.That(status, Is.EqualTo(FileOperationStatus.Canceled), "The status should indicate cancellation.");
        }

        [Test]
        public static void TestEncryptFileWhenCanceledDuringQueryPassphrase()
        {
            FileOperationsController controller = new FileOperationsController(_fileSystemState);
            controller.QueryEncryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.Cancel = true;
            };

            FileOperationStatus status = controller.EncryptFile(_davidCopperfieldTxtPath);
            Assert.That(status, Is.EqualTo(FileOperationStatus.Canceled), "The status should indicate cancellation.");
        }

        [Test]
        public static void TestSimpleDecryptFile()
        {
            FileOperationsController controller = new FileOperationsController(_fileSystemState);
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
                {
                    e.Passphrase = "a";
                };
            bool knownKeyWasAdded = false;
            controller.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
                {
                    knownKeyWasAdded = e.Key == new Passphrase("a").DerivedPassphrase;
                };
            string destinationPath = String.Empty;
            controller.Completed += (object sender, FileOperationEventArgs e) =>
                {
                    destinationPath = e.SaveFileFullName;
                };
            FileOperationStatus status = controller.DecryptFile(_helloWorldAxxPath);

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The status should indicate success.");
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
        public static void TestSimpleDecryptFileOnThreadWorker()
        {
            FileOperationsController controller = new FileOperationsController(_fileSystemState);
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.Passphrase = "a";
            };
            bool knownKeyWasAdded = false;
            controller.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
            {
                knownKeyWasAdded = e.Key == new Passphrase("a").DerivedPassphrase;
            };
            string destinationPath = String.Empty;
            FileOperationStatus status = FileOperationStatus.Unknown;
            controller.Completed += (object sender, FileOperationEventArgs e) =>
            {
                destinationPath = e.SaveFileFullName;
                status = e.Status;
            };
            using (ThreadWorker worker = new ThreadWorker(new ProgressContext()))
            {
                controller.DecryptFile(_helloWorldAxxPath, worker);
                worker.Join();
            }

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The status should indicate success.");
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
        public static void TestDecryptWithCancelDuringQueryDecryptionPassphrase()
        {
            FileOperationsController controller = new FileOperationsController(_fileSystemState);
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.Cancel = true;
            };
            FileOperationStatus status = controller.DecryptFile(_helloWorldAxxPath);

            Assert.That(status, Is.EqualTo(FileOperationStatus.Canceled), "The status should indicate cancellation.");
        }

        [Test]
        public static void TestDecryptWithCancelDuringQuerySaveAs()
        {
            IRuntimeFileInfo expectedDestinationInfo = OS.Current.FileInfo(Path.Combine(Path.GetDirectoryName(_helloWorldAxxPath), "HelloWorld-Key-a.txt"));
            using (Stream stream = expectedDestinationInfo.OpenWrite())
            {
            }

            FileOperationsController controller = new FileOperationsController(_fileSystemState);
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
                {
                    e.Passphrase = "a";
                };
            controller.QuerySaveFileAs += (object sender, FileOperationEventArgs e) =>
                {
                    e.Cancel = true;
                };
            FileOperationStatus status = controller.DecryptFile(_helloWorldAxxPath);

            Assert.That(status, Is.EqualTo(FileOperationStatus.Canceled), "The status should indicate cancellation.");
        }

        [Test]
        public static void TestDecryptWithAlternativeDestinationName()
        {
            IRuntimeFileInfo expectedDestinationInfo = OS.Current.FileInfo(Path.Combine(Path.GetDirectoryName(_helloWorldAxxPath), "HelloWorld-Key-a.txt"));
            using (Stream stream = expectedDestinationInfo.OpenWrite())
            {
            }

            FileOperationsController controller = new FileOperationsController(_fileSystemState);
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.Passphrase = "a";
            };
            controller.QuerySaveFileAs += (object sender, FileOperationEventArgs e) =>
            {
                e.SaveFileFullName = Path.Combine(Path.GetDirectoryName(e.SaveFileFullName), "Other Hello World.txt");
            };
            string destinationPath = String.Empty;
            controller.Completed += (object sender, FileOperationEventArgs e) =>
            {
                destinationPath = e.SaveFileFullName;
            };
            FileOperationStatus status = controller.DecryptFile(_helloWorldAxxPath);

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The status should indicate success.");

            IRuntimeFileInfo destinationInfo = OS.Current.FileInfo(destinationPath);
            string fileContent;
            using (Stream stream = destinationInfo.OpenRead())
            {
                fileContent = new StreamReader(stream).ReadToEnd();
            }
            Assert.That(fileContent.Contains("Hello"), "A file named 'Other Hello World.txt' should contain that text when decrypted.");
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

            FileOperationsController controller = new FileOperationsController(_fileSystemState);
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.Passphrase = "a";
            };
            FileOperationStatus status = controller.DecryptAndLaunch(_helloWorldAxxPath);

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The status should indicate success.");

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
        public static void TestSimpleDecryptAndLaunchOnThreadWorker()
        {
            FakeLauncher launcher = null;
            FakeRuntimeEnvironment environment = (FakeRuntimeEnvironment)OS.Current;
            environment.Launcher = ((string path) =>
            {
                launcher = new FakeLauncher(path);
                return launcher;
            });

            FileOperationsController controller = new FileOperationsController(_fileSystemState);
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.Passphrase = "a";
            };
            FileOperationStatus status = FileOperationStatus.Unknown;
            controller.Completed += (object sender, FileOperationEventArgs e) =>
            {
                status = e.Status;
            };

            using (ThreadWorker worker = new ThreadWorker(new ProgressContext()))
            {
                controller.DecryptAndLaunch(_helloWorldAxxPath, worker);
                worker.Join();
            }

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The status should indicate success.");

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
            FileOperationsController controller = new FileOperationsController(_fileSystemState);
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.Cancel = true;
            };
            FileOperationStatus status = controller.DecryptAndLaunch(_helloWorldAxxPath);
            Assert.That(status, Is.EqualTo(FileOperationStatus.Canceled), "The status should indicate cancellation.");
        }

        [Test]
        public static void TestDecryptWithKnownKey()
        {
            FileOperationsController controller = new FileOperationsController(_fileSystemState);
            _fileSystemState.KnownKeys.Add(new Passphrase("b").DerivedPassphrase);
            _fileSystemState.KnownKeys.Add(new Passphrase("c").DerivedPassphrase);
            _fileSystemState.KnownKeys.Add(new Passphrase("a").DerivedPassphrase);
            _fileSystemState.KnownKeys.Add(new Passphrase("e").DerivedPassphrase);
            bool passphraseWasQueried = false;
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                passphraseWasQueried = true;
            };
            string destinationPath = String.Empty;
            controller.Completed += (object sender, FileOperationEventArgs e) =>
            {
                destinationPath = e.SaveFileFullName;
            };
            bool knownKeyWasAdded = false;
            controller.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
            {
                knownKeyWasAdded = true;
            };
            FileOperationStatus status = controller.DecryptFile(_helloWorldAxxPath);

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The status should indicate success.");
            Assert.That(!knownKeyWasAdded, "An already known key was used, so the KnownKeyAdded event should not have been raised.");
            Assert.That(!passphraseWasQueried, "An already known key was used, so the there should be no need to query for a passphrase.");
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
        public static void TestDecryptFileWithRepeatedPassphraseQueries()
        {
            FileOperationsController controller = new FileOperationsController(_fileSystemState);
            int passphraseTry = 0;
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                switch (++passphraseTry)
                {
                    case 1:
                        e.Passphrase = "b";
                        break;

                    case 2:
                        e.Passphrase = "d";
                        break;

                    case 3:
                        e.Passphrase = "a";
                        break;

                    case 4:
                        e.Passphrase = "e";
                        break;
                };
            };
            string destinationPath = String.Empty;
            controller.Completed += (object sender, FileOperationEventArgs e) =>
            {
                destinationPath = e.SaveFileFullName;
            };
            bool knownKeyWasAdded = false;
            controller.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
            {
                knownKeyWasAdded = e.Key == new Passphrase("a").DerivedPassphrase;
            };
            FileOperationStatus status = controller.DecryptFile(_helloWorldAxxPath);

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The status should indicate success.");
            Assert.That(knownKeyWasAdded, "A new known key was used, so the KnownKeyAdded event should have been raised.");
            Assert.That(passphraseTry, Is.EqualTo(3), "The third key was the correct one.");
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
        public static void TestDecryptFileWithExceptionBeforeStartingDecryption()
        {
            FileOperationsController controller = new FileOperationsController(_fileSystemState);
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
                {
                    e.Passphrase = "a";
                };
            controller.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
                {
                    throw new FileNotFoundException("Just kidding, but we're faking...", e.OpenFileFullName);
                };
            string destinationPath = String.Empty;
            controller.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
                {
                    destinationPath = e.SaveFileFullName;
                };
            FileOperationStatus status = FileOperationStatus.Unknown;
            Assert.DoesNotThrow(() => { status = controller.DecryptFile(_helloWorldAxxPath); });

            Assert.That(status, Is.EqualTo(FileOperationStatus.FileDoesNotExist), "The status should indicate an exception occurred.");
            Assert.That(String.IsNullOrEmpty(destinationPath), "Since an exception occurred, the destination file should not be created.");
        }

        [Test]
        public static void TestEncryptFileThatIsAlreadyEncrypted()
        {
            FileOperationsController controller = new FileOperationsController(_fileSystemState);
            FileOperationStatus status = controller.EncryptFile("test" + OS.Current.AxCryptExtension);

            Assert.That(status, Is.EqualTo(FileOperationStatus.FileAlreadyEncrypted), "The status should indicate that it was already encrypted.");
        }

        [Test]
        public static void TestDecryptWithCancelDuringQueryDecryptionPassphraseOnThreadWorker()
        {
            FileOperationsController controller = new FileOperationsController(_fileSystemState);
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
                {
                    e.Cancel = true;
                };
            FileOperationStatus status = FileOperationStatus.Unknown;
            controller.Completed += (object sender, FileOperationEventArgs e) =>
                {
                    status = e.Status;
                };

            using (ThreadWorker worker = new ThreadWorker(new ProgressContext()))
            {
                controller.DecryptFile(_helloWorldAxxPath, worker);
                worker.Join();
            }

            Assert.That(status, Is.EqualTo(FileOperationStatus.Canceled), "The status should indicate cancellation.");
        }
    }
}