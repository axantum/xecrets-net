using Axantum.AxCrypt.Core.Ipc;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    internal class FakeRequestClient : IRequestClient
    {
        public CommandStatus Dispatch(string method, string content)
        {
            return FakeDispatcher(method, content);
        }

        public Func<string, string, CommandStatus> FakeDispatcher { get; set; }
    }
}