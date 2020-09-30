using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Core.Portable
{
    public interface ISingleThread : IDisposable
    {
        Task Enter();

        void Leave();
    }
}