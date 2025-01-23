#region Coypright and GPL License

/*
 * Xecrets Net - Copyright © 2022-2025, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets Net, parts of which in turn are derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * However, this code is not derived from AxCrypt and is separately copyrighted and only licensed as follows unless
 * explicitly licensed otherwise. If you use any part of this code in your software, please see https://www.gnu.org/licenses/
 * for details of what this means for you.
 *
 * Xecrets Net is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets Net is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets Net.  If not, see <https://www.gnu.org/licenses/>.
 *
 * The source repository can be found at https://github.com/ please go there for more information, suggestions and
 * contributions. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Coypright and GPL License

using AxCrypt.Abstractions.Algorithm;
using AxCrypt.Core.Runtime;

namespace Xecrets.Net.Implementation.Cryptography
{
    public class AxCrypt1HmacSha1Wrapper : AxCryptHMACSHA1
    {
        private readonly System.Security.Cryptography.KeyedHashAlgorithm _hmac;

        public AxCrypt1HmacSha1Wrapper(System.Security.Cryptography.KeyedHashAlgorithm hmac)
        {
            _hmac = hmac;
            _hashName = nameof(AxCrypt1HmacSha1Wrapper);
        }

        private string _hashName;

        public override string HashName { get { return _hashName; } set { _hashName = value; } }

        public override byte[] Key()
        {
            return _hmac.Key;
        }

        public override void SetKey(byte[] value)
        {
            _hmac.Key = EnsureBlockSizeForKeyDueToBugInMonoKeyPropertySetter(value);
        }

        private static byte[] EnsureBlockSizeForKeyDueToBugInMonoKeyPropertySetter(byte[] key)
        {
            if (key.Length <= 20)
            {
                return key;
            }
            return System.Security.Cryptography.SHA1.Create().ComputeHash(key);
        }

        public override byte[] ComputeHash(byte[] buffer)
        {
            return _hmac.ComputeHash(buffer);
        }

        public override byte[] ComputeHash(byte[] buffer, int offset, int count)
        {
            return _hmac.ComputeHash(buffer, offset, count);
        }

        public override byte[] ComputeHash(System.IO.Stream inputStream)
        {
            return _hmac.ComputeHash(inputStream);
        }

        public override byte[] Hash()
        {
            return _hmac.Hash ?? throw new InternalErrorException("The Hash was null.");
        }

        public override int HashSize
        {
            get { return _hmac.HashSize; }
        }

        public override void Initialize()
        {
            _hmac.Initialize();
        }

        public override HMAC Initialize(ISymmetricKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Initialize();
            SetKey(key.GetBytes());
            return this;
        }

        public override bool CanReuseTransform
        {
            get { return _hmac.CanReuseTransform; }
        }

        public override bool CanTransformMultipleBlocks
        {
            get { return _hmac.CanTransformMultipleBlocks; }
        }

        public override int InputBlockSize
        {
            get { return _hmac.InputBlockSize; }
        }

        public override int OutputBlockSize
        {
            get { return _hmac.OutputBlockSize; }
        }

        public override int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[]? outputBuffer, int outputOffset)
        {
            return _hmac.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
        }

        public override byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            return _hmac.TransformFinalBlock(inputBuffer, inputOffset, inputCount);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _hmac.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
