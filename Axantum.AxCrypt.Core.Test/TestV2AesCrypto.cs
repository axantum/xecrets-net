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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Algorithm;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Portable;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Fake;
using Axantum.AxCrypt.Mono.Portable;
using NUnit.Framework;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestV2AesCrypto
    {
        private static byte[] testPlaintext = new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff };
        private static byte[] testKey = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f };
        private static byte[] testCipertext = new byte[] { 0x8e, 0xa2, 0xb7, 0xca, 0x51, 0x67, 0x45, 0xbf, 0xea, 0xfc, 0x49, 0x90, 0x4b, 0x49, 0x60, 0x89 };

        // Test vectors from NIST Special Publication 800-38A
        private static byte[] nistKey = { 0x60, 0x3d, 0xeb, 0x10, 0x15, 0xca, 0x71, 0xbe, 0x2b, 0x73, 0xae, 0xf0, 0x85, 0x7d, 0x77, 0x81, 0x1f, 0x35, 0x2c, 0x07, 0x3b, 0x61, 0x08, 0xd7, 0x2d, 0x98, 0x10, 0xa3, 0x09, 0x14, 0xdf, 0xf4 };

        private static byte[] nistInitCounter = { 0xf0, 0xf1, 0xf2, 0xf3, 0xf4, 0xf5, 0xf6, 0xf7, 0xf8, 0xf9, 0xfa, 0xfb, 0xfc, 0xfd, 0xfe, 0xff, };

        private static byte[] nistPlaintext1 = { 0x6b, 0xc1, 0xbe, 0xe2, 0x2e, 0x40, 0x9f, 0x96, 0xe9, 0x3d, 0x7e, 0x11, 0x73, 0x93, 0x17, 0x2a, };
        private static byte[] nistCiphertext1 = { 0x60, 0x1e, 0xc3, 0x13, 0x77, 0x57, 0x89, 0xa5, 0xb7, 0xa7, 0xf5, 0x04, 0xbb, 0xf3, 0xd2, 0x28, };

        private static byte[] nistPlaintext2 = { 0xae, 0x2d, 0x8a, 0x57, 0x1e, 0x03, 0xac, 0x9c, 0x9e, 0xb7, 0x6f, 0xac, 0x45, 0xaf, 0x8e, 0x51, };
        private static byte[] nistCiphertext2 = { 0xf4, 0x43, 0xe3, 0xca, 0x4d, 0x62, 0xb5, 0x9a, 0xca, 0x84, 0xe9, 0x90, 0xca, 0xca, 0xf5, 0xc5, };

        private static byte[] nistPlaintext3 = { 0x30, 0xc8, 0x1c, 0x46, 0xa3, 0x5c, 0xe4, 0x11, 0xe5, 0xfb, 0xc1, 0x19, 0x1a, 0x0a, 0x52, 0xef, };
        private static byte[] nistCiphertext3 = { 0x2b, 0x09, 0x30, 0xda, 0xa2, 0x3d, 0xe9, 0x4c, 0xe8, 0x70, 0x17, 0xba, 0x2d, 0x84, 0x98, 0x8d, };

        private static byte[] nistPlaintext4 = { 0xf6, 0x9f, 0x24, 0x45, 0xdf, 0x4f, 0x9b, 0x17, 0xad, 0x2b, 0x41, 0x7b, 0xe6, 0x6c, 0x37, 0x10, };
        private static byte[] nistCiphertext4 = { 0xdf, 0xc9, 0xc5, 0x8d, 0xb6, 0x7a, 0xad, 0xa6, 0x13, 0xc2, 0xdd, 0x08, 0x45, 0x79, 0x41, 0xa6, };

        [SetUp]
        public static void Setup()
        {
            TypeMap.Register.Singleton<IRuntimeEnvironment>(() => new FakeRuntimeEnvironment());
            TypeMap.Register.Singleton<IRandomGenerator>(() => new FakeRandomGenerator());
            TypeMap.Register.Singleton<IPortableFactory>(() => new PortableFactory());
            TypeMap.Register.New<Aes>(() => PortableFactory.AesManaged());
        }

        [TearDown]
        public static void Teardown()
        {
            TypeMap.Register.Clear();
        }

        [Test]
        public static void TestEncrypt()
        {
            ICrypto crypto = new V2AesCrypto(new SymmetricKey(testKey), new SymmetricIV(testPlaintext), 0);

            byte[] zeroPlain = new byte[testCipertext.Length];

            byte[] cipherText = crypto.Encrypt(zeroPlain);

            Assert.That(cipherText.IsEquivalentTo(testCipertext));
        }

        [Test]
        public static void TestEncryptPartialBlock()
        {
            ICrypto crypto = new V2AesCrypto(new SymmetricKey(testKey), new SymmetricIV(testPlaintext), 3);
            byte[] zeroPlain = new byte[5];

            byte[] cipherText = crypto.Encrypt(zeroPlain);
            byte[] partOfTestCipherText = new byte[5];
            Array.Copy(testCipertext, 3, partOfTestCipherText, 0, 5);

            Assert.That(cipherText.IsEquivalentTo(partOfTestCipherText));
        }

        [Test]
        public static void TestEncryptSeveralBlocks()
        {
            byte[] iv = new byte[16];
            Array.Copy(nistInitCounter, iv, 8);

            long blockCounter = nistInitCounter.GetBigEndianValue(8, 8);
            ICrypto crypto;

            crypto = new V2AesCrypto(new SymmetricKey(nistKey), new SymmetricIV(iv), blockCounter << 4);
            byte[] cipherText1 = crypto.Encrypt(nistPlaintext1);
            Assert.That(cipherText1.IsEquivalentTo(nistCiphertext1));

            crypto = new V2AesCrypto(new SymmetricKey(nistKey), new SymmetricIV(iv), (++blockCounter) << 4);
            byte[] cipherText2 = crypto.Encrypt(nistPlaintext2);
            Assert.That(cipherText2.IsEquivalentTo(nistCiphertext2));

            crypto = new V2AesCrypto(new SymmetricKey(nistKey), new SymmetricIV(iv), (++blockCounter) << 4);
            byte[] cipherText3 = crypto.Encrypt(nistPlaintext3);
            Assert.That(cipherText3.IsEquivalentTo(nistCiphertext3));

            crypto = new V2AesCrypto(new SymmetricKey(nistKey), new SymmetricIV(iv), (++blockCounter) << 4);
            byte[] cipherText4 = crypto.Encrypt(nistPlaintext4);
            Assert.That(cipherText4.IsEquivalentTo(nistCiphertext4));
        }

        [Test]
        public static void TestConstructorWithBadArguments()
        {
            SymmetricKey nullKey = null;

            SymmetricKey testKey = new SymmetricKey(128);
            SymmetricIV testIV = new SymmetricIV(128);

            ICrypto crypto = null;
            Assert.Throws<ArgumentNullException>(() => crypto = new V2AesCrypto(nullKey, testIV, 0));

            testKey = new SymmetricKey(64);
            Assert.Throws<ArgumentException>(() => crypto = new V2AesCrypto(testKey, testIV, 0));

            testKey = new SymmetricKey(256);
            testIV = new SymmetricIV(64);
            Assert.Throws<ArgumentException>(() => crypto = new V2AesCrypto(testKey, testIV, 0));

            testIV = new SymmetricIV(128);
            Assert.DoesNotThrow(() => crypto = new V2AesCrypto(testKey, testIV, 0));

            Assert.That(crypto, Is.Not.Null);
        }
    }
}