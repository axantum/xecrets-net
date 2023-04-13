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
 * The source is maintained at http://bitbucket.org/AxCrypt.Desktop.Window-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.IO;
using AxCrypt.Core.Session;
using AxCrypt.Fake;

using NUnit.Framework;

using System.Text;

using Xecrets.File.Core.Test.Properties;

using static AxCrypt.Abstractions.TypeResolve;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestActiveFile
    {
        private static string _rootPath;
        private static string _testTextPath;
        private static string _davidCopperfieldTxtPath;
        private static string _uncompressedAxxPath;
        private static string _helloWorldAxxPath;

        private CryptoImplementation _cryptoImplementation;

        public TestActiveFile(CryptoImplementation cryptoImplementation)
        {
            _cryptoImplementation = cryptoImplementation;
        }

        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup();
            SetupAssembly.AssemblySetupCrypto(_cryptoImplementation);

            _rootPath = Path.GetPathRoot(Environment.CurrentDirectory);
            _testTextPath = _rootPath.PathCombine("test.txt");
            _davidCopperfieldTxtPath = _rootPath.PathCombine("Users", "AxCrypt", "David Copperfield.txt");
            _uncompressedAxxPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).PathCombine("Uncompressed.axx");
            _helloWorldAxxPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).PathCombine("HelloWorld.axx");

            FakeDataStore.AddFile(_testTextPath, FakeDataStore.TestDate1Utc, FakeDataStore.TestDate2Utc, FakeDataStore.TestDate1Utc, FakeDataStore.ExpandableMemoryStream(Encoding.UTF8.GetBytes("This is a short file")));
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
            IDataStore nullFileInfo = null;
            IDataStore decryptedFileInfo = New<IDataStore>(_testTextPath);
            IDataStore encryptedFileInfo = New<IDataStore>(_helloWorldAxxPath);
            LogOnIdentity key = new LogOnIdentity("key");
            LogOnIdentity nullKey = null;
            ActiveFile nullActiveFile = null;

            ActiveFile originalActiveFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.None, new V1Aes128CryptoFactory().CryptoId);
            Assert.Throws<ArgumentNullException>(() => { if (new ActiveFile(nullActiveFile) == null) { } });
            Assert.Throws<ArgumentNullException>(() => { if (new ActiveFile(nullActiveFile, key) == null) { } });
            Assert.Throws<ArgumentNullException>(() => { if (new ActiveFile(originalActiveFile, nullKey) == null) { } });
            Assert.Throws<ArgumentNullException>(() => { if (new ActiveFile(nullActiveFile, ActiveFileStatus.None) == null) { } });
            Assert.Throws<ArgumentNullException>(() => { if (new ActiveFile(nullActiveFile, DateTime.MinValue, ActiveFileStatus.None) == null) { } });
            Assert.Throws<ArgumentNullException>((TestDelegate)(() => { if (new ActiveFile(nullFileInfo, decryptedFileInfo, new LogOnIdentity("a"), ActiveFileStatus.None, (Guid)new V1Aes128CryptoFactory().CryptoId) == null) { } }));
            Assert.Throws<ArgumentNullException>((TestDelegate)(() => { if (new ActiveFile(encryptedFileInfo, nullFileInfo, new LogOnIdentity("b"), ActiveFileStatus.None, (Guid)new V1Aes128CryptoFactory().CryptoId) == null) { } }));
            Assert.Throws<ArgumentNullException>((TestDelegate)(() => { if (new ActiveFile(encryptedFileInfo, decryptedFileInfo, nullKey, ActiveFileStatus.None, (Guid)new V1Aes128CryptoFactory().CryptoId) == null) { } }));
        }

        [Test]
        public void TestConstructor()
        {
            LogOnIdentity key = new LogOnIdentity("key");
            IDataStore decryptedFileInfo = New<IDataStore>(_testTextPath);
            IDataStore encryptedFileInfo = New<IDataStore>(_helloWorldAxxPath);

            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.None, new V1Aes128CryptoFactory().CryptoId);
            decryptedFileInfo = activeFile.DecryptedFileInfo;
            Assert.That(decryptedFileInfo.IsAvailable, Is.True, "The file should exist in the fake file system.");
            Assert.That(decryptedFileInfo.FullName, Is.EqualTo(_testTextPath), "The file should be named as it was in the constructor");
            Assert.That(decryptedFileInfo.LastWriteTimeUtc, Is.EqualTo(decryptedFileInfo.LastWriteTimeUtc), "When a LastWriteTime is not specified, the decrypted file should be used to determine the value.");
            ((FakeNow)New<INow>()).TimeFunction = (() => { return DateTime.UtcNow.AddMinutes(1); });

            ActiveFile otherFile = new ActiveFile(activeFile, ActiveFileStatus.AssumedOpenAndDecrypted);
            Assert.That(otherFile.Status, Is.EqualTo(ActiveFileStatus.AssumedOpenAndDecrypted), "The status should be as given in the constructor.");
            Assert.That(otherFile.DecryptedFileInfo.FullName, Is.EqualTo(activeFile.DecryptedFileInfo.FullName), "This should be copied from the original instance.");
            Assert.That(otherFile.EncryptedFileInfo.FullName, Is.EqualTo(activeFile.EncryptedFileInfo.FullName), "This should be copied from the original instance.");
            Assert.That(otherFile.Identity, Is.EqualTo(activeFile.Identity), "This should be copied from the original instance.");
            Assert.That(otherFile.Properties.LastActivityTimeUtc, Is.GreaterThan(activeFile.Properties.LastActivityTimeUtc), "This should not be copied from the original instance, but should be a later time.");
            Assert.That(otherFile.ThumbprintMatch(activeFile.Identity.Passphrase), Is.True, "The thumbprints should match.");

            activeFile.DecryptedFileInfo.LastWriteTimeUtc = activeFile.DecryptedFileInfo.LastWriteTimeUtc.AddDays(1);
            otherFile = new ActiveFile(activeFile, New<INow>().Utc, ActiveFileStatus.AssumedOpenAndDecrypted);
            Assert.That(activeFile.IsModified, Is.True, "The original instance has not been encrypted since the last change.");
            Assert.That(otherFile.IsModified, Is.False, "The copy indicates that it has been encrypted and thus is not modified.");
        }

        [Test]
        public void TestCopyConstructorWithKey()
        {
            LogOnIdentity key = new LogOnIdentity("key");
            IDataStore decryptedFileInfo = New<IDataStore>(_testTextPath);
            IDataStore encryptedFileInfo = New<IDataStore>(_helloWorldAxxPath);
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.None, new V1Aes128CryptoFactory().CryptoId);
            LogOnIdentity newKey = new LogOnIdentity("newKey");

            ActiveFile newActiveFile = new ActiveFile(activeFile, newKey);
            Assert.That(activeFile.Identity, Is.Not.EqualTo(newKey), "Ensure that it's really a different key.");
            Assert.That(newActiveFile.Identity, Is.EqualTo(newKey), "The constructor should assign the new key to the new ActiveFile instance.");
        }

        [Test]
        public void TestThumbprint()
        {
            IDataStore decryptedFileInfo = New<IDataStore>(_testTextPath);
            IDataStore encryptedFileInfo = New<IDataStore>(_helloWorldAxxPath);

            LogOnIdentity key = new LogOnIdentity("key");

            using (MemoryStream stream = new MemoryStream())
            {
                ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.None, new V1Aes128CryptoFactory().CryptoId);
                string json = Resolve.Serializer.Serialize(activeFile);
                ActiveFile deserializedActiveFile = Resolve.Serializer.Deserialize<ActiveFile>(json);

                Assert.That(deserializedActiveFile.ThumbprintMatch(key.Passphrase), Is.True, "The deserialized object should match the thumbprint with the key.");
            }
        }

        [Test]
        public void TestThumbprintNullKey()
        {
            IDataStore decryptedFileInfo = New<IDataStore>(_testTextPath);
            IDataStore encryptedFileInfo = New<IDataStore>(_helloWorldAxxPath);

            LogOnIdentity key = new LogOnIdentity("key");
            using (MemoryStream stream = new MemoryStream())
            {
                ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, key, ActiveFileStatus.None, new V1Aes128CryptoFactory().CryptoId);
                Assert.Throws<ArgumentNullException>(() =>
                {
                    Passphrase nullKey = null;
                    activeFile.ThumbprintMatch(nullKey);
                });
            }
        }

        [Test]
        public void TestMethodIsModified()
        {
            IDataStore decryptedFileInfo = New<IDataStore>(Path.Combine(_rootPath, "doesnotexist.txt"));
            IDataStore encryptedFileInfo = New<IDataStore>(_helloWorldAxxPath);
            ActiveFile activeFile = new ActiveFile(encryptedFileInfo, decryptedFileInfo, new LogOnIdentity("new"), ActiveFileStatus.None, new V1Aes128CryptoFactory().CryptoId);
            Assert.That(activeFile.IsModified, Is.False, "A non-existing decrypted file should not be treated as modified.");
        }

        [Test]
        public void TestVisualStateLowEncryption()
        {
            ActiveFile activeFile;
            LogOnIdentity key = new LogOnIdentity("key");

            activeFile = new ActiveFile(New<IDataStore>(@"C:\encrypted.axx"), New<IDataStore>(@"C:\decrypted.txt"), key, ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().CryptoId);
            Assert.That(activeFile.VisualState, Is.EqualTo(ActiveFileVisualStates.EncryptedWithKnownKey | ActiveFileVisualStates.LowEncryption));

            activeFile = new ActiveFile(activeFile);
            Assert.That(activeFile.VisualState, Is.EqualTo(ActiveFileVisualStates.EncryptedWithoutKnownKey | ActiveFileVisualStates.LowEncryption));

            activeFile = new ActiveFile(activeFile, ActiveFileStatus.DecryptedIsPendingDelete);
            Assert.That(activeFile.VisualState, Is.EqualTo(ActiveFileVisualStates.DecryptedWithoutKnownKey | ActiveFileVisualStates.LowEncryption));

            activeFile = new ActiveFile(activeFile, key);
            Assert.That(activeFile.VisualState, Is.EqualTo(ActiveFileVisualStates.DecryptedWithKnownKey | ActiveFileVisualStates.LowEncryption));

            activeFile = new ActiveFile(activeFile, ActiveFileStatus.AssumedOpenAndDecrypted);
            Assert.That(activeFile.VisualState, Is.EqualTo(ActiveFileVisualStates.DecryptedWithKnownKey | ActiveFileVisualStates.LowEncryption));

            activeFile = new ActiveFile(activeFile);
            Assert.That(activeFile.VisualState, Is.EqualTo(ActiveFileVisualStates.DecryptedWithoutKnownKey | ActiveFileVisualStates.LowEncryption));

            activeFile = new ActiveFile(activeFile, ActiveFileStatus.Error);
            Assert.Throws<InvalidOperationException>(() => { if (activeFile.VisualState == ActiveFileVisualStates.None) { } });
        }

        [Test]
        public void TestVisualStateHiEncryption()
        {
            ActiveFile activeFile;
            LogOnIdentity key = new LogOnIdentity("key");

            activeFile = new ActiveFile(New<IDataStore>(@"C:\encrypted.axx"), New<IDataStore>(@"C:\decrypted.txt"), key, ActiveFileStatus.NotDecrypted, new V2Aes256CryptoFactory().CryptoId);
            Assert.That(activeFile.VisualState, Is.EqualTo(ActiveFileVisualStates.EncryptedWithKnownKey));

            activeFile = new ActiveFile(activeFile);
            Assert.That(activeFile.VisualState, Is.EqualTo(ActiveFileVisualStates.EncryptedWithoutKnownKey));

            activeFile = new ActiveFile(activeFile, ActiveFileStatus.DecryptedIsPendingDelete);
            Assert.That(activeFile.VisualState, Is.EqualTo(ActiveFileVisualStates.DecryptedWithoutKnownKey));

            activeFile = new ActiveFile(activeFile, key);
            Assert.That(activeFile.VisualState, Is.EqualTo(ActiveFileVisualStates.DecryptedWithKnownKey));

            activeFile = new ActiveFile(activeFile, ActiveFileStatus.AssumedOpenAndDecrypted);
            Assert.That(activeFile.VisualState, Is.EqualTo(ActiveFileVisualStates.DecryptedWithKnownKey));

            activeFile = new ActiveFile(activeFile);
            Assert.That(activeFile.VisualState, Is.EqualTo(ActiveFileVisualStates.DecryptedWithoutKnownKey));

            activeFile = new ActiveFile(activeFile, ActiveFileStatus.Error);
            Assert.Throws<InvalidOperationException>(() => { if (activeFile.VisualState == ActiveFileVisualStates.None) { } });
        }
    }
}
