using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Abstractions
{
    public interface IInternetState
    {
        bool Connected { get; }

        IInternetState Clear();
    }
}