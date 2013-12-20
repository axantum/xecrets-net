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

        public void AcceptRequest(CommandServiceEventArgs command)
        {
            OnRequest(new RequestCommandEventArgs(command));
        }

        public event EventHandler<RequestCommandEventArgs> Request;

        protected virtual void OnRequest(RequestCommandEventArgs e)
        {
            EventHandler<RequestCommandEventArgs> handler = Request;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}