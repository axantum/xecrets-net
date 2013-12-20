using Axantum.AxCrypt.Core.Ipc;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    internal class FakeRequestClient : IRequestClient
    {
        public CommandStatus Dispatch(CommandServiceEventArgs command)
        {
            return FakeDispatcher(command);
        }

        public Func<CommandServiceEventArgs, CommandStatus> FakeDispatcher { get; set; }
    }
}