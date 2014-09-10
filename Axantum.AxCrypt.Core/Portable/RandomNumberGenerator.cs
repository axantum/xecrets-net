using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Portable
{
    public abstract class RandomNumberGenerator : IDisposable
    {
        public abstract void GetBytes(byte[] data);

        public abstract void Dispose();
    }
}