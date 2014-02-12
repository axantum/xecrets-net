﻿#region Coypright and License

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
        private static readonly AesKey _keyEncryptingKey128 = new AesKey(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F });
        private static readonly AesKey _keyData128 = new AesKey(new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF });
        private static byte[] _wrapped128 = new byte[] { 0x1F, 0xA6, 0x8B, 0x0A, 0x81, 0x12, 0xB4, 0x47, 0xAE, 0xF3, 0x4B, 0xD8, 0xFB, 0x5A, 0x7B, 0x82, 0x9D, 0x3E, 0x86, 0x23, 0x71, 0xD2, 0xCF, 0xE5 };

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

            Assert.Throws<ArgumentException>(() => new V2KeyWrapHeaderBlock(new byte[247]));
            Assert.Throws<ArgumentNullException>(() => new V2KeyWrapHeaderBlock(null));
        }

        [Test]
        public static void TestConstructorFromKeyUsingKeyWrap128TestVectors()
        {
            var mock = new Mock<IRandomGenerator>();
            mock.Setup<byte[]>(x => x.Generate(It.Is<int>(v => v == 248))).Returns(new byte[248]);
            mock.Setup<byte[]>(x => x.Generate(It.Is<int>(v => v == (16 + 16)))).Returns(_keyData128.GetBytes());

            Factory.Instance.Singleton<IRandomGenerator>(() => mock.Object);

            V2KeyWrapHeaderBlock header = new V2KeyWrapHeaderBlock(_keyEncryptingKey128, 6);

            byte[] bytes = header.GetDataBlockBytes();
            byte[] wrapped = new byte[24];
            Array.Copy(bytes, 0, wrapped, 0, 24);

            Assert.That(wrapped, Is.EquivalentTo(_wrapped128));
        }
    }
}