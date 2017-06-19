using System;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.UI;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Runtime
{
    public class ApplicationTimeout : IDisposable
    {
        private DelayedAction _signOutDelayedAction;

        public ApplicationTimeout(TimeSpan timeOut)
        {
            if (timeOut == TimeSpan.Zero)
            {
                return;
            }

            _signOutDelayedAction = new DelayedAction(New<IDelayTimer>(), timeOut);
            _signOutDelayedAction.Action += SignOutAction;
            _signOutDelayedAction.StartIdleTimer();
        }

        public void ResetIdleSignOutTimer()
        {
            _signOutDelayedAction?.StartIdleTimer();
        }

        private async void SignOutAction(object sender, EventArgs e)
        {
            if (New<LicensePolicy>().Capabilities.Has(LicenseCapability.TimeOut))
            {
                await New<KnownIdentities>().SetDefaultEncryptionIdentity(LogOnIdentity.Empty);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }
            _signOutDelayedAction?.Dispose();
            _signOutDelayedAction = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}