using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxCrypt.Core.Header
{
    public class V2AsymmetricMasterKeyEncryptedHeaderBlock : StringEncryptedHeaderBlockBase
    {
        public V2AsymmetricMasterKeyEncryptedHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.AsymmetricMasterKey, dataBlock)
        {
        }

        public V2AsymmetricMasterKeyEncryptedHeaderBlock(ICrypto headerCrypto)
            : base(HeaderBlockType.AsymmetricMasterKey, headerCrypto)
        {
        }

        public override object Clone()
        {
            V2AsymmetricMasterKeyEncryptedHeaderBlock block = new V2AsymmetricMasterKeyEncryptedHeaderBlock((byte[])GetDataBlockBytesReference().Clone());
            return CopyTo(block);
        }

        public IAsymmetricPublicKey? MasterPublicKey
        {
            get
            {
                if (String.IsNullOrEmpty(StringValue))
                {
                    return null;
                }
                return Resolve.Serializer.Deserialize<IAsymmetricPublicKey>(StringValue);
            }
            set
            {
                StringValue = Resolve.Serializer.Serialize(value);
            }
        }
    }
}
