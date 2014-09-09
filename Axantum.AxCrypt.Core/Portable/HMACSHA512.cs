using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Portable
{
    public abstract class HMACSHA512 : HMAC
    {
        public abstract HMACSHA512 Create(byte[] key);
    }
}