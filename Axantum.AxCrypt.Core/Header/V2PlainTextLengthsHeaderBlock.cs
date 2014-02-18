using Axantum.AxCrypt.Core.Extensions;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Header
{
    public class V2PlaintextLengthsHeaderBlock : EncryptedHeaderBlock
    {
        public V2PlaintextLengthsHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.PlaintextLengths, dataBlock)
        {
        }

        public V2PlaintextLengthsHeaderBlock()
            : this(new byte[sizeof(long) + sizeof(long)])
        {
        }

        public override object Clone()
        {
            V2PlaintextLengthsHeaderBlock block = new V2PlaintextLengthsHeaderBlock((byte[])GetDataBlockBytesReference().Clone());
            return block;
        }

        /// <summary>
        /// The uncompressed size of the data in bytes
        /// </summary>
        public long PlaintextLength
        {
            get
            {
                byte[] rawBlock = HeaderCrypto.Decrypt(GetDataBlockBytesReference());
                long plaintextLength = rawBlock.GetLittleEndianValue(0, sizeof(long));
                return plaintextLength;
            }

            set
            {
                byte[] rawBlock = HeaderCrypto.Decrypt(GetDataBlockBytesReference());
                byte[] plaintextLength = value.GetLittleEndianBytes();
                Array.Copy(plaintextLength, 0, rawBlock, 0, plaintextLength.Length);
                byte[] encryptedBlock = HeaderCrypto.Encrypt(rawBlock);
                SetDataBlockBytesReference(encryptedBlock);
            }
        }

        /// <summary>
        /// The Compressed size of the data in bytes
        /// </summary>
        public long CompressedPlaintextLength
        {
            get
            {
                byte[] rawBlock = HeaderCrypto.Decrypt(GetDataBlockBytesReference());
                long compressedPlaintextLength = rawBlock.GetLittleEndianValue(sizeof(long), sizeof(long));
                return compressedPlaintextLength;
            }

            set
            {
                byte[] rawBlock = HeaderCrypto.Decrypt(GetDataBlockBytesReference());
                byte[] compressedPlaintextLength = value.GetLittleEndianBytes();
                Array.Copy(compressedPlaintextLength, 0, rawBlock, sizeof(long), compressedPlaintextLength.Length);
                byte[] encryptedBlock = HeaderCrypto.Encrypt(rawBlock);
                SetDataBlockBytesReference(encryptedBlock);
            }
        }
    }
}