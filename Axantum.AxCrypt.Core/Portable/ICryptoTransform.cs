using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Portable
{
    public interface ICryptoTransform : IDisposable
    {
        bool CanReuseTransform { get; }

        bool CanTransformMultipleBlocks { get; }

        int InputBlockSize { get; }

        int OutputBlockSize { get; }

        int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset);

        byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount);
    }
}