using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Common
{
    public class AxCryptOnlineState
    {
        private bool _isOnline = true;

        public bool IsOnline
        {
            get
            {
                return _isOnline;
            }
            set
            {
                _isOnline = value;
            }
        }

        public bool IsOffline
        {
            get
            {
                return !_isOnline;
            }
            set
            {
                _isOnline = !value;
            }
        }
    }
}