#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
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
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Fake;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

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
        private static readonly string _helloWorldAxxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Subfolder", "HelloWorld.axx");

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
            IDataStore sourceFileInfo = New<IDataStore>(_testTextPath);
            IDataStore destinationFileInfo = sourceFileInfo.CreateEncryptedName();
            IAxCryptDocument document = new V1AxCryptDocument();
            IDataStore decryptedFileInfo = New<IDataStore>(Path.Combine(_rootPath, "decrypted test.txt"));

            IAxCryptDocument nullDocument = null;
            IDataStore nullFileInfo = null;
            LogOnIdentity nullKey = null;
            ProgressContext nullProgress = null;
            EncryptionParameters nullEncryptionParameters = null;
            Stream nullStream = null;
            string nullString = null;
            Action<Stream> nullStreamAction = null;

            Assert.Throws<ArgumentNullException>(() => { new AxCryptFile().Encrypt(nullFileInfo, destinationFileInfo, new EncryptionParameters(new V1Aes128CryptoFactory().CryptoId, new Passphrase("axcrypt")), AxCryptOptions.EncryptWithCompression, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { new AxCryptFile().Encrypt(sourceFileInfo, nullFileInfo, new EncryptionParameters(new V1Aes128CryptoFactory().CryptoId, new Passphrase("axcrypt")), AxCryptOptions.EncryptWithCompression, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { new AxCryptFile().Encrypt(sourceFileInfo, destinationFileInfo, nullEncryptionParameters, AxCryptOptions.EncryptWithCompression, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { new AxCryptFile().Encrypt(sourceFileInfo, destinationFileInfo, new EncryptionParameters(new V1Aes128CryptoFactory().CryptoId, new Passphrase("axcrypt")), AxCryptOptions.EncryptWithCompression, nullProgress); });

            Assert.Throws<ArgumentNullException>(() => { new AxCryptFile().Encrypt(nullFileInfo, new MemoryStream(), EncryptionParameters.Empty, AxCryptOptions.None, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { new AxCryptFile().Encrypt(sourceFileInfo, nullStream, EncryptionParameters.Empty, AxCryptOptions.None, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { new AxCryptFile().Encrypt(sourceFileInfo, new MemoryStream(), nullEncryptionParameters, AxCryptOptions.None, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { new AxCryptFile().Encrypt(sourceFileInfo, new MemoryStream(), EncryptionParameters.Empty, AxCryptOptions.None, nullProgress); });

            Assert.Throws<ArgumentNullException>((TestDelegate)(() => { New<AxCryptFile>().Decrypt(nullDocument, decryptedFileInfo, AxCryptOptions.SetFileTimes, new ProgressContext()); }));
            Assert.Throws<ArgumentNullException>((TestDelegate)(() => { New<AxCryptFile>().Decrypt(document, nullFileInfo, AxCryptOptions.SetFileTimes, new ProgressContext()); }));
            Assert.Throws<ArgumentNullException>((TestDelegate)(() => { New<AxCryptFile>().Decrypt(document, decryptedFileInfo, AxCryptOptions.SetFileTimes, nullProgress); }));

            Assert.Throws<ArgumentNullException>((TestDelegate)(() => { New<AxCryptFile>().Decrypt(nullFileInfo, decryptedFileInfo, LogOnIdentity.Empty, AxCryptOptions.SetFileTimes, new ProgressContext()); }));
            Assert.Throws<ArgumentNullException>((TestDelegate)(() => { New<AxCryptFile>().Decrypt(sourceFileInfo, nullFileInfo, LogOnIdentity.Empty, AxCryptOptions.SetFileTimes, new ProgressContext()); }));
            Assert.Throws<ArgumentNullException>((TestDelegate)(() => { New<AxCryptFile>().Decrypt(sourceFileInfo, decryptedFileInfo, nullKey, AxCryptOptions.SetFileTimes, new ProgressContext()); }));
            Assert.Throws<ArgumentNullException>((TestDelegate)(() => { New<AxCryptFile>().Decrypt(sourceFileInfo, decryptedFileInfo, LogOnIdentity.Empty, AxCryptOptions.SetFileTimes, nullProgress); }));

            Assert.Throws<ArgumentNullException>((TestDelegate)(() => { New<AxCryptFile>().Decrypt(nullFileInfo, Path.Combine(_rootPath, "Directory"), LogOnIdentity.Empty, AxCryptOptions.SetFileTimes, new ProgressContext()); }));
            Assert.Throws<ArgumentNullException>((TestDelegate)(() => { New<AxCryptFile>().Decrypt(sourceFileInfo, nullString, LogOnIdentity.Empty, AxCryptOptions.SetFileTimes, new ProgressContext()); }));
            Assert.Throws<ArgumentNullException>((TestDelegate)(() => { New<AxCryptFile>().Decrypt(sourceFileInfo, Path.Combine(_rootPath, "Directory"), nullKey, AxCryptOptions.SetFileTimes, new ProgressContext()); }));
            Assert.Throws<ArgumentNullException>((TestDelegate)(() => { New<AxCryptFile>().Decrypt(sourceFileInfo, Path.Combine(_rootPath, "Directory"), LogOnIdentity.Empty, AxCryptOptions.SetFileTimes, nullProgress); }));

            Assert.Throws<ArgumentNullException>((TestDelegate)(() => { New<AxCryptFile>().Document(nullFileInfo, LogOnIdentity.Empty, new ProgressContext()); }));
            Assert.Throws<ArgumentNullException>((TestDelegate)(() => { New<AxCryptFile>().Document(sourceFileInfo, nullKey, new ProgressContext()); }));
            Assert.Throws<ArgumentNullException>((TestDelegate)(() => { New<AxCryptFile>().Document(sourceFileInfo, LogOnIdentity.Empty, nullProgress); }));

            Assert.Throws<ArgumentNullException>((TestDelegate)(() => { New<AxCryptFile>().WriteToFileWithBackup(null, (Stream stream) => { }, new ProgressContext()); }));
            using (FileLock fileInfoLock = FileLock.Acquire(New<IDataStore>(_testTextPath)))
            {
                Assert.Throws<ArgumentNullException>((TestDelegate)(() => { New<AxCryptFile>().WriteToFileWithBackup(fileInfoLock, nullStreamAction, new ProgressContext()); }));
            }

            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.MakeAxCryptFileName(nullFileInfo); });
            FileLock nullFileLock = null;
            Assert.Throws<ArgumentNullException>((TestDelegate)(() => { New<AxCryptFile>().Wipe(nullFileLock, new ProgressContext()); }));
        }

        [Test]
        public void TestSmallEncryptDecrypt()
        {
            IDataStore sourceFileInfo = New<IDataStore>(_testTextPath);
            IDataStore destinationFileInfo = sourceFileInfo.CreateEncryptedName();
            Assert.That(destinationFileInfo.Name, Is.EqualTo("test-txt.axx"), "Wrong encrypted file name based on the plain text file name.");
            EncryptionParameters encryptionParameters = new EncryptionParameters(new V1Aes128CryptoFactory().CryptoId, new Passphrase("axcrypt"));

            new AxCryptFile().Encrypt(sourceFileInfo, destinationFileInfo, encryptionParameters, AxCryptOptions.EncryptWithCompression, new ProgressContext());
            using (IAxCryptDocument document = New<AxCryptFile>().Document(destinationFileInfo, new LogOnIdentity("axcrypt"), new ProgressContext()))
            {
                Assert.That(document.PassphraseIsValid, Is.True, "The passphrase should be ok.");
                Assert.That(document.FileName, Is.EqualTo("test.txt"), "Unexpected file name in headers.");
                Assert.That(document.CreationTimeUtc, Is.EqualTo(FakeDataStore.TestDate1Utc));
                Assert.That(document.LastAccessTimeUtc, Is.EqualTo(FakeDataStore.TestDate2Utc));
                Assert.That(document.LastWriteTimeUtc, Is.EqualTo(FakeDataStore.TestDate3Utc));
                IDataStore decryptedFileInfo = New<IDataStore>(Path.Combine(_rootPath, "decrypted test.txt"));
                New<AxCryptFile>().Decrypt(document, decryptedFileInfo, AxCryptOptions.SetFileTimes, new ProgressContext());
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
            IDataStore sourceFileInfo = New<IDataStore>(_testTextPath);
            IDataStore destinationFileInfo = sourceFileInfo.CreateEncryptedName();
            Assert.That(destinationFileInfo.Name, Is.EqualTo("test-txt.axx"), "Wrong encrypted file name based on the plain text file name.");
            using (Stream destinationStream = destinationFileInfo.OpenWrite())
            {
                EncryptionParameters parameters = new EncryptionParameters(new V2Aes128CryptoFactory().CryptoId, new Passphrase("axcrypt"));
                new AxCryptFile().Encrypt(sourceFileInfo, destinationStream, parameters, AxCryptOptions.EncryptWithCompression, new ProgressContext());
            }

            using (IAxCryptDocument document = New<AxCryptFile>().Document(destinationFileInfo, new LogOnIdentity("axcrypt"), new ProgressContext()))
            {
                Assert.That(document.PassphraseIsValid, Is.True, "The passphrase should be ok.");
            }
        }

        [Test]
        public void TestDecryptToDestinationDirectory()
        {
            IDataStore sourceFileInfo = New<IDataStore>(_helloWorldAxxPath);
            string destinationDirectory = Path.Combine(_rootPath, "Encrypted");

            string destinationFileName = New<AxCryptFile>().Decrypt(sourceFileInfo, destinationDirectory, new LogOnIdentity("a"), AxCryptOptions.None, new ProgressContext());
            Assert.That(destinationFileName, Is.EqualTo("HelloWorld-Key-a.txt"), "The correct filename should be returned from decryption.");
        }

        [Test]
        public void TestDecryptToDestinationDirectoryWithWrongPassphrase()
        {
            IDataStore sourceFileInfo = New<IDataStore>(_helloWorldAxxPath);
            string destinationDirectory = Path.Combine(_rootPath, "Encrypted");

            string destinationFileName = New<AxCryptFile>().Decrypt(sourceFileInfo, destinationDirectory, new LogOnIdentity("Wrong Passphrase"), AxCryptOptions.None, new ProgressContext());
            Assert.That(destinationFileName, Is.Null, "When the wrong passphrase is given, the returned file name should be null to signal this.");
        }

        [Test]
        public void TestDecryptWithCancel()
        {
            IDataStore sourceFileInfo = New<IDataStore>(_helloWorldAxxPath);
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
                bool keyIsOk = document.Load(passphrase, new V1Aes128CryptoFactory().CryptoId, headers);
                Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                IDataStore destinationInfo = New<IDataStore>(_rootPath.PathCombine("Destination", "Decrypted.txt"));

                FakeRuntimeEnvironment environment = (FakeRuntimeEnvironment)OS.Current;
                environment.CurrentTiming.CurrentTiming = new TimeSpan(0, 0, 0, 0, 100);
                Assert.Throws<OperationCanceledException>((TestDelegate)(() => { New<AxCryptFile>().Decrypt(document, destinationInfo, AxCryptOptions.None, progress); }));
            }
        }

        [Test]
        public void TestLargeEncryptDecrypt()
        {
            string sourceFullName = _davidCopperfieldTxtPath;

            IDataStore sourceRuntimeFileInfo = New<IDataStore>(sourceFullName);
            IDataStore destinationRuntimeFileInfo = sourceRuntimeFileInfo.CreateEncryptedName();
            LogOnIdentity passphrase = new LogOnIdentity("laDabled@tAmeopot33");
            EncryptionParameters encryptionParameters = new EncryptionParameters(new V1Aes128CryptoFactory().CryptoId, passphrase.Passphrase);

            new AxCryptFile().Encrypt(sourceRuntimeFileInfo, destinationRuntimeFileInfo, encryptionParameters, AxCryptOptions.SetFileTimes | AxCryptOptions.EncryptWithCompression, new ProgressContext());

            Assert.That(destinationRuntimeFileInfo.CreationTimeUtc, Is.EqualTo(sourceRuntimeFileInfo.CreationTimeUtc), "We're expecting file times to be set as the original from the headers.");
            Assert.That(destinationRuntimeFileInfo.LastAccessTimeUtc, Is.EqualTo(sourceRuntimeFileInfo.LastAccessTimeUtc), "We're expecting file times to be set as the original from the headers.");
            Assert.That(destinationRuntimeFileInfo.LastWriteTimeUtc, Is.EqualTo(sourceRuntimeFileInfo.LastWriteTimeUtc), "We're expecting file times to be set as the original from the headers.");

            DirectoryInfo decryptedDirectoryInfo = new DirectoryInfo(_rootPath.PathCombine("Destination", "Decrypted.txt"));
            string decryptedFullName = Path.Combine(decryptedDirectoryInfo.FullName, "David Copperfield.txt");
            IDataStore decryptedRuntimeFileInfo = New<IDataStore>(decryptedFullName);

            New<AxCryptFile>().Decrypt(destinationRuntimeFileInfo, decryptedRuntimeFileInfo, passphrase, AxCryptOptions.SetFileTimes, new ProgressContext());

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
            IDataStore sourceFileInfo = New<IDataStore>(_testTextPath);
            IDataStore encryptedFileInfo = sourceFileInfo.CreateEncryptedName();
            Assert.That(encryptedFileInfo.Name, Is.EqualTo("test-txt.axx"), "Wrong encrypted file name based on the plain text file name.");
            EncryptionParameters encryptionParameters = new EncryptionParameters(new V1Aes128CryptoFactory().CryptoId, new Passphrase("axcrypt"));
            new AxCryptFile().Encrypt(sourceFileInfo, encryptedFileInfo, encryptionParameters, AxCryptOptions.EncryptWithCompression, new ProgressContext());

            IDataStore decryptedFileInfo = New<IDataStore>(Path.Combine(_rootPath, "decrypted.txt"));
            bool isPassphraseOk = New<AxCryptFile>().Decrypt(encryptedFileInfo, decryptedFileInfo, new LogOnIdentity("wrong"), AxCryptOptions.None, new ProgressContext());
            Assert.That(isPassphraseOk, Is.False, "The passphrase is wrong and should be wrong!");
        }

        [Test]
        public void TestUncompressedEncryptedDecryptAxCrypt17()
        {
            IDataStore sourceRuntimeFileInfo = New<IDataStore>(_uncompressedAxxPath);
            IDataStore destinationRuntimeFileInfo = New<IDataStore>(Path.Combine(Path.GetDirectoryName(_uncompressedAxxPath), "Uncompressed.zip"));
            Passphrase passphrase = new Passphrase("Uncompressable");
            using (V1AxCryptDocument document = new V1AxCryptDocument())
            {
                bool isOk = document.Load(passphrase, new V1Aes128CryptoFactory().CryptoId, sourceRuntimeFileInfo.OpenRead());
                Assert.That(isOk, Is.True, "The document should load ok.");
                New<AxCryptFile>().Decrypt(document, destinationRuntimeFileInfo, AxCryptOptions.None, new ProgressContext());
                Assert.That(document.DocumentHeaders.UncompressedLength, Is.EqualTo(0), "Since the data is not compressed, there should not be a CompressionInfo, but in 1.x there is, with value zero.");
            }
        }

        [Test]
        public void TestWriteToFileWithBackup()
        {
            string destinationFilePath = _rootPath.PathCombine("Written", "File.txt");
            using (MemoryStream inputStream = FakeDataStore.ExpandableMemoryStream(Encoding.UTF8.GetBytes("A string with some text")))
            {
                using (FileLock destinationFileLock = FileLock.Acquire(New<IDataStore>(destinationFilePath)))
                {
                    New<AxCryptFile>().WriteToFileWithBackup(destinationFileLock, (Stream stream) => { inputStream.CopyTo(stream, 4096); }, new ProgressContext());
                    using (TextReader read = new StreamReader(destinationFileLock.DataStore.OpenRead()))
                    {
                        string readString = read.ReadToEnd();
                        Assert.That(readString, Is.EqualTo("A string with some text"), "Where expecting the same string to be read back.");
                    }
                }
            }
        }

        [Test]
        public void TestWriteToFileWithBackupWithCancel()
        {
            IDataStore destinationFileInfo = New<IDataStore>(_rootPath.PathCombine("Written", "File.txt"));
            using (FileLock destinationFileLock = FileLock.Acquire(destinationFileInfo))
            {
                using (MemoryStream inputStream = FakeDataStore.ExpandableMemoryStream(Encoding.UTF8.GetBytes("A string with some text")))
                {
                    Assert.Throws<OperationCanceledException>((TestDelegate)(() => { New<AxCryptFile>().WriteToFileWithBackup(destinationFileLock, (Stream stream) => { throw new OperationCanceledException(); }, new ProgressContext()); }));
                    string tempFilePath = _rootPath.PathCombine("Written", "File.bak");
                    IDataStore tempFileInfo = New<IDataStore>(tempFilePath);
                    Assert.That(tempFileInfo.IsAvailable, Is.False, "The .bak file should be removed.");
                }
            }
        }

        [Test]
        public void TestWriteToFileWithBackupWhenDestinationExists()
        {
            string destinationFilePath = _rootPath.PathCombine("Written", "AnExistingFile.txt");
            IDataStore destinationFileInfo = New<IDataStore>(destinationFilePath);
            IDataStore bakFileInfo = New<IDataStore>(_rootPath.PathCombine("Written", "AnExistingFile.bak"));
            Assert.That(bakFileInfo.IsAvailable, Is.False, "The file should not exist to start with.");
            using (Stream writeStream = destinationFileInfo.OpenWrite())
            {
                byte[] bytes = Encoding.UTF8.GetBytes("A string");
                writeStream.Write(bytes, 0, bytes.Length);
            }

            using (FileLock destinationFileLock = FileLock.Acquire(destinationFileInfo))
            {
                using (MemoryStream inputStream = FakeDataStore.ExpandableMemoryStream(Encoding.UTF8.GetBytes("A string with some text")))
                {
                    New<AxCryptFile>().WriteToFileWithBackup(destinationFileLock, (Stream stream) => { inputStream.CopyTo(stream, 4096); }, new ProgressContext());
                    using (TextReader read = new StreamReader(destinationFileInfo.OpenRead()))
                    {
                        string readString = read.ReadToEnd();
                        Assert.That(readString, Is.EqualTo("A string with some text"), "Where expecting the same string to be read back.");
                    }
                }
            }
            Assert.That(bakFileInfo.IsAvailable, Is.False, "The file should not exist afterwards either.");
        }

        [Test]
        public void TestMakeAxCryptFileName()
        {
            string testFile = _rootPath.PathCombine("Directory", "file.txt");
            string axxFile = _rootPath.PathCombine("Directory", "file-txt.axx");
            string madeName = AxCryptFile.MakeAxCryptFileName(New<IDataStore>(testFile));
            Assert.That(madeName, Is.EqualTo(axxFile), "The AxCrypt version of the name is unexpected.");
        }

        [Test]
        public void TestWipe()
        {
            string testFile = _rootPath.PathCombine("Folder", "file-to-be-wiped.txt");
            IDataStore fileInfo = New<IDataStore>(testFile);
            using (Stream writeStream = fileInfo.OpenWrite())
            {
            }
            Assert.That(fileInfo.IsAvailable, "Now it should exist.");
            using (FileLock fileLock = FileLock.Acquire(fileInfo))
            {
                New<AxCryptFile>().Wipe(fileLock, new ProgressContext());
            }
            Assert.That(!fileInfo.IsAvailable, "And now it should not exist after wiping.");
        }

        [Test]
        public void TestEncryptFileWithBackupFileInfoAndWipeNullArguments()
        {
            string sourceFilePath = _davidCopperfieldTxtPath;
            string destinationFilePath = Path.Combine(Path.GetDirectoryName(sourceFilePath), "David Copperfield-txt.axx");

            IDataStore sourceFileInfo = New<IDataStore>(sourceFilePath);
            IDataStore destinationFileInfo = New<IDataStore>(destinationFilePath);
            using (FileLock destinationFileLock = FileLock.Acquire(destinationFileInfo))
            {
                IDataStore nullFileInfo = null;
                FileLock nullFileLock = null;

                EncryptionParameters nullEncryptionParameters = null;
                EncryptionParameters encryptionParameters = new EncryptionParameters(Guid.Empty, Passphrase.Empty);

                ProgressContext progress = new ProgressContext();
                ProgressContext nullProgress = null;

                Assert.Throws<ArgumentNullException>((TestDelegate)(() => { New<AxCryptFile>().EncryptFileWithBackupAndWipe(nullFileInfo, destinationFileLock, encryptionParameters, progress); }));
                Assert.Throws<ArgumentNullException>((TestDelegate)(() => { New<AxCryptFile>().EncryptFileWithBackupAndWipe(sourceFileInfo, nullFileLock, encryptionParameters, progress); }));
                Assert.Throws<ArgumentNullException>((TestDelegate)(() => { New<AxCryptFile>().EncryptFileWithBackupAndWipe(sourceFileInfo, destinationFileLock, nullEncryptionParameters, progress); }));
                Assert.Throws<ArgumentNullException>((TestDelegate)(() => { New<AxCryptFile>().EncryptFileWithBackupAndWipe(sourceFileInfo, destinationFileLock, encryptionParameters, nullProgress); }));
            }
        }

        [Test]
        public void TestEncryptFileWithBackupAndWipeFileInfo()
        {
            string sourceFilePath = _davidCopperfieldTxtPath;
            string destinationFilePath = Path.Combine(Path.GetDirectoryName(sourceFilePath), "David Copperfield-txt.axx");

            IDataStore sourceFileInfo = New<IDataStore>(sourceFilePath);
            IDataStore destinationFileInfo = New<IDataStore>(destinationFilePath);

            EncryptionParameters encryptionParameters = new EncryptionParameters(new V2Aes128CryptoFactory().CryptoId, new Passphrase("a"));

            ProgressContext progress = new ProgressContext();
            using (FileLock destinationFileLock = FileLock.Acquire(destinationFileInfo))
            {
                New<AxCryptFile>().EncryptFileWithBackupAndWipe(sourceFileInfo, destinationFileLock, encryptionParameters, progress);
            }

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

            Assert.Throws<ArgumentNullException>((TestDelegate)(() => { New<AxCryptFile>().EncryptFileWithBackupAndWipe(nullFileName, destinationFilePath, encryptionParameters, progress); }));
            Assert.Throws<ArgumentNullException>((TestDelegate)(() => { New<AxCryptFile>().EncryptFileWithBackupAndWipe(sourceFilePath, nullFileName, encryptionParameters, progress); }));
            Assert.Throws<ArgumentNullException>((TestDelegate)(() => { New<AxCryptFile>().EncryptFileWithBackupAndWipe(sourceFilePath, destinationFilePath, nullEncryptionParameters, progress); }));
            Assert.Throws<ArgumentNullException>((TestDelegate)(() => { New<AxCryptFile>().EncryptFileWithBackupAndWipe(sourceFilePath, destinationFilePath, encryptionParameters, nullProgress); }));
        }

        [Test]
        public void TestEncryptFileWithBackupAndWipeFileName()
        {
            string sourceFilePath = _davidCopperfieldTxtPath;
            string destinationFilePath = Path.Combine(Path.GetDirectoryName(sourceFilePath), "David Copperfield-txt.axx");

            EncryptionParameters encryptionParameters = new EncryptionParameters(new V2Aes256CryptoFactory().CryptoId, new Passphrase("b"));
            ProgressContext progress = new ProgressContext();

            New<AxCryptFile>().EncryptFileWithBackupAndWipe(sourceFilePath, destinationFilePath, encryptionParameters, progress);

            IDataStore sourceFileInfo = New<IDataStore>(sourceFilePath);
            IDataStore destinationFileInfo = New<IDataStore>(destinationFilePath);
            Assert.That(sourceFileInfo.IsAvailable, Is.False, "The source should be wiped.");
            Assert.That(destinationFileInfo.IsAvailable, Is.True, "The destination should be created and exist now.");
        }

        [Test]
        public async Task TestDecryptFileUniqueWithWipeOfOriginal()
        {
            IDataStore sourceFileInfo = New<IDataStore>(_helloWorldAxxPath);
            IDataStore destinationFileInfo = New<IDataStore>(Path.Combine(Path.GetDirectoryName(_helloWorldAxxPath), "HelloWorld-Key-a.txt"));
            LogOnIdentity passphrase = new LogOnIdentity("a");

            Assert.That(sourceFileInfo.IsAvailable, Is.True, "The source should exist.");
            Assert.That(destinationFileInfo.IsAvailable, Is.False, "The source should not exist yet.");

            await New<AxCryptFile>().DecryptFileUniqueWithWipeOfOriginalAsync(sourceFileInfo, passphrase, new ProgressContext());

            Assert.That(sourceFileInfo.IsAvailable, Is.False, "The source should be wiped.");
            Assert.That(destinationFileInfo.IsAvailable, Is.True, "The destination should be created and exist now.");
        }

        // Fails intermittently at "The source should be wiped". Problably a race situation. Must be investigated.
        // 2016-03-10: Attempted fix by adding call to Resolve.ProgressBackground.WaitForIdle(), and implementing it in FakeProgressBackground.
        // 2016-06-21: Attempted fix by 'capturing' result of enumeration of encrypted files in DecryptFilesInsideFolderUniqueWithWipeOfOriginalAsync(). Still not fixed.
        // 2016-09-12: Not fixed.
        // 2016-10-25: Still not fixed.
        // 2016-12-20: Still not fixed. Also see TestWipeFilesCancelingAfterOneWithWork which failed at the same time.
        // 2017-05-17: Attempted fix. Seems cause was a race where there were actually two files in the folder, only one with the right password. If the right one
        //             was tried first it, worked. If the other one was tried first, all was cancelled and nothing happened. Found when removing parallelism and
        //             it deterministically failed.
        [Test]
        public async Task TestDecryptFilesUniqueWithWipeOfOriginal()
        {
            TypeMap.Register.Singleton<ParallelFileOperation>(() => new ParallelFileOperation());
            TypeMap.Register.Singleton<IProgressBackground>(() => new FakeProgressBackground());
            IDataStore sourceFileInfo = New<IDataStore>(_helloWorldAxxPath);
            IDataContainer sourceFolderInfo = New<IDataContainer>(Path.GetDirectoryName(sourceFileInfo.FullName));

            IDataStore destinationFileInfo = New<IDataStore>(Path.Combine(Path.GetDirectoryName(_helloWorldAxxPath), "HelloWorld-Key-a.txt"));
            LogOnIdentity passphrase = new LogOnIdentity("a");

            Mock<IStatusChecker> mockStatusChecker = new Mock<IStatusChecker>();

            Assert.That(sourceFileInfo.IsAvailable, Is.True, "The source should exist.");
            Assert.That(destinationFileInfo.IsAvailable, Is.False, "The source should not exist yet.");

            await New<AxCryptFile>().DecryptFilesInsideFolderUniqueWithWipeOfOriginalAsync(sourceFolderInfo, passphrase, mockStatusChecker.Object, new ProgressContext());

            Assert.That(sourceFileInfo.IsAvailable, Is.False, "The source should be wiped.");
            Assert.That(destinationFileInfo.IsAvailable, Is.True, "The destination should be created and exist now.");
        }

        [Test]
        public void TestEncryptFileUniqueWithBackupAndWipeWithNoCollision()
        {
            IDataStore sourceFileInfo = New<IDataStore>(_davidCopperfieldTxtPath);
            sourceFileInfo.Container.CreateFolder();
            IDataStore destinationFileInfo = New<IDataStore>(Path.Combine(Path.GetDirectoryName(_davidCopperfieldTxtPath), "David Copperfield-txt.axx"));

            EncryptionParameters encryptionParameters = new EncryptionParameters(new V1Aes128CryptoFactory().CryptoId, new Passphrase("allan"));

            New<AxCryptFile>().EncryptFileUniqueWithBackupAndWipe(sourceFileInfo, encryptionParameters, new ProgressContext());

            Assert.That(sourceFileInfo.IsAvailable, Is.False, "The source should be wiped.");
            Assert.That(destinationFileInfo.IsAvailable, Is.True, "The destination should be created and exist now.");
        }

        [Test]
        public void TestEncryptFileUniqueWithBackupAndWipeWithCollision()
        {
            IDataStore sourceFileInfo = New<IDataStore>(_davidCopperfieldTxtPath);
            sourceFileInfo.Container.CreateFolder();
            sourceFileInfo.Container.CreateNewFile("David Copperfield-txt.axx");

            IDataStore alternateDestinationFileInfo = New<IDataStore>(Path.Combine(Path.GetDirectoryName(_davidCopperfieldTxtPath), "David Copperfield-txt.1.axx"));

            EncryptionParameters encryptionParameters = new EncryptionParameters(new V2Aes256CryptoFactory().CryptoId, new Passphrase("allan"));

            New<AxCryptFile>().EncryptFileUniqueWithBackupAndWipe(sourceFileInfo, encryptionParameters, new ProgressContext());

            Assert.That(sourceFileInfo.IsAvailable, Is.False, "The source should be wiped.");
            Assert.That(alternateDestinationFileInfo.IsAvailable, Is.True, "The destination should be created and exist now.");
        }

        [Test]
        public void TestEncryptFilesUniqueWithBackupAndWipeWithNoCollision()
        {
            IDataStore sourceFileInfo = New<IDataStore>(_davidCopperfieldTxtPath);
            sourceFileInfo.Container.CreateFolder();
            IDataContainer sourceFolderInfo = sourceFileInfo.Container;
            IDataStore destinationFileInfo = sourceFileInfo.Container.FileItemInfo("David Copperfield-txt.axx");

            EncryptionParameters encryptionParameters = new EncryptionParameters(new V1Aes128CryptoFactory().CryptoId, new Passphrase("allan"));

            New<AxCryptFile>().EncryptFoldersUniqueWithBackupAndWipe(new IDataContainer[] { sourceFolderInfo }, encryptionParameters, new ProgressContext());

            Assert.That(sourceFileInfo.IsAvailable, Is.False, "The source should be wiped.");
            Assert.That(destinationFileInfo.IsAvailable, Is.True, "The destination should be created and exist now.");
        }

        [Test]
        public void TestEncryptFilesUniqueWithBackupAndWipeWithCollision()
        {
            IDataStore sourceFileInfo = New<IDataStore>(_davidCopperfieldTxtPath);
            sourceFileInfo.Container.CreateFolder();
            IDataContainer sourceFolderInfo = sourceFileInfo.Container;
            sourceFolderInfo.CreateNewFile("David Copperfield-txt.axx");
            IDataStore alternateDestinationFileInfo = sourceFolderInfo.FileItemInfo("David Copperfield-txt.1.axx");
            Assert.That(alternateDestinationFileInfo.IsAvailable, Is.False, "The destination should not be created and exist yet.");

            EncryptionParameters encryptionParameters = new EncryptionParameters(new V2Aes128CryptoFactory().CryptoId, new Passphrase("allan"));

            New<AxCryptFile>().EncryptFoldersUniqueWithBackupAndWipe(new IDataContainer[] { sourceFolderInfo }, encryptionParameters, new ProgressContext());

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
            IDataStore fileInfo = New<IDataStore>(filePath);

            using (FileLock fileLock = FileLock.Acquire(fileInfo))
            {
                Assert.DoesNotThrow((TestDelegate)(() => { New<AxCryptFile>().Wipe(fileLock, progress); }));
            }
            Assert.That(!progressed, "There should be no progress-notification since nothing should happen.");
        }

        [Test]
        public void TestWipeWithDelayedUntilDoneCancel()
        {
            IDataStore fileInfo = New<IDataStore>(_davidCopperfieldTxtPath);

            IProgressContext progress = new CancelProgressContext(new ProgressContext(TimeSpan.Zero));
            progress.Progressing += (object sender, ProgressEventArgs e) =>
            {
                ((IProgressContext)sender).Cancel = true;
            };
            using (FileLock fileLock = FileLock.Acquire(fileInfo))
            {
                Assert.Throws<OperationCanceledException>((TestDelegate)(() => { New<AxCryptFile>().Wipe(fileLock, progress); }));
            }
            Assert.That(!fileInfo.IsAvailable, "The file should be completely wiped, even if canceled at start.");
        }

        [Test]
        public void TestDocumentNullArguments()
        {
            IDataStore nullSourceFile = null;
            LogOnIdentity nullPassphrase = null;
            IProgressContext nullProgress = null;

            IDataStore sourceFile = New<IDataStore>(@"C:\Folder\File.txt");
            LogOnIdentity passphrase = new LogOnIdentity("allan");
            IProgressContext progress = new ProgressContext();

            IAxCryptDocument document = null;
            Assert.Throws<ArgumentNullException>(() => document = New<AxCryptFile>().Document(nullSourceFile, passphrase, progress));
            Assert.Throws<ArgumentNullException>(() => document = New<AxCryptFile>().Document(sourceFile, nullPassphrase, progress));
            Assert.Throws<ArgumentNullException>(() => document = New<AxCryptFile>().Document(sourceFile, passphrase, nullProgress));
            Assert.That(document, Is.Null);
        }
    }
}