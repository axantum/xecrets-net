using System;
using System.Linq;
using System.Security.Cryptography;

namespace Axantum.AxCrypt.Core.Crypto
{
    public abstract class CryptoBase : ICrypto
    {
        private IPassphrase _key;

        /// <summary>
        /// Gets the key associated with this instance.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public IPassphrase Key
        {
            get
            {
                return _key;
            }

            protected set
            {
                _key = value;
            }
        }

        public abstract int BlockLength { get; }

        /// <summary>
        /// Create an instance of a transform suitable for NIST Key Wrap.
        /// </summary>
        /// <param name="salt"></param>
        /// <param name="keyWrapDirection"></param>
        /// <returns></returns>
        /// <value>
        /// An instance of the transform.
        ///   </value>
        public abstract IKeyWrapTransform CreateKeyWrapTransform(Salt salt, KeyWrapDirection keyWrapDirection);

        /// <summary>
        /// Decrypt in one operation.
        /// </summary>
        /// <param name="cipherText"></param>
        /// <returns>
        /// The decrypted result minus any padding
        /// </returns>
        public abstract byte[] Decrypt(byte[] cipherText);

        /// <summary>
        /// Encrypt in one operation
        /// </summary>
        /// <param name="plaintext">The complete plaintext bytes</param>
        /// <returns>
        /// The cipher text, complete with any padding
        /// </returns>
        public abstract byte[] Encrypt(byte[] plaintext);

        /// <summary>
        /// Using this instances parameters, create a decryptor
        /// </summary>
        /// <returns>
        /// A new decrypting transformation instance
        /// </returns>
        public abstract ICryptoTransform CreateDecryptingTransform();

        /// <summary>
        /// Using this instances parameters, create an encryptor
        /// </summary>
        /// <returns>
        /// A new encrypting transformation instance
        /// </returns>
        public abstract ICryptoTransform CreateEncryptingTransform();
    }
}