using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Ipc
{
    public class RequestCommandArgs : EventArgs
    {
        public string CommandMessage { get; private set; }

        public RequestCommandArgs(string commandMessage)
        {
            CommandMessage = commandMessage;
        }
    }
}