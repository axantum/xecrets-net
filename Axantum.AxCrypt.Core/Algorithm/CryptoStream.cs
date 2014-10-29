using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Algorithm
{
    public abstract class CryptoStream : Stream
    {
        public abstract Stream Initialize(Stream stream, ICryptoTransform transform, CryptoStreamMode mode);
    }
}