using AxCrypt.Core.Crypto;
using AxCrypt.Core.UI;
using System;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Runtime
{
    public class InactivitySignOut : IDisposable
    {
        private DelayedAction _signOutDelayedAction;

        public InactivitySignOut(TimeSpan timeout)
        {
            if (timeout == TimeSpan.Zero || !New<LicensePolicy>().Capabilities.Has(LicenseCapability.InactivitySignOut))
            {
                return;
            }

            _signOutDelayedAction = new DelayedAction(New<IDelayTimer>(), timeout);
            _signOutDelayedAction.Action += SignOutAction;
            _signOutDelayedAction.StartIdleTimer();
        }

        public void RestartInactivityTimer()
        {
            _signOutDelayedAction?.StartIdleTimer();
        }

        private async void SignOutAction(object sender, EventArgs e)
        {
            if (!New<LicensePolicy>().Capabilities.Has(LicenseCapability.InactivitySignOut))
            {
                return;
            }
            await New<KnownIdentities>().SetDefaultEncryptionIdentity(LogOnIdentity.Empty);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }
            if (_signOutDelayedAction != null)
            {
                _signOutDelayedAction.Dispose();
            }
            _signOutDelayedAction = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}