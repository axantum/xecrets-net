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
using System.Security.Cryptography;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestV1AesCrypto
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
            IPassphrase key = new GenericPassphrase(new SymmetricKey(128));
            SymmetricIV iv = new SymmetricIV(128);

            Assert.Throws<ArgumentNullException>(() =>
            {
                if (new V1AesCrypto(null) == null) { }
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                if (new V1AesCrypto(null, iv, CipherMode.CBC, PaddingMode.None) == null) { }
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                if (new V1AesCrypto(key, null, CipherMode.CBC, PaddingMode.None) == null) { }
            });

            Assert.DoesNotThrow(() =>
            {
                if (new V1AesCrypto(key, iv, CipherMode.CBC, PaddingMode.None) == null) { }
            });
        }
    }
}