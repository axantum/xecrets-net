﻿#region Coypright and License

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

        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.WindowsDesktop)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public static void TestInvalidArguments(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetupCrypto(cryptoImplementation);

            SymmetricKey key = new SymmetricKey(128);
            SymmetricIV iv = new SymmetricIV(128);

            Assert.Throws<ArgumentNullException>(() =>
            {
                if (new V1AesCrypto(new V1Aes128CryptoFactory(), null, SymmetricIV.Zero128) == null) { }
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                if (new V1AesCrypto(new V1Aes128CryptoFactory(), null, iv) == null) { }
            });

            Assert.DoesNotThrow(() =>
            {
                if (new V1AesCrypto(new V1Aes128CryptoFactory(), key, iv) == null) { }
            });
        }
    }
}