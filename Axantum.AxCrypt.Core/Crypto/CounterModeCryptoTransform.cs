#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core.Extensions;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace Axantum.AxCrypt.Core.Crypto
{
    public class CounterModeCryptoTransform : ICryptoTransform
    {
        private SymmetricAlgorithm _algorithm;

        private int _blockLength;

        private long _startBlockCounter;

        private long _currentBlockCounter;

        private int _startBlockOffset;

        private int _currentBlockOffset;

        private ICryptoTransform _cryptoTransform;

        public CounterModeCryptoTransform(SymmetricAlgorithm algorithm, long blockCounter, int blockOffset)
        {
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
            _startBlockCounter = _currentBlockCounter = blockCounter;
            _startBlockOffset = _currentBlockOffset = blockOffset;

            _cryptoTransform = _algorithm.CreateEncryptor();
            _blockLength = _cryptoTransform.InputBlockSize;
        }

        public bool CanReuseTransform
        {
            get { return true; }
        }

        public bool CanTransformMultipleBlocks
        {
            get { return true; }
        }

        public int InputBlockSize
        {
            get { return _cryptoTransform.InputBlockSize; }
        }

        public int OutputBlockSize
        {
            get { return _cryptoTransform.OutputBlockSize; }
        }

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
            _currentBlockCounter = _startBlockCounter;
            _currentBlockOffset = _startBlockOffset;
        }

        private void TransformBlockInternal(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            byte[] keyStreamblock = new byte[_blockLength];
            int remainingCount = inputCount;
            while (remainingCount > 0)
            {
                _cryptoTransform.TransformBlock(GetCounterBlock(_currentBlockCounter), 0, _blockLength, keyStreamblock, 0);

                int blockBytes = _blockLength - _currentBlockOffset;
                if (remainingCount < blockBytes)
                {
                    blockBytes = remainingCount;
                }
                keyStreamblock.Xor(_currentBlockOffset, inputBuffer, inputOffset, blockBytes);
                Array.Copy(keyStreamblock, _currentBlockOffset, outputBuffer, outputOffset, blockBytes);

                inputOffset += blockBytes;
                outputOffset += blockBytes;
                remainingCount -= blockBytes;
                _currentBlockOffset += blockBytes;
                if (_currentBlockOffset == _blockLength)
                {
                    _currentBlockOffset = 0;
                    _currentBlockCounter += 1;
                }
            }
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            byte[] outputBuffer = new byte[inputCount];
            TransformBlockInternal(inputBuffer, inputOffset, inputCount, outputBuffer, 0);
            Reset();
            return outputBuffer;
        }

        private byte[] GetCounterBlock(long blockCounter)
        {
            byte[] counterBytes = blockCounter.GetBigEndianBytes();
            byte[] counterBlock = ((byte[])_algorithm.IV.Clone()).Xor(_algorithm.IV.Length - counterBytes.Length, counterBytes, 0, counterBytes.Length);
            return counterBlock;
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
                _cryptoTransform = null;
            }
            if (_algorithm != null)
            {
                _algorithm.Clear();
                _algorithm = null;
            }
        }
    }
}