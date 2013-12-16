using Axantum.AxCrypt.Core.Ipc;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    internal class FakeRequestClient : IRequestClient
    {
        public CommandStatus Dispatch(CommandServiceArgs command)
        {
            return FakeDispatcher(command);
        }

        public Func<CommandServiceArgs, CommandStatus> FakeDispatcher { get; set; }
    }
}