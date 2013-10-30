using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;

namespace Axantum.AxCrypt.Core.Session
{
    public class SessionEventArgs : EventArgs
    {
        public IEnumerable<SessionEvent> SessionEvents { get; private set; }

        public SessionEventArgs(IEnumerable<SessionEvent> sessionEvents)
        {
            SessionEvents = sessionEvents;
        }

        public SessionEventArgs(SessionEvent sessionEvent)
            : this(new SessionEvent[] { sessionEvent })
        {
        }
    }
}
