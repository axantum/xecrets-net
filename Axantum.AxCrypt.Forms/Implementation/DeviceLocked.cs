using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Axantum.AxCrypt.Abstractions.TypeResolve;
using static Axantum.AxCrypt.Forms.NativeMethods;

namespace Axantum.AxCrypt.Forms.Implementation
{
    public class DeviceLocked : IDeviceLocked, IDisposable
    {
        private class BroadcastReceiverForm : Form
        {
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                if (m.Msg != WM_POWERBROADCAST)
                {
                    return;
                }
                if (m.WParam.ToInt32() != PBT_POWERSETTINGCHANGE)
                {
                    return;
                }

                POWERBROADCAST_SETTING ps = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(m.LParam, typeof(POWERBROADCAST_SETTING));
                if (ps.PowerSetting != GUID_MONITOR_POWER_ON && ps.PowerSetting != GUID_CONSOLE_DISPLAY_STATE)
                {
                    return;
                }

                if (ps.DataLength != Marshal.SizeOf(typeof(Int32)))
                {
                    return;
                }

                IntPtr pData = IntPtr.Add(m.LParam, Marshal.SizeOf(ps));

                Int32 iData = (Int32)Marshal.PtrToStructure(pData, typeof(Int32));
                ((DeviceLocked)New<IDeviceLocked>()).Notify(iData == 0);

                return;
            }

            protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);
                Visible = false;
            }
        }

        public event EventHandler<DeviceLockedEventArgs> DeviceWasLocked;

        private IDelayTimer _timer = New<IDelayTimer>();

        private bool? _wasScreenOff;

        private IMessageFilter _messageFilter;

        public DeviceLocked()
        {
            SystemEvents.SessionEnding += SystemEvents_SessionEnding;
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;

            _timer.SetInterval(TimeSpan.FromSeconds(2));
            _timer.Elapsed += PollScreenSaverState;
        }

        private Form _form;

        /// <summary>
        /// Starts this instance. Must be called on the main UI thread.
        /// </summary>
        public void Start(object state)
        {
            _form = new BroadcastReceiverForm();
            RegisterForPowerNotifications(_form.Handle);
            _timer.Start();
        }

        internal void Notify(bool monitorIsOff)
        {
            if (!DidScreenTurnOff(monitorIsOff))
            {
                return;
            }

            OnDeviceWasLocked(new DeviceLockedEventArgs(DeviceLockReason.Temporary));
        }

        private IntPtr _handleToPowerOnNotificationRegistration;

        private IntPtr _handleToMonitorStateNotificationRegistration;

        private void RegisterForPowerNotifications(IntPtr handle)
        {
            _handleToPowerOnNotificationRegistration = NativeMethods.RegisterPowerSettingNotification(handle, ref NativeMethods.GUID_MONITOR_POWER_ON, NativeMethods.DEVICE_NOTIFY_WINDOW_HANDLE);
            _handleToMonitorStateNotificationRegistration = NativeMethods.RegisterPowerSettingNotification(handle, ref NativeMethods.GUID_CONSOLE_DISPLAY_STATE, NativeMethods.DEVICE_NOTIFY_WINDOW_HANDLE);
        }

        private void PollScreenSaverState(object sender, EventArgs e)
        {
            PollScreenSaverState();
            _timer.Start();
        }

        private void PollScreenSaverState()
        {
            const int SPI_GETSCREENSAVERRUNNING = 114;
            bool screenSaverIsRunning = false;

            if (!NativeMethods.SystemParametersInfo(SPI_GETSCREENSAVERRUNNING, 0, ref screenSaverIsRunning, 0))
            {
                return;
            }

            if (!DidScreenTurnOff(screenSaverIsRunning))
            {
                return;
            }

            OnDeviceWasLocked(new DeviceLockedEventArgs(DeviceLockReason.Temporary));
        }

        private bool DidScreenTurnOff(bool isScreenOff)
        {
            if (!_wasScreenOff.HasValue)
            {
                _wasScreenOff = isScreenOff;
            }

            if (isScreenOff == _wasScreenOff)
            {
                return isScreenOff;
            }

            _wasScreenOff = isScreenOff;
            return isScreenOff;
        }

        protected virtual void OnDeviceWasLocked(DeviceLockedEventArgs e)
        {
            New<IUIThread>().PostTo(() => DeviceWasLocked?.Invoke(this, e));
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Suspend:
                    OnDeviceWasLocked(new DeviceLockedEventArgs(DeviceLockReason.Temporary));
                    break;

                default:
                    break;
            }
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.ConsoleDisconnect:
                case SessionSwitchReason.RemoteDisconnect:
                case SessionSwitchReason.SessionLock:
                    OnDeviceWasLocked(new DeviceLockedEventArgs(DeviceLockReason.Temporary));
                    break;

                default:
                    break;
            }
        }

        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionEndReasons.Logoff:
                case SessionEndReasons.SystemShutdown:
                    e.Cancel = true;
                    OnDeviceWasLocked(new DeviceLockedEventArgs(DeviceLockReason.Permanent));
                    break;

                default:
                    break;
            }
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeInternal();
            }
        }

        private void DisposeInternal()
        {
            if (_disposed)
            {
                return;
            }

            SystemEvents.SessionEnding -= SystemEvents_SessionEnding;
            SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;

            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }

            if (_messageFilter != null)
            {
                Application.RemoveMessageFilter(_messageFilter);
                _messageFilter = null;
            }

            if (_handleToPowerOnNotificationRegistration != IntPtr.Zero)
            {
                UnregisterPowerSettingNotification(_handleToPowerOnNotificationRegistration);
                _handleToPowerOnNotificationRegistration = IntPtr.Zero;
            }

            if (_handleToMonitorStateNotificationRegistration != IntPtr.Zero)
            {
                UnregisterPowerSettingNotification(_handleToMonitorStateNotificationRegistration);
                _handleToMonitorStateNotificationRegistration = IntPtr.Zero;
            }

            if (_form != null)
            {
                _form.Close();
                _form.Dispose();
                _form = null;
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DeviceLocked()
        {
            Dispose(false);
        }
    }
}