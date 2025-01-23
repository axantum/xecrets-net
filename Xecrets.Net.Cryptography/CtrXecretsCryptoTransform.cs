#region Coypright and GPL License

/*
 * Xecrets Cli - Copyright © 2022-2025, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets Cli, parts of which in turn are derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * However, this code is not derived from AxCrypt and is separately copyrighted and only licensed as follows unless
 * explicitly licensed otherwise. If you use any part of this code in your software, please see https://www.gnu.org/licenses/
 * for details of what this means for you.
 *
 * Xecrets Cli is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets Cli is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets Cli.  If not, see <https://www.gnu.org/licenses/>.
 *
 * The source repository can be found at https://github.com/ please go there for more information, suggestions and
 * contributions. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Coypright and GPL License

using System.Diagnostics.CodeAnalysis;

using AxCrypt.Abstractions.Algorithm;

namespace Xecrets.Net.Cryptography
{
    public class CtrXecretsCryptoTransform : ICryptoTransform
    {
        private SymmetricAlgorithm _algorithm;

        private readonly long _startCounter;

        private readonly int _startCounterBlockOffset;

        private readonly int _blockLength;

        private int _counterBlockOffset;

        private readonly byte[] _iv;

        private ICryptoTransform _cryptoTransform;

        private readonly byte[] _counterWorkBlock;

        private const int CounterLength = sizeof(long);

        [AllowNull]
        private byte[] _counterBytes;

        public CtrXecretsCryptoTransform(SymmetricAlgorithm algorithm, long startCounter, int startCounterBlockOffset)
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

            _algorithm = algorithm;
            _startCounter = startCounter;
            _startCounterBlockOffset = startCounterBlockOffset;

            _cryptoTransform = _algorithm.CreateEncryptingTransform();

            _blockLength = _cryptoTransform.InputBlockSize;
            _iv = _algorithm.IV();

            _counterWorkBlock = new byte[_blockLength];
            Reset();
        }

        public bool CanReuseTransform { get; } = true;

        public bool CanTransformMultipleBlocks { get; } = true;

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
            _counterBytes = GetBigEndianBytes(_startCounter);
            _counterBlockOffset = _startCounterBlockOffset;
        }

        // This method is optimized for performance, and some common code are expanded as inline instead of being in a separate method.
        private void TransformBlockInternal(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            int counterOffset = _blockLength - CounterLength;
            Span<byte> counterWorkSpan = ((Span<byte>)_counterWorkBlock).Slice(counterOffset);

            // Handle initital partial block
            if (_counterBlockOffset > 0 && inputCount > 0)
            {
                Array.Copy(_iv, _counterWorkBlock, _blockLength);
                for (int i = 0; i < CounterLength; ++i)
                {
                    counterWorkSpan[i] ^= _counterBytes[i];
                }
                byte[] partialOutputBuffer = new byte[_blockLength];
                _cryptoTransform.TransformBlock(_counterWorkBlock, 0, _blockLength, partialOutputBuffer, 0);
                int partialCount = Math.Min(_blockLength - _counterBlockOffset, inputCount);
                for (int i = 0; i < partialCount; ++i)
                {
                    outputBuffer[i] = (byte)(partialOutputBuffer[_counterBlockOffset + i] ^ inputBuffer[inputOffset + i]);
                }
                _counterBlockOffset += partialCount;
                if (_counterBlockOffset == _blockLength)
                {
                    _counterBlockOffset = 0;
                    IncrementCounter();
                }
                inputCount -= partialCount;
                inputOffset += partialCount;
                outputOffset += partialCount;
            }

            // Handle all full blocks
            while (inputCount >= _blockLength)
            {
                Array.Copy(_iv, _counterWorkBlock, _blockLength);

                // For the inner loop, we unroll this loop to avoid the overhead of a loop counter and indexing.
                // The counter is always a long i.e. 8 bytes.
                counterWorkSpan[0] ^= _counterBytes[0];
                counterWorkSpan[1] ^= _counterBytes[1];
                counterWorkSpan[2] ^= _counterBytes[2];
                counterWorkSpan[3] ^= _counterBytes[3];
                counterWorkSpan[4] ^= _counterBytes[4];
                counterWorkSpan[5] ^= _counterBytes[5];
                counterWorkSpan[6] ^= _counterBytes[6];
                counterWorkSpan[7] ^= _counterBytes[7];

                IncrementCounter();

                _cryptoTransform.TransformBlock(_counterWorkBlock, 0, _blockLength, outputBuffer, outputOffset);
                for (int i = 0; i < _blockLength; ++i)
                {
                    outputBuffer[outputOffset + i] ^= inputBuffer[inputOffset + i];
                }
                inputCount -= _blockLength;
                outputOffset += _blockLength;
                inputOffset += _blockLength;
            }

            // Handle final partial block
            if (inputCount > 0)
            {
                Array.Copy(_iv, _counterWorkBlock, _blockLength);
                for (int i = 0; i < CounterLength; ++i)
                {
                    counterWorkSpan[i] ^= _counterBytes[i];
                }
                byte[] partialOutputBuffer = new byte[_blockLength];
                _cryptoTransform.TransformBlock(_counterWorkBlock, 0, _blockLength, partialOutputBuffer, 0);
                for (int i = 0; i < inputCount; ++i)
                {
                    outputBuffer[outputOffset + i] = (byte)(partialOutputBuffer[i] ^ inputBuffer[inputOffset + i]);
                }
                _counterBlockOffset = inputCount;
            }

            void IncrementCounter()
            {
                // Don't loop for performance reasons, the counter is always a long i.e. 8 bytes.
                if (unchecked(++_counterBytes[7]) != 0) return;
                if (unchecked(++_counterBytes[6]) != 0) return;
                if (unchecked(++_counterBytes[5]) != 0) return;
                if (unchecked(++_counterBytes[4]) != 0) return;
                if (unchecked(++_counterBytes[3]) != 0) return;
                if (unchecked(++_counterBytes[2]) != 0) return;
                if (unchecked(++_counterBytes[1]) != 0) return;
                if (unchecked(++_counterBytes[0]) != 0) return;
            }
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            byte[] outputBuffer = new byte[inputCount];
            TransformBlockInternal(inputBuffer, inputOffset, inputCount, outputBuffer, 0);
            Reset();
            return outputBuffer;
        }

        private static byte[] GetBigEndianBytes(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return bytes;
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
            if (_cryptoTransform != null)
            {
                _cryptoTransform.Dispose();
                _cryptoTransform = null!;
            }
            if (_algorithm != null)
            {
                _algorithm.Clear();
                _algorithm = null!;
            }
        }
    }
}
