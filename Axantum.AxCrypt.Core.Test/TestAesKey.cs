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
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Runtime;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestAesKey
    {
        [Test]
        public static void TestInvalidArguments()
        {
            AesKey key = null;
            Assert.DoesNotThrow(() =>
            {
                key = new AesKey(new byte[16]);
            });
            Assert.DoesNotThrow(() =>
            {
                key = new AesKey(new byte[24]);
            });
            Assert.DoesNotThrow(() =>
            {
                key = new AesKey(new byte[32]);
            });

            Assert.Throws<InternalErrorException>(() =>
            {
                key = new AesKey(new byte[0]);
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                key = new AesKey((byte[])null);
            });

            // Use the instance to avoid FxCop errors.
            Object.Equals(key, null);
        }

        [Test]
        public static void TestMethods()
        {
            AesKey key = new AesKey();
            Assert.That(key.GetBytes().Length, Is.EqualTo(16), "The default key length is 128 bits.");
            Assert.That(key.GetBytes(), Is.Not.EquivalentTo(new byte[16]), "A random key cannot be expected to be all zeros.");

            AesKey specifiedKey = new AesKey(key.GetBytes());
            Assert.That(specifiedKey.GetBytes(), Is.EquivalentTo(key.GetBytes()), "The specified key should contain the bits given to it.");
        }

        [Test]
        public static void TestEquals()
        {
            AesKey key1 = new AesKey(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });
            AesKey key2 = new AesKey(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });
            AesKey key3 = new AesKey();

            Assert.That(!key1.Equals(null), "A key is never equal to a null reference.");
            Assert.That(key1.Equals(key2), "Two different, but equivalent keys should compare equal.");
            Assert.That(!key1.Equals(key3), "Two really different keys should not compare equal.");
        }
    }
}