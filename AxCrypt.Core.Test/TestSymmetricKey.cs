#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions Copyright © 2022-2025, Svante Seleborg, All Rights Reserved.
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
using AxCrypt.Fake;
using NUnit.Framework;
using System;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestSymmetricKey
    {
        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup(CryptoImplementation.WindowsDesktop);
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

        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.WindowsDesktop)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public static void TestThumbprint(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetup(cryptoImplementation);

            Passphrase key1 = new Passphrase("genericPassphrase");

            SymmetricKeyThumbprint originalThumbprint = key1.Thumbprint;
            Assert.That(originalThumbprint, Is.EqualTo(key1.Thumbprint), "The thumbprints should be the same.");
        }
    }
}
