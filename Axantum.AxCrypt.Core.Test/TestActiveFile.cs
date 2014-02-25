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
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.Test.Properties;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestActiveFile
    {
        private static readonly string _rootPath = Path.GetPathRoot(Environment.CurrentDirectory);
        private static readonly string _testTextPath = _rootPath.PathCombine("test.txt");
        private static readonly string _davidCopperfieldTxtPath = _rootPath.PathCombine("Users", "AxCrypt", "David Copperfield.txt");
        private static readonly string _uncompressedAxxPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).PathCombine("Uncompressed.axx");
        private static readonly string _helloWorldAxxPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).PathCombine("HelloWorld.axx");

        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();

            FakeRuntimeFileInfo.AddFile(_testTextPath, FakeRuntimeFileInfo.TestDate1Utc, FakeRuntimeFileInfo.TestDate2Utc, FakeRuntimeFileInfo.TestDate1Utc, FakeRuntimeFileInfo.ExpandableMemoryStream(Encoding.UTF8.GetBytes("This is a short file")));
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
            IRuntimeFileInfo nullFileInfo = null;
            IRuntimeFileInfo decryptedFileInfo = Factory.New<IRuntimeFileInfo>(_testTextPath);
            IRuntimeFileInfo encryptedFileInfo = Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath);
            IPassphrase key = new GenericPassphrase("key");
            IPassphrase nullKey = null;
            ActiveFile nullActiveFile = null;

            ActiveFile originalActiveFile = new ActiveFile(decryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.None);
            Assert.Throws<ArgumentNullException>(() => { if (new ActiveFile(nullActiveFile) == null) { } });
            Assert.Throws<ArgumentNullException>(() => { if (new ActiveFile(nullActiveFile, key) == null) { } });
            Assert.Throws<ArgumentNullException>(() => { if (new ActiveFile(originalActiveFile, nullKey) == null) { } });
            Assert.Throws<ArgumentNullException>(() => { if (new ActiveFile(nullActiveFile, ActiveFileStatus.None) == null) { } });
            Assert.Throws<ArgumentNullException>(() => { if (new ActiveFile(nullActiveFile, DateTime.MinValue, ActiveFileStatus.None) == null) { } });
            Assert.Throws<ArgumentNullException>(() => { if (new ActiveFile(nullFileInfo, decryptedFileInfo, new GenericPassphrase("a"), ActiveFileStatus.None) == null) { } });
            Assert.Throws<ArgumentNullException>(() => { if (new ActiveFile(encryptedFileInfo, nullFileInfo, new GenericPassphrase("b"), ActiveFileStatus.None) == null) { } });
            Assert.Throws<ArgumentNullException>(() => { if (new ActiveFile(encryptedFileInfo, decryptedFileInfo, nullKey, ActiveFileStatus.None) == null) { } });
        }

        [Test]
        public static void TestConstructor()
        {
            IPassphrase key = new GenericPassphrase("key");
            IRuntimeFileInfo decryptedFileInfo = Factory.New<IRuntimeFileInfo>(_testTextPath);
            IRuntimeFileInfo encryptedFileInfo = Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath);

            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.None);
            decryptedFileInfo = activeFile.DecryptedFileInfo;
            Assert.That(decryptedFileInfo.Exists, Is.True, "The file should exist in the fake file system.");
            Assert.That(decryptedFileInfo.FullName, Is.EqualTo(_testTextPath), "The file should be named as it was in the constructor");
            Assert.That(decryptedFileInfo.LastWriteTimeUtc, Is.EqualTo(decryptedFileInfo.LastWriteTimeUtc), "When a LastWriteTime is not specified, the decrypted file should be used to determine the value.");
            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = (() => { return DateTime.UtcNow.AddMinutes(1); });

            ActiveFile otherFile = new ActiveFile(activeFile, ActiveFileStatus.AssumedOpenAndDecrypted);
            Assert.That(otherFile.Status, Is.EqualTo(ActiveFileStatus.AssumedOpenAndDecrypted), "The status should be as given in the constructor.");
            Assert.That(otherFile.DecryptedFileInfo.FullName, Is.EqualTo(activeFile.DecryptedFileInfo.FullName), "This should be copied from the original instance.");
            Assert.That(otherFile.EncryptedFileInfo.FullName, Is.EqualTo(activeFile.EncryptedFileInfo.FullName), "This should be copied from the original instance.");
            Assert.That(otherFile.Key, Is.EqualTo(activeFile.Key), "This should be copied from the original instance.");
            Assert.That(otherFile.LastActivityTimeUtc, Is.GreaterThan(activeFile.LastActivityTimeUtc), "This should not be copied from the original instance, but should be a later time.");
            Assert.That(otherFile.ThumbprintMatch(activeFile.Key), Is.True, "The thumbprints should match.");

            activeFile.DecryptedFileInfo.LastWriteTimeUtc = activeFile.DecryptedFileInfo.LastWriteTimeUtc.AddDays(1);
            otherFile = new ActiveFile(activeFile, OS.Current.UtcNow, ActiveFileStatus.AssumedOpenAndDecrypted);
            Assert.That(activeFile.IsModified, Is.True, "The original instance has not been encrypted since the last change.");
            Assert.That(otherFile.IsModified, Is.False, "The copy indicates that it has been encrypted and thus is not modified.");
        }

        [Test]
        public static void TestCopyConstructorWithKey()
        {
            IPassphrase key = new GenericPassphrase("key");
            IRuntimeFileInfo decryptedFileInfo = Factory.New<IRuntimeFileInfo>(_testTextPath);
            IRuntimeFileInfo encryptedFileInfo = Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath);
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.None);
            IPassphrase newKey = new GenericPassphrase("newKey");

            ActiveFile newActiveFile = new ActiveFile(activeFile, newKey);
            Assert.That(activeFile.Key, Is.Not.EqualTo(newKey), "Ensure that it's really a different key.");
            Assert.That(newActiveFile.Key, Is.EqualTo(newKey), "The constructor should assign the new key to the new ActiveFile instance.");
        }

        [Test]
        public static void TestThumbprint()
        {
            IRuntimeFileInfo decryptedFileInfo = Factory.New<IRuntimeFileInfo>(_testTextPath);
            IRuntimeFileInfo encryptedFileInfo = Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath);

            IPassphrase key = new GenericPassphrase("key");

            using (MemoryStream stream = new MemoryStream())
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(ActiveFile));
                ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.None);
                serializer.WriteObject(stream, activeFile);
                stream.Position = 0;
                ActiveFile deserializedActiveFile = (ActiveFile)serializer.ReadObject(stream);

                Assert.That(deserializedActiveFile.ThumbprintMatch(key), Is.True, "The deserialized object should match the thumbprint with the key.");
            }
        }

        [Test]
        public static void TestThumbprintNullKey()
        {
            IRuntimeFileInfo decryptedFileInfo = Factory.New<IRuntimeFileInfo>(_testTextPath);
            IRuntimeFileInfo encryptedFileInfo = Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath);

            IPassphrase key = new GenericPassphrase("key");
            using (MemoryStream stream = new MemoryStream())
            {
                ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.None);
                Assert.Throws<ArgumentNullException>(() =>
                {
                    IPassphrase nullKey = null;
                    activeFile.ThumbprintMatch(nullKey);
                });
            }
        }

        [Test]
        public static void TestMethodIsModified()
        {
            IRuntimeFileInfo decryptedFileInfo = Factory.New<IRuntimeFileInfo>(Path.Combine(_rootPath, "doesnotexist.txt"));
            IRuntimeFileInfo encryptedFileInfo = Factory.New<IRuntimeFileInfo>(_helloWorldAxxPath);
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, new GenericPassphrase("new"), ActiveFileStatus.None);
            Assert.That(activeFile.IsModified, Is.False, "A non-existing decrypted file should not be treated as modified.");
        }

        [Test]
        public static void TestVisualState()
        {
            ActiveFile activeFile;
            IPassphrase key = new GenericPassphrase("key");

            activeFile = new ActiveFile(Factory.New<IRuntimeFileInfo>(@"C:\encrypted.axx"), Factory.New<IRuntimeFileInfo>(@"C:\decrypted.txt"), key, ActiveFileStatus.NotDecrypted);
            Assert.That(activeFile.VisualState, Is.EqualTo(ActiveFileVisualState.EncryptedWithKnownKey));

            activeFile = new ActiveFile(activeFile);
            Assert.That(activeFile.VisualState, Is.EqualTo(ActiveFileVisualState.EncryptedWithoutKnownKey));

            activeFile = new ActiveFile(activeFile, ActiveFileStatus.DecryptedIsPendingDelete);
            Assert.That(activeFile.VisualState, Is.EqualTo(ActiveFileVisualState.DecryptedWithoutKnownKey));

            activeFile = new ActiveFile(activeFile, key);
            Assert.That(activeFile.VisualState, Is.EqualTo(ActiveFileVisualState.DecryptedWithKnownKey));

            activeFile = new ActiveFile(activeFile, ActiveFileStatus.AssumedOpenAndDecrypted);
            Assert.That(activeFile.VisualState, Is.EqualTo(ActiveFileVisualState.DecryptedWithKnownKey));

            activeFile = new ActiveFile(activeFile);
            Assert.That(activeFile.VisualState, Is.EqualTo(ActiveFileVisualState.DecryptedWithoutKnownKey));

            activeFile = new ActiveFile(activeFile, ActiveFileStatus.Error);
            Assert.Throws<InvalidOperationException>(() => { if (activeFile.VisualState == ActiveFileVisualState.None) { } });
        }
    }
}