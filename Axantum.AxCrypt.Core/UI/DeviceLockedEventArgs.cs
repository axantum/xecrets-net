using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.UI
{
    public class DeviceLockedEventArgs : EventArgs
    {
        public DeviceLockedEventArgs(DeviceLockReason reason)
        {
            Reason = reason;
        }

        public DeviceLockReason Reason { get; }
    }
}