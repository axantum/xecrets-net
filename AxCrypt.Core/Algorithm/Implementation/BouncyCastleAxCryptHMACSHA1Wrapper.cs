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

using AxCrypt.Abstractions.Algorithm;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace AxCrypt.Core.Algorithm.Implementation
{
    public class BouncyCastleAxCryptHmacSha1Wrapper : AxCryptHMACSHA1
    {
        private readonly HMac _hmac;

        public BouncyCastleAxCryptHmacSha1Wrapper()
        {
            _hmac = new HMac(new BouncyCastleAxCryptSha1ForHmacWrapper(new Sha1Digest()));
        }

        public override HMAC Initialize(ISymmetricKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            SetKey(key.GetBytes());
            return this;
        }

        public override string HashName
        {
            get
            {
                return _hmac.AlgorithmName;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        [AllowNull]
        private byte[] _key;

        public override byte[] Key()
        {
            return (byte[])_key.Clone();
        }

        public override void SetKey(byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            _key = (byte[])value.Clone();
            _hmac.Init(new KeyParameter(_key));
        }

        public override byte[] ComputeHash(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            _hmac.Init(new KeyParameter(_key));
            _hmac.BlockUpdate(buffer, 0, buffer.Length);
            return Hash();
        }

        public override byte[] ComputeHash(byte[] buffer, int offset, int count)
        {
            _hmac.BlockUpdate(buffer, offset, count);
            return buffer;
        }

        public override byte[] ComputeHash(System.IO.Stream inputStream)
        {
            throw new NotImplementedException();
        }

        [AllowNull]
        private byte[] _hash;

        public override byte[] Hash()
        {
            if (_hash == null)
            {
                _hash = new byte[OutputBlockSize];
                _ = _hmac.DoFinal(_hash, 0);
            }
            return (byte[])_hash.Clone();
        }

        public override int HashSize
        {
            get
            {
                return _hmac.GetMacSize();
            }
        }

        public override void Initialize()
        {
            _hmac.Init(new KeyParameter(_key));
            _hash = null;
        }

        public override bool CanReuseTransform
        {
            get { return true; }
        }

        public override bool CanTransformMultipleBlocks
        {
            get { return true; }
        }

        public override int InputBlockSize
        {
            get { return _hmac.GetUnderlyingDigest().GetByteLength(); }
        }

        public override int OutputBlockSize
        {
            get { return _hmac.GetUnderlyingDigest().GetDigestSize(); }
        }

        public override int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[]? outputBuffer, int outputOffset)
        {
            _hmac.BlockUpdate(inputBuffer, inputOffset, inputCount);
            return inputCount;
        }

        public override byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            _hmac.BlockUpdate(inputBuffer, inputOffset, inputCount);
            return Hash();
        }
    }
}
