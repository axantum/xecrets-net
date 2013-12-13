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

        public event EventHandler<RequestCommandArgs> Request;

        protected virtual void OnRequest()
        {
            EventHandler<RequestCommandArgs> handler = Request;
            if (handler != null)
            {
                handler(this, new RequestCommandArgs(String.Empty));
            }
        }
    }
}