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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

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
            using (WatchedFolder watchedFolder = new WatchedFolder(@"C:\folder", IdentityPublicTag.Empty))
            {
                Assert.That(watchedFolder.Tag.Matches(IdentityPublicTag.Empty));
            }
        }

        [Test]
        public static void TestArgumentNullConstructor()
        {
            string nullString = null;
            WatchedFolder watchedFolder = null;
            IdentityPublicTag nullTag = null;
            Assert.Throws<ArgumentNullException>(() => { watchedFolder = new WatchedFolder(nullString, IdentityPublicTag.Empty); });
            Assert.Throws<ArgumentNullException>(() => { watchedFolder = new WatchedFolder(String.Empty, nullTag); });
            Assert.Throws<ArgumentNullException>(() => { watchedFolder = new WatchedFolder(nullString, IdentityPublicTag.Empty); });
            if (watchedFolder != null) { }
        }

        [Test]
        public static void TestEquals()
        {
            using (WatchedFolder watchedFolder1a = new WatchedFolder(@"c:\test1", IdentityPublicTag.Empty))
            {
                WatchedFolder watchedFolder1aReference = watchedFolder1a;
                using (WatchedFolder watchedFolder1b = new WatchedFolder(@"c:\test1", IdentityPublicTag.Empty))
                {
                    using (WatchedFolder watchedFolder2 = new WatchedFolder(@"c:\test2", IdentityPublicTag.Empty))
                    {
                        WatchedFolder nullWatchedFolder = null;

                        Assert.That(watchedFolder1a.Equals(watchedFolder1aReference), "Reference equality should make them equal.");
                        Assert.That(!watchedFolder1a.Equals(watchedFolder1b), "Value comparison should not make them equal.");
                        Assert.That(!watchedFolder1a.Equals(nullWatchedFolder), "Never equal to null.");
                        Assert.That(!watchedFolder1a.Equals(watchedFolder2), "Not same reference, not equal.");
                    }
                }
            }
        }

        [Test]
        public static void TestGetHashCode()
        {
            using (WatchedFolder watchedFolder1a = new WatchedFolder(@"c:\test1", IdentityPublicTag.Empty))
            {
                WatchedFolder watchedFolder1aReference = watchedFolder1a;
                using (WatchedFolder watchedFolder1b = new WatchedFolder(@"c:\test1", IdentityPublicTag.Empty))
                {
                    WatchedFolder watchedFolder1bReference = watchedFolder1b;
                    using (WatchedFolder watchedFolder2 = new WatchedFolder(@"c:\test2", IdentityPublicTag.Empty))
                    {
                        WatchedFolder watchedFolder2Reference = watchedFolder2;

                        Assert.That(watchedFolder1a.GetHashCode(), Is.Not.EqualTo(watchedFolder1b.GetHashCode()), "Different instances - different hash code.");
                        Assert.That(watchedFolder1a.GetHashCode(), Is.Not.EqualTo(watchedFolder1b.GetHashCode()), "Different instances - different hash code.");
                        Assert.That(watchedFolder1a.GetHashCode(), Is.Not.EqualTo(watchedFolder2.GetHashCode()), "Different values - different hash code.");

                        Assert.That(watchedFolder1a.GetHashCode(), Is.EqualTo(watchedFolder1aReference.GetHashCode()), "Same reference - same hash code.");
                        Assert.That(watchedFolder1b.GetHashCode(), Is.EqualTo(watchedFolder1bReference.GetHashCode()), "Same reference - same hash code.");
                        Assert.That(watchedFolder2.GetHashCode(), Is.EqualTo(watchedFolder2Reference.GetHashCode()), "Same reference - same hash code.");
                    }
                }
            }
        }

        [Test]
        public static void TestOperatorOverloads()
        {
            using (WatchedFolder watchedFolder1a = new WatchedFolder(@"c:\test1", IdentityPublicTag.Empty))
            {
                WatchedFolder watchedFolder1aReference = watchedFolder1a;
                using (WatchedFolder watchedFolder1b = new WatchedFolder(@"c:\test1", IdentityPublicTag.Empty))
                {
                    WatchedFolder watchedFolder1bReference = watchedFolder1b;
                    using (WatchedFolder watchedFolder2 = new WatchedFolder(@"c:\test2", IdentityPublicTag.Empty))
                    {
                        WatchedFolder watchedFolder2Reference = watchedFolder2;

                        Assert.That(watchedFolder1a != watchedFolder1b, "Different instances, not same.");
                        Assert.That(watchedFolder1a != watchedFolder2, "Different values, not same.");
                        Assert.That(watchedFolder1aReference == watchedFolder1a, "Same instance, same");
                        Assert.That(watchedFolder1bReference == watchedFolder1b, "Same instance, same");
                        Assert.That(watchedFolder2Reference == watchedFolder2, "Same instance, same");
                    }
                }
            }
        }

        [Test]
        public static void TestObjectEquals()
        {
            using (WatchedFolder watchedFolder1aTyped = new WatchedFolder(@"c:\test1", IdentityPublicTag.Empty))
            {
                object watchedFolder1a = watchedFolder1aTyped;
                object watchedFolder1aReference = watchedFolder1a;
                using (WatchedFolder watchedFolder1bTyped = new WatchedFolder(@"c:\test1", IdentityPublicTag.Empty))
                {
                    object watchedFolder1b = watchedFolder1bTyped;
                    object watchedFolder2 = @"c:\test1";
                    object nullObject = null;

                    Assert.That(watchedFolder1a.Equals(watchedFolder1b), Is.False, "Different instances, different value.");
                    Assert.That(watchedFolder1a.Equals(watchedFolder1aReference), Is.True, "Same instance.");
                    Assert.That(watchedFolder1a.Equals(watchedFolder2), Is.False, "Different values");
                    Assert.That(watchedFolder1a.Equals(watchedFolder2), Is.False, "Different types.");
                    Assert.That(watchedFolder1a.Equals(nullObject), Is.False, "Null is not equal to anything but null.");
                }
            }
        }

        [Test]
        public static void TestDispose()
        {
            Assert.DoesNotThrow(() =>
            {
                using (WatchedFolder watchedFolder = new WatchedFolder(@"c:\test1", IdentityPublicTag.Empty))
                {
                    Assert.DoesNotThrow(() => watchedFolder.Dispose());
                }
            });
        }
    }
}