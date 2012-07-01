using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.System
{
    public interface ILauncher : IDisposable
    {
        event EventHandler Exited;

        bool HasExited { get; }

        bool WasStarted { get; }

        string Path { get; }
    }
}