using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Abstractions
{
    public interface ICacheKey
    {
        string Key { get; }

        TimeSpan Expiration { get; }
    }
}