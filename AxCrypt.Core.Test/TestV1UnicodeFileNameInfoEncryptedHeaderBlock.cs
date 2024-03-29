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

using AxCrypt.Core.Crypto;
using AxCrypt.Core.Header;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Text;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestV1UnicodeFileNameInfoEncryptedHeaderBlock
    {
        private class UnicodeFileNameInfoHeaderBlockForTest : V1UnicodeFileNameInfoEncryptedHeaderBlock
        {
            public UnicodeFileNameInfoHeaderBlockForTest(ICrypto headerCrypto)
                : base(headerCrypto)
            {
            }

            public void SetBadNameWithoutEndingNul()
            {
                byte[] rawFileName = Encoding.ASCII.GetBytes("ABCDEFGHIJK.LMNO");
                byte[] dataBlock = new byte[16];
                rawFileName.CopyTo(dataBlock, 0);
                SetDataBlockBytesReference(HeaderCrypto.Encrypt(dataBlock));
            }
        }

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
        public static void TestNonTerminatingFileName(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetupCrypto(cryptoImplementation);

            UnicodeFileNameInfoHeaderBlockForTest unicodeFileInfoHeaderBlock = new UnicodeFileNameInfoHeaderBlockForTest(new V1AesCrypto(new V1Aes128CryptoFactory(), new V1DerivedKey(new Passphrase("passphrase")).DerivedKey, SymmetricIV.Zero128));

            unicodeFileInfoHeaderBlock.FileName = "ABCDEFGHIJ.LMN";
            unicodeFileInfoHeaderBlock.SetBadNameWithoutEndingNul();

            Assert.Throws<InvalidOperationException>(() =>
            {
                string fileName = unicodeFileInfoHeaderBlock.FileName;

                // Avoid FxCop errors
                Object.Equals(fileName, null);
            });
        }
    }
}