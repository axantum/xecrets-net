using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Header
{
    public class DataHeaderBlock : HeaderBlock
    {
        public Int64 DataLength { get; private set; }
        public DataHeaderBlock(HeaderBlockType headerBlockType, byte[] dataBlock)
            : base(headerBlockType, dataBlock)
        {
            DataLength = BitConverter.ToInt64(dataBlock, 0);
        }
    }
}
