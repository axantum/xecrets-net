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
using Axantum.AxCrypt.Core.Runtime;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestV2KeyWrapHeaderBlock
    {
        private static readonly SymmetricKey _keyEncryptingKey128 = new SymmetricKey(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F });
        private static readonly SymmetricKey _keyData128 = new SymmetricKey(new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF });
        private static readonly byte[] _wrapped128 = new byte[] { 0x1F, 0xA6, 0x8B, 0x0A, 0x81, 0x12, 0xB4, 0x47, 0xAE, 0xF3, 0x4B, 0xD8, 0xFB, 0x5A, 0x7B, 0x82, 0x9D, 0x3E, 0x86, 0x23, 0x71, 0xD2, 0xCF, 0xE5 };

        private static readonly SymmetricKey _keyEncryptingKey256 = new SymmetricKey(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F });
        private static readonly byte[] _keyData256 = new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };
        private static readonly byte[] _wrapped256 = new byte[] { 0x28, 0xC9, 0xF4, 0x04, 0xC4, 0xB8, 0x10, 0xF4, 0xCB, 0xCC, 0xB3, 0x5C, 0xFB, 0x87, 0xF8, 0x26, 0x3F, 0x57, 0x86, 0xE2, 0xD8, 0x0E, 0xD3, 0x26, 0xCB, 0xC7, 0xF0, 0xE7, 0x1A, 0x99, 0xF4, 0x3B, 0xFB, 0x98, 0x8B, 0x9B, 0x7A, 0x02, 0xDD, 0x21 };

        [SetUp]
        public static void Setup()
        {
            Factory.Instance.Singleton<IRuntimeEnvironment>(() => new FakeRuntimeEnvironment());
        }

        [TearDown]
        public static void Teardown()
        {
            Factory.Instance.Clear();
        }

        [Test]
        public static void TestConstructorFromBytes()
        {
            byte[] datablock = new byte[248];
            for (int i = 247; i >= 0; --i)
            {
                datablock[247 - i] = (byte)i;
            }

            V2KeyWrapHeaderBlock header = new V2KeyWrapHeaderBlock(datablock);
            Assert.That(header.GetDataBlockBytes(), Is.EquivalentTo(datablock));

            header = null;
            Assert.Throws<ArgumentException>(() => header = new V2KeyWrapHeaderBlock(new byte[247]));
            Assert.Throws<ArgumentNullException>(() => header = new V2KeyWrapHeaderBlock(null));
            Assert.That(header, Is.Null);
        }

        [Test]
        public static void TestConstructorFromKeyUsingKeyWrap128TestVectors()
        {
            var mock = new Mock<IRandomGenerator>();
            mock.Setup<byte[]>(x => x.Generate(It.Is<int>(v => v == 248))).Returns(new byte[248]);
            mock.Setup<byte[]>(x => x.Generate(It.Is<int>(v => v == (16 + 16)))).Returns(_keyData128.GetBytes());
            Factory.Instance.Singleton<IRandomGenerator>(() => mock.Object);

            V2KeyWrapHeaderBlock header = new V2KeyWrapHeaderBlock(new V2AesCrypto(_keyEncryptingKey128, SymmetricIV.Zero128), 6);

            byte[] bytes = header.GetDataBlockBytes();
            byte[] wrapped = new byte[24];
            Array.Copy(bytes, 0, wrapped, 0, wrapped.Length);

            Assert.That(wrapped, Is.EquivalentTo(_wrapped128));
        }

        [Test]
        public static void TestConstructorFromKeyUsingKeyWrap256TestVectors()
        {
            var mock = new Mock<IRandomGenerator>();
            mock.Setup<byte[]>(x => x.Generate(It.Is<int>(v => v == 248))).Returns(new byte[248]);
            mock.Setup<byte[]>(x => x.Generate(It.Is<int>(v => v == (32 + 16)))).Returns(_keyData256);
            Factory.Instance.Singleton<IRandomGenerator>(() => mock.Object);

            V2KeyWrapHeaderBlock header = new V2KeyWrapHeaderBlock(new V2AesCrypto(_keyEncryptingKey256, SymmetricIV.Zero128), 6);

            byte[] bytes = header.GetDataBlockBytes();
            byte[] wrapped = new byte[40];
            Array.Copy(bytes, 0, wrapped, 0, wrapped.Length);

            Assert.That(wrapped, Is.EquivalentTo(_wrapped256));
        }

        [Test]
        public static void TestUnwrapMasterKeyAndIV256WithZeroRandomNumbers()
        {
            var mock = new Mock<IRandomGenerator>();
            mock.Setup<byte[]>(x => x.Generate(It.IsAny<int>())).Returns<int>(v => new byte[v]);
            Factory.Instance.Singleton<IRandomGenerator>(() => mock.Object);

            SymmetricKey keyEncryptingKey = new SymmetricKey(256);
            V2KeyWrapHeaderBlock header = new V2KeyWrapHeaderBlock(new V2AesCrypto(keyEncryptingKey, new SymmetricIV(128)), 250);

            byte[] keyData = header.UnwrapMasterKey(new V2AesCrypto(keyEncryptingKey, new SymmetricIV(128)));
            Assert.That(keyData.Length, Is.EqualTo(48));

            byte[] expectedOriginalKeyData = new byte[48];
            Assert.That(keyData, Is.EquivalentTo(expectedOriginalKeyData));
        }

        [Test]
        public static void TestUnwrapMasterKeyAndIV256WithNonzeroRandomNumbers()
        {
            Factory.Instance.Singleton<IRandomGenerator>(() => new FakeRandomGenerator());

            SymmetricKey keyEncryptingKey = new SymmetricKey(256);
            V2KeyWrapHeaderBlock header = new V2KeyWrapHeaderBlock(new V2AesCrypto(keyEncryptingKey, new SymmetricIV(128)), 125);

            byte[] keyData = header.UnwrapMasterKey(new V2AesCrypto(keyEncryptingKey, new SymmetricIV(128)));
            Assert.That(keyData.Length, Is.EqualTo(48));

            byte[] expectedOriginalKeyData = new byte[48];
            expectedOriginalKeyData[0] = keyData[0];
            for (int i = 1; i < expectedOriginalKeyData.Length; ++i)
            {
                expectedOriginalKeyData[i] = (byte)(expectedOriginalKeyData[i - 1] + 1);
            }
            Assert.That(keyData, Is.EquivalentTo(expectedOriginalKeyData));
        }
    }
}