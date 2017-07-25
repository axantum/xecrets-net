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

        public PublicKeyUpdateStatus this[UserPublicKey index] {
            get
            {
                PublicKeyUpdateStatus value;
                if (_publicKeyUpdateStatus.TryGetValue(index.PublicKey.Thumbprint, out value))
                {
                    return value;
                }
                return PublicKeyUpdateStatus.NotRecentlyUpdated;    
            }
            set
            {
                _publicKeyUpdateStatus[index.PublicKey.Thumbprint] = value;
            }
        }

        public void Clear()
        {
            _publicKeyUpdateStatus.Clear();
        }
    }
}
