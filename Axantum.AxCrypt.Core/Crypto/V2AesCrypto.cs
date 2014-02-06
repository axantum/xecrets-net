using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Axantum.AxCrypt.Core.Extensions;

namespace Axantum.AxCrypt.Core.Crypto
{
    /// <summary>
    /// Implements V2 AES Cryptography, briefly AES-256 in CTR-Mode.
    /// </summary>
    public class V2AesCrypto : ICrypto
    {
        private Aes _aes;

        private long _blockCounter;

        private int _blockByteSize;

        private int _blockOffset;

        /// <summary>
        /// Instantiate a transformation
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="iv">Initial Vector</param>
        /// <param name="blockCounter">The block counter.</param>
        /// <exception cref="System.ArgumentNullException">
        /// key
        /// or
        /// iv
        /// </exception>
        public V2AesCrypto(AesKey key, AesIV iv, long blockCounter, int blockOffset)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (iv == null)
            {
                throw new ArgumentNullException("iv");
            }

            _aes = new AesManaged();
            _aes.Key = key.GetBytes();
            _aes.Mode = CipherMode.ECB;
            _aes.IV = iv.GetBytes();
            _aes.Padding = PaddingMode.None;

            _blockByteSize = _aes.BlockSize / 8;
            _blockCounter = blockCounter;
            _blockOffset = blockOffset;
        }

        public V2AesCrypto(AesKey key, AesIV iv, long keyStreamByteIndex)
            : this(key, iv, keyStreamByteIndex / iv.Length, (int)(keyStreamByteIndex % iv.Length))
        {
        }

        /// <summary>
        /// Decrypt in one operation.
        /// </summary>
        /// <param name="ciphertext">The complete cipher text</param>
        /// <returns>
        /// The decrypted result minus any padding
        /// </returns>
        public byte[] Decrypt(byte[] ciphertext)
        {
            return KeyStream(ciphertext.Length).Xor(ciphertext);
        }

        /// <summary>
        /// Encrypt in one operation
        /// </summary>
        /// <param name="plaintext">The complete plaintext bytes</param>
        /// <returns>
        /// The cipher text, complete with any padding
        /// </returns>
        public byte[] Encrypt(byte[] plaintext)
        {
            return KeyStream(plaintext.Length).Xor(plaintext);
        }

        private byte[] KeyStream(int length)
        {
            byte[] keyStream = new byte[length];

            using (ICryptoTransform encryptor = _aes.CreateEncryptor())
            {
                byte[] counterBlock = encryptor.TransformFinalBlock(GetCounterBlock(_blockCounter), 0, _blockByteSize);
                for (int i = 0; i < length; )
                {
                    if (_blockOffset == _blockByteSize)
                    {
                        _blockOffset = 0;
                        ++_blockCounter;
                        counterBlock = encryptor.TransformFinalBlock(GetCounterBlock(_blockCounter), 0, _blockByteSize);
                    }
                    keyStream[i++] = counterBlock[_blockOffset++];
                }
            }
            return keyStream;
        }

        private byte[] GetCounterBlock(long blockCounter)
        {
            byte[] counterBytes = blockCounter.GetBigEndianBytes();
            byte[] counterBlock = ((byte[])_aes.IV.Clone()).Xor(_aes.IV.Length - counterBytes.Length, counterBytes, 0, counterBytes.Length);
            return counterBlock;
        }

        /// <summary>
        /// Using this instances parameters, create a decryptor
        /// </summary>
        /// <returns>
        /// A new decrypting transformation instance
        /// </returns>
        public ICryptoTransform CreateDecryptingTransform()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Using this instances parameters, create an encryptor
        /// </summary>
        /// <returns>
        /// A new encrypting transformation instance
        /// </returns>
        public ICryptoTransform CreateEncryptingTransform()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}