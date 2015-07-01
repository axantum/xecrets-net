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
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Core.UI;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Text;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestAxCryptFile
    {
        private static readonly string _rootPath = Path.GetPathRoot(Environment.CurrentDirectory);
        private static readonly string _testTextPath = Path.Combine(_rootPath, "test.txt");
        private static readonly string _davidCopperfieldTxtPath = _rootPath.PathCombine("Users", "AxCrypt", "David Copperfield.txt");
        private static readonly string _uncompressedAxxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Uncompressed.axx");
        private static readonly string _helloWorldAxxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HelloWorld.axx");

        private CryptoImplementation _cryptoImplementation;

        public TestAxCryptFile(CryptoImplementation cryptoImplementation)
        {
            _cryptoImplementation = cryptoImplementation;
        }

        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup();
            SetupAssembly.AssemblySetupCrypto(_cryptoImplementation);

            FakeDataStore.AddFile(_testTextPath, FakeDataStore.TestDate1Utc, FakeDataStore.TestDate2Utc, FakeDataStore.TestDate3Utc, FakeDataStore.ExpandableMemoryStream(Encoding.UTF8.GetBytes("This is a short file")));
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
            IDataStore sourceFileInfo = TypeMap.Resolve.New<IDataStore>(_testTextPath);
            IDataStore destinationFileInfo = sourceFileInfo.CreateEncryptedName();
            IAxCryptDocument document = new V1AxCryptDocument();
            IDataStore decryptedFileInfo = TypeMap.Resolve.New<IDataStore>(Path.Combine(_rootPath, "decrypted test.txt"));

            IAxCryptDocument nullDocument = null;
            IDataStore nullFileInfo = null;
            LogOnIdentity nullKey = null;
            ProgressContext nullProgress = null;
            EncryptionParameters nullEncryptionParameters = null;
            Stream nullStream = null;
            string nullString = null;
            Action<Stream> nullStreamAction = null;

            Assert.Throws<ArgumentNullException>(() => { new AxCryptFile().Encrypt(nullFileInfo, destinationFileInfo, new EncryptionParameters(V1Aes128CryptoFactory.CryptoId, new Passphrase("axcrypt")), AxCryptOptions.EncryptWithCompression, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { new AxCryptFile().Encrypt(sourceFileInfo, nullFileInfo, new EncryptionParameters(V1Aes128CryptoFactory.CryptoId, new Passphrase("axcrypt")), AxCryptOptions.EncryptWithCompression, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { new AxCryptFile().Encrypt(sourceFileInfo, destinationFileInfo, nullEncryptionParameters, AxCryptOptions.EncryptWithCompression, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { new AxCryptFile().Encrypt(sourceFileInfo, destinationFileInfo, new EncryptionParameters(V1Aes128CryptoFactory.CryptoId, new Passphrase("axcrypt")), AxCryptOptions.EncryptWithCompression, nullProgress); });

            Assert.Throws<ArgumentNullException>(() => { new AxCryptFile().Encrypt(nullFileInfo, new MemoryStream(), EncryptionParameters.Empty, AxCryptOptions.None, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { new AxCryptFile().Encrypt(sourceFileInfo, nullStream, EncryptionParameters.Empty, AxCryptOptions.None, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { new AxCryptFile().Encrypt(sourceFileInfo, new MemoryStream(), nullEncryptionParameters, AxCryptOptions.None, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { new AxCryptFile().Encrypt(sourceFileInfo, new MemoryStream(), EncryptionParameters.Empty, AxCryptOptions.None, nullProgress); });

            Assert.Throws<ArgumentNullException>(() => { TypeMap.Resolve.New<AxCryptFile>().Decrypt(nullDocument, decryptedFileInfo, AxCryptOptions.SetFileTimes, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { TypeMap.Resolve.New<AxCryptFile>().Decrypt(document, nullFileInfo, AxCryptOptions.SetFileTimes, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { TypeMap.Resolve.New<AxCryptFile>().Decrypt(document, decryptedFileInfo, AxCryptOptions.SetFileTimes, nullProgress); });

            Assert.Throws<ArgumentNullException>(() => { TypeMap.Resolve.New<AxCryptFile>().Decrypt(nullFileInfo, decryptedFileInfo, LogOnIdentity.Empty, AxCryptOptions.SetFileTimes, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { TypeMap.Resolve.New<AxCryptFile>().Decrypt(sourceFileInfo, nullFileInfo, LogOnIdentity.Empty, AxCryptOptions.SetFileTimes, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { TypeMap.Resolve.New<AxCryptFile>().Decrypt(sourceFileInfo, decryptedFileInfo, nullKey, AxCryptOptions.SetFileTimes, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { TypeMap.Resolve.New<AxCryptFile>().Decrypt(sourceFileInfo, decryptedFileInfo, LogOnIdentity.Empty, AxCryptOptions.SetFileTimes, nullProgress); });

            Assert.Throws<ArgumentNullException>(() => { TypeMap.Resolve.New<AxCryptFile>().Decrypt(nullFileInfo, Path.Combine(_rootPath, "Directory"), LogOnIdentity.Empty, AxCryptOptions.SetFileTimes, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { TypeMap.Resolve.New<AxCryptFile>().Decrypt(sourceFileInfo, nullString, LogOnIdentity.Empty, AxCryptOptions.SetFileTimes, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { TypeMap.Resolve.New<AxCryptFile>().Decrypt(sourceFileInfo, Path.Combine(_rootPath, "Directory"), nullKey, AxCryptOptions.SetFileTimes, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { TypeMap.Resolve.New<AxCryptFile>().Decrypt(sourceFileInfo, Path.Combine(_rootPath, "Directory"), LogOnIdentity.Empty, AxCryptOptions.SetFileTimes, nullProgress); });

            Assert.Throws<ArgumentNullException>(() => { TypeMap.Resolve.New<AxCryptFile>().Document(nullFileInfo, LogOnIdentity.Empty, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { TypeMap.Resolve.New<AxCryptFile>().Document(sourceFileInfo, nullKey, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { TypeMap.Resolve.New<AxCryptFile>().Document(sourceFileInfo, LogOnIdentity.Empty, nullProgress); });

            Assert.Throws<ArgumentNullException>(() => { TypeMap.Resolve.New<AxCryptFile>().WriteToFileWithBackup(null, (Stream stream) => { }, new ProgressContext()); });
            IDataStore fileInfo = TypeMap.Resolve.New<IDataStore>(_testTextPath);
            Assert.Throws<ArgumentNullException>(() => { TypeMap.Resolve.New<AxCryptFile>().WriteToFileWithBackup(fileInfo, nullStreamAction, new ProgressContext()); });

            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.MakeAxCryptFileName(nullFileInfo); });
            Assert.Throws<ArgumentNullException>(() => { TypeMap.Resolve.New<AxCryptFile>().Wipe(nullFileInfo, new ProgressContext()); });
        }

        [Test]
        public void TestSmallEncryptDecrypt()
        {
            IDataStore sourceFileInfo = TypeMap.Resolve.New<IDataStore>(_testTextPath);
            IDataStore destinationFileInfo = sourceFileInfo.CreateEncryptedName();
            Assert.That(destinationFileInfo.Name, Is.EqualTo("test-txt.axx"), "Wrong encrypted file name based on the plain text file name.");
            EncryptionParameters encryptionParameters = new EncryptionParameters(V1Aes128CryptoFactory.CryptoId, new Passphrase("axcrypt"));

            new AxCryptFile().Encrypt(sourceFileInfo, destinationFileInfo, encryptionParameters, AxCryptOptions.EncryptWithCompression, new ProgressContext());
            using (IAxCryptDocument document = TypeMap.Resolve.New<AxCryptFile>().Document(destinationFileInfo, new LogOnIdentity("axcrypt"), new ProgressContext()))
            {
                Assert.That(document.PassphraseIsValid, Is.True, "The passphrase should be ok.");
                Assert.That(document.FileName, Is.EqualTo("test.txt"), "Unexpected file name in headers.");
                Assert.That(document.CreationTimeUtc, Is.EqualTo(FakeDataStore.TestDate1Utc));
                Assert.That(document.LastAccessTimeUtc, Is.EqualTo(FakeDataStore.TestDate2Utc));
                Assert.That(document.LastWriteTimeUtc, Is.EqualTo(FakeDataStore.TestDate3Utc));
                IDataStore decryptedFileInfo = TypeMap.Resolve.New<IDataStore>(Path.Combine(_rootPath, "decrypted test.txt"));
                TypeMap.Resolve.New<AxCryptFile>().Decrypt(document, decryptedFileInfo, AxCryptOptions.SetFileTimes, new ProgressContext());
                using (Stream decryptedStream = decryptedFileInfo.OpenRead())
                {
                    string decrypted = new StreamReader(decryptedStream, Encoding.UTF8).ReadToEnd();
                    Assert.That(decrypted, Is.EqualTo("This is a short file"));
                }
                Assert.That(decryptedFileInfo.CreationTimeUtc, Is.EqualTo(document.CreationTimeUtc), "We're expecting file times to be set as the original from the headers.");
                Assert.That(decryptedFileInfo.LastAccessTimeUtc, Is.EqualTo(document.LastAccessTimeUtc), "We're expecting file times to be set as the original from the headers.");
                Assert.That(decryptedFileInfo.LastWriteTimeUtc, Is.EqualTo(document.LastWriteTimeUtc), "We're expecting file times to be set as the original from the headers.");
            }
        }

        [Test]
        public void TestEncryptToStream()
        {
            IDataStore sourceFileInfo = TypeMap.Resolve.New<IDataStore>(_testTextPath);
            IDataStore destinationFileInfo = sourceFileInfo.CreateEncryptedName();
            Assert.That(destinationFileInfo.Name, Is.EqualTo("test-txt.axx"), "Wrong encrypted file name based on the plain text file name.");
            using (Stream destinationStream = destinationFileInfo.OpenWrite())
            {
                EncryptionParameters parameters = new EncryptionParameters(V2Aes128CryptoFactory.CryptoId, new Passphrase("axcrypt"));
                new AxCryptFile().Encrypt(sourceFileInfo, destinationStream, parameters, AxCryptOptions.EncryptWithCompression, new ProgressContext());
            }

            using (IAxCryptDocument document = TypeMap.Resolve.New<AxCryptFile>().Document(destinationFileInfo, new LogOnIdentity("axcrypt"), new ProgressContext()))
            {
                Assert.That(document.PassphraseIsValid, Is.True, "The passphrase should be ok.");
            }
        }

        [Test]
        public void TestDecryptToDestinationDirectory()
        {
            IDataStore sourceFileInfo = TypeMap.Resolve.New<IDataStore>(_helloWorldAxxPath);
            string destinationDirectory = Path.Combine(_rootPath, "Encrypted");

            string destinationFileName = TypeMap.Resolve.New<AxCryptFile>().Decrypt(sourceFileInfo, destinationDirectory, new LogOnIdentity("a"), AxCryptOptions.None, new ProgressContext());
            Assert.That(destinationFileName, Is.EqualTo("HelloWorld-Key-a.txt"), "The correct filename should be returned from decryption.");
        }

        [Test]
        public void TestDecryptToDestinationDirectoryWithWrongPassphrase()
        {
            IDataStore sourceFileInfo = TypeMap.Resolve.New<IDataStore>(_helloWorldAxxPath);
            string destinationDirectory = Path.Combine(_rootPath, "Encrypted");

            string destinationFileName = TypeMap.Resolve.New<AxCryptFile>().Decrypt(sourceFileInfo, destinationDirectory, new LogOnIdentity("Wrong Passphrase"), AxCryptOptions.None, new ProgressContext());
            Assert.That(destinationFileName, Is.Null, "When the wrong passphrase is given, the returned file name should be null to signal this.");
        }

        [Test]
        public void TestDecryptWithCancel()
        {
            IDataStore sourceFileInfo = TypeMap.Resolve.New<IDataStore>(_helloWorldAxxPath);
            Passphrase passphrase = new Passphrase("a");
            IProgressContext progress = new CancelProgressContext(new ProgressContext(new TimeSpan(0, 0, 0, 0, 100)));
            progress.Progressing += (object sender, ProgressEventArgs e) =>
            {
                progress.Cancel = true;
            };
            Headers headers = new Headers();
            AxCryptReaderBase reader = headers.CreateReader(new LookAheadStream(new ProgressStream(sourceFileInfo.OpenRead(), progress)));
            using (IAxCryptDocument document = AxCryptReaderBase.Document(reader))
            {
                bool keyIsOk = document.Load(passphrase, V1Aes128CryptoFactory.CryptoId, headers);
                Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                IDataStore destinationInfo = TypeMap.Resolve.New<IDataStore>(_rootPath.PathCombine("Destination", "Decrypted.txt"));

                FakeRuntimeEnvironment environment = (FakeRuntimeEnvironment)OS.Current;
                environment.CurrentTiming.CurrentTiming = new TimeSpan(0, 0, 0, 0, 100);
                Assert.Throws<OperationCanceledException>(() => { TypeMap.Resolve.New<AxCryptFile>().Decrypt(document, destinationInfo, AxCryptOptions.None, progress); });
            }
        }

        [Test]
        public void TestLargeEncryptDecrypt()
        {
            string sourceFullName = _davidCopperfieldTxtPath;

            IDataStore sourceRuntimeFileInfo = TypeMap.Resolve.New<IDataStore>(sourceFullName);
            IDataStore destinationRuntimeFileInfo = sourceRuntimeFileInfo.CreateEncryptedName();
            LogOnIdentity passphrase = new LogOnIdentity("laDabled@tAmeopot33");
            EncryptionParameters encryptionParameters = new EncryptionParameters(V1Aes128CryptoFactory.CryptoId, passphrase.Passphrase);

            new AxCryptFile().Encrypt(sourceRuntimeFileInfo, destinationRuntimeFileInfo, encryptionParameters, AxCryptOptions.SetFileTimes | AxCryptOptions.EncryptWithCompression, new ProgressContext());

            Assert.That(destinationRuntimeFileInfo.CreationTimeUtc, Is.EqualTo(sourceRuntimeFileInfo.CreationTimeUtc), "We're expecting file times to be set as the original from the headers.");
            Assert.That(destinationRuntimeFileInfo.LastAccessTimeUtc, Is.EqualTo(sourceRuntimeFileInfo.LastAccessTimeUtc), "We're expecting file times to be set as the original from the headers.");
            Assert.That(destinationRuntimeFileInfo.LastWriteTimeUtc, Is.EqualTo(sourceRuntimeFileInfo.LastWriteTimeUtc), "We're expecting file times to be set as the original from the headers.");

            DirectoryInfo decryptedDirectoryInfo = new DirectoryInfo(_rootPath.PathCombine("Destination", "Decrypted.txt"));
            string decryptedFullName = Path.Combine(decryptedDirectoryInfo.FullName, "David Copperfield.txt");
            IDataStore decryptedRuntimeFileInfo = TypeMap.Resolve.New<IDataStore>(decryptedFullName);

            TypeMap.Resolve.New<AxCryptFile>().Decrypt(destinationRuntimeFileInfo, decryptedRuntimeFileInfo, passphrase, AxCryptOptions.SetFileTimes, new ProgressContext());

            Assert.That(decryptedRuntimeFileInfo.CreationTimeUtc, Is.EqualTo(sourceRuntimeFileInfo.CreationTimeUtc), "We're expecting file times to be set as the original from the headers.");
            Assert.That(decryptedRuntimeFileInfo.LastAccessTimeUtc, Is.EqualTo(sourceRuntimeFileInfo.LastAccessTimeUtc), "We're expecting file times to be set as the original from the headers.");
            Assert.That(decryptedRuntimeFileInfo.LastWriteTimeUtc, Is.EqualTo(sourceRuntimeFileInfo.LastWriteTimeUtc), "We're expecting file times to be set as the original from the headers.");

            using (StreamReader sourceStreamReader = new StreamReader(sourceRuntimeFileInfo.OpenRead(), Encoding.GetEncoding("Windows-1252")))
            {
                using (StreamReader decryptedStreamReader = new StreamReader(decryptedRuntimeFileInfo.OpenRead(), Encoding.GetEncoding("Windows-1252")))
                {
                    string source = sourceStreamReader.ReadToEnd();
                    string decrypted = decryptedStreamReader.ReadToEnd();

                    Assert.That(decrypted, Is.EqualTo(source), "Comparing original plain text with the decrypted encrypted plain text.");
                }
            }
        }

        [Test]
        public void TestInvalidPassphrase()
        {
            IDataStore sourceFileInfo = TypeMap.Resolve.New<IDataStore>(_testTextPath);
            IDataStore encryptedFileInfo = sourceFileInfo.CreateEncryptedName();
            Assert.That(encryptedFileInfo.Name, Is.EqualTo("test-txt.axx"), "Wrong encrypted file name based on the plain text file name.");
            EncryptionParameters encryptionParameters = new EncryptionParameters(V1Aes128CryptoFactory.CryptoId, new Passphrase("axcrypt"));
            new AxCryptFile().Encrypt(sourceFileInfo, encryptedFileInfo, encryptionParameters, AxCryptOptions.EncryptWithCompression, new ProgressContext());

            IDataStore decryptedFileInfo = TypeMap.Resolve.New<IDataStore>(Path.Combine(_rootPath, "decrypted.txt"));
            bool isPassphraseOk = TypeMap.Resolve.New<AxCryptFile>().Decrypt(encryptedFileInfo, decryptedFileInfo, new LogOnIdentity("wrong"), AxCryptOptions.None, new ProgressContext());
            Assert.That(isPassphraseOk, Is.False, "The passphrase is wrong and should be wrong!");
        }

        [Test]
        public void TestUncompressedEncryptedDecryptAxCrypt17()
        {
            IDataStore sourceRuntimeFileInfo = TypeMap.Resolve.New<IDataStore>(_uncompressedAxxPath);
            IDataStore destinationRuntimeFileInfo = TypeMap.Resolve.New<IDataStore>(Path.Combine(Path.GetDirectoryName(_uncompressedAxxPath), "Uncompressed.zip"));
            Passphrase passphrase = new Passphrase("Uncompressable");
            using (V1AxCryptDocument document = new V1AxCryptDocument())
            {
                bool isOk = document.Load(passphrase, V1Aes128CryptoFactory.CryptoId, sourceRuntimeFileInfo.OpenRead());
                Assert.That(isOk, Is.True, "The document should load ok.");
                TypeMap.Resolve.New<AxCryptFile>().Decrypt(document, destinationRuntimeFileInfo, AxCryptOptions.None, new ProgressContext());
                Assert.That(document.DocumentHeaders.UncompressedLength, Is.EqualTo(0), "Since the data is not compressed, there should not be a CompressionInfo, but in 1.x there is, with value zero.");
            }
        }

        [Test]
        public void TestWriteToFileWithBackup()
        {
            string destinationFilePath = _rootPath.PathCombine("Written", "File.txt");
            using (MemoryStream inputStream = FakeDataStore.ExpandableMemoryStream(Encoding.UTF8.GetBytes("A string with some text")))
            {
                IDataStore destinationFileInfo = TypeMap.Resolve.New<IDataStore>(destinationFilePath);
                TypeMap.Resolve.New<AxCryptFile>().WriteToFileWithBackup(destinationFileInfo, (Stream stream) => { inputStream.CopyTo(stream, 4096); }, new ProgressContext());
                using (TextReader read = new StreamReader(destinationFileInfo.OpenRead()))
                {
                    string readString = read.ReadToEnd();
                    Assert.That(readString, Is.EqualTo("A string with some text"), "Where expecting the same string to be read back.");
                }
            }
        }

        [Test]
        public void TestWriteToFileWithBackupWithCancel()
        {
            IDataStore destinationFileInfo = TypeMap.Resolve.New<IDataStore>(_rootPath.PathCombine("Written", "File.txt"));
            using (MemoryStream inputStream = FakeDataStore.ExpandableMemoryStream(Encoding.UTF8.GetBytes("A string with some text")))
            {
                Assert.Throws<OperationCanceledException>(() => { TypeMap.Resolve.New<AxCryptFile>().WriteToFileWithBackup(destinationFileInfo, (Stream stream) => { throw new OperationCanceledException(); }, new ProgressContext()); });
                string tempFilePath = _rootPath.PathCombine("Written", "File.bak");
                IDataStore tempFileInfo = TypeMap.Resolve.New<IDataStore>(tempFilePath);
                Assert.That(tempFileInfo.IsAvailable, Is.False, "The .bak file should be removed.");
            }
        }

        [Test]
        public void TestWriteToFileWithBackupWhenDestinationExists()
        {
            string destinationFilePath = _rootPath.PathCombine("Written", "AnExistingFile.txt");
            IDataStore destinationFileInfo = TypeMap.Resolve.New<IDataStore>(destinationFilePath);
            IDataStore bakFileInfo = TypeMap.Resolve.New<IDataStore>(_rootPath.PathCombine("Written", "AnExistingFile.bak"));
            Assert.That(bakFileInfo.IsAvailable, Is.False, "The file should not exist to start with.");
            using (Stream writeStream = destinationFileInfo.OpenWrite())
            {
                byte[] bytes = Encoding.UTF8.GetBytes("A string");
                writeStream.Write(bytes, 0, bytes.Length);
            }

            using (MemoryStream inputStream = FakeDataStore.ExpandableMemoryStream(Encoding.UTF8.GetBytes("A string with some text")))
            {
                TypeMap.Resolve.New<AxCryptFile>().WriteToFileWithBackup(destinationFileInfo, (Stream stream) => { inputStream.CopyTo(stream, 4096); }, new ProgressContext());
                using (TextReader read = new StreamReader(destinationFileInfo.OpenRead()))
                {
                    string readString = read.ReadToEnd();
                    Assert.That(readString, Is.EqualTo("A string with some text"), "Where expecting the same string to be read back.");
                }
            }
            Assert.That(bakFileInfo.IsAvailable, Is.False, "The file should not exist afterwards either.");
        }

        [Test]
        public void TestMakeAxCryptFileName()
        {
            string testFile = _rootPath.PathCombine("Directory", "file.txt");
            string axxFile = _rootPath.PathCombine("Directory", "file-txt.axx");
            string madeName = AxCryptFile.MakeAxCryptFileName(TypeMap.Resolve.New<IDataStore>(testFile));
            Assert.That(madeName, Is.EqualTo(axxFile), "The AxCrypt version of the name is unexpected.");
        }

        [Test]
        public void TestWipe()
        {
            string testFile = _rootPath.PathCombine("Folder", "file-to-be-wiped.txt");
            IDataStore fileInfo = TypeMap.Resolve.New<IDataStore>(testFile);
            using (Stream writeStream = fileInfo.OpenWrite())
            {
            }
            Assert.That(fileInfo.IsAvailable, "Now it should exist.");
            TypeMap.Resolve.New<AxCryptFile>().Wipe(fileInfo, new ProgressContext());
            Assert.That(!fileInfo.IsAvailable, "And now it should not exist after wiping.");
        }

        [Test]
        public void TestEncryptFileWithBackupFileInfoAndWipeNullArguments()
        {
            string sourceFilePath = _davidCopperfieldTxtPath;
            string destinationFilePath = Path.Combine(Path.GetDirectoryName(sourceFilePath), "David Copperfield-txt.axx");

            IDataStore sourceFileInfo = TypeMap.Resolve.New<IDataStore>(sourceFilePath);
            IDataStore destinationFileInfo = TypeMap.Resolve.New<IDataStore>(destinationFilePath);
            IDataStore nullFileInfo = null;

            EncryptionParameters nullEncryptionParameters = null;
            EncryptionParameters encryptionParameters = new EncryptionParameters(Guid.Empty, Passphrase.Empty);

            ProgressContext progress = new ProgressContext();
            ProgressContext nullProgress = null;

            Assert.Throws<ArgumentNullException>(() => { TypeMap.Resolve.New<AxCryptFile>().EncryptFileWithBackupAndWipe(nullFileInfo, destinationFileInfo, encryptionParameters, progress); });
            Assert.Throws<ArgumentNullException>(() => { TypeMap.Resolve.New<AxCryptFile>().EncryptFileWithBackupAndWipe(sourceFileInfo, nullFileInfo, encryptionParameters, progress); });
            Assert.Throws<ArgumentNullException>(() => { TypeMap.Resolve.New<AxCryptFile>().EncryptFileWithBackupAndWipe(sourceFileInfo, destinationFileInfo, nullEncryptionParameters, progress); });
            Assert.Throws<ArgumentNullException>(() => { TypeMap.Resolve.New<AxCryptFile>().EncryptFileWithBackupAndWipe(sourceFileInfo, destinationFileInfo, encryptionParameters, nullProgress); });
        }

        [Test]
        public void TestEncryptFileWithBackupAndWipeFileInfo()
        {
            string sourceFilePath = _davidCopperfieldTxtPath;
            string destinationFilePath = Path.Combine(Path.GetDirectoryName(sourceFilePath), "David Copperfield-txt.axx");

            IDataStore sourceFileInfo = TypeMap.Resolve.New<IDataStore>(sourceFilePath);
            IDataStore destinationFileInfo = TypeMap.Resolve.New<IDataStore>(destinationFilePath);

            EncryptionParameters encryptionParameters = new EncryptionParameters(V2Aes128CryptoFactory.CryptoId, new Passphrase("a"));

            ProgressContext progress = new ProgressContext();

            TypeMap.Resolve.New<AxCryptFile>().EncryptFileWithBackupAndWipe(sourceFileInfo, destinationFileInfo, encryptionParameters, progress);

            Assert.That(sourceFileInfo.IsAvailable, Is.False, "The source should be wiped.");
            Assert.That(destinationFileInfo.IsAvailable, Is.True, "The destination should be created and exist now.");
        }

        [Test]
        public void TestEncryptFileWithBackupFileNameAndWipeNullArguments()
        {
            string sourceFilePath = _davidCopperfieldTxtPath;
            string destinationFilePath = Path.Combine(Path.GetDirectoryName(sourceFilePath), "David Copperfield-txt.axx");

            string nullFileName = null;

            EncryptionParameters encryptionParameters = new EncryptionParameters(Guid.Empty, Passphrase.Empty);
            EncryptionParameters nullEncryptionParameters = null;

            ProgressContext progress = new ProgressContext();
            ProgressContext nullProgress = null;

            Assert.Throws<ArgumentNullException>(() => { TypeMap.Resolve.New<AxCryptFile>().EncryptFileWithBackupAndWipe(nullFileName, destinationFilePath, encryptionParameters, progress); });
            Assert.Throws<ArgumentNullException>(() => { TypeMap.Resolve.New<AxCryptFile>().EncryptFileWithBackupAndWipe(sourceFilePath, nullFileName, encryptionParameters, progress); });
            Assert.Throws<ArgumentNullException>(() => { TypeMap.Resolve.New<AxCryptFile>().EncryptFileWithBackupAndWipe(sourceFilePath, destinationFilePath, nullEncryptionParameters, progress); });
            Assert.Throws<ArgumentNullException>(() => { TypeMap.Resolve.New<AxCryptFile>().EncryptFileWithBackupAndWipe(sourceFilePath, destinationFilePath, encryptionParameters, nullProgress); });
        }

        [Test]
        public void TestEncryptFileWithBackupAndWipeFileName()
        {
            string sourceFilePath = _davidCopperfieldTxtPath;
            string destinationFilePath = Path.Combine(Path.GetDirectoryName(sourceFilePath), "David Copperfield-txt.axx");

            EncryptionParameters encryptionParameters = new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("b"));
            ProgressContext progress = new ProgressContext();

            TypeMap.Resolve.New<AxCryptFile>().EncryptFileWithBackupAndWipe(sourceFilePath, destinationFilePath, encryptionParameters, progress);

            IDataStore sourceFileInfo = TypeMap.Resolve.New<IDataStore>(sourceFilePath);
            IDataStore destinationFileInfo = TypeMap.Resolve.New<IDataStore>(destinationFilePath);
            Assert.That(sourceFileInfo.IsAvailable, Is.False, "The source should be wiped.");
            Assert.That(destinationFileInfo.IsAvailable, Is.True, "The destination should be created and exist now.");
        }

        [Test]
        public void TestDecryptFileUniqueWithWipeOfOriginal()
        {
            IDataStore sourceFileInfo = TypeMap.Resolve.New<IDataStore>(_helloWorldAxxPath);
            IDataStore destinationFileInfo = TypeMap.Resolve.New<IDataStore>(Path.Combine(Path.GetDirectoryName(_helloWorldAxxPath), "HelloWorld-Key-a.txt"));
            LogOnIdentity passphrase = new LogOnIdentity("a");

            Assert.That(sourceFileInfo.IsAvailable, Is.True, "The source should exist.");
            Assert.That(destinationFileInfo.IsAvailable, Is.False, "The source should not exist yet.");

            TypeMap.Resolve.New<AxCryptFile>().DecryptFileUniqueWithWipeOfOriginal(sourceFileInfo, passphrase, new ProgressContext());

            Assert.That(sourceFileInfo.IsAvailable, Is.False, "The source should be wiped.");
            Assert.That(destinationFileInfo.IsAvailable, Is.True, "The destination should be created and exist now.");
        }

        [Test]
        public void TestDecryptFilesUniqueWithWipeOfOriginal()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new ParallelFileOperation());
            TypeMap.Register.Singleton<IProgressBackground>(() => new FakeProgressBackground());
            TypeMap.Register.Singleton<IUIThread>(() => new FakeUIThread());
            IDataStore sourceFileInfo = TypeMap.Resolve.New<IDataStore>(_helloWorldAxxPath);
            IDataContainer sourceFolderInfo = TypeMap.Resolve.New<IDataContainer>(Path.GetDirectoryName(sourceFileInfo.FullName));
            sourceFolderInfo.CreateFolder();
            IDataStore destinationFileInfo = TypeMap.Resolve.New<IDataStore>(Path.Combine(Path.GetDirectoryName(_helloWorldAxxPath), "HelloWorld-Key-a.txt"));
            LogOnIdentity passphrase = new LogOnIdentity("a");

            Mock<IStatusChecker> mockStatusChecker = new Mock<IStatusChecker>();

            Assert.That(sourceFileInfo.IsAvailable, Is.True, "The source should exist.");
            Assert.That(destinationFileInfo.IsAvailable, Is.False, "The source should not exist yet.");

            TypeMap.Resolve.New<AxCryptFile>().DecryptFilesInsideFolderUniqueWithWipeOfOriginal(sourceFolderInfo, passphrase, mockStatusChecker.Object, new ProgressContext());

            Assert.That(sourceFileInfo.IsAvailable, Is.False, "The source should be wiped.");
            Assert.That(destinationFileInfo.IsAvailable, Is.True, "The destination should be created and exist now.");
        }

        [Test]
        public void TestEncryptFileUniqueWithBackupAndWipeWithNoCollision()
        {
            IDataStore sourceFileInfo = TypeMap.Resolve.New<IDataStore>(_davidCopperfieldTxtPath);
            sourceFileInfo.Container.CreateFolder();
            IDataStore destinationFileInfo = TypeMap.Resolve.New<IDataStore>(Path.Combine(Path.GetDirectoryName(_davidCopperfieldTxtPath), "David Copperfield-txt.axx"));

            EncryptionParameters encryptionParameters = new EncryptionParameters(V1Aes128CryptoFactory.CryptoId, new Passphrase("allan"));
            LogOnIdentity passphrase = new LogOnIdentity("allan");

            TypeMap.Resolve.New<AxCryptFile>().EncryptFileUniqueWithBackupAndWipe(sourceFileInfo, encryptionParameters, new ProgressContext());

            Assert.That(sourceFileInfo.IsAvailable, Is.False, "The source should be wiped.");
            Assert.That(destinationFileInfo.IsAvailable, Is.True, "The destination should be created and exist now.");
        }

        [Test]
        public void TestEncryptFileUniqueWithBackupAndWipeWithCollision()
        {
            IDataStore sourceFileInfo = TypeMap.Resolve.New<IDataStore>(_davidCopperfieldTxtPath);
            sourceFileInfo.Container.CreateFolder();
            sourceFileInfo.Container.CreateNewFile("David Copperfield-txt.axx");

            IDataStore alternateDestinationFileInfo = TypeMap.Resolve.New<IDataStore>(Path.Combine(Path.GetDirectoryName(_davidCopperfieldTxtPath), "David Copperfield-txt.1.axx"));

            EncryptionParameters encryptionParameters = new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("allan"));

            TypeMap.Resolve.New<AxCryptFile>().EncryptFileUniqueWithBackupAndWipe(sourceFileInfo, encryptionParameters, new ProgressContext());

            Assert.That(sourceFileInfo.IsAvailable, Is.False, "The source should be wiped.");
            Assert.That(alternateDestinationFileInfo.IsAvailable, Is.True, "The destination should be created and exist now.");
        }

        [Test]
        public void TestEncryptFilesUniqueWithBackupAndWipeWithNoCollision()
        {
            IDataStore sourceFileInfo = TypeMap.Resolve.New<IDataStore>(_davidCopperfieldTxtPath);
            sourceFileInfo.Container.CreateFolder();
            IDataContainer sourceFolderInfo = sourceFileInfo.Container;
            IDataStore destinationFileInfo = sourceFileInfo.Container.FileItemInfo("David Copperfield-txt.axx");

            EncryptionParameters encryptionParameters = new EncryptionParameters(V1Aes128CryptoFactory.CryptoId, new Passphrase("allan"));

            TypeMap.Resolve.New<AxCryptFile>().EncryptFoldersUniqueWithBackupAndWipe(new IDataContainer[] { sourceFolderInfo }, encryptionParameters, new ProgressContext());

            Assert.That(sourceFileInfo.IsAvailable, Is.False, "The source should be wiped.");
            Assert.That(destinationFileInfo.IsAvailable, Is.True, "The destination should be created and exist now.");
        }

        [Test]
        public void TestEncryptFilesUniqueWithBackupAndWipeWithCollision()
        {
            IDataStore sourceFileInfo = TypeMap.Resolve.New<IDataStore>(_davidCopperfieldTxtPath);
            sourceFileInfo.Container.CreateFolder();
            IDataContainer sourceFolderInfo = sourceFileInfo.Container;
            sourceFolderInfo.CreateNewFile("David Copperfield-txt.axx");
            IDataStore alternateDestinationFileInfo = sourceFolderInfo.FileItemInfo("David Copperfield-txt.1.axx");
            Assert.That(alternateDestinationFileInfo.IsAvailable, Is.False, "The destination should not be created and exist yet.");

            EncryptionParameters encryptionParameters = new EncryptionParameters(V2Aes128CryptoFactory.CryptoId, new Passphrase("allan"));

            TypeMap.Resolve.New<AxCryptFile>().EncryptFoldersUniqueWithBackupAndWipe(new IDataContainer[] { sourceFolderInfo }, encryptionParameters, new ProgressContext());

            Assert.That(sourceFileInfo.IsAvailable, Is.False, "The source should be wiped.");

            Assert.That(alternateDestinationFileInfo.IsAvailable, Is.True, "The destination should be created and exist now.");
        }

        [Test]
        public void TestWipeFileDoesNotExist()
        {
            ProgressContext progress = new ProgressContext(TimeSpan.Zero);
            bool progressed = false;
            progress.Progressing += (object sender, ProgressEventArgs e) =>
            {
                progressed = true;
            };

            string filePath = Path.Combine(Path.Combine(_rootPath, "Folder"), "DoesNot.Exist");
            IDataStore fileInfo = TypeMap.Resolve.New<IDataStore>(filePath);

            Assert.DoesNotThrow(() => { TypeMap.Resolve.New<AxCryptFile>().Wipe(fileInfo, progress); });
            Assert.That(!progressed, "There should be no progress-notification since nothing should happen.");
        }

        [Test]
        public void TestWipeWithDelayedUntilDoneCancel()
        {
            IDataStore fileInfo = TypeMap.Resolve.New<IDataStore>(_davidCopperfieldTxtPath);

            IProgressContext progress = new CancelProgressContext(new ProgressContext(TimeSpan.Zero));
            progress.Progressing += (object sender, ProgressEventArgs e) =>
            {
                ((IProgressContext)sender).Cancel = true;
            };
            Assert.Throws<OperationCanceledException>(() => { TypeMap.Resolve.New<AxCryptFile>().Wipe(fileInfo, progress); });
            Assert.That(!fileInfo.IsAvailable, "The file should be completely wiped, even if canceled at start.");
        }

        [Test]
        public void TestDocumentNullArguments()
        {
            IDataStore nullSourceFile = null;
            LogOnIdentity nullPassphrase = null;
            IProgressContext nullProgress = null;

            IDataStore sourceFile = TypeMap.Resolve.New<IDataStore>(@"C:\Folder\File.txt");
            LogOnIdentity passphrase = new LogOnIdentity("allan");
            IProgressContext progress = new ProgressContext();

            IAxCryptDocument document = null;
            Assert.Throws<ArgumentNullException>(() => document = TypeMap.Resolve.New<AxCryptFile>().Document(nullSourceFile, passphrase, progress));
            Assert.Throws<ArgumentNullException>(() => document = TypeMap.Resolve.New<AxCryptFile>().Document(sourceFile, nullPassphrase, progress));
            Assert.Throws<ArgumentNullException>(() => document = TypeMap.Resolve.New<AxCryptFile>().Document(sourceFile, passphrase, nullProgress));
            Assert.That(document, Is.Null);
        }
    }
}