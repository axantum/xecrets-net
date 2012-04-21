#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Reader;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestFileNameInfoHeaderBlock
    {
        private class FileNameInfoHeaderBlockForTest : FileNameInfoHeaderBlock
        {
            public void SetBadNameWithoutEndingNul()
            {
                byte[] rawFileName = Encoding.ASCII.GetBytes("ABCDEFGHIJK.LMNO");
                byte[] dataBlock = new byte[16];
                rawFileName.CopyTo(dataBlock, 0);
                SetDataBlockBytesReference(HeaderCrypto.Encrypt(dataBlock));
            }
        }

        [Test]
        public static void TestNonTerminatingFileName()
        {
            FileNameInfoHeaderBlockForTest fileInfoHeaderBlock = new FileNameInfoHeaderBlockForTest();
            fileInfoHeaderBlock.HeaderCrypto = new AesCrypto(new AesKey());

            fileInfoHeaderBlock.FileName = "ABCDEFGHIJK.LMN";
            fileInfoHeaderBlock.SetBadNameWithoutEndingNul();

            Assert.Throws<InvalidOperationException>(() =>
            {
                string fileName = fileInfoHeaderBlock.FileName;
            });
        }
    }
}