using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Header
{
    public class V2AsymmetricKeyWrapHeaderBlock : HeaderBlock
    {
        private const int DATABLOCK_LENGTH = 4096 / 8;

        public V2AsymmetricKeyWrapHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.V2AsymmetricKeyWrap, dataBlock)
        {
        }

        public V2AsymmetricKeyWrapHeaderBlock(IAsymmetricPublicKey publicKey, SymmetricKey masterKey, SymmetricIV masterIV)
            : this(Resolve.RandomGenerator.Generate(DATABLOCK_LENGTH))
        {
            byte[] encrypted = publicKey.Transform(masterKey + masterIV);
            GetDataBlockBytesReference().SetFrom(encrypted);
        }

        public override object Clone()
        {
            V2AsymmetricKeyWrapHeaderBlock block = new V2AsymmetricKeyWrapHeaderBlock((byte[])GetDataBlockBytesReference().Clone());
            return block;
        }

        private IAsymmetricPrivateKey _privateKey;

        /// <summary>
        /// Sets the private key.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        public void SetPrivateKey(IAsymmetricPrivateKey privateKey)
        {
            _privateKey = privateKey;
            _decryptedDataBlock = null;
        }

        private byte[] _decryptedDataBlock = null;

        private byte[] DecryptedDataBlock
        {
            get
            {
                if (_decryptedDataBlock == null)
                {
                    _decryptedDataBlock = _privateKey.Transform(GetDataBlockBytesReference()) ?? new byte[DATABLOCK_LENGTH];
                }
                return _decryptedDataBlock;
            }
        }

        /// <summary>
        /// Create an ICrypto instance from the decrypted asymmetric key wrap.
        /// </summary>
        /// <param name="cryptoFactory"></param>
        /// <param name="keyStreamOffset"></param>
        /// <returns>An ICrypto instance, initialized with key and iv.</returns>
        public ICrypto Crypto(ICryptoFactory cryptoFactory, long keyStreamOffset)
        {
            byte[] iv = new byte[cryptoFactory.BlockSize / 8];
            byte[] masterKey = new byte[cryptoFactory.KeySize / 8];

            Array.Copy(DecryptedDataBlock, 0, masterKey, 0, masterKey.Length);
            Array.Copy(DecryptedDataBlock, masterKey.Length, iv, 0, iv.Length);

            ICrypto crypto = cryptoFactory.CreateCrypto(new SymmetricKey(masterKey), new SymmetricIV(iv), keyStreamOffset);
            return crypto;
        }
    }
}