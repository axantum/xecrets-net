#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions copyright © 2022-2024, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets Cli, but is derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * The changes and additions are separately copyrighted and only licensed under GPL v3 or later as detailed below,
 * unless explicitly licensed otherwise. If you use any part of these changes and additions in your software,
 * please see https://www.gnu.org/licenses/ for details of what this means for you.
 * 
 * Warning: If you are using the original AxCrypt code under a non-GPL v3 or later license, these changes and additions
 * are not included in that license. If you use these changes under those circumstances, all your code becomes subject to
 * the GPL v3 or later license, according to the principle of strong copyleft as applied to GPL v3 or later.
 *
 * Xecrets Cli is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets Cli is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets Cli. If not, see
 * https://www.gnu.org/licenses/.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions, as well for commit history detailing changes and additions that fall under the strong
 * copyleft provisions mentioned above. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Xecrets Cli Copyright and GPL License notice
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
using AxCrypt.Core.Runtime;
using NUnit.Framework;
using System;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestDataHmac
    {
        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup(Fake.CryptoImplementation.WindowsDesktop);
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestInvalidArguments()
        {
            V1Hmac hmac = null;
            Assert.Throws<ArgumentNullException>(() =>
            {
                hmac = new V1Hmac(null);
            });

            Assert.Throws<InternalErrorException>(() =>
            {
                hmac = new V1Hmac(new byte[20]);
            });

            // Use the instance to avoid FxCop errors.
            Object.Equals(hmac, null);
        }

        [Test]
        public static void TestMethods()
        {
            V1Hmac hmac = new V1Hmac(new byte[] { 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 });

            Assert.That(hmac.Length, Is.EqualTo(V1Hmac.RequiredLength), "The length should always be 16.");
            Assert.That(hmac.GetBytes(), Is.EquivalentTo(new byte[] { 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 }), "Check that GetBytes() returns the expected.");
            Assert.That(hmac, Is.EqualTo(new V1Hmac(new byte[] { 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 })), "Check Equals() override.");
            Assert.That(hmac == new V1Hmac(new byte[] { 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 }), Is.True, "Check operator== override.");
            Assert.That(hmac != new V1Hmac(new byte[] { 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 }), Is.False, "Check operator!= override.");

            V1Hmac hmac2 = new V1Hmac(new byte[] { 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 });
            Assert.That(hmac.GetHashCode(), Is.Not.EqualTo(0), "The hash code should not be zero (can be, but it's not in this case).");
            Assert.That(hmac.GetHashCode(), Is.EqualTo(hmac2.GetHashCode()), "The hash code for two different instances with same value should be the same.");

            Assert.That(hmac.Equals(null), Is.False, "An instance is never equal to null.");
            Assert.That(hmac.Equals(new object()), Is.False, "An instance is never equal to another instance of a differing type.");
            V1Hmac hmacSynonym = hmac;
            Assert.That(hmac == hmacSynonym, Is.True, "These should compare equal via reference equality.");
        }
    }
}
