using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Abstractions
{
    public interface ICacheKey
    {
        ICacheKey? ParentCacheKey { get; }

        string Key { get; }

        TimeSpan Expiration { get; }
    }
}
