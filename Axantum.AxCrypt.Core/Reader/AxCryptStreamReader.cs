using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Reader
{
    public class AxCryptStreamReader : AxCryptReader
    {
        public AxCryptStreamReader(Stream inputStream)
        {
            LookAheadStream lookAheadStream = inputStream as LookAheadStream;
            if (lookAheadStream == null)
            {
                lookAheadStream = new LookAheadStream(inputStream);
            }
            InputStream = lookAheadStream;
        }
    }
}