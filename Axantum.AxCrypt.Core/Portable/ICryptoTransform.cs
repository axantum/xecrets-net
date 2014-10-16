using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Portable
{
    public interface ICryptoTransform : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether the current transform can be reused.
        /// </summary>
        /// <returns>true if the current transform can be reused; otherwise, false.</returns>
        bool CanReuseTransform { get; }

        /// <summary>
        /// Gets a value indicating whether multiple blocks can be transformed.
        /// </summary>
        /// <returns>true if multiple blocks can be transformed; otherwise, false.</returns>
        bool CanTransformMultipleBlocks { get; }

        /// <summary>
        /// Gets the input block size.
        /// </summary>
        /// <returns>The size of the input data blocks in bytes.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        int InputBlockSize { get; }

        /// <summary>
        /// Gets the output block size.
        /// </summary>
        /// <returns>The size of the output data blocks in bytes.</returns>
        int OutputBlockSize { get; }

        /// <summary>
        /// Transforms the specified region of the input byte array and copies the resulting transform to the specified region of the output byte array.
        /// </summary>
        /// <param name="inputBuffer">The input for which to compute the transform.</param>
        /// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
        /// <param name="outputBuffer">The output to which to write the transform.</param>
        /// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
        /// <returns>
        /// The number of bytes written.
        /// </returns>
        int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset);

        /// <summary>
        /// Transforms the specified region of the specified byte array.
        /// </summary>
        /// <param name="inputBuffer">The input for which to compute the transform.</param>
        /// <param name="inputOffset">The offset into the byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the byte array to use as data.</param>
        /// <returns>
        /// The computed transform.
        /// </returns>
        /// <exception cref="System.NotImplementedException">Implementation only supports whole block processing and ECB.</exception>
        byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount);
    }
}