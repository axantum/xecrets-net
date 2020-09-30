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
 * The source is maintained at http://bitbucket.org/AxCrypt.Desktop.Window-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Core.Crypto;
using NUnit.Framework;
using System;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestSalt
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
            Salt salt = null;
            Assert.DoesNotThrow(() =>
            {
                salt = new Salt(0);
                Assert.That(salt.Length, Is.EqualTo(0), "As a special case, zero length salt is supported - equivalent to no salt.");

                salt = new Salt(128);
                Assert.That(salt.Length, Is.EqualTo(16), "The length should be what was asked for.");
                Assert.That(salt.GetBytes(), Is.Not.EquivalentTo(new byte[16]), "A random salt is not likely to be all zeros.");

                salt = new Salt(192);
                Assert.That(salt.Length, Is.EqualTo(24), "The length should be what was asked for.");
                Assert.That(salt.GetBytes(), Is.Not.EquivalentTo(new byte[24]), "A random salt is not likely to be all zeros.");

                salt = new Salt(256);
                Assert.That(salt.Length, Is.EqualTo(32), "The length should be what was asked for.");
                Assert.That(salt.GetBytes(), Is.Not.EquivalentTo(new byte[32]), "A random salt is not likely to be all zeros.");

                salt = new Salt(new byte[16]);
                Assert.That(salt.GetBytes(), Is.EquivalentTo(new byte[16]), "A salt with all zeros was requested.");

                salt = new Salt(new byte[24]);
                Assert.That(salt.GetBytes(), Is.EquivalentTo(new byte[24]), "A salt with all zeros was requested.");

                salt = new Salt(new byte[32]);
                Assert.That(salt.GetBytes(), Is.EquivalentTo(new byte[32]), "A salt with all zeros was requested.");

                salt = new Salt(new byte[0]);
                Assert.That(salt.Length, Is.EqualTo(0), "As a special case, zero length salt is supported - equivalent to no salt.");
            }
            );

            Assert.Throws<ArgumentNullException>(() =>
            {
                salt = new Salt(null);
            });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                salt = new Salt(-128);
            });
        }
    }
}