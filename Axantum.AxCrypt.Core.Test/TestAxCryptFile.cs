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
using System.IO;
using System.Text;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.System;
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestAxCryptFile
    {
        private static IRuntimeEnvironment _environment;

        [TestFixtureSetUp]
        public static void SetupFixture()
        {
            _environment = AxCryptEnvironment.Current;
            AxCryptEnvironment.Current = new FakeRuntimeEnvironment();

            FakeRuntimeFileInfo.AddFile(@"c:\test.txt", FakeRuntimeFileInfo.TestDate1Utc, FakeRuntimeFileInfo.TestDate2Utc, FakeRuntimeFileInfo.TestDate3Utc, new MemoryStream(Encoding.UTF8.GetBytes("This is a short file")));
            FakeRuntimeFileInfo.AddFile(@"c:\Users\AxCrypt\David Copperfield.txt", FakeRuntimeFileInfo.TestDate4Utc, FakeRuntimeFileInfo.TestDate5Utc, FakeRuntimeFileInfo.TestDate6Utc, new MemoryStream(Resources.David_Copperfield));
            FakeRuntimeFileInfo.AddFile(@"c:\Documents\Uncompressed.axx", new MemoryStream(Resources.Uncompressable_zip));
            FakeRuntimeFileInfo.AddFile(@"c:\Documents\HelloWorld.axx", new MemoryStream(Resources.HelloWorld_Key_a_txt));
        }

        [TestFixtureTearDownAttribute]
        public static void TeardownFixture()
        {
            AxCryptEnvironment.Current = _environment;
            FakeRuntimeFileInfo.ClearFiles();
        }

        [Test]
        public static void TestInvalidArguments()
        {
            IRuntimeFileInfo sourceFileInfo = AxCryptEnvironment.Current.FileInfo(@"c:\test.txt");
            IRuntimeFileInfo destinationFileInfo = sourceFileInfo.CreateEncryptedName();
            AxCryptDocument document = new AxCryptDocument();
            IRuntimeFileInfo decryptedFileInfo = AxCryptEnvironment.Current.FileInfo(@"c:\decrypted test.txt");

            AxCryptDocument nullDocument = null;
            IRuntimeFileInfo nullFileInfo = null;
            AesKey nullKey = null;
            ProgressContext nullProgress = null;
            Passphrase nullPassphrase = null;
            Stream nullStream = null;
            string nullString = null;
            Action<Stream> nullStreamAction = null;

            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Encrypt(nullFileInfo, destinationFileInfo, new Passphrase("axcrypt"), AxCryptOptions.EncryptWithCompression, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Encrypt(sourceFileInfo, nullFileInfo, new Passphrase("axcrypt"), AxCryptOptions.EncryptWithCompression, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Encrypt(sourceFileInfo, destinationFileInfo, nullPassphrase, AxCryptOptions.EncryptWithCompression, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Encrypt(sourceFileInfo, destinationFileInfo, new Passphrase("axcrypt"), AxCryptOptions.EncryptWithCompression, nullProgress); });

            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Encrypt(nullFileInfo, new MemoryStream(), new AesKey(), AxCryptOptions.None, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Encrypt(sourceFileInfo, nullStream, new AesKey(), AxCryptOptions.None, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Encrypt(sourceFileInfo, new MemoryStream(), nullKey, AxCryptOptions.None, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Encrypt(sourceFileInfo, new MemoryStream(), new AesKey(), AxCryptOptions.None, nullProgress); });

            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Decrypt(nullDocument, decryptedFileInfo, AxCryptOptions.SetFileTimes, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Decrypt(document, nullFileInfo, AxCryptOptions.SetFileTimes, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Decrypt(document, decryptedFileInfo, AxCryptOptions.SetFileTimes, nullProgress); });

            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Decrypt(nullFileInfo, decryptedFileInfo, new AesKey(), AxCryptOptions.SetFileTimes, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Decrypt(sourceFileInfo, nullFileInfo, new AesKey(), AxCryptOptions.SetFileTimes, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Decrypt(sourceFileInfo, decryptedFileInfo, nullKey, AxCryptOptions.SetFileTimes, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Decrypt(sourceFileInfo, decryptedFileInfo, new AesKey(), AxCryptOptions.SetFileTimes, nullProgress); });

            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Decrypt(nullFileInfo, @"c:\Directory", new AesKey(), AxCryptOptions.SetFileTimes, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Decrypt(sourceFileInfo, nullString, new AesKey(), AxCryptOptions.SetFileTimes, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Decrypt(sourceFileInfo, @"c:\Directory", nullKey, AxCryptOptions.SetFileTimes, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Decrypt(sourceFileInfo, @"c:\Directory", new AesKey(), AxCryptOptions.SetFileTimes, nullProgress); });

            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Document(nullFileInfo, new AesKey(), new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Document(sourceFileInfo, nullKey, new ProgressContext()); });
            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Document(sourceFileInfo, new AesKey(), nullProgress); });

            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.WriteToFileWithBackup(null, (Stream stream) => { }); });
            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.WriteToFileWithBackup(@"c:\test.txt", nullStreamAction); });

            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.MakeAxCryptFileName(nullFileInfo); });
            Assert.Throws<ArgumentNullException>(() => { AxCryptFile.Wipe(nullFileInfo); });
        }

        [Test]
        public static void TestSmallEncryptDecrypt()
        {
            IRuntimeFileInfo sourceFileInfo = AxCryptEnvironment.Current.FileInfo(@"c:\test.txt");
            IRuntimeFileInfo destinationFileInfo = sourceFileInfo.CreateEncryptedName();
            Assert.That(destinationFileInfo.Name, Is.EqualTo("test-txt.axx"), "Wrong encrypted file name based on the plain text file name.");
            AxCryptFile.Encrypt(sourceFileInfo, destinationFileInfo, new Passphrase("axcrypt"), AxCryptOptions.EncryptWithCompression, new ProgressContext());
            using (AxCryptDocument document = AxCryptFile.Document(destinationFileInfo, new Passphrase("axcrypt").DerivedPassphrase, new ProgressContext()))
            {
                Assert.That(document.PassphraseIsValid, Is.True, "The passphrase should be ok.");
                Assert.That(document.DocumentHeaders.FileName, Is.EqualTo("test.txt"), "Unexpected file name in headers.");
                Assert.That(document.DocumentHeaders.CreationTimeUtc, Is.EqualTo(FakeRuntimeFileInfo.TestDate1Utc));
                Assert.That(document.DocumentHeaders.LastAccessTimeUtc, Is.EqualTo(FakeRuntimeFileInfo.TestDate2Utc));
                Assert.That(document.DocumentHeaders.LastWriteTimeUtc, Is.EqualTo(FakeRuntimeFileInfo.TestDate3Utc));
                IRuntimeFileInfo decryptedFileInfo = AxCryptEnvironment.Current.FileInfo(@"c:\decrypted test.txt");
                AxCryptFile.Decrypt(document, decryptedFileInfo, AxCryptOptions.SetFileTimes, new ProgressContext());
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
            IRuntimeFileInfo sourceFileInfo = AxCryptEnvironment.Current.FileInfo(@"c:\test.txt");
            IRuntimeFileInfo destinationFileInfo = sourceFileInfo.CreateEncryptedName();
            Assert.That(destinationFileInfo.Name, Is.EqualTo("test-txt.axx"), "Wrong encrypted file name based on the plain text file name.");
            using (Stream destinationStream = destinationFileInfo.OpenWrite())
            {
                AxCryptFile.Encrypt(sourceFileInfo, destinationStream, new Passphrase("axcrypt").DerivedPassphrase, AxCryptOptions.EncryptWithCompression, new ProgressContext());
            }

            using (AxCryptDocument document = AxCryptFile.Document(destinationFileInfo, new Passphrase("axcrypt").DerivedPassphrase, new ProgressContext()))
            {
                Assert.That(document.PassphraseIsValid, Is.True, "The passphrase should be ok.");
            }
        }

        [Test]
        public static void TestDecryptToDestinationDirectory()
        {
            IRuntimeFileInfo sourceFileInfo = AxCryptEnvironment.Current.FileInfo(@"c:\Documents\HelloWorld.axx");
            string destinationDirectory = @"c:\Encrypted";

            string destinationFileName = AxCryptFile.Decrypt(sourceFileInfo, destinationDirectory, new Passphrase("a").DerivedPassphrase, AxCryptOptions.None, new ProgressContext());
            Assert.That(destinationFileName, Is.EqualTo("HelloWorld-Key-a.txt"), "The correct filename should be returned from decryption.");
        }

        [Test]
        public static void TestDecryptToDestinationDirectoryWithWrongPassphrase()
        {
            IRuntimeFileInfo sourceFileInfo = AxCryptEnvironment.Current.FileInfo(@"c:\Documents\HelloWorld.axx");
            string destinationDirectory = @"c:\Encrypted";

            string destinationFileName = AxCryptFile.Decrypt(sourceFileInfo, destinationDirectory, new Passphrase("Wrong Passphrase").DerivedPassphrase, AxCryptOptions.None, new ProgressContext());
            Assert.That(destinationFileName, Is.Null, "When the wrong passphrase is given, the returned file name should be null to signal this.");
        }

        [Test]
        public static void TestDecryptWithCancel()
        {
            IRuntimeFileInfo sourceFileInfo = AxCryptEnvironment.Current.FileInfo(@"c:\Documents\HelloWorld.axx");
            using (AxCryptDocument document = new AxCryptDocument())
            {
                Passphrase passphrase = new Passphrase("a");
                using (Stream sourceStream = sourceFileInfo.OpenRead())
                {
                    bool keyIsOk = document.Load(sourceStream, passphrase.DerivedPassphrase);
                    Assert.That(keyIsOk, Is.True, "The passphrase provided is correct!");
                    IRuntimeFileInfo destinationInfo = AxCryptEnvironment.Current.FileInfo(@"c:\Destination\Decrypted.txt");

                    ProgressContext progress = new ProgressContext(TimeSpan.Zero);
                    progress.Progressing += (object sender, ProgressEventArgs e) =>
                    {
                        throw new OperationCanceledException();
                    };

                    Assert.Throws<OperationCanceledException>(() => { AxCryptFile.Decrypt(document, destinationInfo, AxCryptOptions.None, progress); });
                }
            }
        }

        [Test]
        public static void TestLargeEncryptDecrypt()
        {
            DirectoryInfo sourceDirectory = new DirectoryInfo(@"C:\Users\AxCrypt");
            FileInfo sourceFileInfo = new FileInfo(Path.Combine(sourceDirectory.FullName, "David Copperfield.txt"));

            IRuntimeFileInfo sourceRuntimeFileInfo = AxCryptEnvironment.Current.FileInfo(sourceFileInfo);
            IRuntimeFileInfo destinationRuntimeFileInfo = sourceRuntimeFileInfo.CreateEncryptedName();
            Passphrase passphrase = new Passphrase("laDabled@tAmeopot33");

            AxCryptFile.Encrypt(sourceRuntimeFileInfo, destinationRuntimeFileInfo, passphrase, AxCryptOptions.SetFileTimes | AxCryptOptions.EncryptWithCompression, new ProgressContext());

            Assert.That(destinationRuntimeFileInfo.CreationTimeUtc, Is.EqualTo(sourceRuntimeFileInfo.CreationTimeUtc), "We're expecting file times to be set as the original from the headers.");
            Assert.That(destinationRuntimeFileInfo.LastAccessTimeUtc, Is.EqualTo(sourceRuntimeFileInfo.LastAccessTimeUtc), "We're expecting file times to be set as the original from the headers.");
            Assert.That(destinationRuntimeFileInfo.LastWriteTimeUtc, Is.EqualTo(sourceRuntimeFileInfo.LastWriteTimeUtc), "We're expecting file times to be set as the original from the headers.");

            DirectoryInfo decryptedDirectoryInfo = new DirectoryInfo(@"C:\Users\AxCrypt\Decrypted");
            FileInfo decryptedFileInfo = new FileInfo(Path.Combine(decryptedDirectoryInfo.FullName, "David Copperfield.txt"));
            IRuntimeFileInfo decryptedRuntimeFileInfo = AxCryptEnvironment.Current.FileInfo(decryptedFileInfo);

            AxCryptFile.Decrypt(destinationRuntimeFileInfo, decryptedRuntimeFileInfo, passphrase.DerivedPassphrase, AxCryptOptions.SetFileTimes, new ProgressContext());

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
            IRuntimeFileInfo sourceFileInfo = AxCryptEnvironment.Current.FileInfo(@"c:\test.txt");
            IRuntimeFileInfo encryptedFileInfo = sourceFileInfo.CreateEncryptedName();
            Assert.That(encryptedFileInfo.Name, Is.EqualTo("test-txt.axx"), "Wrong encrypted file name based on the plain text file name.");
            AxCryptFile.Encrypt(sourceFileInfo, encryptedFileInfo, new Passphrase("axcrypt"), AxCryptOptions.EncryptWithCompression, new ProgressContext());

            IRuntimeFileInfo decryptedFileInfo = AxCryptEnvironment.Current.FileInfo(@"c:\decrypted.txt");
            bool isPassphraseOk = AxCryptFile.Decrypt(encryptedFileInfo, decryptedFileInfo, new Passphrase("wrong").DerivedPassphrase, AxCryptOptions.None, new ProgressContext());
            Assert.That(isPassphraseOk, Is.False, "The passphrase is wrong and should be wrong!");
        }

        [Test]
        public static void TestUncompressedEncryptedDecryptAxCrypt17()
        {
            IRuntimeFileInfo sourceRuntimeFileInfo = AxCryptEnvironment.Current.FileInfo(@"c:\Documents\Uncompressed.axx");
            IRuntimeFileInfo destinationRuntimeFileInfo = AxCryptEnvironment.Current.FileInfo(@"c:\Documents\Uncompressed.zip");
            Passphrase passphrase = new Passphrase("Uncompressable");
            using (AxCryptDocument document = new AxCryptDocument())
            {
                bool isOk = document.Load(sourceRuntimeFileInfo.OpenRead(), passphrase.DerivedPassphrase);
                Assert.That(isOk, Is.True, "The document should load ok.");
                AxCryptFile.Decrypt(document, destinationRuntimeFileInfo, AxCryptOptions.None, new ProgressContext());
                Assert.That(document.DocumentHeaders.UncompressedLength, Is.EqualTo(0), "Since the data is not compressed, there should not be a CompressionInfo, but in 1.x there is, with value zero.");
            }
        }

        [Test]
        public static void TestWriteToFileWithBackup()
        {
            string destinationFilePath = @"c:\Written\File.txt";
            using (MemoryStream inputStream = new MemoryStream(Encoding.UTF8.GetBytes("A string with some text")))
            {
                AxCryptFile.WriteToFileWithBackup(destinationFilePath, (Stream stream) => { inputStream.CopyTo(stream); });
                IRuntimeFileInfo destinationFileInfo = AxCryptEnvironment.Current.FileInfo(destinationFilePath);
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
            string destinationFilePath = @"c:\Written\File.txt";
            using (MemoryStream inputStream = new MemoryStream(Encoding.UTF8.GetBytes("A string with some text")))
            {
                Assert.Throws<OperationCanceledException>(() => { AxCryptFile.WriteToFileWithBackup(destinationFilePath, (Stream stream) => { throw new OperationCanceledException(); }); });
                string tempFilePath = @"c:\Written\File.bak";
                IRuntimeFileInfo tempFileInfo = AxCryptEnvironment.Current.FileInfo(tempFilePath);
                Assert.That(tempFileInfo.Exists, Is.False, "The .bak file should be removed.");
            }
        }

        [Test]
        public static void TestWriteToFileWithBackupWhenDestinationExists()
        {
            string destinationFilePath = @"c:\Written\AnExistingFile.txt";
            IRuntimeFileInfo bakFileInfo = AxCryptEnvironment.Current.FileInfo(@"c:\Written\AnExistingFile.bak");
            Assert.That(bakFileInfo.Exists, Is.False, "The file should not exist to start with.");
            using (Stream writeStream = AxCryptEnvironment.Current.FileInfo(destinationFilePath).OpenWrite())
            {
                byte[] bytes = Encoding.UTF8.GetBytes("A string");
                writeStream.Write(bytes, 0, bytes.Length);
            }

            using (MemoryStream inputStream = new MemoryStream(Encoding.UTF8.GetBytes("A string with some text")))
            {
                AxCryptFile.WriteToFileWithBackup(destinationFilePath, (Stream stream) => { inputStream.CopyTo(stream); });
                IRuntimeFileInfo destinationFileInfo = AxCryptEnvironment.Current.FileInfo(destinationFilePath);
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
            string testFile = @"c:\Directory\file.txt";
            string axxFile = @"c:\Directory\file-txt.axx";
            string madeName = AxCryptFile.MakeAxCryptFileName(AxCryptEnvironment.Current.FileInfo(testFile));
            Assert.That(madeName, Is.EqualTo(axxFile), "The AxCrypt version of the name is unexpected.");
        }
    }
}