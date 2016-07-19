using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Forms.Implementation
{
    public class InternetState : IInternetState, IDisposable
    {
        private bool? _currentState;

        public InternetState()
        {
            NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
        }

        private void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            Clear();
        }

        public bool Connected
        {
            get
            {
                if (!_currentState.HasValue)
                {
                    _currentState = GetCurrentState();
                }
                return _currentState.Value;
            }
        }

        public void Clear()
        {
            _currentState = null;
        }

        private static bool GetCurrentState()
        {
            if (New<IUserSettings>().OfflineMode)
            {
                return false;
            }

            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return false;
            }

            int flags;
            if (!NativeMethods.InternetGetConnectedState(out flags, 0))
            {
                return false;
            }

            if (!NativeMethods.IsInternetConnected())
            {
                return false;
            }

            return true;
        }

        private bool _isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (!_isDisposed)
            {
                NetworkChange.NetworkAvailabilityChanged -= NetworkChange_NetworkAvailabilityChanged;
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}