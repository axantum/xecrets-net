#region Coypright and GPL License

/*
 * Xecrets.Net - Copyright © 2022-2026, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets.Net, parts of which in turn are derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * However, this code is not derived from AxCrypt and is separately copyrighted and only licensed as follows unless
 * explicitly licensed otherwise. If you use any part of this code in your software, please see https://www.gnu.org/licenses/
 * for details of what this means for you.
 *
 * Xecrets.Net is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets.Net is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets.Net.  If not, see <https://www.gnu.org/licenses/>.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Coypright and GPL License

using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.InteropServices;

using AxCrypt.Abstractions.Algorithm;

namespace Xecrets.Net.Cryptography
{
    public class CtrXecretsCryptoTransform : ICryptoTransform
    {
        private SymmetricAlgorithm _algorithm;

        private readonly long _blockCounter;

        private readonly int _blockOffset;

        private readonly int _blockLength;

        private readonly byte[] _iv;

        private ICryptoTransform _cryptoTransform;

        private readonly int _counterOffset;

        private readonly byte[] _counterWorkBlock;

        private readonly byte[] _counterWorkBlocks;

        private readonly byte[] _keyStreamBlocks;

        private readonly byte[] _partialOutputBuffer;

        private const int CounterLength = sizeof(long);

        private const int PreferredBatchLength = 64 * 1024;

        private readonly ulong _ivCounterSuffix;

        private ulong _currentBlockCounter;

        private int _currentBlockOffset;

        public CtrXecretsCryptoTransform(SymmetricAlgorithm algorithm, long blockCounter, int blockOffset)
        {
            ArgumentNullException.ThrowIfNull(algorithm);

            if (algorithm.Mode != CipherMode.ECB)
            {
                algorithm.Clear();
                throw new ArgumentException("The algorithm must be in ECB mode.");
            }
            if (algorithm.Padding != PaddingMode.None)
            {
                algorithm.Clear();
                throw new ArgumentException("The algorithm must be set to work without padding.");
            }
            _blockLength = algorithm.BlockSize / 8;
            if (_blockLength < CounterLength)
            {
                algorithm.Clear();
                throw new ArgumentException($"The algorithm block size must be at least {CounterLength} bytes.");
            }
            if (blockOffset < 0 || blockOffset >= _blockLength)
            {
                algorithm.Clear();
                throw new ArgumentOutOfRangeException(nameof(blockOffset), $"The block offset must be in the range 0 to {_blockLength - 1}.");
            }
            _iv = algorithm.IV();
            if (_iv.Length != _blockLength)
            {
                algorithm.Clear();
                throw new ArgumentException("The IV length must be the same as the algorithm block length.");
            }

            _algorithm = algorithm;
            _blockCounter = blockCounter;
            _blockOffset = blockOffset;

            _cryptoTransform = _algorithm.CreateEncryptingTransform();
            _counterOffset = _blockLength - CounterLength;
            _ivCounterSuffix = BinaryPrimitives.ReadUInt64BigEndian(_iv.AsSpan(_counterOffset, CounterLength));

            _counterWorkBlock = new byte[_blockLength];
            _partialOutputBuffer = new byte[_blockLength];

            int batchBlockCount = _cryptoTransform.CanTransformMultipleBlocks
                ? Math.Max(1, PreferredBatchLength / _blockLength)
                : 1;
            int batchLength = batchBlockCount * _blockLength;
            _counterWorkBlocks = new byte[batchLength];
            _keyStreamBlocks = new byte[batchLength];
            InitializeCounterBlockPrefixes();
            Reset();
        }

        public bool CanReuseTransform { get; } = true;

        public bool CanTransformMultipleBlocks => _cryptoTransform.CanTransformMultipleBlocks;

        public int InputBlockSize => _cryptoTransform.InputBlockSize;

        public int OutputBlockSize => _cryptoTransform.OutputBlockSize;

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if (inputCount % _blockLength != 0)
            {
                throw new ArgumentException("Only whole blocks may be transformed.");
            }

            TransformBlockInternal(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
            return inputCount;
        }

        private void Reset()
        {
            _currentBlockCounter = unchecked((ulong)_blockCounter);
            _currentBlockOffset = _blockOffset;
        }

        // This method is optimized for performance, and some common code are expanded as inline instead of being in a separate method.
        private void TransformBlockInternal(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            // Handle initital partial block
            if (_currentBlockOffset > 0 && inputCount > 0)
            {
                FillCounterBlock(_counterWorkBlock.AsSpan());
                _cryptoTransform.TransformBlock(_counterWorkBlock, 0, _blockLength, _partialOutputBuffer, 0);
                int partialCount = Math.Min(_blockLength - _currentBlockOffset, inputCount);
                for (int i = 0; i < partialCount; ++i)
                {
                    outputBuffer[outputOffset + i] = (byte)(_partialOutputBuffer[_currentBlockOffset + i] ^ inputBuffer[inputOffset + i]);
                }
                _currentBlockOffset += partialCount;
                if (_currentBlockOffset == _blockLength)
                {
                    _currentBlockOffset = 0;
                    unchecked
                    {
                        ++_currentBlockCounter;
                    }
                }
                inputCount -= partialCount;
                inputOffset += partialCount;
                outputOffset += partialCount;
            }

            // Handle all full blocks
            while (inputCount >= _blockLength)
            {
                int bytesToTransform = Math.Min(inputCount - inputCount % _blockLength, _counterWorkBlocks.Length);
                int blocksToTransform = bytesToTransform / _blockLength;

                FillCounterBlocks(blocksToTransform);
                _cryptoTransform.TransformBlock(_counterWorkBlocks, 0, bytesToTransform, _keyStreamBlocks, 0);
                Xor(
                    outputBuffer.AsSpan(outputOffset, bytesToTransform),
                    _keyStreamBlocks.AsSpan(0, bytesToTransform),
                    inputBuffer.AsSpan(inputOffset, bytesToTransform));

                inputCount -= bytesToTransform;
                outputOffset += bytesToTransform;
                inputOffset += bytesToTransform;
            }

            // Handle final partial block
            if (inputCount > 0)
            {
                FillCounterBlock(_counterWorkBlock.AsSpan());
                _cryptoTransform.TransformBlock(_counterWorkBlock, 0, _blockLength, _partialOutputBuffer, 0);
                for (int i = 0; i < inputCount; ++i)
                {
                    outputBuffer[outputOffset + i] = (byte)(_partialOutputBuffer[i] ^ inputBuffer[inputOffset + i]);
                }
                _currentBlockOffset = inputCount;
            }
        }

        private void FillCounterBlocks(int blockCount)
        {
            Span<byte> counterWorkBlocks = _counterWorkBlocks;
            ulong counter = _currentBlockCounter;
            int counterOffset = _counterOffset;

            for (int i = 0; i < blockCount; ++i)
            {
                BinaryPrimitives.WriteUInt64BigEndian(counterWorkBlocks.Slice(counterOffset, CounterLength), _ivCounterSuffix ^ counter);
                counterOffset += _blockLength;
                unchecked
                {
                    ++counter;
                }
            }

            _currentBlockCounter = counter;
        }

        private void InitializeCounterBlockPrefixes()
        {
            if (_counterOffset == 0)
            {
                return;
            }

            ReadOnlySpan<byte> ivPrefix = _iv.AsSpan(0, _counterOffset);
            ivPrefix.CopyTo(_counterWorkBlock);

            for (int offset = 0; offset < _counterWorkBlocks.Length; offset += _blockLength)
            {
                ivPrefix.CopyTo(_counterWorkBlocks.AsSpan(offset, _counterOffset));
            }
        }

        private void FillCounterBlock(Span<byte> counterBlock)
        {
            BinaryPrimitives.WriteUInt64BigEndian(counterBlock.Slice(_counterOffset, CounterLength), _ivCounterSuffix ^ _currentBlockCounter);
        }

        private static void Xor(Span<byte> destination, ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
        {
            int offset = 0;
            int vectorLength = Vector<byte>.Count;

            ref byte destinationRef = ref MemoryMarshal.GetReference(destination);
            ref byte leftRef = ref MemoryMarshal.GetReference(left);
            ref byte rightRef = ref MemoryMarshal.GetReference(right);

            // This is a micro-optimization, cutting some loop overhead, but more importantly perhaps, making it easier
            // for look-ahead and branch prediction.
            int unrolledVectorLength = vectorLength * 4;
            while (offset <= destination.Length - unrolledVectorLength)
            {
                (Vector.LoadUnsafe(ref leftRef, (nuint)offset) ^ Vector.LoadUnsafe(ref rightRef, (nuint)offset))
                    .StoreUnsafe(ref destinationRef, (nuint)offset);
                (Vector.LoadUnsafe(ref leftRef, (nuint)(offset + vectorLength)) ^ Vector.LoadUnsafe(ref rightRef, (nuint)(offset + vectorLength)))
                    .StoreUnsafe(ref destinationRef, (nuint)(offset + vectorLength));
                (Vector.LoadUnsafe(ref leftRef, (nuint)(offset + vectorLength * 2)) ^ Vector.LoadUnsafe(ref rightRef, (nuint)(offset + vectorLength * 2)))
                    .StoreUnsafe(ref destinationRef, (nuint)(offset + vectorLength * 2));
                (Vector.LoadUnsafe(ref leftRef, (nuint)(offset + vectorLength * 3)) ^ Vector.LoadUnsafe(ref rightRef, (nuint)(offset + vectorLength * 3)))
                    .StoreUnsafe(ref destinationRef, (nuint)(offset + vectorLength * 3));
                offset += unrolledVectorLength;
            }

            while (offset <= destination.Length - vectorLength)
            {
                (Vector.LoadUnsafe(ref leftRef, (nuint)offset) ^ Vector.LoadUnsafe(ref rightRef, (nuint)offset))
                    .StoreUnsafe(ref destinationRef, (nuint)offset);
                offset += vectorLength;
            }

            while (offset < destination.Length)
            {
                destination[offset] = (byte)(left[offset] ^ right[offset]);
                ++offset;
            }
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            byte[] outputBuffer = new byte[inputCount];
            TransformBlockInternal(inputBuffer, inputOffset, inputCount, outputBuffer, 0);
            Reset();
            return outputBuffer;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeInternal();
            }
        }

        private void DisposeInternal()
        {
            if (_cryptoTransform != null!)
            {
                _cryptoTransform.Dispose();
                _cryptoTransform = null!;
            }
            if (_algorithm != null!)
            {
                _algorithm.Clear();
                _algorithm = null!;
            }
        }
    }
}
