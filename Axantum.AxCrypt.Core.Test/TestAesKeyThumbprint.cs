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
            KeyWrapSalt nullSalt = null;
            Assert.Throws<ArgumentNullException>(() => { if (new AesKeyThumbprint(nullKey, new KeyWrapSalt(16), 10) == null) { } });
            Assert.Throws<ArgumentNullException>(() => { if (new AesKeyThumbprint(new AesKey(), nullSalt, 10) == null) { } });
        }

        [Test]
        public static void TestAesKeyThumbprintMethods()
        {
            AesKey key1 = new AesKey(new byte[] { 5, 6, 7, 8, 2, 4, 55, 77, 34, 65, 89, 12, 45, 87, 54, 255 });
            AesKey key2 = new AesKey(new byte[] { 5, 6, 7, 8, 2, 4, 55, 77, 34, 65, 89, 12, 45, 87, 54, 255 });
            KeyWrapSalt salt1 = new KeyWrapSalt(16);
            KeyWrapSalt salt2 = new KeyWrapSalt(salt1.GetBytes());

            AesKeyThumbprint thumbprint1 = new AesKeyThumbprint(key1, salt1, 10);
            AesKeyThumbprint thumbprint2 = new AesKeyThumbprint(key2, salt2, 10);

            Assert.That(thumbprint1 == thumbprint2, "Two thumb prints made from the same key and salt bytes, although different AesKey instances should be equivalent.");

            AesKeyThumbprint thumbprint3 = new AesKeyThumbprint(new AesKey(), new KeyWrapSalt(AesKey.DefaultKeyLength), 10);
            Assert.That(thumbprint2 != thumbprint3, "Two very different keys and salts should not be equivalent.");
        }

        [Test]
        public static void TestComparisons()
        {
            AesKey key1 = new AesKey(new byte[] { 5, 6, 7, 8, 2, 4, 55, 77, 34, 65, 89, 12, 45, 87, 54, 255 });
            AesKey key2 = new AesKey(new byte[] { 5, 6, 7, 8, 2, 4, 55, 77, 34, 65, 89, 12, 45, 87, 54, 255 });
            KeyWrapSalt salt1 = new KeyWrapSalt(AesKey.DefaultKeyLength);
            KeyWrapSalt salt2 = new KeyWrapSalt(AesKey.DefaultKeyLength);

            AesKeyThumbprint thumbprint1a = new AesKeyThumbprint(key1, salt1, 13);
            AesKeyThumbprint thumbprint1a_alias = thumbprint1a;
            AesKeyThumbprint thumbprint1b = new AesKeyThumbprint(key1, salt2, 25);
            AesKeyThumbprint thumbprint2a = new AesKeyThumbprint(key2, salt2, 25);
            AesKeyThumbprint thumbprint2b = new AesKeyThumbprint(key2, salt1, 13);
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
            KeyWrapSalt salt1 = new KeyWrapSalt(AesKey.DefaultKeyLength);
            KeyWrapSalt salt2 = new KeyWrapSalt(AesKey.DefaultKeyLength);

            AesKeyThumbprint thumbprint1a = new AesKeyThumbprint(key1, salt1, 17);
            AesKeyThumbprint thumbprint1b = new AesKeyThumbprint(key1, salt2, 17);
            AesKeyThumbprint thumbprint2a = new AesKeyThumbprint(key2, salt2, 17);

            Assert.That(thumbprint1a.GetHashCode() != thumbprint1b.GetHashCode(), "The salt is different, so the hash code should be different.");
            Assert.That(thumbprint1b.GetHashCode() == thumbprint2a.GetHashCode(), "The keys are equivalent, and the salt the same, so the hash code should be different.");
        }
    }
}