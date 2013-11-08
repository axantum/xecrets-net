using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Session;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Test
{
    public static class TestWatchedFolder
    {
        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestArgumentNullConstructor()
        {
            string nullString = null;
            WatchedFolder watchedFolder;
            Assert.Throws<ArgumentNullException>(() => { watchedFolder = new WatchedFolder(nullString, AesKeyThumbprint.Zero); });
        }

        [Test]
        public static void TestEquals()
        {
            WatchedFolder watchedFolder1a = new WatchedFolder(@"c:\test1", AesKeyThumbprint.Zero);
            WatchedFolder watchedFolder1aReference = watchedFolder1a;
            WatchedFolder watchedFolder1b = new WatchedFolder(@"c:\test1", AesKeyThumbprint.Zero);
            WatchedFolder watchedFolder2 = new WatchedFolder(@"c:\test2", AesKeyThumbprint.Zero);
            WatchedFolder nullWatchedFolder = null;

            Assert.That(watchedFolder1a.Equals(watchedFolder1aReference), "Reference equality should make them equal.");
            Assert.That(watchedFolder1a.Equals(watchedFolder1b), "Value comparison should make them equal.");
            Assert.That(!watchedFolder1a.Equals(nullWatchedFolder), "Never equal to null.");
            Assert.That(!watchedFolder1a.Equals(watchedFolder2), "Different values, not equal.");
        }

        [Test]
        public static void TestGetHashCode()
        {
            WatchedFolder watchedFolder1a = new WatchedFolder(@"c:\test1", AesKeyThumbprint.Zero);
            WatchedFolder watchedFolder1b = new WatchedFolder(@"c:\test1", AesKeyThumbprint.Zero);
            WatchedFolder watchedFolder2 = new WatchedFolder(@"c:\test2", AesKeyThumbprint.Zero);

            Assert.That(watchedFolder1a.GetHashCode(), Is.EqualTo(watchedFolder1b.GetHashCode()), "Different instances - same hash code.");
            Assert.That(watchedFolder1a.GetHashCode(), Is.Not.EqualTo(watchedFolder2.GetHashCode()), "Different values - different hash code.");
        }

        [Test]
        public static void TestOperatorOverloads()
        {
            WatchedFolder watchedFolder1a = new WatchedFolder(@"c:\test1", AesKeyThumbprint.Zero);
            WatchedFolder watchedFolder1b = new WatchedFolder(@"c:\test1", AesKeyThumbprint.Zero);
            WatchedFolder watchedFolder2 = new WatchedFolder(@"c:\test2", AesKeyThumbprint.Zero);

            Assert.That(watchedFolder1a == watchedFolder1b, Is.True, "Different instances, same value.");
            Assert.That(watchedFolder1a != watchedFolder2, Is.True, "Different values, not same.");
        }

        [Test]
        public static void TestObjectEquals()
        {
            object watchedFolder1a = new WatchedFolder(@"c:\test1", AesKeyThumbprint.Zero);
            object watchedFolder1aReference = watchedFolder1a;
            object watchedFolder1b = new WatchedFolder(@"c:\test1", AesKeyThumbprint.Zero);
            object watchedFolder2 = @"c:\test1";
            object nullObject = null;

            Assert.That(watchedFolder1a.Equals(watchedFolder1b), Is.True, "Different instances, same value.");
            Assert.That(watchedFolder1a.Equals(watchedFolder1aReference), Is.True, "Same instance.");
            Assert.That(watchedFolder1a.Equals(watchedFolder2), Is.False, "Different values");
            Assert.That(watchedFolder1a.Equals(watchedFolder2), Is.False, "Different types.");
            Assert.That(watchedFolder1a.Equals(nullObject), Is.False, "Null is not equal to anything but null.");
        }

        [Test]
        public static void TestDispose()
        {
            WatchedFolder watchedFolder = new WatchedFolder(@"c:\test1", AesKeyThumbprint.Zero);

            Assert.DoesNotThrow(() => watchedFolder.Dispose());
            Assert.DoesNotThrow(() => watchedFolder.Dispose());
        }
    }
}