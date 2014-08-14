using Axantum.AxCrypt.Core.Crypto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Header
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

        public IEnumerable<string> Recipients
        {
            get
            {
                if (String.IsNullOrEmpty(StringValue))
                {
                    return new string[0];
                }
                return JsonConvert.DeserializeObject<IEnumerable<string>>(StringValue);
            }
            set
            {
                StringValue = JsonConvert.SerializeObject(value ?? new string[0]);
            }
        }
    }
}