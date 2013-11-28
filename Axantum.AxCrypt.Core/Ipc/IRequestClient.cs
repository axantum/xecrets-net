using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Ipc
{
    public interface IRequestClient
    {
        CommandStatus Dispatch(string method, string content);
    }
}