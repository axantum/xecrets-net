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
using Axantum.AxCrypt.Fake;
using NUnit.Framework;
using System;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestFileInfoEncryptedHeaderBlock
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

        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.WindowsDesktop)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public static void TestNonUtcFileTimes(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetupCrypto(cryptoImplementation);

            FileInfoEncryptedHeaderBlock fileInfoHeaderBlock = new FileInfoEncryptedHeaderBlock(new V1AesCrypto(new V1Aes128CryptoFactory(), new V1DerivedKey(new Passphrase("nonutc")).DerivedKey, SymmetricIV.Zero128));

            DateTime utcNow = OS.Current.UtcNow;
            DateTime localNow = utcNow.ToLocalTime();

            fileInfoHeaderBlock.CreationTimeUtc = localNow;
            Assert.That(fileInfoHeaderBlock.CreationTimeUtc.Kind, Is.EqualTo(DateTimeKind.Utc), "The local time should be converted to UTC by the setter.");
            Assert.That(fileInfoHeaderBlock.CreationTimeUtc, Is.EqualTo(utcNow), "The setter should have set the time to value of local time converted to UTC.");

            fileInfoHeaderBlock.LastAccessTimeUtc = localNow;
            Assert.That(fileInfoHeaderBlock.LastAccessTimeUtc.Kind, Is.EqualTo(DateTimeKind.Utc), "The local time should be converted to UTC by the setter.");
            Assert.That(fileInfoHeaderBlock.LastAccessTimeUtc, Is.EqualTo(utcNow), "The setter should have set the time to value of local time converted to UTC.");

            fileInfoHeaderBlock.LastWriteTimeUtc = localNow;
            Assert.That(fileInfoHeaderBlock.LastWriteTimeUtc.Kind, Is.EqualTo(DateTimeKind.Utc), "The local time should be converted to UTC by the setter.");
            Assert.That(fileInfoHeaderBlock.LastWriteTimeUtc, Is.EqualTo(utcNow), "The setter should have set the time to value of local time converted to UTC.");
        }
    }
}