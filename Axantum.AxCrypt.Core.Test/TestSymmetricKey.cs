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
using NUnit.Framework;
using System;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestSymmetricKey
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
            SymmetricKey key = null;
            Assert.DoesNotThrow(() =>
            {
                key = new SymmetricKey(new byte[16]);
            });
            Assert.DoesNotThrow(() =>
            {
                key = new SymmetricKey(new byte[24]);
            });
            Assert.DoesNotThrow(() =>
            {
                key = new SymmetricKey(new byte[32]);
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                key = new SymmetricKey((byte[])null);
            });

            // Use the instance to avoid FxCop errors.
            Object.Equals(key, null);
        }

        [Test]
        public static void TestMethods()
        {
            SymmetricKey key = new SymmetricKey(128);
            Assert.That(key.GetBytes().Length, Is.EqualTo(16), "The default key length is 128 bits.");
            Assert.That(key.GetBytes(), Is.Not.EquivalentTo(new byte[16]), "A random key cannot be expected to be all zeros.");

            SymmetricKey specifiedKey = new SymmetricKey(key.GetBytes());
            Assert.That(specifiedKey.GetBytes(), Is.EquivalentTo(key.GetBytes()), "The specified key should contain the bits given to it.");
        }

        [Test]
        public static void TestEquals()
        {
            SymmetricKey key1 = new SymmetricKey(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });
            SymmetricKey key2 = new SymmetricKey(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });
            SymmetricKey key3 = new SymmetricKey(128);

            Assert.That(!key1.Equals(null), "A key is never equal to a null reference.");
            Assert.That(key1.Equals(key2), "Two different, but equivalent keys should compare equal.");
            Assert.That(!key1.Equals(key3), "Two really different keys should not compare equal.");
        }

        [Test]
        public static void TestObjectEquals()
        {
            object key1 = new SymmetricKey(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });
            object key2 = new SymmetricKey(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });
            object key3 = new SymmetricKey(128);

            Assert.That(!key1.Equals(null), "A key is never equal to a null reference.");
            Assert.That(key1.Equals(key2), "Two different, but equivalent keys should compare equal.");
            Assert.That(!key1.Equals(key3), "Two really different keys should not compare equal.");

            Assert.That(key1.GetHashCode(), Is.EqualTo(key2.GetHashCode()), "The hashcodes should be the same for two different but equivalent keys.");
        }

        [Test]
        public static void TestOperatorEquals()
        {
            SymmetricKey key1 = new SymmetricKey(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });
            SymmetricKey key2 = new SymmetricKey(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });
            SymmetricKey key3 = new SymmetricKey(128);
            SymmetricKey key3alias = key3;
            SymmetricKey nullKey = null;

            Assert.That(key3 == key3alias, "A key is always equal to itself.");
            Assert.That(key1 != nullKey, "A key is never equal to a null reference.");
            Assert.That(nullKey != key1, "A key is never equal to a null reference.");
            Assert.That(key1 == key2, "Two different, but equivalent keys should compare equal.");
            Assert.That(key1 != key3, "Two really different keys should not compare equal.");
        }

        [Test]
        public static void TestThumbprint()
        {
            IPassphrase key1 = new GenericPassphrase("genericPassphrase");

            SymmetricKeyThumbprint originalThumbprint = key1.Thumbprint;
            Assert.That(originalThumbprint, Is.EqualTo(key1.Thumbprint), "The thumbprints should be the same.");
        }
    }
}