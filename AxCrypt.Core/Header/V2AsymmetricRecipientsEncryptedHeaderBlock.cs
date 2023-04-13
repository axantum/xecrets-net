using AxCrypt.Core.Crypto;

namespace AxCrypt.Core.Header
{
    public class V2AsymmetricRecipientsEncryptedHeaderBlock : StringEncryptedHeaderBlockBase
    {
        public V2AsymmetricRecipientsEncryptedHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.AsymmetricRecipients, dataBlock)
        {
        }

        public V2AsymmetricRecipientsEncryptedHeaderBlock(ICrypto headerCrypto)
            : base(HeaderBlockType.AsymmetricRecipients, headerCrypto)
        {
        }

        public override object Clone()
        {
            V2AsymmetricRecipientsEncryptedHeaderBlock block = new V2AsymmetricRecipientsEncryptedHeaderBlock((byte[])GetDataBlockBytesReference().Clone());
            return CopyTo(block);
        }

        public Recipients Recipients
        {
            get
            {
                if (string.IsNullOrEmpty(StringValue))
                {
                    return Recipients.Empty;
                }
                return Resolve.Serializer.Deserialize<Recipients>(StringValue) ?? throw new InvalidOperationException("Deserialize returned null for Recipients.");
            }
            set
            {
                StringValue = Resolve.Serializer.Serialize(value ?? Recipients.Empty);
            }
        }
    }
}
