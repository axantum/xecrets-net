using AxCrypt.Core.UI;
using AxCrypt.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace AxCrypt.Forms.Implementation
{
    public class DeviceLocked : IDeviceLocked, IDisposable
    {
        private class BroadcastReceiverForm : Form
        {
            private WindowsDeviceLocking _deviceLocking;

            public BroadcastReceiverForm(WindowsDeviceLocking deviceLocking)
            {
                _deviceLocking = deviceLocking;
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                _deviceLocking.Message(m.Msg, m.WParam, m.LParam);
            }

            protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);
                Visible = false;
            }
        }

        private WindowsDeviceLocking _deviceLocking = new WindowsDeviceLocking();

        public event EventHandler<DeviceLockedEventArgs> DeviceWasLocked;

        public DeviceLocked()
        {
            _deviceLocking.DeviceWasLocked += DeviceLocked_DeviceWasLocked;
        }

        private void DeviceLocked_DeviceWasLocked(object sender, DeviceLockedEventArgs e)
        {
            DeviceWasLocked?.Invoke(this, e);
        }

        private Form _form;

        /// <summary>
        /// Starts this instance. Must be called on the main UI thread.
        /// </summary>
        public void Start(object state)
        {
            _form = new BroadcastReceiverForm(_deviceLocking);
            _deviceLocking.Start(_form.Handle);
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

            if (_deviceLocking != null)
            {
                _deviceLocking.DeviceWasLocked -= DeviceLocked_DeviceWasLocked;
                _deviceLocking.Dispose();
                _deviceLocking = null;
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