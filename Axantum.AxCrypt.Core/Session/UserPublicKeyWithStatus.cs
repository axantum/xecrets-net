using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;

namespace Axantum.AxCrypt.Core.Session
{
    public class UserPublicKeyWithStatus
    {
        
        public UserPublicKey PublicKey { get; set; }
        public UserPublicKeyUpdateStatus UpdateStatus { get; set; }
    }
}
