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
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestFileOperationsController
    {
        private static readonly string _rootPath = Path.GetPathRoot(Environment.CurrentDirectory);
        private static readonly string _davidCopperfieldTxtPath = _rootPath.PathCombine("Users", "AxCrypt", "David Copperfield.txt");
        private static readonly string _uncompressedAxxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Uncompressed.axx");
        private static readonly string _helloWorldAxxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HelloWorld.axx");

        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();

            FakeRuntimeFileInfo.AddFile(_davidCopperfieldTxtPath, FakeRuntimeFileInfo.TestDate4Utc, FakeRuntimeFileInfo.TestDate5Utc, FakeRuntimeFileInfo.TestDate6Utc, FakeRuntimeFileInfo.ExpandableMemoryStream(Encoding.GetEncoding(1252).GetBytes(Resources.david_copperfield)));
            FakeRuntimeFileInfo.AddFile(_uncompressedAxxPath, FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.uncompressable_zip));
            FakeRuntimeFileInfo.AddFile(_helloWorldAxxPath, FakeRuntimeFileInfo.ExpandableMemoryStream(Resources.helloworld_key_a_txt));

            Factory.Instance.Singleton<IUIThread>(() => new FakeUIThread());
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestSimpleEncryptFile()
        {
            FileOperationsController controller = new FileOperationsController();
            string destinationPath = String.Empty;
            controller.QueryEncryptionPassphrase += (object sender, FileOperationEventArgs e) =>
                {
                    e.Passphrase = "allan";
                };
            controller.Completed += (object sender, FileOperationEventArgs e) =>
            {
                destinationPath = e.SaveFileFullName;
            };

            FileOperationStatus status = controller.EncryptFile(Factory.New<IRuntimeFileInfo>(_davidCopperfieldTxtPath));
            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The status should indicate success.");

            IRuntimeFileInfo destinationInfo = Factory.New<IRuntimeFileInfo>(destinationPath);
            Assert.That(destinationInfo.Exists, "After encryption the destination file should be created.");
            SymmetricKey key = new V1Passphrase("allan").DerivedPassphrase;
            using (IAxCryptDocument document = new V1AxCryptDocument())
            {
                using (Stream stream = destinationInfo.OpenRead())
                {
                    document.Load(key, stream);
                    Assert.That(document.PassphraseIsValid, "The encrypted document should be valid and encrypted with the passphrase given.");
                }
            }
        }

        [Test]
        public static void TestSimpleEncryptFileOnThreadWorker()
        {
            FileOperationsController controller = new FileOperationsController();
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

            controller.EncryptFile(Factory.New<IRuntimeFileInfo>(_davidCopperfieldTxtPath));
            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The status should indicate success.");

            IRuntimeFileInfo destinationInfo = Factory.New<IRuntimeFileInfo>(destinationPath);
            Assert.That(destinationInfo.Exists, "After encryption the destination file should be created.");
            SymmetricKey key = new V1Passphrase("allan").DerivedPassphrase;
            using (IAxCryptDocument document = new V1AxCryptDocument())
            {
                using (Stream stream = destinationInfo.OpenRead())
                {
                    document.Load(key, stream);
                    Assert.That(document.PassphraseIsValid, "The encrypted document should be valid and encrypted with the passphrase given.");
                }
            }
        }

        [Test]
        public static void TestEncryptFileWithDefaultEncryptionKey()
        {
            Instance.KnownKeys.DefaultEncryptionKey = new V1Passphrase("default").DerivedPassphrase;
            FileOperationsController controller = new FileOperationsController();
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

            FileOperationStatus status = controller.EncryptFile(Factory.New<IRuntimeFileInfo>(_davidCopperfieldTxtPath));
            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The status should indicate success.");
            Assert.That(!queryEncryptionPassphraseWasCalled, "No query of encryption passphrase should be needed since there is a default set.");

            IRuntimeFileInfo destinationInfo = Factory.New<IRuntimeFileInfo>(destinationPath);
            Assert.That(destinationInfo.Exists, "After encryption the destination file should be created.");
            SymmetricKey key = new V1Passphrase("default").DerivedPassphrase;
            using (IAxCryptDocument document = new V1AxCryptDocument())
            {
                using (Stream stream = destinationInfo.OpenRead())
                {
                    document.Load(key, stream);
                    Assert.That(document.PassphraseIsValid, "The encrypted document should be valid and encrypted with the default passphrase given.");
                }
            }
        }

        [Test]
        public static void TestEncryptFileWhenDestinationExists()
        {
            IRuntimeFileInfo sourceInfo = Factory.New<IRuntimeFileInfo>(_davidCopperfieldTxtPath);
            IRuntimeFileInfo expectedDestinationInfo = Factory.New<IRuntimeFileInfo>(AxCryptFile.MakeAxCryptFileName(sourceInfo));
            using (Stream stream = expectedDestinationInfo.OpenWrite())
            {
            }

            FileOperationsController controller = new FileOperationsController();
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

            FileOperationStatus status = controller.EncryptFile(Factory.New<IRuntimeFileInfo>(_davidCopperfieldTxtPath));
            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The status should indicate success.");

            Assert.That(Path.GetFileName(destinationPath), Is.EqualTo("alternative-name.axx"), "The alternative name should be used, since the default existed.");
            IRuntimeFileInfo destinationInfo = Factory.New<IRuntimeFileInfo>(destinationPath);
            Assert.That(destinationInfo.Exists, "After encryption the destination file should be created.");
            SymmetricKey key = new V1Passphrase("allan").DerivedPassphrase;
            using (IAxCryptDocument document = new V1AxCryptDocument())
            {
                using (Stream stream = destinationInfo.OpenRead())
                {
                    document.Load(key, stream);
                    Assert.That(document.PassphraseIsValid, "The encrypted document should be valid and encrypted with the passphrase given.");
                }
            }
        }

        [Test]
        public static void TestEncryptFileWhenCanceledDuringQuerySaveAs()
        {
            IRuntimeFileInfo sourceInfo = Factory.New<IRuntimeFileInfo>(_davidCopperfieldTxtPath);
            IRuntimeFileInfo expectedDestinationInfo = Factory.New<IRuntimeFileInfo>(AxCryptFile.MakeAxCryptFileName(sourceInfo));
            using (Stream stream = expectedDestinationInfo.OpenWrite())
            {
            }

            FileOperationsController controller = new FileOperationsController();
            controller.QuerySaveFileAs += (object sender, FileOperationEventArgs e) =>
            {
                e.Cancel = true;
            };

            FileOperationStatus status = controller.EncryptFile(Factory.New<IRuntimeFileInfo>(_davidCopperfieldTxtPath));
            Assert.That(status, Is.EqualTo(FileOperationStatus.Canceled), "The status should indicate cancellation.");
        }

        [Test]
        public static void TestEncryptFileWhenCanceledDuringQueryPassphrase()
        {
            FileOperationsController controller = new FileOperationsController();
            controller.QueryEncryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.Cancel = true;
            };

            FileOperationStatus status = controller.EncryptFile(Factory.New<IRuntimeFileInfo>(_davidCopperfieldTxtPath));
            Assert.That(status, Is.EqualTo(FileOperationStatus.Canceled), "The status should indicate cancellation.");
        }

        [Test]
        public static void TestSimpleDecryptFile()
        {
            FileOperationsController controller = new FileOperationsController();
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
                {
                    e.Passphrase = "a";
                };
            bool knownKeyWasAdded = false;
            controller.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
                {
                    knownKeyWasAdded = e.Key == new V1Passphrase("a").DerivedPassphrase;
                };
            string destinationPath = String.Empty;
            controller.Completed += (object sender, FileOperationEventArgs e) =>
                {
                    destinationPath = e.SaveFileFullName;
                };
            FileOperationStatus status = controller.DecryptFile(Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath));

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The status should indicate success.");
            Assert.That(knownKeyWasAdded, "A new known key was used, so the KnownKeyAdded event should have been raised.");
            IRuntimeFileInfo destinationInfo = Factory.New<IRuntimeFileInfo>(destinationPath);
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
            FileOperationsController controller = new FileOperationsController();
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.Passphrase = "a";
            };
            bool knownKeyWasAdded = false;
            controller.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
            {
                knownKeyWasAdded = e.Key == new V1Passphrase("a").DerivedPassphrase;
            };
            string destinationPath = String.Empty;
            FileOperationStatus status = FileOperationStatus.Unknown;
            controller.Completed += (object sender, FileOperationEventArgs e) =>
            {
                destinationPath = e.SaveFileFullName;
                status = e.Status;
            };

            controller.DecryptFile(Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath));

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The status should indicate success.");
            Assert.That(knownKeyWasAdded, "A new known key was used, so the KnownKeyAdded event should have been raised.");
            IRuntimeFileInfo destinationInfo = Factory.New<IRuntimeFileInfo>(destinationPath);
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
            FileOperationsController controller = new FileOperationsController();
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.Cancel = true;
            };
            FileOperationStatus status = controller.DecryptFile(Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath));

            Assert.That(status, Is.EqualTo(FileOperationStatus.Canceled), "The status should indicate cancellation.");
        }

        [Test]
        public static void TestDecryptWithSkipDuringQueryDecryptionPassphrase()
        {
            IRuntimeFileInfo expectedDestinationInfo = Factory.New<IRuntimeFileInfo>(Path.Combine(Path.GetDirectoryName(_helloWorldAxxPath), "HelloWorld-Key-a.txt"));
            using (Stream stream = expectedDestinationInfo.OpenWrite())
            {
            }

            FileOperationsController controller = new FileOperationsController();
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.Skip = true;
            };
            bool saveAs = false;
            controller.QuerySaveFileAs += (sender, e) => saveAs = true;
            FileOperationStatus status = controller.DecryptFile(Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath));

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The status should indicate success.");
            Assert.That(saveAs, Is.False, "No Save As should happen, since skip was indicated.");
        }

        [Test]
        public static void TestDecryptWithCancelDuringQuerySaveAs()
        {
            IRuntimeFileInfo expectedDestinationInfo = Factory.New<IRuntimeFileInfo>(Path.Combine(Path.GetDirectoryName(_helloWorldAxxPath), "HelloWorld-Key-a.txt"));
            using (Stream stream = expectedDestinationInfo.OpenWrite())
            {
            }

            FileOperationsController controller = new FileOperationsController();
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
                {
                    e.Passphrase = "a";
                };
            controller.QuerySaveFileAs += (object sender, FileOperationEventArgs e) =>
                {
                    e.Cancel = true;
                };
            FileOperationStatus status = controller.DecryptFile(Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath));

            Assert.That(status, Is.EqualTo(FileOperationStatus.Canceled), "The status should indicate cancellation.");
        }

        [Test]
        public static void TestDecryptWithAlternativeDestinationName()
        {
            IRuntimeFileInfo expectedDestinationInfo = Factory.New<IRuntimeFileInfo>(Path.Combine(Path.GetDirectoryName(_helloWorldAxxPath), "HelloWorld-Key-a.txt"));
            using (Stream stream = expectedDestinationInfo.OpenWrite())
            {
            }

            FileOperationsController controller = new FileOperationsController();
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
            FileOperationStatus status = controller.DecryptFile(Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath));

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The status should indicate success.");

            IRuntimeFileInfo destinationInfo = Factory.New<IRuntimeFileInfo>(destinationPath);
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

            FileOperationsController controller = new FileOperationsController();
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.Passphrase = "a";
            };
            FileOperationStatus status = controller.DecryptAndLaunch(Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath));

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The status should indicate success.");

            Assert.That(launcher, Is.Not.Null, "There should be a call to launch.");
            Assert.That(Path.GetFileName(launcher.Path), Is.EqualTo("HelloWorld-Key-a.txt"), "The file should be decrypted and the name should be the original from the encrypted headers.");

            IRuntimeFileInfo destinationInfo = Factory.New<IRuntimeFileInfo>(launcher.Path);
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

            FileOperationsController controller = new FileOperationsController();
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.Passphrase = "a";
            };
            FileOperationStatus status = FileOperationStatus.Unknown;
            controller.Completed += (object sender, FileOperationEventArgs e) =>
            {
                status = e.Status;
            };

            controller.DecryptAndLaunch(Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath));

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The status should indicate success.");

            Assert.That(launcher, Is.Not.Null, "There should be a call to launch.");
            Assert.That(Path.GetFileName(launcher.Path), Is.EqualTo("HelloWorld-Key-a.txt"), "The file should be decrypted and the name should be the original from the encrypted headers.");

            IRuntimeFileInfo destinationInfo = Factory.New<IRuntimeFileInfo>(launcher.Path);
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
            FileOperationsController controller = new FileOperationsController();
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.Cancel = true;
            };
            FileOperationStatus status = controller.DecryptAndLaunch(Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath));
            Assert.That(status, Is.EqualTo(FileOperationStatus.Canceled), "The status should indicate cancellation.");
        }

        [Test]
        public static void TestDecryptWithKnownKey()
        {
            FileOperationsController controller = new FileOperationsController();
            Instance.KnownKeys.Add(new V1Passphrase("b").DerivedPassphrase);
            Instance.KnownKeys.Add(new V1Passphrase("c").DerivedPassphrase);
            Instance.KnownKeys.Add(new V1Passphrase("a").DerivedPassphrase);
            Instance.KnownKeys.Add(new V1Passphrase("e").DerivedPassphrase);
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
            FileOperationStatus status = controller.DecryptFile(Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath));

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The status should indicate success.");
            Assert.That(!knownKeyWasAdded, "An already known key was used, so the KnownKeyAdded event should not have been raised.");
            Assert.That(!passphraseWasQueried, "An already known key was used, so the there should be no need to query for a passphrase.");
            IRuntimeFileInfo destinationInfo = Factory.New<IRuntimeFileInfo>(destinationPath);
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
            FileOperationsController controller = new FileOperationsController();
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
                knownKeyWasAdded = e.Key == new V1Passphrase("a").DerivedPassphrase;
            };
            FileOperationStatus status = controller.DecryptFile(Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath));

            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The status should indicate success.");
            Assert.That(knownKeyWasAdded, "A new known key was used, so the KnownKeyAdded event should have been raised.");
            Assert.That(passphraseTry, Is.EqualTo(3), "The third key was the correct one.");
            IRuntimeFileInfo destinationInfo = Factory.New<IRuntimeFileInfo>(destinationPath);
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
            FileOperationsController controller = new FileOperationsController();
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
            Assert.DoesNotThrow(() => { status = controller.DecryptFile(Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath)); });

            Assert.That(status, Is.EqualTo(FileOperationStatus.FileDoesNotExist), "The status should indicate an exception occurred.");
            Assert.That(String.IsNullOrEmpty(destinationPath), "Since an exception occurred, the destination file should not be created.");
        }

        [Test]
        public static void TestEncryptFileThatIsAlreadyEncrypted()
        {
            FileOperationsController controller = new FileOperationsController();
            FileOperationStatus status = controller.EncryptFile(Factory.New<IRuntimeFileInfo>("test" + OS.Current.AxCryptExtension));

            Assert.That(status, Is.EqualTo(FileOperationStatus.FileAlreadyEncrypted), "The status should indicate that it was already encrypted.");
        }

        [Test]
        public static void TestDecryptWithCancelDuringQueryDecryptionPassphraseOnThreadWorker()
        {
            FileOperationsController controller = new FileOperationsController();
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
                {
                    e.Cancel = true;
                };
            FileOperationStatus status = FileOperationStatus.Unknown;
            controller.Completed += (object sender, FileOperationEventArgs e) =>
                {
                    status = e.Status;
                };

            controller.DecryptFile(Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath));

            Assert.That(status, Is.EqualTo(FileOperationStatus.Canceled), "The status should indicate cancellation.");
        }

        [Test]
        public static void TestSimpleWipe()
        {
            FileOperationsController controller = new FileOperationsController();
            controller.WipeQueryConfirmation += (object sender, FileOperationEventArgs e) =>
            {
                e.Cancel = false;
                e.Skip = false;
                e.ConfirmAll = false;
            };
            FileOperationStatus status = controller.WipeFile(Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath));
            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The wipe should indicate success.");

            IRuntimeFileInfo fileInfo = Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath);
            Assert.That(!fileInfo.Exists, "The file should not exist after wiping.");
        }

        [Test]
        public static void TestSimpleWipeOnThreadWorker()
        {
            FileOperationsController controller = new FileOperationsController();
            controller.WipeQueryConfirmation += (object sender, FileOperationEventArgs e) =>
            {
                e.Cancel = false;
                e.Skip = false;
                e.ConfirmAll = false;
            };

            string destinationPath = String.Empty;
            FileOperationStatus status = FileOperationStatus.Unknown;
            controller.Completed += (object sender, FileOperationEventArgs e) =>
            {
                destinationPath = e.SaveFileFullName;
                status = e.Status;
            };

            controller.WipeFile(Factory.New<IRuntimeFileInfo>(_davidCopperfieldTxtPath));
            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The status should indicate success.");

            IRuntimeFileInfo destinationInfo = Factory.New<IRuntimeFileInfo>(destinationPath);
            Assert.That(!destinationInfo.Exists, "After wiping the destination file should not exist.");
        }

        [Test]
        public static void TestWipeWithCancel()
        {
            FileOperationsController controller = new FileOperationsController();
            controller.WipeQueryConfirmation += (object sender, FileOperationEventArgs e) =>
            {
                e.Cancel = true;
            };
            FileOperationStatus status = controller.WipeFile(Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath));
            Assert.That(status, Is.EqualTo(FileOperationStatus.Canceled), "The wipe should indicate cancellation.");

            IRuntimeFileInfo fileInfo = Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath);
            Assert.That(fileInfo.Exists, "The file should still exist after wiping that was canceled during confirmation.");
        }

        [Test]
        public static void TestWipeWithSkip()
        {
            FileOperationsController controller = new FileOperationsController();
            controller.WipeQueryConfirmation += (object sender, FileOperationEventArgs e) =>
            {
                e.Skip = true;
            };
            FileOperationStatus status = controller.WipeFile(Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath));
            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The wipe should indicate success even when skipping.");

            IRuntimeFileInfo fileInfo = Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath);
            Assert.That(fileInfo.Exists, "The file should still exist after wiping that was skipped during confirmation.");
        }

        [Test]
        public static void TestWipeWithConfirmAll()
        {
            ProgressContext progress = new ProgressContext();
            FileOperationsController controller = new FileOperationsController(progress);
            int confirmationCount = 0;
            controller.WipeQueryConfirmation += (object sender, FileOperationEventArgs e) =>
            {
                if (confirmationCount++ > 0)
                {
                    throw new InvalidOperationException("The event should not be raised a second time.");
                }
                e.ConfirmAll = true;
            };
            progress.NotifyLevelStart();
            FileOperationStatus status = controller.WipeFile(Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath));
            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The wipe should indicate success.");

            IRuntimeFileInfo fileInfo = Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath);
            Assert.That(!fileInfo.Exists, "The file should not exist after wiping.");

            Assert.DoesNotThrow(() => { status = controller.WipeFile(Factory.New<IRuntimeFileInfo>(_davidCopperfieldTxtPath)); });
            Assert.That(status, Is.EqualTo(FileOperationStatus.Success), "The wipe should indicate success.");
            progress.NotifyLevelFinished();

            fileInfo = Factory.New<IRuntimeFileInfo>(_davidCopperfieldTxtPath);
            Assert.That(!fileInfo.Exists, "The file should not exist after wiping.");
        }

        [Test]
        public static void TestVerifyEncrypted()
        {
            FileOperationsController controller = new FileOperationsController();
            bool passphraseWasQueried = false;
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                passphraseWasQueried = true;
                e.Cancel = true;
            };
            bool knownKeyWasAdded = false;
            controller.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
            {
                knownKeyWasAdded = true;
            };

            FileOperationStatus status = controller.VerifyEncrypted(Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath));
            Assert.That(status, Is.EqualTo(FileOperationStatus.Canceled));
            Assert.That(knownKeyWasAdded, Is.False);
            Assert.That(passphraseWasQueried, Is.True);

            controller = new FileOperationsController();
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.Passphrase = "a";
            };
            controller.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
            {
                knownKeyWasAdded = true;
            };

            Instance.KnownKeys.Add(new V1Passphrase("b").DerivedPassphrase);
            Instance.KnownKeys.Add(new V1Passphrase("c").DerivedPassphrase);

            status = controller.VerifyEncrypted(Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath));
            Assert.That(status, Is.EqualTo(FileOperationStatus.Success));
            Assert.That(knownKeyWasAdded, Is.True, "A known key should have been added.");
        }
    }
}