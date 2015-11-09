using Axantum.AxCrypt.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Api
{
    public class OfflineApiException : ApiException
    {
        public OfflineApiException()
            : base()
        {
        }

        public OfflineApiException(string message)
            : base(message, ErrorStatus.ApiOffline)
        {
        }

        public OfflineApiException(string message, Exception innerException)
            : base(message, ErrorStatus.ApiOffline, innerException)
        {
        }
    }
}