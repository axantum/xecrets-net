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

using AxCrypt.Abstractions;
using AxCrypt.Abstractions.Algorithm;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xecrets.Net.Core.Test.LegacyImplementation
{
    internal class BouncyCastleAesTransform : ICryptoTransform
    {
        private IBufferedCipher _cipher;

        /// <summary>
        /// Initializes a new instance of the <see cref="BouncyCastleAesTransform"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="encrypting">if set to <c>true</c> used for encryption, otherwise for decryption.</param>
        public BouncyCastleAesTransform(byte[] key, byte[] iv, bool encrypting, CipherMode cipherMode, PaddingMode paddingMode)
        {
            _cipher = GetCipherWithModeAndPadding(cipherMode, paddingMode);

            ICipherParameters keyParameter = new KeyParameter(key);
            if (cipherMode == CipherMode.CBC)
            {
                keyParameter = new ParametersWithIV(keyParameter, iv);
            }
            _cipher.Init(encrypting, keyParameter);
        }

        private static IBufferedCipher GetCipherWithModeAndPadding(CipherMode cipherMode, PaddingMode paddingMode)
        {
            if (cipherMode == CipherMode.ECB && paddingMode == PaddingMode.None)
            {
                return new BufferedBlockCipher(new AesFastEngine());
            }
            if (cipherMode == CipherMode.CBC && paddingMode == PaddingMode.PKCS7)
            {
                return new PaddedBufferedBlockCipher(new CbcBlockCipher(new AesFastEngine()), new Pkcs7Padding());
            }
            if (cipherMode == CipherMode.CBC && paddingMode == PaddingMode.None)
            {
                return new BufferedBlockCipher(new CbcBlockCipher(new AesFastEngine()));
            }
            throw new NotSupportedException("Only ECB and CBC w/Pkcs7 supported.");
        }

        /// <summary>
        /// Gets a value indicating whether the current transform can be reused.
        /// </summary>
        /// <returns>true if the current transform can be reused; otherwise, false.</returns>
        public bool CanReuseTransform
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether multiple blocks can be transformed.
        /// </summary>
        /// <returns>true if multiple blocks can be transformed; otherwise, false.</returns>
        public bool CanTransformMultipleBlocks
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the input block size.
        /// </summary>
        /// <returns>The size of the input data blocks in bytes.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public int InputBlockSize
        {
            get { return _cipher.GetBlockSize(); }
        }

        /// <summary>
        /// Gets the output block size.
        /// </summary>
        /// <returns>The size of the output data blocks in bytes.</returns>
        public int OutputBlockSize
        {
            get { return _cipher.GetBlockSize(); }
        }

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
        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            int outputCount = 0;
            for (int offset = 0; offset < inputCount; offset += InputBlockSize)
            {
                int len = _cipher.ProcessBytes(inputBuffer, inputOffset + offset, InputBlockSize, outputBuffer, outputOffset + outputCount);
                outputCount += len;
            }
            return outputCount;
        }

        /// <summary>
        /// Transforms the specified region of the specified byte array.
        /// </summary>
        /// <param name="inputBuffer">The input for which to compute the transform.</param>
        /// <param name="inputOffset">The offset into the byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the byte array to use as data.</param>
        /// <returns>
        /// The computed transform.
        /// </returns>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Implementation only supports whole block processing and ECB.</exception>
        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            try
            {
                byte[] final = new byte[_cipher.GetOutputSize(inputCount)];
                int len = _cipher.ProcessBytes(inputBuffer, inputOffset, inputCount, final, 0);
                len += _cipher.DoFinal(final, len);
                if (len != final.Length)
                {
                    byte[] shorter = new byte[len];
                    Array.Copy(final, 0, shorter, 0, len);
                    final = shorter;
                }

                _cipher.Reset();
                return final;
            }
            catch (Org.BouncyCastle.Crypto.CryptoException ce)
            {
                throw new AxCrypt.Core.Runtime.CryptoException("Error in cryptographic transformation.", ErrorStatus.CryptographicError, ce);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
