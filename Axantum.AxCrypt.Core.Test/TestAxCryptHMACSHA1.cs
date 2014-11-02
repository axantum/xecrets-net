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

using Axantum.AxCrypt.Core.Algorithm;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Portable;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Both HMAC and SHA1 are meaningful acronyms and this is the way .NET does the naming.")]
    public class TestAxCryptHMACSHA1
    {
        private CryptoImplementation _cryptoImplementation;

        public TestAxCryptHMACSHA1(CryptoImplementation cryptoImplementation)
        {
            _cryptoImplementation = cryptoImplementation;
        }

        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup();
            SetupAssembly.AssemblySetupCrypto(_cryptoImplementation);
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public void TestInvalidArguments()
        {
            HMAC hmac = null;
            Assert.Throws<ArgumentNullException>(() =>
            {
                hmac = TypeMap.Resolve.New<AxCryptHMACSHA1>().Initialize(null);
            });

            // Use the instance to avoid FxCop errors.
            Object.Equals(hmac, null);
        }

        [Test]
        public void TestMethods()
        {
            SymmetricKey key = new SymmetricKey(128);
            HMAC hmac = TypeMap.Resolve.New<AxCryptHMACSHA1>().Initialize(key);

            Assert.That(hmac.Key, Is.EquivalentTo(key.GetBytes()), "Ensure that we're using the specified key.");
        }
    }
}