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
using AxCrypt.Core.Header;
using AxCrypt.Fake;
using NUnit.Framework;
using System;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestKeyWrap1HeaderBlock
    {
        private class KeyWrap1HeaderBlockForTest : V1KeyWrap1HeaderBlock
        {
            public KeyWrap1HeaderBlockForTest(SymmetricKey key)
                : base(key, 13)
            {
            }

            public void SetValuesDirect(byte[] wrapped, Salt salt, long keyWrapIterations)
            {
                Set(wrapped, salt, keyWrapIterations);
            }
        }

        private CryptoImplementation _cryptoImplementation;

        public TestKeyWrap1HeaderBlock(CryptoImplementation cryptoImplementation)
        {
            _cryptoImplementation = cryptoImplementation;
        }

        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup(_cryptoImplementation);
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public void TestSetBadArguments()
        {
            KeyWrap1HeaderBlockForTest keyWrap1HeaderBlock = new KeyWrap1HeaderBlockForTest(new V1DerivedKey(new Passphrase("passphrase")).DerivedKey);

            Salt okSalt = new Salt(128);
            Salt badSalt = new Salt(256);

            Assert.Throws<ArgumentNullException>(() =>
            {
                keyWrap1HeaderBlock.SetValuesDirect(null, okSalt, 100);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                keyWrap1HeaderBlock.SetValuesDirect(new byte[0], okSalt, 100);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                keyWrap1HeaderBlock.SetValuesDirect(new byte[16], okSalt, 100);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                keyWrap1HeaderBlock.SetValuesDirect(new byte[32], okSalt, 100);
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                keyWrap1HeaderBlock.SetValuesDirect(new byte[24], null, 100);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                keyWrap1HeaderBlock.SetValuesDirect(new byte[24], badSalt, 100);
            });
        }
    }
}
