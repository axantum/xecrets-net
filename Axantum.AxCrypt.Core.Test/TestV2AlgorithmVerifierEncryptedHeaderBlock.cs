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

using Axantum.AxCrypt.Core.Algorithm;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.Portable;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Fake;
using Axantum.AxCrypt.Mono.Portable;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestV2AlgorithmVerifierEncryptedHeaderBlock
    {
        private CryptoImplementation _cryptoImplementation;

        public TestV2AlgorithmVerifierEncryptedHeaderBlock(CryptoImplementation cryptoImplementation)
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
        public void TestClone()
        {
            V2AlgorithmVerifierEncryptedHeaderBlock headerBlock = new V2AlgorithmVerifierEncryptedHeaderBlock(new V2AesCrypto(SymmetricKey.Zero256, SymmetricIV.Zero128, 0));
            Assert.That(headerBlock.IsVerified, Is.True, "A properly instantiated header should always have IsVerified as true");

            V2AlgorithmVerifierEncryptedHeaderBlock clone = (V2AlgorithmVerifierEncryptedHeaderBlock)headerBlock.Clone();
            Assert.That(clone.IsVerified, Is.True, "The clone should also be properly verified.");
        }

        [Test]
        public void TestOkVerification()
        {
            SymmetricKey key = new SymmetricKey(Resolve.RandomGenerator.Generate(32));
            SymmetricIV iv = new SymmetricIV(Resolve.RandomGenerator.Generate(16));
            V2AlgorithmVerifierEncryptedHeaderBlock headerBlock = new V2AlgorithmVerifierEncryptedHeaderBlock(new V2AesCrypto(key, iv, 0));
            Assert.That(headerBlock.IsVerified, Is.True, "A properly instantiated header should always have IsVerified as true");

            byte[] dataBlock = headerBlock.GetDataBlockBytes();

            V2AlgorithmVerifierEncryptedHeaderBlock newBlock = new V2AlgorithmVerifierEncryptedHeaderBlock(dataBlock);
            newBlock.HeaderCrypto = new V2AesCrypto(key, iv, 0);
            Assert.That(newBlock.IsVerified, Is.True, "It's a new block with the same key, should be ok.");
        }

        [Test]
        public void TestFailedVerification()
        {
            SymmetricKey key = new SymmetricKey(Resolve.RandomGenerator.Generate(32));
            SymmetricIV iv = new SymmetricIV(Resolve.RandomGenerator.Generate(16));
            V2AlgorithmVerifierEncryptedHeaderBlock headerBlock = new V2AlgorithmVerifierEncryptedHeaderBlock(new V2AesCrypto(key, iv, 0));
            Assert.That(headerBlock.IsVerified, Is.True, "A properly instantiated header should always have IsVerified as true");

            byte[] dataBlock = headerBlock.GetDataBlockBytes();

            V2AlgorithmVerifierEncryptedHeaderBlock newBlock = new V2AlgorithmVerifierEncryptedHeaderBlock(dataBlock);

            SymmetricKey key128 = new SymmetricKey(Resolve.RandomGenerator.Generate(16));
            newBlock.HeaderCrypto = new V2AesCrypto(key128, iv, 0);
            Assert.That(newBlock.IsVerified, Is.False, "Wrong algorithm, should not verify.");

            SymmetricKey wrongKey256 = new SymmetricKey(Resolve.RandomGenerator.Generate(32));
            newBlock.HeaderCrypto = new V2AesCrypto(wrongKey256, iv, 0);
            Assert.That(newBlock.IsVerified, Is.False, "Wrong key, should not verify.");

            SymmetricIV wrongIv = new SymmetricIV(Resolve.RandomGenerator.Generate(16));
            newBlock.HeaderCrypto = new V2AesCrypto(key, wrongIv, 0);
            Assert.That(newBlock.IsVerified, Is.False, "Wrong IV, should not verify.");

            newBlock.HeaderCrypto = new V2AesCrypto(key, iv, 256);
            Assert.That(newBlock.IsVerified, Is.False, "Wrong key stream index, should not verify.");

            newBlock.HeaderCrypto = new V2AesCrypto(key, iv, 0);
            Assert.That(newBlock.IsVerified, Is.True, "Everything is as it should, so it should verify.");
        }
    }
}