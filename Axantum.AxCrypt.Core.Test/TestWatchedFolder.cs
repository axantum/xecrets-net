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
using Axantum.AxCrypt.Core.Session;
using NUnit.Framework;
using System;
using System.Linq;

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
        public static void TestConstructor()
        {
            WatchedFolder watchedFolder = new WatchedFolder(@"C:\folder");
            Assert.That(watchedFolder.Thumbprint, Is.EqualTo(AesKeyThumbprint.Zero));
        }

        [Test]
        public static void TestArgumentNullConstructor()
        {
            string nullString = null;
            WatchedFolder watchedFolder = null;
            AesKeyThumbprint nullThumbprint = null;
            Assert.Throws<ArgumentNullException>(() => { watchedFolder = new WatchedFolder(nullString, AesKeyThumbprint.Zero); });
            Assert.Throws<ArgumentNullException>(() => { watchedFolder = new WatchedFolder(String.Empty, nullThumbprint); });
            Assert.Throws<ArgumentNullException>(() => { watchedFolder = new WatchedFolder(nullString); });
            if (watchedFolder != null) { }
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