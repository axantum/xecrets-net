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

        private static FakeRuntimeEnvironment _fakeRuntimeEnvironment;

        private static readonly string _rootPath = Path.GetPathRoot(Environment.CurrentDirectory);
        private static readonly string _testTextPath = _rootPath.PathCombine("test.txt");
        private static readonly string _davidCopperfieldTxtPath = _rootPath.PathCombine("Users", "AxCrypt", "David Copperfield.txt");
        private static readonly string _uncompressedAxxPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).PathCombine("Uncompressed.axx");
        private static readonly string _helloWorldAxxPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).PathCombine("HelloWorld.axx");

        [TestFixtureSetUp]
        public static void SetupFixture()
        {
            _environment = AxCryptEnvironment.Current;
            AxCryptEnvironment.Current = _fakeRuntimeEnvironment = new FakeRuntimeEnvironment();

            FakeRuntimeFileInfo.AddFile(_testTextPath, FakeRuntimeFileInfo.TestDate1Utc, FakeRuntimeFileInfo.TestDate2Utc, FakeRuntimeFileInfo.TestDate1Utc, new MemoryStream(Encoding.UTF8.GetBytes("This is a short file")));
            FakeRuntimeFileInfo.AddFile(_davidCopperfieldTxtPath, FakeRuntimeFileInfo.TestDate4Utc, FakeRuntimeFileInfo.TestDate5Utc, FakeRuntimeFileInfo.TestDate6Utc, new MemoryStream(Encoding.GetEncoding(1252).GetBytes(Resources.david_copperfield)));
            FakeRuntimeFileInfo.AddFile(_uncompressedAxxPath, new MemoryStream(Resources.uncompressable_zip));
            FakeRuntimeFileInfo.AddFile(_helloWorldAxxPath, new MemoryStream(Resources.helloworld_key_a_txt));
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
            ILauncher nullProcess = null;
            IRuntimeFileInfo decryptedFileInfo = AxCryptEnvironment.Current.FileInfo(_testTextPath);
            IRuntimeFileInfo encryptedFileInfo = AxCryptEnvironment.Current.FileInfo(_helloWorldAxxPath);
            AesKey key = new AesKey();
            AesKey nullKey = null;
            ILauncher process = new FakeLauncher(String.Empty);
            ActiveFile nullActiveFile = null;

            ActiveFile originalActiveFile = new ActiveFile(decryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.None, process);
            Assert.Throws<ArgumentNullException>(() => { new ActiveFile(nullActiveFile, key); });
            Assert.Throws<ArgumentNullException>(() => { new ActiveFile(originalActiveFile, nullKey); });
            Assert.Throws<ArgumentNullException>(() => { new ActiveFile(nullActiveFile, ActiveFileStatus.None, nullProcess); });
            Assert.Throws<ArgumentNullException>(() => { new ActiveFile(nullActiveFile, ActiveFileStatus.None); });
            Assert.Throws<ArgumentNullException>(() => { new ActiveFile(nullActiveFile, DateTime.MinValue, ActiveFileStatus.None); });
            Assert.Throws<ArgumentNullException>(() => { new ActiveFile(nullFileInfo, decryptedFileInfo, new AesKey(), ActiveFileStatus.None, nullProcess); });
            Assert.Throws<ArgumentNullException>(() => { new ActiveFile(encryptedFileInfo, nullFileInfo, new AesKey(), ActiveFileStatus.None, nullProcess); });
            Assert.Throws<ArgumentNullException>(() => { new ActiveFile(encryptedFileInfo, decryptedFileInfo, nullKey, ActiveFileStatus.None, nullProcess); });
        }

        [Test]
        public static void TestConstructor()
        {
            AesKey key = new AesKey();
            ILauncher process = new FakeLauncher(String.Empty);
            IRuntimeFileInfo decryptedFileInfo = AxCryptEnvironment.Current.FileInfo(_testTextPath);
            IRuntimeFileInfo encryptedFileInfo = AxCryptEnvironment.Current.FileInfo(_helloWorldAxxPath);
            using (ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.None, process))
            {
                decryptedFileInfo = activeFile.DecryptedFileInfo;
                Assert.That(decryptedFileInfo.Exists, Is.True, "The file should exist in the fake file system.");
                Assert.That(decryptedFileInfo.FullName, Is.EqualTo(_testTextPath), "The file should be named as it was in the constructor");
                Assert.That(decryptedFileInfo.LastWriteTimeUtc, Is.EqualTo(decryptedFileInfo.LastWriteTimeUtc), "When a LastWriteTime is not specified, the decrypted file should be used to determine the value.");
                Assert.That(activeFile.Process, Is.EqualTo(process), "The process should be set from the constructor.");
                _fakeRuntimeEnvironment.TimeFunction = (() => { return DateTime.UtcNow.AddMinutes(1); });
                using (ActiveFile otherFile = new ActiveFile(activeFile, ActiveFileStatus.AssumedOpenAndDecrypted))
                {
                    Assert.That(otherFile.Status, Is.EqualTo(ActiveFileStatus.AssumedOpenAndDecrypted), "The status should be as given in the constructor.");
                    Assert.That(otherFile.DecryptedFileInfo.FullName, Is.EqualTo(activeFile.DecryptedFileInfo.FullName), "This should be copied from the original instance.");
                    Assert.That(otherFile.EncryptedFileInfo.FullName, Is.EqualTo(activeFile.EncryptedFileInfo.FullName), "This should be copied from the original instance.");
                    Assert.That(otherFile.Key, Is.EqualTo(activeFile.Key), "This should be copied from the original instance.");
                    Assert.That(otherFile.LastActivityTimeUtc, Is.GreaterThan(activeFile.LastActivityTimeUtc), "This should not be copied from the original instance, but should be a later time.");
                    Assert.That(otherFile.Process, Is.EqualTo(process), "This should be copied from the original instance.");
                    Assert.That(activeFile.Process, Is.Null, "The process should only be owned by one instance at a time.");
                    Assert.That(otherFile.ThumbprintMatch(activeFile.Key), Is.True, "The thumbprints should match.");
                }

                using (ActiveFile otherFile = new ActiveFile(activeFile, ActiveFileStatus.AssumedOpenAndDecrypted, process))
                {
                    Assert.That(otherFile.Process, Is.EqualTo(process), "This should be copied from the instance provided in the constructor.");
                }

                activeFile.DecryptedFileInfo.LastWriteTimeUtc = activeFile.DecryptedFileInfo.LastWriteTimeUtc.AddDays(1);
                using (ActiveFile otherFile = new ActiveFile(activeFile, AxCryptEnvironment.Current.UtcNow, ActiveFileStatus.AssumedOpenAndDecrypted))
                {
                    Assert.That(activeFile.IsModified, Is.True, "The original instance has not been encrypted since the last change.");
                    Assert.That(otherFile.IsModified, Is.False, "The copy indicates that it has been encrypted and thus is not modified.");
                }
            }
        }

        [Test]
        public static void TestCopyConstructorWithKey()
        {
            ILauncher process = new FakeLauncher(String.Empty);
            AesKey key = new AesKey();
            IRuntimeFileInfo decryptedFileInfo = AxCryptEnvironment.Current.FileInfo(_testTextPath);
            IRuntimeFileInfo encryptedFileInfo = AxCryptEnvironment.Current.FileInfo(_helloWorldAxxPath);
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
            IRuntimeFileInfo decryptedFileInfo = AxCryptEnvironment.Current.FileInfo(_testTextPath);
            IRuntimeFileInfo encryptedFileInfo = AxCryptEnvironment.Current.FileInfo(_helloWorldAxxPath);
            ILauncher process = new FakeLauncher(String.Empty);

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
        public static void TestThumbprintNullKey()
        {
            IRuntimeFileInfo decryptedFileInfo = AxCryptEnvironment.Current.FileInfo(_testTextPath);
            IRuntimeFileInfo encryptedFileInfo = AxCryptEnvironment.Current.FileInfo(_helloWorldAxxPath);
            ILauncher process = new FakeLauncher(String.Empty);

            AesKey key = new AesKey();
            using (MemoryStream stream = new MemoryStream())
            {
                using (ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.None, process))
                {
                    Assert.Throws<ArgumentNullException>(() =>
                    {
                        AesKey nullKey = null;
                        activeFile.ThumbprintMatch(nullKey);
                    });
                }
            }
        }

        [Test]
        public static void TestMethodIsModified()
        {
            IRuntimeFileInfo decryptedFileInfo = AxCryptEnvironment.Current.FileInfo(Path.Combine(_rootPath, "doesnotexist.txt"));
            IRuntimeFileInfo encryptedFileInfo = AxCryptEnvironment.Current.FileInfo(_helloWorldAxxPath);
            using (ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, new AesKey(), ActiveFileStatus.None, null))
            {
                Assert.That(activeFile.IsModified, Is.False, "A non-existing decrypted file should not be treated as modified.");
            }
        }
    }
}