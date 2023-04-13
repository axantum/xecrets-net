using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Abstractions
{
    public static class Constant
    {
        public static readonly Task CompletedTask = Task.FromResult<object?>(null);

        public static readonly Task<bool> TrueTask = Task.FromResult(true);

        public static readonly Task<bool> FalseTask = Task.FromResult(false);
    }
}