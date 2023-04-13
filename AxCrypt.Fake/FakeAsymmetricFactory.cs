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
using AxCrypt.Core.Crypto.Asymmetric;

using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace AxCrypt.Fake
{
    public class FakeAsymmetricFactory : IAsymmetricFactory
    {
        private readonly IAsymmetricFactory _factory = new BouncyCastleAsymmetricFactory();

        private readonly string _paddingHashAlgorithm;

        public FakeAsymmetricFactory(string paddingHashAlgorithm)
        {
            _paddingHashAlgorithm = paddingHashAlgorithm;
        }

        public IEnumerable<JsonConverter> GetConverters()
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

        public IAsymmetricKeyPair CreateKeyPair(string publicKeyPem, string privateKeyPem)
        {
            return _factory.CreateKeyPair(publicKeyPem, privateKeyPem);
        }

        public IAsymmetricKeyPair CreateKeyPair(int bits)
        {
            return _factory.CreateKeyPair(bits);
        }

        public IAsymmetricKeyPair CreateKeyPair(byte[] n, byte[] e, byte[] d, byte[] p, byte[] q, byte[] dp, byte[] dq, byte[] qinv)
        {
            return _factory.CreateKeyPair(n, e, d, p, q, dp, dq, qinv);
        }

        public ICryptoHash CreatePaddingHash(int keyBits)
        {
            return new FakePaddingHash(_paddingHashAlgorithm);
        }

        private class FakePaddingHash : ICryptoHash
        {
            [AllowNull]
            private HashAlgorithm _hash;

            private readonly string _paddingHashAlgorithm;

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
                _ = _hash.TransformBlock(buffer, 0, 1, buffer, 0);
            }

            public void BlockUpdate(byte[] input, int offset, int length)
            {
                _ = _hash.TransformBlock(input, offset, length, input, offset);
            }

            public int DoFinal(byte[] output, int offset)
            {
                _ = _hash.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                _hash.Hash!.CopyTo(array: output, offset);
                Reset();
                return _hash.HashSize / 8;
            }

            public void Reset()
            {
                _hash = _paddingHashAlgorithm switch
                {
                    "SHA1" => SHA1.Create(),
                    "MD5" => MD5.Create(),
                    _ => throw new ArgumentException("Unsupported hash algorithm.", nameof(_paddingHashAlgorithm)),
                };
            }
        }
    }
}
