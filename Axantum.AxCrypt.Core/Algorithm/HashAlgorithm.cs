using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Algorithm
{
    public abstract class HashAlgorithm : ICryptoTransform
    {
        public abstract byte[] ComputeHash(byte[] buffer);

        public abstract byte[] ComputeHash(byte[] buffer, int offset, int count);

        public abstract byte[] ComputeHash(Stream inputStream);

        public abstract byte[] Hash { get; }

        public abstract int HashSize { get; }

        public abstract void Initialize();

        public abstract bool CanReuseTransform { get; }

        public abstract bool CanTransformMultipleBlocks { get; }

        public abstract int InputBlockSize { get; }

        public abstract int OutputBlockSize { get; }

        public abstract int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset);

        public abstract byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount);

        public abstract void Dispose();
    }
}