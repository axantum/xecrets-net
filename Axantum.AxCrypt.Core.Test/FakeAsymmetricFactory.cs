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
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Axantum.AxCrypt.Core.Test
{
    internal class FakeAsymmetricFactory : IAsymmetricFactory
    {
        private IAsymmetricFactory _factory = new BouncyCastleAsymmetricFactory();

        private string _paddingHashAlgorithm;

        public FakeAsymmetricFactory(string paddingHashAlgorithm)
        {
            _paddingHashAlgorithm = paddingHashAlgorithm;
        }

        public JsonConverter[] GetConverters()
        {
            return _factory.GetConverters();
        }

        public IAsymmetricPrivateKey CreatePrivateKey(string privateKeyPem)
        {
            return _factory.CreatePrivateKey(privateKeyPem); ;
        }

        public IAsymmetricPublicKey CreatePublicKey(string publicKeyPem)
        {
            return _factory.CreatePublicKey(publicKeyPem);
        }

        public IAsymmetricKeyPair CreateKeyPair(int bits)
        {
            return _factory.CreateKeyPair(bits);
        }

        public IAsymmetricKeyPair CreateKeyPair(byte[] n, byte[] e, byte[] d, byte[] p, byte[] q, byte[] dp, byte[] dq, byte[] qinv)
        {
            return _factory.CreateKeyPair(n, e, d, p, q, dp, dq, qinv);
        }

        public ICryptoHash CreatePaddingHash()
        {
            return new FakePaddingHash(_paddingHashAlgorithm);
        }

        private class FakePaddingHash : ICryptoHash
        {
            private HashAlgorithm _hash;

            private string _paddingHashAlgorithm;

            public FakePaddingHash(string paddingHashAlgorithm)
            {
                _paddingHashAlgorithm = paddingHashAlgorithm;
                Reset();
            }

            public string AlgorithmName
            {
                get { return _paddingHashAlgorithm; }
            }

            public int HashSize
            {
                get { return _hash.HashSize / 8; }
            }

            public int BufferLength
            {
                get { return _hash.InputBlockSize / 8; }
            }

            public void Update(byte input)
            {
                byte[] buffer = new byte[] { input };
                _hash.TransformBlock(buffer, 0, 1, buffer, 0);
            }

            public void BlockUpdate(byte[] input, int offset, int length)
            {
                _hash.TransformBlock(input, offset, length, input, offset);
            }

            public int DoFinal(byte[] output, int offset)
            {
                _hash.TransformFinalBlock(new byte[0], 0, 0);
                _hash.Hash.CopyTo(output, offset);
                Reset();
                return _hash.HashSize / 8;
            }

            public void Reset()
            {
                _hash = HashAlgorithm.Create(_paddingHashAlgorithm);
            }
        }
    }
}