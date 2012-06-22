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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.System;
using Axantum.AxCrypt.Core.Test.Properties;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestActiveFile
    {
        private static IRuntimeEnvironment _environment;

        [TestFixtureSetUp]
        public static void SetupFixture()
        {
            _environment = AxCryptEnvironment.Current;
            AxCryptEnvironment.Current = new FakeRuntimeEnvironment();

            FakeRuntimeFileInfo.AddFile(@"c:\test.txt", FakeRuntimeFileInfo.TestDate1Utc, FakeRuntimeFileInfo.TestDate2Utc, FakeRuntimeFileInfo.TestDate1Utc, new MemoryStream(Encoding.UTF8.GetBytes("This is a short file")));
            FakeRuntimeFileInfo.AddFile(@"c:\Users\AxCrypt\David Copperfield.txt", FakeRuntimeFileInfo.TestDate4Utc, FakeRuntimeFileInfo.TestDate5Utc, FakeRuntimeFileInfo.TestDate6Utc, new MemoryStream(Resources.David_Copperfield));
            FakeRuntimeFileInfo.AddFile(@"c:\Documents\Uncompressed.axx", new MemoryStream(Resources.Uncompressable_zip));
            FakeRuntimeFileInfo.AddFile(@"c:\Documents\HelloWorld.axx", new MemoryStream(Resources.HelloWorld_Key_a_txt));
        }

        [TestFixtureTearDown]
        public static void TeardownFixture()
        {
            AxCryptEnvironment.Current = _environment;
            FakeRuntimeFileInfo.ClearFiles();
        }

        [Test]
        public static void TestInvalidArguments()
        {
            IRuntimeFileInfo nullFileInfo = null;
            Process nullProcess = null;
            IRuntimeFileInfo decryptedFileInfo = AxCryptEnvironment.Current.FileInfo(@"c:\test.txt");
            IRuntimeFileInfo encryptedFileInfo = AxCryptEnvironment.Current.FileInfo(@"c:\Documents\HelloWorld.axx");
            AesKey key = new AesKey();
            AesKey nullKey = null;
            Process process = new Process();
            ActiveFile nullActiveFile = null;

            ActiveFile originalActiveFile = new ActiveFile(decryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.None, process);
            ActiveFile activeFile;
            Assert.Throws<ArgumentNullException>(() => { activeFile = new ActiveFile(nullActiveFile, key); });
            Assert.Throws<ArgumentNullException>(() => { activeFile = new ActiveFile(originalActiveFile, nullKey); });
            Assert.Throws<ArgumentNullException>(() => { activeFile = new ActiveFile(nullActiveFile, ActiveFileStatus.None, nullProcess); });
            Assert.Throws<ArgumentNullException>(() => { activeFile = new ActiveFile(nullActiveFile, ActiveFileStatus.None); });
            Assert.Throws<ArgumentNullException>(() => { activeFile = new ActiveFile(nullActiveFile, DateTime.MinValue, ActiveFileStatus.None); });
            Assert.Throws<ArgumentNullException>(() => { activeFile = new ActiveFile(nullFileInfo, decryptedFileInfo, new AesKey(), ActiveFileStatus.None, nullProcess); });
            Assert.Throws<ArgumentNullException>(() => { activeFile = new ActiveFile(encryptedFileInfo, nullFileInfo, new AesKey(), ActiveFileStatus.None, nullProcess); });
            Assert.Throws<ArgumentNullException>(() => { activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, nullKey, ActiveFileStatus.None, nullProcess); });
        }

        [Test]
        public static void TestConstructor()
        {
            AesKey key = new AesKey();
            Process process = new Process();
            IRuntimeFileInfo decryptedFileInfo = AxCryptEnvironment.Current.FileInfo(@"c:\test.txt");
            IRuntimeFileInfo encryptedFileInfo = AxCryptEnvironment.Current.FileInfo(@"c:\Documents\HelloWorld.axx");
            using (ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.None, process))
            {
                decryptedFileInfo = activeFile.DecryptedFileInfo;
                Assert.That(decryptedFileInfo.Exists, Is.True, "The file should exist in the fake file system.");
                Assert.That(decryptedFileInfo.FullName, Is.EqualTo(@"c:\test.txt"), "The file should be named as it was in the constructor");
                Assert.That(decryptedFileInfo.LastWriteTimeUtc, Is.EqualTo(decryptedFileInfo.LastWriteTimeUtc), "When a LastWriteTime is not specified, the decrypted file should be used to determine the value.");
                Assert.That(activeFile.Process, Is.EqualTo(process), "The process should be set from the constructor.");
                Thread.Sleep(200);
                using (ActiveFile otherFile = new ActiveFile(activeFile, ActiveFileStatus.AssumedOpenAndDecrypted))
                {
                    Assert.That(otherFile.Status, Is.EqualTo(ActiveFileStatus.AssumedOpenAndDecrypted), "The status should be as given in the constructor.");
                    Assert.That(otherFile.DecryptedFileInfo.FullName, Is.EqualTo(activeFile.DecryptedFileInfo.FullName), "This should be copied from the original instance.");
                    Assert.That(otherFile.EncryptedFileInfo.FullName, Is.EqualTo(activeFile.EncryptedFileInfo.FullName), "This should be copied from the original instance.");
                    Assert.That(otherFile.Key, Is.EqualTo(activeFile.Key), "This should be copied from the original instance.");
                    Assert.That(otherFile.LastAccessTimeUtc, Is.GreaterThan(activeFile.LastAccessTimeUtc), "This should not be copied from the original instance, but should be a later time.");
                    Assert.That(otherFile.Process, Is.EqualTo(process), "This should be copied from the original instance.");
                    Assert.That(activeFile.Process, Is.Null, "The process should only be owned by one instance at a time.");
                    Assert.That(otherFile.ThumbprintMatch(activeFile.Key), Is.True, "The thumbprints should match.");
                }

                using (ActiveFile otherFile = new ActiveFile(activeFile, ActiveFileStatus.AssumedOpenAndDecrypted, process))
                {
                    Assert.That(otherFile.Process, Is.EqualTo(process), "This should be copied from the instance provided in the constructor.");
                }

                activeFile.DecryptedFileInfo.LastWriteTimeUtc = activeFile.DecryptedFileInfo.LastWriteTimeUtc.AddDays(1);
                using (ActiveFile otherFile = new ActiveFile(activeFile, DateTime.UtcNow, ActiveFileStatus.AssumedOpenAndDecrypted))
                {
                    Assert.That(activeFile.IsModified, Is.True, "The original instance has not been encrypted since the last change.");
                    Assert.That(otherFile.IsModified, Is.False, "The copy indicates that it has been encrypted and thus is not modified.");
                }
            }
        }

        [Test]
        public static void TestCopyConstructorWithKey()
        {
            Process process = new Process();
            AesKey key = new AesKey();
            IRuntimeFileInfo decryptedFileInfo = AxCryptEnvironment.Current.FileInfo(@"c:\test.txt");
            IRuntimeFileInfo encryptedFileInfo = AxCryptEnvironment.Current.FileInfo(@"c:\Documents\HelloWorld.axx");
            using (ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.None, process))
            {
                AesKey newKey = new AesKey();
                using (ActiveFile newActiveFile = new ActiveFile(activeFile, newKey))
                {
                    Assert.That(activeFile.Key, Is.Not.EqualTo(newKey), "Ensure that it's really a different key.");
                    Assert.That(newActiveFile.Key, Is.EqualTo(newKey), "The constructor should assign the new key to the new ActiveFile instance.");
                }
            }
        }

        [Test]
        public static void TestThumbprint()
        {
            IRuntimeFileInfo decryptedFileInfo = AxCryptEnvironment.Current.FileInfo(@"c:\test.txt");
            IRuntimeFileInfo encryptedFileInfo = AxCryptEnvironment.Current.FileInfo(@"c:\Documents\HelloWorld.axx");
            Process process = new Process();

            AesKey key = new AesKey();

            using (MemoryStream stream = new MemoryStream())
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(ActiveFile));
                using (ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.None, process))
                {
                    serializer.WriteObject(stream, activeFile);
                }
                stream.Position = 0;
                ActiveFile deserializedActiveFile = (ActiveFile)serializer.ReadObject(stream);

                Assert.That(deserializedActiveFile.ThumbprintMatch(key), Is.True, "The deserialized object should match the thumbprint with the key.");
            }
        }

        [Test]
        public static void TestIsModified()
        {
            IRuntimeFileInfo decryptedFileInfo = AxCryptEnvironment.Current.FileInfo(@"c:\doesnotexist.txt");
            IRuntimeFileInfo encryptedFileInfo = AxCryptEnvironment.Current.FileInfo(@"c:\Documents\HelloWorld.axx");
            using (ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, new AesKey(), ActiveFileStatus.None, null))
            {
                Assert.That(activeFile.IsModified, Is.False, "A non-existing decrypted file should not be treated as modified.");
            }
        }
    }
}