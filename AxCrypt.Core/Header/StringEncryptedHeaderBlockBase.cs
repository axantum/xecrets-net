using AxCrypt.Core.Crypto;
using AxCrypt.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxCrypt.Core.Header
{
    public abstract class StringEncryptedHeaderBlockBase : EncryptedHeaderBlock
    {
        protected StringEncryptedHeaderBlockBase(HeaderBlockType blockType, byte[] dataBlock)
            : base(blockType, dataBlock)
        {
        }

        protected StringEncryptedHeaderBlockBase(HeaderBlockType blockType, ICrypto headerCrypto)
            : this(blockType, new byte[0])
        {
            HeaderCrypto = headerCrypto;
            StringValue = String.Empty;
        }

        protected string StringValue
        {
            get
            {
                byte[] rawValue = HeaderCrypto.Decrypt(GetDataBlockBytesReference());

                int end = rawValue.Locate(new byte[] { 0, 0, }, 0, rawValue.Length, 1);
                if (end == -1)
                {
                    throw new InvalidOperationException("Could not find terminating double null byte in string value");
                }

                string stringValue = Encoding.UTF8.GetString(rawValue, 0, end);

                return stringValue;
            }

            set
            {
                byte[] rawStringValue = Encoding.UTF8.GetBytes(value);
                byte[] doubleNullTerminatedRawStringValue = new byte[rawStringValue.Length + 2];
                rawStringValue.CopyTo(doubleNullTerminatedRawStringValue, 0);

                byte[] dataBlock = Resolve.RandomGenerator.Generate(doubleNullTerminatedRawStringValue.Length <= 256 ? 256 : doubleNullTerminatedRawStringValue.Length);
                doubleNullTerminatedRawStringValue.CopyTo(dataBlock, 0);
                SetDataBlockBytesReference(HeaderCrypto.Encrypt(dataBlock));
            }
        }
    }
}