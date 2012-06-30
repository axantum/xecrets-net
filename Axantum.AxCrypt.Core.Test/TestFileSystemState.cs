using System;
using System.Collections.Generic;
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
            FileSystemState state = FileSystemState.Load(AxCryptEnvironment.Current.FileInfo(@"c:\mytemp\mystate.xml"));

            Assert.That(state, Is.Not.Null, "An instance should always be instantiated.");
            Assert.That(state.ActiveFiles.Count(), Is.EqualTo(0), "A new state should not have any active files.");
        }

        [Test]
        public static void TestLoadExisting()
        {
            FileSystemState state = FileSystemState.Load(AxCryptEnvironment.Current.FileInfo(@"c:\mytemp\mystate.xml"));

            Assert.That(state, Is.Not.Null, "An instance should always be instantiated.");
            Assert.That(state.ActiveFiles.Count(), Is.EqualTo(0), "A new state should not have any active files.");

            ActiveFile activeFile = new ActiveFile(AxCryptEnvironment.Current.FileInfo(@"C:\Encrypted-txt.axx"), AxCryptEnvironment.Current.FileInfo(@"C:\Decrypted.txt"), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted, null);
            state.Add(activeFile);
            state.Save();

            FileSystemState reloadedState = FileSystemState.Load(AxCryptEnvironment.Current.FileInfo(@"c:\mytemp\mystate.xml"));
            Assert.That(reloadedState, Is.Not.Null, "An instance should always be instantiated.");
            Assert.That(reloadedState.ActiveFiles.Count(), Is.EqualTo(1), "The reloaded state should have one active file.");
            Assert.That(reloadedState.ActiveFiles.First().ThumbprintMatch(activeFile.Key), Is.True, "The reloaded thumbprint should  match the key.");
        }

        [Test]
        public static void TestChangedEvent()
        {
            FileSystemState state = FileSystemState.Load(AxCryptEnvironment.Current.FileInfo(@"c:\mytemp\mystate.xml"));
            bool wasHere;
            state.Changed += new EventHandler<EventArgs>((object sender, EventArgs e) => { wasHere = true; });
            ActiveFile activeFile = new ActiveFile(AxCryptEnvironment.Current.FileInfo(@"C:\Encrypted-txt.axx"), AxCryptEnvironment.Current.FileInfo(@"C:\Decrypted.txt"), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted, null);

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
            FileSystemState state = FileSystemState.Load(AxCryptEnvironment.Current.FileInfo(@"c:\mytemp\mystate.xml"));
            ActiveFile activeFile = new ActiveFile(AxCryptEnvironment.Current.FileInfo(@"C:\Encrypted-txt.axx"), AxCryptEnvironment.Current.FileInfo(@"C:\Decrypted.txt"), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.Error | ActiveFileStatus.IgnoreChange | ActiveFileStatus.NotShareable, null);
            state.Add(activeFile);
            state.Save();

            FileSystemState reloadedState = FileSystemState.Load(AxCryptEnvironment.Current.FileInfo(@"c:\mytemp\mystate.xml"));
            Assert.That(reloadedState, Is.Not.Null, "An instance should always be instantiated.");
            Assert.That(reloadedState.ActiveFiles.Count(), Is.EqualTo(1), "The reloaded state should have one active file.");
            Assert.That(reloadedState.ActiveFiles.First().Status, Is.EqualTo(ActiveFileStatus.AssumedOpenAndDecrypted), "When reloading saved state, some statuses should be masked away.");
        }

        [Test]
        public static void TestFindEncryptedAndDecryptedPath()
        {
            FileSystemState state = FileSystemState.Load(AxCryptEnvironment.Current.FileInfo(@"c:\mytemp\mystate.xml"));
            ActiveFile activeFile = new ActiveFile(AxCryptEnvironment.Current.FileInfo(@"C:\Encrypted-txt.axx"), AxCryptEnvironment.Current.FileInfo(@"C:\Decrypted.txt"), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.Error | ActiveFileStatus.IgnoreChange | ActiveFileStatus.NotShareable, null);
            state.Add(activeFile);

            ActiveFile byDecryptedPath = state.FindDecryptedPath(@"C:\Decrypted.txt");
            Assert.That(byDecryptedPath, Is.EqualTo(activeFile), "The search should return the same instance.");

            ActiveFile byEncryptedPath = state.FindEncryptedPath(@"C:\Encrypted-txt.axx");
            Assert.That(byEncryptedPath, Is.EqualTo(byDecryptedPath), "The search should return the same instance.");

            ActiveFile notFoundEncrypted = state.FindEncryptedPath(@"C:\notfoundfile.txt");
            Assert.That(notFoundEncrypted, Is.Null, "A search that does not succeed should return null.");

            ActiveFile notFoundDecrypted = state.FindDecryptedPath(@"C:\notfoundfile.txt");
            Assert.That(notFoundDecrypted, Is.Null, "A search that does not succeed should return null.");
        }

        [Test]
        public static void TestForEach()
        {
            bool changedEventWasRaised = false;
            FileSystemState state = FileSystemState.Load(AxCryptEnvironment.Current.FileInfo(@"c:\mytemp\mystate.xml"));
            state.Changed += ((object sender, EventArgs e) =>
            {
                changedEventWasRaised = true;
            });

            ActiveFile activeFile;
            activeFile = new ActiveFile(AxCryptEnvironment.Current.FileInfo(@"C:\Encrypted1-txt.axx"), AxCryptEnvironment.Current.FileInfo(@"C:\Decrypted1.txt"), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.Error | ActiveFileStatus.IgnoreChange | ActiveFileStatus.NotShareable, null);
            state.Add(activeFile);
            activeFile = new ActiveFile(AxCryptEnvironment.Current.FileInfo(@"C:\Encrypted2-txt.axx"), AxCryptEnvironment.Current.FileInfo(@"C:\Decrypted2.txt"), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.Error | ActiveFileStatus.IgnoreChange | ActiveFileStatus.NotShareable, null);
            state.Add(activeFile);
            activeFile = new ActiveFile(AxCryptEnvironment.Current.FileInfo(@"C:\Encrypted3-txt.axx"), AxCryptEnvironment.Current.FileInfo(@"C:\Decrypted3.txt"), new AesKey(), ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.Error | ActiveFileStatus.IgnoreChange | ActiveFileStatus.NotShareable, null);
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
    }
}