using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Extensions
{
    public static class EnumExtensions
    {
        public static bool HasMask(this AxCryptOptions options, AxCryptOptions mask)
        {
            return (options & mask) == mask;
        }

        public static bool HasMask(this ActiveFileStatus status, ActiveFileStatus mask)
        {
            return (status & mask) == mask;
        }
    }
}