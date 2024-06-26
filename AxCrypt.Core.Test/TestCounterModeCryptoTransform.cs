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

using AxCrypt.Abstractions;
using AxCrypt.Abstractions.Algorithm;
using AxCrypt.Core.Algorithm;
using AxCrypt.Core.Crypto;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Linq;

using Xecrets.Net.Cryptography;

using static AxCrypt.Abstractions.TypeResolve;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestCounterModeCryptoTransform
    {
        private CryptoImplementation _cryptoImplementation;

        public TestCounterModeCryptoTransform(CryptoImplementation cryptoImplementation)
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
        public void TestConstructorWithBadArguments()
        {
            SymmetricAlgorithm algorithm;
            ICryptoTransform transform = null;

            try
            {
                algorithm = New<Aes>();
                algorithm.Mode = CipherMode.CBC;
                Assert.Throws<ArgumentException>(() => transform = new CtrXecretsCryptoTransform(New<Aes>(), 0, 0));

                algorithm = New<Aes>();
                algorithm.Mode = CipherMode.ECB;
                algorithm.Padding = PaddingMode.PKCS7;
                Assert.Throws<ArgumentException>(() => transform = new CtrXecretsCryptoTransform(algorithm, 0, 0));

                algorithm = New<Aes>();
                algorithm.Mode = CipherMode.ECB;
                algorithm.Padding = PaddingMode.None;
                Assert.DoesNotThrow(() => transform = new CtrXecretsCryptoTransform(algorithm, 0, 0));
            }
            finally
            {
                if (transform != null)
                {
                    transform.Dispose();
                }
            }
        }

        [Test]
        public void TestCanReuseTransform()
        {
            SymmetricAlgorithm algorithm = New<Aes>();
            algorithm.Mode = CipherMode.ECB;
            algorithm.Padding = PaddingMode.None;
            using (ICryptoTransform transform = new CtrXecretsCryptoTransform(algorithm, 0, 0))
            {
                Assert.That(transform.CanReuseTransform);
            }
        }

        [Test]
        public void TestTransformBlockWithBadArgument()
        {
            SymmetricAlgorithm algorithm = New<Aes>();
            algorithm.Mode = CipherMode.ECB;
            algorithm.Padding = PaddingMode.None;
            using (ICryptoTransform transform = new CtrXecretsCryptoTransform(algorithm, 0, 0))
            {
                Assert.Throws<ArgumentException>(() => transform.TransformBlock(new byte[transform.InputBlockSize + 1], 0, transform.InputBlockSize + 1, new byte[transform.InputBlockSize + 1], 0));
                Assert.DoesNotThrow(() => transform.TransformBlock(new byte[transform.InputBlockSize], 0, transform.InputBlockSize, new byte[transform.InputBlockSize], 0));
            }
        }
    }
}
