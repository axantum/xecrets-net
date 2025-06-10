using AxCrypt.Core.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxCrypt.Core.Header
{
    public class V2AsymmetricMasterKeysEncryptedHeaderBlock : StringEncryptedHeaderBlockBase
    {
        public V2AsymmetricMasterKeysEncryptedHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.AsymmetricMasterKeys, dataBlock)
        {
        }

        public V2AsymmetricMasterKeysEncryptedHeaderBlock(ICrypto headerCrypto)
            : base(HeaderBlockType.AsymmetricMasterKeys, headerCrypto)
        {
        }

        public override object Clone()
        {
            V2AsymmetricMasterKeysEncryptedHeaderBlock block = new V2AsymmetricMasterKeysEncryptedHeaderBlock((byte[])GetDataBlockBytesReference().Clone());
            return CopyTo(block);
        }

        public MasterKeys MasterKeys
        {
            get
            {
                if (String.IsNullOrEmpty(StringValue))
                {
                    return MasterKeys.Empty;
                }

                return Resolve.Serializer.Deserialize<MasterKeys>(StringValue);
            }
            set
            {
                StringValue = Resolve.Serializer.Serialize(value ?? MasterKeys.Empty);
            }
        }
    }
}