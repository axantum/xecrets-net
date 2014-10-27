using Axantum.AxCrypt.Core.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Mono.Cryptography
{
    internal class AxCryptHMACSHA1Wrapper : Axantum.AxCrypt.Core.Portable.AxCryptHMACSHA1
    {
        private System.Security.Cryptography.HMAC _hmac;

        public AxCryptHMACSHA1Wrapper()
        {
            _hmac = Axantum.AxCrypt.Mono.Cryptography.AxCryptHMACSHA1.Create();
        }

        public override string HashName
        {
            get
            {
                return _hmac.HashName;
            }
            set
            {
                _hmac.HashName = value;
            }
        }

        public override byte[] Key
        {
            get
            {
                return _hmac.Key;
            }
            set
            {
                _hmac.Key = value;
            }
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

        public override byte[] Hash
        {
            get { return _hmac.Hash; }
        }

        public override int HashSize
        {
            get { return _hmac.HashSize; }
        }

        public override void Initialize()
        {
            _hmac.Initialize();
        }

        public override Core.Portable.HMAC Initialize(SymmetricKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            _hmac.Initialize();
            _hmac.Key = key.GetBytes();
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

        public override int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            return _hmac.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
        }

        public override byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            return _hmac.TransformFinalBlock(inputBuffer, inputOffset, inputCount);
        }

        public override void Dispose()
        {
            _hmac.Dispose();
        }
    }
}