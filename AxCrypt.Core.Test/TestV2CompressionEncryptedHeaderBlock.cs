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
using AxCrypt.Core.Header;
using AxCrypt.Core.Portable;
using AxCrypt.Core.Runtime;
using AxCrypt.Fake;
using AxCrypt.Mono.Portable;
using NUnit.Framework;
using System;
using System.Linq;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestV2CompressionEncryptedHeaderBlock
    {
        [Test]
        public static void TestClone()
        {
            TypeMap.Register.Singleton<IRandomGenerator>(() => new FakeRandomGenerator());
            TypeMap.Register.Singleton<IRuntimeEnvironment>(() => new FakeRuntimeEnvironment());
            TypeMap.Register.Singleton<IPortableFactory>(() => new PortableFactory());
            TypeMap.Register.New<Aes>(() => PortableFactory.AesManaged());

            V2CompressionEncryptedHeaderBlock compressionHeaderBlock = new V2CompressionEncryptedHeaderBlock(new V2AesCrypto(SymmetricKey.Zero256, SymmetricIV.Zero128, 0));
            compressionHeaderBlock.IsCompressed = false;
            Assert.That(compressionHeaderBlock.IsCompressed, Is.False);

            compressionHeaderBlock.IsCompressed = true;
            V2CompressionEncryptedHeaderBlock clone = (V2CompressionEncryptedHeaderBlock)compressionHeaderBlock.Clone();
            Assert.That(clone.IsCompressed, Is.True);

            TypeMap.Register.Clear();
        }
    }
}