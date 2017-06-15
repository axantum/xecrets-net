using System;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.UI;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Runtime
{
    public class ApplicationTimeout
    {
        private DelayedAction _action;

        public void Timeout(int timeOutInSecond)
        {
            _action = new DelayedAction(New<IDelayTimer>(), TimeSpan.FromMilliseconds(timeOutInSecond * 1000));
            _action.Action += LogOffAction;
            _action.StartIdleTimer();
        }

        private async void LogOffAction(object sender, EventArgs e)
        {
            await New<KnownIdentities>().SetDefaultEncryptionIdentity(LogOnIdentity.Empty);
        }
    }
}
