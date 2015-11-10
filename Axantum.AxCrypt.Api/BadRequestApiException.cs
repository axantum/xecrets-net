using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Api
{
    public class BadRequestApiException : ApiException
    {
        public BadRequestApiException()
            : base()
        {
        }

        public BadRequestApiException(string message)
            : this(message, ErrorStatus.BadApiRequest)
        {
        }

        public BadRequestApiException(string message, ErrorStatus errorStatus)
            : base(message, errorStatus)
        {
        }

        public BadRequestApiException(string message, HttpStatusCode httpStatusCode)
            : base(message, httpStatusCode)
        {
        }

        public BadRequestApiException(string message, Exception innerException)
            : this(message, ErrorStatus.BadApiRequest, innerException)
        {
        }

        public BadRequestApiException(string message, ErrorStatus errorStatus, Exception innerException)
            : base(message, errorStatus, innerException)
        {
        }
    }
}