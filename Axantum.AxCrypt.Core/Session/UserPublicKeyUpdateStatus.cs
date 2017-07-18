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
        private IDictionary<string, PublicKeyUpdateStatus> _publicKeyUpdateStatus;

        public UserPublicKeyUpdateStatus()
        {
            _publicKeyUpdateStatus = new Dictionary<string, PublicKeyUpdateStatus>();
        }

        public PublicKeyUpdateStatus this[UserPublicKey index] {
            get
            {
                if (!_publicKeyUpdateStatus.ContainsKey(index.PublicKey.Thumbprint.ToString()))
                {
                    return PublicKeyUpdateStatus.None;
                }
                return _publicKeyUpdateStatus[index.PublicKey.Thumbprint.ToString()];
            }
            set
            {
                if (!_publicKeyUpdateStatus.ContainsKey(index.PublicKey.Thumbprint.ToString()))
                {
                    _publicKeyUpdateStatus.Add(index.PublicKey.Thumbprint.ToString(), value);
                    return;
                }
                _publicKeyUpdateStatus[index.PublicKey.Thumbprint.ToString()] = value;
            }
        }

        public void Clear()
        {
            _publicKeyUpdateStatus = _publicKeyUpdateStatus.ToDictionary(p => p.Key, p => PublicKeyUpdateStatus.NotRecentlyUpdated);
        }
    }
}
