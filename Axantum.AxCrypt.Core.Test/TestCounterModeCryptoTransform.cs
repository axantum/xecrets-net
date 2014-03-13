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
using System.Linq;
using System.Security.Cryptography;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestCounterModeCryptoTransform
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
        public static void TestConstructorWithBadArguments()
        {
            SymmetricAlgorithm algorithm = new AesManaged();
            ICryptoTransform transform = null;

            try
            {
                algorithm.Mode = CipherMode.CBC;
                Assert.Throws<ArgumentException>(() => transform = new CounterModeCryptoTransform(algorithm, 0, 0));

                algorithm.Mode = CipherMode.ECB;
                algorithm.Padding = PaddingMode.PKCS7;
                Assert.Throws<ArgumentException>(() => transform = new CounterModeCryptoTransform(algorithm, 0, 0));

                algorithm.Padding = PaddingMode.None;
                Assert.DoesNotThrow(() => transform = new CounterModeCryptoTransform(algorithm, 0, 0));
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
        public static void TestCanReuseTransform()
        {
            SymmetricAlgorithm algorithm = new AesManaged();
            algorithm.Mode = CipherMode.ECB;
            algorithm.Padding = PaddingMode.None;
            using (ICryptoTransform transform = new CounterModeCryptoTransform(algorithm, 0, 0))
            {
                Assert.That(transform.CanReuseTransform);
            }
        }

        [Test]
        public static void TestTransformBlockWithBadArgument()
        {
            SymmetricAlgorithm algorithm = new AesManaged();
            algorithm.Mode = CipherMode.ECB;
            algorithm.Padding = PaddingMode.None;
            using (ICryptoTransform transform = new CounterModeCryptoTransform(algorithm, 0, 0))
            {
                Assert.Throws<ArgumentException>(() => transform.TransformBlock(new byte[transform.InputBlockSize + 1], 0, transform.InputBlockSize + 1, new byte[transform.InputBlockSize + 1], 0));
                Assert.DoesNotThrow(() => transform.TransformBlock(new byte[transform.InputBlockSize], 0, transform.InputBlockSize, new byte[transform.InputBlockSize], 0));
            }
        }
    }
}