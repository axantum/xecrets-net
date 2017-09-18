using System;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Common
{
    public class UpdateCheck
    {
        public UpdateCheck(UpdateCheckTypes updateCheckInitiatedBy, DateTime lastUpdateCheckUtc)
        {
            UpdateCheckInitiatedBy = updateCheckInitiatedBy;
            LastUpdateCheckUtc = lastUpdateCheckUtc;
        }

        public UpdateCheckTypes UpdateCheckInitiatedBy { get; set; }
        public DateTime LastUpdateCheckUtc { get; set; }
    }
}