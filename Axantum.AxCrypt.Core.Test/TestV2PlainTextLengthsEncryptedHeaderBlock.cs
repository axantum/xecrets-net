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
            Factory.Instance.Singleton<IRuntimeEnvironment>(() => new FakeRuntimeEnvironment());
            Factory.Instance.Singleton<IRandomGenerator>(() => new FakeRandomGenerator());
        }

        [TearDown]
        public static void Teardown()
        {
            Factory.Instance.Clear();
        }

        [Test]
        public static void TestClone()
        {
            V2PlaintextLengthsEncryptedHeaderBlock block = new V2PlaintextLengthsEncryptedHeaderBlock(new V2AesCrypto(new V2Passphrase("secret", 256)));
            V2PlaintextLengthsEncryptedHeaderBlock clone = (V2PlaintextLengthsEncryptedHeaderBlock)block.Clone();

            Assert.That(!Object.ReferenceEquals(block.GetDataBlockBytes(), clone.GetDataBlockBytes()));
            Assert.That(block.GetDataBlockBytes(), Is.EquivalentTo(clone.GetDataBlockBytes()));
        }

        [Test]
        public static void TestPlaintextLength()
        {
            V2PlaintextLengthsEncryptedHeaderBlock block = new V2PlaintextLengthsEncryptedHeaderBlock(new V2AesCrypto(new V2Passphrase("secret", 256)));

            Assert.That(block.PlaintextLength, Is.EqualTo(0));

            block.PlaintextLength = 123;
            block.CompressedPlaintextLength = 456;
            Assert.That(block.PlaintextLength, Is.EqualTo(123));
        }

        [Test]
        public static void TestCompressedPlaintextLength()
        {
            V2PlaintextLengthsEncryptedHeaderBlock block = new V2PlaintextLengthsEncryptedHeaderBlock(new V2AesCrypto(new V2Passphrase("secret", 256)));

            Assert.That(block.CompressedPlaintextLength, Is.EqualTo(0));

            block.PlaintextLength = 123;
            block.CompressedPlaintextLength = 456;
            Assert.That(block.CompressedPlaintextLength, Is.EqualTo(456));
        }
    }
}