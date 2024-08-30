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

using AxCrypt.Abstractions;
using AxCrypt.Abstractions.Algorithm;
using AxCrypt.Core.Algorithm;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Extensions;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xecrets.Net.Core.Test.LegacyImplementation;

using static AxCrypt.Abstractions.TypeResolve;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace AxCrypt.Core.Test.CryptoValidation
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestSha256
    {
        private CryptoImplementation _cryptoImplementation;

        public TestSha256(CryptoImplementation cryptoImplementation)
        {
            _cryptoImplementation = cryptoImplementation;
        }

        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup(_cryptoImplementation);

            TypeMap.Register.Singleton<IRandomGenerator>(() => new FakePseudoRandomGenerator());
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public void TestInformalAbc()
        {
            byte[] bytes = Encoding.ASCII.GetBytes("abc");

            Sha256 hash = New<Sha256>();

            byte[] actual;
            actual = hash.ComputeHash(bytes);
            Assert.That(actual, Is.EquivalentTo("BA7816BF 8F01CFEA 414140DE 5DAE2223 B00361A3 96177A9C B410FF61 F20015AD".FromHex()));
        }

        [Test]
        public void TestInformalLongerText()
        {
            byte[] bytes = Encoding.ASCII.GetBytes("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq");

            Sha256 hash = New<Sha256>();

            byte[] actual;
            actual = hash.ComputeHash(bytes);
            Assert.That(actual, Is.EquivalentTo("248D6A61 D20638B8 E5C02693 0C3E6039 A33CE459 64FF2167 F6ECEDD4 19DB06C1".FromHex()));
        }

        [Test]
        public void TestInformalMillionA()
        {
            byte[] bytes = new byte[] { 97, 97, 97, 97, 97, 97, 97 };

            Sha256 hash = New<Sha256>();

            for (int i = 0; i < 1000000; i += bytes.Length)
            {
                if (1000000 - i > bytes.Length)
                {
                    hash.TransformBlock(bytes, 0, bytes.Length, null, 0);
                }
                else
                {
                    hash.TransformBlock(bytes, 0, 1000000 - i, null, 0);
                }
            }
            hash.TransformFinalBlock(new byte[0], 0, 0);

            byte[] actual;
            actual = hash.Hash();
            Assert.That(actual, Is.EquivalentTo("CDC76E5C 9914FB92 81A1C7E2 84D73E67 F1809A48 A497200E 046D39CC C7112CD0".FromHex()));
        }
    }
}
