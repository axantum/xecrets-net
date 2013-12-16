using Axantum.AxCrypt.Core.Ipc;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    internal class FakeRequestServer : IRequestServer
    {
        public void Start()
        {
        }

        public void Shutdown()
        {
        }

        public void AcceptRequest(CommandServiceArgs command)
        {
            OnRequest(new RequestCommandArgs(command));
        }

        public event EventHandler<RequestCommandArgs> Request;

        protected virtual void OnRequest(RequestCommandArgs e)
        {
            EventHandler<RequestCommandArgs> handler = Request;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}