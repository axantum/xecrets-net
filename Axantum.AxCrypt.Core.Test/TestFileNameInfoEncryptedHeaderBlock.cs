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
using NUnit.Framework;
using System;
using System.Text;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestFileNameInfoEncryptedHeaderBlock
    {
        private class FileNameInfoHeaderBlockForTest : V1FileNameInfoEncryptedHeaderBlock
        {
            public FileNameInfoHeaderBlockForTest()
                : base(new V1AesCrypto(new GenericPassphrase(SymmetricKey.Zero128), SymmetricIV.Zero128))
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

        [Test]
        public static void TestNonTerminatingFileName()
        {
            FileNameInfoHeaderBlockForTest fileInfoHeaderBlock = new FileNameInfoHeaderBlockForTest();
            fileInfoHeaderBlock.HeaderCrypto = new V1AesCrypto(new V1Passphrase(new Passphrase("nonterminating")), SymmetricIV.Zero128);

            fileInfoHeaderBlock.FileName = "ABCDEFGHIJK.LMN";
            fileInfoHeaderBlock.SetBadNameWithoutEndingNul();

            Assert.Throws<InvalidOperationException>(() =>
            {
                string fileName = fileInfoHeaderBlock.FileName;

                // Avoid FxCop errors
                Object.Equals(fileName, null);
            });
        }
    }
}