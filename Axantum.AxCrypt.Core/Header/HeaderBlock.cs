using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Header
{
    public abstract class HeaderBlock
    {
        private byte[] _dataBlock;

        protected HeaderBlock(HeaderBlockType headerBlockType, byte[] dataBlock)
        {
            HeaderBlockType = headerBlockType;
            _dataBlock = dataBlock;
        }

        public HeaderBlockType HeaderBlockType { get; protected set; }

        public byte[] GetDataBlock()
        {
            return _dataBlock;
        }
    }
}