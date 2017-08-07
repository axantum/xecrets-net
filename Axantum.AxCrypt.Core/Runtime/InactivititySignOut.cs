using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.UI;
using System;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Runtime
{
    public class InactivititySignOut : IDisposable
    {
        private DelayedAction _signOutDelayedAction;

        public InactivititySignOut(TimeSpan timeOut)
        {
            if (timeOut == TimeSpan.Zero || !New<LicensePolicy>().Capabilities.Has(LicenseCapability.InactivitySignOut))
            {
                return;
            }

            _signOutDelayedAction = new DelayedAction(New<IDelayTimer>(), timeOut);
            _signOutDelayedAction.Action += SignOutAction;
            _signOutDelayedAction.StartIdleTimer();
        }

        public void RestartInactivitityTimer()
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