using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;

namespace Axantum.AxCrypt.Core.Session
{
    public class UserPublicKeyUpdateStatus
    {
        private IDictionary<PublicKeyThumbprint, PublicKeyUpdateStatus> _publicKeyUpdateStatus;

        public UserPublicKeyUpdateStatus()
        {
            _publicKeyUpdateStatus = new Dictionary<PublicKeyThumbprint, PublicKeyUpdateStatus>();
        }

        public IDictionary<PublicKeyThumbprint, PublicKeyUpdateStatus> UpdateStatus
        {
            get { return _publicKeyUpdateStatus; }
            set { _publicKeyUpdateStatus = value; }
        }
    }
}
