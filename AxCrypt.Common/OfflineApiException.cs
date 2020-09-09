using AxCrypt.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Common
{
    public class OfflineApiException : ApiException
    {
        public OfflineApiException()
            : base()
        {
            New<AxCryptOnlineState>().IsOnline = false;
        }

        public OfflineApiException(string message)
            : base(message, ErrorStatus.ApiOffline)
        {
            New<AxCryptOnlineState>().IsOnline = false;
        }

        public OfflineApiException(string message, Exception innerException)
            : base(message, ErrorStatus.ApiOffline, innerException)
        {
            New<AxCryptOnlineState>().IsOnline = false;
        }
    }
}