using AxCrypt.Abstractions.Algorithm;
using AxCrypt.Core.Algorithm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AxCrypt.Mono.Cryptography
{
    public class Sha256Wrapper : Sha256
    {
        private System.Security.Cryptography.HashAlgorithm _hashAlgorithm;

        public Sha256Wrapper(System.Security.Cryptography.HashAlgorithm hashAlgorithm)
        {
            _hashAlgorithm = hashAlgorithm;
        }

        public override byte[] ComputeHash(byte[] buffer)
        {
            return _hashAlgorithm.ComputeHash(buffer);
        }

        public override byte[] ComputeHash(byte[] buffer, int offset, int count)
        {
            return _hashAlgorithm.ComputeHash(buffer, offset, count);
        }

        public override byte[] ComputeHash(Stream inputStream)
        {
            return _hashAlgorithm.ComputeHash(inputStream);
        }

        public override byte[] Hash()
        {
            return _hashAlgorithm.Hash;
        }

        public override int HashSize
        {
            get { return _hashAlgorithm.HashSize; }
        }

        public override void Initialize()
        {
            _hashAlgorithm.Initialize();
        }

        public override bool CanReuseTransform
        {
            get { return _hashAlgorithm.CanReuseTransform; }
        }

        public override bool CanTransformMultipleBlocks
        {
            get { return _hashAlgorithm.CanTransformMultipleBlocks; }
        }

        public override int InputBlockSize
        {
            get { return _hashAlgorithm.InputBlockSize; }
        }

        public override int OutputBlockSize
        {
            get { return _hashAlgorithm.OutputBlockSize; }
        }

        public override int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            return _hashAlgorithm.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
        }

        public override byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            return _hashAlgorithm.TransformFinalBlock(inputBuffer, inputOffset, inputCount);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _hashAlgorithm.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}