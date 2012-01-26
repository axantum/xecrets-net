using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Reader;

namespace Axantum.AxCrypt.Core
{
    public class AxCryptDocument
    {
        private AxCryptReader _axCryptReader;

        public AxCryptDocument()
        {
        }

        public void Load(AxCryptReader axCryptReader)
        {
            _axCryptReader = axCryptReader;
        }
    }
}