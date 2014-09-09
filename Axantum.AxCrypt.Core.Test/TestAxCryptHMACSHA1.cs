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
using Axantum.AxCrypt.Core.Portable;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Both HMAC and SHA1 are meaningful acronyms and this is the way .NET does the naming.")]
    public static class TestAxCryptHMACSHA1
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
            HMAC hmac = null;
            Assert.Throws<ArgumentNullException>(() =>
            {
                hmac = Instance.Portable.AxCryptHMACSHA1(null);
            });

            // Use the instance to avoid FxCop errors.
            Object.Equals(hmac, null);
        }

        [Test]
        public static void TestMethods()
        {
            SymmetricKey key = new SymmetricKey(128);
            HMAC hmac = Instance.Portable.AxCryptHMACSHA1(key);

            Assert.That(hmac.Key, Is.EquivalentTo(key.GetBytes()), "Ensure that we're using the specified key.");
        }
    }
}