using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Portable
{
    public interface ISingleThread : IDisposable
    {
        void Enter();

        void Leave();
    }
}