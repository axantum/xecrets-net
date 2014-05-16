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
using Axantum.AxCrypt.Core.Header;
using NUnit.Framework;
using System;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestKeyWrap1HeaderBlock
    {
        private class KeyWrap1HeaderBlockForTest : V1KeyWrap1HeaderBlock
        {
            public KeyWrap1HeaderBlockForTest(ICrypto crypto)
                : base(crypto, 13)
            {
            }

            public void SetValuesDirect(byte[] wrapped, Salt salt, long keyWrapIterations)
            {
                Set(wrapped, salt, keyWrapIterations);
            }
        }

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
        public static void TestSetBadArguments()
        {
            KeyWrap1HeaderBlockForTest keyWrap1HeaderBlock = new KeyWrap1HeaderBlockForTest(new V1AesCrypto(new V1Passphrase(new Passphrase("passphrase")), SymmetricIV.Zero128));

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