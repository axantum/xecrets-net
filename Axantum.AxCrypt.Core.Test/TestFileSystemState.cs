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
            Assert.That(state, Is.Not.Null, "An instance should always be instantiated.");
            Assert.That(state.ActiveFiles.Count(), Is.EqualTo(1), "The reloaded state should have one active file.");
            Assert.That(state.ActiveFiles.First().ThumbprintMatch(activeFile.Key), Is.True, "The reloaded thumbprint should  match the key.");
        }
    }
}