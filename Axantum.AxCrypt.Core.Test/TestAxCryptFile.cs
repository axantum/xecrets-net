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

using System;
using System.IO;
using System.Text;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestAxCryptFile
    {
        private static readonly string _rootPath = Path.GetPathRoot(Environment.CurrentDirectory);
        private static readonly string _testTextPath = Path.Combine(_rootPath, "test.txt");
        private static readonly string _davidCopperfieldTxtPath = _rootPath.PathCombine("Users", "AxCrypt", "David Copperfield.txt");
        private static readonly string _uncompressedAxxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Uncompressed.axx");
        private static readonly string _helloWorldAxxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HelloWorld.axx");

        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();

            FakeRuntimeFileInfo.AddFile(_testTextPath, FakeRuntimeFileInfo.TestDate1Utc, FakeRuntimeFileInfo.TestDate2Utc, FakeRuntimeFileInfo.TestDate3Utc, FakeRuntimeFileInfo.ExpandableMemoryStream(Encoding.UTF8.GetBytes("This is a short file")));
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
            IRuntimeFileInfo sourceFileInfo = Factory.New<IRuntimeFileInfo>(_testTextPath);
            IRuntimeFileInfo destinationFileInfo = sourceFileInfo.CreateEncryptedName();
            AxCryptDocument document = new AxCryptDocument(new AesKey(128));
            IRuntimeFileInfo decryptedFileInfo = Factory.New<IRuntimeFileInfo>(Path.Combine(_rootPath, "decrypted test.txt"));

            AxCryptDocument nullDocument = null;
            IRuntimeFileInfo nullFileInfo = null;
            AesKey nullKey = null;
            ProgressContext nullProgress = null;
            Passphrase nullPassphrase = null;
            Stream nullStream = null;
            string nullString = null;
            Action<Stream> nullStreamAction = null;

            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().Encrypt(nullFileInfo, destinationFileInfo, new Passphrase("axcrypt"), AxCryptOptions.EncryptWithCompression, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().Encrypt(sourceFileInfo, nullFileInfo, new Passphrase("axcrypt"), AxCryptOptions.EncryptWithCompression, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().Encrypt(sourceFileInfo, destinationFileInfo, nullPassphrase, AxCryptOptions.EncryptWithCompression, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().Encrypt(sourceFileInfo, destinationFileInfo, new Passphrase("axcrypt"), AxCryptOptions.EncryptWithCompression, nullProgress); });

            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Encrypt(nullFileInfo, new MemoryStream(), new AesKey(128), AxCryptOptions.None, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Encrypt(sourceFileInfo, nullStream, new AesKey(128), AxCryptOptions.None, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Encrypt(sourceFileInfo, new MemoryStream(), nullKey, AxCryptOptions.None, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Encrypt(sourceFileInfo, new MemoryStream(), new AesKey(128), AxCryptOptions.None, nullProgress); });

            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().Decrypt(nullDocument, decryptedFileInfo, AxCryptOptions.SetFileTimes, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().Decrypt(document, nullFileInfo, AxCryptOptions.SetFileTimes, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().Decrypt(document, decryptedFileInfo, AxCryptOptions.SetFileTimes, nullProgress); });

            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().Decrypt(nullFileInfo, decryptedFileInfo, new AesKey(128), AxCryptOptions.SetFileTimes, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().Decrypt(sourceFileInfo, nullFileInfo, new AesKey(128), AxCryptOptions.SetFileTimes, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().Decrypt(sourceFileInfo, decryptedFileInfo, nullKey, AxCryptOptions.SetFileTimes, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().Decrypt(sourceFileInfo, decryptedFileInfo, new AesKey(128), AxCryptOptions.SetFileTimes, nullProgress); });

            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().Decrypt(nullFileInfo, Path.Combine(_rootPath, "Directory"), new AesKey(128), AxCryptOptions.SetFileTimes, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().Decrypt(sourceFileInfo, nullString, new AesKey(128), AxCryptOptions.SetFileTimes, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().Decrypt(sourceFileInfo, Path.Combine(_rootPath, "Directory"), nullKey, AxCryptOptions.SetFileTimes, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().Decrypt(sourceFileInfo, Path.Combine(_rootPath, "Directory"), new AesKey(128), AxCryptOptions.SetFileTimes, nullProgress); });

            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().Document(nullFileInfo, new AesKey(128), new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().Document(sourceFileInfo, nullKey, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().Document(sourceFileInfo, new AesKey(128), nullProgress); });

            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().WriteToFileWithBackup(null, (Stream stream) => { }, new ProgressContext()); });
            IRuntimeFileInfo fileInfo = Factory.New<IRuntimeFileInfo>(_testTextPath);
            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().WriteToFileWithBackup(fileInfo, nullStreamAction, new ProgressContext()); });

            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.MakeAxCryptFileName(nullFileInfo); });
            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().Wipe(nullFileInfo, new ProgressContext()); });
        }

        [Test]
        public static void TestSmallEncryptDecrypt()
        {
            IRuntimeFileInfo sourceFileInfo = Factory.New<IRuntimeFileInfo>(_testTextPath);
            IRuntimeFileInfo destinationFileInfo = sourceFileInfo.CreateEncryptedName();
            Assert.That(destinationFileInfo.Name, Is.EqualTo("test-txt.axx"), "Wrong encrypted file name based on the plain text file name.");
            Factory.New<AxCryptFile>().Encrypt(sourceFileInfo, destinationFileInfo, new Passphrase("axcrypt"), AxCryptOptions.EncryptWithCompression, new ProgressContext());
            using (AxCryptDocument document = Factory.New<AxCryptFile>().Document(destinationFileInfo, new Passphrase("axcrypt").DerivedPassphrase, new ProgressContext()))
            {
                Assert.That(document.PassphraseIsValid, Is.True, "The passphrase should be ok.");
                Assert.That(document.DocumentHeaders.FileName, Is.EqualTo("test.txt"), "Unexpected file name in headers.");
                Assert.That(document.DocumentHeaders.CreationTimeUtc, Is.EqualTo(FakeRuntimeFileInfo.TestDate1Utc));
                Assert.That(document.DocumentHeaders.LastAccessTimeUtc, Is.EqualTo(FakeRuntimeFileInfo.TestDate2Utc));
                Assert.That(document.DocumentHeaders.LastWriteTimeUtc, Is.EqualTo(FakeRuntimeFileInfo.TestDate3Utc));
                IRuntimeFileInfo decryptedFileInfo = Factory.New<IRuntimeFileInfo>(Path.Combine(_rootPath, "decrypted test.txt"));
                Factory.New<AxCryptFile>().Decrypt(document, decryptedFileInfo, AxCryptOptions.SetFileTimes, new ProgressContext());
                using (Stream decryptedStream = decryptedFileInfo.OpenRead())
                {
                    string decrypted = new StreamReader(decryptedStream, Encoding.UTF8).ReadToEnd();
                    Assert.That(decrypted, Is.EqualTo("This is a short file"));
                }
                Assert.That(decryptedFileInfo.CreationTimeUtc, Is.EqualTo(document.DocumentHeaders.CreationTimeUtc), "We're expecting file times to be set as the original from the headers.");
                Assert.That(decryptedFileInfo.LastAccessTimeUtc, Is.EqualTo(document.DocumentHeaders.LastAccessTimeUtc), "We're expecting file times to be set as the original from the headers.");
                Assert.That(decryptedFileInfo.LastWriteTimeUtc, Is.EqualTo(document.DocumentHeaders.LastWriteTimeUtc), "We're expecting file times to be set as the original from the headers.");
            }
        }

        [Test]
        public static void TestEncryptToStream()
        {
            IRuntimeFileInfo sourceFileInfo = Factory.New<IRuntimeFileInfo>(_testTextPath);
            IRuntimeFileInfo destinationFileInfo = sourceFileInfo.CreateEncryptedName();
            Assert.That(destinationFileInfo.Name, Is.EqualTo("test-txt.axx"), "Wrong encrypted file name based on the plain text file name.");
            using (Stream destinationStream = destinationFileInfo.OpenWrite())
            {
                AxCryptFile.Encrypt(sourceFileInfo, destinationStream, new Passphrase("axcrypt").DerivedPassphrase, AxCryptOptions.EncryptWithCompression, new ProgressContext());
            }

            using (AxCryptDocument document = Factory.New<AxCryptFile>().Document(destinationFileInfo, new Passphrase("axcrypt").DerivedPassphrase, new ProgressContext()))
            {
                Assert.That(document.PassphraseIsValid, Is.True, "The passphrase should be ok.");
            }
        }

        [Test]
        public static void TestDecryptToDestinationDirectory()
        {
            IRuntimeFileInfo sourceFileInfo = Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath);
            string destinationDirectory = Path.Combine(_rootPath, "Encrypted");

            string destinationFileName = Factory.New<AxCryptFile>().Decrypt(sourceFileInfo, destinationDirectory, new Passphrase("a").DerivedPassphrase, AxCryptOptions.None, new ProgressContext());
            Assert.That(destinationFileName, Is.EqualTo("HelloWorld-Key-a.txt"), "The correct filename should be returned from decryption.");
        }

        [Test]
        public static void TestDecryptToDestinationDirectoryWithWrongPassphrase()
        {
            IRuntimeFileInfo sourceFileInfo = Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath);
            string destinationDirectory = Path.Combine(_rootPath, "Encrypted");

            string destinationFileName = Factory.New<AxCryptFile>().Decrypt(sourceFileInfo, destinationDirectory, new Passphrase("Wrong Passphrase").DerivedPassphrase, AxCryptOptions.None, new ProgressContext());
            Assert.That(destinationFileName, Is.Null, "When the wrong passphrase is given, the returned file name should be null to signal this.");
        }

        [Test]
        public static void TestDecryptWithCancel()
        {
            IRuntimeFileInfo sourceFileInfo = Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath);
            Passphrase passphrase = new Passphrase("a");
            using (AxCryptDocument document = new AxCryptDocument(passphrase.DerivedPassphrase))
            {
                using (Stream sourceStream = sourceFileInfo.OpenRead())
                {
                    bool keyIsOk = document.Load(sourceStream);
                    Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                    IRuntimeFileInfo destinationInfo = Factory.New<IRuntimeFileInfo>(_rootPath.PathCombine("Destination", "Decrypted.txt"));

                    IProgressContext progress = new CancelProgressContext(new ProgressContext(TimeSpan.Zero));
                    progress.Progressing += (object sender, ProgressEventArgs e) =>
                    {
                        progress.Cancel = true;
                    };

                    Assert.Throws<OperationCanceledException>(() => { Factory.New<AxCryptFile>().Decrypt(document, destinationInfo, AxCryptOptions.None, progress); });
                }
            }
        }

        [Test]
        public static void TestLargeEncryptDecrypt()
        {
            string sourceFullName = _davidCopperfieldTxtPath;

            IRuntimeFileInfo sourceRuntimeFileInfo = Factory.New<IRuntimeFileInfo>(sourceFullName);
            IRuntimeFileInfo destinationRuntimeFileInfo = sourceRuntimeFileInfo.CreateEncryptedName();
            Passphrase passphrase = new Passphrase("laDabled@tAmeopot33");

            Factory.New<AxCryptFile>().Encrypt(sourceRuntimeFileInfo, destinationRuntimeFileInfo, passphrase, AxCryptOptions.SetFileTimes | AxCryptOptions.EncryptWithCompression, new ProgressContext());

            Assert.That(destinationRuntimeFileInfo.CreationTimeUtc, Is.EqualTo(sourceRuntimeFileInfo.CreationTimeUtc), "We're expecting file times to be set as the original from the headers.");
            Assert.That(destinationRuntimeFileInfo.LastAccessTimeUtc, Is.EqualTo(sourceRuntimeFileInfo.LastAccessTimeUtc), "We're expecting file times to be set as the original from the headers.");
            Assert.That(destinationRuntimeFileInfo.LastWriteTimeUtc, Is.EqualTo(sourceRuntimeFileInfo.LastWriteTimeUtc), "We're expecting file times to be set as the original from the headers.");

            DirectoryInfo decryptedDirectoryInfo = new DirectoryInfo(_rootPath.PathCombine("Destination", "Decrypted.txt"));
            string decryptedFullName = Path.Combine(decryptedDirectoryInfo.FullName, "David Copperfield.txt");
            IRuntimeFileInfo decryptedRuntimeFileInfo = Factory.New<IRuntimeFileInfo>(decryptedFullName);

            Factory.New<AxCryptFile>().Decrypt(destinationRuntimeFileInfo, decryptedRuntimeFileInfo, passphrase.DerivedPassphrase, AxCryptOptions.SetFileTimes, new ProgressContext());

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
        public static void TestInvalidPassphrase()
        {
            IRuntimeFileInfo sourceFileInfo = Factory.New<IRuntimeFileInfo>(_testTextPath);
            IRuntimeFileInfo encryptedFileInfo = sourceFileInfo.CreateEncryptedName();
            Assert.That(encryptedFileInfo.Name, Is.EqualTo("test-txt.axx"), "Wrong encrypted file name based on the plain text file name.");
            Factory.New<AxCryptFile>().Encrypt(sourceFileInfo, encryptedFileInfo, new Passphrase("axcrypt"), AxCryptOptions.EncryptWithCompression, new ProgressContext());

            IRuntimeFileInfo decryptedFileInfo = Factory.New<IRuntimeFileInfo>(Path.Combine(_rootPath, "decrypted.txt"));
            bool isPassphraseOk = Factory.New<AxCryptFile>().Decrypt(encryptedFileInfo, decryptedFileInfo, new Passphrase("wrong").DerivedPassphrase, AxCryptOptions.None, new ProgressContext());
            Assert.That(isPassphraseOk, Is.False, "The passphrase is wrong and should be wrong!");
        }

        [Test]
        public static void TestUncompressedEncryptedDecryptAxCrypt17()
        {
            IRuntimeFileInfo sourceRuntimeFileInfo = Factory.New<IRuntimeFileInfo>(_uncompressedAxxPath);
            IRuntimeFileInfo destinationRuntimeFileInfo = Factory.New<IRuntimeFileInfo>(Path.Combine(Path.GetDirectoryName(_uncompressedAxxPath), "Uncompressed.zip"));
            Passphrase passphrase = new Passphrase("Uncompressable");
            using (AxCryptDocument document = new AxCryptDocument(passphrase.DerivedPassphrase))
            {
                bool isOk = document.Load(sourceRuntimeFileInfo.OpenRead());
                Assert.That(isOk, Is.True, "The document should load ok.");
                Factory.New<AxCryptFile>().Decrypt(document, destinationRuntimeFileInfo, AxCryptOptions.None, new ProgressContext());
                Assert.That(document.DocumentHeaders.UncompressedLength, Is.EqualTo(0), "Since the data is not compressed, there should not be a CompressionInfo, but in 1.x there is, with value zero.");
            }
        }

        [Test]
        public static void TestWriteToFileWithBackup()
        {
            string destinationFilePath = _rootPath.PathCombine("Written", "File.txt");
            using (MemoryStream inputStream = FakeRuntimeFileInfo.ExpandableMemoryStream(Encoding.UTF8.GetBytes("A string with some text")))
            {
                IRuntimeFileInfo destinationFileInfo = Factory.New<IRuntimeFileInfo>(destinationFilePath);
                Factory.New<AxCryptFile>().WriteToFileWithBackup(destinationFileInfo, (Stream stream) => { inputStream.CopyTo(stream, 4096); }, new ProgressContext());
                using (TextReader read = new StreamReader(destinationFileInfo.OpenRead()))
                {
                    string readString = read.ReadToEnd();
                    Assert.That(readString, Is.EqualTo("A string with some text"), "Where expecting the same string to be read back.");
                }
            }
        }

        [Test]
        public static void TestWriteToFileWithBackupWithCancel()
        {
            IRuntimeFileInfo destinationFileInfo = Factory.New<IRuntimeFileInfo>(_rootPath.PathCombine("Written", "File.txt"));
            using (MemoryStream inputStream = FakeRuntimeFileInfo.ExpandableMemoryStream(Encoding.UTF8.GetBytes("A string with some text")))
            {
                Assert.Throws<OperationCanceledException>(() => { Factory.New<AxCryptFile>().WriteToFileWithBackup(destinationFileInfo, (Stream stream) => { throw new OperationCanceledException(); }, new ProgressContext()); });
                string tempFilePath = _rootPath.PathCombine("Written", "File.bak");
                IRuntimeFileInfo tempFileInfo = Factory.New<IRuntimeFileInfo>(tempFilePath);
                Assert.That(tempFileInfo.Exists, Is.False, "The .bak file should be removed.");
            }
        }

        [Test]
        public static void TestWriteToFileWithBackupWhenDestinationExists()
        {
            string destinationFilePath = _rootPath.PathCombine("Written", "AnExistingFile.txt");
            IRuntimeFileInfo destinationFileInfo = Factory.New<IRuntimeFileInfo>(destinationFilePath);
            IRuntimeFileInfo bakFileInfo = Factory.New<IRuntimeFileInfo>(_rootPath.PathCombine("Written", "AnExistingFile.bak"));
            Assert.That(bakFileInfo.Exists, Is.False, "The file should not exist to start with.");
            using (Stream writeStream = destinationFileInfo.OpenWrite())
            {
                byte[] bytes = Encoding.UTF8.GetBytes("A string");
                writeStream.Write(bytes, 0, bytes.Length);
            }

            using (MemoryStream inputStream = FakeRuntimeFileInfo.ExpandableMemoryStream(Encoding.UTF8.GetBytes("A string with some text")))
            {
                Factory.New<AxCryptFile>().WriteToFileWithBackup(destinationFileInfo, (Stream stream) => { inputStream.CopyTo(stream, 4096); }, new ProgressContext());
                using (TextReader read = new StreamReader(destinationFileInfo.OpenRead()))
                {
                    string readString = read.ReadToEnd();
                    Assert.That(readString, Is.EqualTo("A string with some text"), "Where expecting the same string to be read back.");
                }
            }
            Assert.That(bakFileInfo.Exists, Is.False, "The file should not exist afterwards either.");
        }

        [Test]
        public static void TestMakeAxCryptFileName()
        {
            string testFile = _rootPath.PathCombine("Directory", "file.txt");
            string axxFile = _rootPath.PathCombine("Directory", "file-txt.axx");
            string madeName = AxCryptFile.MakeAxCryptFileName(Factory.New<IRuntimeFileInfo>(testFile));
            Assert.That(madeName, Is.EqualTo(axxFile), "The AxCrypt version of the name is unexpected.");
        }

        [Test]
        public static void TestWipe()
        {
            string testFile = _rootPath.PathCombine("Folder", "file-to-be-wiped.txt");
            IRuntimeFileInfo fileInfo = Factory.New<IRuntimeFileInfo>(testFile);
            using (Stream writeStream = fileInfo.OpenWrite())
            {
            }
            Assert.That(fileInfo.Exists, "Now it should exist.");
            Factory.New<AxCryptFile>().Wipe(fileInfo, new ProgressContext());
            Assert.That(!fileInfo.Exists, "And now it should not exist after wiping.");
        }

        [Test]
        public static void TestEncryptFileWithBackupFileInfoAndWipeNullArguments()
        {
            string sourceFilePath = _davidCopperfieldTxtPath;
            string destinationFilePath = Path.Combine(Path.GetDirectoryName(sourceFilePath), "David Copperfield-txt.axx");

            IRuntimeFileInfo sourceFileInfo = Factory.New<IRuntimeFileInfo>(sourceFilePath);
            IRuntimeFileInfo destinationFileInfo = Factory.New<IRuntimeFileInfo>(destinationFilePath);
            IRuntimeFileInfo nullFileInfo = null;

            AesKey key = new AesKey(128);
            AesKey nullKey = null;

            ProgressContext progress = new ProgressContext();
            ProgressContext nullProgress = null;

            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().EncryptFileWithBackupAndWipe(nullFileInfo, destinationFileInfo, key, progress); });
            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().EncryptFileWithBackupAndWipe(sourceFileInfo, nullFileInfo, key, progress); });
            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().EncryptFileWithBackupAndWipe(sourceFileInfo, destinationFileInfo, nullKey, progress); });
            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().EncryptFileWithBackupAndWipe(sourceFileInfo, destinationFileInfo, key, nullProgress); });
        }

        [Test]
        public static void TestEncryptFileWithBackupAndWipeFileInfo()
        {
            string sourceFilePath = _davidCopperfieldTxtPath;
            string destinationFilePath = Path.Combine(Path.GetDirectoryName(sourceFilePath), "David Copperfield-txt.axx");

            IRuntimeFileInfo sourceFileInfo = Factory.New<IRuntimeFileInfo>(sourceFilePath);
            IRuntimeFileInfo destinationFileInfo = Factory.New<IRuntimeFileInfo>(destinationFilePath);

            AesKey key = new AesKey(128);

            ProgressContext progress = new ProgressContext();

            Factory.New<AxCryptFile>().EncryptFileWithBackupAndWipe(sourceFileInfo, destinationFileInfo, key, progress);

            Assert.That(sourceFileInfo.Exists, Is.False, "The source should be wiped.");
            Assert.That(destinationFileInfo.Exists, Is.True, "The destination should be created and exist now.");
        }

        [Test]
        public static void TestEncryptFileWithBackupFileNameAndWipeNullArguments()
        {
            string sourceFilePath = _davidCopperfieldTxtPath;
            string destinationFilePath = Path.Combine(Path.GetDirectoryName(sourceFilePath), "David Copperfield-txt.axx");

            string nullFileName = null;

            AesKey key = new AesKey(128);
            AesKey nullKey = null;

            ProgressContext progress = new ProgressContext();
            ProgressContext nullProgress = null;

            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().EncryptFileWithBackupAndWipe(nullFileName, destinationFilePath, key, progress); });
            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().EncryptFileWithBackupAndWipe(sourceFilePath, nullFileName, key, progress); });
            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().EncryptFileWithBackupAndWipe(sourceFilePath, destinationFilePath, nullKey, progress); });
            Assert.Throws<ArgumentNullException>(() => { Factory.New<AxCryptFile>().EncryptFileWithBackupAndWipe(sourceFilePath, destinationFilePath, key, nullProgress); });
        }

        [Test]
        public static void TestEncryptFileWithBackupAndWipeFileName()
        {
            string sourceFilePath = _davidCopperfieldTxtPath;
            string destinationFilePath = Path.Combine(Path.GetDirectoryName(sourceFilePath), "David Copperfield-txt.axx");

            AesKey key = new AesKey(128);
            ProgressContext progress = new ProgressContext();

            Factory.New<AxCryptFile>().EncryptFileWithBackupAndWipe(sourceFilePath, destinationFilePath, key, progress);

            IRuntimeFileInfo sourceFileInfo = Factory.New<IRuntimeFileInfo>(sourceFilePath);
            IRuntimeFileInfo destinationFileInfo = Factory.New<IRuntimeFileInfo>(destinationFilePath);
            Assert.That(sourceFileInfo.Exists, Is.False, "The source should be wiped.");
            Assert.That(destinationFileInfo.Exists, Is.True, "The destination should be created and exist now.");
        }

        [Test]
        public static void TestDecryptFileUniqueWithWipeOfOriginal()
        {
            IRuntimeFileInfo sourceFileInfo = Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath);
            IRuntimeFileInfo destinationFileInfo = Factory.New<IRuntimeFileInfo>(Path.Combine(Path.GetDirectoryName(_helloWorldAxxPath), "HelloWorld-Key-a.txt"));
            Passphrase passphrase = new Passphrase("a");

            Assert.That(sourceFileInfo.Exists, Is.True, "The source should exist.");
            Assert.That(destinationFileInfo.Exists, Is.False, "The source should not exist yet.");

            Factory.New<AxCryptFile>().DecryptFileUniqueWithWipeOfOriginal(sourceFileInfo, passphrase.DerivedPassphrase, new ProgressContext());

            Assert.That(sourceFileInfo.Exists, Is.False, "The source should be wiped.");
            Assert.That(destinationFileInfo.Exists, Is.True, "The destination should be created and exist now.");
        }

        [Test]
        public static void TestDecryptFilesUniqueWithWipeOfOriginal()
        {
            Factory.Instance.Singleton<ParallelFileOperation>(() => new ParallelFileOperation());
            Factory.Instance.Singleton<IProgressBackground>(() => new FakeProgressBackground());
            Factory.Instance.Singleton<IUIThread>(() => new FakeUIThread());
            IRuntimeFileInfo sourceFileInfo = Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath);
            sourceFileInfo.CreateFolder();
            IRuntimeFileInfo sourceFolderInfo = Factory.New<IRuntimeFileInfo>(Path.GetDirectoryName(sourceFileInfo.FullName));
            IRuntimeFileInfo destinationFileInfo = Factory.New<IRuntimeFileInfo>(Path.Combine(Path.GetDirectoryName(_helloWorldAxxPath), "HelloWorld-Key-a.txt"));
            Passphrase passphrase = new Passphrase("a");

            Assert.That(sourceFileInfo.Exists, Is.True, "The source should exist.");
            Assert.That(destinationFileInfo.Exists, Is.False, "The source should not exist yet.");

            Factory.New<AxCryptFile>().DecryptFilesInsideFolderUniqueWithWipeOfOriginal(sourceFolderInfo, passphrase.DerivedPassphrase, new ProgressContext());

            Assert.That(sourceFileInfo.Exists, Is.False, "The source should be wiped.");
            Assert.That(destinationFileInfo.Exists, Is.True, "The destination should be created and exist now.");
        }

        [Test]
        public static void TestEncryptFileUniqueWithBackupAndWipeWithNoCollision()
        {
            IRuntimeFileInfo sourceFileInfo = Factory.New<IRuntimeFileInfo>(_davidCopperfieldTxtPath);
            sourceFileInfo.CreateFolder();
            IRuntimeFileInfo destinationFileInfo = Factory.New<IRuntimeFileInfo>(Path.Combine(Path.GetDirectoryName(_davidCopperfieldTxtPath), "David Copperfield-txt.axx"));

            Passphrase passphrase = new Passphrase("allan");

            Factory.New<AxCryptFile>().EncryptFileUniqueWithBackupAndWipe(sourceFileInfo, passphrase.DerivedPassphrase, new ProgressContext());

            Assert.That(sourceFileInfo.Exists, Is.False, "The source should be wiped.");
            Assert.That(destinationFileInfo.Exists, Is.True, "The destination should be created and exist now.");
        }

        [Test]
        public static void TestEncryptFileUniqueWithBackupAndWipeWithCollision()
        {
            IRuntimeFileInfo sourceFileInfo = Factory.New<IRuntimeFileInfo>(_davidCopperfieldTxtPath);
            sourceFileInfo.CreateFolder();
            IRuntimeFileInfo destinationFileInfo = Factory.New<IRuntimeFileInfo>(Path.Combine(Path.GetDirectoryName(_davidCopperfieldTxtPath), "David Copperfield-txt.axx"));
            destinationFileInfo.CreateNewFile();

            IRuntimeFileInfo alternateDestinationFileInfo = Factory.New<IRuntimeFileInfo>(Path.Combine(Path.GetDirectoryName(_davidCopperfieldTxtPath), "David Copperfield-txt.1.axx"));

            Passphrase passphrase = new Passphrase("allan");

            Factory.New<AxCryptFile>().EncryptFileUniqueWithBackupAndWipe(sourceFileInfo, passphrase.DerivedPassphrase, new ProgressContext());

            Assert.That(sourceFileInfo.Exists, Is.False, "The source should be wiped.");
            Assert.That(alternateDestinationFileInfo.Exists, Is.True, "The destination should be created and exist now.");
        }

        [Test]
        public static void TestEncryptFilesUniqueWithBackupAndWipeWithNoCollision()
        {
            IRuntimeFileInfo sourceFileInfo = Factory.New<IRuntimeFileInfo>(_davidCopperfieldTxtPath);
            sourceFileInfo.CreateFolder();
            IRuntimeFileInfo sourceFolderInfo = Factory.New<IRuntimeFileInfo>(Path.GetDirectoryName(sourceFileInfo.FullName));
            IRuntimeFileInfo destinationFileInfo = Factory.New<IRuntimeFileInfo>(Path.Combine(Path.GetDirectoryName(_davidCopperfieldTxtPath), "David Copperfield-txt.axx"));

            Passphrase passphrase = new Passphrase("allan");

            Factory.New<AxCryptFile>().EncryptFilesUniqueWithBackupAndWipe(new IRuntimeFileInfo[] { sourceFolderInfo }, passphrase.DerivedPassphrase, new ProgressContext());

            Assert.That(sourceFileInfo.Exists, Is.False, "The source should be wiped.");
            Assert.That(destinationFileInfo.Exists, Is.True, "The destination should be created and exist now.");
        }

        [Test]
        public static void TestEncryptFilesUniqueWithBackupAndWipeWithCollision()
        {
            IRuntimeFileInfo sourceFileInfo = Factory.New<IRuntimeFileInfo>(_davidCopperfieldTxtPath);
            sourceFileInfo.CreateFolder();
            IRuntimeFileInfo sourceFolderInfo = Factory.New<IRuntimeFileInfo>(Path.GetDirectoryName(sourceFileInfo.FullName));
            IRuntimeFileInfo destinationFileInfo = Factory.New<IRuntimeFileInfo>(Path.Combine(Path.GetDirectoryName(_davidCopperfieldTxtPath), "David Copperfield-txt.axx"));
            destinationFileInfo.CreateNewFile();

            IRuntimeFileInfo alternateDestinationFileInfo = Factory.New<IRuntimeFileInfo>(Path.Combine(Path.GetDirectoryName(_davidCopperfieldTxtPath), "David Copperfield-txt.1.axx"));

            Passphrase passphrase = new Passphrase("allan");

            Factory.New<AxCryptFile>().EncryptFilesUniqueWithBackupAndWipe(new IRuntimeFileInfo[] { sourceFolderInfo }, passphrase.DerivedPassphrase, new ProgressContext());

            Assert.That(sourceFileInfo.Exists, Is.False, "The source should be wiped.");
            Assert.That(alternateDestinationFileInfo.Exists, Is.True, "The destination should be created and exist now.");
        }

        [Test]
        public static void TestWipeFileDoesNotExist()
        {
            ProgressContext progress = new ProgressContext(TimeSpan.Zero);
            bool progressed = false;
            progress.Progressing += (object sender, ProgressEventArgs e) =>
            {
                progressed = true;
            };

            string filePath = Path.Combine(Path.Combine(_rootPath, "Folder"), "DoesNot.Exist");
            IRuntimeFileInfo fileInfo = Factory.New<IRuntimeFileInfo>(filePath);

            Assert.DoesNotThrow(() => { Factory.New<AxCryptFile>().Wipe(fileInfo, progress); });
            Assert.That(!progressed, "There should be no progress-notification since nothing should happen.");
        }

        [Test]
        public static void TestWipeWithDelayedUntilDoneCancel()
        {
            IRuntimeFileInfo fileInfo = Factory.New<IRuntimeFileInfo>(_davidCopperfieldTxtPath);

            IProgressContext progress = new CancelProgressContext(new ProgressContext(TimeSpan.Zero));
            progress.Progressing += (object sender, ProgressEventArgs e) =>
            {
                ((IProgressContext)sender).Cancel = true;
            };
            Assert.Throws<OperationCanceledException>(() => { Factory.New<AxCryptFile>().Wipe(fileInfo, progress); });
            Assert.That(!fileInfo.Exists, "The file should be completely wiped, even if canceled at start.");
        }
    }
}