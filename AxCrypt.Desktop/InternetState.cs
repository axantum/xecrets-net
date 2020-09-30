using AxCrypt.Abstractions;
using AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Desktop
{
    public class InternetState : IInternetState, IDisposable
    {
        private bool? _currentState;

        private bool _supportsNetworkAvailabilityChanged;

        public InternetState()
        {
            if (New<UserSettings>().OfflineMode)
            {
                return;
            }

            try
            {
                NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
                _supportsNetworkAvailabilityChanged = true;
            }
            catch (SocketException sex)
            {
                New<IReport>().Exception(sex);
            }
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

        public IInternetState Clear()
        {
            _currentState = null;
            return this;
        }

        private static bool GetCurrentState()
        {
            if (New<UserSettings>().OfflineMode)
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

            if (_isDisposed)
            {
                return;
            }

            if (_supportsNetworkAvailabilityChanged)
            {
                NetworkChange.NetworkAvailabilityChanged -= NetworkChange_NetworkAvailabilityChanged;
            }
            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}