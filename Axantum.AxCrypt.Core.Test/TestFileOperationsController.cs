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
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestFileOperationsController
    {
        private static readonly string _rootPath = Path.GetPathRoot(Environment.CurrentDirectory);
        private static readonly string _davidCopperfieldTxtPath = _rootPath.PathCombine("Users", "AxCrypt", "David Copperfield.txt");
        private static readonly string _uncompressedAxxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Uncompressed.axx");
        private static readonly string _helloWorldAxxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HelloWorld.axx");

        private CryptoImplementation _cryptoImplementation;

        public TestFileOperationsController(CryptoImplementation cryptoImplementation)
        {
            _cryptoImplementation = cryptoImplementation;
        }

        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup();
            SetupAssembly.AssemblySetupCrypto(_cryptoImplementation);

            FakeDataStore.AddFile(_davidCopperfieldTxtPath, FakeDataStore.TestDate4Utc, FakeDataStore.TestDate5Utc, FakeDataStore.TestDate6Utc, FakeDataStore.ExpandableMemoryStream(Encoding.GetEncoding(1252).GetBytes(Resources.david_copperfield)));
            FakeDataStore.AddFile(_uncompressedAxxPath, FakeDataStore.ExpandableMemoryStream(Resources.uncompressable_zip));
            FakeDataStore.AddFile(_helloWorldAxxPath, FakeDataStore.ExpandableMemoryStream(Resources.helloworld_key_a_txt));

            TypeMap.Register.Singleton<IUIThread>(() => new FakeUIThread());
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public void TestSimpleEncryptFile()
        {
            FileOperationsController controller = new FileOperationsController();
            string destinationPath = String.Empty;
            controller.QueryEncryptionPassphrase += (object sender, FileOperationEventArgs e) =>
                {
                    e.LogOnIdentity = new LogOnIdentity("allan");
                };
            controller.Completed += (object sender, FileOperationEventArgs e) =>
            {
                destinationPath = e.SaveFileFullName;
            };

            FileOperationContext status = controller.EncryptFile(New<IDataStore>(_davidCopperfieldTxtPath));
            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Success), "The status should indicate success.");

            IDataStore destinationInfo = New<IDataStore>(destinationPath);
            Assert.That(destinationInfo.IsAvailable, "After encryption the destination file should be created.");
            using (V2AxCryptDocument document = new V2AxCryptDocument())
            {
                using (Stream stream = destinationInfo.OpenRead())
                {
                    document.Load(new Passphrase("allan"), V2Aes256CryptoFactory.CryptoId, stream);
                    Assert.That(document.PassphraseIsValid, "The encrypted document should be valid and encrypted with the passphrase given.");
                }
            }
        }

        [Test]
        public void TestSimpleEncryptFileOnThreadWorker()
        {
            FileOperationsController controller = new FileOperationsController();
            controller.QueryEncryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.LogOnIdentity = new LogOnIdentity("allan");
            };
            string destinationPath = String.Empty;
            FileOperationContext status = new FileOperationContext(String.Empty, ErrorStatus.Unknown);
            controller.Completed += (object sender, FileOperationEventArgs e) =>
            {
                destinationPath = e.SaveFileFullName;
                status = e.Status;
            };

            controller.EncryptFile(New<IDataStore>(_davidCopperfieldTxtPath));
            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Success), "The status should indicate success.");

            IDataStore destinationInfo = New<IDataStore>(destinationPath);
            Assert.That(destinationInfo.IsAvailable, "After encryption the destination file should be created.");
            using (V2AxCryptDocument document = new V2AxCryptDocument())
            {
                using (Stream stream = destinationInfo.OpenRead())
                {
                    document.Load(new Passphrase("allan"), V2Aes256CryptoFactory.CryptoId, stream);
                    Assert.That(document.PassphraseIsValid, "The encrypted document should be valid and encrypted with the passphrase given.");
                }
            }
        }

        [Test]
        public void TestEncryptFileWithDefaultEncryptionKey()
        {
            TypeMap.Register.Singleton<ICryptoPolicy>(() => new LegacyCryptoPolicy());
            Resolve.KnownIdentities.DefaultEncryptionIdentity = new LogOnIdentity("default");
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

            FileOperationContext status = controller.EncryptFile(New<IDataStore>(_davidCopperfieldTxtPath));
            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Success), "The status should indicate success.");
            Assert.That(!queryEncryptionPassphraseWasCalled, "No query of encryption passphrase should be needed since there is a default set.");

            IDataStore destinationInfo = New<IDataStore>(destinationPath);
            Assert.That(destinationInfo.IsAvailable, "After encryption the destination file should be created.");
            using (V1AxCryptDocument document = new V1AxCryptDocument())
            {
                using (Stream stream = destinationInfo.OpenRead())
                {
                    document.Load(new Passphrase("default"), V1Aes128CryptoFactory.CryptoId, stream);
                    Assert.That(document.PassphraseIsValid, "The encrypted document should be valid and encrypted with the default passphrase given.");
                }
            }
        }

        [Test]
        public void TestEncryptFileWhenDestinationExists()
        {
            IDataStore sourceInfo = New<IDataStore>(_davidCopperfieldTxtPath);
            IDataStore expectedDestinationInfo = New<IDataStore>(AxCryptFile.MakeAxCryptFileName(sourceInfo));
            using (Stream stream = expectedDestinationInfo.OpenWrite())
            {
            }

            FileOperationsController controller = new FileOperationsController();
            string destinationPath = String.Empty;
            LogOnIdentity logOnIdentity = null;
            controller.QueryEncryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.LogOnIdentity = new LogOnIdentity("allan");
            };
            controller.QuerySaveFileAs += (object sender, FileOperationEventArgs e) =>
            {
                e.SaveFileFullName = Path.Combine(Path.GetDirectoryName(e.SaveFileFullName), "alternative-name.axx");
            };
            Guid cryptoId = Guid.Empty;
            controller.Completed += (object sender, FileOperationEventArgs e) =>
            {
                destinationPath = e.SaveFileFullName;
                logOnIdentity = e.LogOnIdentity;
                cryptoId = e.CryptoId;
            };

            FileOperationContext status = controller.EncryptFile(New<IDataStore>(_davidCopperfieldTxtPath));
            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Success), "The status should indicate success.");

            Assert.That(Path.GetFileName(destinationPath), Is.EqualTo("alternative-name.axx"), "The alternative name should be used, since the default existed.");
            IDataStore destinationInfo = New<IDataStore>(destinationPath);
            Assert.That(destinationInfo.IsAvailable, "After encryption the destination file should be created.");

            EncryptionParameters encryptionParameters = new EncryptionParameters(cryptoId, logOnIdentity.Passphrase);
            encryptionParameters.Add(logOnIdentity.PublicKeys);

            Headers headers = new Headers();
            AxCryptReaderBase reader = headers.CreateReader(new LookAheadStream(destinationInfo.OpenRead()));
            using (IAxCryptDocument document = AxCryptReaderBase.Document(reader))
            {
                document.Load(logOnIdentity.Passphrase, cryptoId, headers);
                Assert.That(document.PassphraseIsValid, "The encrypted document should be valid and encrypted with the passphrase given.");
            }
        }

        [Test]
        public void TestEncryptFileWhenCanceledDuringQuerySaveAs()
        {
            IDataStore sourceInfo = New<IDataStore>(_davidCopperfieldTxtPath);
            IDataStore expectedDestinationInfo = New<IDataStore>(AxCryptFile.MakeAxCryptFileName(sourceInfo));
            using (Stream stream = expectedDestinationInfo.OpenWrite())
            {
            }

            FileOperationsController controller = new FileOperationsController();
            controller.QuerySaveFileAs += (object sender, FileOperationEventArgs e) =>
            {
                e.Cancel = true;
            };

            FileOperationContext status = controller.EncryptFile(New<IDataStore>(_davidCopperfieldTxtPath));
            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Canceled), "The status should indicate cancellation.");
        }

        [Test]
        public void TestEncryptFileWhenCanceledDuringQueryPassphrase()
        {
            FileOperationsController controller = new FileOperationsController();
            controller.QueryEncryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.Cancel = true;
            };

            FileOperationContext status = controller.EncryptFile(New<IDataStore>(_davidCopperfieldTxtPath));
            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Canceled), "The status should indicate cancellation.");
        }

        [Test]
        public void TestSimpleDecryptFile()
        {
            FileOperationsController controller = new FileOperationsController();
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
                {
                    e.LogOnIdentity = new LogOnIdentity("a");
                };
            bool knownKeyWasAdded = false;
            controller.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
                {
                    knownKeyWasAdded = e.LogOnIdentity.Equals(new LogOnIdentity("a"));
                };
            string destinationPath = String.Empty;
            controller.Completed += (object sender, FileOperationEventArgs e) =>
                {
                    destinationPath = e.SaveFileFullName;
                };
            FileOperationContext status = controller.DecryptFile(New<IDataStore>(_helloWorldAxxPath));

            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Success), "The status should indicate success.");
            Assert.That(knownKeyWasAdded, "A new known key was used, so the KnownKeyAdded event should have been raised.");
            IDataStore destinationInfo = New<IDataStore>(destinationPath);
            Assert.That(destinationInfo.IsAvailable, "After decryption the destination file should be created.");

            string fileContent;
            using (Stream stream = destinationInfo.OpenRead())
            {
                fileContent = new StreamReader(stream).ReadToEnd();
            }
            Assert.That(fileContent.Contains("Hello"), "A file named Hello World should contain that text when decrypted.");
        }

        [Test]
        public void TestSimpleDecryptFileOnThreadWorker()
        {
            FileOperationsController controller = new FileOperationsController();
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.LogOnIdentity = new LogOnIdentity("a");
            };
            bool knownKeyWasAdded = false;
            controller.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
            {
                knownKeyWasAdded = e.LogOnIdentity.Equals(new LogOnIdentity("a"));
            };
            string destinationPath = String.Empty;
            FileOperationContext status = new FileOperationContext(String.Empty, ErrorStatus.Unknown);
            controller.Completed += (object sender, FileOperationEventArgs e) =>
            {
                destinationPath = e.SaveFileFullName;
                status = e.Status;
            };

            controller.DecryptFile(New<IDataStore>(_helloWorldAxxPath));

            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Success), "The status should indicate success.");
            Assert.That(knownKeyWasAdded, "A new known key was used, so the KnownKeyAdded event should have been raised.");
            IDataStore destinationInfo = New<IDataStore>(destinationPath);
            Assert.That(destinationInfo.IsAvailable, "After decryption the destination file should be created.");

            string fileContent;
            using (Stream stream = destinationInfo.OpenRead())
            {
                fileContent = new StreamReader(stream).ReadToEnd();
            }
            Assert.That(fileContent.Contains("Hello"), "A file named Hello World should contain that text when decrypted.");
        }

        [Test]
        public void TestDecryptWithCancelDuringQueryDecryptionPassphrase()
        {
            FileOperationsController controller = new FileOperationsController();
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.Cancel = true;
            };
            FileOperationContext status = controller.DecryptFile(New<IDataStore>(_helloWorldAxxPath));

            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Canceled), "The status should indicate cancellation.");
        }

        [Test]
        public void TestDecryptWithSkipDuringQueryDecryptionPassphrase()
        {
            IDataStore expectedDestinationInfo = New<IDataStore>(Path.Combine(Path.GetDirectoryName(_helloWorldAxxPath), "HelloWorld-Key-a.txt"));
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
            FileOperationContext status = controller.DecryptFile(New<IDataStore>(_helloWorldAxxPath));

            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Success), "The status should indicate success.");
            Assert.That(saveAs, Is.False, "No Save As should happen, since skip was indicated.");
        }

        [Test]
        public void TestDecryptWithCancelDuringQuerySaveAs()
        {
            IDataStore expectedDestinationInfo = New<IDataStore>(Path.Combine(Path.GetDirectoryName(_helloWorldAxxPath), "HelloWorld-Key-a.txt"));
            using (Stream stream = expectedDestinationInfo.OpenWrite())
            {
            }

            FileOperationsController controller = new FileOperationsController();
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
                {
                    e.LogOnIdentity = new LogOnIdentity("a");
                };
            controller.QuerySaveFileAs += (object sender, FileOperationEventArgs e) =>
                {
                    e.Cancel = true;
                };
            FileOperationContext status = controller.DecryptFile(New<IDataStore>(_helloWorldAxxPath));

            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Canceled), "The status should indicate cancellation.");
        }

        [Test]
        public void TestDecryptWithAlternativeDestinationName()
        {
            IDataStore expectedDestinationInfo = New<IDataStore>(Path.Combine(Path.GetDirectoryName(_helloWorldAxxPath), "HelloWorld-Key-a.txt"));
            using (Stream stream = expectedDestinationInfo.OpenWrite())
            {
            }

            FileOperationsController controller = new FileOperationsController();
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.LogOnIdentity = new LogOnIdentity("a");
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
            FileOperationContext status = controller.DecryptFile(New<IDataStore>(_helloWorldAxxPath));

            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Success), "The status should indicate success.");

            IDataStore destinationInfo = New<IDataStore>(destinationPath);
            string fileContent;
            using (Stream stream = destinationInfo.OpenRead())
            {
                fileContent = new StreamReader(stream).ReadToEnd();
            }
            Assert.That(fileContent.Contains("Hello"), "A file named 'Other Hello World.txt' should contain that text when decrypted.");
        }

        [Test]
        public void TestSimpleDecryptAndLaunch()
        {
            FakeLauncher launcher = new FakeLauncher();
            bool called = false;
            TypeMap.Register.New<ILauncher>(() => { called = true; return launcher; });

            FileOperationsController controller = new FileOperationsController();
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.LogOnIdentity = new LogOnIdentity("a");
            };
            FileOperationContext status = controller.DecryptAndLaunch(New<IDataStore>(_helloWorldAxxPath));

            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Success), "The status should indicate success.");

            Assert.That(called, Is.True, "There should be a call to launch.");
            Assert.That(Path.GetFileName(launcher.Path), Is.EqualTo("HelloWorld-Key-a.txt"), "The file should be decrypted and the name should be the original from the encrypted headers.");

            IDataStore destinationInfo = New<IDataStore>(launcher.Path);
            Assert.That(destinationInfo.IsAvailable, "After decryption the destination file should be created.");

            string fileContent;
            using (Stream stream = destinationInfo.OpenRead())
            {
                fileContent = new StreamReader(stream).ReadToEnd();
            }

            Assert.That(fileContent.Contains("Hello"), "A file named Hello World should contain that text when decrypted.");
        }

        [Test]
        public void TestSimpleDecryptAndLaunchOnThreadWorker()
        {
            FakeLauncher launcher = new FakeLauncher();
            bool called = false;
            TypeMap.Register.New<ILauncher>(() => { called = true; return launcher; });

            FileOperationsController controller = new FileOperationsController();
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.LogOnIdentity = new LogOnIdentity("a");
            };
            FileOperationContext status = new FileOperationContext(String.Empty, ErrorStatus.Unknown);
            controller.Completed += (object sender, FileOperationEventArgs e) =>
            {
                status = e.Status;
            };

            controller.DecryptAndLaunch(New<IDataStore>(_helloWorldAxxPath));

            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Success), "The status should indicate success.");

            Assert.That(called, Is.True, "There should be a call to launch.");
            Assert.That(Path.GetFileName(launcher.Path), Is.EqualTo("HelloWorld-Key-a.txt"), "The file should be decrypted and the name should be the original from the encrypted headers.");

            IDataStore destinationInfo = New<IDataStore>(launcher.Path);
            Assert.That(destinationInfo.IsAvailable, "After decryption the destination file should be created.");

            string fileContent;
            using (Stream stream = destinationInfo.OpenRead())
            {
                fileContent = new StreamReader(stream).ReadToEnd();
            }

            Assert.That(fileContent.Contains("Hello"), "A file named Hello World should contain that text when decrypted.");
        }

        [Test]
        public void TestCanceledDecryptAndLaunch()
        {
            FileOperationsController controller = new FileOperationsController();
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.Cancel = true;
            };
            FileOperationContext status = controller.DecryptAndLaunch(New<IDataStore>(_helloWorldAxxPath));
            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Canceled), "The status should indicate cancellation.");
        }

        [Test]
        public void TestDecryptWithKnownKey()
        {
            FileOperationsController controller = new FileOperationsController();
            Resolve.KnownIdentities.Add(new LogOnIdentity("b"));
            Resolve.KnownIdentities.Add(new LogOnIdentity("c"));
            Resolve.KnownIdentities.Add(new LogOnIdentity("a"));
            Resolve.KnownIdentities.Add(new LogOnIdentity("e"));
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
            FileOperationContext status = controller.DecryptFile(New<IDataStore>(_helloWorldAxxPath));

            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Success), "The status should indicate success.");
            Assert.That(!knownKeyWasAdded, "An already known key was used, so the KnownKeyAdded event should not have been raised.");
            Assert.That(!passphraseWasQueried, "An already known key was used, so the there should be no need to query for a passphrase.");
            IDataStore destinationInfo = New<IDataStore>(destinationPath);
            Assert.That(destinationInfo.IsAvailable, "After decryption the destination file should be created.");

            string fileContent;
            using (Stream stream = destinationInfo.OpenRead())
            {
                fileContent = new StreamReader(stream).ReadToEnd();
            }
            Assert.That(fileContent.Contains("Hello"), "A file named Hello World should contain that text when decrypted.");
        }

        [Test]
        public void TestDecryptFileWithRepeatedPassphraseQueries()
        {
            FileOperationsController controller = new FileOperationsController();
            int passphraseTry = 0;
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                switch (++passphraseTry)
                {
                    case 1:
                        e.LogOnIdentity = new LogOnIdentity("b");
                        break;

                    case 2:
                        e.LogOnIdentity = new LogOnIdentity("d");
                        break;

                    case 3:
                        e.LogOnIdentity = new LogOnIdentity("a");
                        break;

                    case 4:
                        e.LogOnIdentity = new LogOnIdentity("e");
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
                knownKeyWasAdded = e.LogOnIdentity.Equals(new LogOnIdentity("a"));
            };
            FileOperationContext status = controller.DecryptFile(New<IDataStore>(_helloWorldAxxPath));

            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Success), "The status should indicate success.");
            Assert.That(knownKeyWasAdded, "A new known key was used, so the KnownKeyAdded event should have been raised.");
            Assert.That(passphraseTry, Is.EqualTo(3), "The third key was the correct one.");
            IDataStore destinationInfo = New<IDataStore>(destinationPath);
            Assert.That(destinationInfo.IsAvailable, "After decryption the destination file should be created.");

            string fileContent;
            using (Stream stream = destinationInfo.OpenRead())
            {
                fileContent = new StreamReader(stream).ReadToEnd();
            }
            Assert.That(fileContent.Contains("Hello"), "A file named Hello World should contain that text when decrypted.");
        }

        [Test]
        public void TestDecryptFileWithExceptionBeforeStartingDecryption()
        {
            FileOperationsController controller = new FileOperationsController();
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
                {
                    e.LogOnIdentity = new LogOnIdentity("a");
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
            FileOperationContext status = new FileOperationContext(String.Empty, ErrorStatus.Unknown);
            Assert.DoesNotThrow(() => { status = controller.DecryptFile(New<IDataStore>(_helloWorldAxxPath)); });

            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.FileDoesNotExist), "The status should indicate an exception occurred.");
            Assert.That(String.IsNullOrEmpty(destinationPath), "Since an exception occurred, the destination file should not be created.");
        }

        [Test]
        public void TestEncryptFileThatIsAlreadyEncrypted()
        {
            FileOperationsController controller = new FileOperationsController();
            FileOperationContext status = controller.EncryptFile(New<IDataStore>("test" + OS.Current.AxCryptExtension));

            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.FileAlreadyEncrypted), "The status should indicate that it was already encrypted.");
        }

        [Test]
        public void TestDecryptWithCancelDuringQueryDecryptionPassphraseOnThreadWorker()
        {
            FileOperationsController controller = new FileOperationsController();
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
                {
                    e.Cancel = true;
                };
            FileOperationContext status = new FileOperationContext(String.Empty, ErrorStatus.Unknown);
            controller.Completed += (object sender, FileOperationEventArgs e) =>
                {
                    status = e.Status;
                };

            controller.DecryptFile(New<IDataStore>(_helloWorldAxxPath));

            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Canceled), "The status should indicate cancellation.");
        }

        [Test]
        public void TestSimpleWipe()
        {
            FileOperationsController controller = new FileOperationsController();
            controller.WipeQueryConfirmation += (object sender, FileOperationEventArgs e) =>
            {
                e.Cancel = false;
                e.Skip = false;
                e.ConfirmAll = false;
            };
            FileOperationContext status = controller.WipeFile(New<IDataStore>(_helloWorldAxxPath));
            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Success), "The wipe should indicate success.");

            IDataStore fileInfo = New<IDataStore>(_helloWorldAxxPath);
            Assert.That(!fileInfo.IsAvailable, "The file should not exist after wiping.");
        }

        [Test]
        public void TestSimpleWipeOnThreadWorker()
        {
            FileOperationsController controller = new FileOperationsController();
            controller.WipeQueryConfirmation += (object sender, FileOperationEventArgs e) =>
            {
                e.Cancel = false;
                e.Skip = false;
                e.ConfirmAll = false;
            };

            string destinationPath = String.Empty;
            FileOperationContext status = new FileOperationContext(String.Empty, ErrorStatus.Unknown);
            controller.Completed += (object sender, FileOperationEventArgs e) =>
            {
                destinationPath = e.SaveFileFullName;
                status = e.Status;
            };

            controller.WipeFile(New<IDataStore>(_davidCopperfieldTxtPath));
            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Success), "The status should indicate success.");

            IDataStore destinationInfo = New<IDataStore>(destinationPath);
            Assert.That(!destinationInfo.IsAvailable, "After wiping the destination file should not exist.");
        }

        [Test]
        public void TestWipeWithCancel()
        {
            FileOperationsController controller = new FileOperationsController();
            controller.WipeQueryConfirmation += (object sender, FileOperationEventArgs e) =>
            {
                e.Cancel = true;
            };
            FileOperationContext status = controller.WipeFile(New<IDataStore>(_helloWorldAxxPath));
            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Canceled), "The wipe should indicate cancellation.");

            IDataStore fileInfo = New<IDataStore>(_helloWorldAxxPath);
            Assert.That(fileInfo.IsAvailable, "The file should still exist after wiping that was canceled during confirmation.");
        }

        [Test]
        public void TestWipeWithSkip()
        {
            FileOperationsController controller = new FileOperationsController();
            controller.WipeQueryConfirmation += (object sender, FileOperationEventArgs e) =>
            {
                e.Skip = true;
            };
            FileOperationContext status = controller.WipeFile(New<IDataStore>(_helloWorldAxxPath));
            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Success), "The wipe should indicate success even when skipping.");

            IDataStore fileInfo = New<IDataStore>(_helloWorldAxxPath);
            Assert.That(fileInfo.IsAvailable, "The file should still exist after wiping that was skipped during confirmation.");
        }

        [Test]
        public void TestWipeWithConfirmAll()
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
            FileOperationContext status = controller.WipeFile(New<IDataStore>(_helloWorldAxxPath));
            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Success), "The wipe should indicate success.");

            IDataStore fileInfo = New<IDataStore>(_helloWorldAxxPath);
            Assert.That(!fileInfo.IsAvailable, "The file should not exist after wiping.");

            Assert.DoesNotThrow(() => { status = controller.WipeFile(New<IDataStore>(_davidCopperfieldTxtPath)); });
            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Success), "The wipe should indicate success.");
            progress.NotifyLevelFinished();

            fileInfo = New<IDataStore>(_davidCopperfieldTxtPath);
            Assert.That(!fileInfo.IsAvailable, "The file should not exist after wiping.");
        }

        [Test]
        public void TestVerifyEncrypted()
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

            FileOperationContext status = controller.VerifyEncrypted(New<IDataStore>(_helloWorldAxxPath));
            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Canceled));
            Assert.That(knownKeyWasAdded, Is.False);
            Assert.That(passphraseWasQueried, Is.True);

            controller = new FileOperationsController();
            controller.QueryDecryptionPassphrase += (object sender, FileOperationEventArgs e) =>
            {
                e.LogOnIdentity = new LogOnIdentity("a");
            };
            controller.KnownKeyAdded += (object sender, FileOperationEventArgs e) =>
            {
                knownKeyWasAdded = true;
            };

            Resolve.KnownIdentities.Add(new LogOnIdentity("b"));
            Resolve.KnownIdentities.Add(new LogOnIdentity("c"));

            status = controller.VerifyEncrypted(New<IDataStore>(_helloWorldAxxPath));
            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Success));
            Assert.That(knownKeyWasAdded, Is.True, "A known key should have been added.");
        }
    }
}