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
using AxCrypt.Core.Runtime;
using AxCrypt.Fake;
using NUnit.Framework;
using System;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestSubkey
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

        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.WindowsDesktop)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public static void TestSubkeyMethods(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetup(cryptoImplementation);

            SymmetricKey key = new SymmetricKey(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });
            Subkey subkey = null;
            Assert.Throws<ArgumentNullException>(() =>
            {
                subkey = new Subkey(null, HeaderSubkey.Data);
            });

            Assert.Throws<InternalErrorException>(() =>
            {
                subkey = new Subkey(key, HeaderSubkey.None);
            });

            subkey = new Subkey(key, HeaderSubkey.Data);
            Assert.That(subkey.Key.Size, Is.EqualTo(128), "A Subkey is exactly 16 bytes.");
            Assert.That(subkey.Key.GetBytes(), Is.EquivalentTo(new byte[] { 0x5f, 0xfe, 0x14, 0x56, 0xd5, 0x94, 0xf9, 0x22, 0x42, 0xe3, 0x66, 0x8f, 0x8c, 0xe6, 0xea, 0xc6 }), "Comparing with a pre-calculated value assumed to be correct.");
            subkey = new Subkey(key, HeaderSubkey.Headers);
            Assert.That(subkey.Key.Size, Is.EqualTo(128), "A Subkey is exactly 16 bytes.");
            Assert.That(subkey.Key.GetBytes(), Is.EquivalentTo(new byte[] { 0x1c, 0x81, 0x0e, 0xe7, 0x65, 0xe7, 0x0b, 0x8f, 0x7a, 0xa3, 0x2b, 0x03, 0x05, 0x07, 0xf8, 0x8a }), "Comparing with a pre-calculated value assumed to be correct.");
            subkey = new Subkey(key, HeaderSubkey.Hmac);
            Assert.That(subkey.Key.Size, Is.EqualTo(128), "A Subkey is exactly 16 bytes.");
            Assert.That(subkey.Key.GetBytes(), Is.EquivalentTo(new byte[] { 0xdb, 0xf1, 0x84, 0x11, 0x2e, 0xb9, 0x11, 0x16, 0x59, 0x71, 0x2b, 0xaf, 0xcf, 0xf2, 0xab, 0x24 }), "Comparing with a pre-calculated value assumed to be correct.");
            subkey = new Subkey(key, HeaderSubkey.Validator);
            Assert.That(subkey.Key.Size, Is.EqualTo(128), "A Subkey is exactly 16 bytes.");
            Assert.That(subkey.Key.GetBytes(), Is.EquivalentTo(new byte[] { 0x45, 0x22, 0xa0, 0x3d, 0x98, 0x00, 0x9d, 0x55, 0x45, 0xed, 0x42, 0xfb, 0xd8, 0x35, 0x78, 0xd0 }), "Comparing with a pre-calculated value assumed to be correct.");

            Assert.That(new Subkey(key, HeaderSubkey.Hmac).Key.GetBytes(), Is.EquivalentTo(new Subkey(key, HeaderSubkey.Hmac).Key.GetBytes()), "The subkey generation should be stable.");
        }
    }
}
