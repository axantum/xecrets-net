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
using Axantum.AxCrypt.Core.Header;
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
    public static class TestV2PlaintextLengthsEncryptedHeaderBlock
    {
        [SetUp]
        public static void Setup()
        {
            TypeMap.Register.Singleton<IRuntimeEnvironment>(() => new FakeRuntimeEnvironment());
            TypeMap.Register.Singleton<IPortableFactory>(() => new PortableFactory());
            TypeMap.Register.Singleton<IRandomGenerator>(() => new FakeRandomGenerator());
            TypeMap.Register.Singleton<CryptoFactory>(() => CreateCryptoFactory());
            TypeMap.Register.New<ICryptoPolicy>(() => new ProCryptoPolicy());
            TypeMap.Register.New<HMACSHA512>(() => PortableFactory.HMACSHA512());
            TypeMap.Register.New<Aes>(() => PortableFactory.AesManaged());
        }

        private static CryptoFactory CreateCryptoFactory()
        {
            CryptoFactory factory = new CryptoFactory();
            factory.Add(() => new V2Aes256CryptoFactory());
            factory.Add(() => new V1Aes128CryptoFactory());

            return factory;
        }

        [TearDown]
        public static void Teardown()
        {
            TypeMap.Register.Clear();
        }

        [Test]
        public static void TestClone()
        {
            V2PlaintextLengthsEncryptedHeaderBlock block = new V2PlaintextLengthsEncryptedHeaderBlock(new V2AesCrypto(new V2DerivedKey(new Passphrase("secret"), 256).DerivedKey, SymmetricIV.Zero128, 0));
            V2PlaintextLengthsEncryptedHeaderBlock clone = (V2PlaintextLengthsEncryptedHeaderBlock)block.Clone();

            Assert.That(!Object.ReferenceEquals(block.GetDataBlockBytes(), clone.GetDataBlockBytes()));
            Assert.That(block.GetDataBlockBytes(), Is.EquivalentTo(clone.GetDataBlockBytes()));
        }

        [Test]
        public static void TestPlaintextLength()
        {
            V2PlaintextLengthsEncryptedHeaderBlock block = new V2PlaintextLengthsEncryptedHeaderBlock(new V2AesCrypto(new V2DerivedKey(new Passphrase("secret"), 256).DerivedKey, SymmetricIV.Zero128, 0));

            Assert.That(block.PlaintextLength, Is.EqualTo(0));

            block.PlaintextLength = 123;
            block.CompressedPlaintextLength = 456;
            Assert.That(block.PlaintextLength, Is.EqualTo(123));
        }

        [Test]
        public static void TestCompressedPlaintextLength()
        {
            V2PlaintextLengthsEncryptedHeaderBlock block = new V2PlaintextLengthsEncryptedHeaderBlock(new V2AesCrypto(new V2DerivedKey(new Passphrase("secret"), 256).DerivedKey, SymmetricIV.Zero128, 0));

            Assert.That(block.CompressedPlaintextLength, Is.EqualTo(0));

            block.PlaintextLength = 123;
            block.CompressedPlaintextLength = 456;
            Assert.That(block.CompressedPlaintextLength, Is.EqualTo(456));
        }
    }
}