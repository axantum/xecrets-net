using System;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.UI;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Runtime
{
    public class ApplicationTimeout
    {
        private DelayedAction _action;

        public void Timeout()
        {
            if (New<LicensePolicy>().Capabilities.Has(LicenseCapability.TimeOut) && Resolve.UserSettings.TimeOutDurationInSecond != 0)
            {
                _action = new DelayedAction(New<IDelayTimer>(), TimeSpan.FromMilliseconds(Resolve.UserSettings.TimeOutDurationInSecond * 1000));
                _action.Action += LogOffAction;
                _action.StartIdleTimer();
            }
        }

        private async void LogOffAction(object sender, EventArgs e)
        {
            await New<KnownIdentities>().SetDefaultEncryptionIdentity(LogOnIdentity.Empty);
        }
    }
}
