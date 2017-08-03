using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public class TestFileFilter
    {
        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup();
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public void FileFilterEmptyFilter()
        {
            FileFilter filter = new FileFilter();

            Assert.That(filter.IsEncryptable(new FakeDataStore(@"test.axx")), Is.False);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"test.txt")), Is.True);
        }

        [Test]
        public void FileFilterThrowsArgumentNull()
        {
            FileFilter filter = new FileFilter();

            Assert.Throws<ArgumentNullException>(() => filter.AddUnencryptable(null));
            Assert.Throws<ArgumentNullException>(() => filter.AddUnencryptableExtension(null));
            Assert.Throws<ArgumentNullException>(() => filter.IsEncryptable(null));
        }

        [Test]
        public void FileFilterTestUnencryptablePatterns()
        {
            FileFilter filter = new FileFilter();

            string s = $"\\{Path.DirectorySeparatorChar}";
            filter.AddUnencryptable(new Regex(@"\\\.dropbox$".Replace(@"\\", s)));
            filter.AddUnencryptable(new Regex(@"\\desktop\.ini$".Replace(@"\\", s)));
            filter.AddUnencryptable(new Regex(@".*\.tmp$"));

            Assert.That(filter.IsEncryptable(new FakeDataStore(@"anywhere\.dropbox")), Is.False);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"C:\Somewhere\file.dropbox")), Is.True);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@".dropboxx")), Is.True);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@".dropbo")), Is.True);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"folder\desktop.ini")), Is.False);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"file-desktop.ini")), Is.True);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"anything.tmp")), Is.False);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"C:\anywhere\anything.tmp.tmp")), Is.False);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"C:\anywhere\anything.tmp.tmpx")), Is.True);
        }

        [Test]
        public void FileFilterTestUnencryptableExtension()
        {
            FileFilter filter = new FileFilter();

            filter.AddUnencryptableExtension("gdoc");

            Assert.That(filter.IsEncryptable(new FakeDataStore(@"file.gdoc")), Is.False);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"file..gdoc")), Is.False);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"anywhere\file.gdoc")), Is.False);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"file.gdoc\file.txt")), Is.True);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"filegdoc")), Is.True);
            Assert.That(filter.IsEncryptable(new FakeDataStore(@"filegdoc.txt")), Is.True);
        }
    }
}