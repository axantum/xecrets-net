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
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.System;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestFileSystemState
    {
        private static IRuntimeEnvironment _environment;

        private static readonly string _rootPath = Path.GetPathRoot(Environment.CurrentDirectory);
        private static readonly string _mystateXmlPath = _rootPath.PathCombine("mytemp", "mystate.txt");
        private static readonly string _encryptedAxxPath = Path.Combine(_rootPath, "Encrypted-txt.axx");
        private static readonly string _encrypted1AxxPath = Path.Combine(_rootPath, "Encrypted1-txt.axx");
        private static readonly string _encrypted2AxxPath = Path.Combine(_rootPath, "Encrypted2-txt.axx");
        private static readonly string _encrypted3AxxPath = Path.Combine(_rootPath, "Encrypted3-txt.axx");
        private static readonly string _encrypted4AxxPath = Path.Combine(_rootPath, "Encrypted4-txt.axx");
        private static readonly string _decryptedTxtPath = Path.Combine(_rootPath, "Decrypted.txt");
        private static readonly string _decrypted1TxtPath = Path.Combine(_rootPath, "Decrypted1.txt");
        private static readonly string _decrypted2TxtPath = Path.Combine(_rootPath, "Decrypted2.txt");
        private static readonly string _decrypted3TxtPath = Path.Combine(_rootPath, "Decrypted3.txt");
        private static readonly string _decrypted4TxtPath = Path.Combine(_rootPath, "Decrypted4.txt");

        [TestFixtureSetUp]
        public static void SetupFixture()
        {
            _environment = AxCryptEnvironment.Current;
            AxCryptEnvironment.Current = new FakeRuntimeEnvironment();
        }

        [TestFixtureTearDown]
        public static void TeardownFixture()
        {
            AxCryptEnvironment.Current = _environment;
            FakeRuntimeFileInfo.ClearFiles();
        }

        [TearDown]
        public static void Teardown()
        {
            FakeRuntimeFileInfo.ClearFiles();
        }

        [Test]
        public static void TestLoadNew()
        {
            FileSystemState state = FileSystemState.Load(AxCryptEnvironment.Current.FileInfo(_mystateXmlPath));

            Assert.That(state, Is.Not.Null, "An instance should always be instantiated.");
            Assert.That(state.ActiveFiles.Count(), Is.EqualTo(0), "A new state should not have any active files.");
        }

        [Test]
        public static void TestLoadExisting()
        {
            FileSystemState state = FileSystemState.Load(AxCryptEnvironment.Current.FileInfo(_mystateXmlPath));

            Assert.That(state, Is.Not.Null, "An instance should always be instantiated.");
            Assert.That(state.ActiveFiles.Count(), Is.EqualTo(0), "A new state should not have any active files.");

            ActiveFile activeFile = new ActiveFile(AxCryptEnvironment.Current.FileInfo(_encryptedAxxPath), AxCryptEnvironment.Current.FileInfo(_decryptedTxtPath), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted, null);
            state.Add(activeFile);
            state.Save();

            FileSystemState reloadedState = FileSystemState.Load(AxCryptEnvironment.Current.FileInfo(_mystateXmlPath));
            Assert.That(reloadedState, Is.Not.Null, "An instance should always be instantiated.");
            Assert.That(reloadedState.ActiveFiles.Count(), Is.EqualTo(1), "The reloaded state should have one active file.");
            Assert.That(reloadedState.ActiveFiles.First().ThumbprintMatch(activeFile.Key), Is.True, "The reloaded thumbprint should  match the key.");
        }

        [Test]
        public static void TestChangedEvent()
        {
            FileSystemState state = FileSystemState.Load(AxCryptEnvironment.Current.FileInfo(_mystateXmlPath));
            bool wasHere;
            state.Changed += new EventHandler<EventArgs>((object sender, EventArgs e) => { wasHere = true; });
            ActiveFile activeFile = new ActiveFile(AxCryptEnvironment.Current.FileInfo(_encryptedAxxPath), AxCryptEnvironment.Current.FileInfo(_decryptedTxtPath), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted, null);

            wasHere = false;
            state.Add(activeFile);
            Assert.That(state.ActiveFiles.Count(), Is.EqualTo(1), "After the Add() the state should have one active file.");
            Assert.That(wasHere, Is.True, "After the Add(), the changed event should have been raised.");

            wasHere = false;
            state.Remove(activeFile);
            Assert.That(wasHere, Is.True, "After the Remove(), the changed event should have been raised.");
            Assert.That(state.ActiveFiles.Count(), Is.EqualTo(0), "After the Remove() the state should have no active files.");

            wasHere = false;
            state.ActiveFiles = new ActiveFile[] { activeFile };
            Assert.That(state.ActiveFiles.Count(), Is.EqualTo(1), "After the assignment to ActiveFiles the state should have one active file.");
            Assert.That(wasHere, Is.True, "After the assignment to ActiveFiles, the changed event should have been raised.");
        }

        [Test]
        public static void TestStatusMaskAtLoad()
        {
            FileSystemState state = FileSystemState.Load(AxCryptEnvironment.Current.FileInfo(_mystateXmlPath));
            ActiveFile activeFile = new ActiveFile(AxCryptEnvironment.Current.FileInfo(_encryptedAxxPath), AxCryptEnvironment.Current.FileInfo(_decryptedTxtPath), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.Error | ActiveFileStatus.IgnoreChange | ActiveFileStatus.NotShareable, null);
            state.Add(activeFile);
            state.Save();

            FileSystemState reloadedState = FileSystemState.Load(AxCryptEnvironment.Current.FileInfo(_mystateXmlPath));
            Assert.That(reloadedState, Is.Not.Null, "An instance should always be instantiated.");
            Assert.That(reloadedState.ActiveFiles.Count(), Is.EqualTo(1), "The reloaded state should have one active file.");
            Assert.That(reloadedState.ActiveFiles.First().Status, Is.EqualTo(ActiveFileStatus.AssumedOpenAndDecrypted), "When reloading saved state, some statuses should be masked away.");
        }

        [Test]
        public static void TestFindEncryptedAndDecryptedPath()
        {
            FileSystemState state = FileSystemState.Load(AxCryptEnvironment.Current.FileInfo(_mystateXmlPath));
            ActiveFile activeFile = new ActiveFile(AxCryptEnvironment.Current.FileInfo(_encryptedAxxPath), AxCryptEnvironment.Current.FileInfo(_decryptedTxtPath), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.Error | ActiveFileStatus.IgnoreChange | ActiveFileStatus.NotShareable, null);
            state.Add(activeFile);

            ActiveFile byDecryptedPath = state.FindDecryptedPath(_decryptedTxtPath);
            Assert.That(byDecryptedPath, Is.EqualTo(activeFile), "The search should return the same instance.");

            ActiveFile byEncryptedPath = state.FindEncryptedPath(_encryptedAxxPath);
            Assert.That(byEncryptedPath, Is.EqualTo(byDecryptedPath), "The search should return the same instance.");

            ActiveFile notFoundEncrypted = state.FindEncryptedPath(Path.Combine(_rootPath, "notfoundfile.txt"));
            Assert.That(notFoundEncrypted, Is.Null, "A search that does not succeed should return null.");

            ActiveFile notFoundDecrypted = state.FindDecryptedPath(Path.Combine(_rootPath, "notfoundfile.txt"));
            Assert.That(notFoundDecrypted, Is.Null, "A search that does not succeed should return null.");
        }

        [Test]
        public static void TestForEach()
        {
            bool changedEventWasRaised = false;
            FileSystemState state = FileSystemState.Load(AxCryptEnvironment.Current.FileInfo(_mystateXmlPath));
            state.Changed += ((object sender, EventArgs e) =>
            {
                changedEventWasRaised = true;
            });

            ActiveFile activeFile;
            activeFile = new ActiveFile(AxCryptEnvironment.Current.FileInfo(_encrypted1AxxPath), AxCryptEnvironment.Current.FileInfo(_decrypted1TxtPath), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.Error | ActiveFileStatus.IgnoreChange | ActiveFileStatus.NotShareable, null);
            state.Add(activeFile);
            activeFile = new ActiveFile(AxCryptEnvironment.Current.FileInfo(_encrypted2AxxPath), AxCryptEnvironment.Current.FileInfo(_decrypted2TxtPath), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.Error | ActiveFileStatus.IgnoreChange | ActiveFileStatus.NotShareable, null);
            state.Add(activeFile);
            activeFile = new ActiveFile(AxCryptEnvironment.Current.FileInfo(_encrypted3AxxPath), AxCryptEnvironment.Current.FileInfo(_decrypted3TxtPath), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.Error | ActiveFileStatus.IgnoreChange | ActiveFileStatus.NotShareable, null);
            state.Add(activeFile);
            Assert.That(changedEventWasRaised, Is.True, "The change event should have been raised by the adding of active files.");

            changedEventWasRaised = false;
            Assert.That(state.ActiveFiles.Count(), Is.EqualTo(3), "There should be three.");
            int i = 0;
            state.ForEach(ChangedEventMode.RaiseOnlyOnModified, (ActiveFile activeFileArgument) =>
            {
                ++i;
                return activeFileArgument;
            });
            Assert.That(i, Is.EqualTo(3), "The iteration should have visited three active files.");
            Assert.That(changedEventWasRaised, Is.False, "No change event should have been raised.");

            i = 0;
            state.ForEach(ChangedEventMode.RaiseAlways, (ActiveFile activeFileArgument) =>
            {
                ++i;
                return activeFileArgument;
            });
            Assert.That(i, Is.EqualTo(3), "The iteration should have visited three active files.");
            Assert.That(changedEventWasRaised, Is.True, "The change event should have been raised.");

            changedEventWasRaised = false;
            i = 0;
            state.ForEach(ChangedEventMode.RaiseAlways, (ActiveFile activeFileArgument) =>
            {
                ++i;
                return new ActiveFile(activeFileArgument, activeFile.Status | ActiveFileStatus.Error);
            });
            Assert.That(i, Is.EqualTo(3), "The iteration should have visited three active files.");
            Assert.That(changedEventWasRaised, Is.True, "The change event should have been raised.");
        }

        [Test]
        public static void TestDecryptedActiveFiles()
        {
            FileSystemState state = FileSystemState.Load(AxCryptEnvironment.Current.FileInfo(_mystateXmlPath));

            ActiveFile decryptedFile1 = new ActiveFile(AxCryptEnvironment.Current.FileInfo(_encryptedAxxPath), AxCryptEnvironment.Current.FileInfo(_decryptedTxtPath), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted, null);
            state.Add(decryptedFile1);

            ActiveFile decryptedFile2 = new ActiveFile(AxCryptEnvironment.Current.FileInfo(_encrypted2AxxPath), AxCryptEnvironment.Current.FileInfo(_decrypted2TxtPath), new AesKey(), ActiveFileStatus.DecryptedIsPendingDelete, null);
            state.Add(decryptedFile2);

            ActiveFile notDecryptedFile = new ActiveFile(AxCryptEnvironment.Current.FileInfo(_encrypted3AxxPath), AxCryptEnvironment.Current.FileInfo(_decrypted3TxtPath), new AesKey(), ActiveFileStatus.NotDecrypted, null);
            state.Add(notDecryptedFile);

            ActiveFile errorFile = new ActiveFile(AxCryptEnvironment.Current.FileInfo(_encrypted4AxxPath), AxCryptEnvironment.Current.FileInfo(_decrypted4TxtPath), new AesKey(), ActiveFileStatus.Error, null);
            state.Add(errorFile);

            IList<ActiveFile> decryptedFiles = state.DecryptedActiveFiles;
            Assert.That(decryptedFiles.Count, Is.EqualTo(2), "There should be two decrypted files.");
            Assert.That(decryptedFiles.Contains(decryptedFile1), "A file marked as AssumedOpenAndDecrypted should be found.");
            Assert.That(decryptedFiles.Contains(decryptedFile2), "A file marked as DecryptedIsPendingDelete should be found.");
            Assert.That(decryptedFiles.Contains(notDecryptedFile), Is.Not.True, "A file marked as NotDecrypted should not be found.");
        }
    }
}