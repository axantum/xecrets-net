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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Core.Crypto;
using AxCrypt.Core.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxCrypt.Core.Header
{
    public class V2AlgorithmVerifierEncryptedHeaderBlock : EncryptedHeaderBlock
    {
        public V2AlgorithmVerifierEncryptedHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.AlgorithmVerifier, dataBlock)
        {
        }

        public V2AlgorithmVerifierEncryptedHeaderBlock(ICrypto headerCrypto)
            : this(new byte[16])
        {
            HeaderCrypto = headerCrypto;
            SetVerifier(GenerateVerifier());
        }

        public override object Clone()
        {
            V2AlgorithmVerifierEncryptedHeaderBlock block = new V2AlgorithmVerifierEncryptedHeaderBlock((byte[])GetDataBlockBytesReference().Clone());
            return CopyTo(block);
        }

        private static readonly byte[] _complementer = new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, };

        private static byte[] GenerateVerifier()
        {
            byte[] original = Resolve.RandomGenerator.Generate(16);
            byte[] complement = original.Xor(_complementer);

            byte[] verifier = new byte[32];
            original.CopyTo(verifier, 0);
            complement.CopyTo(verifier, 16);

            return verifier;
        }

        private void SetVerifier(byte[] verifier)
        {
            byte[] encryptedBlock = HeaderCrypto.Encrypt(verifier);
            SetDataBlockBytesReference(encryptedBlock);
        }

        private static readonly byte[] _zeros = new byte[16];

        public bool IsVerified
        {
            get
            {
                byte[] rawBlock = HeaderCrypto.Decrypt(GetDataBlockBytesReference());

                if (rawBlock.Length != 32)
                {
                    return false;
                }

                byte[] presumedZeroData = rawBlock.Reduce(16);

                return presumedZeroData.IsEquivalentTo(_zeros);
            }
        }
    }
}