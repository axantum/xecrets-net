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
            FakeRuntimeFileInfo.AddFile(@"c:\test-txt.axx", FakeRuntimeFileInfo.TestDate6Utc, new MemoryStream());
            FakeRuntimeFileInfo.AddFile(@"c:\decrypted test.txt", new MemoryStream());
        }

        [TestFixtureTearDownAttribute]
        public static void TeardownFixture()
        {
            Environment.Current = _environment;
            FakeRuntimeFileInfo.ClearFiles();
        }

        [Test]
        public static void TestEncryptDecrypt()
        {
            AxCryptFile.Encrypt(Environment.Current.FileInfo(@"c:\test.txt"), Environment.Current.FileInfo(@"c:\test-txt.axx"), new Passphrase("axcrypt"));
            using (AxCryptDocument document = AxCryptFile.Document(Environment.Current.FileInfo(@"c:\test-txt.axx"), new Passphrase("axcrypt")))
            {
                Assert.That(document.PassphraseIsValid, Is.True, "The passphrase should be ok.");
                Assert.That(document.DocumentHeaders.FileName, Is.EqualTo("test.txt"), "Unexpected file name in headers.");
                Assert.That(document.DocumentHeaders.CreationTimeUtc, Is.EqualTo(FakeRuntimeFileInfo.TestDate1Utc));
                Assert.That(document.DocumentHeaders.LastAccessTimeUtc, Is.EqualTo(FakeRuntimeFileInfo.TestDate2Utc));
                Assert.That(document.DocumentHeaders.LastWriteTimeUtc, Is.EqualTo(FakeRuntimeFileInfo.TestDate3Utc));
                IRuntimeFileInfo decryptedFileInfo = Environment.Current.FileInfo(@"c:\decrypted test.txt");
                AxCryptFile.Decrypt(document, decryptedFileInfo);
                using (Stream decryptedStream = decryptedFileInfo.OpenRead())
                {
                    string decrypted = new StreamReader(decryptedStream, Encoding.UTF8).ReadToEnd();
                    Assert.That(decrypted, Is.EqualTo("This is a short file"));
                }
            }
        }
    }
}