#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions copyright © 2022-2024, Svante Seleborg, All Rights Reserved.
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

using AxCrypt.Core.Crypto;
using AxCrypt.Core.Header;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Text;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestFileNameInfoEncryptedHeaderBlock
    {
        private class FileNameInfoHeaderBlockForTest : V1FileNameInfoEncryptedHeaderBlock
        {
            public FileNameInfoHeaderBlockForTest()
                : base(new V1AesCrypto(new V1Aes128CryptoFactory(), SymmetricKey.Zero128, SymmetricIV.Zero128))
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
            SetupAssembly.AssemblySetup(CryptoImplementation.WindowsDesktop);
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
            SetupAssembly.AssemblySetup(cryptoImplementation);

            FileNameInfoHeaderBlockForTest fileInfoHeaderBlock = new FileNameInfoHeaderBlockForTest();
            fileInfoHeaderBlock.HeaderCrypto = new V1AesCrypto(new V1Aes128CryptoFactory(), new V1DerivedKey(new Passphrase("nonterminating")).DerivedKey, SymmetricIV.Zero128);

            fileInfoHeaderBlock.FileName = "ABCDEFGHIJK.LMN";
            fileInfoHeaderBlock.SetBadNameWithoutEndingNul();

            Assert.Throws<InvalidOperationException>(() =>
            {
                string fileName = fileInfoHeaderBlock.FileName;

                // Avoid FxCop errors
                Object.Equals(fileName, null);
            });
        }

        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.WindowsDesktop)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public static void TestFileNameInfoAnsiName(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetup(cryptoImplementation);

            V1AesCrypto headerCrypto = new V1AesCrypto(new V1Aes128CryptoFactory(), new V1DerivedKey(new Passphrase("nonterminating")).DerivedKey, SymmetricIV.Zero128);
            V1FileNameInfoEncryptedHeaderBlock fileInfoHeaderBlock = new V1FileNameInfoEncryptedHeaderBlock(headerCrypto);

            fileInfoHeaderBlock.FileName = "Dépôsé.txt";

            Assert.That(fileInfoHeaderBlock.FileName, Is.EqualTo("Dépôsé.txt"));
        }

        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.WindowsDesktop)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public static void TestFileNameInfoAnsiNameWithNonAnsiCharacters(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetup(cryptoImplementation);

            V1AesCrypto headerCrypto = new V1AesCrypto(new V1Aes128CryptoFactory(), new V1DerivedKey(new Passphrase("nonterminating")).DerivedKey, SymmetricIV.Zero128);
            V1FileNameInfoEncryptedHeaderBlock fileInfoHeaderBlock = new V1FileNameInfoEncryptedHeaderBlock(headerCrypto);

            fileInfoHeaderBlock.FileName = "Секретный.txt";

            Assert.That(fileInfoHeaderBlock.FileName, Is.EqualTo("_________.txt"));
        }
    }
}
