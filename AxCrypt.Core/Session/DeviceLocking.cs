using AxCrypt.Abstractions;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Session
{
    public sealed class DeviceLocking : IDisposable
    {
        private Func<Task> _temporaryLocking;

        private Func<Task> _permanentLocking;

        public DeviceLocking(Func<Task> temporaryLocking, Func<Task> permanentLocking)
        {
            _temporaryLocking = temporaryLocking;
            _permanentLocking = permanentLocking;

            New<IDeviceLocked>().DeviceWasLocked += DeviceWasLocked;
            New<IDeviceLocked>().Start(null);
        }

        private DeviceLockReason _currentLock = DeviceLockReason.None;

        private async void DeviceWasLocked(object sender, DeviceLockedEventArgs e)
        {
            if (!New<IUIThread>().IsOn)
            {
                throw new InternalErrorException("Must be on UI thread to handle device locking events.");
            }

            switch (e.Reason)
            {
                case DeviceLockReason.Permanent:
                    if (_currentLock != DeviceLockReason.None && _currentLock != DeviceLockReason.Temporary)
                    {
                        break;
                    }

                    _currentLock = DeviceLockReason.Permanent;
                    try
                    {
                        await _permanentLocking();
                    }
                    finally
                    {
                        _currentLock = DeviceLockReason.None;
                    }
                    break;

                case DeviceLockReason.Temporary:
                    if (_currentLock != DeviceLockReason.None)
                    {
                        break;
                    }

                    if (New<UserSettings>().InactivitySignOutTime == TimeSpan.Zero)
                    {
                        return;
                    }

                    _currentLock = DeviceLockReason.Temporary;
                    try
                    {
                        await _temporaryLocking();
                    }
                    finally
                    {
                        _currentLock = DeviceLockReason.None;
                    }
                    break;

                default:
                    break;
            }
        }

        private bool _disposed = false;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            New<IDeviceLocked>().DeviceWasLocked -= DeviceWasLocked;
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}