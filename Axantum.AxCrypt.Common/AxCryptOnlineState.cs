using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Common
{
    public class AxCryptOnlineState
    {
        private bool? _isOnline;

        public event EventHandler OnlineStateChanged;

        public bool IsFirstSignIn { get; set; } = true;

        public bool IsOnline
        {
            get
            {
                return _isOnline.GetValueOrDefault(true);
            }
            set
            {
                bool? wasOnline = _isOnline;
                _isOnline = value;
                if (wasOnline != _isOnline)
                {
                    OnOnlineStateChanged(new EventArgs());
                }
            }
        }

        public bool IsOffline
        {
            get
            {
                return !_isOnline.GetValueOrDefault(true);
            }
            set
            {
                bool? wasOnline = _isOnline;
                _isOnline = !value;
                if (wasOnline != _isOnline)
                {
                    OnOnlineStateChanged(new EventArgs());
                }
            }
        }

        public void RaiseOnlineStateChanged()
        {
            OnOnlineStateChanged(new EventArgs());
        }

        protected virtual void OnOnlineStateChanged(EventArgs e)
        {
            OnlineStateChanged?.Invoke(this, e);
        }
    }
}