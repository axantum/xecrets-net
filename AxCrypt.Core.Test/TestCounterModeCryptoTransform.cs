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

using AxCrypt.Abstractions;
using AxCrypt.Abstractions.Algorithm;
using AxCrypt.Core.Algorithm;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Extensions;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Linq;

using Xecrets.Net.Cryptography;

using static AxCrypt.Abstractions.TypeResolve;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono, typeof(CounterModeCryptoTransform))]
    [TestFixture(CryptoImplementation.Mono, typeof(CtrXecretsCryptoTransform))]
    [TestFixture(CryptoImplementation.WindowsDesktop, typeof(CounterModeCryptoTransform))]
    [TestFixture(CryptoImplementation.WindowsDesktop, typeof(CtrXecretsCryptoTransform))]
    [TestFixture(CryptoImplementation.BouncyCastle, typeof(CounterModeCryptoTransform))]
    [TestFixture(CryptoImplementation.BouncyCastle, typeof(CtrXecretsCryptoTransform))]
    public class TestCounterModeCryptoTransform
    {
        private CryptoImplementation _cryptoImplementation;

        private readonly Type _transformType;

        public TestCounterModeCryptoTransform(CryptoImplementation cryptoImplementation, Type transformType)
        {
            _cryptoImplementation = cryptoImplementation;
            _transformType = transformType;
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
        public void TestConstructorWithBadArguments()
        {
            SymmetricAlgorithm algorithm;
            ICryptoTransform transform = null;

            try
            {
                algorithm = New<Aes>();
                algorithm.Mode = CipherMode.CBC;
                Assert.Throws<ArgumentException>(() => transform = CreateTransform(algorithm, 0, 0));

                algorithm = New<Aes>();
                algorithm.Mode = CipherMode.ECB;
                algorithm.Padding = PaddingMode.PKCS7;
                Assert.Throws<ArgumentException>(() => transform = CreateTransform(algorithm, 0, 0));

                algorithm = New<Aes>();
                algorithm.Mode = CipherMode.ECB;
                algorithm.Padding = PaddingMode.None;
                Assert.DoesNotThrow(() => transform = CreateTransform(algorithm, 0, 0));
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
            using (ICryptoTransform transform = CreateTransform(algorithm, 0, 0))
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
            using (ICryptoTransform transform = CreateTransform(algorithm, 0, 0))
            {
                Assert.Throws<ArgumentException>(() => transform.TransformBlock(new byte[transform.InputBlockSize + 1], 0, transform.InputBlockSize + 1, new byte[transform.InputBlockSize + 1], 0));
                Assert.DoesNotThrow(() => transform.TransformBlock(new byte[transform.InputBlockSize], 0, transform.InputBlockSize, new byte[transform.InputBlockSize], 0));
            }
        }

        [Test]
        public void TestNistSp80038Aes256CtrVector()
        {
            // NIST Special Publication 800-38A, Appendix F.5.5 CTR-AES256.Encrypt:
            // https://nvlpubs.nist.gov/nistpubs/Legacy/SP/nistspecialpublication800-38a.pdf
            byte[] key =
            {
                0x60, 0x3d, 0xeb, 0x10, 0x15, 0xca, 0x71, 0xbe,
                0x2b, 0x73, 0xae, 0xf0, 0x85, 0x7d, 0x77, 0x81,
                0x1f, 0x35, 0x2c, 0x07, 0x3b, 0x61, 0x08, 0xd7,
                0x2d, 0x98, 0x10, 0xa3, 0x09, 0x14, 0xdf, 0xf4,
            };
            byte[] initialCounter =
            {
                0xf0, 0xf1, 0xf2, 0xf3, 0xf4, 0xf5, 0xf6, 0xf7,
                0xf8, 0xf9, 0xfa, 0xfb, 0xfc, 0xfd, 0xfe, 0xff,
            };
            byte[] plaintext =
            {
                0x6b, 0xc1, 0xbe, 0xe2, 0x2e, 0x40, 0x9f, 0x96,
                0xe9, 0x3d, 0x7e, 0x11, 0x73, 0x93, 0x17, 0x2a,
                0xae, 0x2d, 0x8a, 0x57, 0x1e, 0x03, 0xac, 0x9c,
                0x9e, 0xb7, 0x6f, 0xac, 0x45, 0xaf, 0x8e, 0x51,
                0x30, 0xc8, 0x1c, 0x46, 0xa3, 0x5c, 0xe4, 0x11,
                0xe5, 0xfb, 0xc1, 0x19, 0x1a, 0x0a, 0x52, 0xef,
                0xf6, 0x9f, 0x24, 0x45, 0xdf, 0x4f, 0x9b, 0x17,
                0xad, 0x2b, 0x41, 0x7b, 0xe6, 0x6c, 0x37, 0x10,
            };
            byte[] expectedCiphertext =
            {
                0x60, 0x1e, 0xc3, 0x13, 0x77, 0x57, 0x89, 0xa5,
                0xb7, 0xa7, 0xf5, 0x04, 0xbb, 0xf3, 0xd2, 0x28,
                0xf4, 0x43, 0xe3, 0xca, 0x4d, 0x62, 0xb5, 0x9a,
                0xca, 0x84, 0xe9, 0x90, 0xca, 0xca, 0xf5, 0xc5,
                0x2b, 0x09, 0x30, 0xda, 0xa2, 0x3d, 0xe9, 0x4c,
                0xe8, 0x70, 0x17, 0xba, 0x2d, 0x84, 0x98, 0x8d,
                0xdf, 0xc9, 0xc5, 0x8d, 0xb6, 0x7a, 0xad, 0xa6,
                0x13, 0xc2, 0xdd, 0x08, 0x45, 0x79, 0x41, 0xa6,
            };

            byte[] iv = new byte[initialCounter.Length];
            Array.Copy(initialCounter, iv, 8);
            long blockCounter = initialCounter.GetBigEndianValue(8, 8);

            byte[] ciphertext;
            using (ICryptoTransform transform = CreateTransform(key, iv, blockCounter, 0))
            {
                ciphertext = transform.TransformFinalBlock(plaintext, 0, plaintext.Length);
            }

            Assert.That(ciphertext, Is.EqualTo(expectedCiphertext));
        }

        [Test]
        public void TestTransformFinalBlockMatchesChunkedTransform()
        {
            byte[] input = Enumerable.Range(0, 150000).Select(i => (byte)i).ToArray();

            byte[] oneShot;
            using (ICryptoTransform transform = CreateTransform(0, 0))
            {
                oneShot = transform.TransformFinalBlock(input, 0, input.Length);
            }

            byte[] chunked = new byte[input.Length];
            using (ICryptoTransform transform = CreateTransform(0, 0))
            {
                int transformed = transform.TransformBlock(input, 0, 65536, chunked, 0);
                transformed += transform.TransformBlock(input, transformed, 65536, chunked, transformed);
                byte[] final = transform.TransformFinalBlock(input, transformed, input.Length - transformed);
                Array.Copy(final, 0, chunked, transformed, final.Length);
            }

            Assert.That(chunked, Is.EqualTo(oneShot));
        }

        [Test]
        public void TestTransformBlockWithStartBlockOffsetAndOutputOffset()
        {
            byte[] input = Enumerable.Range(0, 16).Select(i => (byte)(i + 11)).ToArray();
            byte[] expected;
            using (ICryptoTransform transform = CreateTransform(7, 5))
            {
                expected = transform.TransformFinalBlock(input, 0, input.Length);
            }

            byte[] output = Enumerable.Repeat((byte)0xa5, 32).ToArray();
            using (ICryptoTransform transform = CreateTransform(7, 5))
            {
                int written = transform.TransformBlock(input, 0, input.Length, output, 9);
                Assert.That(written, Is.EqualTo(input.Length));
            }

            Assert.That(output.Take(9), Is.All.EqualTo(0xa5));
            Assert.That(output.Skip(9).Take(input.Length), Is.EqualTo(expected));
            Assert.That(output.Skip(9 + input.Length), Is.All.EqualTo(0xa5));
        }

        private ICryptoTransform CreateTransform(long startCounter, int startCounterBlockOffset)
        {
            SymmetricAlgorithm algorithm = New<Aes>();
            algorithm.Mode = CipherMode.ECB;
            algorithm.Padding = PaddingMode.None;
            algorithm.SetKey(Enumerable.Range(0, 16).Select(i => (byte)(i + 1)).ToArray());
            algorithm.SetIV(Enumerable.Range(0, 16).Select(i => (byte)(0xf0 - i)).ToArray());
            return CreateTransform(algorithm, startCounter, startCounterBlockOffset);
        }

        private ICryptoTransform CreateTransform(byte[] key, byte[] iv, long blockCounter, int blockOffset)
        {
            SymmetricAlgorithm algorithm = New<Aes>();
            algorithm.Mode = CipherMode.ECB;
            algorithm.Padding = PaddingMode.None;
            algorithm.SetKey(key);
            algorithm.SetIV(iv);
            return CreateTransform(algorithm, blockCounter, blockOffset);
        }

        private ICryptoTransform CreateTransform(SymmetricAlgorithm algorithm, long blockCounter, int blockOffset)
        {
            if (_transformType == typeof(CounterModeCryptoTransform))
            {
                return new CounterModeCryptoTransform(algorithm, blockCounter, blockOffset);
            }
            if (_transformType == typeof(CtrXecretsCryptoTransform))
            {
                return new CtrXecretsCryptoTransform(algorithm, blockCounter, blockOffset);
            }

            throw new InvalidOperationException($"Unsupported transform type '{_transformType.FullName}'.");
        }
    }
}
