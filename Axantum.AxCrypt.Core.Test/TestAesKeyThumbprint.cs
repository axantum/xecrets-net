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

            byte[] thumbprint1 = new AesKeyThumbprint(key1, salt1).GetThumbprintBytes();
            byte[] thumbprint2 = new AesKeyThumbprint(key2, salt2).GetThumbprintBytes();

            Assert.That(thumbprint1.IsEquivalentTo(thumbprint2), "Two thumbprints made from the same key and salt bytes, although different AesKey instances should be equivalent.");
            Assert.That(!thumbprint2.IsEquivalentTo(new AesKeyThumbprint(new AesKey(), OS.Current.GetRandomBytes(16)).GetThumbprintBytes()), "To very different keys and salts should not be equivalent.");
        }
    }
}