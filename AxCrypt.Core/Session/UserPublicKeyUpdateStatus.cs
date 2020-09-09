using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AxCrypt.Core.Crypto.Asymmetric;

namespace AxCrypt.Core.Session
{
    public class UserPublicKeyUpdateStatus
    {
        private IDictionary<PublicKeyThumbprint, PublicKeyUpdateStatus> _publicKeyUpdateStatus;

        public UserPublicKeyUpdateStatus()
        {
            _publicKeyUpdateStatus = new Dictionary<PublicKeyThumbprint, PublicKeyUpdateStatus>();
        }

        public PublicKeyUpdateStatus Status(UserPublicKey userPublicKey)
        {
            if (userPublicKey == null)
            {
                throw new ArgumentNullException(nameof(userPublicKey));
            }

            PublicKeyUpdateStatus value;
            if (_publicKeyUpdateStatus.TryGetValue(userPublicKey.PublicKey.Thumbprint, out value))
            {
                return value;
            }
            return PublicKeyUpdateStatus.NotRecentlyUpdated;
        }

        public void SetStatus(UserPublicKey userPublicKey, PublicKeyUpdateStatus status)
        {
            if (userPublicKey == null)
            {
                throw new ArgumentNullException(nameof(userPublicKey));
            }

            _publicKeyUpdateStatus[userPublicKey.PublicKey.Thumbprint] = status;
        }

        public void Clear()
        {
            _publicKeyUpdateStatus.Clear();
        }
    }
}