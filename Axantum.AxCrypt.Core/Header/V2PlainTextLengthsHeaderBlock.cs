using Axantum.AxCrypt.Core.Extensions;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Header
{
    public class V2PlainTextLengthsHeaderBlock : EncryptedHeaderBlock
    {
        public V2PlainTextLengthsHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.PlainTextLengths, dataBlock)
        {
        }

        public V2PlainTextLengthsHeaderBlock()
            : this(new byte[sizeof(long) + sizeof(long)])
        {
        }

        public override object Clone()
        {
            V2PlainTextLengthsHeaderBlock block = new V2PlainTextLengthsHeaderBlock((byte[])GetDataBlockBytesReference().Clone());
            return block;
        }

        /// <summary>
        /// The uncompressed size of the data in bytes
        /// </summary>
        public long PlainTextLength
        {
            get
            {
                byte[] rawBlock = HeaderCrypto.Decrypt(GetDataBlockBytesReference());
                long plainTextLength = rawBlock.GetLittleEndianValue(0, sizeof(long));
                return plainTextLength;
            }

            set
            {
                byte[] plainTextLength = value.GetLittleEndianBytes();
                Array.Copy(plainTextLength, 0, GetDataBlockBytesReference(), 0, plainTextLength.Length);
                byte[] encryptedBlock = HeaderCrypto.Encrypt(GetDataBlockBytesReference());
                SetDataBlockBytesReference(encryptedBlock);
            }
        }

        /// <summary>
        /// The Compressed size of the data in bytes
        /// </summary>
        public long CompressedPlainTextLength
        {
            get
            {
                byte[] rawBlock = HeaderCrypto.Decrypt(GetDataBlockBytesReference());
                long compressedPlainTextLength = rawBlock.GetLittleEndianValue(sizeof(long), sizeof(long));
                return compressedPlainTextLength;
            }

            set
            {
                byte[] compressedPlainTextLength = value.GetLittleEndianBytes();
                Array.Copy(compressedPlainTextLength, 0, GetDataBlockBytesReference(), sizeof(long), compressedPlainTextLength.Length);
                byte[] encryptedBlock = HeaderCrypto.Encrypt(GetDataBlockBytesReference());
                SetDataBlockBytesReference(encryptedBlock);
            }
        }
    }
}