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
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Crypto;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestAesKeyThumbprint
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
        public static void TestInvalidArguments()
        {
            AesKey nullKey = null;
            Byte[] nullArray = null;
            Assert.Throws<ArgumentNullException>(() => { if (new AesKeyThumbprint(nullKey, new byte[] { }) == null) { } });
            Assert.Throws<ArgumentNullException>(() => { if (new AesKeyThumbprint(new AesKey(), nullArray) == null) { } });
        }

        [Test]
        public static void TestAesKeyThumbprintMethods()
        {
            AesKey key1 = new AesKey(new byte[] { 5, 6, 7, 8, 2, 4, 55, 77, 34, 65, 89, 12, 45, 87, 54, 255 });
            AesKey key2 = new AesKey(new byte[] { 5, 6, 7, 8, 2, 4, 55, 77, 34, 65, 89, 12, 45, 87, 54, 255 });
            byte[] salt1 = OS.Current.GetRandomBytes(32);
            byte[] salt2 = (byte[])salt1.Clone();

            byte[] thumbprint1 = new AesKeyThumbprint(key1, salt1).GetBytes();
            byte[] thumbprint2 = new AesKeyThumbprint(key2, salt2).GetBytes();

            Assert.That(thumbprint1.IsEquivalentTo(thumbprint2), "Two thumbprints made from the same key and salt bytes, although different AesKey instances should be equivalent.");
            Assert.That(!thumbprint2.IsEquivalentTo(new AesKeyThumbprint(new AesKey(), OS.Current.GetRandomBytes(16)).GetBytes()), "To very different keys and salts should not be equivalent.");
        }

        [Test]
        public static void TestComparisons()
        {
            AesKey key1 = new AesKey(new byte[] { 5, 6, 7, 8, 2, 4, 55, 77, 34, 65, 89, 12, 45, 87, 54, 255 });
            AesKey key2 = new AesKey(new byte[] { 5, 6, 7, 8, 2, 4, 55, 77, 34, 65, 89, 12, 45, 87, 54, 255 });
            byte[] salt1 = OS.Current.GetRandomBytes(32);
            byte[] salt2 = OS.Current.GetRandomBytes(32);

            AesKeyThumbprint thumbprint1a = new AesKeyThumbprint(key1, salt1);
            AesKeyThumbprint thumbprint1a_alias = thumbprint1a;
            AesKeyThumbprint thumbprint1b = new AesKeyThumbprint(key1, salt2);
            AesKeyThumbprint thumbprint2a = new AesKeyThumbprint(key2, salt2);
            AesKeyThumbprint thumbprint2b = new AesKeyThumbprint(key2, salt1);
            AesKeyThumbprint nullThumbprint = null;

            Assert.That(thumbprint1a == thumbprint1a_alias, "Same instance should of course compare equal.");
            Assert.That(nullThumbprint != thumbprint1a, "A null should not compare equal to any other instance.");
            Assert.That(thumbprint1a != nullThumbprint, "A null should not compare equal to any other instance.");
            Assert.That(thumbprint1a == thumbprint2b, "Same raw key and salt, but different instance, should compare equal.");
            Assert.That(thumbprint1b == thumbprint2a, "Same raw key and salt, but different instance, should compare equal.");
            Assert.That(thumbprint1a != thumbprint1b, "Same raw key but different salt, should compare inequal.");
            Assert.That(thumbprint2a != thumbprint2b, "Same raw key but different salt, should compare inequal.");

            object object1a = thumbprint1a;
            object object2b = thumbprint2b;
            Assert.That(object1a.Equals(nullThumbprint), Is.False, "An instance does not equals null.");
            Assert.That(object1a.Equals(object2b), Is.True, "The two instances are equivalent.");

            object badTypeObject = key1;
            Assert.That(object1a.Equals(badTypeObject), Is.False, "The object being compared to is of the wrong type.");
        }

        [Test]
        public static void TestGetHashCode()
        {
            AesKey key1 = new AesKey(new byte[] { 5, 6, 7, 8, 2, 4, 55, 77, 34, 65, 89, 12, 45, 87, 54, 255 });
            AesKey key2 = new AesKey(new byte[] { 5, 6, 7, 8, 2, 4, 55, 77, 34, 65, 89, 12, 45, 87, 54, 255 });
            byte[] salt1 = OS.Current.GetRandomBytes(32);
            byte[] salt2 = OS.Current.GetRandomBytes(32);

            AesKeyThumbprint thumbprint1a = new AesKeyThumbprint(key1, salt1);
            AesKeyThumbprint thumbprint1b = new AesKeyThumbprint(key1, salt2);
            AesKeyThumbprint thumbprint2a = new AesKeyThumbprint(key2, salt2);

            Assert.That(thumbprint1a.GetHashCode() != thumbprint1b.GetHashCode(), "The salt is different, so the hash code should be different.");
            Assert.That(thumbprint1b.GetHashCode() == thumbprint2a.GetHashCode(), "The keys are equivalent, and the salt the same, so the hash code should be different.");
        }

        [Test]
        public static void TestMatchAny()
        {
            AesKey key1 = new AesKey(new byte[] { 5, 6, 7, 8, 2, 4, 55, 77, 34, 65, 89, 12, 45, 87, 54, 255 });
            AesKey key2 = new AesKey(new byte[] { 5, 6, 7, 8, 2, 4, 55, 77, 34, 65, 89, 12, 45, 87, 54, 255 });
            byte[] salt1 = OS.Current.GetRandomBytes(32);
            byte[] salt2 = OS.Current.GetRandomBytes(32);

            AesKeyThumbprint thumbprint1a = new AesKeyThumbprint(key1, salt1);
            AesKeyThumbprint thumbprint1b = new AesKeyThumbprint(key1, salt2);
            AesKeyThumbprint thumbprint2a = new AesKeyThumbprint(key2, salt2);
            AesKeyThumbprint thumbprint2b = new AesKeyThumbprint(key2, salt1);

            Assert.That(thumbprint1a.MatchAny(new AesKeyThumbprint[] { }), Is.False, "No instance can be matched in an empty collection");
            Assert.That(thumbprint1a.MatchAny(new AesKeyThumbprint[] { thumbprint1b, thumbprint2a }), Is.False, "No instance should be matched in this collection");
            Assert.That(thumbprint1a.MatchAny(new AesKeyThumbprint[] { thumbprint1b, thumbprint2a, thumbprint2b }), Is.True, "One instance should be matched in this collection");
        }
    }
}