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
using Axantum.AxCrypt.Core.Test.Properties;
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
            _environment = Environment.Current;
            Environment.Current = new FakeRuntimeEnvironment();

            FakeRuntimeFileInfo.AddFile(@"c:\test.txt", FakeRuntimeFileInfo.TestDate1Utc, FakeRuntimeFileInfo.TestDate2Utc, FakeRuntimeFileInfo.TestDate3Utc, new MemoryStream(Encoding.UTF8.GetBytes("This is a short file")));
            FakeRuntimeFileInfo.AddFile(@"c:\Users\AxCrypt\David Copperfield.txt", FakeRuntimeFileInfo.TestDate4Utc, FakeRuntimeFileInfo.TestDate5Utc, FakeRuntimeFileInfo.TestDate6Utc, new MemoryStream(Resources.David_Copperfield));
            FakeRuntimeFileInfo.AddFile(@"c:\Documents\Uncompressed.axx", new MemoryStream(Resources.Uncompressable_zip));
        }

        [TestFixtureTearDownAttribute]
        public static void TeardownFixture()
        {
            Environment.Current = _environment;
            FakeRuntimeFileInfo.ClearFiles();
        }

        [Test]
        public static void TestSmallEncryptDecrypt()
        {
            IRuntimeFileInfo sourceFileInfo = Environment.Current.FileInfo(@"c:\test.txt");
            IRuntimeFileInfo destinationFileInfo = sourceFileInfo.CreateEncryptedName();
            Assert.That(destinationFileInfo.Name, Is.EqualTo("test-txt.axx"), "Wrong encrypted file name based on the plain text file name.");
            AxCryptFile.Encrypt(sourceFileInfo, destinationFileInfo, new Passphrase("axcrypt"), AxCryptOptions.EncryptWithCompression);
            using (AxCryptDocument document = AxCryptFile.Document(destinationFileInfo, new Passphrase("axcrypt")))
            {
                Assert.That(document.PassphraseIsValid, Is.True, "The passphrase should be ok.");
                Assert.That(document.DocumentHeaders.FileName, Is.EqualTo("test.txt"), "Unexpected file name in headers.");
                Assert.That(document.DocumentHeaders.CreationTimeUtc, Is.EqualTo(FakeRuntimeFileInfo.TestDate1Utc));
                Assert.That(document.DocumentHeaders.LastAccessTimeUtc, Is.EqualTo(FakeRuntimeFileInfo.TestDate2Utc));
                Assert.That(document.DocumentHeaders.LastWriteTimeUtc, Is.EqualTo(FakeRuntimeFileInfo.TestDate3Utc));
                IRuntimeFileInfo decryptedFileInfo = Environment.Current.FileInfo(@"c:\decrypted test.txt");
                AxCryptFile.Decrypt(document, decryptedFileInfo, AxCryptOptions.SetFileTimes);
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
        public static void TestLargeEncryptDecrypt()
        {
            DirectoryInfo sourceDirectory = new DirectoryInfo(@"C:\Users\AxCrypt");
            FileInfo sourceFileInfo = new FileInfo(Path.Combine(sourceDirectory.FullName, "David Copperfield.txt"));

            IRuntimeFileInfo sourceRuntimeFileInfo = Environment.Current.FileInfo(sourceFileInfo);
            IRuntimeFileInfo destinationRuntimeFileInfo = sourceRuntimeFileInfo.CreateEncryptedName();
            Passphrase passphrase = new Passphrase("laDabled@tAmeopot33");

            AxCryptFile.Encrypt(sourceRuntimeFileInfo, destinationRuntimeFileInfo, passphrase, AxCryptOptions.SetFileTimes | AxCryptOptions.EncryptWithCompression);

            Assert.That(destinationRuntimeFileInfo.CreationTimeUtc, Is.EqualTo(sourceRuntimeFileInfo.CreationTimeUtc), "We're expecting file times to be set as the original from the headers.");
            Assert.That(destinationRuntimeFileInfo.LastAccessTimeUtc, Is.EqualTo(sourceRuntimeFileInfo.LastAccessTimeUtc), "We're expecting file times to be set as the original from the headers.");
            Assert.That(destinationRuntimeFileInfo.LastWriteTimeUtc, Is.EqualTo(sourceRuntimeFileInfo.LastWriteTimeUtc), "We're expecting file times to be set as the original from the headers.");

            DirectoryInfo decryptedDirectoryInfo = new DirectoryInfo(@"C:\Users\AxCrypt\Decrypted");
            FileInfo decryptedFileInfo = new FileInfo(Path.Combine(decryptedDirectoryInfo.FullName, "David Copperfield.txt"));
            IRuntimeFileInfo decryptedRuntimeFileInfo = Environment.Current.FileInfo(decryptedFileInfo);

            AxCryptFile.Decrypt(destinationRuntimeFileInfo, decryptedRuntimeFileInfo, passphrase, AxCryptOptions.SetFileTimes);

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
            IRuntimeFileInfo sourceFileInfo = Environment.Current.FileInfo(@"c:\test.txt");
            IRuntimeFileInfo encryptedFileInfo = sourceFileInfo.CreateEncryptedName();
            Assert.That(encryptedFileInfo.Name, Is.EqualTo("test-txt.axx"), "Wrong encrypted file name based on the plain text file name.");
            AxCryptFile.Encrypt(sourceFileInfo, encryptedFileInfo, new Passphrase("axcrypt"), AxCryptOptions.EncryptWithCompression);

            IRuntimeFileInfo decryptedFileInfo = Environment.Current.FileInfo(@"c:\decrypted.txt");
            bool isPassphraseOk = AxCryptFile.Decrypt(encryptedFileInfo, decryptedFileInfo, new Passphrase("wrong"), AxCryptOptions.None);
            Assert.That(isPassphraseOk, Is.False, "The passphrase is wrong and should be wrong!");
        }

        [Test]
        public static void TestUncompressedEncryptedDecryptAxCrypt17()
        {
            IRuntimeFileInfo sourceRuntimeFileInfo = Environment.Current.FileInfo(@"c:\Documents\Uncompressed.axx");
            IRuntimeFileInfo destinationRuntimeFileInfo = Environment.Current.FileInfo(@"c:\Documents\Uncompressed.zip");
            Passphrase passphrase = new Passphrase("Uncompressable");
            using (AxCryptDocument document = new AxCryptDocument())
            {
                bool isOk = document.Load(sourceRuntimeFileInfo.OpenRead(), passphrase);
                Assert.That(isOk, Is.True, "The document should load ok.");
                AxCryptFile.Decrypt(document, destinationRuntimeFileInfo, AxCryptOptions.None);
                Assert.That(document.DocumentHeaders.UncompressedLength, Is.EqualTo(0), "Since the data is not compressed, there should not be a CompressionInfo, but in 1.x there is, with value zero.");
            }
        }
    }
}