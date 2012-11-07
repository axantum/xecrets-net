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
    public static class TestKeyWrapSalt
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
        public static void TestMethodsEtc()
        {
            KeyWrapSalt salt = null;
            Assert.DoesNotThrow(() =>
            {
                salt = new KeyWrapSalt(16);
                Assert.That(salt.Length, Is.EqualTo(16), "The length should be what was asked for.");
                Assert.That(salt.GetBytes(), Is.Not.EquivalentTo(new byte[16]), "A random salt is not likely to be all zeros.");

                salt = new KeyWrapSalt(24);
                Assert.That(salt.Length, Is.EqualTo(24), "The length should be what was asked for.");
                Assert.That(salt.GetBytes(), Is.Not.EquivalentTo(new byte[24]), "A random salt is not likely to be all zeros.");

                salt = new KeyWrapSalt(32);
                Assert.That(salt.Length, Is.EqualTo(32), "The length should be what was asked for.");
                Assert.That(salt.GetBytes(), Is.Not.EquivalentTo(new byte[32]), "A random salt is not likely to be all zeros.");

                salt = new KeyWrapSalt(new byte[16]);
                Assert.That(salt.GetBytes(), Is.EquivalentTo(new byte[16]), "A salt with all zeros was requested.");

                salt = new KeyWrapSalt(new byte[24]);
                Assert.That(salt.GetBytes(), Is.EquivalentTo(new byte[24]), "A salt with all zeros was requested.");

                salt = new KeyWrapSalt(new byte[32]);
                Assert.That(salt.GetBytes(), Is.EquivalentTo(new byte[32]), "A salt with all zeros was requested.");

                salt = new KeyWrapSalt(new byte[0]);
                Assert.That(salt.Length, Is.EqualTo(0), "As a special case, zero length salt is supported - equivalent to no salt.");
            }
            );

            Assert.Throws<ArgumentNullException>(() =>
            {
                salt = new KeyWrapSalt(null);
            });

            Assert.Throws<InternalErrorException>(() =>
            {
                salt = new KeyWrapSalt(0);
            });

            Assert.Throws<InternalErrorException>(() =>
            {
                salt = new KeyWrapSalt(-16);
            });

            Assert.Throws<InternalErrorException>(() =>
            {
                salt = new KeyWrapSalt(48);
            });

            Assert.Throws<InternalErrorException>(() =>
            {
                salt = new KeyWrapSalt(new byte[12]);
            });
        }
    }
}