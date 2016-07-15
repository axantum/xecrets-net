using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.UI
{
    public interface IDeviceLocked
    {
        event EventHandler<DeviceLockedEventArgs> DeviceWasLocked;

        void Start(object state);
    }
}